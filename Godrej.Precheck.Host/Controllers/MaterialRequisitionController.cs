using Godrej.Precheck.Models.DTOs.MaterialRequisition;
using Godrej.Precheck.Service.Service.MaterialRequisitionService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Godrej.Precheck.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialRequisitionController : ControllerBase
    {
        private readonly ILogger<MaterialRequisitionController> _logger;
        private readonly IMaterialRequisitionService _materialRequisitionService;

        public MaterialRequisitionController(
            ILogger<MaterialRequisitionController> logger,
            IMaterialRequisitionService materialRequisitionService)
        {
            _logger = logger;
            _materialRequisitionService = materialRequisitionService;
        }

        [Authorize]
        [HttpGet("swapping-details")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetSwappingDetails()
        {
            _logger.LogInformation("Request received for MaterialRequisitionController:GetSwappingDetails");

            try
            {
                var response = await _materialRequisitionService.GetSwappingDetails();

                if (response == null || !response.Any())
                {
                    _logger.LogWarning("MaterialRequisitionController:GetSwappingDetails - No swapping details found");
                    return Ok(new List<object>());
                }

                _logger.LogInformation("MaterialRequisitionController:GetSwappingDetails - Successfully retrieved {Count} records", response.Count);
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "MaterialRequisitionController:GetSwappingDetails - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MaterialRequisitionController:GetSwappingDetails - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetMaterialRequisitions([FromQuery] string? status = null, [FromQuery] int statusid=0)
        {
            _logger.LogInformation("Request received for MaterialRequisitionController:GetMaterialRequisitions with status: {Status}", status ?? "all");

            try
            {
                List<Models.DataModel.MaterialRequisition.MaterialRequisitionResponse> response;
                
                if (!string.IsNullOrEmpty(status))
                {
                    response = await _materialRequisitionService.GetMaterialRequisitionsByStatus(status,statusid);
                }
                else
                {
                    response = await _materialRequisitionService.GetMaterialRequisitions();
                }

                if (response == null || !response.Any())
                {
                    _logger.LogWarning("MaterialRequisitionController:GetMaterialRequisitions - No material requisitions found");
                    return Ok(new List<object>());
                }

                _logger.LogInformation($"MaterialRequisitionController:GetMaterialRequisitions - Successfully retrieved {response.Count} records");
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "MaterialRequisitionController:GetMaterialRequisitions - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MaterialRequisitionController:GetMaterialRequisitions - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateMaterialRequisition([FromBody] CreateMaterialRequisitionRequestDto request)
        {
            _logger.LogInformation("Request received for MaterialRequisitionController:CreateMaterialRequisition");

            try
            {
                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                var result = await _materialRequisitionService.CreateMaterialRequisition(request, userId);

                _logger.LogInformation($"MaterialRequisitionController:CreateMaterialRequisition - Successfully created MaterialRequisitionId: {result.NewId}, RequestNumber: {result.RequestNumber}");
                return StatusCode(201, new
                {
                    message = "Material requisition created successfully.",
                    materialRequisitionId = result.NewId,
                    requestNumber = result.RequestNumber
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "MaterialRequisitionController:CreateMaterialRequisition - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MaterialRequisitionController:CreateMaterialRequisition - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("SwapComponents")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateSwappedDrawingNumber([FromBody] CreateSwappedDrawingNumberRequestDto request)
        {
            _logger.LogInformation("Request received for MaterialRequisitionController:CreateSwappedDrawingNumber");

            if (request == null)
            {
                return BadRequest(new { message = "Invalid request body." });
            }

            try
            {
                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                var result = await _materialRequisitionService.CreateSwappedDrawingNumber(request, userId);

                if (result <= 0)
                {
                    return BadRequest(new { message = "Failed to create swapped drawing number." });
                }
                 
                _logger.LogInformation(
                    "MaterialRequisitionController:CreateSwappedDrawingNumber - Successfully created for SwapTransactionID: {SwapTransactionID}"
                    );

                return StatusCode(201, new
                {
                    message = "Swapped drawing number created successfully."
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "MaterialRequisitionController:CreateSwappedDrawingNumber - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MaterialRequisitionController:CreateSwappedDrawingNumber - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateMaterialRequisition([FromBody] UpdateMaterialRequisitionRequestDto request)
        {
            _logger.LogInformation($"Request received for MaterialRequisitionController:UpdateMaterialRequisition for MaterialRequisitionId: {request.MaterialRequisitionId}");

            try
            {
                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                var response = await _materialRequisitionService.UpdateMaterialRequisition(request, userId);

                if (response <= 0)
                {
                    _logger.LogWarning($"MaterialRequisitionController:UpdateMaterialRequisition - Update failed for MaterialRequisitionId: {request.MaterialRequisitionId}");
                    return BadRequest(new { message = "Failed to update material requisition." });
                }

                _logger.LogInformation($"MaterialRequisitionController:UpdateMaterialRequisition - Successfully updated MaterialRequisitionId: {request.MaterialRequisitionId}");
                return Ok(new
                {
                    message = "Material requisition updated successfully.",
                    materialRequisitionId = request.MaterialRequisitionId
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "MaterialRequisitionController:UpdateMaterialRequisition - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MaterialRequisitionController:UpdateMaterialRequisition - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("canclerequest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CancelRequest([FromBody] CancelMaterialRequisitionRequestDto request)
        {
            _logger.LogInformation($"Request received for MaterialRequisitionController:CancelRequest for RequestId: {request.RequestId}");

            try
            {
                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                var response = await _materialRequisitionService.CancelMaterialRequisition(request, userId);

                if (response <= 0)
                {
                    _logger.LogWarning($"MaterialRequisitionController:CancelRequest - Cancel failed for RequestId: {request.RequestId}");
                    return BadRequest(new { message = "Failed to cancel material requisition." });
                }

                _logger.LogInformation($"MaterialRequisitionController:CancelRequest - Successfully cancelled RequestId: {request.RequestId}");
                return Ok(new
                {
                    message = "Material requisition cancelled successfully.",
                    requestId = request.RequestId
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "MaterialRequisitionController:CancelRequest - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MaterialRequisitionController:CancelRequest - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpGet("export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportMaterialRequisitionsToExcel()
        {
            _logger.LogInformation("Request received for MaterialRequisitionController:ExportMaterialRequisitionsToExcel");

            try
            {
                var materialRequisitions = await _materialRequisitionService.GetMaterialRequisitions();

                if (materialRequisitions == null || !materialRequisitions.Any())
                {
                    _logger.LogWarning("MaterialRequisitionController:ExportMaterialRequisitionsToExcel - No material requisitions found");
                    return NotFound("No material requisition data found.");
                }

                var excelContent = _materialRequisitionService.ExportToExcel(materialRequisitions);

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"MaterialRequisition_{timestamp}.xlsx";

                _logger.LogInformation($"MaterialRequisitionController:ExportMaterialRequisitionsToExcel - Successfully generated Excel file {fileName} with {materialRequisitions.Count} records");

                // Add headers to ensure proper file download
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

                return File(
                    excelContent,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "MaterialRequisitionController:ExportMaterialRequisitionsToExcel - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MaterialRequisitionController:ExportMaterialRequisitionsToExcel - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }
    }
}
