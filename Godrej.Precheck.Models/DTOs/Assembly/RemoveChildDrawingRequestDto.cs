using System.ComponentModel.DataAnnotations;

namespace Godrej.Precheck.Models.DTOs.Assembly
{
    public class RemoveChildDrawingRequestDto
    {
        [Required]
        public string AssemblyDrawingNumber { get; set; }

        [Required]
        public string AssemblyLnItemCode { get; set; }

        [Required]
        public string ChildDrawingNumber { get; set; }

        [Required]
        public string ChildLnItemCode { get; set; }
    }
}
