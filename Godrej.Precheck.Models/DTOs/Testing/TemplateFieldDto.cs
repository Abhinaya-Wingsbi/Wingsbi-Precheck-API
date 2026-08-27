using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Testing
{
    public class TemplateFieldDto
    {
        public int Id { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? FieldLabel { get; set; }
        public string? FieldValue { get; set; }
        public int? StageId { get; set; }
        public string? FieldType { get; set; }
        public int? DisplayOrder { get; set; }
        public bool IsRowField { get; set; }
        public int? FormulaHeaderId { get; set; }
        public string? FormulaHeaderName { get; set; }
        public string? FormulaHeaderValue { get; set; }
    }
}
