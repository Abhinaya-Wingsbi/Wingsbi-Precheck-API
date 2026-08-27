using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DTOs.Login;
using Godrej.Precheck.Models.DTOs.Precheck;
using Godrej.Precheck.Models.DTOs.Register;
using Godrej.Precheck.Models.DTOs.Reset;
using Godrej.Precheck.Models.DTOs.User;
using Godrej.Precheck.Service.Service.AuthService;
using Godrej.Precheck.Service.Service.CommonSevice;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Godrej.Precheck.Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase

    {
        private readonly IAuthService _authService;
        private readonly ICommonService _commonService;

        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthService authService, ICommonService commonService, ILogger<AuthController> logger)

        {
            _authService = authService;
            _commonService = commonService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody]LoginRequest request)
        {
            _logger.LogInformation($"Request received for AuthController:Login-{request}");

            try
            {
                var response = await _authService.LoginAsync(request);

                if (response == null)
                {
                    _logger.LogWarning($"AuthController:Login - Login failed. Invalid credentials for {request}");
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                _logger.LogInformation($"AuthController:Login - Successful login for {request} and response is {response}");
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "AuthController:Login - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuthController:Login - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }


        [HttpPost("reset")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<User>> Reset([FromBody]ResetRequest request)
        {
            _logger.LogInformation($"Request received for AuthController: ResetPassword Method{request}");

            try
            {
                var response = await _authService.ResetAsync(request);

                if (!response)
                {
                    _logger.LogWarning($"AuthController:Reset - Reset Password failed for {request.UserId}");
                    return BadRequest(new { message = "Reset Password failed. Please try again." });
                }

                _logger.LogInformation($"AuthController:Reset Password - Successful reset for {response}", request.UserId);
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "AuthController:Reset Password - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuthController:Reset Password - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }



        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<User>> Register([FromBody]RegisterRequest request)
        {
            _logger.LogInformation($"Request received for AuthController: Register Method{request}");

            try
            {
                var response = await _authService.RegisterAsync(request);

                if (!response)
                {
                    _logger.LogWarning($"AuthController:Register - Registration failed for {request.Email}");
                    return BadRequest(new { message = "Registration failed. Please try again." });
                }

                _logger.LogInformation($"AuthController:Register - Successful registration for {response}", request.Email);
                return Ok(response);
            }
            catch (ValidationException ex)
            {
                _logger.LogError($"Validation Error during RegisterUser: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuthController:Register - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetAllDepartment")]
        public async Task<IActionResult> GetAllDepartment()
        {

            try
            {
                _logger.LogInformation("Request for CommonController:GetAllDepartment method");

                var result = await _commonService.GetAllDepartment();

                if (result == null)
                {
                    _logger.LogInformation("Response for CommonController:GetAllDepartment method: No Unit found.");

                    return NotFound();
                }

                _logger.LogInformation($"Response for CommonController:GetAllDepartment method: {result}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetAllDepartment: {ex}");
                return BadRequest(ex);

            }


        }

        [Authorize]
        [HttpPost("UpdateDepartment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateDepartment([FromBody] UpdateDepartmentRequestDto request)
        {
            _logger.LogInformation("Request received for UpdateDepartment: Id={Id}, DepartmentName={DepartmentName}", request.Id, request.DepartmentName);
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request cannot be null or empty" });

                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.ModifiedBy = userId;

                var result = await _commonService.UpdateDepartmentAsync(request);
                if (!result)
                {
                    _logger.LogWarning("Department not found or failed to update: Id={Id}", request.Id);
                    return NotFound(new { message = "Department not found or failed to update." });
                }

                _logger.LogInformation("Successfully updated department: Id={Id}", request.Id);
                return Ok(new { message = "Department updated successfully." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Validation error in UpdateDepartment: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in UpdateDepartment: Id={Id}", request.Id);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        [Authorize]
        [HttpPost("DeleteDepartment/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            _logger.LogInformation("Request received for DeleteDepartment: Id={Id}", id);
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid department ID." });

                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                var result = await _commonService.DeleteDepartmentAsync(id, userId);

                if (!result)
                {
                    _logger.LogWarning("Department not found or already deleted: Id={Id}", id);
                    return NotFound(new { message = "Department not found or already deleted." });
                }

                _logger.LogInformation("Successfully deleted department: Id={Id}", id);
                return Ok(new { message = "Department deleted successfully." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Validation error in DeleteDepartment: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in DeleteDepartment: Id={Id}", id);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetUserRoles")]
        public async Task<IActionResult> GetUserRoles()
        {

            try
            {
                _logger.LogInformation("Request for CommonController:GetAllDepartment method");

                var result = await _commonService.GetUserRoles();

                if (result == null)
                {
                    _logger.LogInformation("Response for CommonController:GetUserRoles method: No Unit found.");

                    return NotFound();
                }

                _logger.LogInformation($"Response for CommonController:GetUserRoles method: {result}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetUserRoles: {ex}");
                return BadRequest(ex);

            }
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetAllPlants")]
        public async Task<IActionResult> GetAllPlants()
        {

            try
            {
                _logger.LogInformation("Request for CommonController:GetAllPlants method");

                var result = await _commonService.GetAllPlants();

                if (result == null)
                {
                    _logger.LogInformation("Response for CommonController:GetAllPlants method: No Unit found.");

                    return NotFound();
                }

                _logger.LogInformation($"Response for CommonController:GetAllPlants method: {result}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetAllPlants: {ex}");
                return BadRequest(ex);

            }
        }

            [HttpGet]
            [ProducesResponseType(StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [Route("GetSecurityQuestion")]
            public async Task<IActionResult> GetAllSecuriyQuestionAsync()
            {
                try
                {
                    _logger.LogInformation("Request for CommonController:GetAllModules method");

                    var result = await _commonService.GetAllSecurityQuestions();

                    if (result == null)
                    {
                        _logger.LogInformation($"Response for CommonController:GetAllModules method:No modules found.");

                        return NotFound();
                    }

                    var precheckModuleResponse = result.Adapt<List<GetSecurityQuestionInfo>>();

                    _logger.LogInformation($"Response for CommonController:GetAllModules method: {precheckModuleResponse}");
                    return Ok(precheckModuleResponse);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception Error for CommonController:GetAllModules: {ex}");
                    return BadRequest(ex);

                }
            }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("AddUserRole")]
        public async Task<IActionResult> AddUserRole([FromBody] UserRole role)
        {
            try
            {
                _logger.LogInformation("Request for AuthController:AddUserRole");
                if (role == null) return BadRequest("Invalid role data");

                var result = await _commonService.AddUserRole(role);
                _logger.LogInformation($"Role added successfully with ID: {result}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AddUserRole: {ex}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("UpdateUserRole")]
        public async Task<IActionResult> UpdateUserRole([FromBody] UserRole role)
        {
            try
            {
                _logger.LogInformation($"Request for AuthController:UpdateUserRole for ID {role.Id}");
                if (role == null) return BadRequest("Invalid role data");

                var result = await _commonService.UpdateUserRole(role);
                if (!result) return NotFound("Role not found or update failed");
                _logger.LogInformation($"Role updated successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateUserRole: {ex}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("DeleteUserRole/{id}")]
        public async Task<IActionResult> DeleteUserRole(int id)
        {
            try
            {
                _logger.LogInformation($"Request for AuthController:DeleteUserRole for ID {id}");
                // Assuming modifiedBy is 1 (System/Admin) for now, or extract from token
                var result = await _commonService.DeleteUserRole(id, 1);
                if (!result) return NotFound("Role not found or delete failed");
                _logger.LogInformation($"Role deleted successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteUserRole: {ex}");
                return BadRequest(ex.Message);
            }
        }
        }
    }
