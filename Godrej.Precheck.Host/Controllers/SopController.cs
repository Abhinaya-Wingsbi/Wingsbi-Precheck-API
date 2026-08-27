using Godrej.Precheck.Models.DTOs.Precheck;
using Godrej.Precheck.Models.DTOs.Sop;
using Godrej.Precheck.Models.DTOs.Bom;
using Godrej.Precheck.Service.Service.CommonSevice;
using Godrej.Precheck.Service.Service.SopService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Godrej.Precheck.Host.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class SopController : ControllerBase
    {
        private readonly ILogger<SopController> _logger;
        private readonly ISopService _sopService;
        private readonly ICommonService _commonService;

        public SopController(ILogger<SopController> logger, ISopService sopService, ICommonService commonService)
        {
            _logger = logger;
            _sopService = sopService;
            _commonService = commonService;
        }

        [Authorize]
        // GET api/<SopController>/5
        [HttpGet("allassemblies")]
        public async Task<IActionResult> GetAllModulesAsync()
        {
            try
            {
                _logger.LogInformation("Request for SopController:GetAllModules method");

                var result = await _sopService.GetAllAssembly();

                if (result == null)
                {
                    _logger.LogInformation("Response for CommonController:GetAllModules method:No modules found.");

                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetAllModules: {ex}");
                return BadRequest(ex);

            }
        }

        [Authorize]
        // GET api/<SopController>/5
        [HttpPost("GetSop")]
        public async Task<IActionResult> GetSopForAssembly([FromBody] GetSopRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid request parameters." });
            }

            _logger.LogInformation("Request received for SopController:GetSopForAssembly");

            try
            {
                var response = await _sopService.GetSopForAssembly(request);

                if (response == null)
                {
                    _logger.LogWarning("SopController:GetSopForAssembly - No SOP details found");
                    return NotFound(new { message = "No SOP details found for the given criteria." });
                }

                _logger.LogInformation($"SopController:GetSopForAssembly - Successfully retrieved {response.Count} SOP details");
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "SopController:GetSopForAssembly - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SopController:GetSopForAssembly - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("GetSopExcludingRawMaterial")]
        public async Task<IActionResult> GetSopForAssemblyExcludingRawMaterial([FromBody] GetSopRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid request parameters." });
            }

            _logger.LogInformation("Request received for SopController:GetSopForAssemblyExcludingRawMaterial");

            try
            {
                var response = await _sopService.GetSopForAssembly(request, excludeRawMaterial: true);

                if (response == null)
                {
                    _logger.LogWarning("SopController:GetSopForAssemblyExcludingRawMaterial - No SOP details found");
                    return NotFound(new { message = "No SOP details found for the given criteria." });
                }

                _logger.LogInformation($"SopController:GetSopForAssemblyExcludingRawMaterial - Successfully retrieved {response.Count} SOP details");
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "SopController:GetSopForAssemblyExcludingRawMaterial - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SopController:GetSopForAssemblyExcludingRawMaterial - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost]
        [Route("exportSop")]
        public async Task<IActionResult> ExportSopForAssemblyAsync([FromBody] GetSopRequestDto request)
        {
            _logger.LogInformation("Request received for PrecheckController:ExportSopForAssembly");

            try
            {
               var drawingNumbers =await _commonService.GetAllDrawingNumberService();
                var AssemblyDrawing = drawingNumbers.Where(x => x.Id == request.AssemblyDrawingId).First().AssemblyNumber;
                var sopResponse = await _sopService.GetSopForAssembly(request);
                var response = _sopService.ExportToExcel(sopResponse, request.AssemblyDrawing);

                var timestamp = DateTime.Now.ToString("dd-MM-yy HH mm ss");
                var fileName = $"{AssemblyDrawing}_{request.SerielNumberId}_{timestamp}.xlsx";

                _logger.LogInformation($"PrecheckController:ExportSopForAssembly - Successfully generated Excel file {fileName}");

                // Add these headers to ensure proper file download
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

                return File(
                    response,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:ExportSopForAssembly - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:ExportSopForAssembly - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost]
        [Route("exportSopExcludingRawMaterial")]
        public async Task<IActionResult> ExportSopForAssemblyExcludingRawMaterialAsync([FromBody] GetSopRequestDto request)
        {
            _logger.LogInformation("Request received for PrecheckController:ExportSopForAssemblyExcludingRawMaterial");

            try
            {
                var drawingNumbers = await _commonService.GetAllDrawingNumberService();
                var AssemblyDrawing = drawingNumbers.Where(x => x.Id == request.AssemblyDrawingId).First().AssemblyNumber;
                var sopResponse = await _sopService.GetSopForAssembly(request, excludeRawMaterial: true);
                var response = _sopService.ExportToExcel(sopResponse, request.AssemblyDrawing);

                var timestamp = DateTime.Now.ToString("dd-MM-yy HH mm ss");
                var fileName = $"{AssemblyDrawing}_{request.SerielNumberId}_{timestamp}.xlsx";

                _logger.LogInformation($"PrecheckController:ExportSopForAssemblyExcludingRawMaterial - Successfully generated Excel file {fileName}");

                // Add these headers to ensure proper file download
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

                return File(
                    response,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:ExportSopForAssemblyExcludingRawMaterial - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:ExportSopForAssemblyExcludingRawMaterial - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        #region BOM Details Endpoints

        /// <summary>
        /// Get BOM details by assembly number.
        /// Returns recursive tree structure of all child components.
        /// </summary>
        [Authorize]
        [HttpGet("GetBomDetails")]
        public async Task<IActionResult> GetBomDetails([FromQuery] string assemblyNumber)
        {
            _logger.LogInformation($"SopController:GetBomDetails - Request for assembly: {assemblyNumber}");

            try
            {
                if (string.IsNullOrWhiteSpace(assemblyNumber))
                {
                    return BadRequest(new { message = "Assembly number is required." });
                }

                var response = await _sopService.GetBomDetails(assemblyNumber);

                if (response == null || !response.Any())
                {
                    _logger.LogWarning($"SopController:GetBomDetails - No BOM found for assembly: {assemblyNumber}");
                    return Ok(new List<BomDetailsResponseDto>());
                }

                _logger.LogInformation($"SopController:GetBomDetails - Found {response.Count} items for assembly: {assemblyNumber}");
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "SopController:GetBomDetails - Invalid argument");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SopController:GetBomDetails - Unexpected error occurred");
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        /// <summary>
        /// Search for assembly numbers by partial match.
        /// Returns list of assemblies for autocomplete.
        /// </summary>
        [Authorize]
        [HttpGet("SearchAssembly")]
        public async Task<IActionResult> SearchAssemblyNumbers([FromQuery] string searchText)
        {
            _logger.LogInformation($"SopController:SearchAssemblyNumbers - Searching for: {searchText}");

            try
            {
                var response = await _sopService.SearchAssemblyNumbers(searchText ?? "");

                _logger.LogInformation($"SopController:SearchAssemblyNumbers - Found {response?.Count ?? 0} assemblies");
                return Ok(response ?? new List<AssemblySearchResponseDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SopController:SearchAssemblyNumbers - Unexpected error occurred");
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        /// <summary>
        /// Export BOM details to Excel file.
        /// </summary>
        [Authorize]
        [HttpGet("ExportBom")]
        public async Task<IActionResult> ExportBomAsync([FromQuery] string assemblyNumber)
        {
            _logger.LogInformation($"SopController:ExportBom - Request for assembly: {assemblyNumber}");

            try
            {
                if (string.IsNullOrWhiteSpace(assemblyNumber))
                {
                    return BadRequest(new { message = "Assembly number is required." });
                }

                var bomData = await _sopService.GetBomDetails(assemblyNumber);
                var response = _sopService.ExportBomToExcel(bomData, assemblyNumber);

                var timestamp = DateTime.Now.ToString("dd-MM-yy_HH-mm-ss");
                var fileName = $"BOM_{assemblyNumber}_{timestamp}.xlsx";

                _logger.LogInformation($"SopController:ExportBom - Successfully generated Excel file {fileName}");

                // Add these headers to ensure proper file download
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

                return File(
                    response,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "SopController:ExportBom - Invalid argument");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SopController:ExportBom - Unexpected error occurred");
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        #endregion
    }

}


