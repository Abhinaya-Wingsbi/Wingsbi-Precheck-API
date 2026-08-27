using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class GetAvailableComponentsRequest
    {
        public int ProdSeriesId { get; set; }
        public int? DrawingNumberId { get; set; }
        public decimal? Quantity { get; set; }

        public int TotalQrQty { get; set; }

    }

}
