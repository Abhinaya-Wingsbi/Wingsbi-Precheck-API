using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DTOs.Archive;

namespace Godrej.Precheck.Service.Service.ArchiveService
{
    /// <summary>
    /// Interface for backup database archive service
    /// Provides archive functionality using PrecheckDB_QA database
    /// </summary>
    public interface IBackupArchiveService
    {
        /// <summary>
        /// Get COMP data using IDs for backward compatibility
        /// </summary>
        /// <param name="productionSeriesId">Production series ID</param>
        /// <param name="assemblyNumberId">Assembly number ID</param>
        /// <param name="componentId">Component ID</param>
        /// <returns>List of simple COMP data</returns>
        Task<List<SimpleCompDataResponse>> GetCompDataAsync(int? productionSeriesId, int? assemblyNumberId, string componentId);

        /// <summary>
        /// Get COMP data using names (preferred method for backup database)
        /// </summary>
        /// <param name="productionSeries">Production series name</param>
        /// <param name="assemblyNumber">Assembly number</param>
        /// <param name="componentId">Component ID</param>
        /// <returns>List of simple COMP data</returns>
        Task<List<SimpleCompDataResponse>> GetCompDataByNamesAsync(string productionSeries, string assemblyNumber, string componentId);

        /// <summary>
        /// Get advanced archive data with full filtering capabilities
        /// </summary>
        /// <param name="request">Advanced filter request</param>
        /// <returns>List of detailed archive data</returns>
        Task<List<BackupCompDataResponse>> GetAdvancedArchiveDataAsync(BackupArchiveFilterRequest request);

        /// <summary>
        /// Get paginated archive data
        /// </summary>
        /// <param name="request">Filter request with pagination</param>
        /// <returns>Paginated archive data</returns>
        Task<BackupArchiveDataPagedResponse> GetPagedArchiveDataAsync(BackupArchiveFilterRequest request);

        /// <summary>
        /// Get dropdown options for filtering
        /// </summary>
        /// <returns>Dropdown options</returns>
        Task<BackupArchiveDropdownResponse> GetDropdownOptionsAsync();

        /// <summary>
        /// Get archive statistics
        /// </summary>
        /// <returns>Statistics data</returns>
        Task<BackupArchiveStatisticsResponse> GetStatisticsAsync();

        /// <summary>
        /// Get drawing number to COMP table mappings
        /// </summary>
        /// <param name="drawingNumber">Optional drawing number filter</param>
        /// <returns>List of mappings</returns>
        Task<List<DrawingCompMappingResponse>> GetDrawingCompMappingsAsync(string drawingNumber = null);

        /// <summary>
        /// Get consumption summary by assembly
        /// </summary>
        /// <param name="assemblyPattern">Assembly pattern</param>
        /// <param name="productionSeries">Production series</param>
        /// <returns>Consumption summary</returns>
        Task<List<BackupConsumptionSummaryResponse>> GetConsumptionSummaryAsync(string assemblyPattern, string productionSeries);

        /// <summary>
        /// Search archive data with free text
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Search results</returns>
        Task<List<BackupCompDataResponse>> SearchArchiveDataAsync(string searchTerm, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// Search for drawing numbers consumed in specific assembly
        /// Based on ConsumedIn pattern like "D/K324-0000-000CB/321"
        /// </summary>
        /// <param name="productionSeries">Production series (e.g., "D")</param>
        /// <param name="assemblyNumber">Assembly number (e.g., "K324-0000-000CB")</param>
        /// <param name="componentId">Component ID (e.g., "321")</param>
        /// <returns>List of drawing numbers consumed in the assembly</returns>
        Task<List<SimpleCompDataResponse>> SearchByConsumedInAsync(string productionSeries, string assemblyNumber, string componentId);
    }
}
