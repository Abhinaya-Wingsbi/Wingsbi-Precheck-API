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


        public byte[] ExportToExcel(List<MaterialRequisitionResponse> materialRequisitions, List<string>? selectedColumns = null)
        {
            try
            {
                _logger.LogInformation("Starting Excel export for {Count} material requisitions", materialRequisitions.Count);

                var columns = ResolveColumns(selectedColumns);

                using (var workbook = new XSSFWorkbook())
                {
                    var sheet = workbook.CreateSheet("MaterialRequisitionData");

                    var headerStyle = CreateHeaderStyle(workbook);
                    var borderStyle = CreateBorderStyle(workbook);

                    WriteHeaders(sheet, headerStyle, columns);

                    for (int i = 0; i < materialRequisitions.Count; i++)
                    {
                        WriteDataRow(sheet, materialRequisitions[i], borderStyle, i + 1, columns);
                    }

                    AutoSizeColumns(sheet, columns.Count);

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

        private sealed class ColumnDefinition
        {
            public string Header { get; }
            public Func<MaterialRequisitionResponse, string?> ValueSelector { get; }

            public ColumnDefinition(string header, Func<MaterialRequisitionResponse, string?> valueSelector)
            {
                Header = header;
                ValueSelector = valueSelector;
            }
        }

        private static readonly Dictionary<string, ColumnDefinition> ColumnMap =
            new Dictionary<string, ColumnDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                { "materialRequisitionId", new ColumnDefinition("Material Requisition ID", i => i.MaterialRequisitionId.ToString()) },
                { "requestNumber", new ColumnDefinition("Request Number", i => i.RequestNumber) },
                { "projectNumber", new ColumnDefinition("Project Number", i => i.ProjectNumber) },
                { "productionOrderNumber", new ColumnDefinition("Production Order Number", i => i.ProductionOrderNumber) },
                { "productionSeries", new ColumnDefinition("Production Series", i => i.ProductionSeries) },
                { "drawingNumber", new ColumnDefinition("Drawing Number", i => i.DrawingNumber) },
                { "nomenclature", new ColumnDefinition("Nomenclature", i => i.Nomenclature) },
                { "lnItemCode", new ColumnDefinition("LN Item Code", i => i.LnItemCode) },
                { "idNumber", new ColumnDefinition("ID Number", i => i.IdNumber) },
                { "irNumber", new ColumnDefinition("IR Number", i => i.IrNumber) },
                { "msnNumber", new ColumnDefinition("MSN Number", i => i.MsnNumber) },
                { "mrirNumber", new ColumnDefinition("MRIR Number", i => i.MrirNumber) },
                { "consumedInDrawing", new ColumnDefinition("Consumed In Drawing", i => i.ConsumedInDrawing) },
                { "remarks", new ColumnDefinition("Remarks", i => i.Remarks) },
                { "quantity", new ColumnDefinition("Quantity", i => i.Quantity?.ToString()) },
                { "unit", new ColumnDefinition("Unit", i => i.Unit) },
                { "date", new ColumnDefinition("Date", i => i.MyDate?.ToString("yyyy-MM-dd")) },
                { "componentCodeId", new ColumnDefinition("Component Code ID", i => i.ComponentCodeId?.ToString()) },
                { "componentType", new ColumnDefinition("Component Type", i => i.ComponentType) },
                { "srNumber", new ColumnDefinition("SR Number", i => i.SrNumber?.ToString()) },
                { "username", new ColumnDefinition("Username", i => i.Username) },
                { "hwno", new ColumnDefinition("HW No", i => i.Hwno) },
                { "requestOwner", new ColumnDefinition("Request Owner", i => i.RequestOwner) },
                { "status", new ColumnDefinition("Status", i => i.Status) },
                { "precheckDate", new ColumnDefinition("Precheck Date", i => i.PrecheckDate?.ToString("yyyy-MM-dd")) },
                { "createdDate", new ColumnDefinition("Created Date", i => i.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss")) },
                { "modifiedDate", new ColumnDefinition("Modified Date", i => i.ModifiedDate?.ToString("yyyy-MM-dd HH:mm:ss")) },
            };

        private static readonly string[] DefaultColumnOrder = new[]
        {
            "materialRequisitionId", "requestNumber", "projectNumber", "productionOrderNumber",
            "productionSeries", "drawingNumber", "nomenclature", "lnItemCode",
            "idNumber", "irNumber", "msnNumber", "mrirNumber", "consumedInDrawing",
            "remarks", "quantity", "unit", "date", "componentCodeId", "componentType",
            "srNumber", "username", "hwno", "requestOwner", "status",
            "precheckDate", "createdDate", "modifiedDate"
        };

        private static List<ColumnDefinition> ResolveColumns(List<string>? selectedColumns)
        {
            var keys = (selectedColumns != null && selectedColumns.Any())
                ? selectedColumns
                : DefaultColumnOrder.ToList();

            var columns = new List<ColumnDefinition>();
            foreach (var key in keys)
            {
                if (!string.IsNullOrWhiteSpace(key) && ColumnMap.TryGetValue(key.Trim(), out var definition))
                {
                    columns.Add(definition);
                }
            }

            return columns.Any() ? columns : DefaultColumnOrder.Select(k => ColumnMap[k]).ToList();
        }

        private static void WriteHeaders(ISheet sheet, ICellStyle headerStyle, List<ColumnDefinition> columns)
        {
            var headerRow = sheet.CreateRow(0);
            for (int i = 0; i < columns.Count; i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(columns[i].Header);
                cell.CellStyle = headerStyle;
            }
        }

        private static void WriteDataRow(ISheet sheet, MaterialRequisitionResponse item, ICellStyle borderStyle, int rowIndex, List<ColumnDefinition> columns)
        {
            var row = sheet.CreateRow(rowIndex);
            for (int i = 0; i < columns.Count; i++)
            {
                CreateCell(row, i, columns[i].ValueSelector(item), borderStyle);
            }
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
