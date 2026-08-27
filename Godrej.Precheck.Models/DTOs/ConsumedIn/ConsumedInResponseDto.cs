using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.ConsumedIn
{
    public class ConsumedInResponseDto
    {
        public string IdNumber { get; set; } 
        public string IRNumber { get; set; } 
        public string MSNNumber { get; set; } 
        public string ConsumedInDrawing { get; set; } 
        public string ConsumedInProductionOrderNumber { get; set; } 
        public string Username { get; set; } 
        public decimal Quantity { get; set; }
        public DateTime? Date { get; set; }
        public string? LnItemCode { get; set; }
        public int? LnItemCodeId { get; set; }
        public bool? IsRejected { get; set; }
        public string? RejectionReason { get; set; }
    }

}
