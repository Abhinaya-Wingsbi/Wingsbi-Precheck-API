using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel
{
    public class User
    {
        public int Id { get; set; } 
        public string Email { get; set; } 
        public string UserName { get; set; } 
        public string UserId { get; set; } 
        public int PlantId { get; set; }
        public string Plant { get; set; }
        public string PasswordHash { get; set; } 
        public string SecurityStamp { get; set; } 
        public DateTime? LastLoginAt { get; set; } 
        public int CreatedBy { get; set; } 
        public DateTime? CreatedDate { get; set; }
        public int ModifiedBy { get; set; } 
        public DateTime? ModifiedDate { get; set; } 
        public bool IsActive { get; set; } 
        public int? UserRoleId { get; set; } 
        public string? Role { get; set; }
        public int? ApprovedBy { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int SecurityQuestionId { get; set; }

        public string SecurityAnswer { get; set; }
    }


}
