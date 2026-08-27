using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.User
{
    public class UpdateDepartmentRequestDto
    {
        public int Id { get; set; }
        public string DepartmentName { get; set; }
        public int ModifiedBy { get; set; }
    }
}
