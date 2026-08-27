using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.DrawingNumber
{
    public class AddUnitRequestDto
    {
        [Required]
        public string UnitName { get; set; }
        public int CreatedBy { get; set; }
    }
}
