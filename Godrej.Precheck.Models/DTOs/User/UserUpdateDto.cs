using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.User
{
    public class UserUpdateDto
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public int DepartmentId { get; set; }
        public int PlantId { get; set; }
        public int UserRoleId { get; set; }
        public int? ModifiedBy { get; set; }
        public int? SecurityQuestionId { get; set; }
        public string? SecurityAnswer { get; set; }
    }
}
