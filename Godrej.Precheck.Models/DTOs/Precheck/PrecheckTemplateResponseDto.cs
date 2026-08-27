using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class PrecheckTemplateResponseDto
    {
        public int Id { get; set; }
        public string AssemblyNumber { get; set; }
        public string DrawingNumber { get; set; }
        public string Nomenclature { get; set; }
    }
}
