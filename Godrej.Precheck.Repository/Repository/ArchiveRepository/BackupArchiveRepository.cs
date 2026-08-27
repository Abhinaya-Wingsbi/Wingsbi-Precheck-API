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
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Godrej.Precheck.Repository.Repository.ArchiveRepository
{
    public class BackupArchiveRepository : IBackupArchiveRepository
    {
        private readonly ILogger<BackupArchiveRepository> _logger;
        private readonly string _backupConnectionString;

        public BackupArchiveRepository(ILogger<BackupArchiveRepository> logger, IConfiguration configuration)
        {
            _logger = logger;
            _backupConnectionString = configuration.GetConnectionString("BackupConnection") 
                ?? throw new ArgumentNullException("BackupConnection string is not configured");
        }

        public async Task<List<BackupCompDataResponse>> GetBackupArchiveDataAsync(BackupArchiveFilterRequest request)
        {
            try
            {
                _logger.LogInformation($"Getting backup archive data with filters: ProdSeries={request.ConsumedInProdSeries}, Assembly={request.ConsumedInAssembly}, ComponentId={request.ConsumedInId}");

                var parameters = new[]
                {
                    new SqlParameter("@ConsumedInProdSeries", (object)request.ConsumedInProdSeries ?? DBNull.Value),
                    new SqlParameter("@ConsumedInAssembly", (object)request.ConsumedInAssembly ?? DBNull.Value),
                    new SqlParameter("@ConsumedInId", (object)request.ConsumedInId ?? DBNull.Value),
                    new SqlParameter("@DrawingNumber", (object)request.DrawingNumber ?? DBNull.Value),
                    new SqlParameter("@ComponentId", (object)request.ComponentId ?? DBNull.Value),
                    new SqlParameter("@Nomenclature", (object)request.Nomenclature ?? DBNull.Value),
                    new SqlParameter("@IDNos", (object)request.IDNos ?? DBNull.Value),
                    new SqlParameter("@CompTableName", (object)request.CompTableName ?? DBNull.Value)
                };

                var data = await ExecuteQueryAsync<BackupCompDataResponse>(
                    BackupArchiveQueries.GET_BACKUP_ARCHIVE_DATA, 
                    parameters);

                _logger.LogInformation($"Retrieved {data.Count} records from backup database");
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting backup archive data: {ex.Message}");
                throw;
            }
        }

        public async Task<List<SimpleCompDataResponse>> GetSimpleArchiveDataAsync(SimpleArchiveFilterRequest request)
        {
            try
            {
                _logger.LogInformation($"Getting simple archive data - ProductionSeries={request.ProductionSeries}, AssemblyNumber={request.AssemblyNumber}, ComponentId={request.ComponentId}");

                // Convert simple request to backup request format
                var backupRequest = new BackupArchiveFilterRequest
                {
                    ConsumedInProdSeries = request.ProductionSeries,
                    ConsumedInAssembly = request.AssemblyNumber,
                    ConsumedInId = request.ComponentId,
                    DrawingNumber = request.DrawingNumber
                };

                var backupData = await GetBackupArchiveDataAsync(backupRequest);

                // Convert to simple response format for backward compatibility
                var simpleData = backupData.Select(bd => new SimpleCompDataResponse
                {
                    Id = bd.Id,
                    DrawingNumber = bd.DrawingNumber,
                    ChildDrawingNumberId = bd.PONumber,
                    Nomenclature = bd.Nomenclature,
                    IrNumber = bd.IRNumber,
                    MsnNumber = bd.MSNNumber,
                    Quantity = bd.Quantity,
                    ConsumedIn = bd.ConsumedIn,
                    Remarks = bd.Remarks,
                    UserName = bd.UserName,
                    CreatedDate = bd.CreatedDate,
                    AssemblyNumber = bd.AssemblyNumber,
                    ProductionSeries = bd.ProductionSeries
                }).ToList();

                _logger.LogInformation($"Converted {simpleData.Count} records to simple format");
                return simpleData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting simple archive data: {ex.Message}");
                throw;
            }
        }

        public async Task<BackupArchiveDataPagedResponse> GetPagedArchiveDataAsync(BackupArchiveFilterRequest request)
        {
            try
            {
                // Get total count first
                var countParams = new[]
                {
                    new SqlParameter("@ConsumedInProdSeries", (object)request.ConsumedInProdSeries ?? DBNull.Value),
                    new SqlParameter("@ConsumedInAssembly", (object)request.ConsumedInAssembly ?? DBNull.Value),
                    new SqlParameter("@ConsumedInId", (object)request.ConsumedInId ?? DBNull.Value),
                    new SqlParameter("@DrawingNumber", (object)request.DrawingNumber ?? DBNull.Value),
                    new SqlParameter("@ComponentId", (object)request.ComponentId ?? DBNull.Value),
                    new SqlParameter("@Nomenclature", (object)request.Nomenclature ?? DBNull.Value),
                    new SqlParameter("@IDNos", (object)request.IDNos ?? DBNull.Value),
                    new SqlParameter("@CompTableName", (object)request.CompTableName ?? DBNull.Value)
                };

                var countResult = await ExecuteScalarAsync<int>(
                    BackupArchiveQueries.GET_BACKUP_ARCHIVE_DATA_COUNT, 
                    countParams);

                // Get paged data if search term is provided
                List<BackupCompDataResponse> data;
                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    var searchParams = new[]
                    {
                        new SqlParameter("@SearchTerm", request.SearchTerm),
                        new SqlParameter("@Offset", ((request.PageNumber ?? 1) - 1) * (request.PageSize ?? 50)),
                        new SqlParameter("@PageSize", request.PageSize ?? 50)
                    };

                    data = await ExecuteQueryAsync<BackupCompDataResponse>(
                        BackupArchiveQueries.SEARCH_ARCHIVE_DATA, 
                        searchParams);
                }
                else
                {
                    // For non-search queries, get all data and apply pagination in memory
                    // This is acceptable for archive data as it's typically smaller datasets
                    data = await GetBackupArchiveDataAsync(request);
                    
                    if (request.PageNumber.HasValue && request.PageSize.HasValue)
                    {
                        var skip = (request.PageNumber.Value - 1) * request.PageSize.Value;
                        data = data.Skip(skip).Take(request.PageSize.Value).ToList();
                    }
                }

                return new BackupArchiveDataPagedResponse
                {
                    Data = data,
                    TotalRecords = countResult,
                    PageNumber = request.PageNumber ?? 1,
                    PageSize = request.PageSize ?? data.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting paged archive data: {ex.Message}");
                throw;
            }
        }

        public async Task<BackupArchiveDropdownResponse> GetDropdownOptionsAsync()
        {
            try
            {
                _logger.LogInformation("Getting dropdown options from backup database");

                var productionSeries = await ExecuteScalarListQueryAsync(
                    BackupArchiveQueries.GET_BACKUP_PRODUCTION_SERIES, 
                    new SqlParameter[0]);

                var assemblyNumbers = await ExecuteScalarListQueryAsync(
                    BackupArchiveQueries.GET_BACKUP_ASSEMBLY_NUMBERS, 
                    new SqlParameter[0]);

                var drawingNumbers = await ExecuteScalarListQueryAsync(
                    BackupArchiveQueries.GET_BACKUP_DRAWING_NUMBERS, 
                    new SqlParameter[0]);

                var nomenclatures = await ExecuteScalarListQueryAsync(
                    BackupArchiveQueries.GET_BACKUP_NOMENCLATURES, 
                    new SqlParameter[0]);

                return new BackupArchiveDropdownResponse
                {
                    ProductionSeries = productionSeries,
                    AssemblyNumbers = assemblyNumbers,
                    DrawingNumbers = drawingNumbers,
                    Nomenclatures = nomenclatures,
                    ComponentTypes = new List<string>() // Can be populated later if needed
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting dropdown options: {ex.Message}");
                throw;
            }
        }

        public async Task<BackupArchiveStatisticsResponse> GetStatisticsAsync()
        {
            try
            {
                _logger.LogInformation("Getting archive statistics from backup database");

                var stats = await ExecuteQueryAsync<BackupArchiveStatisticsResponse>(
                    BackupArchiveQueries.GET_BACKUP_ARCHIVE_STATISTICS, 
                    new SqlParameter[0]);

                return stats.FirstOrDefault() ?? new BackupArchiveStatisticsResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting archive statistics: {ex.Message}");
                throw;
            }
        }

        public async Task<List<DrawingCompMappingResponse>> GetDrawingCompMappingsAsync(string drawingNumber = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(drawingNumber))
                {
                    var parameters = new[] { new SqlParameter("@DrawingNumber", drawingNumber) };
                    return await ExecuteQueryAsync<DrawingCompMappingResponse>(
                        BackupArchiveQueries.GET_MAPPING_BY_DRAWING_NUMBER, 
                        parameters);
                }
                else
                {
                    return await ExecuteQueryAsync<DrawingCompMappingResponse>(
                        BackupArchiveQueries.GET_DRAWING_COMP_MAPPINGS, 
                        new SqlParameter[0]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting drawing comp mappings: {ex.Message}");
                throw;
            }
        }

        public async Task<List<BackupConsumptionSummaryResponse>> GetConsumptionSummaryAsync(string assemblyPattern, string productionSeries)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@AssemblyPattern", (object)assemblyPattern ?? DBNull.Value),
                    new SqlParameter("@ProductionSeries", (object)productionSeries ?? DBNull.Value)
                };

                return await ExecuteQueryAsync<BackupConsumptionSummaryResponse>(
                    BackupArchiveQueries.GET_BACKUP_CONSUMPTION_BY_ASSEMBLY, 
                    parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting consumption summary: {ex.Message}");
                throw;
            }
        }

        public async Task<List<SimpleCompDataResponse>> SearchByConsumedInAsync(string productionSeries, string assemblyNumber, string componentId)
        {
            try
            {
                _logger.LogInformation($"Searching by ConsumedIn - ProductionSeries: {productionSeries}, Assembly: {assemblyNumber}, ComponentId: {componentId}");

                var parameters = new[]
                {
                    new SqlParameter("@ProductionSeries", (object)productionSeries ?? DBNull.Value),
                    new SqlParameter("@AssemblyNumber", (object)assemblyNumber ?? DBNull.Value),
                    new SqlParameter("@ComponentId", (object)componentId ?? DBNull.Value)
                };

                return await ExecuteQueryAsync<SimpleCompDataResponse>(
                    BackupArchiveQueries.SEARCH_BY_CONSUMED_IN, 
                    parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching by ConsumedIn: {ex.Message}");
                throw;
            }
        }

        private async Task<List<T>> ExecuteQueryAsync<T>(string sql, SqlParameter[] parameters) where T : class, new()
        {
            var results = new List<T>();

            using (var connection = new SqlConnection(_backupConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddRange(parameters);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var item = MapReaderToObject<T>(reader);
                            results.Add(item);
                        }
                    }
                }
            }

            return results;
        }

        private async Task<T> ExecuteScalarAsync<T>(string sql, SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_backupConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddRange(parameters);
                    var result = await command.ExecuteScalarAsync();
                    return result == null ? default(T) : (T)Convert.ChangeType(result, typeof(T));
                }
            }
        }

        private async Task<List<string>> ExecuteScalarListQueryAsync(string sql, SqlParameter[] parameters)
        {
            var results = new List<string>();

            using (var connection = new SqlConnection(_backupConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddRange(parameters);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var value = reader.GetValue(0)?.ToString();
                            if (!string.IsNullOrEmpty(value))
                            {
                                results.Add(value);
                            }
                        }
                    }
                }
            }

            return results;
        }

        private T MapReaderToObject<T>(SqlDataReader reader) where T : class, new()
        {
            var obj = new T();
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                try
                {
                    if (HasColumn(reader, property.Name) && reader[property.Name] != DBNull.Value)
                    {
                        var value = reader[property.Name];
                        if (property.PropertyType == typeof(string))
                        {
                            property.SetValue(obj, value?.ToString());
                        }
                        else if (property.PropertyType == typeof(long) || property.PropertyType == typeof(long?))
                        {
                            property.SetValue(obj, Convert.ToInt64(value));
                        }
                        else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
                        {
                            property.SetValue(obj, Convert.ToInt32(value));
                        }
                        else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                        {
                            property.SetValue(obj, Convert.ToDateTime(value));
                        }
                        else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
                        {
                            property.SetValue(obj, Convert.ToBoolean(value));
                        }
                        else
                        {
                            property.SetValue(obj, value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to map property {property.Name}: {ex.Message}");
                }
            }

            return obj;
        }

        private bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
