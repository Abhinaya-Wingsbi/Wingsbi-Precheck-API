using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DTOs.Archive;
using Godrej.Precheck.Repository.Repository.ArchiveRepository;
using Microsoft.Extensions.Logging;

namespace Godrej.Precheck.Service.Service.ArchiveService
{
    public class BackupArchiveService : IBackupArchiveService
    {
        private readonly ILogger<BackupArchiveService> _logger;
        private readonly IBackupArchiveRepository _backupArchiveRepository;

        public BackupArchiveService(ILogger<BackupArchiveService> logger, IBackupArchiveRepository backupArchiveRepository)
        {
            _logger = logger;
            _backupArchiveRepository = backupArchiveRepository;
        }

        public async Task<List<SimpleCompDataResponse>> GetCompDataAsync(int? productionSeriesId, int? assemblyNumberId, string componentId)
        {
            try
            {
                _logger.LogInformation($"BackupArchiveService: Getting COMP data - ProductionSeriesId: {productionSeriesId}, AssemblyNumberId: {assemblyNumberId}, ComponentId: {componentId}");

                // For now, we'll use the string-based filtering since we're working with backup database
                // In future, you might want to add lookup tables to convert IDs to names
                var request = new SimpleArchiveFilterRequest
                {
                    ProductionSeriesId = productionSeriesId,
                    AssemblyNumberId = assemblyNumberId,
                    ComponentId = componentId
                };

                var result = await _backupArchiveRepository.GetSimpleArchiveDataAsync(request);

                _logger.LogInformation($"BackupArchiveService: Retrieved {result.Count} records from backup database");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"BackupArchiveService: Error getting COMP data - ProductionSeriesId: {productionSeriesId}, AssemblyNumberId: {assemblyNumberId}, ComponentId: {componentId}");
                throw;
            }
        }

        public async Task<List<SimpleCompDataResponse>> GetCompDataByNamesAsync(string productionSeries, string assemblyNumber, string componentId)
        {
            try
            {
                _logger.LogInformation($"BackupArchiveService: Getting COMP data by names - ProductionSeries: {productionSeries}, AssemblyNumber: {assemblyNumber}, ComponentId: {componentId}");

                var request = new SimpleArchiveFilterRequest
                {
                    ProductionSeries = productionSeries,
                    AssemblyNumber = assemblyNumber,
                    ComponentId = componentId
                };

                var result = await _backupArchiveRepository.GetSimpleArchiveDataAsync(request);

                _logger.LogInformation($"BackupArchiveService: Retrieved {result.Count} records by names from backup database");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"BackupArchiveService: Error getting COMP data by names - ProductionSeries: {productionSeries}, AssemblyNumber: {assemblyNumber}, ComponentId: {componentId}");
                throw;
            }
        }

        public async Task<List<BackupCompDataResponse>> GetAdvancedArchiveDataAsync(BackupArchiveFilterRequest request)
        {
            try
            {
                _logger.LogInformation($"BackupArchiveService: Getting advanced archive data with filters");

                var result = await _backupArchiveRepository.GetBackupArchiveDataAsync(request);

                _logger.LogInformation($"BackupArchiveService: Retrieved {result.Count} advanced archive records");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"BackupArchiveService: Error getting advanced archive data");
                throw;
            }
        }

        public async Task<BackupArchiveDataPagedResponse> GetPagedArchiveDataAsync(BackupArchiveFilterRequest request)
        {
            try
            {
                _logger.LogInformation($"BackupArchiveService: Getting paged archive data");

                var result = await _backupArchiveRepository.GetPagedArchiveDataAsync(request);

                _logger.LogInformation($"BackupArchiveService: Retrieved {result.Data.Count} paged records out of {result.TotalRecords} total");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"BackupArchiveService: Error getting paged archive data");
                throw;
            }
        }

        public async Task<BackupArchiveDropdownResponse> GetDropdownOptionsAsync()
        {
            try
            {
                _logger.LogInformation($"BackupArchiveService: Getting dropdown options");

                var result = await _backupArchiveRepository.GetDropdownOptionsAsync();

                _logger.LogInformation($"BackupArchiveService: Retrieved dropdown options - ProductionSeries: {result.ProductionSeries?.Count}, AssemblyNumbers: {result.AssemblyNumbers?.Count}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"BackupArchiveService: Error getting dropdown options");
                throw;
            }
        }

        public async Task<BackupArchiveStatisticsResponse> GetStatisticsAsync()
        {
            try
            {
                _logger.LogInformation($"BackupArchiveService: Getting archive statistics");

                var result = await _backupArchiveRepository.GetStatisticsAsync();

                _logger.LogInformation($"BackupArchiveService: Retrieved statistics - TotalRecords: {result.TotalRecords}, TotalDrawingNumbers: {result.TotalDrawingNumbers}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"BackupArchiveService: Error getting statistics");
                throw;
            }
        }

        public async Task<List<DrawingCompMappingResponse>> GetDrawingCompMappingsAsync(string drawingNumber = null)
        {
            try
            {
                _logger.LogInformation($"BackupArchiveService: Getting drawing comp mappings for: {drawingNumber ?? "all"}");

                var result = await _backupArchiveRepository.GetDrawingCompMappingsAsync(drawingNumber);

                _logger.LogInformation($"BackupArchiveService: Retrieved {result.Count} mapping records");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"BackupArchiveService: Error getting drawing comp mappings");
                throw;
            }
        }

        public async Task<List<BackupConsumptionSummaryResponse>> GetConsumptionSummaryAsync(string assemblyPattern, string productionSeries)
        {
            try
            {
                _logger.LogInformation($"BackupArchiveService: Getting consumption summary - Assembly: {assemblyPattern}, ProductionSeries: {productionSeries}");

                var result = await _backupArchiveRepository.GetConsumptionSummaryAsync(assemblyPattern, productionSeries);

                _logger.LogInformation($"BackupArchiveService: Retrieved {result.Count} consumption summary records");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"BackupArchiveService: Error getting consumption summary");
                throw;
            }
        }

        public async Task<List<BackupCompDataResponse>> SearchArchiveDataAsync(string searchTerm, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                _logger.LogInformation($"BackupArchiveService: Searching archive data with term: {searchTerm}");

                var request = new BackupArchiveFilterRequest
                {
                    SearchTerm = searchTerm,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _backupArchiveRepository.GetBackupArchiveDataAsync(request);

                _logger.LogInformation($"BackupArchiveService: Found {result.Count} records for search term: {searchTerm}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"BackupArchiveService: Error searching archive data with term: {searchTerm}");
                throw;
            }
        }

        public async Task<List<SimpleCompDataResponse>> SearchByConsumedInAsync(string productionSeries, string assemblyNumber, string componentId)
        {
            try
            {
                _logger.LogInformation($"BackupArchiveService: Search by ConsumedIn - ProductionSeries: {productionSeries}, Assembly: {assemblyNumber}, ComponentId: {componentId}");

                var result = await _backupArchiveRepository.SearchByConsumedInAsync(productionSeries, assemblyNumber, componentId);

                _logger.LogInformation($"BackupArchiveService: Found {result.Count} drawing numbers consumed in assembly {productionSeries}/{assemblyNumber}/{componentId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"BackupArchiveService: Error searching by ConsumedIn - ProductionSeries: {productionSeries}, Assembly: {assemblyNumber}, ComponentId: {componentId}");
                throw;
            }
        }
    }
}
