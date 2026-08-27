using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.MSNNumber
{
    public class UpdateMSNDto
    {
        public string? MsnNumber { get; set; }
        public int? DrawingNumberId { get; set; }
        public string? LnItemCode { get; set; }
        public int? IdNumberStart { get; set; }
        public int? IdNumberEnd { get; set; }
        public int? Quantity { get; set; }
        public string? Remark { get; set; }
        public string? Stage { get; set; } // Keep for display purposes
        public int? StageId { get; set; } // Use this for update
        public string? Supplier { get; set; }
        public int ModifiedBy { get; set; }
        public string? IdNumberRange { get; set; }
        public int? NomenclatureId { get; set; }
        public int? ComponentTypeId { get; set; }
        public string? OperationNumber { get; set; }
    }
}
