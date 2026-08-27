using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel
{
    public class RefreshToken
    {

        public int Id { get; set; }
        public int UserId { get; set; } 
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public bool IsActive { get; set; }
        public string Token { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? LastActivity { get; set; } // Optional: Last time the token was used
        public DateTime? RevokedAt { get; set; } // Optional: When the token was revoked

    }
}
