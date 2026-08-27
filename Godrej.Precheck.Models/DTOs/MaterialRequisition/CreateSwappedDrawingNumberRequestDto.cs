namespace Godrej.Precheck.Models.DTOs.MaterialRequisition
{
    public class CreateSwappedDrawingNumberRequestDto
    {
        public int SwappedDrawingNumberID { get; set; }
        public int? FromSwappedIdNumber { get; set; }
        public int? ToSwappedIdNumber { get; set; }
        public string? SwappedFromPONumber { get; set; }
        public string? SwappedToPONumber { get; set; }
        public string? IdNumber { get; set; }
    }
}
