using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.User
{
    public class AddUserRequestDto
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public int? RoleId { get; set; }
        public string? Password { get; set; }
        public int? DepartmentId { get; set; }
        [JsonIgnore]
        public string? SecurityStamp { get; set; }
    }
}
