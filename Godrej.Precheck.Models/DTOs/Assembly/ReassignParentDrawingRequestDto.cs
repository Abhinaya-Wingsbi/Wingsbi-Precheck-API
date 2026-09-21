namespace Godrej.Precheck.Models.DTOs.Assembly
{
    public class ReassignParentDrawingRequestDto
    {
        public string? DrawingNumberLnitemcode { get; set; }

        public string? ParentDrawingNumberLnitemcode { get; set; }

        public string? FindNo { get; set; }

        public string? UpdatedFindNo { get; set; }

        public decimal? Quantity { get; set; }
    }
}
