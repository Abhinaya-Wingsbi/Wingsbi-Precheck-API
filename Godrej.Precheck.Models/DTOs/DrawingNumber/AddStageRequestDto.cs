using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.DrawingNumber
{
    public class AddStageRequestDto
    {
        public string StageName { get; set; }
        public string StageType { get; set; }
        public int CreatedBy { get; set; }
    }
}
