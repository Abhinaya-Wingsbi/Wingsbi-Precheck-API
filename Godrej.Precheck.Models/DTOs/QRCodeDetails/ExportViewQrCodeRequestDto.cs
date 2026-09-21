using System;
using System.Collections.Generic;

namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    /// <summary>
    /// Request body for POST /api/QRCode/ExportViewQrCode.
    /// Two mutually exclusive ways to pick which QR codes to export:
    ///  - QRCodeNumbers: export this specific, already-known list.
    ///  - SearchQuery/ProdSeries/CreatedBy/FromDate/ToDate: same filter shape as GetBarcodeDetailsWithParameters.
    /// SelectedColumns controls which columns land in the exported workbook (empty/null = all).
    /// </summary>
    public class ExportViewQrCodeRequestDto
    {
        public List<string>? QRCodeNumbers { get; set; }
        public List<string>? BatchIdNumbers { get; set; }
        public int? QrCodeStatusId { get; set; }

        public string? SearchQuery { get; set; }
        public List<string>? ProdSeries { get; set; }
        public List<int>? CreatedBy { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public List<string>? SelectedColumns { get; set; }
    }
}
