using System.ComponentModel.DataAnnotations;

namespace Godrej.Precheck.Models.DTOs.DrawingNumber
{
    public class DeleteDrawingNumberRequestDto
    {
        [Required] public string DrawingNumber { get; set; }
        [Required] public string LnItemCode { get; set; }
    }
}
