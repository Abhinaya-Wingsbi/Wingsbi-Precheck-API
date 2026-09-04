using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DTOs.Barcode;
using Godrej.Precheck.Models.DTOs.Precheck;
using Godrej.Precheck.Models.DTOs.QRCodeDetails;
using Godrej.Precheck.Service.Service.QRCodeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace QRCodeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QRCodeController : ControllerBase
    {
        private readonly ILogger<QRCodeController> _logger;
        private readonly IQRCodeService _qrCodeService;

        public QRCodeController(ILogger<QRCodeController> logger, IQRCodeService qrCodeService)
        {
            _logger = logger;
            _qrCodeService = qrCodeService;
        }

        /// <summary>
        /// Sanitizes a string to be used safely in a filename by replacing invalid characters
        /// </summary>
        private string SanitizeFilename(string input, string defaultValue = "Unknown")
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return defaultValue;
            }

            return input.Replace("/", "_")
                       .Replace("\\", "_")
                       .Replace(":", "_")
                       .Replace("*", "_")
                       .Replace("?", "_")
                       .Replace("\"", "_")
                       .Replace("<", "_")
                       .Replace(">", "_")
                       .Replace("|", "_");
        }

        private static List<QRCodeDetailsResponseDto> OrderByLatestCreated(IEnumerable<QRCodeDetailsResponseDto> qrCodes)
        {
            return qrCodes?
                .OrderByDescending(q => q.CreatedDate ?? DateTime.MinValue)
                .ThenByDescending(q => q.ModifiedDate ?? DateTime.MinValue)
                .ToList() ?? new List<QRCodeDetailsResponseDto>();
        }

        private static List<QRCodeDetailsResponseDto> OrderByLatestCreatedAscending(IEnumerable<QRCodeDetailsResponseDto> qrCodes)
        {
            return qrCodes?
                .OrderBy(q => q.CreatedDate ?? DateTime.MinValue)
                .ThenBy(q => q.ModifiedDate ?? DateTime.MinValue)
                .ToList() ?? new List<QRCodeDetailsResponseDto>();
        }

        private static List<StandardQRDetailsResponseDto> OrderByLatestCreatedStandard(IEnumerable<StandardQRDetailsResponseDto> qrCodes)
        {
            return qrCodes?
                .OrderByDescending(q => q.CreatedDate ?? DateTime.MinValue)
                .ThenByDescending(q => q.ModifiedDate ?? DateTime.MinValue)
                .ToList() ?? new List<StandardQRDetailsResponseDto>();
        }

        private static List<StandardQRDetailsResponseDto> OrderByLatestCreatedStandardAscending(IEnumerable<StandardQRDetailsResponseDto> qrCodes)
        {
            return qrCodes?
                .OrderBy(q => q.CreatedDate ?? DateTime.MinValue)
                .ThenBy(q => q.ModifiedDate ?? DateTime.MinValue)
                .ToList() ?? new List<StandardQRDetailsResponseDto>();
        }

        [Authorize]
        [HttpPost("GenerateQRCode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> InsertQRCodeDetails([FromBody] QRCodeDetailsDto request)
        {
            _logger.LogInformation($"Request received for InsertQRCodeDetails: {request}");

            try
            {
                var id = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.CreatedBy = id;

                var response = await _qrCodeService.InsertQRCodeDetailsAsync(request);

                if (response == null)
                {
                    _logger.LogWarning($"InsertQRCodeDetails failed for request: {request}");
                    return BadRequest(new { message = "Failed to insert QR code details." });
                }

                _logger.LogInformation($"InsertQRCodeDetails successful for request: {request}, response: {response}");
                return Ok(response);
            }
            catch (ValidationException ex)
            {
                _logger.LogError($"Validation Error for InsertQRCodeDetails: {ex.Message}");

                try
                {
                    var validationError = JsonSerializer.Deserialize<Validation.PrecheckValidationError>(ex.Message);
                    return BadRequest(validationError);
                }
                catch
                {
                    return BadRequest(new { message = ex.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in InsertQRCodeDetails for request: {request}");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }


        [Authorize]
        [HttpPost("UpdateQRCode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateQRCode([FromBody] UpdateQRCodeDto request)
        {
            _logger.LogInformation($"Request received for UpdateQRCode: {request.QRCodeNumber}");

            try
            {
                var id = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.ModifiedBy = id;

                var response = await _qrCodeService.UpdateQRCodeDetailsAsync(request);

                if (response == null)
                {
                    _logger.LogWarning($"UpdateQRCode failed for QR code: {request.QRCodeNumber}");
                    return BadRequest(new { message = "Failed to update QR code details." });
                }

                _logger.LogInformation($"UpdateQRCode successful for QR code: {request.QRCodeNumber}");
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "UpdateQRCode business error: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in UpdateQRCode for QR code: {request.QRCodeNumber}");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("DisableQRCode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> DisableQRCode([FromBody] DisableQRCodeRequestDto request)
        {
            _logger.LogInformation("Request received for DisableQRCode: {QRCodeNumber}", request.QRCodeNumber);

            try
            {
                var id = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.ModifiedBy = id;

                var response = await _qrCodeService.DisableQRCodeAsync(request);

                if (response == null)
                {
                    _logger.LogWarning("DisableQRCode failed for QR code: {QRCodeNumber}", request.QRCodeNumber);
                    return BadRequest(new { message = "Failed to disable QR code." });
                }

                _logger.LogInformation("DisableQRCode successful for QR code: {QRCodeNumber}", request.QRCodeNumber);
                return Ok(new
                {
                    statusCode = StatusCodes.Status200OK,
                    message = "QR code disabled successfully.",
                    data = response
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Business validation error in DisableQRCode for QR code: {QRCodeNumber}", request.QRCodeNumber);
                return BadRequest(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in DisableQRCode for QR code: {QRCodeNumber}", request.QRCodeNumber);
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost("GenerateBatchQRCodeDetails")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GenerateBatchQRCode([FromBody] BatchQRcodeRequestDto request)
        {
            _logger.LogInformation($"Request received for GenerateBatchQRCode: {request}");

            try
            {
              var response = await _qrCodeService.ProcessBatchService(request);

                if (response == null)
                {
                    _logger.LogWarning($"Get Batch QrCode  failed for request: {request}");
                    return BadRequest(new { message = "Failed to get BatchQR code details." });
                }

                _logger.LogInformation($"Fetched successfully BatchQR code details for request: {request}, response: {response}");
                return Ok(response);
            }           
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in GenerateBatchQRCode for request: {request}");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GenerateStandardFieldQRCodeDetails")]
        public async Task<ActionResult> GenerateStandardFieldQRCodeDetails([FromBody] StandardQRDataDto request)
        {
            _logger.LogInformation($"Request received for GenerateStandardFieldQRCodeDetails: {request}");
            try
            {
                var id = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.CreatedBy = id;
                var response = await _qrCodeService.InsertStandardQRCodeDetailsAsync(request);
                if (response == null)
                {
                    _logger.LogWarning($"GenerateStandardFieldQRCodeDetails failed for request: {request}");
                    return BadRequest(new { message = "Failed to insert QR code details." });
                }
                _logger.LogInformation($"GenerateStandardFieldQRCodeDetails successful for request: {request}, response: {response}");
                
                var summary = response
                    .Where(x => x is not null)
                    .Select(x => new
                    {
                        srNumber = x?.SrNo,
                        qrCodeNumber = x?.QrCodeNumber,
                        id = x?.IdNumber,
                        serialNumberOfQuantity = x?.SerialNumberOfQuantity
                    })
                    .ToList();

                return Ok(new
                {
                    qrCodeDetails = response,
                    serialNumberSummary = summary
                });
            }
            catch (ValidationException ex)
            {
                _logger.LogError($"Validation Error for GenerateStandardFieldQRCodeDetails: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in GenerateStandardFieldQRCodeDetails for request: {request}");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpGet("GetBarcodeDetails")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetQRcodeDetailsAsync([FromQuery] string QRCodeNumber, [FromQuery] int? qrCodeStatusId = null)
        {
            _logger.LogInformation($"Request received for GetQRcodeDetailsAsync with QRCodeNumber: {QRCodeNumber}, qrCodeStatusId: {qrCodeStatusId}");

            try
            {
                var result = await _qrCodeService.GetQRCodeDetailsService(QRCodeNumber, qrCodeStatusId);

                if (result == null)
                {
                    _logger.LogInformation($"No QR code details found for QRCodeNumber: {QRCodeNumber}");
                    return NotFound("No QR code details found or QR code successfully consumed.");
                }

                _logger.LogInformation($"GetQRcodeDetailsAsync successful for QRCodeNumber: {QRCodeNumber}, response: {result}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in GetQRcodeDetailsAsync for QRCodeNumber: {QRCodeNumber}");
                return BadRequest("An error occurred while processing your request. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("GetBarcodeDetailsWithParameters")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetQRcodeDetailsWithParametersAsync(
            [FromQuery] int? CreatedBy = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] GetBarcodeDetailsRequestDto? request = null)
        {
            _logger.LogInformation($"Request received for GetQRcodeDetailsWithParametersAsync with request: {request}, CreatedBy: {CreatedBy}");

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            try
            {
                var result = await _qrCodeService.GetBarcodeDetailsWithParametersService(
                    request?.SearchQuery, request?.ProdSeries, CreatedBy, request?.FromDate, request?.ToDate,
                    pageNumber, pageSize);

                if (result.Data.Count == 0)
                {
                    _logger.LogInformation("No QR code details found for Request {Request}", request);
                    return NotFound("No QR code details found.");
                }

                _logger.LogInformation($"GetQRcodeDetailsWithParametersAsync successful for Request: {request}, page {result.PageNumber} of {result.TotalPages}, response count: {result.Data.Count}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in GetQRcodeDetailsWithParametersAsync for Request: {request}");
                return BadRequest("An error occurred while processing your request. Please try again later.");
            }
        }

        [Authorize]
        [HttpGet("GetConsumedBarcodeDetailsWithParameters")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetConsumedQRcodeDetailsWithParametersAsync([FromQuery] GetQRCodeRequestDto getQRCodeRequestDto)
        {
            _logger.LogInformation($"Request received for GetConsumedQRcodeDetailsWithParametersAsync with getQRCodeRequestDto: {getQRCodeRequestDto}");

            try
            {
                var result = await _qrCodeService.GetConsumedQRCodeDetailsWithParameterService(getQRCodeRequestDto);

                if (result == null)
                {
                    _logger.LogInformation("No consumed QR code details found for Request {Request}", getQRCodeRequestDto);
                    return NotFound("No QR code details found.");
                }

                _logger.LogInformation($"GetConsumedQRcodeDetailsWithParametersAsync successful for Request: {getQRCodeRequestDto}, response: {result}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in GetConsumedQRcodeDetailsWithParametersAsync for Request: {getQRCodeRequestDto}");
                return BadRequest("An error occurred while processing your request. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("ComponentStoreIn")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ComponentStoreIn([FromBody] string QRCodeNumber)
        {
            _logger.LogInformation($"Request received for ComponentStoreIn with QRCodeNumber: {QRCodeNumber}");

            try
            {
                var result = await _qrCodeService.ComponentStoreInService(QRCodeNumber);

                if (result == null)
                {
                    _logger.LogInformation($"No component store-in details found for QRCodeNumber: {QRCodeNumber}");
                    return NotFound("No ComponentStoreIn details found.");
                }

                _logger.LogInformation($"ComponentStoreIn successful for QRCodeNumber: {QRCodeNumber}, response: {result}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in ComponentStoreIn for QRCodeNumber: {QRCodeNumber}");

                // Handle known messages
                if (ex.Message == "Invalid QR code number.")
                {
                    return NotFound(new { message = ex.Message });
                }
                else if (ex.Message == "QR code already consumed.")
                {
                    return BadRequest(new { message = ex.Message });
                }

                // Generic fallback
                return StatusCode(500, new { message = "An error occurred while processing your request. Please try again later." });
            }
        }


        [Authorize]
        [HttpPost("GetStoredComponentsByDate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetStoreInComponenetByDate([FromBody] StoredInQrCodeRequest storeindate)
        {
          
            _logger.LogInformation($"Request received for GetStoreInQRCodeByDate:");

            try
            {
                var result = await _qrCodeService.GetComponentStoreInByDateService(storeindate);

                if (result == null)
                {
                    _logger.LogInformation($"No component store-in details found for date");
                    return NotFound($"No ComponentStoreIn details found for date");
                }

                // Get only unique QR code entries
                var uniqueQrCodeList = result
                    .GroupBy(x => x.QrCodeNumber)
                    .Select(g => g.First())
                    .ToList();

                _logger.LogInformation($"Returning {uniqueQrCodeList.Count} unique QR codes for date");

                return Ok(uniqueQrCodeList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in GetStoreInQRCodeByDate");
                return BadRequest("An error occurred while GetStoreInQRCodeByDate. Please try again later.");
            }
        }


        //Export ConsumedIn API

        [Authorize]
        [HttpPost("ExportStoredInComponentsByDate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExportStoreInComponenetByDate([FromBody] StoredInQrCodeRequest storeInRequest)
        {
            _logger.LogInformation($"Request received for ExportStoreInComponenetByDate:");

            try
            {
                  
                //Get StoredIn Component by Date
                var result = await _qrCodeService.GetComponentStoreInByDateService(storeInRequest);

                if (result == null)
                {
                    _logger.LogInformation($"No component store-in details found for date:");
                    return NotFound($"No ComponentStoreIn details found for date:");
                }

                // Get only unique QR code entries
                var storedInComponent = result
                    .GroupBy(x => x.QrCodeNumber)
                    .Select(g => g.First())
                    .ToList();

                if (!storedInComponent.Any())
                {
                    return NotFound($"No Stored In Component (QR code) data found for");
                }
                DateTime currentDate = DateTime.UtcNow;
                var excelContent = _qrCodeService.ExportQRCodeToExcel(storedInComponent);
                var fileName = $"StoredInQRCode_{currentDate:yyyy-MM-dd}.xlsx";

                _logger.LogInformation($"Successfully exported StoredIn {storedInComponent.Count} QR codes.");

                return File(excelContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in ExportStoreInComponenetByDate:");
                return BadRequest("An error occurred while ExportStoreInComponenetByDate. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("ExportQrCode")]
        public async Task<IActionResult> ExportQrCodesAsync([FromBody] ExportQrCodeRequestDto payload)
        {
            var qrCodeNumbers = ExtractQrCodeNumbers(payload);
            var batchIdNumbers = payload.BatchIdNumbers?
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .ToList() ?? new List<string>();
            _logger.LogInformation($"Request received for ExportQrCodes with {qrCodeNumbers.Count} QR codes and {batchIdNumbers.Count} batch ID numbers.");

            try
            {
                // Check if QR codes are Standard or Manufacturing type
                // We'll check the first QR code to determine the type
                if (!qrCodeNumbers.Any())
                {
                    return NotFound("No QR code data found.");
                }

                // Detect type from first QR code
                bool isStandardType = await _qrCodeService.GetQRCodeDetailsService(qrCodeNumbers.First()) == null;
                
                if (!isStandardType)
                {
                    // Try to get it as old QR code
                    var testOld = await _qrCodeService.GetQRCodeDetailsService(qrCodeNumbers.First());
                    if (testOld != null && testOld.Shapes == null)
                    {
                        isStandardType = false;
                    }
                    else
                    {
                        isStandardType = true;
                    }
                }

                _logger.LogInformation($"Detected QR codes as {(isStandardType ? "Standard" : "Manufacturing")} type");

                // Export based on type
                if (isStandardType)
                {
                    // Handle Standard QR codes
                    var allStandardQRCodeDetails = new List<StandardQRDetailsResponseDto>();
                    var standardDetailsCache = new Dictionary<string, StandardQRDetailsResponseDto?>();
                    var standardPairs = BuildQrBatchPairs(qrCodeNumbers, batchIdNumbers);

                    foreach (var (qrCodeNumber, batchId) in standardPairs)
                    {
                        if (!standardDetailsCache.TryGetValue(qrCodeNumber, out var baseDetails))
                        {
                            baseDetails = await _qrCodeService.GetStandardQRCodeDetailsService(qrCodeNumber);
                            standardDetailsCache[qrCodeNumber] = baseDetails;
                        }

                        if (baseDetails != null)
                        {
                            var details = CloneDetails(baseDetails);
                            details.BatchID = batchId;
                            allStandardQRCodeDetails.Add(details);
                        }
                        else
                        {
                            _logger.LogWarning($"No Standard QR details found for QR Code: {qrCodeNumber}");
                        }
                    }

                    if (!allStandardQRCodeDetails.Any())
                    {
                        return NotFound("No Standard QR code data found.");
                    }

                    var orderedStandardQrCodes = OrderByLatestCreatedStandardAscending(allStandardQRCodeDetails);
                    var excelContent = _qrCodeService.ExportStandardQRCodeToExcel(orderedStandardQrCodes);
                    
                    var firstDetail = orderedStandardQrCodes.First();
                    var userName = User.FindFirst("username")?.Value ?? "User";
                    var drawingNumber = SanitizeFilename(firstDetail.DrawingNumber, "QRCode");
                    var idNumber = SanitizeFilename(firstDetail.IdNumber, "ID");
                    var dateStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var fileName = $"{drawingNumber}_{idNumber}_{dateStr}_{userName}.xlsx";

                    _logger.LogInformation($"Successfully exported {allStandardQRCodeDetails.Count} Standard QR codes.");

                    return File(excelContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
                else
                {
                    // Handle Manufacturing (old) QR codes
                    var allQRCodeDetails = new List<QRCodeDetailsResponseDto>();
                    var qrDetailsCache = new Dictionary<string, QRCodeDetailsResponseDto?>();
                    var qrPairs = BuildQrBatchPairs(qrCodeNumbers, batchIdNumbers);

                    foreach (var (qrCodeNumber, batchId) in qrPairs)
                    {
                        if (!qrDetailsCache.TryGetValue(qrCodeNumber, out var baseDetails))
                        {
                            baseDetails = await _qrCodeService.GetQRCodeDetailsService(qrCodeNumber);
                            qrDetailsCache[qrCodeNumber] = baseDetails;
                        }

                        if (baseDetails != null)
                        {
                            var details = CloneDetails(baseDetails);
                            details.BatchID = batchId;
                            allQRCodeDetails.Add(details);
                        }
                        else
                        {
                            _logger.LogWarning($"No details found for QR Code: {qrCodeNumber}");
                        }
                    }

                    if (!allQRCodeDetails.Any())
                    {
                        return NotFound("No QR code data found.");
                    }

                    var orderedQrCodes = OrderByLatestCreatedAscending(allQRCodeDetails);
                    var excelContent = _qrCodeService.ExportQRCodeToExcel(orderedQrCodes);
                    
                    var firstDetail = orderedQrCodes.First();
                    var userName = User.FindFirst("username")?.Value ?? "User";
                    var drawingNumber = SanitizeFilename(firstDetail.DrawingNumber, "QRCode");
                    var idNumber = SanitizeFilename(firstDetail.IdNumber, "ID");
                    var dateStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var fileName = $"{drawingNumber}_{idNumber}_{dateStr}_{userName}.xlsx";

                    _logger.LogInformation($"Successfully exported {allQRCodeDetails.Count} Manufacturing QR codes.");

                    return File(excelContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during ExportQrCodesAsync");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        private static T CloneDetails<T>(T source)
        {
            var json = JsonSerializer.Serialize(source);
            return JsonSerializer.Deserialize<T>(json)!;
        }

        // When a single QR code covers many batch entries (e.g. one QR, BatchIdNumbers "1/295".."295/295"),
        // repeat that QR code once per batch entry instead of dropping the extras beyond QRCodeNumbers.Count.
        private static List<(string QrCodeNumber, string? BatchId)> BuildQrBatchPairs(List<string> qrCodeNumbers, List<string> batchIdNumbers)
        {
            var pairs = new List<(string, string?)>();

            if (qrCodeNumbers.Count == 1 && batchIdNumbers.Count > 1)
            {
                foreach (var batchId in batchIdNumbers)
                {
                    pairs.Add((qrCodeNumbers[0], batchId));
                }
            }
            else
            {
                for (int i = 0; i < qrCodeNumbers.Count; i++)
                {
                    pairs.Add((qrCodeNumbers[i], i < batchIdNumbers.Count ? batchIdNumbers[i] : null));
                }
            }

            return pairs;
        }

        private static List<string> ExtractQrCodeNumbers(ExportQrCodeRequestDto payload)
        {
            var numbers = new List<string>();

            if (payload.QRCodeNumbers != null)
            {
                numbers.AddRange(payload.QRCodeNumbers.Where(n => !string.IsNullOrWhiteSpace(n))!);
            }

            if (payload.SerialNumberSummary != null)
            {
                numbers.AddRange(payload.SerialNumberSummary
                    .Where(s => !string.IsNullOrWhiteSpace(s?.QrCodeNumber))
                    .Select(s => s!.QrCodeNumber!));
            }

            if (payload.QrCodeDetails != null)
            {
                numbers.AddRange(payload.QrCodeDetails
                    .Where(d => !string.IsNullOrWhiteSpace(d?.QrCodeNumber))
                    .Select(d => d!.QrCodeNumber!));
            }

            return numbers.Distinct().ToList();
        }

        [Authorize]
        [HttpGet("GetConsumedIn")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetConsumedInAsync([FromQuery] ConsumedInRequestDto getConsumedInRequest)
        {
            _logger.LogInformation($"Request received for GetConsumedInAsync with parameters: {getConsumedInRequest}");

            try
            {
                var result = await _qrCodeService.ConsumedInService(getConsumedInRequest);

                if (result == null)
                {
                    _logger.LogInformation($"No QR code consumption details found for request: {getConsumedInRequest}");
                    return NotFound("No QR code details found.");
                }

                _logger.LogInformation($"GetConsumedInAsync successful for request: {getConsumedInRequest}, response: {result}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in GetConsumedInAsync for request: {getConsumedInRequest}");
                return BadRequest("An error occurred while processing your request. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("ExportViewQrCode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportViewQrCodeAsync(
            [FromQuery] int? CreatedBy,
            [FromBody] ExportViewQrCodeRequestDto request)
        {
            _logger.LogInformation($"Request received for ExportViewQrCode with request: {request}");

            try
            {
                // If QRCodeNumbers is provided in request body, fetch each QR code individually,
                // detecting Standard vs Manufacturing type the same way ExportQrCode does.
                if (request.QRCodeNumbers != null && request.QRCodeNumbers.Any())
                {
                    var qrCodeNumberList = request.QRCodeNumbers
                        .Select(q => q?.Trim())
                        .Where(q => !string.IsNullOrWhiteSpace(q))
                        .ToList();

                    _logger.LogInformation($"Processing {qrCodeNumberList.Count} QR codes for export");

                    if (!qrCodeNumberList.Any())
                    {
                        return NotFound("No QR code data found.");
                    }

                    bool isStandardType = await _qrCodeService.GetQRCodeDetailsService(qrCodeNumberList.First(), request.QrCodeStatusId) == null;

                    _logger.LogInformation($"Detected QR codes as {(isStandardType ? "Standard" : "Manufacturing")} type");

                    if (isStandardType)
                    {
                        var allStandardQRCodeDetails = new List<StandardQRDetailsResponseDto>();

                        for (int i = 0; i < qrCodeNumberList.Count; i++)
                        {
                            var details = await _qrCodeService.GetStandardQRCodeDetailsService(qrCodeNumberList[i]);
                            if (details != null)
                            {
                                // BatchIdNumbers is parallel to QRCodeNumbers — assign by index
                                if (request.BatchIdNumbers != null && i < request.BatchIdNumbers.Count)
                                {
                                    details.BatchID = request.BatchIdNumbers[i];
                                }
                                allStandardQRCodeDetails.Add(details);
                            }
                            else
                            {
                                _logger.LogWarning($"No Standard QR details found for QR Code: {qrCodeNumberList[i]}");
                            }
                        }

                        if (!allStandardQRCodeDetails.Any())
                        {
                            return NotFound("No Standard QR code data found.");
                        }

                        var orderedStandardQrCodes = OrderByLatestCreatedStandardAscending(allStandardQRCodeDetails);
                        var standardExcelContent = _qrCodeService.ExportStandardQRCodeToExcel(orderedStandardQrCodes);

                        var firstStandardDetail = orderedStandardQrCodes.First();
                        var standardUserName = User.FindFirst("username")?.Value ?? "User";
                        var standardDrawingNumber = SanitizeFilename(firstStandardDetail.DrawingNumber, "QRCode");
                        var standardIdNumber = SanitizeFilename(firstStandardDetail.IdNumber, "ID");
                        var standardDateStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        var standardFileName = $"{standardDrawingNumber}_{standardIdNumber}_{standardDateStr}_{standardUserName}.xlsx";

                        _logger.LogInformation($"Successfully exported {allStandardQRCodeDetails.Count} Standard QR codes.");

                        return File(standardExcelContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", standardFileName);
                    }
                    else
                    {
                        var allQRCodeDetails = new List<QRCodeDetailsResponseDto>();

                        for (int i = 0; i < qrCodeNumberList.Count; i++)
                        {
                            var details = await _qrCodeService.GetQRCodeDetailsService(qrCodeNumberList[i], request.QrCodeStatusId);
                            if (details != null)
                            {
                                // BatchIdNumbers is parallel to QRCodeNumbers — assign by index
                                if (request.BatchIdNumbers != null && i < request.BatchIdNumbers.Count)
                                {
                                    details.BatchID = request.BatchIdNumbers[i];
                                }
                                allQRCodeDetails.Add(details);
                            }
                            else
                            {
                                _logger.LogWarning($"No details found for QR Code: {qrCodeNumberList[i]}");
                            }
                        }

                        if (!allQRCodeDetails.Any())
                        {
                            return NotFound("No QR code data found for the provided parameters.");
                        }

                        var orderedQrCodes = OrderByLatestCreated(allQRCodeDetails);
                        var excelContent = _qrCodeService.ExportQRCodeToExcel(orderedQrCodes, request.SelectedColumns);

                        var firstDetail = orderedQrCodes.First();
                        var userName = User.FindFirst("username")?.Value ?? "User";
                        var drawingNumber = SanitizeFilename(firstDetail.DrawingNumber, "QRCode");
                        var idNumber = SanitizeFilename(firstDetail.IdNumber, "ID");
                        var dateStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        var fileName = $"{drawingNumber}_{idNumber}_{dateStr}_{userName}.xlsx";

                        _logger.LogInformation($"Successfully exported {allQRCodeDetails.Count} QR codes.");

                        return File(excelContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
                else
                {
                    // Same filter shape as GetBarcodeDetailsWithParameters -- every filter ANDed
                    // together, DrawingNumber/LineItemCode by text, ProdSeries/IdNumbers as arrays.
                    // pageSize: int.MaxValue == "no pagination", export needs every matching row.
                    var pagedResult = await _qrCodeService.GetBarcodeDetailsWithParametersService(
                        request.SearchQuery, request.ProdSeries, CreatedBy, request.FromDate, request.ToDate,
                        pageNumber: 1, pageSize: int.MaxValue);
                    var allQRCodeDetails = pagedResult.Data;

                    if (allQRCodeDetails == null || !allQRCodeDetails.Any())
                    {
                        _logger.LogWarning($"No QR code details found for request: {request}");
                        return NotFound("No QR code data found for the provided parameters.");
                    }

                    var orderedQrCodes = OrderByLatestCreated(allQRCodeDetails);
                    var excelContent = _qrCodeService.ExportQRCodeToExcel(orderedQrCodes, request.SelectedColumns);

                    var firstDetail = orderedQrCodes.First();
                    var userName = User.FindFirst("username")?.Value ?? "User";
                    var drawingNumber = SanitizeFilename(firstDetail.DrawingNumber, "QRCode");
                    var idNumber = SanitizeFilename(firstDetail.IdNumber, "ID");
                    var dateStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var fileName = $"{drawingNumber}_{idNumber}_{dateStr}_{userName}.xlsx";

                    _logger.LogInformation($"Successfully exported {allQRCodeDetails.Count} QR codes.");

                    return File(excelContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error during ExportViewQrCodeAsync for request: {request}");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpGet("GetAllUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            _logger.LogInformation("Request received for GetAllUsersAsync");

            try
            {
                var result = await _qrCodeService.GetAllUsersServiceAsync();

                if (result == null || !result.Any())
                {
                    _logger.LogWarning("No users found in GetAllUsersAsync");
                    return NotFound(new { message = "No active users found." });
                }

                _logger.LogInformation("Successfully returned {Count} users from GetAllUsersAsync", result.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetAllUsersAsync");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpGet("GetDistinctBatchIdNumbers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDistinctBatchIdNumbersAsync()
        {
            _logger.LogInformation("Request received for GetDistinctBatchIdNumbersAsync with ProdSeriesId: {ProdSeriesId}, DrawingId: {DrawingId}");

            try
            {
                var result = await _qrCodeService.GetDistinctBatchIdNumbersServiceAsync();

                if (result == null || !result.Any())
                {
                    _logger.LogInformation("No distinct batch ID numbers found");
                    return NotFound("No distinct batch ID numbers found.");
                }

                _logger.LogInformation("Successfully returned {Count} distinct batch ID numbers", result.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetDistinctBatchIdNumbersAsync");
                return BadRequest("An error occurred while processing your request. Please try again later.");
            }
        }

        [Authorize]
        [HttpGet("GetAllFanManSerialNumbers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllFanManSerialNumbersAsync()
        {
            _logger.LogInformation("Request received for GetAllFanManSerialNumbersAsync");

            try
            {
                var result = await _qrCodeService.GetAllFanManSerialNumbersServiceAsync();

                if (result == null || !result.Any())
                {
                    _logger.LogInformation("No fan man serial numbers found");
                    return NotFound("No fan man serial numbers found.");
                }

                _logger.LogInformation("Successfully returned {Count} fan man serial numbers", result.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetAllFanManSerialNumbersAsync");
                return BadRequest("An error occurred while processing your request. Please try again later.");
            }
        }

        [Authorize]
        [HttpGet("ExportConsumedIn")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExportConsumedInAsync([FromQuery] ConsumedInRequestDto request)
        {
            _logger.LogInformation("Request received for ExportConsumedInAsync with parameters: {@Request}", request);

            try
            {
                var fileContent = await _qrCodeService.ExportConsumedInServiceAsync(request);

                if (fileContent == null || fileContent.Length == 0)
                {
                    _logger.LogInformation("No data found to export for request: {@Request}", request);
                    return NotFound("No data found to export.");
                }

                var fileName = $"ConsumedIn_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                _logger.LogInformation("ExportConsumedInAsync successful. File generated: {FileName}", fileName);

                return File(
                    fileContent,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in ExportConsumedInAsync for request: {@Request}", request);
                return BadRequest("An error occurred while exporting data. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost]
        [Route("BulkUpdateQRCode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BulkUpdateQRCode([FromBody] BulkUpdateQRCodeRequestDto request)
        {
            _logger.LogInformation("Request received for BulkUpdateQRCode");

            try
            {
                var updatedCount = await _qrCodeService.BulkUpdateQRCodeService(request);

                if (updatedCount == 0)
                {
                    _logger.LogWarning("No records updated");
                    return BadRequest(new { message = "No QR codes updated." });
                }

                return Ok(new
                {
                    message = "Bulk update successful",
                    updatedRecords = updatedCount
                });
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, "Validation error in BulkUpdateQRCode");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in BulkUpdateQRCode");
                return StatusCode(500, "Something went wrong");
            }
        }

        [Authorize]
        [HttpGet("GetAvailableQr")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetAvailableQr(
            [FromQuery] string? lnItemCode = null,
            [FromQuery] string? drawingNumber = null,
            [FromQuery] int? prodSeriesId = null,
            [FromQuery] int? qrType = null)
        {
            _logger.LogInformation($"Request received for QRCodeController:GetAvailableQr LnItemCode={lnItemCode}, DrawingNumber={drawingNumber}, ProdSeriesId={prodSeriesId}, QrType={qrType}");

            try
            {
                var request = new GetAvailableQrRequest
                {
                    LnItemCode = lnItemCode,
                    DrawingNumber = drawingNumber,
                    ProdSeriesId = prodSeriesId,
                    QrType = qrType
                };

                var response = await _qrCodeService.GetAvailableQrService(request);

                if (response == null)
                {
                    _logger.LogWarning("QRCodeController:GetAvailableQr - Failed");
                    return BadRequest(new { message = "Failed GetAvailableQr." });
                }

                _logger.LogInformation("QRCodeController:GetAvailableQr - Successfully retrieved details");
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "QRCodeController:GetAvailableQr - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QRCodeController:GetAvailableQr - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }
    }
}

