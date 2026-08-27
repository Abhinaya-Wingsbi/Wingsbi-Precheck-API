using ClosedXML.Excel;
using Godrej.Precheck.Models.DataModel.ProductionOrder;
using Godrej.Precheck.Models.DTOs.Precheck;
using Godrej.Precheck.Models.DTOs.ProductionOrder;
using Godrej.Precheck.Repository.Repository.PrecheckRepository;
using Godrej.Precheck.Repository.Repository.ProductionOrderRepository;
using Godrej.Precheck.Repository.Repository.SopRepository;
using Mapster;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Godrej.Precheck.Service.Service.ProductionOrderService
{
    public class ProductionOrderService : IProductionOrderService
    {
        private readonly IProductionOrderRepository _productionOrderRepository;
        private readonly IPrecheckRepository _precheckRepository;
        private readonly ISopRepository _sopRepository;
        private readonly ILogger<ProductionOrderService> _logger;
        private readonly IMemoryCache _cache;

        //cache key for all production orders list - used in GetAllProductionOrdersAsync for non-filtered list to improve performance
        private const string CacheKey = "po_list_all";

        private const string CacheKeyPrefix = "ProductionOrders_";
        private const int CacheTtlSeconds = 30;

        public ProductionOrderService(
            IProductionOrderRepository productionOrderRepository,
            IPrecheckRepository precheckRepository,
            ISopRepository sopRepository,
            ILogger<ProductionOrderService> logger,
            IMemoryCache cache)
        {
            _productionOrderRepository = productionOrderRepository;
            _precheckRepository = precheckRepository;
            _sopRepository = sopRepository;
            _logger = logger;
            _cache = cache;
        }

        public async Task<ProductionOrderMasterDto?> GetByProductionOrderNumberAsync(string productionOrderNumber)
        {
            return await _productionOrderRepository.GetByProductionOrderNumberAsync(productionOrderNumber);
        }

        public async Task<List<ProductionOrderMasterDto>> GetAllProductionOrdersAsync(int roleId)
        {
            var roleCacheKey = $"{CacheKeyPrefix}Role_{roleId}";

            if (_cache.TryGetValue(roleCacheKey, out List<ProductionOrderMasterDto> cached))
                return cached;

            // Base data cache (shared across roles, avoids redundant DB hits)
            if (!_cache.TryGetValue(CacheKeyPrefix + "Base", out List<ProductionOrderMasterDto> orders))
            {
                orders = await _productionOrderRepository.GetAllProductionOrdersAsync();
                _cache.Set(CacheKeyPrefix + "Base", orders, TimeSpan.FromSeconds(CacheTtlSeconds));
            }

            var leveled = await ApplyBomLevelingByRoleAsync(orders, roleId);

            // Cache the role-specific result too
            _cache.Set(roleCacheKey, leveled, TimeSpan.FromSeconds(CacheTtlSeconds));

            return leveled;
        }

        // Add this method for cache invalidation when POs are created/updated
        public void InvalidateCache()
        {
            _cache.Remove(CacheKey);                              // ← clears filtered cache
            _cache.Remove(CacheKeyPrefix + "Base");               // ← clears base cache
            foreach (var roleId in new[] { 1, 2, 3, 12 })
                _cache.Remove($"{CacheKeyPrefix}Role_{roleId}");  // ← clears role caches
        }

        public async Task<List<ProductionOrderMasterDto>> GetAllProductionOrdersAsync(
            string? dateFilterType,
            DateTime? filterDate,
            DateTime? fromDate,
            DateTime? toDate,
            int? precheckStatus,
            string? poNumber,
            string? lnItemCode,
            int roleId = 0,
            string? drawingNumber = null)
        {
            var orders = await _productionOrderRepository.GetAllProductionOrdersAsync(
                dateFilterType, filterDate, fromDate, toDate, precheckStatus, poNumber, lnItemCode, drawingNumber);
           
            return await ApplyBomLevelingByRoleAsync(orders, roleId);
        }

        private async Task<List<ProductionOrderMasterDto>> ApplyBomLevelingByRoleAsync(
     List<ProductionOrderMasterDto> orders, int roleId)
        {
            // Admin/Planner — return everything unfiltered
            if (roleId != 2 && roleId != 3)
                return orders;

            // Pre-filter uploaded status (no DB needed)
            var candidates = orders
                .Where(o => o.PrecheckStatus != 4)
                .ToList();

            // Collect all unique drawing numbers that need BOM lookup
            var drawingNumbers = candidates
                .Where(o => !string.IsNullOrEmpty(o.DrawingNumber))
                .Select(o => o.DrawingNumber)
                .Distinct()
                .ToList();

            // ✅ ONE DB call instead of 29
            Dictionary<string, int> bomCounts = new();
            if (drawingNumbers.Any())
            {
                try
                {
                    bomCounts = await _sopRepository.GetBomComponentCountsAsync(drawingNumbers);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch bulk BOM counts");
                }
            }

            // Filter in memory — zero DB calls
            var filteredOrders = new List<ProductionOrderMasterDto>();

            _logger.LogInformation("ApplyBomLevelingByRoleAsync: RoleId={RoleId}, Candidates={CandidateCount}, DrawingNumbers={DrawingCount}, BomCountsReturned={BomCountsCount}",
    roleId, candidates.Count, drawingNumbers.Count, bomCounts.Count);

            foreach (var order in candidates)
            {
                if (string.IsNullOrEmpty(order.DrawingNumber))
                {
                    if (roleId == 3) filteredOrders.Add(order); // Store sees no-drawing orders
                    continue;
                }

                bomCounts.TryGetValue(order.DrawingNumber, out int count);

                if (roleId == 2 && count <= 1) filteredOrders.Add(order); // QC: exactly 1 component
                if (roleId == 3 && count > 1) filteredOrders.Add(order); // Store: more than 1
            }

            return filteredOrders;
        }

        public async Task<ProductionOrderDetailsDto> GetProductionOrderDetailsAsync(string productionOrderNumber)
        {
            _logger.LogInformation("Fetching Production Order Details for PO: {PO}", productionOrderNumber);

            var master = await _productionOrderRepository.GetByProductionOrderNumberAsync(productionOrderNumber);
            if (master == null)
            {
                throw new Exception($"Production Order '{productionOrderNumber}' not found");
            }
            var bomItems = new List<MakeOrderResponseDto>();
            if (master.DrawingNumberId.HasValue && master.Quantity.HasValue)
            {
                var result = await _precheckRepository.GetPrecheckTemplateResponsesAsync(master.DrawingNumberId.Value);
                bomItems = result.Adapt<List<MakeOrderResponseDto>>();

                // Calculate TotalQuantity = Quantity × number of IDs in this production order
                bomItems.ForEach(item => item.TotalQuantity = item.Quantity * master.Quantity);

                // Calculate AvailableQuantity and TotalQrQty from stored QR codes for each component
                foreach (var item in bomItems)
                {
                    // Calculate available count of QR codes
                    item.AvailableQuantity = await _precheckRepository.GetAvailableComponentQunatity(item.DrawingNumberId);

                    // Fetch the actual components to sum up the available quantities
                    var childRequest = new GetAvailableComponentsRequest
                    {
                        DrawingNumberId = item.DrawingNumberId
                    };
                    var childResults = await _precheckRepository.GetAvailableComponentForOrder(childRequest);
                    item.TotalQrQty = childResults.Sum(x => x.RemainingQuantity);
                }
            }

            return new ProductionOrderDetailsDto
            {
                Master = master,
                BomItems = bomItems
            };
        }

        public async Task<ProductionOrderUploadResultDto> UploadExcelAsync(Stream fileStream, int createdBy)
        {
            var result = new ProductionOrderUploadResultDto();
            var rows = new List<ProductionOrderUploadRowDto>();

            try
            {
                // Parse Excel
                using var workbook = new XLWorkbook(fileStream);
                var worksheet = workbook.Worksheets.First();
                var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;

                for (int i = 2; i <= rowCount; i++) // Skip header row
                {
                    var row = new ProductionOrderUploadRowDto
                    {
                        ProductionOrderNumber = worksheet.Cell(i, 1).GetString()?.Trim(),
                        ProjectCode = worksheet.Cell(i, 2).GetString()?.Trim(),
                        ProjectDescription = worksheet.Cell(i, 3).GetString()?.Trim(),
                        ItemCode = worksheet.Cell(i, 4).GetString()?.Trim(),
                        ItemDescription = worksheet.Cell(i, 5).GetString()?.Trim(),
                        StartIdNumber = worksheet.Cell(i, 6).GetString()?.Trim(),
                        Quantity = worksheet.Cell(i, 7).TryGetValue(out int qty) ? qty : 0,
                        MRIRNumber = worksheet.Cell(i, 8).GetString()?.Trim(),
                        MIN=worksheet.Cell(i, 9).GetString()?.Trim(),
                        Status=worksheet.Cell(i, 10).GetString()?.Trim(),
                        BuildNumber = worksheet.Cell(i, 11).GetString()?.Trim(),
                        SnagSheetNo = worksheet.Cell(i, 12).GetString()?.Trim()
                    };

                    if (!string.IsNullOrEmpty(row.ProductionOrderNumber))
                    {
                        rows.Add(row);
                    }
                }

                result.TotalRows = rows.Count;
                
                // Process each row
                foreach (var row in rows)
                {
                    try
                    {
                        await ProcessRowAsync(row, createdBy);
                        result.Imported++;
                    }
                    catch (Exception ex)
                    {
                        result.Skipped++;
                        result.Errors.Add($"Row '{row.ProductionOrderNumber}': {ex.Message}");
                        _logger.LogWarning(ex, "Error processing row {PO}", row.ProductionOrderNumber);
                    }
                }
                InvalidateCache();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Excel file");
                result.Errors.Add($"Excel parsing error: {ex.Message}");
            }

            return result;
        }
        private async Task ProcessRowAsync(ProductionOrderUploadRowDto row, int createdBy)
        {
            // 1. Parse Start ID (e.g., "GA0153" -> prefix="GA", startNo=153)
            var (prefix, startNo) = ParseStartIdNumber(row.StartIdNumber);
            if (prefix == null || startNo == null)
            {
                throw new Exception($"Invalid Start ID format: '{row.StartIdNumber}'");
            }

            // 2. Lookup ProdSeriesId by prefix
            var (prodSeriesId, _) = await _productionOrderRepository.LookupProdSeriesByPrefixAsync(prefix);
            if (prodSeriesId == null)
            {
                throw new Exception($"Production Series '{prefix}' not found");
            }

            // 3. Check if already exists
            var exists = await _productionOrderRepository.CheckPOExistsAsync(
                row.ProductionOrderNumber!, prodSeriesId.Value, startNo.Value);
            if (exists)
            {
                throw new Exception($"Production Order already exists");
            }

            // 4. Lookup DrawingNumberId by LnItemCode
            var (drawingNumberId, lnItemCodeId, _, _) = await _productionOrderRepository.LookupDrawingByLnItemCodeAsync(row.ItemCode!);
            if (drawingNumberId == null)
            {
                throw new Exception($"Item Code '{row.ItemCode}' not found in drawing mapping");
            }

            // 4a. Check the new ID range [StartIdNumber, StartIdNumber+Quantity-1] doesn't overlap
            // an existing range already used under the same LnItemCode + ProdSeries pair
            var overlapResult = await _productionOrderRepository.CheckProdSeriesStartIdOverlapAsync(prodSeriesId!.Value, lnItemCodeId!.Value, startNo.Value, row.Quantity);
            if (overlapResult.HasOverlap)
            {
                var suggestedStartId = (overlapResult.MaxEndIdNumber ?? 0) + 1;
                throw new Exception(
                    $"with Start ID Number '{row.StartIdNumber}' are already in use. " +
                    $"Please use Start ID Number '{suggestedStartId} or higher' to avoid overlap.");
            }

            // 5. Insert into ProductionOrderMaster
            var master = new ProductionOrderMaster
            {
                ProductionOrderNumber = row.ProductionOrderNumber!,
                ProjectNumber = row.ProjectCode,
                ProjectDescription = row.ProjectDescription,
                LnItemCode = row.ItemCode,
                ItemDescription = row.ItemDescription,
                ProdSeriesId = prodSeriesId,
                StartIdNumber = startNo,
                Quantity = row.Quantity,
                DrawingNumberId = drawingNumberId,
                LnItemCodeId = lnItemCodeId,
                CreatedBy = createdBy,
                MRIRNumber = row.MRIRNumber,
                MIN = row.MIN,
                Status = row.Status,
                BuildNumber = row.BuildNumber,
                SnagSheetNo = row.SnagSheetNo
            };

            var masterId = await _productionOrderRepository.InsertProductionOrderMasterAsync(master);

            // 6. Get Assembly Mapping for child components
            var assemblyTemplate = await _precheckRepository.GetPrecheckTemplateResponsesAsync(drawingNumberId.Value);

            // 7. Create ProjectDetails and ProjectPrecheckDetails for each ID in range.
            // All inserts below share one open connection instead of each opening its own,
            // since this loop can run hundreds of times per row.
            int endNo = startNo.Value + row.Quantity - 1;
            using (var connection = await _productionOrderRepository.CreateOpenConnectionAsync())
            {
                for (int idNumber = startNo.Value; idNumber <= endNo; idNumber++)
                {
                    // Insert ProjectDetails
                    var projectDetailsId = await _productionOrderRepository.InsertProjectDetailsWithPOIdAsync(
                        idNumber,
                        prodSeriesId.Value,
                        row.ProjectCode ?? row.ProductionOrderNumber!,
                        row.ProductionOrderNumber!,
                        drawingNumberId.Value,
                        masterId,
                        createdBy,
                        connection);

                    // Insert ProjectPrecheckDetails for each child component
                    foreach (var child in assemblyTemplate)
                    {
                        if (child.ComponentType == "ID")
                        {
                            for (int j = 0; j < (child.Quantity ?? 1); j++)
                            {
                                await _productionOrderRepository.InsertProjectPrecheckDetailsWithPOIdAsync(
                                    child.DrawingNumberId,
                                    prodSeriesId.Value,
                                    projectDetailsId,
                                    1,
                                    child.ComponentType,
                                    masterId,
                                    createdBy,
                                    connection);
                            }
                        }
                        else
                        {
                            await _productionOrderRepository.InsertProjectPrecheckDetailsWithPOIdAsync(
                                child.DrawingNumberId,
                                prodSeriesId.Value,
                                projectDetailsId,
                                child.Quantity ?? 1,
                                child.ComponentType,
                                masterId,
                                createdBy,
                                connection);
                        }
                    }
                }
            }

            _logger.LogInformation("Successfully processed PO: {PO} with {Count} IDs",
                row.ProductionOrderNumber, row.Quantity);
        }

        public async Task<bool> UpdateProductionOrderAsync(UpdateProductionOrderDto dto, int updatedBy)
        {
            _logger.LogInformation("Updating Production Order: {PO}", dto.ProductionOrderNumber);

            var existingPO = await _productionOrderRepository.GetByProductionOrderNumberUpdatePOAsync(dto.ProductionOrderNumber, dto.Id);
            if (existingPO == null)
            {
                throw new Exception($"Production Order '{dto.ProductionOrderNumber}' does not exist");
            }

            if (!dto.StartIdNumber.HasValue)
            {
                throw new Exception("Start ID Number is required");
            }
            int startNo = dto.StartIdNumber.Value;

            if (!dto.ProdSeriesId.HasValue)
            {
                throw new Exception("Production Series ID is required");
            }
            int prodSeriesId = dto.ProdSeriesId.Value;

            var (drawingNumberId, lnItemCodeId, _, _) = await _productionOrderRepository.LookupDrawingByLnItemCodeAsync(dto.ItemCode!);
            if (drawingNumberId == null)
            {
                throw new Exception($"Item Code '{dto.ItemCode}' not found in drawing mapping");
            }

            // Update Master
            var master = new ProductionOrderMaster
            {
                ProductionOrderNumber = dto.ProductionOrderNumber,
                ProjectNumber = dto.ProjectCode,
                ProjectDescription = dto.ProjectDescription,
                LnItemCode = dto.ItemCode,
                ItemDescription = dto.ItemDescription,
                ProdSeriesId = prodSeriesId,
                StartIdNumber = startNo,
                Quantity = dto.Quantity,
                DrawingNumberId = drawingNumberId,
                LnItemCodeId = lnItemCodeId,
                MRIRNumber = dto.MRIRNumber,
                Id = dto.Id,
                MIN = dto.Min,
                BuildNumber = dto.BuildNumber,
                SnagSheetNo = dto.SnagSheetNo
            };

            await _productionOrderRepository.UpdateProductionOrderMasterAsync(master, updatedBy);

            // Delete old details and recreate
            await _productionOrderRepository.DeleteProjectDetailsWithPOIdAsync(existingPO.Id);

            var assemblyTemplate = await _precheckRepository.GetPrecheckTemplateResponsesAsync(drawingNumberId.Value);

            int endNo = startNo + (dto.Quantity ?? 0) - 1;
            for (int idNumber = startNo; idNumber <= endNo; idNumber++)
            {
                var projectDetailsId = await _productionOrderRepository.InsertProjectDetailsWithPOIdAsync(
                    idNumber,
                    prodSeriesId,
                    dto.ProjectCode ?? dto.ProductionOrderNumber,
                    dto.ProductionOrderNumber,
                    drawingNumberId.Value,
                    existingPO.Id,
                    updatedBy);

                foreach (var child in assemblyTemplate)
                {
                    if (child.ComponentType == "ID")
                    {
                        for (int j = 0; j < (child.Quantity ?? 1); j++)
                        {
                            await _productionOrderRepository.InsertProjectPrecheckDetailsWithPOIdAsync(
                                child.DrawingNumberId,
                                prodSeriesId,
                                projectDetailsId,
                                1,
                                child.ComponentType,
                                existingPO.Id,
                                updatedBy);
                        }
                    }
                    else
                    {
                        await _productionOrderRepository.InsertProjectPrecheckDetailsWithPOIdAsync(
                            child.DrawingNumberId,
                            prodSeriesId,
                            projectDetailsId,
                            child.Quantity ?? 1,
                            child.ComponentType,
                            existingPO.Id,
                            updatedBy);
                    }
                }
            }

            _logger.LogInformation("Successfully updated PO: {PO} and regenerated details", dto.ProductionOrderNumber);
            InvalidateCache();
            return true;
        }

        public async Task<byte[]> DownloadTemplateAsync()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Production Order");

            // Set Headers
            worksheet.Cell(1, 1).Value = "Production Order";
            worksheet.Cell(1, 2).Value = "Project Code";
            worksheet.Cell(1, 3).Value = "Project Description";
            worksheet.Cell(1, 4).Value = "Item Code";
            worksheet.Cell(1, 5).Value = "Item Description";
            worksheet.Cell(1, 6).Value = "Start ID Number";
            worksheet.Cell(1, 7).Value = "Quantity";
            worksheet.Cell(1, 8).Value = "MRIRNumber";
            worksheet.Cell(1, 9).Value = "MIN";
            worksheet.Cell(1, 10).Value="Status";
            worksheet.Cell(1, 11).Value = "Build Number";
            worksheet.Cell(1, 12).Value = "Snag Sheet Number";
            
            // Formatting
            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#E11584"); // Godrej Pink
            headerRow.Style.Font.FontColor = XLColor.White;
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static (string? Prefix, int? Number) ParseStartIdNumber(string? startId)
        {
            if (string.IsNullOrEmpty(startId))
                return (null, null);

            // Match pattern: letters followed by digits (e.g., "GA0153")
            var match = Regex.Match(startId, @"^([A-Za-z]+)(\d+)$");
            if (!match.Success)
                return (null, null);

            var prefix = match.Groups[1].Value;
            if (int.TryParse(match.Groups[2].Value, out int number))
            {
                return (prefix, number);
            }

            return (null, null);
        }

        public async Task<List<ProductionOrderMasterDto>> GetAllPONumbersAsync(string? search = null)
        {
            _logger.LogInformation("Fetching all PO Numbers with search: {Search}", search);
            return await _productionOrderRepository.GetAllPONumbersAsync(search);
        }

        public async Task<ProductionOrderCountsDto> GetProductionOrderCountsAsync(ProductionOrderCountFilterDto filter)
        {
            _logger.LogInformation("Service: Fetching Production Order Counts with filters");

            try
            {
                var orders = await GetAllProductionOrdersAsync(
                        filter.DateFilterType, filter.FilterDate, filter.FromDate, filter.ToDate, filter.PrecheckStatus, filter.PoNumber, filter.LnItemCode, filter.RoleId,filter.DrawingNumber);

                var counts = new ProductionOrderCountsDto
                {
                    TotalCount = orders.Count,
                    CompletedCount = orders.Count(o => (o.PrecheckStatus ?? 1) == 3),
                    PartialCount = orders.Count(o => (o.PrecheckStatus ?? 1) == 2),
                    PendingCount = orders.Count(o => (o.PrecheckStatus ?? 1) == 1),
                    UploadedCount=orders.Count(o => (o.PrecheckStatus ?? 1) == 4 )
                };

                return counts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Service while fetching Production Order Counts");
                throw;
            }
        }

        public async Task<byte[]> ExportProductionOrdersAsync(
                string? dateFilterType,
                DateTime? filterDate,
                DateTime? fromDate,
                DateTime? toDate,
                int? precheckStatus,
                string? poNumber,
                string? lnItemCode,
                int roleId = 0)
        {
            _logger.LogInformation("Service: Exporting Production Orders");

            try
            {

                var orders = await GetAllProductionOrdersAsync(
                        dateFilterType, filterDate, fromDate, toDate, precheckStatus, poNumber, lnItemCode, roleId);

                var counts = new ProductionOrderCountsDto
                {
                    TotalCount = orders.Count,
                    CompletedCount = orders.Count(o => (o.PrecheckStatus ?? 1) == 3),
                    PartialCount = orders.Count(o => (o.PrecheckStatus ?? 1) == 2),
                    PendingCount = orders.Count(o => (o.PrecheckStatus ?? 1) == 1)
                };

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Production Orders");


                int headerRow = 1;

                var headers = new[]
                {
        "PO Number", "Project Code", "Description", "Item Code", "Item Desc",
        "Series", "Start ID", "Quantity", "Status", "Rack Loc",
        "Created Date", "Last Modified", "MRIR Number"
    };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(headerRow, i + 1).Value = headers[i];
                }

                worksheet.Range(headerRow, 1, headerRow, headers.Length);
                // Freeze header
                worksheet.SheetView.FreezeRows(1);


                int row = headerRow + 1;

                foreach (var order in orders)
                {
                    worksheet.Cell(row, 1).Value = order.ProductionOrderNumber;
                    worksheet.Cell(row, 2).Value = order.ProjectNumber;
                    worksheet.Cell(row, 3).Value = order.ProjectDescription;
                    worksheet.Cell(row, 4).Value = order.LnItemCode;
                    worksheet.Cell(row, 5).Value = order.ItemDescription;
                    worksheet.Cell(row, 6).Value = order.ProductionSeries;
                    worksheet.Cell(row, 7).Value = order.StartIdNumber;
                    worksheet.Cell(row, 8).Value = order.Quantity;
                    worksheet.Cell(row, 9).Value = order.PrecheckStatusName;
                    worksheet.Cell(row, 10).Value = order.RackLocation;

                    worksheet.Cell(row, 11).Value = order.CreatedDate;
                    worksheet.Cell(row, 11).Style.DateFormat.Format = "dd-MM-yyyy";

                    worksheet.Cell(row, 12).Value = order.ModifiedDate;
                    worksheet.Cell(row, 12).Style.DateFormat.Format = "dd-MM-yyyy";

                    worksheet.Cell(row, 13).Value = order.MRIRNumber;

                    row++;
                }


                worksheet.Range(headerRow + 1, 1, row - 1, headers.Length);

                int summaryStartRow = row + 2;

                worksheet.Cell(summaryStartRow, 1).Value = "Production Order Summary";
                worksheet.Range(summaryStartRow, 1, summaryStartRow, 2)
                         .Merge()
                         .Style.Font.Bold = true;

                worksheet.Cell(summaryStartRow + 1, 1).Value = "Total Orders";
                worksheet.Cell(summaryStartRow + 1, 2).Value = counts.TotalCount;

                worksheet.Cell(summaryStartRow + 2, 1).Value = "Completed";
                worksheet.Cell(summaryStartRow + 2, 2).Value = counts.CompletedCount;

                worksheet.Cell(summaryStartRow + 3, 1).Value = "Partial";
                worksheet.Cell(summaryStartRow + 3, 2).Value = counts.PartialCount;

                worksheet.Cell(summaryStartRow + 4, 1).Value = "Pending";
                worksheet.Cell(summaryStartRow + 4, 2).Value = counts.PendingCount;

                worksheet.Range(summaryStartRow + 1, 1, summaryStartRow + 5, 2);


                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting Production Orders");
                throw;
            }
        }

        public async Task<MinStatusUploadResultDto> UploadMinStatusExcelAsync(Stream fileStream, int updatedBy)
        {
            var result = new MinStatusUploadResultDto();

            try
            {
                using var workbook = new XLWorkbook(fileStream);
                var worksheet = workbook.Worksheets.First();
                var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;

                var poList = new List<MinStatusUploadRowDto>();

                for (int i = 2; i <= rowCount; i++)
                {
                    var poNumber = worksheet.Cell(i, 1).GetString()?.Trim();
                    var minVal = worksheet.Cell(i, 9).GetString()?.Trim();
                    var statusVal = worksheet.Cell(i, 10).GetString()?.Trim();

                    if (string.IsNullOrEmpty(poNumber))
                        continue;

                    result.TotalRows++;

                    poList.Add(new MinStatusUploadRowDto
                    {
                        ProductionOrderNumber = poNumber,
                        Min = minVal,
                        Status = statusVal
                    });
                }

                var dbResult = await _productionOrderRepository.UpdateMinStatusAsync(poList);
                InvalidateCache();
                result.UpdatedRows = dbResult.UpdatedRows;
                result.NotFoundProductionOrderNumbers = dbResult.NotFoundProductionOrderNumbers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Excel file for Min/Status update");
                result.Errors.Add($"Excel parsing error: {ex.Message}");
            }

            return result;
        }


        public async Task<bool> DeleteProductionOrderAsync(DeleteProductionOrderRequestDto request)
        {
            _logger.LogInformation("Service: DeleteProductionOrderAsync called for PO: {PO}, IDNumber: {IDNumber}",
                request.ProductionOrderNumber, request.IdNumber);
            try
            {
                var result = await _productionOrderRepository.DeleteProductionOrderAsync(request);

                if (!result)
                {
                    _logger.LogWarning("Service: Production Order not found or already inactive — PO: {PO}, IDNumber: {IDNumber}",
                        request.ProductionOrderNumber, request.IdNumber);
                }
                else
                {
                    _logger.LogInformation("Service: Successfully deleted Production Order — PO: {PO}, IDNumber: {IDNumber}",
                        request.ProductionOrderNumber, request.IdNumber);
                    InvalidateCache();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error occurred while deleting Production Order — PO: {PO}, IDNumber: {IDNumber}",
                    request.ProductionOrderNumber, request.IdNumber);
                throw;
            }
        }


    }
}
