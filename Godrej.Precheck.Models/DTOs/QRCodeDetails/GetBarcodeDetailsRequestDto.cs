using System;
using System.Collections.Generic;

namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    public class BarcodeSearchQueryDto
    {
        public string? QRCodeNumber { get; set; }
        public string? DrawingNumber { get; set; }
        public string? LineItemCode { get; set; }
        public List<string>? IdNumbers { get; set; }
    }

    /// <summary>
    /// Request body for POST /api/QRCode/GetBarcodeDetailsWithParameters.
    /// CreatedBy stays on the query string (?CreatedBy=127); every other filter is here.
    /// All filters combine with AND.
    /// </summary>
    public class GetBarcodeDetailsRequestDto
    {
        public BarcodeSearchQueryDto? SearchQuery { get; set; }
        public List<string>? ProdSeries { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
