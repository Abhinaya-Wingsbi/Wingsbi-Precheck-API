using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class GetAvailableComponentsResponse
    {
        public int Id { get; set; }
        public int? DrawingnumberId { get; set; }
        public string DrawingNumber { get; set; }
        public string LnItemCode { get; set; }
        public int? Prodseriesid { get; set; }
        public string ProductionSeries { get; set; }
        public string Location { get; set; }
        public string QrCodeNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? ManufacturingDate { get; set; }
        public string IdNumber { get; set; }
        public decimal Quantity { get; set; }
        public string ProjectNumber { get; set; }
        public string ProductionOrderNumber { get; set; }
        public string Status { get; set; }
        public decimal RemainingQuantity { get; set; }
        public string Unit { get; set; }
        public string Remarks { get; set; }
        public string FanManNo { get; set; }

        /// <summary>
        /// Sum of Quantity across all rows sharing this row's DrawingnumberId (not just this row).
        /// </summary>
        public decimal TotalQrQuantity { get; set; }

        /// <summary>
        /// Count of rows sharing this row's DrawingnumberId (not just this row).
        /// </summary>
        public int TotalQrNumber { get; set; }
    }
}
