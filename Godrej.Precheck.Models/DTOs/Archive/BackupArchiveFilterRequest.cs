using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Archive
{
    /// <summary>
    /// Request DTO for filtering archive COMP data from backup database
    /// Uses separated consumed_in fields for better querying
    /// </summary>
    public class BackupArchiveFilterRequest
    {
        /// <summary>
        /// Production Series (e.g., D, SH, A, B, C, etc.)
        /// Extracted from the first part of ConsumedIn field
        /// </summary>
        public string ConsumedInProdSeries { get; set; }

        /// <summary>
        /// Assembly Number (e.g., K326-0000-000CB, K329-0100-0CB)
        /// Extracted from the middle part of ConsumedIn field
        /// </summary>
        public string ConsumedInAssembly { get; set; }

        /// <summary>
        /// Component ID (e.g., 862, 863, T122, etc.)
        /// Extracted from the last part of ConsumedIn field
        /// </summary>
        public string ConsumedInId { get; set; }

        public string DrawingNumber { get; set; }

        /// <summary>
        /// Component ID from IDNos field (e.g., FIM, A/5, A/2)
        /// </summary>
        public string ComponentId { get; set; }

        /// <summary>
        /// Nomenclature for filtering by component type (e.g., Wire, Pin, Seal)
        /// </summary>
        public string Nomenclature { get; set; }

        public string IDNos { get; set; }

        public string CompTableName { get; set; }

        public string IRNumber { get; set; }

        public string MSNNumber { get; set; }

        public string ItemCode { get; set; }

        public string ComponentType { get; set; }

        /// <summary>
        /// Free text search term (searches across multiple fields)
        /// </summary>
        public string SearchTerm { get; set; }

        public int? PageNumber { get; set; }

        public int? PageSize { get; set; }
    }

    /// <summary>
    /// Simplified request for backward compatibility with existing API
    /// Maps to the separated fields in backup database
    /// </summary>
    public class SimpleArchiveFilterRequest
    {
        /// <summary>
        /// Production Series ID (for compatibility)
        /// Will be mapped to ConsumedInProdSeries
        /// </summary>
        public int? ProductionSeriesId { get; set; }

        /// <summary>
        /// Assembly Number ID (for compatibility)
        /// Will be mapped to ConsumedInAssembly
        /// </summary>
        public int? AssemblyNumberId { get; set; }

        /// <summary>
        /// Component ID (for compatibility)
        /// Will be mapped to ConsumedInId
        /// </summary>
        public string ComponentId { get; set; }

        public string ProductionSeries { get; set; }

        public string AssemblyNumber { get; set; }

        public string DrawingNumber { get; set; }
    }
}
