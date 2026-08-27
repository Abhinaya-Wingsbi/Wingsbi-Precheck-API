namespace Godrej.Precheck.Models.DTOs.Assembly
{
    public class AssemblyDrawingMappingDto
    {
        public string AssemblyNumber { get; set; }
        public string DrawingNumber { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public decimal? Quantity { get; set; }
        public string Unit { get; set; }
        public string ParentDrawingNumber { get; set; }
        public string? ConsumedProdSeriesId { get; set; }
        public string FindNo { get; set; }
        public string Nomenclature { get; set; }
        public string AssemblyLnItemCode { get; set; }
        public string ChildLnItemCode { get; set; }
        public decimal? RemainingQuantity { get; set; }
        public bool? DrawingNumberStatus { get; set; }
        public string? ComponentType { get; set; }
    }
}
