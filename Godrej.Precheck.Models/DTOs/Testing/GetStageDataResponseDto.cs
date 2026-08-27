namespace Godrej.Precheck.Models.DTOs.Testing
{
    public class GetStageDataResponseDto
    {
        public string DrawingNumber { get; set; } = string.Empty;
        public string MsnNumber { get; set; } = string.Empty;
        public int StageId { get; set; }
        public int TotalRows { get; set; }
        public bool Stage1Completed { get; set; }
        public bool Stage2Completed { get; set; }
        public bool Stage3Completed { get; set; }
        public bool CurrentStageCompleted { get; set; }
        public List<HeaderFieldValueDto> HeaderFields { get; set; } = new();
        public List<StageFieldValueDto> FixedFields { get; set; } = new();
        public List<StageRowDataDto> Rows { get; set; } = new();
    }

    public class HeaderFieldValueDto
    {
        public int Id { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? FieldLabel { get; set; }
        public string? FieldType { get; set; }
        public string? Value { get; set; }
    }

    public class StageRowDataDto
    {
        public int RowNumber { get; set; }
        public List<StageFieldValueDto> Fields { get; set; } = new();
    }
}
