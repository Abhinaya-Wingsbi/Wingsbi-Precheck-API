using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DTOs.Login;
using Godrej.Precheck.Models.DTOs.Register;
using Godrej.Precheck.Models.DTOs.Reset;

namespace Godrej.Precheck.Service.Service.AuthService
{
    public interface IAuthService
    {      
        Task<AuthResponse> LoginAsync(LoginRequest request);

        Task<bool> RegisterAsync(RegisterRequest request);

        Task<bool> ResetAsync(ResetRequest request);

       }
}
