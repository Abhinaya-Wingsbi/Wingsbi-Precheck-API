using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel.Archive;
using Godrej.Precheck.Models.DTOs.Archive;
using Godrej.Precheck.Repository.Database;
using Godrej.Precheck.Repository.Queries;
using Microsoft.Extensions.Logging;

namespace Godrej.Precheck.Repository.Repository.ArchiveRepository
{
    public class ArchiveRepository : IArchiveRepository
    {
        private readonly ILogger<ArchiveRepository> _logger;
        private readonly IApplicationDbContext _db;

        public ArchiveRepository(ILogger<ArchiveRepository> logger, IApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<ArchiveDataPagedResponse> GetArchiveDataAsync(ArchiveFilterRequest request)
        {
            try
            {
                _logger.LogInformation($"Getting archive data with filters: Assembly={request.AssemblyNumber}, ProductionSeries={request.ProductionSeries}, IdNumber={request.IdNumber}");

                // Note: This method is deprecated - use GetAllArchiveDataAsync() for non-paginated results
                // Removing offset since we're using the non-paginated query

                // Get total count
                var countResult = await _db.GetSingle<int>(
                    ArchiveQueries.GET_ARCHIVE_DATA_COUNT,
                    new
                    {
                        AssemblyNumber = request.AssemblyNumber,
                        ProductionSeries = request.ProductionSeries,
                        IdNumber = request.IdNumber,
                        DrawingNumberId = request.DrawingNumberId,
                        ProductionSeriesId = request.ProductionSeriesId,
                        AssemblyNumberId = request.AssemblyNumberId
                    });

                // Use the non-paginated query instead 
                var data = await _db.GetAll<ArchiveDataResponse>(
                    ArchiveQueries.GET_ALL_ARCHIVE_DATA,
                    new
                    {
                        AssemblyNumber = request.AssemblyNumber,
                        ProductionSeries = request.ProductionSeries,
                        IdNumber = request.IdNumber,
                        DrawingNumberId = request.DrawingNumberId,
                        ProductionSeriesId = request.ProductionSeriesId,
                        AssemblyNumberId = request.AssemblyNumberId
                        // Removed Offset and PageSize - no longer needed
                    });

                var response = new ArchiveDataPagedResponse
                {
                    Data = data.ToList(),
                    TotalRecords = data.Count(), // Actual count of returned data
                    PageNumber = 1, // Always 1 since we return all data
                    PageSize = data.Count() // Same as total records since no pagination
                };

                _logger.LogInformation($"Retrieved {data.Count()} records out of {countResult} total records");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting archive data: {ex.Message}");
                throw;
            }
        }

        public async Task<ArchiveDropdownResponse> GetDropdownOptionsAsync()
        {
            try
            {
                _logger.LogInformation("Getting dropdown options for archive filters");

                var assemblyNumbersTask = _db.GetAll<string>(ArchiveQueries.GET_ASSEMBLY_NUMBERS, null);
                var productionSeriesTask = _db.GetAll<string>(ArchiveQueries.GET_PRODUCTION_SERIES, null);

                await Task.WhenAll(assemblyNumbersTask, productionSeriesTask);

                var response = new ArchiveDropdownResponse
                {
                    AssemblyNumbers = assemblyNumbersTask.Result.ToList(),
                    ProductionSeries = productionSeriesTask.Result.ToList()
                };

                _logger.LogInformation($"Retrieved {response.AssemblyNumbers.Count} assembly numbers and {response.ProductionSeries.Count} production series");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting dropdown options: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CompData>> GetComponentsByDrawingNumberAsync(string drawingNumber)
        {
            try
            {
                _logger.LogInformation($"Getting components for drawing number: {drawingNumber}");

                var components = await _db.GetAll<CompData>(
                    ArchiveQueries.GET_COMPONENTS_BY_DRAWING_NUMBER,
                    new { DrawingNumber = drawingNumber });

                _logger.LogInformation($"Retrieved {components.Count()} components for drawing number {drawingNumber}");
                return components.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting components by drawing number: {ex.Message}");
                throw;
            }
        }

        public async Task<List<object>> GetConsumptionByAssemblyAsync(string assemblyPattern, string productionSeries)
        {
            try
            {
                _logger.LogInformation($"Getting consumption by assembly pattern: {assemblyPattern}, production series: {productionSeries}");

                var consumption = await _db.GetAll<object>(
                    ArchiveQueries.GET_CONSUMPTION_BY_ASSEMBLY,
                    new
                    {
                        AssemblyPattern = assemblyPattern,
                        ProductionSeries = productionSeries
                    });

                _logger.LogInformation($"Retrieved {consumption.Count()} consumption records");
                return consumption.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting consumption by assembly: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ArchiveDataResponse>> GetArchiveDataForExportAsync(ArchiveFilterRequest request)
        {
            try
            {
                _logger.LogInformation("Getting archive data for export");

                var data = await _db.GetAll<ArchiveDataResponse>(
                    ArchiveQueries.GET_ARCHIVE_DATA_FOR_EXPORT,
                    new
                    {
                        AssemblyNumber = request.AssemblyNumber,
                        ProductionSeries = request.ProductionSeries,
                        IdNumber = request.IdNumber,
                        DrawingNumberId = request.DrawingNumberId
                    });

                _logger.LogInformation($"Retrieved {data.Count()} records for export");
                return data.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting archive data for export: {ex.Message}");
                throw;
            }
        }

        public async Task<object> GetArchiveStatisticsAsync()
        {
            try
            {
                _logger.LogInformation("Getting archive statistics");

                var statistics = await _db.GetSingle<object>(ArchiveQueries.GET_ARCHIVE_STATISTICS, null);

                _logger.LogInformation("Retrieved archive statistics");
                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting archive statistics: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> HasCompDataAsync(int drawingNumberId)
        {
            try
            {
                _logger.LogInformation($"Checking if COMP data exists for drawing number ID: {drawingNumberId}");

                var query = @"
                    SELECT COUNT(cd.Id) 
                    FROM tbl_comp_data cd
                    INNER JOIN tbl_comp_data_info cdi ON cd.CompInfoId = cdi.Id
                    WHERE cdi.DrawingNumberId = @DrawingNumberId 
                    AND cd.IsActive = 1 
                    AND cdi.IsActive = 1";

                var count = await _db.GetSingle<int>(query, new { DrawingNumberId = drawingNumberId });
                
                var hasData = count > 0;
                _logger.LogInformation($"Drawing number ID {drawingNumberId} has COMP data: {hasData}");
                
                return hasData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking COMP data existence: {ex.Message}");
                throw;
            }
        }

        public async Task<List<string>> GetAssemblyNumbersByPatternAsync(string pattern)
        {
            try
            {
                _logger.LogInformation($"Getting assembly numbers by pattern: {pattern}");

                var query = @"
                    SELECT DISTINCT AssemblyNumber
                    FROM tbl_comp_data 
                    WHERE IsActive = 1 
                    AND AssemblyNumber IS NOT NULL 
                    AND AssemblyNumber != ''
                    AND AssemblyNumber LIKE '%' + @Pattern + '%'
                    ORDER BY AssemblyNumber";

                var assemblyNumbers = await _db.GetAll<string>(query, new { Pattern = pattern });

                _logger.LogInformation($"Retrieved {assemblyNumbers.Count()} assembly numbers matching pattern");
                return assemblyNumbers.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting assembly numbers by pattern: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ArchiveDataResponse>> GetComponentHistoryAsync(string componentId)
        {
            try
            {
                _logger.LogInformation($"Getting component history for: {componentId}");

                var query = @"
                    SELECT 
                        cd.Id,
                        cd.IDNos as PONumber,
                        dn.drawingnumber as DrawingNumber,
                        ISNULL(nom.nomenclature, 'N/A') as Nomenclature,
                        cd.Quantity,
                        cd.ComponentId as IDNumber,
                        cd.IRNos as IRNumber,
                        cd.MSNNos as MSNNumber,
                        'Consumed' as Status,
                        COALESCE(cd.MyDate, cd.CreatedDate) as CreatedDate,
                        cd.AssemblyNumber,
                        cd.ProductionSeries,
                        cd.ConsumedIn,
                        cd.Remarks,
                        cd.UserName
                    FROM tbl_comp_data cd
                    LEFT JOIN tbl_comp_data_info cdi ON cd.CompInfoId = cdi.Id
                    LEFT JOIN tbl_drawingnumber dn ON cdi.DrawingNumberId = dn.Id
                    LEFT JOIN tbl_drawingnomenclaturemapping nommap ON dn.Id = nommap.drawingnumberid
                    LEFT JOIN tbl_nomenclature nom ON nommap.nomenclatureid = nom.Id
                    WHERE cd.IsActive = 1 
                        AND (cdi.IsActive = 1 OR cdi.IsActive IS NULL)
                        AND (dn.isactive = 1 OR dn.isactive IS NULL)
                        AND cd.ComponentId = @ComponentId
                    ORDER BY COALESCE(cd.MyDate, cd.CreatedDate) DESC";

                var history = await _db.GetAll<ArchiveDataResponse>(query, new { ComponentId = componentId });

                _logger.LogInformation($"Retrieved {history.Count()} history records for component {componentId}");
                return history.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting component history: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ArchiveDataResponse>> GetAllArchiveDataAsync(ArchiveFilterRequest request)
        {
            try
            {
                _logger.LogInformation($"Getting ALL archive data without pagination - ProductionSeriesId={request.ProductionSeriesId}, AssemblyNumberId={request.AssemblyNumberId}, IdNumber={request.IdNumber}");

                // Get all data without pagination
                var data = await _db.GetAll<ArchiveDataResponse>(
                    ArchiveQueries.GET_ALL_ARCHIVE_DATA,
                    new
                    {
                        AssemblyNumber = request.AssemblyNumber,
                        ProductionSeries = request.ProductionSeries,
                        IdNumber = request.IdNumber,
                        DrawingNumberId = request.DrawingNumberId,
                        ProductionSeriesId = request.ProductionSeriesId,
                        AssemblyNumberId = request.AssemblyNumberId
                    });

                _logger.LogInformation($"Retrieved {data.Count()} total records without pagination");
                return data.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting all archive data: {ex.Message}");
                throw;
            }
        }
    }
}
