using System;

namespace Godrej.Precheck.Models.DataModel.ProductionOrder
{
    public class ProductionOrderMaster
    {
        public int Id { get; set; }
        public string ProductionOrderNumber { get; set; } = string.Empty;
        public string? ProjectNumber { get; set; }
        public string? ProjectDescription { get; set; }
        public string? LnItemCode { get; set; }
        public string? ItemDescription { get; set; }
        public int? ProdSeriesId { get; set; }
        public int? StartIdNumber { get; set; }
        public int? Quantity { get; set; }
        public int? DrawingNumberId { get; set; }
        public int? LnItemCodeId { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? MRIRNumber { get; set; }
        public string? MIN { get; set; }
        public string? Status { get; set; }
        public string? BuildNumber { get; set; }
        public string? SnagSheetNo { get; set; }
    }
}
