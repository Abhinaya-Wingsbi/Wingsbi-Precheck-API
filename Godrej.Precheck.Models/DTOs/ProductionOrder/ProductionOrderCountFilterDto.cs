using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.ProductionOrder
{
    public class ProductionOrderCountFilterDto
    {
        public string? DateFilterType { get; set; }
        public DateTime? FilterDate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? PrecheckStatus { get; set; }
        public string? DrawingNumber { get; set; }
        public string? PoNumber { get; set; }
        public string? LnItemCode { get; set; }
        public int RoleId { get; set; }
    }
}
