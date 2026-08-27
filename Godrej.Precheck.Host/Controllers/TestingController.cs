using Godrej.Precheck.Models.DTOs.Testing;
using Godrej.Precheck.Service.Service.TestingService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Godrej.Precheck.Host.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TestingController : ControllerBase
    {
        private readonly ILogger<TestingController> _logger;
        private readonly ITestingService _testingService;

        public TestingController(ILogger<TestingController> logger, ITestingService testingService)
        {
            _logger = logger;
            _testingService = testingService;
        }

        [HttpGet("GetTemplateFields")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTemplateFieldsByDrawingNumber([FromQuery] GetTemplateFieldsByDrawingNumberRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.DrawingNumber))
            {
                return BadRequest(new { message = "drawingNumber is required." });
            }

            try
            {
                _logger.LogInformation("Request received for template fields. DrawingNumber: {DrawingNumber}", request.DrawingNumber);

                var result = await _testingService.GetTemplateFieldsByDrawingNumberAsync(request.DrawingNumber, request.MsnNumber, request.MsnQuantity, request.StageId);

                if (result.HeaderFields.Count == 0 && result.ColumnGroups.Count == 0)
                {
                    _logger.LogWarning("No template fields found for DrawingNumber: {DrawingNumber}", request.DrawingNumber);
                    return NotFound(new { message = $"No template fields found for drawing number '{request.DrawingNumber}'." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting template fields for DrawingNumber: {DrawingNumber}", request.DrawingNumber);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpPost("InsertInspectionValues")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> InsertInspectionValues(
       [FromBody] InsertInspectionValuesRequestDto request)
        {
            if (!ModelState.IsValid || request == null)
                return BadRequest(new { message = "Valid payload is required." });

            try
            {
                _logger.LogInformation(
                    "Request received for TemplateId: {TemplateId}, DrawingNumber: {DrawingNumber}"
                    , request.DrawingNumber);

                var response = await _testingService.InsertInspectionValuesAsync(request);
                return Ok(response);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex,
                    "Validation failed for TemplateId: {TemplateId}, DrawingNumber: {DrawingNumber}",
                    request.DrawingNumber);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error for TemplateId: {TemplateId}, DrawingNumber: {DrawingNumber}",
                    request.DrawingNumber);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        [Authorize]
        [HttpGet("GetPrecheckCompletedComponents")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPrecheckCompletedComponents()
        {
            try
            {
                _logger.LogInformation("Request received to get all precheck completed components.");

                var result = await _testingService.GetPrecheckCompletedComponentsAsync();

                if (result.Count == 0)
                {
                    _logger.LogWarning("No precheck completed components found.");
                    return NotFound(new { message = "No components with completed precheck found." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting precheck completed components.");
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpGet("GetDrawingStageStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDrawingStageStatus()
        {
            try
            {
                _logger.LogInformation("GetDrawingStageStatus called.");

                var result = await _testingService.GetDrawingStageStatusAsync();

                if (result.Count == 0)
                    return NotFound(new { message = "No inspection records found." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetDrawingStageStatus.");
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpPost("SaveStageData")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SaveStageData([FromBody] SaveStageDataRequestDto request)
        {
            if (!ModelState.IsValid || request == null)
                return BadRequest(new { message = "Valid payload is required." });

            try
            {
                _logger.LogInformation(
                    "SaveStageData called for DrawingNumber: {DrawingNumber}, StageId: {StageId}",
                    request.DrawingNumber, request.StageId);

                var result = await _testingService.SaveStageDataAsync(request);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex,
                    "Validation failed in SaveStageData for DrawingNumber: {DrawingNumber}",
                    request.DrawingNumber);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error in SaveStageData for DrawingNumber: {DrawingNumber}",
                    request.DrawingNumber);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpGet("GetStageData")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetStageData(
            [FromQuery] string drawingNumber,
            [FromQuery] string msnNumber,
            [FromQuery] int stageId)
        {
            if (string.IsNullOrWhiteSpace(drawingNumber))
                return BadRequest(new { message = "drawingNumber is required." });

            if (string.IsNullOrWhiteSpace(msnNumber))
                return BadRequest(new { message = "msnNumber is required." });

            if (stageId < 1 || stageId > 3)
                return BadRequest(new { message = "stageId must be 1, 2, or 3." });

            try
            {
                _logger.LogInformation(
                    "GetStageData called for DrawingNumber: {DrawingNumber}, MsnNumber: {MsnNumber}, StageId: {StageId}",
                    drawingNumber, msnNumber, stageId);

                var result = await _testingService.GetStageDataAsync(drawingNumber, msnNumber, stageId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error in GetStageData for DrawingNumber: {DrawingNumber}",
                    drawingNumber);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpGet("ExportInspectionPdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportInspectionPdf(
            [FromQuery] string drawingNumber,
            [FromQuery] string msnNumber,
            [FromQuery] int msnQuantity = 4)
        {
            if (string.IsNullOrWhiteSpace(drawingNumber))
                return BadRequest(new { message = "DrawingNumber is required." });

            if (string.IsNullOrWhiteSpace(msnNumber))
                return BadRequest(new { message = "msnNumber is required." });

            if (msnQuantity < 1)
                return BadRequest(new { message = "msnQuantity must be at least 1." });

            try
            {
                _logger.LogInformation(
                    "Export PDF request for DrawingNumber: {DrawingNumber}, MsnNumber: {MsnNumber}, MsnQuantity: {MsnQuantity}",
                    drawingNumber, msnNumber, msnQuantity);

                var pdfBytes = await _testingService.ExportInspectionAsPdfAsync(drawingNumber, msnNumber, msnQuantity);

                var fileName = $"Inspection_{drawingNumber}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed for export: {DrawingNumber}", drawingNumber);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error exporting PDF for: {DrawingNumber}", drawingNumber);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }


        [HttpGet("ExportDebug")]
        public async Task<IActionResult> ExportDebug([FromQuery] string drawingNumber)
        {
            if (string.IsNullOrWhiteSpace(drawingNumber))
                return BadRequest(new { message = "DrawingNumber is required." });

            var result = await _testingService.GetExportDebugDataAsync(drawingNumber);
            return Ok(result);
        }

        [HttpGet("GetTemplateHtml")]
        public async Task<IActionResult> GetTemplateHtml([FromQuery] string drawingNumber)
        {
            if (string.IsNullOrWhiteSpace(drawingNumber))
                return BadRequest(new { message = "DrawingNumber is required." });

            var html = await _testingService.GetRawTemplateHtmlAsync(drawingNumber);
            if (html == null)
                return NotFound(new { message = $"No template found for drawing number '{drawingNumber}'." });

            return Content(html, "text/html");
        }

        [HttpGet("GetFieldNames")]
        public async Task<IActionResult> GetFieldNames([FromQuery] string drawingNumber)
        {
            if (string.IsNullOrWhiteSpace(drawingNumber))
                return BadRequest(new { message = "DrawingNumber is required." });

            var result = await _testingService.GetFieldNamesForExportAsync(drawingNumber);
            return Ok(result);
        }
    }
}
