using Godrej.Precheck.Models.DTOs.DrawingNumber;
using Godrej.Precheck.Service.Service.DrawingNumberService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Godrej.Precheck.Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DrawingNumberController : ControllerBase
    {
        private readonly ILogger<DrawingNumberController> _logger;
        private readonly IDrawingNumberService _drawingNumberService;

        public DrawingNumberController(
            ILogger<DrawingNumberController> logger,
            IDrawingNumberService drawingNumberService)
        {
            _logger = logger;
            _drawingNumberService = drawingNumberService;
        }

        /// <summary>
        /// Get drawing number mappings - View accessible to all authenticated users
        /// Returns all mappings (LnItemCode, Nomenclature, RackLocation, ComponentType, DocumentType, Unit) for a drawing number
        /// </summary>
        [Authorize]
        [HttpGet("GetDrawingMappings/{drawingNumberId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetDrawingMappingDto>> GetDrawingMappings(int drawingNumberId)
        {
            _logger.LogInformation($"Request received for GetDrawingMappings: DrawingNumberId={drawingNumberId}");

            try
            {
                // Get drawing number details which includes all mappings
                var drawingNumber = await _drawingNumberService.GetDrawingMappingsAsync(drawingNumberId);

                if (drawingNumber == null)
                {
                    _logger.LogWarning($"Drawing number not found for ID: {drawingNumberId}");
                    return NotFound($"Drawing number with ID {drawingNumberId} not found.");
                }

                _logger.LogInformation($"Successfully retrieved mappings for DrawingNumberId: {drawingNumberId}");
                return Ok(drawingNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in GetDrawingMappings for DrawingNumberId: {drawingNumberId}");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Insert or update drawing number mappings - Admin only
        /// </summary>
        [Authorize]
        [HttpPost("InsertDrawingMappings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DrawingMappingResponseDto>> InsertDrawingMappings(
            [FromBody] InsertDrawingMappingDto request)
         {
            _logger.LogInformation($"InsertDrawingMappings for DrawingNumberId={request.DrawingNumberId}");

            try
            {
                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.CreatedBy = userId;

                var response = await _drawingNumberService.InsertDrawingMappingsAsync(request);

                if (!response.Success)
                {
                    _logger.LogWarning($"InsertDrawingMappings failed for DrawingNumberId {request.DrawingNumberId}: {response.Message}");
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in InsertDrawingMappings for DrawingNumberId={request.DrawingNumberId}");
                return StatusCode(500, new DrawingMappingResponseDto
                {
                    DrawingNumberId = request.DrawingNumberId ?? 0,
                    Success = false,
                    Message = "An unexpected error occurred. Please try again later.",
                    Details = new MappingDetails()
                });
            }
        }


    }
}

