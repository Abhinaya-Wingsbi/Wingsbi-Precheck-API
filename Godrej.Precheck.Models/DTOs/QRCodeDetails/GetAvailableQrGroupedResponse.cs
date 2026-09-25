namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    /// <summary>
    /// One row per distinct DrawingNumber+LnItemCode with at least one active QR code
    /// (tbl_qrcodedetails.isactive = 1 AND qrcodestatusid = 1), totals summed across its QR codes.
    /// </summary>
    public class GetAvailableQrGroupedResponse
    {
        public int DrawingNumberId { get; set; }
        public string DrawingNumber { get; set; }
        public string LnItemCode { get; set; }

        /// <summary>
        /// From tbl_drawingprodseriesmapping.availableseriesid (via ProductionSeries). Comma-separated
        /// when the drawing maps to more than one production series.
        /// </summary>
        public string ProdSeriesId { get; set; }
        public string ProductionSeries { get; set; }
        public string ComponentType { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalRemainingQuantity { get; set; }
        public int QrCount { get; set; }
    }
}
