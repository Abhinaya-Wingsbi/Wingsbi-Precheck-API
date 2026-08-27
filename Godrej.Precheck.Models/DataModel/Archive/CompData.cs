using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel.Archive
{
    /// <summary>
    /// Represents the consolidated COMP data from all COMP tables
    /// </summary>
    public class CompData
    {
        public int Id { get; set; }
        public int CompInfoId { get; set; }
        public string IDNos { get; set; } // Component ID (A/5, A/2, FIM, etc.)
        public string IRNos { get; set; }
        public string MSNNos { get; set; }
        public string ConsumedIn { get; set; } // Assembly where component is consumed (e.g., D/K326-0000-000CB/862)
        public string Remarks { get; set; }
        public decimal? Quantity { get; set; }
        public DateTime? MyDate { get; set; }
        public string SrNos { get; set; }
        public string UserName { get; set; }
        
        // Parsed fields from ConsumedIn
        public string AssemblyId { get; set; } // Same as IDNos, kept for clarity
        public string ProductionSeries { get; set; } // Extracted from ConsumedIn (D, SH, A, etc.)
        public string AssemblyNumber { get; set; } // Extracted assembly number from ConsumedIn
        public string ComponentId { get; set; } // Last part of ConsumedIn (862, 863, etc.)
        
        public bool IsActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation properties
        public virtual CompDataInfo CompInfo { get; set; }
    }
}
