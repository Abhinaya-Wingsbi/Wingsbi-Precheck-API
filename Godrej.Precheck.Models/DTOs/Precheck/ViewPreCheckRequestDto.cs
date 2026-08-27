using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class ViewPreCheckRequestDto
    {
        public string? ProductionOrderNumber { get; set; } = null;
        public int? ProductionSeriesId { get; set; }
        public int? Id { get; set; }
        public int? DrawingNumberId { get; set; }
        public bool RemainingPrecheck { get; set; }
    }
}
