using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Register
{
    public class RegisterRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }

        public int UserroleId { get; set; }
        public int PlantId { get; set; }

        public int DeptId { get; set; }

        public int SecurityQuestionId { get; set; }

        public string SecurityAnswer { get; set; }

    }
}
