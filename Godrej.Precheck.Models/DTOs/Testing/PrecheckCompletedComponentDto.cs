namespace Godrej.Precheck.Models.DTOs.Testing
{
    public class PrecheckCompletedComponentDto
    {
        public int Id { get; set; }
        public string? ProductionOrderNumber { get; set; }
        public string? ProjectNumber { get; set; }
        public string? ProjectDescription { get; set; }
        public string? LnItemCode { get; set; }
        public string? ItemDescription { get; set; }
        public string? DrawingNumber { get; set; }
        public string? Nomenclature { get; set; }
        public string? ComponentType { get; set; }
        public string? Min { get; set; }
        public string? BuildNumber { get; set; }
        public int? Quantity { get; set; }
        public string? MrirNumber { get; set; }
        public int PrecheckStatus { get; set; }
        public string PrecheckStatusName { get; set; } = "Completed";
        public DateTime? LastModifiedDate { get; set; }
        public string? MsnNumber { get; set; }
        public int? MsnQuantity { get; set; }
    }
}
