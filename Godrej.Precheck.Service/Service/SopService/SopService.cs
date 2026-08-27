using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel.Sop;
using Godrej.Precheck.Models.DTOs.Precheck;
using Godrej.Precheck.Models.DTOs.Sop;
using Godrej.Precheck.Models.DTOs.Bom;
using Godrej.Precheck.Repository.Repository.SopRepository;
using Godrej.Precheck.Service.Cache;
using Godrej.Precheck.Service.Service.CommonSevice;
using Mapster;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Godrej.Precheck.Service.Service.SopService
{
    public class SopService : ISopService
    {
        private readonly ILogger<SopService> _logger;
        private readonly ISopRepository _sopRepository;
        private readonly ICommonService _commonService;
        private readonly ICacheService _cacheService;

        public class SerialNumberCounter
        {
            public int Value { get; set; }
        }

        public SopService(ILogger<SopService> logger, ISopRepository sopRepository, ICacheService cacheService,ICommonService commonService)
        {
            _logger = logger;
            _sopRepository = sopRepository;
            _cacheService = cacheService;
            _commonService = commonService;
        }

        private static bool IsNullOrEmptyOrNA(string value)
        {
            return string.IsNullOrWhiteSpace(value) || 
                   string.Equals(value, "NA", StringComparison.OrdinalIgnoreCase) || 
                   string.Equals(value, "N/A", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<List<SopAssemblyResponseDto>> GetAllAssembly()
        {
            try
            {
                // Get all assemblies from cache or repository with mapping
                var assemblies = await _cacheService.GetOrSetAsync(
                    CacheSettings.AssemblyCacheKey,
                    async () =>
                    {
                        var result = await _sopRepository.GetAllAssembly();
                        return result.Adapt<List<SopAssemblyResponseDto>>();
                    },
                    CacheSettings.AssemblyCacheDuration
                );

                return assemblies;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<GetSopResponseDto>> GetSopForAssembly(GetSopRequestDto request)
        {
            return await GetSopForAssembly(request, excludeRawMaterial: false);
        }

        public async Task<List<GetSopResponseDto>> GetSopForAssembly(GetSopRequestDto request, bool excludeRawMaterial)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var result = await _sopRepository.GetAllSopTemplate(request.AssemblyDrawingId);
            // Process the SOP template to get the multiple row
           var properTemplateResult = ProcessSopTemplateResponse(result);

            if (excludeRawMaterial)
            {
                // Dropping a row here also drops its whole subtree, since AppendFlatChildren only ever
                // recurses into children it finds in this same list (templates.Where(t => t.Assembly ==
                // parentDrawingnumber)) - a removed raw-material row can never be reached as a parent either.
                properTemplateResult = properTemplateResult.Where(t => !IsRawMaterial(t.LnItemCode)).ToList();
            }

            // GEt the SOP data for the template drawing Ids
            string drawingNumbers = GetUniqueDrawingNumbers(properTemplateResult, request);

            var sopData = await _sopRepository.GetSopPrecheckData(drawingNumbers);

            var rootBuildAndSnag = await _sopRepository.GetRootSopBuildAndSnag(request.AssemblyDrawingId, request.ProdSeriesId, request.SerielNumberId);

            var response =await GetSopResponse(request, properTemplateResult, sopData, rootBuildAndSnag.Build, rootBuildAndSnag.SnagSheetNo);
            return response;
        }

        // A component counts as raw material when its own drawing's LnItemCode contains neither "WJD" nor
        // "RM" anywhere - matches the convention this codebase already uses elsewhere to distinguish
        // manufactured/assembly items (LnItemCode like "WJD...") from raw material (LnItemCode like
        // "46121600FM..."). A missing LnItemCode is treated as raw material too, since it can't contain
        // either marker.
        private static bool IsRawMaterial(string? lnItemCode)
        {
            if (string.IsNullOrWhiteSpace(lnItemCode))
            {
                return true;
            }

            return !lnItemCode.Contains("WJD", StringComparison.OrdinalIgnoreCase)
                && !lnItemCode.Contains("RM", StringComparison.OrdinalIgnoreCase);
        }



        private List<GetSopTemplateResponse> ProcessSopTemplateResponse(List<GetSopTemplateResponse> originalResponse)
        {
            var result = new List<GetSopTemplateResponse>();

            foreach (var row in originalResponse)
            {
                if (row.DrawingComponentTypeId == 3 && row.Quantity > 1)
                {
                    // Add the original row with updated quantity
                    var originalRow = new GetSopTemplateResponse
                    {
                        Assembly = row.Assembly,
                        AssemblyNumber = row.AssemblyNumber,
                        AssemblyProductSeries = row.AssemblyProductSeries,
                        DrawingNumberId = row.DrawingNumberId,
                        DrawingNumber = row.DrawingNumber,
                        DrawingNomenclature = row.DrawingNomenclature,
                        DrawingComponentTypeId = row.DrawingComponentTypeId,
                        DrawingComponentTypeName = row.DrawingComponentTypeName,
                        DrawingProductSeries = row.DrawingProductSeries,
                        Level = row.Level,
                        IdHierarchyPath = row.IdHierarchyPath,
                        Quantity = 1,
                        Unit = row.Unit,
                        FindNo = row.FindNo
                    };
                    result.Add(originalRow);

                    // Create additional rows with quantity = 1
                    for (int i = 1; i < row.Quantity; i++)
                    {
                        result.Add(new GetSopTemplateResponse
                        {
                            Assembly = row.Assembly,
                            AssemblyNumber = row.AssemblyNumber,
                            AssemblyProductSeries = row.AssemblyProductSeries,
                            DrawingNumberId = row.DrawingNumberId,
                            DrawingNumber = row.DrawingNumber,
                            DrawingNomenclature = row.DrawingNomenclature,
                            DrawingComponentTypeId = row.DrawingComponentTypeId,
                            DrawingComponentTypeName = row.DrawingComponentTypeName,
                            DrawingProductSeries = row.DrawingProductSeries,
                            Level = row.Level,
                            IdHierarchyPath = row.IdHierarchyPath,
                            Quantity = 1,
                            Unit = row.Unit,
                            FindNo = row.FindNo
                        });
                    }
                }
                else
                {
                    // For all other rows, add them as is without any modification
                    result.Add(row);
                }
            }

            return result;
        }
        private string GetUniqueDrawingNumbers(List<GetSopTemplateResponse> drawings, GetSopRequestDto request)
        {
            try
            {
                if (drawings == null || !drawings.Any())
                    return string.Empty;

                var uniqueDrawingNumbers = drawings
                    .Select(x => x.DrawingNumberId)
                    .Distinct()
                    .ToList();

                uniqueDrawingNumbers.Add(request.AssemblyDrawingId);

                return string.Join(",", uniqueDrawingNumbers);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting unique drawing numbers: {ex.Message}");
            }
        }

        // Model classes
        public class ConsumptionDetailsModel
        {
            public string AssemblyNumber { get; set; }
            public string IdNumber { get; set; }
            public string Id { get; set; }
            public int IdIdentity { get; set; }
            public int DrawingNumberId { get; set; }
            public int DrawingId { get; set; }
            public decimal Quantity { get; set; }
            public string MsnNumber { get; set; }
            public string MrirNumber { get; set; }
            public string Remarks { get; set; }
            public int ProdSeriesId { get; set; }
            public string IrNumber { get; set; }
            public string Nomenclature { get; set; }
            public string Unit { get; set; }
            public string ComponentType { get; set; }
            public int ComponentTypeId { get; set; }
            public string ProjectDescription { get; set; }
            public string ProductionOrderNumber { get; set; }
            public string? Build { get; set; }
            public string? SnagSheetNo { get; set; }
            public string ConsumedinProductionOrderNumber { get; set; }
            public string? QrBuildNumber { get; set; }
            public string? ConsumedQrCodeNumber { get; set; }
        }
        // Refactored methods
        public async Task<List<GetSopResponseDto>> GetSopResponse(GetSopRequestDto request, List<GetSopTemplateResponse> templates, List<SopConsumptionResponse> consumptions, string rootBuild, string rootSnagSheetNo)
        {
            var result = new List<GetSopResponseDto>();
            var serialNumberCounter = new SerialNumberCounter { Value = 1 };

            //build top level node 

            var topConsumptionDetails = await GetTopConsumptionDetails(request.AssemblyDrawingId, request.ProdSeriesId, request.SerielNumberId.ToString(), request.SerielNumberId, consumptions, 0);

            var topItem = new GetSopResponseDto()
            {
                SerialNumber = serialNumberCounter.Value++,
                DrawingNumber = topConsumptionDetails.AssemblyNumber,
                IdNumber = topConsumptionDetails.IdNumber.ToString(),
                Nomenclature =topConsumptionDetails.Nomenclature,
                Quantity = topConsumptionDetails.Quantity.ToString(),
                //IrNumber = topConsumptionDetails.IrNumber,
                //MsnNumber = topConsumptionDetails.MsnNumber,
                Remarks = topConsumptionDetails.Remarks,
                AssemblyNumber = null,
                Unit = topConsumptionDetails.Unit,
                DrawingNumberId = request.AssemblyDrawingId,
                ProdSeriesId = request.ProdSeriesId,
                Id = request.SerielNumberId.ToString(),
                Level = 0,
                Build = topConsumptionDetails.Build ?? rootBuild,
                Snag_Sheet_No = topConsumptionDetails.SnagSheetNo ?? rootSnagSheetNo
            };
            result.Add(topItem);

            // Index consumptions once by the (childDrawing, parentDrawing) pair GetConsumptionDetails
            // actually matches on, instead of re-scanning the full list from scratch on every tree node.
            // For a large, widely-reused assembly this list can be almost the entire precheck table
            // (hundreds of thousands of rows) while the tree itself has thousands of nodes - scanning the
            // whole list per node was O(nodes * consumptions), which is what made GetSop hang on big BOMs.
            var consumptionsByKey = consumptions
                .GroupBy(c => (DrawingNumberId: c.DrawingNumberId ?? 0, ConsumedinDrawingNumberId: c.ConsumedinDrawingNumberId))
                .ToDictionary(g => g.Key, g => g.ToList());

            // Recursively build the tree
            await AppendFlatChildren(
                topItem.DrawingNumberId,
                topItem.ProdSeriesId,
                topItem.Id,
                topConsumptionDetails.IdIdentity,
                topItem.DrawingNumberId, // rootDrawingnumber
                topItem.Id,              // rootIdNumber
                topConsumptionDetails.IdIdentity, // rootIdIdentity
                templates,
                consumptionsByKey,
                1,
                serialNumberCounter,
                result,
                topItem);

            result = result.OrderBy(x => x.Level).ThenBy(x => x.SerialNumber).ToList();
            int updatedSerialNumber = 1;

            foreach (var item in result)
            {
                item.SerialNumber = updatedSerialNumber++;
            }

            return new List<GetSopResponseDto> { topItem };
        }

        private async Task AppendFlatChildren(
                    int parentDrawingnumber,
                    int parentProdSeries,
                    string parentIdNumber,
                    int parentIdIdentity,
                    int rootDrawingnumber,
                    string rootIdNumber,
                    int rootIdIdentity,
                    List<GetSopTemplateResponse> templates,
                    Dictionary<(int DrawingNumberId, int ConsumedinDrawingNumberId), List<SopConsumptionResponse>> consumptionsByKey,
                    int sopLevel,
                    SerialNumberCounter serialNumberCounter,
                    List<GetSopResponseDto> accumulator,
                    GetSopResponseDto parentNode)
        {
            // Find child templates
            var childTemplates = templates
                .Where(t =>
                    t.Assembly == parentDrawingnumber
                   )
                .ToList();

            foreach (var tmpl in childTemplates)
            {
                // Get consumption details from the root's precheck data
                var consumptionDetails = GetConsumptionDetails(
                    rootDrawingnumber,
                    parentProdSeries,
                    rootIdNumber,
                    rootIdIdentity,
                    tmpl.DrawingNumberId,
                    consumptionsByKey);

                var childItem = new GetSopResponseDto
                {
                    SerialNumber = serialNumberCounter.Value++,
                    DrawingNumber = tmpl.DrawingNumber,
                    //ProdSeries = tmpl.ParentProdSeries,
                    IdNumber = consumptionDetails.IdNumber,
                    Nomenclature = tmpl.DrawingNomenclature,
                    Quantity = Convert.ToString(consumptionDetails.Quantity),
                    IrNumber = consumptionDetails.IrNumber,
                    MsnNumber = consumptionDetails.MsnNumber,
                    MrirNumber = consumptionDetails.MrirNumber,
                    Remarks = consumptionDetails.Remarks,
                    AssemblyNumber = consumptionDetails.AssemblyNumber,
                    Unit = tmpl.Unit,
                    Level = sopLevel,
                    DrawingNumberId = consumptionDetails.DrawingNumberId,
                    ProdSeriesId = consumptionDetails.ProdSeriesId,
                    Id = consumptionDetails.Id,
                    // For every non-root node, Build is the build number of the specific QR code actually
                    // consumed for THIS component (tbl_qrcodedetails.buildnumber via the row's own qrcodeid),
                    // not the production order's build number - that only applies to the root assembly node
                    // (set separately, above, from rootBuild/pom.buildnumber). A component with no precheck
                    // record of its own - or one whose consumption never recorded a qrcodeid - genuinely has
                    // no build number here, so this is left blank rather than inherited from the parent.
                    Build = consumptionDetails.QrBuildNumber,
                    Snag_Sheet_No = consumptionDetails.SnagSheetNo,
                    FindNo = tmpl.FindNo
                };
                accumulator.Add(childItem);
                
                if (parentNode.Children == null)
                {
                    parentNode.Children = new List<GetSopResponseDto>();
                }
                parentNode.Children.Add(childItem);

                // Pivot context if child has its own precheck record (sub-assembly)
                int currentRootDrawing = rootDrawingnumber;
                string currentRootIdNumber = rootIdNumber;
                int currentRootIdIdentity = rootIdIdentity;

                string pivotIdentifier = consumptionDetails.IrNumber ?? consumptionDetails.MsnNumber;


                if (!IsNullOrEmptyOrNA(pivotIdentifier))
                {
                    string idNumbersOnly = "";
                    //special case for batch
                    if (string.Equals(consumptionDetails.ComponentType, "BATCH", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(consumptionDetails.Remarks))
                        {
                            idNumbersOnly = consumptionDetails.Remarks.Split('-').First().Trim();
                        }
                    }

                    if (string.IsNullOrEmpty(idNumbersOnly))
                    {
                        if (!string.IsNullOrEmpty(consumptionDetails.IdNumber))
                        {
                            idNumbersOnly = consumptionDetails.IdNumber.Split(new char[] { '/', '-' }).Last();
                        }
                        else
                        {
                            idNumbersOnly = consumptionDetails.IdNumber;
                        }
                    }

                    if (!string.IsNullOrEmpty(idNumbersOnly))
                    {
                        int subAssemblyProjectId = await _sopRepository.GetSubAssemblyProjectId(pivotIdentifier, idNumbersOnly);
                        if (subAssemblyProjectId > 0)
                        {
                            currentRootDrawing = consumptionDetails.DrawingNumberId;
                            currentRootIdNumber = idNumbersOnly;
                            currentRootIdIdentity = subAssemblyProjectId;
                        }
                    }
                }

                //// Recurse further
                await AppendFlatChildren(
                    consumptionDetails.DrawingNumberId,
                    consumptionDetails.ProdSeriesId,
                    consumptionDetails.Id,
                    consumptionDetails.IdIdentity,
                    currentRootDrawing,
                    currentRootIdNumber,
                    currentRootIdIdentity,
                    templates,
                    consumptionsByKey,
                    sopLevel + 1,
                    serialNumberCounter,
                    accumulator,
                    childItem);
            }
        }

        private async Task<ConsumptionDetailsModel> GetTopConsumptionDetails(
                 int drawingNumber,
                 int prodSeries,
                 string idNumber,
                 int idIdentity,
                 List<SopConsumptionResponse> consumptions,
                 int sopLevel)
        {
            var allDrawingResponseDtos = await _commonService.GetAllDrawingNumberService();
            var selectedDrawingNumber = allDrawingResponseDtos?.Find(x => x.Id == drawingNumber);

            var allProdSeriesResponseDtos = await _commonService.ProductionSeriesService();
            var selectedProdSeries = allProdSeriesResponseDtos?.Find(x => x.Id == prodSeries);

            // Try to find if this root item itself has precheck data (e.g. if it was prechecked as a child of some other project)
            var matchingConsumption = consumptions?
                .OrderByDescending(c => !IsNullOrEmptyOrNA(c.IrNumber))
                .FirstOrDefault(c =>
                    c.DrawingNumberId == drawingNumber &&
                    (c.Id == idNumber || c.ProjectPrecheckDetailsId == idIdentity || (idIdentity > 0 && c.Id == idIdentity.ToString()))
                );

            return new ConsumptionDetailsModel
            {
                AssemblyNumber = selectedDrawingNumber?.DrawingNumber ?? "N/A",
                IdNumber = (selectedProdSeries?.ProductionSeries ?? "N/A") + "/" + idNumber,
                Id = idNumber,
                IdIdentity = idIdentity,
                DrawingNumberId = drawingNumber,
                ProdSeriesId = prodSeries,
                Quantity = 1,
                Unit = "EA", // Default unit for assembly
                Nomenclature = selectedDrawingNumber?.Nomenclature ?? "N/A",
                IrNumber = IsNullOrEmptyOrNA(matchingConsumption?.IrNumber) ? string.Empty : matchingConsumption.IrNumber,
                MsnNumber = IsNullOrEmptyOrNA(matchingConsumption?.MsnNumber) ? string.Empty : matchingConsumption.MsnNumber,
                MrirNumber = IsNullOrEmptyOrNA(matchingConsumption?.MrirNumber) ? string.Empty : matchingConsumption.MrirNumber,
                Remarks = matchingConsumption?.Remarks,
                ComponentType = matchingConsumption?.ComponentType,
                ComponentTypeId = matchingConsumption?.ComponentTypeId ?? 0,
                ProjectDescription = matchingConsumption?.Remarks,
                Build = matchingConsumption?.Build,
                SnagSheetNo = matchingConsumption?.SnagSheetNo
            };
        }

        private ConsumptionDetailsModel GetConsumptionDetails(
              int drawingNumber,
              int prodSeries,
              string idNumber,
              int idIdentity,
              int childdrawingNumber,
              Dictionary<(int DrawingNumberId, int ConsumedinDrawingNumberId), List<SopConsumptionResponse>> consumptionsByKey)
        {
            // Improved matching logic:
            // 1. Must match child drawing
            // 2. Must match parent drawing
            // 3. Should match parent ID (Identity) OR parent Serial Number (Id)
            //
            // consumptionsByKey is pre-grouped by (DrawingNumberId, ConsumedinDrawingNumberId) - the two
            // required equality conditions - so this call jumps straight to the (usually small) candidate
            // list instead of scanning every consumption row in the whole assembly on every one of the
            // thousands of tree-node calls this runs from. DrawingNumberId/ConsumedinDrawingNumberId are
            // guaranteed by the key itself and don't need re-checking; only the idIdentity/idNumber
            // tiebreak is still evaluated per candidate.
            var candidates = consumptionsByKey.TryGetValue((childdrawingNumber, drawingNumber), out var list)
                ? list
                : new List<SopConsumptionResponse>();

            var matchingConsumption = candidates
                .Where(c =>
                    (idIdentity > 0 && c.ConsumedinIdIdentity == idIdentity) ||
                    (!IsNullOrEmptyOrNA(idNumber) && c.ConsumedinId == idNumber))
                .OrderByDescending(c => !IsNullOrEmptyOrNA(c.IrNumber))
                .ThenByDescending(c => c.ConsumedinProdSeriesId == prodSeries)
                .FirstOrDefault();

            if (matchingConsumption == null)
            {
                // Not logged: a BOM line simply hasn't been prechecked yet, which is routine for any
                // assembly that isn't 100% complete, and this fires once per unmatched template row - a
                // large, mostly-unconsumed assembly can have thousands of them. This app's Serilog setup
                // (Program.cs) hardcodes MinimumLevel.Debug and writes every level to both a file and the
                // console, so no log level here is actually silent - logging this at any level meant
                // thousands of synchronous writes inside a recursive tree-walk, which was the dominant
                // cost behind GetSop's slow response on large BOMs.

                // Return an empty model if there's no match
                return new ConsumptionDetailsModel
                {
                    DrawingNumberId = childdrawingNumber,
                    Id = "0",
                    IdIdentity = 0,
                    Quantity = 1,
                    ProdSeriesId = prodSeries,
                    IrNumber = string.Empty,
                    MsnNumber = string.Empty,
                    MrirNumber = string.Empty,
                    Remarks = string.Empty,
                    ComponentType = string.Empty,
                    ComponentTypeId = 0,
                    ProjectDescription = string.Empty
                };
            }
            
            candidates.Remove(matchingConsumption);

            // Just convert Quantity to string (or store as decimal in the model if you prefer)
            return new ConsumptionDetailsModel
            {

                AssemblyNumber = matchingConsumption.ConsumedInDrawing,
                IdNumber = matchingConsumption.IdNumber,
                Id = matchingConsumption.Id,
                IdIdentity = matchingConsumption.ProjectPrecheckDetailsId,
                DrawingNumberId = matchingConsumption.DrawingNumberId ?? 0,
                Quantity = matchingConsumption.Quantity,
                Unit = matchingConsumption.Unit ?? string.Empty,
                MsnNumber = IsNullOrEmptyOrNA(matchingConsumption.MsnNumber) ? string.Empty : matchingConsumption.MsnNumber,
                MrirNumber = IsNullOrEmptyOrNA(matchingConsumption.MrirNumber) ? string.Empty : matchingConsumption.MrirNumber,
                Remarks = matchingConsumption.Remarks?.ToString() ?? string.Empty,
                ProdSeriesId = matchingConsumption.ProdSeriesId ?? 0,
                IrNumber = IsNullOrEmptyOrNA(matchingConsumption.IrNumber) ? string.Empty : matchingConsumption.IrNumber,
                Nomenclature = matchingConsumption.NomenclatureId?.ToString() ?? string.Empty,
                // Return the direct consumption fields as well
                DrawingId = matchingConsumption.DrawingNumberId ?? 0,
                ComponentType = matchingConsumption.ComponentType,
                ComponentTypeId = matchingConsumption.ComponentTypeId,
                ProjectDescription = matchingConsumption.Remarks?.ToString() ?? string.Empty,
                Build = matchingConsumption.Build,
                SnagSheetNo = matchingConsumption.SnagSheetNo,
                QrBuildNumber = matchingConsumption.QrBuildNumber,
                ConsumedQrCodeNumber = matchingConsumption.ConsumedQrCodeNumber
            };
        }



        public byte[] ExportToExcel(List<GetSopResponseDto> items, string projectId)
        {
            var flatItems = new List<GetSopResponseDto>();
            void Flatten(GetSopResponseDto node)
            {
                flatItems.Add(node);
                if (node.Children != null)
                {
                    foreach (var child in node.Children)
                    {
                        Flatten(child);
                    }
                }
            }

            if (items != null)
            {
                foreach (var item in items)
                {
                    Flatten(item);
                }
            }

            // Flatten() above already walks the tree depth-first (each node immediately followed by its
            // own children before moving to the next sibling) - resorting by Level here would instead group
            // all level-1 rows together, then all level-2 rows together, etc. across every branch, breaking
            // the assembly -> its children structure the export is supposed to mirror.
            var sortedFlatItems = flatItems;

            using (var workbook = new XSSFWorkbook())
            {
                var sheet = workbook.CreateSheet("SOP");

                // Create fonts
                var boldFont = workbook.CreateFont();
                boldFont.IsBold = true;
                boldFont.FontHeightInPoints = 10;
                
                var redBoldFont = workbook.CreateFont();
                redBoldFont.IsBold = true;
                redBoldFont.Color = IndexedColors.Red.Index;
                redBoldFont.FontHeightInPoints = 10;

                var normalFont = workbook.CreateFont();
                normalFont.FontHeightInPoints = 10;

                // Create styles
                var borderStyleLeft = workbook.CreateCellStyle();
                borderStyleLeft.BorderTop = BorderStyle.Thin;
                borderStyleLeft.BorderBottom = BorderStyle.Thin;
                borderStyleLeft.BorderLeft = BorderStyle.Thin;
                borderStyleLeft.BorderRight = BorderStyle.Thin;
                borderStyleLeft.Alignment = HorizontalAlignment.Left;
                borderStyleLeft.VerticalAlignment = VerticalAlignment.Center;
                borderStyleLeft.SetFont(normalFont);

                var borderStyleCenter = workbook.CreateCellStyle();
                borderStyleCenter.BorderTop = BorderStyle.Thin;
                borderStyleCenter.BorderBottom = BorderStyle.Thin;
                borderStyleCenter.BorderLeft = BorderStyle.Thin;
                borderStyleCenter.BorderRight = BorderStyle.Thin;
                borderStyleCenter.Alignment = HorizontalAlignment.Center;
                borderStyleCenter.VerticalAlignment = VerticalAlignment.Center;
                borderStyleCenter.SetFont(normalFont);

                var boldStyleLeft = workbook.CreateCellStyle();
                boldStyleLeft.CloneStyleFrom(borderStyleLeft);
                boldStyleLeft.SetFont(boldFont);

                var boldStyleCenter = workbook.CreateCellStyle();
                boldStyleCenter.CloneStyleFrom(borderStyleCenter);
                boldStyleCenter.SetFont(boldFont);

                var titleStyle = workbook.CreateCellStyle();
                titleStyle.CloneStyleFrom(borderStyleLeft);
                titleStyle.SetFont(boldFont);
                titleStyle.WrapText = true;

                var rightTopStyle = workbook.CreateCellStyle();
                rightTopStyle.CloneStyleFrom(borderStyleLeft);
                rightTopStyle.SetFont(boldFont);

                var rightBottomStyle = workbook.CreateCellStyle();
                rightBottomStyle.CloneStyleFrom(borderStyleCenter);
                rightBottomStyle.SetFont(redBoldFont);

                var headerStyle = workbook.CreateCellStyle();
                headerStyle.CloneStyleFrom(borderStyleCenter);
                headerStyle.SetFont(boldFont);

                // Initialize top 3 rows
                var row0 = sheet.CreateRow(0); row0.HeightInPoints = 20;
                var row1 = sheet.CreateRow(1); row1.HeightInPoints = 20;
                var row2 = sheet.CreateRow(2); row2.HeightInPoints = 20;

                // Ensure all cells in the merged regions are created with borders so they render correctly
                for (int r = 0; r <= 2; r++)
                {
                    var currentRow = sheet.GetRow(r);
                    for (int c = 0; c <= 10; c++)
                    {
                        var cell = currentRow.CreateCell(c);
                        cell.CellStyle = borderStyleLeft; // Default border
                    }
                }

                // Add merged regions
                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 2, 0, 1)); // Logo (A1:B3)
                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 2, 2, 5)); // Title (C1:F3)
                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 6, 9)); // Doc No (G1:J1)
                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(1, 1, 6, 9)); // Assembly No (G2:J2)
                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(2, 2, 6, 9)); // ID No (G3:J3)

                // Title Cell (C1)
                var titleCell = row0.GetCell(2);
                string titleNomenclature = items?.FirstOrDefault()?.Nomenclature ?? "Assembly";
                titleCell.SetCellValue($"Standard of Preparation for \"{titleNomenclature}\"\nProject: - GLP/4");
                titleCell.CellStyle = titleStyle;

                // Right top cells
                var docCell = row0.GetCell(6);
                docCell.SetCellValue("Doc.No. SOP/F3/SH/ 02");
                docCell.CellStyle = rightTopStyle;

                var assemblyCell = row1.GetCell(6);
                string assemblyNumber = projectId ?? "";
                assemblyCell.SetCellValue($"Assembly No: {assemblyNumber}");
                assemblyCell.CellStyle = rightTopStyle;

                var idCell = row2.GetCell(6);
                string idNumber = items?.FirstOrDefault()?.IdNumber ?? "";
                idCell.SetCellValue($"ID No: {idNumber}");
                idCell.CellStyle = rightBottomStyle;

                // Logo
                try 
                {
                    string contentPath = Path.Combine(Directory.GetCurrentDirectory(), "Content", "godrej_logo.jpeg");
                    if(File.Exists(contentPath))
                    {
                        byte[] logoBytes = File.ReadAllBytes(contentPath);
                        int pictureIdx = workbook.AddPicture(logoBytes, PictureType.JPEG);
                        ICreationHelper helper = workbook.GetCreationHelper();
                        IDrawing drawing = sheet.CreateDrawingPatriarch();
                        IClientAnchor anchor = helper.CreateClientAnchor();
                        anchor.Col1 = 0;
                        anchor.Row1 = 0;
                        anchor.Col2 = 2;
                        anchor.Row2 = 3;
                        anchor.AnchorType = AnchorType.MoveAndResize;
                        IPicture picture = drawing.CreatePicture(anchor, pictureIdx);
                    }
                } 
                catch { /* Ignore logo if not found */ }

                // Table Headers
                var tableHeaderRow = sheet.CreateRow(3);
                tableHeaderRow.HeightInPoints = 25;
                string[] headers = new string[]
                {
                    "Sr No","Level", "Position Number", "Drawing No.", "Nomenclature", "Build number",
                    "Qty", "ID No", "IR No", "MSN", "MRIR Number", "Snag Sheet Number", "Remarks"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = tableHeaderRow.CreateCell(i);
                    cell.SetCellValue(headers[i]);
                    cell.CellStyle = headerStyle;
                }

                // Add data rows
                int rowNum = 4;
                foreach (var item in sortedFlatItems)
                {
                    // Any node that has its own children (the root, or a sub-assembly further down
                    // the tree) is a parent to something below it - bold it so it stands out from the
                    // components listed under it.
                    bool isParentAssembly = item.Children != null && item.Children.Count > 0;
                    var cellStyleLeft = isParentAssembly ? boldStyleLeft : borderStyleLeft;
                    var cellStyleCenter = isParentAssembly ? boldStyleCenter : borderStyleCenter;

                    var row = sheet.CreateRow(rowNum++);
                    CreateCell(row, 0, item.SerialNumber.ToString(), cellStyleCenter);
                    CreateCell(row, 1, item.Level.ToString(), cellStyleCenter); // NEW
                    CreateCell(row, 2, item.FindNo, cellStyleCenter);
                    CreateCell(row, 3, item.DrawingNumber, cellStyleLeft);
                    CreateCell(row, 4, item.Nomenclature, cellStyleLeft);
                    CreateCell(row, 5, item.Build, cellStyleCenter);
                    CreateCell(row, 6, item.Quantity, cellStyleCenter);
                    CreateCell(row, 7, item.IdNumber, cellStyleCenter);
                    CreateCell(row, 8, item.IrNumber, cellStyleCenter);
                    CreateCell(row, 9, item.MsnNumber, cellStyleCenter);
                    CreateCell(row, 10, item.MrirNumber, cellStyleCenter);
                    CreateCell(row, 11, item.Snag_Sheet_No, cellStyleCenter);
                    CreateCell(row, 12, item.Remarks, cellStyleLeft);
                }

                // Column Widths
                sheet.SetColumnWidth(0, 6 * 256);   // Sr No
                sheet.SetColumnWidth(1, 8 * 256);   // Level (NEW)
                sheet.SetColumnWidth(2, 14 * 256);  // Position Number
                sheet.SetColumnWidth(3, 20 * 256);  // Drawing No
                sheet.SetColumnWidth(4, 25 * 256);  // Nomenclature
                sheet.SetColumnWidth(5, 10 * 256);  // Build
                sheet.SetColumnWidth(6, 6 * 256);   // Qty
                sheet.SetColumnWidth(7, 12 * 256);  // ID No
                sheet.SetColumnWidth(8, 15 * 256);  // IR No
                sheet.SetColumnWidth(9, 12 * 256);  // MSN
                sheet.SetColumnWidth(10, 15 * 256); // MRIR Number
                sheet.SetColumnWidth(11, 15 * 256); // Snag Sheet No
                sheet.SetColumnWidth(12, 20 * 256); // Remarks

                // Convert to byte array
                using (var ms = new MemoryStream())
                {
                    workbook.Write(ms);
                    return ms.ToArray();
                }
            }
        }
        
        private static void CreateCell(IRow row, int column, string value, ICellStyle style)
        {
            var cell = row.CreateCell(column);
            cell.SetCellValue(value);
            cell.CellStyle = style;
        }

        #region BOM Details Methods

        /// <summary>
        /// Get recursive BOM details by assembly number.
        /// Returns complete hierarchical tree of child components.
        /// </summary>
        public async Task<List<BomDetailsResponseDto>> GetBomDetails(string assemblyNumber)
        {
            _logger.LogInformation($"SopService:GetBomDetails - Getting BOM for assembly: {assemblyNumber}");
            try
            {
                if (string.IsNullOrWhiteSpace(assemblyNumber))
                {
                    throw new ArgumentException("Assembly number is required");
                }

                var result = await _sopRepository.GetRecursiveBomByAssembly(assemblyNumber);

                // Process result to identify which items have children
                if (result != null && result.Any())
                {
                    var childDrawingIds = result.Select(r => r.ChildDrawingId).ToHashSet();
                    foreach (var item in result)
                    {
                        item.HasChildren = result.Any(r => r.ParentDrawingId == item.ChildDrawingId);
                        item.IsExpanded = item.Level == 0; // Auto-expand first level
                    }
                }

                _logger.LogInformation($"SopService:GetBomDetails - Found {result?.Count ?? 0} BOM items for assembly: {assemblyNumber}");
                return result ?? new List<BomDetailsResponseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting BOM details for assembly: {assemblyNumber}");
                throw;
            }
        }

        /// <summary>
        /// Search for assembly numbers by partial match.
        /// </summary>
        public async Task<List<AssemblySearchResponseDto>> SearchAssemblyNumbers(string searchText)
        {
            _logger.LogInformation($"SopService:SearchAssemblyNumbers - Searching for: {searchText}");
            try
            {
                var result = await _sopRepository.SearchAssemblyNumbers(searchText ?? "");
                _logger.LogInformation($"SopService:SearchAssemblyNumbers - Found {result?.Count ?? 0} assemblies");
                return result ?? new List<AssemblySearchResponseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching assembly numbers: {searchText}");
                throw;
            }
        }

        /// <summary>
        /// Export BOM details to Excel.
        /// </summary>
        public byte[] ExportBomToExcel(List<BomDetailsResponseDto> items, string assemblyNumber)
        {
            _logger.LogInformation($"SopService:ExportBomToExcel - Exporting {items?.Count ?? 0} items for assembly: {assemblyNumber}");
            
            using (var workbook = new XSSFWorkbook())
            {
                var sheet = workbook.CreateSheet("BOM Details");

                // Create styles
                var headerStyle = workbook.CreateCellStyle();
                var headerFont = workbook.CreateFont();
                headerFont.IsBold = true;
                headerFont.FontHeightInPoints = 11;
                headerStyle.SetFont(headerFont);
                headerStyle.FillForegroundColor = IndexedColors.Grey25Percent.Index;
                headerStyle.FillPattern = FillPattern.SolidForeground;
                headerStyle.Alignment = HorizontalAlignment.Center;
                headerStyle.VerticalAlignment = VerticalAlignment.Center;
                headerStyle.BorderTop = BorderStyle.Thin;
                headerStyle.BorderBottom = BorderStyle.Thin;
                headerStyle.BorderLeft = BorderStyle.Thin;
                headerStyle.BorderRight = BorderStyle.Thin;

                var borderStyle = workbook.CreateCellStyle();
                borderStyle.BorderTop = BorderStyle.Thin;
                borderStyle.BorderBottom = BorderStyle.Thin;
                borderStyle.BorderLeft = BorderStyle.Thin;
                borderStyle.BorderRight = BorderStyle.Thin;

                // Title row
                var titleRow = sheet.CreateRow(0);
                var titleCell = titleRow.CreateCell(0);
                titleCell.SetCellValue($"BOM Details for Assembly: {assemblyNumber}");
                var titleStyle = workbook.CreateCellStyle();
                var titleFont = workbook.CreateFont();
                titleFont.IsBold = true;
                titleFont.FontHeightInPoints = 14;
                titleStyle.SetFont(titleFont);
                titleCell.CellStyle = titleStyle;
                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 0, 12));

                // Add table headers
                var tableHeaderRow = sheet.CreateRow(2);
                string[] headers = new string[]
                {
                    "Level", "Drawing Number", "Nomenclature", "LN Item Code", "Component Type",
                    "Qty", "Parent Drawing", "Find No"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = tableHeaderRow.CreateCell(i);
                    cell.SetCellValue(headers[i]);
                    cell.CellStyle = headerStyle;
                }

                // Add data rows
                int rowNum = 3;
                int serialNumber = 1;
                foreach (var item in items ?? new List<BomDetailsResponseDto>())
                {
                    var row = sheet.CreateRow(rowNum++);
                    CreateCell(row, 0, item.Level.ToString(), borderStyle);
                    CreateCell(row, 1, item.ChildDrawingNumber ?? "", borderStyle);
                    CreateCell(row, 2, item.Nomenclature ?? "", borderStyle);
                    CreateCell(row, 3, item.LnItemCode ?? "", borderStyle);
                    CreateCell(row, 4, item.ComponentType ?? "", borderStyle);
                    CreateCell(row, 5, item.Quantity?.ToString() ?? "", borderStyle);
                    CreateCell(row, 6, item.ParentDrawingNumber ?? "", borderStyle);
                    CreateCell(row, 7, item.FindNo ?? "", borderStyle);
                    serialNumber++;
                }

                // Auto-size columns
                for (int i = 0; i < headers.Length; i++)
                {
                    sheet.AutoSizeColumn(i);
                    if (sheet.GetColumnWidth(i) < 3000)
                    {
                        sheet.SetColumnWidth(i, 3000);
                    }
                }

                // Convert to byte array
                using (var ms = new MemoryStream())
                {
                    workbook.Write(ms);
                    return ms.ToArray();
                }
            }
        }

        #endregion
    }
}
