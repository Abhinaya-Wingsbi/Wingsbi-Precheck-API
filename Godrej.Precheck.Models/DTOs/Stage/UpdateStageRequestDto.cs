using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Stage
{
    public class UpdateStageRequestDto
    {
        public int Id { get; set; }
        public string StageName { get; set; }
        public string StageType { get; set; }
        public int? ModifiedBy { get; set; }
    }
}
