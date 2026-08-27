using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Testing
{
    public class ExportInspectionRequestDto
    {
        [Required]
        public string DrawingNumber { get; set; }
    }
}
