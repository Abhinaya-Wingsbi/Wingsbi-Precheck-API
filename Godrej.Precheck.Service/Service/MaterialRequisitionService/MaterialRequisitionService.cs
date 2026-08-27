using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel.MaterialRequisition;
using Godrej.Precheck.Models.DTOs.MaterialRequisition;
using Godrej.Precheck.Repository.Repository.MaterialRequisitionRepository;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Godrej.Precheck.Service.Service.MaterialRequisitionService
{
    public class MaterialRequisitionService : IMaterialRequisitionService
    {
        private readonly IMaterialRequisitionRepository _materialRequisitionRepository;
        private readonly ILogger<MaterialRequisitionService> _logger;

        public MaterialRequisitionService(
            IMaterialRequisitionRepository materialRequisitionRepository,
            ILogger<MaterialRequisitionService> logger)
        {
            _materialRequisitionRepository = materialRequisitionRepository;
            _logger = logger;
        }

        public async Task<List<MaterialRequisitionResponse>> GetMaterialRequisitions()
        {
            _logger.LogInformation("Request for MaterialRequisitionService:GetMaterialRequisitions");
            try
            {
                var result = await _materialRequisitionRepository.GetMaterialRequisitions();
                _logger.LogInformation($"MaterialRequisitionService:GetMaterialRequisitions - Successfully retrieved {result.Count} records");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in MaterialRequisitionService:GetMaterialRequisitions");
                throw;
            }
        }

        public async Task<List<SwappingDetailsResponse>> GetSwappingDetails()
        {
            _logger.LogInformation("Request for MaterialRequisitionService:GetSwappingDetails");
            try
            {
                var result = await _materialRequisitionRepository.GetSwappingDetails();
                _logger.LogInformation("MaterialRequisitionService:GetSwappingDetails - Successfully retrieved {Count} records", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in MaterialRequisitionService:GetSwappingDetails");
                throw;
            }
        }

        public async Task<int> UpdateMaterialRequisition(UpdateMaterialRequisitionRequestDto request, int modifiedBy)
        {
            _logger.LogInformation($"Request for MaterialRequisitionService:UpdateMaterialRequisition for MaterialRequisitionId: {request.MaterialRequisitionId}");
            try
            {
                var result = await _materialRequisitionRepository.UpdateMaterialRequisition(request, modifiedBy);
                _logger.LogInformation($"MaterialRequisitionService:UpdateMaterialRequisition - Successfully updated MaterialRequisitionId: {request.MaterialRequisitionId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in MaterialRequisitionService:UpdateMaterialRequisition for MaterialRequisitionId: {request.MaterialRequisitionId}");
                throw;
            }
        }

        public async Task<int> CancelMaterialRequisition(CancelMaterialRequisitionRequestDto request, int modifiedBy)
        {
            _logger.LogInformation($"Request for MaterialRequisitionService:CancelMaterialRequisition for RequestId: {request.RequestId}");
            try
            {
                var result = await _materialRequisitionRepository.CancelMaterialRequisition(request, modifiedBy);
                _logger.LogInformation($"MaterialRequisitionService:CancelMaterialRequisition - Successfully cancelled RequestId: {request.RequestId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in MaterialRequisitionService:CancelMaterialRequisition for RequestId: {request.RequestId}");
                throw;
            }
        }

        public async Task<List<MaterialRequisitionResponse>> GetMaterialRequisitionsByStatus(string status,int statusId)
        {
            _logger.LogInformation($"Request for MaterialRequisitionService:GetMaterialRequisitionsByStatus with status: {status}");
            try
            {
                var result = await _materialRequisitionRepository.GetMaterialRequisitionsByStatus(status,statusId);
                _logger.LogInformation($"MaterialRequisitionService:GetMaterialRequisitionsByStatus - Successfully retrieved {result.Count} records");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in MaterialRequisitionService:GetMaterialRequisitionsByStatus for status: {status}");
                throw;
            }
        }

        public async Task<(int NewId, string RequestNumber)> CreateMaterialRequisition(CreateMaterialRequisitionRequestDto request, int createdBy)
        {
            _logger.LogInformation("Request for MaterialRequisitionService:CreateMaterialRequisition");
            try
            {
                var result = await _materialRequisitionRepository.CreateMaterialRequisition(request, createdBy);
                _logger.LogInformation($"MaterialRequisitionService:CreateMaterialRequisition - Successfully created with Id: {result.NewId}, RequestNumber: {result.RequestNumber}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in MaterialRequisitionService:CreateMaterialRequisition");
                throw;
            }
        }

        public async Task<int> CreateSwappedDrawingNumber(CreateSwappedDrawingNumberRequestDto request, int createdBy)
        {
            _logger.LogInformation("Request for MaterialRequisitionService:CreateSwappedDrawingNumber");
            try
            {
                //Check Componenttpe of drawing number is new component or not based on drawing number id
                var results= await _materialRequisitionRepository.CheckComponentType(request.SwappedDrawingNumberID);

                if(results != 3)
                {
                    _logger.LogWarning($"MaterialRequisitionService:CreateSwappedDrawingNumber - DrawingNumberID: {request.SwappedDrawingNumberID} is not a new component. Aborting swap operation.");
                    throw new InvalidOperationException("Swapped Component is not type of ID");
                }

                //InActive Old record in target PO
                var newComponentInsert = await _materialRequisitionRepository.SwapNewComponentInNewAssembly(request, createdBy);


                //InActive old record in tbl_projectprecheckdetails and insert same record with same details with InActive as true
                int projectPrecheckdetailsId = await _materialRequisitionRepository.InActiveProjectPrecheckDetails(request, createdBy);
              

                _logger.LogInformation(
                    "MaterialRequisitionService:CreateSwappedDrawingNumber - Successfully created for SwapTransactionID: {SwapTransactionID}"
                    );
                return newComponentInsert;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in MaterialRequisitionService:CreateSwappedDrawingNumber");
                throw;
            }
        }


        public byte[] ExportToExcel(List<MaterialRequisitionResponse> materialRequisitions)
        {
            try
            {
                _logger.LogInformation("Starting Excel export for {Count} material requisitions", materialRequisitions.Count);

                using (var workbook = new XSSFWorkbook())
                {
                    var sheet = workbook.CreateSheet("MaterialRequisitionData");

                    // Create styles
                    var headerStyle = CreateHeaderStyle(workbook);
                    var borderStyle = CreateBorderStyle(workbook);

                    // Write headers
                    WriteHeaders(sheet, headerStyle);

                    // Write data rows
                    for (int i = 0; i < materialRequisitions.Count; i++)
                    {
                        WriteDataRow(sheet, materialRequisitions[i], borderStyle, i + 1);
                    }

                    // Adjust column widths
                    AutoSizeColumns(sheet, Headers.Length);

                    // Convert workbook to byte array
                    using (var ms = new MemoryStream())
                    {
                        workbook.Write(ms);
                        _logger.LogInformation($"Excel export completed successfully for {materialRequisitions.Count} material requisitions");
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during Excel export");
                throw;
            }
        }

        private static ICellStyle CreateHeaderStyle(IWorkbook workbook)
        {
            var style = workbook.CreateCellStyle();
            var font = workbook.CreateFont();
            font.IsBold = true;
            style.SetFont(font);
            style.FillForegroundColor = IndexedColors.Grey25Percent.Index;
            style.FillPattern = FillPattern.SolidForeground;
            style.Alignment = HorizontalAlignment.Center;
            style.VerticalAlignment = VerticalAlignment.Center;
            return style;
        }

        private static ICellStyle CreateBorderStyle(IWorkbook workbook)
        {
            var style = workbook.CreateCellStyle();
            style.BorderTop = BorderStyle.Thin;
            style.BorderBottom = BorderStyle.Thin;
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;
            return style;
        }

        private static readonly string[] Headers = new string[]
        {
            "Material Requisition ID", "Request Number", "Project Number", "Production Order Number",
            "Production Series", "Drawing Number", "Nomenclature", "LN Item Code",
            "ID Number", "IR Number", "MSN Number", "MRIR Number", "Consumed In Drawing",
            "Remarks", "Quantity", "Unit", "Date", "Component Code ID", "Component Type",
            "SR Number", "Username", "HW No", "Request Owner", "Status",
            "Precheck Date", "Created Date", "Modified Date"
        };

        private static void WriteHeaders(ISheet sheet, ICellStyle headerStyle)
        {
            var headerRow = sheet.CreateRow(0);
            for (int i = 0; i < Headers.Length; i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(Headers[i]);
                cell.CellStyle = headerStyle;
            }
        }

        private static void WriteDataRow(ISheet sheet, MaterialRequisitionResponse item, ICellStyle borderStyle, int rowIndex)
        {
            var row = sheet.CreateRow(rowIndex);
            int colIndex = 0;
            
            CreateCell(row, colIndex++, item.MaterialRequisitionId.ToString(), borderStyle);
            CreateCell(row, colIndex++, item.RequestNumber, borderStyle);
            CreateCell(row, colIndex++, item.ProjectNumber, borderStyle);
            CreateCell(row, colIndex++, item.ProductionOrderNumber, borderStyle);
            CreateCell(row, colIndex++, item.ProductionSeries, borderStyle);
            CreateCell(row, colIndex++, item.DrawingNumber, borderStyle);
            CreateCell(row, colIndex++, item.Nomenclature, borderStyle);
            CreateCell(row, colIndex++, item.LnItemCode, borderStyle);
            CreateCell(row, colIndex++, item.IdNumber, borderStyle);
            CreateCell(row, colIndex++, item.IrNumber, borderStyle);
            CreateCell(row, colIndex++, item.MsnNumber, borderStyle);
            CreateCell(row, colIndex++, item.MrirNumber, borderStyle);
            CreateCell(row, colIndex++, item.ConsumedInDrawing, borderStyle);
            CreateCell(row, colIndex++, item.Remarks, borderStyle);
            CreateCell(row, colIndex++, item.Quantity?.ToString(), borderStyle);
            CreateCell(row, colIndex++, item.Unit, borderStyle);
            CreateCell(row, colIndex++, item.MyDate?.ToString("yyyy-MM-dd"), borderStyle);
            CreateCell(row, colIndex++, item.ComponentCodeId?.ToString(), borderStyle);
            CreateCell(row, colIndex++, item.ComponentType, borderStyle);
            CreateCell(row, colIndex++, item.SrNumber?.ToString(), borderStyle);
            CreateCell(row, colIndex++, item.Username, borderStyle);
            CreateCell(row, colIndex++, item.Hwno, borderStyle);
            CreateCell(row, colIndex++, item.RequestOwner, borderStyle);
            CreateCell(row, colIndex++, item.Status, borderStyle);
            CreateCell(row, colIndex++, item.PrecheckDate?.ToString("yyyy-MM-dd"), borderStyle);
            CreateCell(row, colIndex++, item.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss"), borderStyle);
            CreateCell(row, colIndex++, item.ModifiedDate?.ToString("yyyy-MM-dd HH:mm:ss"), borderStyle);
        }

        private static void CreateCell(IRow row, int column, string value, ICellStyle style)
        {
            var cell = row.CreateCell(column);
            cell.SetCellValue(value ?? string.Empty);
            cell.CellStyle = style;
        }

        private static void AutoSizeColumns(ISheet sheet, int columnCount)
        {
            for (int i = 0; i < columnCount; i++)
            {
                sheet.AutoSizeColumn(i);
            }
        }
    }
}
