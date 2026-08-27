namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    public class DisableQRCodeRequestDto
    {
        public string QRCodeNumber { get; set; }
        public string? Remarks { get; set; }
        public int? ModifiedBy { get; set; }
    }
}
