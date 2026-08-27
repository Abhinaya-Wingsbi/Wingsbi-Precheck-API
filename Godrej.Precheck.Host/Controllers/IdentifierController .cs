using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DTOs.Identifier;
using Godrej.Precheck.Models.DTOs.IRNumber;
using Godrej.Precheck.Models.DTOs.MSNNumber;
using Godrej.Precheck.Repository.Queries;
using Godrej.Precheck.Service.Service.CommonSevice;
using Godrej.Precheck.Service.Service.IdentifierService;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Godrej.Precheck.Api.Controllers
{

    [ApiController]
    [Route("api/reports")]
    public class IdentifierController : ControllerBase
    {
        private readonly ILogger<IdentifierController> _logger;
        private readonly IIdentifierService _identifierService;
        private readonly ICommonService _commonService;
        private readonly IConverter _converter;

        public IdentifierController(ILogger<IdentifierController> logger, IIdentifierService identifierService, ICommonService commonService, IConverter converter)
        {
            _logger = logger;
            _identifierService = identifierService;
            _commonService = commonService;
            _converter = converter;
        }

        //GET ALL IRNUMBER - View accessible to all authenticated users
        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetAllIRNumber")]
        public async Task<IActionResult> GetAllIRNumber([FromQuery] GetAllIRNumberRequestDto getAllIRNumberRequestDto)
        {
            try
            {
                _logger.LogInformation($"Request for IdentifierController:GetAllIRNumber method:{getAllIRNumberRequestDto}");

                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                var deptId = Convert.ToInt32(User.FindFirst("deptid")?.Value);
                getAllIRNumberRequestDto.userId = userId;
                getAllIRNumberRequestDto.departmentId = deptId;

                var result = await _commonService.IRNumberService(getAllIRNumberRequestDto);

                if (result == null)
                {
                    _logger.LogInformation("Response for IdentifierController:GetAllIRNumber method: No GetAllIRNumber found.");

                    return NotFound();
                }

                var IRNumberResponse = result.Adapt<List<IRNumberDto>>();

                _logger.LogInformation($"Response for IdentifierController:GetAllIRNumber method: {result}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for IdentifierController:GetAllIRNumber method: {ex}");
                return BadRequest(ex);

            }
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetIRNumberByDrawingNumber")]
        public async Task<IActionResult> GetIRNumberByDrawingNumber([FromQuery]  GetIRNumberByDrawingNumberRequest getIRNumberByDrawingNumberRequest)
        {
            try
            {
                _logger.LogInformation($"Request for IdentifierController:GetIRNumberByDrawingNumber method:{getIRNumberByDrawingNumberRequest}");

                var result = await _commonService.IRNumberByDrawingNumberService(getIRNumberByDrawingNumberRequest);

                if (result == null)
                {
                    _logger.LogInformation("Response for IdentifierController:GetIRNumberByDrawingNumber method: No IRNumber found.");

                    return NotFound();
                }

                var IRNumberResponse = result.Adapt<List<IRNumberDto>>();

                _logger.LogInformation($"Response for IdentifierController:GetIRNumberByDrawingNumber method: {result}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for IdentifierController:GetIRNumberByDrawingNumber method: {ex}");
                return BadRequest(ex);

            }
        }

        //GET ALL MSNNUMBER - View accessible to all authenticated users
        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetAllMSNNumber")]
        public async Task<IActionResult> GetAllMSNNumber([FromQuery] GetAllMSNNumberRequestDto getAllMSNNumber)
        {
            try
            {
                _logger.LogInformation($"Request for IdentifierController:GetAllMSNNumber method : {getAllMSNNumber}");

                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                var deptId = Convert.ToInt32(User.FindFirst("deptid")?.Value);
                getAllMSNNumber.userId = userId;
                getAllMSNNumber.departmentId = deptId;
                var result = await _commonService.MSNNumberService(getAllMSNNumber);

                if (result == null)
                {
                    _logger.LogInformation("Response for IdentifierController:GetAllMSNNumber method: No MSNNumber found.");

                    return NotFound();
                }

                var MSNNumberResponse = result.Adapt<List<MSNNumberDto>>();

                _logger.LogInformation($"Response for IdentifierController:GetAllMSNNumber method: {result}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for IdentifierController:GetAllMSNNumber method: {ex}");
                return BadRequest(ex);

            }
        }

       
        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetMSNNumberByDrawingNumber")]
        public async Task<IActionResult> GetMSNNumberByDrawingNumber([FromQuery] GetMSNNumberByDrawingNumberRequest getMSNNumberByDrawingNumberRequest)
        {
            try
            {
                _logger.LogInformation($"Request for IdentifierController:GetMSNNumberByDrawingNumber method: {getMSNNumberByDrawingNumberRequest}");


                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                var deptId = Convert.ToInt32(User.FindFirst("deptid")?.Value);

                var result = await _commonService.MSNNumberByDrawingNumberService(getMSNNumberByDrawingNumberRequest);

                if (result == null)
                {
                    _logger.LogInformation("Response for IdentifierController:GetMSNNumberByDrawingNumber method: No MSNNumber found.");

                    return NotFound();
                }

                //var MSNNumberResponse = result.Adapt<List<MSNNumberDto>>();

                _logger.LogInformation($"Response for IdentifierController:GetMSNNumberByDrawingNumber method: {result}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for IdentifierController:GetMSNNumberByDrawingNumber method: {ex}");
                return BadRequest(ex);

            }
        }

        [Authorize]
        [HttpPost("IRNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IRNumbers>> InsertIRNumber([FromBody] IRNumberDto request)
        {
            _logger.LogInformation($"Request for IdentifierController: InsertIRNumber :{request}");
            try
            {
                var Id =Convert.ToInt32(User.FindFirst("id")?.Value);
                var deptId = Convert.ToInt32(User.FindFirst("deptid")?.Value);
                string DepartmentName = User.FindFirstValue("department");
                request.DepartmentName = DepartmentName;
                request.CreatedBy = Id;
                request.DepartmentId = deptId;
                var response = await _identifierService.InsertIRNumberAsync(request);

                if (response == null)
                {
                    _logger.LogWarning("InsertIRNumber - Failed for {IRNumberDto}", request);
                    return BadRequest(new { message = "Failed to insert IRNumber details." });
                }

                _logger.LogInformation("InsertIRNumber - Successfully inserted IRNumber details for {IRNumberDto}", request);
                return Ok(response);
            }
            catch (ValidationException ex)
            {
                _logger.LogError($"Validation Error for InsertIRNumber: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InsertIRNumber - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("MSNNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MSNNumbers>> InsertMSNNumber([FromBody] MSNNumberDto request)
        {
            _logger.LogInformation($"Request for IdentifierController: InsertMSNNumber{request}");
            try
            {

                var Id = Convert.ToInt32(User.FindFirst("id")?.Value);
                var deptId = Convert.ToInt32(User.FindFirst("deptid")?.Value);
                string DepartmentName = User.FindFirstValue("department");
                request.DepartmentId = deptId;
                request.DepartmentName = DepartmentName;
                request.CreatedBy = Id;
                var response = await _identifierService.InsertMSNNumberAsync(request);

                if (response == null)
                {
                    _logger.LogWarning("Response for IdentifierController:InsertMSNNumber - Failed for {MSNNumberDto}", request);
                    return BadRequest(new { message = "Failed to insert MSNNumber details." });
                }

                _logger.LogInformation($"Response for IdentifierController:InsertMSNNumber - Successfully inserted MSNNumber details for{request}");
                return Ok(response);
            }
            catch (ValidationException ex)
            {
                _logger.LogError($"Validation Error for InsertMSNNumber: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InsertMSNNumber - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("UpdateIRNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IRNumbers>> UpdateIRNumber([FromBody] UpdateIRDto request)
        {
            _logger.LogInformation($"Request for IdentifierController: UpdateIRNumber{request}");
            try
            {

                var Id = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.ModifiedBy = Id;
                var response = await _identifierService.UpadateIRNumberAsync(request);

                if (response == null)
                {
                    _logger.LogWarning("UpdateIRNumber - Failed for {Request}", request);
                    return BadRequest(new { message = $"Failed to Update IRNumber {request.IrNumber} details." });
                }

                _logger.LogInformation("UpdateIRNumber - Successfully Update IRNumber details for {Response}", response);
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "UpdateIRNumber - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateIRNumber - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }



        [Authorize]
        [HttpPost("UpdateMSNNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MSNNumbers>> UpdateMSNNumber([FromBody] UpdateMSNDto request)
        {
            _logger.LogInformation($"Request for IdentifierController: UpdateMSNNumber {request}");
            try
            {
                var Id = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.ModifiedBy = Id;
                var response = await _identifierService.UpadateMSNNumber(request);

                if (response == null)
                {
                    _logger.LogWarning("UpdateMSNNumber - Failed for {MSNNumberDto}", request);
                    return BadRequest(new { message = $"Failed to UpdateMSNNumber MSNNumber {request.MsnNumber} details." });
                }

                _logger.LogInformation("UpdateMSNNumber - Successfully Updated MSNNumber details for {MSNNumberDto}", response);
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "UpdateMSNNumber - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateMSNNumber - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("StandardIRNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IRNumbers>> InsertStandardIRNumber([FromBody] StandardIRNumberDto request)
        {
            _logger.LogInformation($"Request for IdentifierController: InsertStandardIRNumber :{request}");
            try
            {
                var Id = Convert.ToInt32(User.FindFirst("id")?.Value);
                var deptId = Convert.ToInt32(User.FindFirst("deptid")?.Value);
                string DepartmentName = User.FindFirstValue("department");
                request.DepartmentName = DepartmentName;
                request.CreatedBy = Id;
                request.DepartmentId = deptId;
                var response = await _identifierService.InsertStandardIRNumberAsync(request);

                if (response == null)
                {
                    _logger.LogWarning("InsertStandardIRNumber - Failed for {StandardIRNumberDto}", request);
                    return BadRequest(new { message = "Failed to insert Standard IRNumber details." });
                }

                _logger.LogInformation("InsertStandardIRNumber - Successfully inserted Standard IRNumber details for {StandardIRNumberDto}", request);
                return Ok(response);
            }
            catch (ValidationException ex)
            {
                _logger.LogError($"Validation Error for InsertStandardIRNumber: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InsertStandardIRNumber - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("StandardMSNNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MSNNumbers>> InsertStandardMSNNumber([FromBody] StandardMSNNumberDto request)
        {
            _logger.LogInformation($"Request for IdentifierController: InsertStandardMSNNumber{request}");
            try
            {
                var Id = Convert.ToInt32(User.FindFirst("id")?.Value);
                var deptId = Convert.ToInt32(User.FindFirst("deptid")?.Value);
                string DepartmentName = User.FindFirstValue("department");
                request.DepartmentId = deptId;
                request.DepartmentName = DepartmentName;
                request.CreatedBy = Id;
                var response = await _identifierService.InsertStandardMSNNumberAsync(request);

                if (response == null)
                {
                    _logger.LogWarning("Response for IdentifierController:InsertStandardMSNNumber - Failed for {StandardMSNNumberDto}", request);
                    return BadRequest(new { message = "Failed to insert Standard MSNNumber details." });
                }

                _logger.LogInformation($"Response for IdentifierController:InsertStandardMSNNumber - Successfully inserted Standard MSNNumber details for{request}");
                return Ok(response);
            }
            catch (ValidationException ex)
            {
                _logger.LogError($"Validation Error for InsertStandardMSNNumber: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InsertStandardMSNNumber - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("Irnumbers")]
        public async Task<IActionResult> GetAllIRNumberDistinctValues()
        {
            try
            {
                _logger.LogInformation("Request for IdentifierController:GetAllIRNumberDistinctValues");

                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                var deptId = Convert.ToInt32(User.FindFirst("deptid")?.Value);

                var result = await _commonService.GetAllIRNumberDistinctValuesService();

                if (result == null || !result.Any())
                {
                    _logger.LogInformation("No IR Numbers found.");
                    return NotFound();
                }

                _logger.LogInformation("Successfully returned IR Numbers");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in GetAllIRNumberDistinctValues: {ex}");
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("MSNNumbers")]
        public async Task<IActionResult> GetAllMSNNumber()
        {
            try
            {
                _logger.LogInformation("Request for IdentifierController:GetAllMSNNumber");

                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                var deptId = Convert.ToInt32(User.FindFirst("deptid")?.Value);

                var result = await _commonService.GetAllMSNNumberService();

                return Ok(result ?? new List<MSNDistinctValues>());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in GetAllMSNNumber: {ex}");
                return BadRequest(ex);
            }
        }
        [HttpPost("DownloadMSNMemo")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DownloadMSNMemo([FromBody] Godrej.Precheck.Models.DTOs.Identifier.DownloadMSNMemoDto request)
        {
            try
            {
                _logger.LogInformation("Request for IdentifierController:DownloadMSNMemo");
                request ??= new Godrej.Precheck.Models.DTOs.Identifier.DownloadMSNMemoDto();

                // Set the logged-in user's ID so the service can fetch their signature
                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.CreatedBy = userId;

                string htmlContent = await _identifierService.GenerateDownloadMSNMemoHtml(request);

                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings = new GlobalSettings
                    {
                        ColorMode    = ColorMode.Color,
                        Orientation  = Orientation.Portrait,
                        PaperSize    = PaperKind.A4,
                        Margins      = new MarginSettings
                        {
                            Top    = 12,
                            Bottom = 12,
                            Left   = 10,
                            Right  = 10,
                            Unit   = Unit.Millimeters
                        },
                        DocumentTitle = "MSN Memo"
                    },
                    Objects =
                    {
                        new ObjectSettings
                        {
                            HtmlContent = htmlContent,
                            WebSettings = new WebSettings
                            {
                                DefaultEncoding           = "utf-8",
                                EnableIntelligentShrinking = true,
                                LoadImages                 = true
                            }
                        }
                    }
                };

                byte[] pdfBytes = _converter.Convert(doc);

                _logger.LogInformation("IdentifierController:DownloadMSNMemo - PDF generated ({Size} bytes)", pdfBytes.Length);
                return File(pdfBytes, "application/pdf", "MSNMemo.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in IdentifierController:DownloadMSNMemo");
                return BadRequest(new { message = "Failed to generate MSN Memo.", error = ex.Message });
            }
        }

    }
}
