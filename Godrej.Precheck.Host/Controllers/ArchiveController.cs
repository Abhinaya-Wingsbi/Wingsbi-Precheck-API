using System.ComponentModel.DataAnnotations;
using Godrej.Precheck.Models.DTOs.Archive;
using Godrej.Precheck.Service.Service.ArchiveService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Godrej.Precheck.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class ArchiveController : ControllerBase
    {
        private readonly IBackupArchiveService _backupArchiveService;
        private readonly ILogger<ArchiveController> _logger;

        public ArchiveController(
            IBackupArchiveService backupArchiveService,
            ILogger<ArchiveController> logger)
        {
            _backupArchiveService = backupArchiveService;
            _logger = logger;
        }

        /// <summary>
        /// Archive Search - Find which drawing numbers are consumed in assembly
        /// Search by ConsumedIn pattern like "D/K324-0000-000CB/321"
        /// D = Production Series, K324-0000-000CB = Assembly/Drawing Number, 321 = Component ID
        /// </summary>
        /// <param name="request">Search request with production series, assembly, and component ID</param>
        /// <returns>List of drawing numbers consumed in the specified assembly</returns>
        [Authorize]
        [HttpPost("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchArchive([FromBody] ArchiveSearchRequest request)
        {
            try
            {
                _logger.LogInformation($"Archive search - ProductionSeries: {request.ProductionSeries}, DrawingNumber: {request.DrawingNumber}, IdNumber: {request.IdNumber}");

                var result = await _backupArchiveService.SearchByConsumedInAsync(request.ProductionSeries, request.DrawingNumber, request.IdNumber);

                if (result == null || !result.Any())
                {
                    return NotFound(new
                    {
                        success = false,
                        data = new List<object>(),
                        totalRecords = 0,
                        message = "No drawing numbers found consuming in the specified assembly"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = result,
                    totalRecords = result.Count(),
                    message = $"Found {result.Count()} drawing numbers consumed in assembly {request.ProductionSeries}/{request.DrawingNumber}/{request.IdNumber}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in archive search - ProductionSeries: {request.ProductionSeries}, DrawingNumber: {request.DrawingNumber}, IdNumber: {request.IdNumber}");
                return StatusCode(500, new
                {
                    success = false,
                    data = new List<object>(),
                    totalRecords = 0,
                    message = "Internal server error while searching archive data"
                });
            }
        }
    }
}
