using Godrej.Precheck.Models.DTOs.DrawingNumber;
using Godrej.Precheck.Models.DTOs.Stage;
using Godrej.Precheck.Models.DTOs.User;
using Godrej.Precheck.Service.Service.CommonSevice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
namespace Godrej.Precheck.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly ICommonService _commonService;

        public UserController(ILogger<UserController> logger, ICommonService commonService)
        {
            _logger = logger;
            _commonService = commonService;
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("{UserId}")]
        public async Task<IActionResult> GetUserById([FromRoute] int UserId)
        {
            _logger.LogInformation("[GetUserById] Request received. UserId: {UserId}", UserId);

            try
            {
                var result = await _commonService.UserService(UserId);

                if (result == null)
                {
                    _logger.LogWarning("[GetUserById] No user found for UserId: {UserId}", UserId);
                    return NotFound(new { message = "User not found" });
                }

                _logger.LogInformation("[GetUserById] Response: {@Result}", result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetUserById] Exception occurred for UserId: {UserId}", UserId);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("searchUser")]
        public async Task<IActionResult> GetUserByName([FromQuery] string name)
        {
            _logger.LogInformation("[GetUserByName] Request received. Name: {Name}", name);

            try
            {
                var result = await _commonService.UserByNameService(name);

                if (result == null)
                {
                    _logger.LogWarning("[GetUserByName] No user found for Name: {Name}", name);
                    return NotFound(new { message = "User not found" });
                }

                _logger.LogInformation("[GetUserByName] Response: {@Result}", result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetUserByName] Exception occurred for Name: {Name}", name);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }


        [HttpPost("Update-ProdSeries")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProdSeries([FromBody] UpdateProdSeriesRequestDto request)
        {
            _logger.LogInformation("Request received for UpdateProdSeries");
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request cannot be null or empty" });
                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.ModifiedBy = userId;
                var result = await _commonService.UpdateProdSeriesAsync(request);
                if (!result)
                    return BadRequest(new { message = "Failed to update production series" });
                return Ok(new { message = "Production series updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateProdSeries");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [HttpPost("Delete-ProdSeries/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProdSeries(int id)
        {
            _logger.LogInformation("Request received for DeleteProdSeries Id: {Id}", id);
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid production series Id" });
                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                var result = await _commonService.DeleteProdSeriesAsync(id, userId);
                if (!result)
                    return BadRequest(new { message = "Failed to delete production series" });
                return Ok(new { message = "Production series deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteProdSeries Id: {Id}", id);
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation("[GetAllUsers] Request received");

            try
            {
                var result = await _commonService.GetAllUsersService();

                if (result == null || !result.Any())
                {
                    _logger.LogWarning("[GetAllUsers] No users found");
                    return Ok(new List<Godrej.Precheck.Models.DataModel.User>());
                }

                _logger.LogInformation("[GetAllUsers] Response: Retrieved {Count} users", result.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetAllUsers] Exception occurred");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetPendingUsers")]
        public async Task<IActionResult> GetPendingUsers()
        {
            _logger.LogInformation("[GetPendingUsers] Request received");

            try
            {
                var result = await _commonService.GetPendingUsersService();

                if (result == null || !result.Any())
                {
                    _logger.LogWarning("[GetPendingUsers] No pending users found");
                    return Ok(new List<Godrej.Precheck.Models.DataModel.User>());
                }

                _logger.LogInformation("[GetPendingUsers] Response: Retrieved {Count} users", result.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetPendingUsers] Exception occurred");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("ApproveUser/{id}")]
        public async Task<IActionResult> ApproveUser(int id)
        {
            _logger.LogInformation("[ApproveUser] Request received for UserId: {UserId}", id);

            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Valid User ID is required" });
                }

                var userIdStr = User.FindFirst("id")?.Value;
                int modifiedBy = 0;
                if (!string.IsNullOrEmpty(userIdStr))
                {
                    int.TryParse(userIdStr, out modifiedBy);
                }

                var result = await _commonService.ApproveUserService(id, modifiedBy);

                if (!result)
                {
                    _logger.LogWarning("[ApproveUser] Update failed for UserId: {UserId}", id);
                    return BadRequest(new { message = "Failed to approve user. User may not exist." });
                }

                _logger.LogInformation("[ApproveUser] Successfully approved UserId: {UserId}", id);
                return Ok(new { message = "User approved successfully", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ApproveUser] Exception occurred for UserId: {UserId}", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [Authorize]
        [HttpPost("AddUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AddUser([FromBody] AddUserRequestDto request)
        {
            _logger.LogInformation("Request received for AddUser: {UserName}", request.UserName);
            try
            {
                var createdBy = Convert.ToInt32(User.FindFirst("id")?.Value);
                var response = await _commonService.AddUserAsync(request, createdBy);
                if (response == null)
                {
                    _logger.LogWarning("AddUser failed for user: {UserName}", request.UserName);
                    return BadRequest(new { message = "Failed to add user." });
                }
                _logger.LogInformation("AddUser successful for user: {UserName}", request.UserName);
                return Ok(new
                {
                    statusCode = StatusCodes.Status200OK,
                    message = "User added successfully.",
                    data = response
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "AddUser business error for user: {UserName}", request.UserName);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in AddUser for user: {UserName}", request.UserName);
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto request)
        {
            _logger.LogInformation("[UpdateUser] Request received for UserId: {UserId}", request?.Id);

            try
            {
                if (request == null || request.Id <= 0)
                {
                    _logger.LogWarning("[UpdateUser] Invalid request - UserId is required");
                    return BadRequest(new { message = "User ID is required" });
                }

                var result = await _commonService.UpdateUserService(request);

                if (!result)
                {
                    _logger.LogWarning("[UpdateUser] Update failed for UserId: {UserId}", request.Id);
                    return BadRequest(new { message = "Failed to update user. User may not exist or is inactive." });
                }

                _logger.LogInformation("[UpdateUser] Successfully updated UserId: {UserId}", request.Id);
                return Ok(new { message = "User updated successfully", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateUser] Exception occurred for UserId: {UserId}", request?.Id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("UpdateUserStatus")]
        public async Task<IActionResult> UpdateUserStatus([FromBody] UserStatusUpdateDto request)
        {
            _logger.LogInformation("[UpdateUserStatus] Request received for UserId: {UserId}", request?.Id);

            try
            {
                if (request == null || request.Id <= 0)
                {
                    _logger.LogWarning("[UpdateUserStatus] Invalid request - UserId is required");
                    return BadRequest(new { message = "User ID is required" });
                }

                var userIdStr = User.FindFirst("id")?.Value;
                if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int modifiedBy))
                {
                    request.ModifiedBy = modifiedBy;
                }

                var result = await _commonService.UpdateUserStatusAsync(request);

                if (!result)
                {
                    _logger.LogWarning("[UpdateUserStatus] Update failed for UserId: {UserId}", request.Id);
                    return BadRequest(new { message = "Failed to update user status. User may not exist." });
                }

                _logger.LogInformation("[UpdateUserStatus] Successfully updated UserId: {UserId}", request.Id);
                return Ok(new { message = "User status updated successfully", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateUserStatus] Exception occurred for UserId: {UserId}", request?.Id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("Page-Role-Access")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllPageRoleAccess([FromQuery] int roleId)
        {
            _logger.LogInformation("Request received for GetAllPageRoleAccess");
            try
            {
                var result = await _commonService.GetAllPageRoleAccessAsync(roleId);
                if (result == null || !result.Any())
                    return NotFound(new { message = "No page role access records found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllPageRoleAccess");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [HttpPost("Update-access")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdatePageRoleAccess([FromBody] List<PageRoleAccessUpdateDto> request)
        {
            _logger.LogInformation("Request received for UpdatePageRoleAccess with {Count} records", request?.Count);
            try
            {
                if (request == null || !request.Any())
                    return BadRequest(new { message = "Request cannot be null or empty" });

                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.ForEach(r => r.ModifiedBy = userId);

                var result = await _commonService.UpdatePageRoleAccessAsync(request);
                return Ok(new { message = $"Successfully updated {result} page role access records" });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error in UpdatePageRoleAccess: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdatePageRoleAccess");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Add new department - Accessible to Admin only
        /// Creates a new department with the provided name
        /// </summary>
        [Authorize]
        [HttpPost("AddDepartment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddDepartment([FromBody] AddDepartmentRequestDto request)
        {
            _logger.LogInformation($"Request received for AddDepartment: DepartmentName={request.DepartmentName}");

            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request cannot be null or empty" });

                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.CreatedBy = userId;

                var result = await _commonService.AddDepartmentAsync(request);

                if (!result)
                {
                    _logger.LogWarning($"Failed to add department: {request.DepartmentName}");
                    return BadRequest(new { message = "Failed to add department." });
                }

                _logger.LogInformation($"Successfully added department: {request.DepartmentName}");
                return Ok(new { message = "Department added successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in AddDepartment for DepartmentName: {request.DepartmentName}");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Add new unit - Accessible to authenticated users
        /// Creates a new unit with the provided name
        /// </summary>
        [Authorize]
        [HttpPost("AddUnit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddUnit([FromBody] AddUnitRequestDto request)
        {
            _logger.LogInformation("Request received for AddUnit: UnitName={UnitName}", request.UnitName);

            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request cannot be null or empty" });

                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.CreatedBy = userId;

                var result = await _commonService.AddUnitAsync(request);

                if (!result)
                {
                    _logger.LogWarning("Failed to add unit: {UnitName}", request.UnitName);
                    return BadRequest(new { message = "Failed to add unit." });
                }

                _logger.LogInformation("Successfully added unit: {UnitName}", request.UnitName);
                return Ok(new { message = "Unit added successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in AddUnit for UnitName: {UnitName}", request.UnitName);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpPost("Add-Shape")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddShape([FromBody] AddShapeDto request)
        {
            _logger.LogInformation("Request received for AddShape");
            try
            {

                if (request == null)
                    return BadRequest(new { message = "Request cannot be null or empty" });

                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.CreatedBy = userId;

                var result = await _commonService.AddShapeAsync(request);

                if (result == null)
                    return BadRequest(new { message = "Failed to add shape" });

                return Ok(new { message = "Shape added successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddShape");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [HttpPost("Add-Stage")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddStage([FromBody] AddStageRequestDto request)
        {
            _logger.LogInformation("Request received for AddShape");
            try
            {

                if (request == null)
                    return BadRequest(new { message = "Request cannot be null or empty" });

                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.CreatedBy = userId;

                var result = await _commonService.AddStageAsync(request);

                if (result == null)
                    return BadRequest(new { message = "Failed to add stage" });

                return Ok(new { message = "Stage added successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddStage");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [HttpPost("Add-ProdSeries")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddProdSeries([FromBody] AddProdSeriesRequestDto request)
        {
            _logger.LogInformation("Request received for AddProdSeries");
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request cannot be null or empty" });
                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.CreatedBy = userId;
                var result = await _commonService.AddProdSeriesAsync(request);
                if (result == null)
                    return BadRequest(new { message = "Failed to add production series" });
                return Ok(new { message = "Production series added successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddProdSeries");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [Authorize]
        [HttpPost("Update-Unit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUnit([FromBody] UpdateUnitRequestDto request)
        {
            _logger.LogInformation("Request received for UpdateUnit: Id={Id}, UnitName={UnitName}", request.Id, request.UnitName);
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request cannot be null or empty" });

                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.ModifiedBy = userId;

                var result = await _commonService.UpdateUnitAsync(request);
                if (!result)
                {
                    _logger.LogWarning("Unit not found or failed to update: Id={Id}", request.Id);
                    return NotFound(new { message = "Unit not found or failed to update." });
                }

                _logger.LogInformation("Successfully updated unit: Id={Id}", request.Id);
                return Ok(new { message = "Unit updated successfully." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Validation error in UpdateUnit: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in UpdateUnit: Id={Id}", request.Id);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        [Authorize]
        [HttpPost("Update-Shape")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateShape([FromBody] UpdateShapeRequestDto request)
        {
            _logger.LogInformation("Request received for UpdateShape: Id={Id}", request.Id);
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request cannot be null or empty" });

                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.ModifiedBy = userId;

                var result = await _commonService.UpdateShapeAsync(request);
                if (!result)
                {
                    _logger.LogWarning("Shape not found or failed to update: Id={Id}", request.Id);
                    return NotFound(new { message = "Shape not found or failed to update." });
                }

                _logger.LogInformation("Successfully updated shape: Id={Id}", request.Id);
                return Ok(new { message = "Shape updated successfully." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Validation error in UpdateShape: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in UpdateShape: Id={Id}", request.Id);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        [Authorize]
        [HttpPost("Update-Stage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateStage([FromBody] UpdateStageRequestDto request)
        {
            _logger.LogInformation("Request received for UpdateStage: Id={Id}", request.Id);
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request cannot be null or empty" });

                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.ModifiedBy = userId;

                var result = await _commonService.UpdateStageAsync(request);
                if (!result)
                {
                    _logger.LogWarning("Stage not found or failed to update: Id={Id}", request.Id);
                    return NotFound(new { message = "Stage not found or failed to update." });
                }

                _logger.LogInformation("Successfully updated stage: Id={Id}", request.Id);
                return Ok(new { message = "Stage updated successfully." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Validation error in UpdateStage: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in UpdateStage: Id={Id}", request.Id);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        //[Authorize]
        [HttpPost("Delete-Unit/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUnit(int id)
        {
            _logger.LogInformation("Request received for DeleteUnit: Id={Id}", id);
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid unit ID." });

                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                var result = await _commonService.DeleteUnitAsync(id, userId);

                if (!result)
                {
                    _logger.LogWarning("Unit not found or already deleted: Id={Id}", id);
                    return NotFound(new { message = "Unit not found or already deleted." });
                }

                _logger.LogInformation("Successfully deleted unit: Id={Id}", id);
                return Ok(new { message = "Unit deleted successfully." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Validation error in DeleteUnit: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in DeleteUnit: Id={Id}", id);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        //[Authorize]
        [HttpPost("Delete-Shape/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteShape(int id)
        {
            _logger.LogInformation("Request received for DeleteShape: Id={Id}", id);
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid shape ID." });

                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                var result = await _commonService.DeleteShapeAsync(id, userId);

                if (!result)
                {
                    _logger.LogWarning("Shape not found or already deleted: Id={Id}", id);
                    return NotFound(new { message = "Shape not found or already deleted." });
                }

                _logger.LogInformation("Successfully deleted shape: Id={Id}", id);
                return Ok(new { message = "Shape deleted successfully." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Validation error in DeleteShape: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in DeleteShape: Id={Id}", id);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        //[Authorize]
        [HttpPost("Delete-Stage/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteStage(int id)
        {
            _logger.LogInformation("Request received for DeleteStage: Id={Id}", id);
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid stage ID." });

                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                var result = await _commonService.DeleteStageAsync(id, userId);

                if (!result)
                {
                    _logger.LogWarning("Stage not found or already deleted: Id={Id}", id);
                    return NotFound(new { message = "Stage not found or already deleted." });
                }

                _logger.LogInformation("Successfully deleted stage: Id={Id}", id);
                return Ok(new { message = "Stage deleted successfully." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Validation error in DeleteStage: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in DeleteStage: Id={Id}", id);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }
        [HttpPost("Upload-Signature")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadSignature([FromBody] UploadSignatureRequestDto request)
        {
            _logger.LogInformation("Request received for UploadSignature. UserId: {UserId}", request?.UserId);
            try
            {

                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.ModifiedBy = userId;

                if (request == null)
                    return BadRequest(new { message = "Request cannot be null or empty." });

                if (request.UserId <= 0)
                    return BadRequest(new { message = "Valid UserId is required." });

                if (string.IsNullOrWhiteSpace(request.Signature))
                    return BadRequest(new { message = "Signature cannot be null or empty." });

                var result = await _commonService.UploadUserSignatureAsync(request);

                if (!result)
                {
                    _logger.LogWarning("UploadSignature failed for UserId: {UserId}", request.UserId);
                    return BadRequest(new { message = "Failed to upload signature." });
                }

                _logger.LogInformation("Signature uploaded successfully for UserId: {UserId}", request.UserId);
                return Ok(new { message = "Signature uploaded successfully.", success = true });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Validation error in UploadSignature: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in UploadSignature for UserId: {UserId}", request?.UserId);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }
        [HttpGet("GetMySignature")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMySignature()
        {
            _logger.LogInformation("Request received for GetMySignature");
            try
            {
                var userIdStr = User.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId) || userId <= 0)
                {
                    _logger.LogWarning("[GetMySignature] Unable to resolve UserId from token.");
                    return BadRequest(new { message = "Invalid or missing user identity in token." });
                }

                var signature = await _commonService.GetUserSignatureAsync(userId);

                _logger.LogInformation("[GetMySignature] Signature {Status} for UserId: {UserId}",
                    signature != null ? "found" : "not found", userId);

                return Ok(new
                {
                    userId = userId,
                    signature = signature ?? string.Empty  // empty string when no signature exists
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("[GetMySignature] Validation error: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetMySignature] Unexpected error");
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }
        [HttpGet("GetUsersWithSignatures")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsersWithSignatures()
        {
            _logger.LogInformation("[GetUsersWithSignatures] Request received");
            try
            {
                var result = await _commonService.GetUsersWithSignaturesAsync();
                _logger.LogInformation("[GetUsersWithSignatures] Returning {Count} records", result?.Count() ?? 0);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetUsersWithSignatures] Unexpected error");
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }
    }
}

