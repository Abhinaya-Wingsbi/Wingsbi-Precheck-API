using System.ComponentModel.DataAnnotations;

namespace Godrej.Precheck.Models.DTOs.Assembly
{
    public class AddAssemblyDrawingMappingRequestDto
    {
       public string? DrawingNumber { get; set; }
        public string? ParentDrawingNumber { get; set; }
        public string? AssemblyLnItemCode { get; set; }
      public string? ChildLnItemCode { get; set; }
        public string? FindNo { get; set; }
        public string? ConsumedProdSeriesId { get; set; }
        public decimal? Quantity { get; set; }
        public string? Unit { get; set; }
        public string? Nomenclature { get; set; }
    }
}
