using System.ComponentModel.DataAnnotations;

namespace Godrej.Precheck.Models.DTOs.Archive
{
    /// <summary>
    /// Archive search request for finding drawing numbers consumed in assembly
    /// Based on ConsumedIn pattern: "D/K324-0000-000CB/321"
    /// </summary>
    public class ArchiveSearchRequest
    {
        /// <summary>
        /// Production series (e.g., "D", "A", "SH")
        /// </summary>
        [Required]
        public string ProductionSeries { get; set; }

        /// <summary>
        /// Assembly/Drawing number (e.g., "K324-0000-000CB")
        /// </summary>
        [Required]
        public string DrawingNumber { get; set; }

        /// <summary>
        /// Component ID number (e.g., "321", "862", "863")
        /// </summary>
        [Required]
        public string IdNumber { get; set; }
    }
}
