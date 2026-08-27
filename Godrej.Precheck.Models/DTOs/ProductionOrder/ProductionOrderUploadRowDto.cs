namespace Godrej.Precheck.Models.DTOs.ProductionOrder
{
    /// <summary>
    /// DTO for each row parsed from Excel upload
    /// </summary>
    public class ProductionOrderUploadRowDto
    {
        public string? ProductionOrderNumber { get; set; }
        public string? ProjectCode { get; set; }
        public string? ProjectDescription { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemDescription { get; set; }
        public string? StartIdNumber { get; set; } // e.g., "GA0153"
        public int Quantity { get; set; }
        public string? MRIRNumber { get; set; }
        public string? MIN { get; set; }
        public string? Status { get; set; }
        public string? BuildNumber { get; set; }
        public string? SnagSheetNo { get; set; }
    }
}
