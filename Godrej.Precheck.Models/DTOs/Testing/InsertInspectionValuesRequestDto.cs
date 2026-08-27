using System.ComponentModel.DataAnnotations;

namespace Godrej.Precheck.Models.DTOs.Testing
{
    public class InsertInspectionValuesRequestDto
    {
        [Required]
        public string DrawingNumber { get; set; } = string.Empty;

        [Required]
        public List<InspectionFieldValueRequestDto> Values { get; set; } = new();
    }
}
