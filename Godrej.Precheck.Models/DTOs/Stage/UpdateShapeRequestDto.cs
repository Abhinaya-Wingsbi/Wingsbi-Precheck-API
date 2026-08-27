using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Stage
{
    public class UpdateShapeRequestDto
    {
        public int Id { get; set; }
        public string ShapeName { get; set; }
        public int? ModifiedBy { get; set; }
    }
}
