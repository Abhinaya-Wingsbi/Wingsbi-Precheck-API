using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Scripts
{
    public class RunScriptRequestDto
    {
        public List<string> FileName { get; set; } = new();
    }
}
