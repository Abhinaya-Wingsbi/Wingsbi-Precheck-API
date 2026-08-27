namespace Godrej.Precheck.Models.DTOs.Testing
{
    public class SaveStageDataResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int MasterId { get; set; }
        public int StageId { get; set; }
        public int RowsSaved { get; set; }
    }
}
