using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DTOs.Reset;

namespace Godrej.Precheck.Repository.Repository.UserRepository
{
    public interface IUserRepository
    {
        //Task AddRefreshTokenAsync(RefreshToken refreshToken);
        //Task<User?> GetUserByEmailAndPasswordAsync(string email, string password);


        Task<User?> GetUserByEmail(string email);

        Task<User?> GetUserByUserName(string UserName);

        Task<User?> GetUserByUserid(string userid);

        Task UpdateUserAsync(ResetModel user);

        //new Implementation 
        Task<User> RegisterUserAsync(User Usermodel);
        Task AddUserAsync(User user);

        //Task<User> AddUserAsync(User user);
        Task AddRefreshTokenAsync(RefreshToken refreshToken);

        //helper 
        Task<User?> GetUserByIdAsync(int userId);
        Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
        Task AddUserRoleAsync(UserRole userRole);
        Task<RefreshToken> GetRefreshTokenAsync(string refreshToken);
        Task<User> GetUserByUserIdAsync(string userid);
    }
}
