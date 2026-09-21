using System.Collections.Generic;

namespace Godrej.Precheck.Models.DTOs.MaterialRequisition
{
    public class ExportMaterialRequisitionRequestDto
    {
        public List<string>? SelectedColumns { get; set; }
        public string? Status { get; set; }
        public int StatusId { get; set; } = 0;
    }
}
