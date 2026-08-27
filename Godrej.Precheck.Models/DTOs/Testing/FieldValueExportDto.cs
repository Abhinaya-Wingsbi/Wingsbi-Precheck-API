using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Testing
{
    public class FieldValueExportDto
    {
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
        public int RowNumber { get; set; }
    }
}
