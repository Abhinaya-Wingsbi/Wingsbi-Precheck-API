using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Stage
{
    public class UpdateUnitRequestDto
    {
        public int Id { get; set; }
        public string UnitName { get; set; }
        public int? ModifiedBy { get; set; }
    }
}
