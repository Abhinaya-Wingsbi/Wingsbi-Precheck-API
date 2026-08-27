using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Login
{
    public class LoginRequest
    {
        public string UserId { get; set; }

        public string Password { get; set; }
    }
}
