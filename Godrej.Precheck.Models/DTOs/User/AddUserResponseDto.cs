using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.User
{
    public class AddUserResponseDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public int? RoleId { get; set; }

        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
