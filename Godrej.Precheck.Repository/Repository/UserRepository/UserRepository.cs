using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DTOs.Reset;
using Godrej.Precheck.Repository.Database;
using Godrej.Precheck.Repository.Queries;
using Microsoft.Extensions.Logging;


namespace Godrej.Precheck.Repository.Repository.UserRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;
        private readonly IApplicationDbContext _db;

        public UserRepository(ILogger<UserRepository> logger, IApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<User?> GetUserByUserid(string userid)
        {
            _logger.LogInformation("Starting GetUserByUserid for userid: {userid}", userid);
            try
            {
                _logger.LogDebug("Executing query to fetch user by email and password");
                var result = await _db.GetSingle<User>(
                    Users.GET_USER_BY_USERID,
                    new { UserId = userid});

                if (result != null)
                {
                    _logger.LogInformation("User found for userid: {userid}", userid);
                }
                else
                {
                    _logger.LogWarning("No user found with userid: {userid}", userid);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user by userid: {userid}. Error: {ErrorMessage}", userid, ex.Message);
                throw;
            }
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            _logger.LogInformation("Starting GetUserByEmail for email: {Email}", email);
            try
            {
                _logger.LogDebug("Executing query to fetch user by email ");
                var result = await _db.GetSingle<User>(
                    Users.GET_USER_BY_EMAIL,
                    new { Email = email });

                if (result != null)
                {
                    _logger.LogInformation($"User found for email", email);
                }
                else
                {
                    _logger.LogWarning("No user found with email:", email);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user by email: Error:", email, ex.Message);
                throw;
            }
        }


        public async Task<User?> GetUserByUserName(string UserName)
        {
            _logger.LogInformation("Starting GetUserByUserName for UserName:", UserName);
            try
            {
                _logger.LogDebug("Executing query to fetch user by UserName ");
                var result = await _db.GetSingle<User>(
                    Users.GET_USER_BY_USERNAME,
                    new { UserName = UserName });

                if (result != null)
                {
                    _logger.LogInformation($"User found for UserName", UserName);
                }
                else
                {
                    _logger.LogWarning("No user found with UserName:", UserName);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user by UserName: Error:", UserName, ex.Message);
                throw;
            }
        }
     

        public async Task<User> RegisterUserAsync(User user)
        {
            _logger.LogInformation("Starting RegisterUserAsync for user: {UserEmail}", user.Email);
            try
            {
                _logger.LogDebug("Executing query to register new user with email: {Email}", user.Email);
                await _db.Execute(
                    Users.INSERT_USER_QUERY,
                    new
                    {
                        userid = user.UserId,
                        Email = user.Email,
                        Password = user.PasswordHash,
                        Name = user.UserName,
                        CreatedDate = DateTime.UtcNow
                    });

                _logger.LogInformation("Successfully registered user with email: {Email}", user.Email);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register user with email: {Email}. Error: {ErrorMessage}", user.Email, ex.Message);
                throw;
            }
        }

        public async Task<User> GetUserByUserIdAsync(string userid)
        {
            _logger.LogInformation("Starting GetUserByUserIdAsync for userid: {Username}", userid);
            try
            {
                _logger.LogDebug("Executing query to fetch user by userid");
                var result = await _db.GetSingle<User>(
                    Users.CHECK_USERNAME_EXISTS_QUERY,
                    new { userid = userid });

                if (result != null)
                {
                    _logger.LogInformation("User found with userid:", userid);
                }
                else
                {
                    _logger.LogWarning("No user found with userid: ", userid);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user by userId: {userId}. Error:", ex.Message);
                throw;
            }
        }

        public async Task UpdateUserAsync(ResetModel user)
        {
            _logger.LogInformation("Starting AddUserAsync for UserId: {UserId}", user.UserId);
            try
            {
                _logger.LogDebug("Executing query to add new user with UserId: {UserId}", user.UserId);
                await _db.Execute(
                    Users.UPDATE_USER_QUERY,
                    new
                    {
                        
                        passwordhash = user.PasswordHash,
                        userId = user.UserId,
                        securitystamp = user.SecurityStamp,
                        securityanswer = user.SecurityAnswer
                    });

                _logger.LogInformation("Successfully added user with UserId: {UserId}", user.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add user with UserId: {UserId}", user.UserId);
                throw;
            }
        }

        public async Task AddUserAsync(User user)
        {
            _logger.LogInformation("Starting AddUserAsync for username: {Username}, email: {Email}", user.UserName, user.Email);
            try
            {
                _logger.LogDebug("Executing query to add new user with username: {Username}, email: {Email}", user.UserName, user.Email);
                await _db.Execute(
                    Users.INSERT_USER_QUERY,
                    new
                    {
                        Username = user.UserName,
                        email = user.Email,
                        passwordhash = user.PasswordHash,
                        userId = user.UserId,
                        UserRoleId = user.UserRoleId,
                        PlantId = user.PlantId,
                        securitystamp = user.SecurityStamp,
                        createddate = DateTime.UtcNow,
                        IsActive = 1,
                        departmentid = user.DepartmentId,
                        securityquestionid = user.SecurityQuestionId,
                        securityanswer = user.SecurityAnswer
                    });

                _logger.LogInformation("Successfully added user with username: {Username}, email: {Email}", user.UserName, user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add user with username: {Username}, email: {Email}. Error: {ErrorMessage}", user.UserName, user.Email, ex.Message);
                throw;
            }
        }

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            _logger.LogInformation("Starting AddRefreshTokenAsync for user: {UserId}", refreshToken.UserId);
            try
            {
                _logger.LogDebug("Executing query to add refresh token. TokenId: {TokenId}, UserId: {UserId}", refreshToken.Id, refreshToken.UserId);
                await _db.Execute(
                    Users.INSERT_REFRESH_TOKEN_QUERY,
                    new
                    {
                        id = refreshToken.Id,
                        userid = refreshToken.UserId,
                        token = refreshToken.Token,
                        lastactivity = refreshToken.LastLoginAt,
                        isactive = 1,
                        expirydate = refreshToken.ExpiryDate
                    });

                _logger.LogInformation("Successfully added refresh token for user: {UserId}. Expiry: {ExpiryDate}", refreshToken.UserId, refreshToken.ExpiryDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add refresh token for user: {UserId}. Error: {ErrorMessage}", refreshToken.UserId, ex.Message);
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            _logger.LogInformation("Starting GetUserByIdAsync for user ID: {UserId}", userId);
            try
            {
                _logger.LogDebug("Executing query to fetch user by ID: {UserId}", userId);
                var result = await _db.GetSingle<User>(
                    Users.GET_USER_BY_ID_QUERY,
                    new { Id = userId });

                if (result != null)
                {
                    _logger.LogInformation("User found with ID: {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("No user found with ID: {UserId}", userId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user by ID: {UserId}. Error: {ErrorMessage}", userId, ex.Message);
                throw;
            }
        }

        public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
        {
            _logger.LogInformation("Starting UpdateRefreshTokenAsync for token ID: {TokenId}, user: {UserId}", refreshToken.Id, refreshToken.UserId);
            try
            {
                _logger.LogDebug("Executing query to update refresh token. TokenId: {TokenId}", refreshToken.Id);
                await _db.Execute(
                    Users.UPDATE_REFRESH_TOKEN_QUERY,
                    new
                    {
                        Id = refreshToken.Id,
                        Token = refreshToken.Token,
                        ExpiryDate = refreshToken.ExpiryDate
                    });

                _logger.LogInformation("Successfully updated refresh token ID: {TokenId}. New expiry: {ExpiryDate}", refreshToken.Id, refreshToken.ExpiryDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update refresh token ID: {TokenId}. Error: {ErrorMessage}", refreshToken.Id, ex.Message);
                throw;
            }
        }

        public async Task AddUserRoleAsync(UserRole userRole)
        {
            _logger.LogInformation("Starting AddUserRoleAsync for user: {UserId}, role: {Role}", userRole.Role);
            try
            {
                _logger.LogDebug("Executing query to add user role. UserId: {UserId}, Role: {Role}",  userRole.Role);
                await _db.Execute(
                    Users.INSERT_USER_ROLE_QUERY,
                    new
                    {
                        UserId = userRole.Id,
                        Role = userRole.Role,
                        IsActive = 1,
                        CreatedDate = DateTime.UtcNow
                    });

                _logger.LogInformation("Successfully added role {Role} to user {UserId}", userRole.Role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add role {Role} to user {UserId}. Error: {ErrorMessage}", userRole.Role);
                throw;
            }
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken)
        {
            _logger.LogInformation("Starting GetRefreshTokenAsync");
            try
            {
                _logger.LogDebug("Executing query to fetch refresh token");
                var result = await _db.GetSingle<RefreshToken>(
                    Users.GET_REFRESH_TOKEN_QUERY,
                    new { Token = refreshToken });

                if (result != null)
                {
                    _logger.LogInformation("Refresh token found. TokenId: {TokenId}, UserId: {UserId}", result.Id, result.UserId);
                }
                else
                {
                    _logger.LogWarning("No matching refresh token found");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve refresh token. Error: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}