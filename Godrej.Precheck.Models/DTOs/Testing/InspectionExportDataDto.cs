using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Testing
{
    public class InspectionExportDataDto
    {
        public int MasterId { get; set; }
        public string DrawingNumber { get; set; }
        public int TemplateId { get; set; }
        public string HtmlTemplate { get; set; }
    }
}
