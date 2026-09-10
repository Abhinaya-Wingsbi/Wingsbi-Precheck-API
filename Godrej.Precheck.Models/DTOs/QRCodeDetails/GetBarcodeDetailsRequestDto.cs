using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godrej.Precheck.Models.Json;

namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    /// <summary>
    /// Request body for POST /api/QRCode/GetBarcodeDetailsWithParameters.
    /// Every filter lives here; only pageNumber/pageSize stay on the query string, for pagination.
    /// SearchQuery is a single free-text value matched against qrCodeNumber, drawingNumber,
    /// lnItemCode, idNumber and productionOrderNumber. All filters combine with AND.
    /// </summary>
    public class GetBarcodeDetailsRequestDto
    {
        public string? SearchQuery { get; set; }
        public List<string>? ProdSeries { get; set; }
        public int? CreatedBy { get; set; }
        [JsonConverter(typeof(NullableDateTimeConverter))]
        public DateTime? FromDate { get; set; }
        [JsonConverter(typeof(NullableDateTimeConverter))]
        public DateTime? ToDate { get; set; }
    }
}
