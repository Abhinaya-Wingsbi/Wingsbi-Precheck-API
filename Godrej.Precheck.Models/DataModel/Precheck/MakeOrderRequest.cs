using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel.Precheck
{
    public class MakeOrderRequest
    {
        public string? ProjectName { get; set; }
        public string? ProductionOrderNumber { get; set; }
        public int? ProductionSeriesId { get; set; }
        public int? Id { get; set; }
        public int DrawingNumberId { get; set; }
        public int CreatedBy { get; set; }
        public int? LnItemCodeId { get; set; }

    }
}
