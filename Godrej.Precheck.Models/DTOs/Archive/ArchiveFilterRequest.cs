using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Archive
{
    /// <summary>
    /// Request DTO for filtering archive COMP data (returns ALL matching records)
    /// </summary>
    public class ArchiveFilterRequest
    {
        public string AssemblyNumber { get; set; } // K329-0100-0CB format (for backward compatibility)
        public string ProductionSeries { get; set; } // A, B, C, D, E, F, G, SH (for backward compatibility)
        public string IdNumber { get; set; } // Component ID number (862, 863, etc.)
        public int? DrawingNumberId { get; set; } // Optional direct filter by drawing number
        public int? ProductionSeriesId { get; set; } // ID from tbl_productionseries
        public int? AssemblyNumberId { get; set; } // ID from tbl_assemblynumber
        // Removed PageNumber and PageSize - now returns ALL matching data
    }
}
