using Godrej.Precheck.Models.DataModel.ProductionOrder;
using Godrej.Precheck.Models.DTOs.ProductionOrder;
using Godrej.Precheck.Models.DTOs.Precheck;
using Godrej.Precheck.Repository.Database;
using Godrej.Precheck.Repository.Queries;
using Microsoft.Extensions.Logging;
using System.Data;
using Dapper;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Godrej.Precheck.Repository.Repository.ProductionOrderRepository
{
    public class ProductionOrderRepository : IProductionOrderRepository
    {
        private readonly ILogger<ProductionOrderRepository> _logger;
        private readonly IApplicationDbContext _db;

        public ProductionOrderRepository(ILogger<ProductionOrderRepository> logger, IApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<List<PendingPrecheckResponseDto>> GetProductionOrdersForPendingPrecheckAsync(int? assemblyDrawingNumberId, int? prodSeriesId, string? productionOrderNumber, string? lnItemCode)
        {
            _logger.LogInformation("Request for ProductionOrderRepository:GetProductionOrdersForPendingPrecheckAsync, AssemblyDrawingNumberId: {AssemblyDrawingNumberId}, ProdSeriesId: {ProdSeriesId}, ProductionOrderNumber: {ProductionOrderNumber}, LnItemCode: {LnItemCode}",
                assemblyDrawingNumberId, prodSeriesId, productionOrderNumber, lnItemCode);
            try
            {
                var results = await _db.GetAll<PendingPrecheckResponseDto>(
                    ProductionOrderQueries.GET_PRODUCTION_ORDERS_FOR_PENDING_PRECHECK,
                    new
                    {
                        AssemblyDrawingNumberId = assemblyDrawingNumberId,
                        ProdSeriesId = prodSeriesId,
                        ProductionOrderNumber = productionOrderNumber,
                        LnItemCode = lnItemCode
                    });

                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching production orders for PendingPrecheck");
                throw;
            }
        }

        public async Task<int> InsertProductionOrderMasterAsync(ProductionOrderMaster master)
        {
            _logger.LogInformation("Inserting ProductionOrderMaster: {PO}", master.ProductionOrderNumber);
            try
            {
                var insertedId = await _db.ExecuteScalar<int>(
                    ProductionOrderQueries.INSERT_PRODUCTION_ORDER_MASTER,
                    new
                    {
                        ProductionOrderNumber = master.ProductionOrderNumber,
                        ProjectNumber = master.ProjectNumber,
                        ProjectDescription = master.ProjectDescription,
                        LnItemCode = master.LnItemCode,
                        ItemDescription = master.ItemDescription,
                        ProdSeriesId = master.ProdSeriesId,
                        StartIdNumber = master.StartIdNumber,
                        Quantity = master.Quantity,
                        DrawingNumberId = master.DrawingNumberId,
                        LnItemCodeId = master.LnItemCodeId,
                        CreatedBy = master.CreatedBy,
                        MRIRNumber = master.MRIRNumber,
                        MIN=master.MIN,
                        Status=master.Status,
                        BuildNumber = master.BuildNumber,
                        SnagSheetNo = master.SnagSheetNo
                    });

                _logger.LogInformation("Inserted ProductionOrderMaster with ID: {Id}", insertedId);
                return insertedId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting ProductionOrderMaster");
                throw;
            }
        }

        public async Task<int> UpdateProductionOrderMasterAsync(ProductionOrderMaster master, int updatedBy)
        {
            _logger.LogInformation("Updating ProductionOrderMaster: {PO}", master.ProductionOrderNumber);
            try
            {
                var result = await _db.ExecuteScalar<int>(
                    ProductionOrderQueries.UPDATE_PRODUCTION_ORDER_MASTER,
                    new
                    {
                        ProductionOrderNumber = master.ProductionOrderNumber,
                        ProjectNumber = master.ProjectNumber,
                        ProjectDescription = master.ProjectDescription,
                        LnItemCode = master.LnItemCode,
                        ItemDescription = master.ItemDescription,
                        ProdSeriesId = master.ProdSeriesId,
                        StartIdNumber = master.StartIdNumber,
                        Quantity = master.Quantity,
                        DrawingNumberId = master.DrawingNumberId,
                        LnItemCodeId = master.LnItemCodeId,
                        UpdatedBy = updatedBy,
                        MRIRNumber = master.MRIRNumber,
                        Id = master.Id,
                        MIN = master.MIN,
                        BuildNumber = master.BuildNumber,
                        SnagSheetNo = master.SnagSheetNo
                    });

                _logger.LogInformation("Updated ProductionOrderMaster: {PO}", master.ProductionOrderNumber);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ProductionOrderMaster");
                throw;
            }
        }

        public async Task<ProductionOrderMasterDto?> GetByProductionOrderNumberAsync(string productionOrderNumber)
        {
            _logger.LogInformation("Fetching ProductionOrderMaster by PO: {PO}", productionOrderNumber);
            try
            {
                var result = await _db.GetSingle<ProductionOrderMasterDto>(
                    ProductionOrderQueries.GET_BY_PO_NUMBER,
                    new { ProductionOrderNumber = productionOrderNumber });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ProductionOrderMaster by PO");
                throw;
            }
        }

        public async Task<ProductionOrderMasterDto?> GetByProductionOrderNumberUpdatePOAsync(string productionOrderNumber, int? Id)
        {
            _logger.LogInformation("Fetching ProductionOrderMaster by PO: {PO}", productionOrderNumber);
            try
            {
                var result = await _db.GetSingle<ProductionOrderMasterDto>(
                    ProductionOrderQueries.GET_BY_PO_NUMBER_UPDATE_PO,
                    new { ProductionOrderNumber = productionOrderNumber, Id = Id });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ProductionOrderMaster by PO");
                throw;
            }
        }

        public async Task<(int? DrawingNumberId, int? LnItemCodeId, string? DrawingNumber, string? Nomenclature)> LookupDrawingByLnItemCodeAsync(string lnItemCode)
        {
            _logger.LogInformation("Looking up Drawing by LnItemCode: {LnItemCode}", lnItemCode);
            try
            {
                var result = await _db.GetSingle<dynamic>(
                    ProductionOrderQueries.LOOKUP_DRAWING_BY_LNITEMCODE,
                    new { LnItemCode = lnItemCode });

                if (result == null)
                    return (null, null, null, null);

                return (
                    (int?)result.DrawingNumberId,
                    (int?)result.LnItemCodeId,
                    (string?)result.DrawingNumber,
                    (string?)result.Nomenclature
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up Drawing by LnItemCode");
                throw;
            }
        }

        public async Task<(int? ProdSeriesId, string? ProductionSeries)> LookupProdSeriesByPrefixAsync(string prefix)
        {
            _logger.LogInformation("Looking up ProdSeries by prefix: {Prefix}", prefix);
            try
            {
                var result = await _db.GetSingle<dynamic>(
                    ProductionOrderQueries.LOOKUP_PRODSERIES_BY_PREFIX,
                    new { Prefix = prefix });

                if (result == null)
                    return (null, null);

                return ((int?)result.id, (string?)result.productionseries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up ProdSeries by prefix");
                throw;
            }
        }

        public async Task<int> InsertProjectDetailsWithPOIdAsync(int idNumbers, int prodSeriesId, string projectNumber, string productionOrderNumber, int drawingNumberId, int productionOrderNumberId, int createdBy, IDbConnection connection = null)
        {
            _logger.LogInformation("Inserting ProjectDetails for ID: {Id}", idNumbers);
            try
            {
                var parameters = new
                {
                    IdNumbers = idNumbers,
                    ProdSeriesId = prodSeriesId,
                    ProjectNumber = projectNumber,
                    ProductionOrderNumber = productionOrderNumber,
                    DrawingNumberId = drawingNumberId,
                    ProductionOrderNumberId = productionOrderNumberId,
                    CreatedBy = createdBy
                };

                var insertedId = connection != null
                    ? await _db.ExecuteScalarOnConnection<int>(connection, ProductionOrderQueries.INSERT_PROJECT_DETAILS_WITH_POID, parameters)
                    : await _db.ExecuteScalar<int>(ProductionOrderQueries.INSERT_PROJECT_DETAILS_WITH_POID, parameters);

                return insertedId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting ProjectDetails");
                throw;
            }
        }

        public async Task<int> InsertProjectPrecheckDetailsWithPOIdAsync(int drawingNumberId, int prodSeriesId, int projectDetailsId, decimal quantity, string componentType, int productionOrderNumberId, int createdBy, IDbConnection connection = null)
        {
            _logger.LogInformation("Inserting ProjectPrecheckDetails for ProjectDetailsId: {Id}", projectDetailsId);
            try
            {
                var parameters = new
                {
                    DrawingNumberId = drawingNumberId,
                    ProdSeriesId = prodSeriesId,
                    ProjectDetailsId = projectDetailsId,
                    Quantity = quantity,
                    ComponentType = componentType,
                    ProductionOrderNumberId = productionOrderNumberId,
                    CreatedBy = createdBy
                };

                var insertedId = connection != null
                    ? await _db.ExecuteScalarOnConnection<int>(connection, ProductionOrderQueries.INSERT_PROJECT_PRECHECK_DETAILS_WITH_POID, parameters)
                    : await _db.ExecuteScalar<int>(ProductionOrderQueries.INSERT_PROJECT_PRECHECK_DETAILS_WITH_POID, parameters);

                return insertedId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting ProjectPrecheckDetails");
                throw;
            }
        }

        public async Task<int> DeleteProjectDetailsWithPOIdAsync(int productionOrderNumberId)
        {
            _logger.LogInformation("Deleting ProjectDetails and PrecheckDetails for PO ID: {Id}", productionOrderNumberId);
            try
            {
                var result = await _db.ExecuteScalar<int>(
                    ProductionOrderQueries.DELETE_PROJECT_DETAILS_WITH_POID,
                    new { ProductionOrderNumberId = productionOrderNumberId });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ProjectDetails for PO");
                throw;
            }
        }

        public Task<IDbConnection> CreateOpenConnectionAsync()
        {
            return _db.CreateOpenConnectionAsync();
        }

        public async Task<bool> CheckPOExistsAsync(string productionOrderNumber, int prodSeriesId, int startIdNumber)
        {
            _logger.LogInformation("Checking if PO exists: {PO}", productionOrderNumber);
            try
            {
                var count = await _db.ExecuteScalar<int>(
                    ProductionOrderQueries.CHECK_PO_EXISTS,
                    new
                    {
                        ProductionOrderNumber = productionOrderNumber,
                        ProdSeriesId = prodSeriesId,
                        StartIdNumber = startIdNumber
                    });

                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if PO exists");
                throw;
            }
        }

        public async Task<(bool HasOverlap, int? MaxEndIdNumber)> CheckProdSeriesStartIdOverlapAsync(int prodSeriesId, int lnItemCodeId, int startIdNumber, int quantity)
        {
            _logger.LogInformation(
                "Checking ID range overlap for ProdSeriesId {ProdSeriesId}, LnItemCodeId {LnItemCodeId}, StartIdNumber {StartIdNumber}, Quantity {Quantity}",
                prodSeriesId, lnItemCodeId, startIdNumber, quantity);
            try
            {
                var result = await _db.GetSingle<dynamic>(
                    ProductionOrderQueries.CHECK_PRODSERIES_STARTID_OVERLAP,
                    new
                    {
                        ProdSeriesId = prodSeriesId,
                        LnItemCodeId = lnItemCodeId,
                        StartIdNumber = startIdNumber,
                        Quantity = quantity
                    });

                bool hasOverlap = result != null && (int)result.HasOverlap == 1;
                int? maxEndIdNumber = result?.MaxEndIdNumber;

                return (hasOverlap, maxEndIdNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking ProdSeries + StartIdNumber range overlap");
                throw;
            }
        }

        public async Task<List<ProductionOrderMasterDto>> GetAllProductionOrdersAsync()
        {
            _logger.LogInformation("Fetching all ProductionOrders");
            try
            {
                _logger.LogInformation("Connecting to database: {Database} on server: {DataSource} for PO Search", _db.Database, _db.DataSource);



                var results = await _db.GetAll<ProductionOrderMasterDto>(
                    ProductionOrderQueries.GET_ALL_PRODUCTION_ORDERS,
                    new { });

                // ✅ Log database name AFTER the call
                _logger.LogInformation("Query executed on Database: {DatabaseName}, Server: {DataSource}",
                    _db.Database,           // Database name
                    _db.DataSource);        // Server/instance name (if available)
                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Query executed on Database: {DatabaseName}, Server: {DataSource}",
                    _db.Database,           // Database name
                    _db.DataSource);        // Server/instance name (if available)
                _logger.LogError(ex, "Error fetching all ProductionOrders");
                throw;
            }
        }



        public async Task<List<ProductionOrderMasterDto>> GetAllProductionOrdersAsync(
            string? dateFilterType,
            DateTime? filterDate,
            DateTime? fromDate,
            DateTime? toDate,
            int? precheckStatus,
            string? poNumber,
            string? lnItemCode, string? drawingnumber)
        {
            _logger.LogInformation("Fetching filtered ProductionOrders with filters - DateType: {DateType}, Status: {Status}",
                dateFilterType, precheckStatus);
            try
            {
                // Build dynamic WHERE clause
                var dateFilter = " AND 1=1";
                var statusFilter = " AND 1=1";
                var poFilter = " AND 1=1";
                var lnItemFilter = " AND 1=1";
                var drawingFilter = " AND 1=1";


                // Date filtering logic
                if (!string.IsNullOrEmpty(dateFilterType) && dateFilterType.Equals("single", StringComparison.OrdinalIgnoreCase) && filterDate.HasValue)
                {
                    // Single date filter - match PO creation date only
                    dateFilter = @" AND CAST(pom.createddate AS DATE) = CAST(@FilterDate AS DATE)";
                }
                else if (!string.IsNullOrEmpty(dateFilterType) && dateFilterType.Equals("range", StringComparison.OrdinalIgnoreCase)
                    && fromDate.HasValue && toDate.HasValue)
                {
                    // Date range filter - match PO creation date only (whole-day inclusive, ignores time-of-day)
                    dateFilter = @" AND CAST(pom.createddate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)";
                }

                //Drawing Number Filter
                if (!string.IsNullOrWhiteSpace(drawingnumber))
                {
                    drawingFilter = " AND dn.drawingnumber LIKE '%' + @DrawingNumber + '%'";
                }

                // Status filtering
                if (precheckStatus.HasValue)
                {
                    statusFilter = " AND COALESCE(psc.CalculatedStatus, 1) = @PrecheckStatus";
                }

                // PO Number filter (string, partial match)
                if (!string.IsNullOrWhiteSpace(poNumber))
                {
                    poFilter = " AND pom.productionordernumber LIKE '%' + @PoNumber + '%'";
                }

                // LN Item Code filter (string, partial match)
                if (!string.IsNullOrWhiteSpace(lnItemCode))
                {
                    lnItemFilter = " AND pom.lnitemcode LIKE '%' + @LnItemCode + '%'";
                }
                // Replace placeholders in query
                var query = ProductionOrderQueries.GET_FILTERED_PRODUCTION_ORDERS
                    .Replace("{DATE_FILTER}", dateFilter)
                    .Replace("{STATUS_FILTER}", statusFilter)
                    .Replace("{PO_FILTER}", poFilter)
                    .Replace("{LNITEM_FILTER}", lnItemFilter)
                    .Replace("{DRAWING_FILTER}", drawingFilter);  // ADD


                var results = await _db.GetAll<ProductionOrderMasterDto>(
                    query,
                    new
                    {
                        FilterDate = filterDate,
                        FromDate = fromDate,
                        ToDate = toDate,
                        PrecheckStatus = precheckStatus,
                        PoNumber = poNumber,
                        LnItemCode = lnItemCode,
                        DrawingNumber = drawingnumber
                    });

                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching filtered ProductionOrders");
                throw;
            }
        }

        public async Task<(List<ProductionOrderMasterDto> Items, int TotalCount)> GetAllProductionOrdersPagedAsync(int pageNumber, int pageSize)
        {
            _logger.LogInformation("Fetching paged ProductionOrders: page {PageNumber}, size {PageSize}", pageNumber, pageSize);
            try
            {
                var offset = (pageNumber - 1) * pageSize;

                var totalCount = await _db.ExecuteScalar<int>(ProductionOrderQueries.GET_ALL_PRODUCTION_ORDERS_COUNT, new { });

                var results = await _db.GetAll<ProductionOrderMasterDto>(
                    ProductionOrderQueries.GET_ALL_PRODUCTION_ORDERS_PAGED,
                    new { Offset = offset, PageSize = pageSize });

                return (results.ToList(), totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching paged ProductionOrders");
                throw;
            }
        }

        public async Task<(List<ProductionOrderMasterDto> Items, int TotalCount)> GetAllProductionOrdersPagedAsync(
            string? dateFilterType,
            DateTime? filterDate,
            DateTime? fromDate,
            DateTime? toDate,
            List<int>? precheckStatus,
            string? poNumber,
            string? lnItemCode,
            string? drawingnumber,
            string? searchQuery,
            List<string>? productionSeries,
            int pageNumber,
            int pageSize)
        {
            _logger.LogInformation("Fetching paged filtered ProductionOrders: page {PageNumber}, size {PageSize}, DateType: {DateType}, Status: {Status}",
                pageNumber, pageSize, dateFilterType, precheckStatus != null ? string.Join(",", precheckStatus) : null);
            try
            {
                var dateFilter = " AND 1=1";
                var statusFilter = " AND 1=1";
                var poFilter = " AND 1=1";
                var lnItemFilter = " AND 1=1";
                var drawingFilter = " AND 1=1";
                var searchFilter = " AND 1=1";
                var seriesFilter = " AND 1=1";

                if (!string.IsNullOrEmpty(dateFilterType) && dateFilterType.Equals("single", StringComparison.OrdinalIgnoreCase) && filterDate.HasValue)
                {
                    dateFilter = @" AND CAST(pom.createddate AS DATE) = CAST(@FilterDate AS DATE)";
                }
                else if (!string.IsNullOrEmpty(dateFilterType) && dateFilterType.Equals("range", StringComparison.OrdinalIgnoreCase)
                    && fromDate.HasValue && toDate.HasValue)
                {
                    dateFilter = @" AND CAST(pom.createddate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)";
                }

                if (!string.IsNullOrWhiteSpace(drawingnumber))
                {
                    drawingFilter = " AND dn.drawingnumber LIKE '%' + @DrawingNumber + '%'";
                }

                if (precheckStatus != null && precheckStatus.Count > 0)
                {
                    statusFilter = " AND COALESCE(psc.CalculatedStatus, 1) IN @PrecheckStatus";
                }

                if (productionSeries != null && productionSeries.Count > 0)
                {
                    seriesFilter = " AND ps.productionseries IN @ProductionSeries";
                }

                if (!string.IsNullOrWhiteSpace(poNumber))
                {
                    poFilter = " AND pom.productionordernumber LIKE '%' + @PoNumber + '%'";
                }

                if (!string.IsNullOrWhiteSpace(lnItemCode))
                {
                    lnItemFilter = " AND pom.lnitemcode LIKE '%' + @LnItemCode + '%'";
                }

                // Generic search box: unlike the field-specific filters above (which are ANDed
                // together), this one checks the search text against lnitemcode and
                // productionordernumber (both live directly on tbl_productionordermaster) OR the
                // drawing number -- dn is already joined via pom.drawingnumberid = dn.id, so matching
                // dn.drawingnumber here is exactly "look up tbl_drawingnumber by name, then match its
                // id against pom.drawingnumberid" without a separate subquery.
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    searchFilter = @" AND (
                        pom.lnitemcode LIKE '%' + @SearchQuery + '%'
                        OR pom.productionordernumber LIKE '%' + @SearchQuery + '%'
                        OR dn.drawingnumber LIKE '%' + @SearchQuery + '%'
                    )";
                }

                var queryParams = new
                {
                    FilterDate = filterDate,
                    FromDate = fromDate,
                    ToDate = toDate,
                    PrecheckStatus = precheckStatus,
                    PoNumber = poNumber,
                    LnItemCode = lnItemCode,
                    DrawingNumber = drawingnumber,
                    SearchQuery = searchQuery,
                    ProductionSeries = productionSeries
                };

                var countQuery = ProductionOrderQueries.GET_FILTERED_PRODUCTION_ORDERS_COUNT
                    .Replace("{DATE_FILTER}", dateFilter)
                    .Replace("{STATUS_FILTER}", statusFilter)
                    .Replace("{PO_FILTER}", poFilter)
                    .Replace("{LNITEM_FILTER}", lnItemFilter)
                    .Replace("{DRAWING_FILTER}", drawingFilter)
                    .Replace("{SEARCH_FILTER}", searchFilter)
                    .Replace("{SERIES_FILTER}", seriesFilter);

                var totalCount = await _db.ExecuteScalar<int>(countQuery, queryParams);

                var offset = (pageNumber - 1) * pageSize;
                var dataQuery = ProductionOrderQueries.GET_FILTERED_PRODUCTION_ORDERS_PAGED
                    .Replace("{DATE_FILTER}", dateFilter)
                    .Replace("{STATUS_FILTER}", statusFilter)
                    .Replace("{PO_FILTER}", poFilter)
                    .Replace("{LNITEM_FILTER}", lnItemFilter)
                    .Replace("{DRAWING_FILTER}", drawingFilter)
                    .Replace("{SEARCH_FILTER}", searchFilter)
                    .Replace("{SERIES_FILTER}", seriesFilter);

                var results = await _db.GetAll<ProductionOrderMasterDto>(
                    dataQuery,
                    new
                    {
                        FilterDate = filterDate,
                        FromDate = fromDate,
                        ToDate = toDate,
                        PrecheckStatus = precheckStatus,
                        PoNumber = poNumber,
                        LnItemCode = lnItemCode,
                        DrawingNumber = drawingnumber,
                        SearchQuery = searchQuery,
                        ProductionSeries = productionSeries,
                        Offset = offset,
                        PageSize = pageSize
                    });

                return (results.ToList(), totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching paged filtered ProductionOrders");
                throw;
            }
        }



        public async Task<List<ProductionOrderMasterDto>> GetAllPONumbersAsync(string? search = null)
        {
            _logger.LogInformation("Fetching all PO Numbers with search: {Search}", search);
            try
            {
                var query = ProductionOrderQueries.GET_ALL_PRODUCTION_ORDERS;

                if (!string.IsNullOrWhiteSpace(search))
                {
                    bool hasWhere = query.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase) >= 0;
                    string filterClause = (hasWhere ? " AND " : " WHERE ")
                                        + "productionordernumber LIKE '%' + @Search + '%' ";

                    int orderByIndex = query.LastIndexOf("ORDER BY", StringComparison.OrdinalIgnoreCase);
                    if (orderByIndex >= 0)
                        query = query.Insert(orderByIndex, filterClause);
                    else
                        query += filterClause;
                }

                _logger.LogInformation("Connecting to database: {Database} on server: {DataSource} for PO Search",
                    _db.Database, _db.DataSource);

                var results = await _db.GetAll<ProductionOrderMasterDto>(query, new { Search = search });

                _logger.LogInformation("Query executed on Database: {DatabaseName}, Server: {DataSource}",
                    _db.Database, _db.DataSource);

                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all PO Numbers from database: {Database}", _db.Database);
                throw;
            }
        }

        public async Task<ProductionOrderCountsDto> GetProductionOrderCountsAsync(
    ProductionOrderCountFilterDto filter)
        {
            _logger.LogInformation("Repository: Fetching Production Order Counts with filters");

            try
            {

                // Normalize date filters
                DateTime? fromDate = filter.FromDate;
                DateTime? toDate = filter.ToDate;

                string dateFilter = "";
                string statusFilter = "";
                string otherFilters = "";

                if (!string.IsNullOrWhiteSpace(filter.DateFilterType))
                {
                    var today = DateTime.Today;

                    switch (filter.DateFilterType.ToLower())
                    {
                        case "today":
                            fromDate = today;
                            toDate = today.AddDays(1);
                            dateFilter = @" AND ((pom.createddate >= @FromDate AND pom.createddate <= @ToDate)
                                            OR (psc.LastModifiedDate >= @FromDate AND psc.LastModifiedDate <= @ToDate))";
                            break;

                        case "yesterday":
                            fromDate = today.AddDays(-1);
                            toDate = today;
                            dateFilter = @" AND ((pom.createddate >= @FromDate AND pom.createddate <= @ToDate)
                                            OR (psc.LastModifiedDate >= @FromDate AND psc.LastModifiedDate <= @ToDate))";
                            break;

                        case "thismonth":
                            fromDate = new DateTime(today.Year, today.Month, 1);
                            toDate = fromDate.Value.AddMonths(1);
                            dateFilter = @" AND ((pom.createddate >= @FromDate AND pom.createddate <= @ToDate)
                                            OR (psc.LastModifiedDate >= @FromDate AND psc.LastModifiedDate <= @ToDate))";
                            break;

                        case "custom":
                            // use FromDate / ToDate as passed
                            dateFilter = @" AND ((pom.createddate >= @FromDate AND pom.createddate <= @ToDate)
                                            OR (psc.LastModifiedDate >= @FromDate AND psc.LastModifiedDate <= @ToDate))";
                            break;

                        case "single":
                            if (filter.FilterDate.HasValue)
                            {
                                dateFilter = @" AND (CAST(pom.createddate AS DATE) = CAST(@FilterDate AS DATE) 
                                                OR CAST(psc.LastModifiedDate AS DATE) = CAST(@FilterDate AS DATE))";
                            }
                            break;
                    }
                }

                if (filter.PrecheckStatus.HasValue)
                {
                    statusFilter = " AND COALESCE(psc.CalculatedStatus, 1) = @PrecheckStatus";
                }

                if (!string.IsNullOrWhiteSpace(filter.PoNumber))
                {
                    otherFilters += " AND pom.productionordernumber LIKE '%' + @ProductionOrderId + '%'";
                }

                if (!string.IsNullOrWhiteSpace(filter.LnItemCode))
                {
                    otherFilters += " AND pom.lnitemcode LIKE '%' + @LineItemCode + '%'";
                }

                var query = ProductionOrderQueries.GET_PRODUCTION_ORDER_COUNTS
                    .Replace("{DATE_FILTER}", dateFilter)
                    .Replace("{STATUS_FILTER}", statusFilter)
                    .Replace("{OTHER_FILTERS}", otherFilters);

                var result = await _db.GetSingle<ProductionOrderCountsDto>(
                    query,
                    new
                    {
                        ProductionOrderId = filter.PoNumber,
                        LineItemCode = filter.LnItemCode,
                        FilterDate = filter.FilterDate,
                        FromDate = fromDate,
                        ToDate = toDate,
                        PrecheckStatus = filter.PrecheckStatus
                    });

                return result ?? new ProductionOrderCountsDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching production order counts");
                throw;
            }
        }

        //    async Task<List<ProductionOrderMasterDto>>
        //IProductionOrderRepository.GetAllProductionOrdersAsync(
        //    string? dateFilterType,
        //    DateTime? filterDate,
        //    DateTime? fromDate,
        //    DateTime? toDate,
        //    int? precheckStatus,
        //    string? poNumber,
        //    string? lnItemCode)
        //    {
        //        _logger.LogInformation(
        //            "Repository: Fetching Production Orders with filters - DateType: {DateType}, Status: {Status}, PO: {PO}, Item: {Item}",
        //            dateFilterType, precheckStatus, poNumber, lnItemCode);

        //        try
        //        {
        //            string dateFilter = "";
        //            string statusFilter = "";
        //            string otherFilters = "";

        //            // Normalize date range
        //            DateTime? normalizedFromDate = fromDate;
        //            DateTime? normalizedToDate = toDate;

        //            if (!string.IsNullOrWhiteSpace(dateFilterType))
        //            {
        //                var today = DateTime.Today;

        //                switch (dateFilterType.ToLower())
        //                {
        //                    case "single":
        //                        if (filterDate.HasValue)
        //                        {
        //                            dateFilter = @" AND (CAST(pom.createddate AS DATE) = CAST(@FilterDate AS DATE)
        //                                    OR CAST(psc.LastModifiedDate AS DATE) = CAST(@FilterDate AS DATE))";
        //                        }
        //                        break;

        //                    case "range":
        //                    case "custom":
        //                        if (fromDate.HasValue && toDate.HasValue)
        //                        {
        //                            dateFilter = @" AND ((pom.createddate >= @FromDate AND pom.createddate < @ToDate)
        //                                    OR (psc.LastModifiedDate >= @FromDate AND psc.LastModifiedDate < @ToDate))";
        //                        }
        //                        break;

        //                    case "today":
        //                        normalizedFromDate = today;
        //                        normalizedToDate = today.AddDays(1);
        //                        dateFilter = @" AND ((pom.createddate >= @FromDate AND pom.createddate < @ToDate)
        //                                OR (psc.LastModifiedDate >= @FromDate AND psc.LastModifiedDate < @ToDate))";
        //                        break;

        //                    case "yesterday":
        //                        normalizedFromDate = today.AddDays(-1);
        //                        normalizedToDate = today;
        //                        dateFilter = @" AND ((pom.createddate >= @FromDate AND pom.createddate < @ToDate)
        //                                OR (psc.LastModifiedDate >= @FromDate AND psc.LastModifiedDate < @ToDate))";
        //                        break;

        //                    case "thismonth":
        //                        normalizedFromDate = new DateTime(today.Year, today.Month, 1);
        //                        normalizedToDate = normalizedFromDate.Value.AddMonths(1);
        //                        dateFilter = @" AND ((pom.createddate >= @FromDate AND pom.createddate < @ToDate)
        //                                OR (psc.LastModifiedDate >= @FromDate AND psc.LastModifiedDate < @ToDate))";
        //                        break;
        //                }
        //            }

        //            // Status filter
        //            if (precheckStatus.HasValue)
        //            {
        //                statusFilter = " AND COALESCE(psc.CalculatedStatus, 1) = @PrecheckStatus";
        //            }

        //            if (!string.IsNullOrWhiteSpace(poNumber))
        //            {
        //                otherFilters += " AND pom.productionordernumber LIKE '%' + @PoNumber + '%'";
        //            }


        //            if (!string.IsNullOrWhiteSpace(lnItemCode))
        //            {
        //                otherFilters += " AND pom.lnitemcode LIKE '%' + @LnItemCode + '%'";
        //            }


        //            var query = ProductionOrderQueries.GET_FILTERED_PRODUCTION_ORDERS
        //                .Replace("{DATE_FILTER}", dateFilter)
        //                .Replace("{STATUS_FILTER}", statusFilter)
        //                .Replace("{OTHER_FILTERS}", otherFilters);

        //            var results = await _db.GetAll<ProductionOrderMasterDto>(
        //                query,
        //                new
        //                {
        //                    FilterDate = filterDate,
        //                    FromDate = normalizedFromDate,
        //                    ToDate = normalizedToDate,
        //                    PrecheckStatus = precheckStatus,
        //                    PoNumber = poNumber,
        //                    LnItemCode = lnItemCode
        //                });

        //            return results.ToList();
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Error fetching Production Orders");
        //            throw;
        //        }
        //    }

        public async Task<MinStatusUploadResultDto> UpdateMinStatusAsync(List<MinStatusUploadRowDto> poList)
        {
            var result = new MinStatusUploadResultDto();

            if (poList == null || poList.Count == 0)
                return result;

            try
            {
                // 1. Get all distinct non-empty PO numbers
                var poNumbers = poList.Select(x => x.ProductionOrderNumber)
                                      .Where(x => !string.IsNullOrWhiteSpace(x))
                                      .Distinct()
                                      .ToList();

                // 2. Query DB to see which POs actually exist
                string checkQuery = "SELECT productionordernumber FROM tbl_productionordermaster WHERE productionordernumber IN @PoNumbers AND isactive = 1";
                var existingPOs = await _db.GetAll<string>(checkQuery, new { PoNumbers = poNumbers });
                var existingSet = new HashSet<string>(existingPOs ?? new List<string>());

                // 3. Separate them into Found and Not Found
                var validUpdates = new List<MinStatusUploadRowDto>();
                var notFound = new List<string>();

                foreach (var item in poList)
                {
                    if (existingSet.Contains(item.ProductionOrderNumber!))
                    {
                        validUpdates.Add(item);
                    }
                    else
                    {
                        notFound.Add(item.ProductionOrderNumber!);
                    }
                }

                // 4. Update the ones that exist using Dapper's batch update capability
                // Passing a List as the parameter object causes Dapper to automatically parameterize and execute it for every item efficiently.
                if (validUpdates.Count > 0)
                {
                    var query = @"
                        UPDATE tbl_productionordermaster 
                        SET min = @Min, status = @Status ,modifieddate=GetDate()
                        WHERE productionordernumber = @ProductionOrderNumber AND isactive = 1";

                    await _db.Execute(query, validUpdates);
                }

                result.UpdatedRows = validUpdates.Count;
                result.NotFoundProductionOrderNumbers = notFound;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk updating Min and Status");
                result.Errors.Add(ex.Message);
                return result;
            }
        }


        public async Task<bool> DeleteProductionOrderAsync(DeleteProductionOrderRequestDto request)
        {
            _logger.LogInformation("Repository: DeleteProductionOrderAsync called — PO: {PO}, IDNumber: {IDNumber}, Quantity: {Quantity}",
                request.ProductionOrderNumber, request.IdNumber, request.Quantity);
            try
            {
                // Build idnumbers list => [4, 5] from startidnumber=4, quantity=2
                var idNumbersList = Enumerable.Range(request.IdNumber, request.Quantity).ToList();

                _logger.LogInformation("Repository: Calculated IDNumbers range — {IDNumbers}",
                    string.Join(", ", idNumbersList));

                var parameters = new
                {
                    request.ProductionOrderNumber,
                    IdNumbers = idNumbersList   // Dapper handles IN clause via list
                };

                // Step 1: Soft delete tbl_projectprecheckdetails (grandchild first)
                var precheckRows = await _db.Execute(
                    ProductionOrderQueries.DELETE_PRECHECK_DETAILS,
                    parameters);
                _logger.LogInformation("Repository: Soft deleted {Count} rows from tbl_projectprecheckdetails — PO: {PO}",
                    precheckRows, request.ProductionOrderNumber);

                // Step 2: Soft delete tbl_projectdetails (child)
                var projectDetailRows = await _db.Execute(
                    ProductionOrderQueries.DELETE_PROJECT_DETAILS,
                    parameters);
                _logger.LogInformation("Repository: Soft deleted {Count} rows from tbl_projectdetails — PO: {PO}",
                    projectDetailRows, request.ProductionOrderNumber);

                // Step 3: Soft delete tbl_productionordermaster (parent)
                var masterParameters = new
                {
                    request.ProductionOrderNumber,
                    request.IdNumber     // startidnumber stays as single value for master
                };

                var masterRows = await _db.Execute(
                    ProductionOrderQueries.DELETE_PRODUCTION_ORDER_MASTER,
                    masterParameters);
                _logger.LogInformation("Repository: Soft deleted {Count} rows from tbl_productionordermaster — PO: {PO}",
                    masterRows, request.ProductionOrderNumber);

                if (masterRows == 0)
                {
                    _logger.LogWarning("Repository: No matching record found in tbl_productionordermaster — PO: {PO}, IDNumber: {IDNumber}",
                        request.ProductionOrderNumber, request.IdNumber);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: Error during soft delete — PO: {PO}, IDNumber: {IDNumber}, Quantity: {Quantity}",
                    request.ProductionOrderNumber, request.IdNumber, request.Quantity);
                throw;
            }
        }
    }
}
