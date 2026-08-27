using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    public class ProjectDetailsResponse
    {
        public string? ProjectNumber { get; set; }
        public string? ProductionOrderNumber { get; set; }
        public int? DrawingNumberId { get; set; }
        public decimal? Quantity { get; set; }
        public int? IdNumbers { get; set; }
        public int? ProdSeriesId { get; set; }
    }
}
