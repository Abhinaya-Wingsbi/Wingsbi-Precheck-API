using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
