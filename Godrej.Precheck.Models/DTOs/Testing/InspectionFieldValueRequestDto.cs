using System.ComponentModel.DataAnnotations;

namespace Godrej.Precheck.Models.DTOs.Testing
{
    public class InspectionFieldValueRequestDto
    {
        [Required]
        public string FieldName { get; set; } = string.Empty;

        public string? Value { get; set; }
    }
}
