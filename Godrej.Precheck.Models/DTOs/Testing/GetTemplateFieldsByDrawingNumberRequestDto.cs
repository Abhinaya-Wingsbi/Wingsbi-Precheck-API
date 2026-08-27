using System.ComponentModel.DataAnnotations;

namespace Godrej.Precheck.Models.DTOs.Testing
{
    public class GetTemplateFieldsByDrawingNumberRequestDto
    {
        [Required]
        public string DrawingNumber { get; set; } = string.Empty;

        // Which inspection instance (Memo Stage No) to prefill saved values from.
        // Optional: omit when starting a brand-new MSN (returns empty field values).
        public string? MsnNumber { get; set; }
        public int? MsnQuantity { get; set; }
        public int? StageId { get; set; }
    }
}
