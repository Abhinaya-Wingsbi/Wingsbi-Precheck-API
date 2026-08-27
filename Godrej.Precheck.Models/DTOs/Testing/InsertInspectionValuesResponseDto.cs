namespace Godrej.Precheck.Models.DTOs.Testing
{
    public class InsertInspectionValuesResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int InspectionMasterId { get; set; }
        public int InsertedCount { get; set; }
    }
}
