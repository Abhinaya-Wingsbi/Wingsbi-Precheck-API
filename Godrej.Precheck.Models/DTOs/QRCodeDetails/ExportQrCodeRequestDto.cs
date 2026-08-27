using System.Collections.Generic;

namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    public class ExportQrCodeRequestDto
    {
        public List<string>? QRCodeNumbers { get; set; }
        public List<string>? BatchIdNumbers { get; set; }
        public List<QrCodeNumberRefDto>? SerialNumberSummary { get; set; }
        public List<QrCodeNumberRefDto>? QrCodeDetails { get; set; }
    }

    public class QrCodeNumberRefDto
    {
        public string? QrCodeNumber { get; set; }
    }
}
