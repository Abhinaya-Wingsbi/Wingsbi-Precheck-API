using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel.Archive;
using Godrej.Precheck.Models.DTOs.Archive;

namespace Godrej.Precheck.Repository.Repository.ArchiveRepository
{
    public interface IArchiveRepository
    {
        /// <summary>
        /// Get paginated archive data based on filter criteria
        /// </summary>
        /// <param name="request">Filter request parameters</param>
        /// <returns>Paginated archive data response</returns>
        Task<ArchiveDataPagedResponse> GetArchiveDataAsync(ArchiveFilterRequest request);

        /// <summary>
        /// Get dropdown options for assembly numbers and production series
        /// </summary>
        /// <returns>Dropdown options</returns>
        Task<ArchiveDropdownResponse> GetDropdownOptionsAsync();

        /// <summary>
        /// Get all components for a specific drawing number
        /// </summary>
        /// <param name="drawingNumber">Drawing number</param>
        /// <returns>List of component data</returns>
        Task<List<CompData>> GetComponentsByDrawingNumberAsync(string drawingNumber);

        /// <summary>
        /// Get consumption details by assembly pattern and production series
        /// </summary>
        /// <param name="assemblyPattern">Assembly pattern to search</param>
        /// <param name="productionSeries">Production series filter</param>
        /// <returns>Consumption summary data</returns>
        Task<List<object>> GetConsumptionByAssemblyAsync(string assemblyPattern, string productionSeries);

        /// <summary>
        /// Get archive data for export (all records matching criteria)
        /// </summary>
        /// <param name="request">Filter request parameters</param>
        /// <returns>List of archive data for export</returns>
        Task<List<ArchiveDataResponse>> GetArchiveDataForExportAsync(ArchiveFilterRequest request);

        /// <summary>
        /// Get archive statistics and summary information
        /// </summary>
        /// <returns>Archive statistics</returns>
        Task<object> GetArchiveStatisticsAsync();

        /// <summary>
        /// Check if COMP data exists for a specific drawing number
        /// </summary>
        /// <param name="drawingNumberId">Drawing number ID</param>
        /// <returns>True if COMP data exists</returns>
        Task<bool> HasCompDataAsync(int drawingNumberId);

        /// <summary>
        /// Get distinct assembly numbers that contain the specified pattern
        /// </summary>
        /// <param name="pattern">Assembly pattern to search</param>
        /// <returns>List of matching assembly numbers</returns>
        Task<List<string>> GetAssemblyNumbersByPatternAsync(string pattern);

        /// <summary>
        /// Get component history for a specific component ID across all assemblies
        /// </summary>
        /// <param name="componentId">Component ID to search</param>
        /// <returns>Component usage history</returns>
        Task<List<ArchiveDataResponse>> GetComponentHistoryAsync(string componentId);

        /// <summary>
        /// Get ALL archive data without pagination based on filter criteria
        /// </summary>
        /// <param name="request">Filter request parameters</param>
        /// <returns>All matching archive data</returns>
        Task<List<ArchiveDataResponse>> GetAllArchiveDataAsync(ArchiveFilterRequest request);
    }
}
