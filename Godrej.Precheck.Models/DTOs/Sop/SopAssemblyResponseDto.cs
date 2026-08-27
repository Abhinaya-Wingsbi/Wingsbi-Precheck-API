using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Sop
{
    public class SopAssemblyResponseDto
    {
        public int Id { get; set; }
        public int DrawingNumberId { get; set; }
        public string Version { get; set; }
        public string DrawingNumber { get; set; }
        public string SopNames { get; set; }
    }
}
