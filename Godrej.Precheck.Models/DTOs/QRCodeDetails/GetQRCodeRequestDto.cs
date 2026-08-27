using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    public class GetQRCodeRequestDto
    {
        public string? QRCodeNumber { get; set; }
        public List<string>? QRCodeNumbers { get; set; }

        public int? ProdSeriesId { get; set; }
        public string? ProductionOrderNumber { get; set; }
        public int? DrawingNumberId { get; set; }
        public int? CreatedBy { get; set; }
        public int? LnItemCodeId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? FromBatchId { get; set; }
        public string? ToBatchId { get; set; }
        public string? FanManNumber { get; set; }
        public List<string>? BatchIdNumbers { get; set; }
        public int? QrCodeStatusId { get; set; }

    }
}
