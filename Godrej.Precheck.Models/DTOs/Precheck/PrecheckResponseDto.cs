using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class PrecheckResponseDto
    {
        public double Quantity { get; set; }
        public string Remarks { get; set; }
        public string? RefDocRemarks { get; set; }
        public string? ProductionOrderNumber { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public int? DrawingNumberId { get; set; }
        public string? Unit { get; set; }
        public string ConsumedDrawingNo { get; set; }
        //project detail specific field
        public int? ProjectNumberId { get; set; }
        public int? ProductionOrderNumberId { get; set; }
        public string? ShortDescription { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string IrNumber { get; set; }
        public string MsnNumber { get; set; }
    }
}
