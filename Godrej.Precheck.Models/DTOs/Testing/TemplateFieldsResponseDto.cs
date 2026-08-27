namespace Godrej.Precheck.Models.DTOs.Testing
{
    public class TemplateFieldsResponseDto
    {
        public List<HeaderFieldDto> HeaderFields { get; set; } = new();
        public List<ColumnGroupDto> ColumnGroups { get; set; } = new();
    }

    public class HeaderFieldDto
    {
        public int Id { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? FieldLabel { get; set; }
        public string? FieldValue { get; set; }
        public string? FieldType { get; set; }
        public int? DisplayOrder { get; set; }
    }

    public class ColumnFieldDefinitionDto
    {
        public int Id { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? FieldLabel { get; set; }
        public string? FieldType { get; set; }
        public int? DisplayOrder { get; set; }
    }

    public class ColumnGroupDto
    {
        public int FormulaHeaderId { get; set; }
        public string ColumnName { get; set; } = string.Empty;
        public string? ColumnValue { get; set; }
        public List<ColumnFieldDefinitionDto> Fields { get; set; } = new();
        /// <summary>
        /// One entry per MSN quantity row. Each dictionary contains "rowIndex" plus
        /// one key per field in Fields, pre-populated with null (or any saved value).
        /// </summary>
        public List<Dictionary<string, object?>> Rows { get; set; } = new();
    }
}
