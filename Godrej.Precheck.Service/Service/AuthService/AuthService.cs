using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DTOs.Login;
using Godrej.Precheck.Models.DTOs.Register;
using Godrej.Precheck.Models.DTOs.Reset;
using Godrej.Precheck.Repository.Repository.UserRepository;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Godrej.Precheck.Service.Service.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;

        private const int SaltSize = 128 / 8;
        private const int Iterations = 100000;
        private const int HashSize = 256 / 8;

        public AuthService(IUserRepository userRepository, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            _logger.LogInformation("Login request received for UserId: {UserId}", request.UserId);

            try
            {
                var user = await _userRepository.GetUserByUserIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Login failed: UserId {UserId} not found.", request.UserId);
                    await Task.Delay(100);
                    throw new ApplicationException("Invalid credentials or user is deactivated");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Login failed: User {UserId} is inactive.", request.UserId);
                    throw new ApplicationException("Invalid credentials or user is deactivated");
                }

                if (user.ApprovedBy != 1)
                {
                    _logger.LogWarning("Login failed: User {UserId} is not approved by admin.", request.UserId);
                    throw new ApplicationException("User not approved by admin");
                }

                if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.SecurityStamp))
                {
                    _logger.LogWarning("Login failed: Invalid password for UserId {UserId}", request.UserId);
                    throw new ApplicationException("Invalid credentials");
                }

                _logger.LogTrace("Generating JWT token for UserId: {UserId}", request.UserId);
                var token = GenerateJwtToken(user);
            _logger.LogTrace("Token Generated Successfully");

                var refreshToken = GenerateRefreshToken();

                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.AddRefreshTokenAsync(new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(7)
                });

                _logger.LogInformation("Login successful for UserId: {UserId}", request.UserId);
                return new AuthResponse { Token = token, RefreshToken = refreshToken };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during login for UserId: {UserId}", request.UserId);
                throw;
            }
        }

        public async Task<bool> ResetAsync(ResetRequest request)
        {
            _logger.LogInformation("Reset request received for UserId: {UserId}", request.UserId);

            try
            {
                
                var existingUser = await _userRepository.GetUserByUserid(request.UserId);

                if (existingUser == null)
                {
                    _logger.LogWarning("User with UserId: {UserId} does not exist.", request.UserId);
                    throw new ApplicationException("User does not exist.");
                }

                if (existingUser.SecurityQuestionId != request.SecurityQuestionId)
                {
                    _logger.LogWarning("Security Question didn't match: SecurityQuestionId {SecurityQuestionId} already exists.", request.SecurityQuestionId);
                    throw new ApplicationException("Incorrect Security Question");
                }

                if (existingUser.SecurityAnswer != request.SecurityAnswer)
                {
                    _logger.LogWarning("Security answer: {Security} is wrong", request.SecurityAnswer);
                    throw new ApplicationException("Incorrect Security Answer");
                }

                var (hash, securityStamp) = HashPassword(request.Password);
                var user = new ResetModel
                {
                    
                    UserId = request.UserId,
                    PasswordHash = hash,
                    SecurityStamp = securityStamp,
                    SecurityAnswer = request.SecurityAnswer
                };

                await _userRepository.UpdateUserAsync(user);
                _logger.LogInformation("User registered successfully with UserId: {UserId}", request.UserId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during registration for UserId: {UserId}", request.UserId);
                throw;
            }
        }




        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            _logger.LogInformation("Register request received for Email: {Email}", request.Email);

            try
            {
                   //validation for EmailId 
                    var UserByEmail = await _userRepository.GetUserByEmail(request.Email);

                    if (UserByEmail != null)
                    {                       
                        _logger.LogWarning("Registration failed: Email {Email} already exists.", request.Email);
                        throw new ValidationException("Email already exists");                       
                    }

                    //validation for UserId
                    var UserByUserId = await _userRepository.GetUserByUserid(request.UserId);

                    if (UserByUserId != null)
                    {                      
                        _logger.LogWarning("Registration failed: UserId {UserId} already exists.", request.UserId);
                        throw new ValidationException("UserId already exists");
                        
                   } 
                
                var (hash, securityStamp) = HashPassword(request.Password);
                var user = new User
                {
                    UserName = request.UserName,
                    UserId = request.UserId,
                    Email = request.Email,
                    PlantId=request.PlantId,
                    UserRoleId =request.UserroleId,
                    PasswordHash = hash,
                    SecurityStamp = securityStamp,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    DepartmentId = request.DeptId,
                    SecurityQuestionId = request.SecurityQuestionId,
                    SecurityAnswer = request.SecurityAnswer
                };

                await _userRepository.AddUserAsync(user);
                _logger.LogInformation("User registered successfully with UserId: {UserId}, Email: {Email}", request.UserId, request.Email);

                return true;
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning("Validation error occurred while registering user: {Message}", vex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during registration for UserName: {UserName}, Email: {Email}", request.UserName, request.Email);
                return false;
            }
        }

        // Updated secure password hashing method
        private (string Hash, string SecurityStamp) HashPassword(string password)
        {
            // Generate a cryptographically secure random salt
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password using PBKDF2 with HMAC-SHA256
            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: HashSize
            );

            // Store both hash and salt as Base64 strings
            string hashString = Convert.ToBase64String(hash);
            string saltString = Convert.ToBase64String(salt);

            return (Hash: hashString, SecurityStamp: saltString);
        }

        // Updated secure password verification method
        private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            try
            {
                byte[] salt = Convert.FromBase64String(storedSalt);
                byte[] expectedHash = Convert.FromBase64String(storedHash);

                // Hash the input password with the stored salt
                byte[] actualHash = KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: Iterations,
                    numBytesRequested: HashSize
                );

                // Use constant-time comparison to prevent timing attacks
                return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password hash");
                return false;
            }
        }

        // Existing methods remain the same
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim("userid", user.UserId),
                    new Claim("username", user.UserName),
                    new Claim("roleid", user.UserRoleId.ToString()),
            new Claim(ClaimTypes.Role, user.Role),  // Instead of "role"
                    //new Claim("plantid", user.PlantId.ToString()),
                    //new Claim("email", user.Email),
            new Claim("deptid",Convert.ToString(user.DepartmentId)),
                    new Claim("department", user.DepartmentName)
                }),
                Expires = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration["Jwt:ExpiryTimeInDays"])),
                Issuer = _configuration["Jwt:Issuer"],         // Add this
                Audience = _configuration["Jwt:Audience"],      // Add this
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256)  // Changed to HmacSha256 from HmacSha256Signature
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        // Removed unused GenerateSecurityStamp method since we now use the salt as SecurityStamp
    }
}