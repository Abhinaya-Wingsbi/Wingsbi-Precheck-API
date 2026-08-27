using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    public class BulkUpdateQRCodeRequestDto
    {
        public List<string> QrCodeNumbers { get; set; }
        public string? MRIRNumber { get; set; }
        public int? IRNumberId { get; set; }
        public int? MSNNumberId { get; set; }
        public string? ProjectNumber { get; set; }
        public string? HeatLotNumber { get; set; }
        public string? Size { get; set; }
        public int? LnItemCodeId { get; set; }
        public int? DrawingNumberId { get; set; }
        public int? ProductionSeriesId { get; set; }
        public string? FanManNumber { get; set; }
        public string? FanManSerialNumber { get; set; }
        public int? RackLocationId { get; set; }
        public int? UnitId { get; set; }
        public string? IdNumber { get; set; }
        public decimal? Quantity { get; set; }
    }
}
