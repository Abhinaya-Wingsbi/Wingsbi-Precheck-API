using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.User
{
    public class AddDepartmentRequestDto
    {
        [Required]
        public string DepartmentName { get; set; }
        public int CreatedBy { get; set; }
    }
}
