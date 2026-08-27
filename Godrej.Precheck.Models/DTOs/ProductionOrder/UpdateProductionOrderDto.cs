namespace Godrej.Precheck.Models.DTOs.ProductionOrder
{
    public class UpdateProductionOrderDto
    {
        public int Id { get; set; }
        public string ProductionOrderNumber { get; set; } = string.Empty;
        public string? ProjectCode { get; set; }
        public string? ProjectDescription { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemDescription { get; set; }
        public int? StartIdNumber { get; set; }
        public int? Quantity { get; set; }
        public string? MRIRNumber { get; set; }
        public int? ProdSeriesId { get; set; }
        public string? Min { get; set; }
        public string? BuildNumber { get; set; }
        public string? SnagSheetNo { get; set; }
    }
}
