using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel.Precheck
{
    public class ProjectPrecheckRequest
    {
       
        public int? ProductionSeriesId { get; set; }
        public int? ProjectDetailsId { get; set; }
        public int DrawingNumberId { get; set; }
        public decimal Quantity { get; set; }
        public int Unit { get; set; }
        public int CreatedBy { get; set; }
        public string ComponentType { get; set; }
        public int? LnItemCodeId { get; set; }
    }
}
