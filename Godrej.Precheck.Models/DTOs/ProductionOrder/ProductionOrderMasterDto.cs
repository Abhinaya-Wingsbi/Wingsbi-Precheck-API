using System.Collections.Generic;
using Godrej.Precheck.Models.DTOs.Bom;

namespace Godrej.Precheck.Models.DTOs.ProductionOrder
{
    /// <summary>
    /// Response DTO for GET /api/ProductionOrder/GetByPONumber
    /// </summary>
    public class ProductionOrderMasterDto
    {
        public int Id { get; set; }
        public string ProductionOrderNumber { get; set; } = string.Empty;
        public string? ProjectNumber { get; set; }
        public string? ProjectDescription { get; set; }
        public string? LnItemCode { get; set; }
        public string? ItemDescription { get; set; }
        public int? ProdSeriesId { get; set; }
        public string? ProductionSeries { get; set; }
        public int? StartIdNumber { get; set; }
        public int? Quantity { get; set; }
        public int? DrawingNumberId { get; set; }
        public string? DrawingNumber { get; set; }
        public int? LnItemCodeId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? PrecheckStatus { get; set; }
        public string? PrecheckStatusName { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string? Nomenclature { get; set; }
        public string? ComponentType { get; set; }
        public string? RackLocation { get; set; }
        public string? MRIRNumber { get; set; }
        public string? Min { get; set; }
        public int? EndIdNumber { get; set; }
        public List<BomDetailsResponseDto> Components { get; set; } = new List<BomDetailsResponseDto>();
        public DateTime? ModifiedDate { get; set; }
        public string? BuildNumber { get; set; }
        public string? SnagSheetNo { get; set; }
        public int? UnitId { get; set; }
        public string? UnitName { get; set; }
    }
}
