using System.Text.Json;
using System.Text.Json.Serialization;

namespace Godrej.Precheck.Models.DTOs.Testing
{
    public class SaveStageDataRequestDto
    {
        public string DrawingNumber { get; set; } = string.Empty;

        // Identifies which inspection instance (Memo Stage No) this save belongs to.
        // Required: without it, saves for different MSN batches of the same drawing
        // would collide onto the same tbl_inspection_master row.
        public string MsnNumber { get; set; } = string.Empty;
        public int StageId { get; set; }
        public int TotalRows { get; set; }
        public List<StageFieldValueDto> FixedFields { get; set; } = new();
        public List<HeaderFieldSaveDto> HeaderFields { get; set; } = new();
        public List<StageRowDto> Rows { get; set; } = new();

        // Captures any extra top-level properties the frontend sends
        // e.g. msnNo, refDoc, total_joints — automatically treated as fixed fields
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraProperties { get; set; }
    }

    public class HeaderFieldSaveDto
    {
        public int? Id { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? FieldValue { get; set; }
    }

    public class StageRowDto
    {
        public int RowNumber { get; set; }
        public List<StageFieldValueDto> Fields { get; set; } = new();
    }

    public class StageFieldValueDto
    {
        public string FieldName { get; set; } = string.Empty;
        public string? Value { get; set; }
    }
}
