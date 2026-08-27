using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DTOs.Archive;

namespace Godrej.Precheck.Repository.Repository.ArchiveRepository
{
    /// <summary>
    /// Interface for backup database archive repository
    /// Handles archive functionality using PrecheckDB_QA database
    /// </summary>
    public interface IBackupArchiveRepository
    {
        /// <summary>
        /// Get archive data from backup database with advanced filtering
        /// </summary>
        /// <param name="request">Filter request with separated consumed_in fields</param>
        /// <returns>List of backup archive data</returns>
        Task<List<BackupCompDataResponse>> GetBackupArchiveDataAsync(BackupArchiveFilterRequest request);

        /// <summary>
        /// Get archive data in simple format for backward compatibility
        /// </summary>
        /// <param name="request">Simple filter request</param>
        /// <returns>List of simple archive data</returns>
        Task<List<SimpleCompDataResponse>> GetSimpleArchiveDataAsync(SimpleArchiveFilterRequest request);

        /// <summary>
        /// Get paginated archive data from backup database
        /// </summary>
        /// <param name="request">Filter request with pagination</param>
        /// <returns>Paginated archive data response</returns>
        Task<BackupArchiveDataPagedResponse> GetPagedArchiveDataAsync(BackupArchiveFilterRequest request);

        /// <summary>
        /// Get dropdown options for filtering from backup database
        /// </summary>
        /// <returns>Dropdown options</returns>
        Task<BackupArchiveDropdownResponse> GetDropdownOptionsAsync();

        /// <summary>
        /// Get archive statistics from backup database
        /// </summary>
        /// <returns>Statistics response</returns>
        Task<BackupArchiveStatisticsResponse> GetStatisticsAsync();

        /// <summary>
        /// Get drawing number to COMP table mappings
        /// </summary>
        /// <param name="drawingNumber">Optional drawing number filter</param>
        /// <returns>List of mappings</returns>
        Task<List<DrawingCompMappingResponse>> GetDrawingCompMappingsAsync(string drawingNumber = null);

        /// <summary>
        /// Get consumption summary by assembly from backup database
        /// </summary>
        /// <param name="assemblyPattern">Assembly pattern to search</param>
        /// <param name="productionSeries">Production series filter</param>
        /// <returns>Consumption summary data</returns>
        Task<List<BackupConsumptionSummaryResponse>> GetConsumptionSummaryAsync(string assemblyPattern, string productionSeries);

        /// <summary>
        /// Search for drawing numbers consumed in specific assembly
        /// </summary>
        /// <param name="productionSeries">Production series</param>
        /// <param name="assemblyNumber">Assembly number</param>
        /// <param name="componentId">Component ID</param>
        /// <returns>List of drawing numbers consumed in the assembly</returns>
        Task<List<SimpleCompDataResponse>> SearchByConsumedInAsync(string productionSeries, string assemblyNumber, string componentId);
    }
}
