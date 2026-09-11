using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Archive
{
    /// <summary>
    /// Response DTO for archive COMP data display
    /// </summary>
    public class ArchiveDataResponse
    {
        public int Id { get; set; }
        public string PONumber { get; set; } // Production Order Number (from AssemblyId/IDNos)
        public string DrawingNumber { get; set; } // Drawing Number from mapping
        public string Nomenclature { get; set; }
        public string Quantity { get; set; } // Quantity with units (e.g., "1.0mtr", "2.5kg")
        public string IDNumber { get; set; } // Component ID (ComponentId from parsed ConsumedIn)
        public string IRNumber { get; set; } // IRNos
        public string MSNNumber { get; set; } // MSNNos
        public string Status { get; set; }
        public DateTime? CreatedDate { get; set; } // MyDate or CreatedDate
        public string AssemblyNumber { get; set; }
        public string ProductionSeries { get; set; }
        public string ConsumedIn { get; set; }
        public string Remarks { get; set; }
        public string UserName { get; set; }
    }

    /// <summary>
    /// Paginated response wrapper
    /// </summary>
    public class ArchiveDataPagedResponse
    {
        public List<ArchiveDataResponse> Data { get; set; } = new List<ArchiveDataResponse>();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }

    /// <summary>
    /// Response for dropdown options
    /// </summary>
    public class ArchiveDropdownResponse
    {
        public List<string> AssemblyNumbers { get; set; } = new List<string>();
        public List<string> ProductionSeries { get; set; } = new List<string>();
    }
}
