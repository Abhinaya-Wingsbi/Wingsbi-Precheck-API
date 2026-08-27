using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class MakeOrderRequestDto
    {
        public string? ProductionOrderNumber { get; set; }
        public int? ProductionSeriesId { get; set; }
        public int DrawingNumberId { get; set; }
        public int CreatedBy { get; set; }
        public List<int> Ids { get; set; }
        public int? LnItemCodeId { get; set; }
        public string? LnItemCode { get; set; }

    }
}
    