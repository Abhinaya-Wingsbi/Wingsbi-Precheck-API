using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel.Archive
{
    /// <summary>
    /// Represents the mapping between Drawing Numbers and COMP tables
    /// </summary>
    public class CompDataInfo
    {
        public int Id { get; set; }
        public string CompTableName { get; set; } // e.g., COMP0001, COMP0002
        public int DrawingNumberId { get; set; }
        public bool IsActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation properties
        public virtual DrawingNumbers DrawingNumber { get; set; }
        public virtual ICollection<CompData> CompDataRecords { get; set; }
    }
}
