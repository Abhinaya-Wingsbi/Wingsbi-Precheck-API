namespace Godrej.Precheck.Models.DTOs.Testing
{
    public class StageRowValueRawDto
    {
        public int RowNumber { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? FieldValue { get; set; }
    }
}
