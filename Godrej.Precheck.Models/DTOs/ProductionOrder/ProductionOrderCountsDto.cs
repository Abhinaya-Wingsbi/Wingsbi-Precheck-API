using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.ProductionOrder
{
    public class ProductionOrderCountsDto
    {
        public int TotalCount { get; set; }
        public int CompletedCount { get; set; }
        public int PartialCount { get; set; }
        public int PendingCount { get; set; }
        public int UploadedCount { get; set; }
    }
}
