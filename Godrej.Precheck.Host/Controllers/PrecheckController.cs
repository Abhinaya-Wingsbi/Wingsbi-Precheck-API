using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DataModel.Precheck;
using Godrej.Precheck.Models.DTOs.Barcode;
using Godrej.Precheck.Models.DTOs.Precheck;
using Godrej.Precheck.Models.DTOs.QRCodeDetails;
using Godrej.Precheck.Service.Service.PrecheckService;
using Godrej.Precheck.Service.Service.ProductionOrderService;
using Godrej.Precheck.Service.Service.QRCodeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QRCodeApi.Controllers;
using System.ComponentModel.DataAnnotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Godrej.Precheck.Host.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class PrecheckController : ControllerBase
    {
        private readonly IPrecheckService _service;
        private readonly ILogger<PrecheckController> _logger;
        private readonly IQRCodeService _qrCodeService;
        private readonly IProductionOrderService _productionOrderService;

        public PrecheckController(ILogger<PrecheckController> logger, IPrecheckService service, IQRCodeService qrCodeService)
        {
            _service = service;
            _logger = logger;
            _qrCodeService = qrCodeService;
        }

        [Authorize]
        [HttpGet("{assemblyNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAssemblyDrawing(string assemblyNumber)
        {
            _logger.LogInformation("Request received for PrecheckController:GetAssemblyDrawing {AssemblyNumber}", assemblyNumber);

            try
            {
                var result = await _service.GetPrecheckAssemblyTemplate(assemblyNumber);
                if (result == null || !result.Any())
                {
                    return NotFound();
                }
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:GetAssemblyDrawing - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:GetAssemblyDrawing - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("MakePrecheck")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> MakePrecheckForAssembly([FromBody] List<PrecheckRequestDto> request)
        {
            if (request == null || !request.Any())
            {
                _logger.LogWarning("MakePrecheckForAssembly: Request is null or empty.");
                return BadRequest(new { message = "Request body cannot be empty." });
            }

            _logger.LogInformation($"Request received for PrecheckController:MakePrecheckForAssembly {request}");

            try
            {
                var Id = Convert.ToInt32(User.FindFirst("id")?.Value);

                var response = await _service.MakePrecheck(request);


                if (response == null)
                {
                    _logger.LogWarning($"PrecheckController:MakePrecheckForAssembly - Insert failed for {response}");
                    return BadRequest(new { message = "Failed MakePrecheckForAssembly." });
                }

                _logger.LogInformation($"PrecheckController:MakePrecheckForAssembly - Successfully inserted details for {response}");
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:MakePrecheckForAssembly - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:MakePrecheckForAssembly - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("BulkPrecheck")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> BulkPrecheck([FromBody] List<BulkPrecheckRequestDto> requests)
        {
            if (requests == null || requests.Count == 0)
            {
                _logger.LogWarning("BulkPrecheck: Request is null or empty.");
                return BadRequest(new { message = "Request body cannot be empty." });
            }

            _logger.LogInformation($"Request received for PrecheckController:BulkPrecheck with {requests.Count} item(s)");

            try
            {
                var Id = Convert.ToInt32(User.FindFirst("id")?.Value);
                var allResponses = new List<ViewPreCheckResponse>();

                foreach (var request in requests)
                {
                    if (request.CreatedBy == 0)
                    {
                        request.CreatedBy = Id;
                    }

                    var response = await _service.BulkPrecheck(request);

                    if (response == null)
                    {
                        _logger.LogWarning($"PrecheckController:BulkPrecheck - Insert failed for {request}");
                        return BadRequest(new { message = "Failed BulkPrecheck." });
                    }

                    allResponses.AddRange(response);
                }

                _logger.LogInformation($"PrecheckController:BulkPrecheck - Successfully inserted details for {allResponses.Count} record(s)");
                return Ok(allResponses);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:BulkPrecheck - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:BulkPrecheck - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("MakePrecheckFromExcel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> MakePrecheckFromExcel(IFormFile file)
        {
            _logger.LogInformation("Request received for PrecheckController:MakePrecheckFromExcel");

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded" });
            }

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) &&
                !file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Invalid file format. Please upload an Excel file (.xlsx or .xls)" });
            }

            try
            {
                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);

                using var stream = file.OpenReadStream();
                var result = await _service.MakePrecheckFromExcelAsync(stream, userId);

                _logger.LogInformation(
                    "PrecheckController:MakePrecheckFromExcel - {Success}/{Total} QR codes prechecked",
                    result.SuccessCount, result.TotalRows);

                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:MakePrecheckFromExcel - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:MakePrecheckFromExcel - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpGet("BulkPrecheckTemplate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DownloadTemplate()
        {
            _logger.LogInformation("Request received for PrecheckController:DownloadTemplate");

            try
            {
                var fileBytes = await _service.DownloadPrecheckExcelTemplateAsync();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Precheck_Template.xlsx");
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:DownloadTemplate - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:DownloadTemplate - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("deleteprecheckdetails")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> DeletePrecheckDetails([FromBody] DeletePrecheckDetailsRequestDto request)
        {
            _logger.LogInformation(
                "Request received for PrecheckController:DeletePrecheckDetails, ProductionOrderNumber: {ProductionOrderNumber}, IdNumber: {IdNumber}, DrawingNumberId: {DrawingNumberId}",
                request.ProductionOrderNumber, request.IdNumber, request.DrawingNumberId);

            try
            {
                var modifiedBy = Convert.ToInt32(User.FindFirst("id")?.Value);
                await _service.DeletePrecheckDetailsAsync(request, modifiedBy);

                _logger.LogInformation("PrecheckController:DeletePrecheckDetails - Successfully deleted precheck detail");
                return Ok(new { message = "Precheck detail deleted successfully." });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:DeletePrecheckDetails - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:DeletePrecheckDetails - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("removeprecheckdetails")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RemovePrecheckDetails([FromBody] DeletePrecheckDetailsRequestDto request)
        {
            _logger.LogInformation(
                "Request received for PrecheckController:RemovePrecheckDetails, ProductionOrderNumber: {ProductionOrderNumber}, IdNumber: {IdNumber}, DrawingNumberId: {DrawingNumberId}",
                request.ProductionOrderNumber, request.IdNumber, request.DrawingNumberId);

            try
            {
                var modifiedBy = Convert.ToInt32(User.FindFirst("id")?.Value);
                await _service.RemovePrecheckDetailsAsync(request, modifiedBy);

                _logger.LogInformation("PrecheckController:RemovePrecheckDetails - Successfully removed precheck detail");
                return Ok(new { message = "Precheck detail removed successfully." });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:RemovePrecheckDetails - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:RemovePrecheckDetails - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("addPrecheckComponent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AddPrecheckComponent([FromBody] AddPrecheckComponentDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.AssemblyLnItemCode) || string.IsNullOrWhiteSpace(request.ChildLnItemCode))
            {
                _logger.LogWarning("PrecheckController:AddPrecheckComponent - AssemblyLnItemCode or ChildLnItemCode is null or empty.");
                return BadRequest(new { message = "AssemblyLnItemCode and ChildLnItemCode are required." });
            }

            _logger.LogInformation(
                "Request received for PrecheckController:AddPrecheckComponent, AssemblyLnItemCode: {AssemblyLnItemCode}, ChildLnItemCode: {ChildLnItemCode}",
                request.AssemblyLnItemCode, request.ChildLnItemCode);

            try
            {
                var createdBy = Convert.ToInt32(User.FindFirst("id")?.Value);
                var response = await _service.AddPrecheckComponentAsync(request, createdBy);

                var message = response.ComponentsAdded == 0
                    ? "Component already present in this assembly."
                    : "Component added successfully.";

                _logger.LogInformation("PrecheckController:AddPrecheckComponent - Completed successfully");
                return Ok(new { message, data = response });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:AddPrecheckComponent - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:AddPrecheckComponent - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("ConsumedInComponents")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ConsumedInComponents([FromBody] ConsumedInComponentsRequestDto request)
        {
            if (request == null || request.DrawingNumberId <= 0)
            {
                _logger.LogWarning("PrecheckController:ConsumedInComponents - DrawingNumberId is missing or invalid.");
                return BadRequest(new { message = "DrawingNumberId is required." });
            }

            _logger.LogInformation(
                "Request received for PrecheckController:ConsumedInComponents, DrawingNumberId: {DrawingNumberId}",
                request.DrawingNumberId);

            try
            {
                var response = await _service.GetConsumedInComponentsAsync(request.DrawingNumberId);

                if (response == null || !response.Any())
                {
                    _logger.LogWarning(
                        "PrecheckController:ConsumedInComponents - No assemblies found consuming DrawingNumberId: {DrawingNumberId}",
                        request.DrawingNumberId);
                    return NotFound(new { message = $"Drawing Number Id {request.DrawingNumberId} is not consumed in any assembly." });
                }

                _logger.LogInformation("PrecheckController:ConsumedInComponents - Found {Count} assemblies", response.Count);
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:ConsumedInComponents - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:ConsumedInComponents - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        //export precheck api
        [Authorize]
        [HttpPost("ExportPrecheckdetails")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ExportPrecheckDetails([FromBody] ViewPreCheckRequestDto request)
        {
            _logger.LogInformation("Request received for PrecheckController:ExportPrecheckDetails {@Request}", request);
            try
            {
                // Call service
                var response = await _service.ExportViewPrecheckDetailsService(request);

                if (response == null || !response.Any())
                {
                    _logger.LogWarning("PrecheckController:ExportPrecheckDetails - No precheck data found for {@Request}", request);
                    return BadRequest(new { message = "No precheck details found." }); 
                }

                _logger.LogInformation("PrecheckController:ExportPrecheckDetails - {Count} records fetched", response.Count);

                // Generate the PDF
                var pdfContent = await _service.GeneratePrecheckPdfAsync(response, request);

                var fileName = "PrecheckDetailsReport.pdf";
                _logger.LogInformation("PrecheckController:ExportPrecheckDetails - Successfully generated PDF: {FileName}", fileName);

                return File(pdfContent, "application/pdf", fileName); 
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:ExportPrecheckDetails - Application error occurred.");
                return BadRequest(new { message = ex.Message }); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:ExportPrecheckDetails - Unexpected error occurred.");
                return StatusCode(500, new { message = "An unexpected error occurred." }); 
            }
        }


        [Authorize]
        [HttpPost("MakePrecheckOrder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> MakeOrder([FromBody] MakeOrderRequestDto request)
        {
            _logger.LogInformation($"Request received for PrecheckController:MakeOrder {request}");
            try
            {
                var Id = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.CreatedBy = Id;
                var response = await _service.MakeOrder(request);

                if (response == null)
                {
                    _logger.LogWarning($"PrecheckController:MakeOrder - Insert failed for {response}");
                    return BadRequest(new { message = "Failed MakeOrder." });
                }

                _logger.LogInformation($"PrecheckController:MakeOrder - Successfully inserted details for {response}");
                return Ok(response);
            }
            catch (ValidationException Vex)
            {
                _logger.LogError($"PrecheckController:MakeOrder - {Vex}");
                return BadRequest(new { message = Vex.Message });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:MakeOrder - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:MakeOrder - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpGet("ViewPrecheck")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ViewPrecheckDetails([FromQuery] ViewPreCheckRequestDto request)
        {
            _logger.LogInformation($"Request received for PrecheckController:ViewPrecheckDetails- {request}");

            try
            {
               var response = await _service.ViewPrecheckDetailsService(request);

                if (response == null)
                {
                    _logger.LogWarning($"PrecheckController:ViewPrecheckDetails - Insert failed for {response}");
                    return BadRequest(new { message = "Failed ViewPrecheckDetails." });
                }

                _logger.LogInformation($"PrecheckController:ViewPrecheckDetails - Successfully Get details for {response}");
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:ViewPrecheckDetails - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:ViewPrecheckDetails - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("PendingPrecheck")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> PendingPrecheck([FromBody] PendingPrecheckRequestDto request)
        {
            _logger.LogInformation("Request received for PrecheckController:PendingPrecheck {@Request}", request);

            try
            {
                var response = await _service.GetPendingPrecheckAsync(request);

                _logger.LogInformation("PrecheckController:PendingPrecheck - {Count} pending production orders found", response.Count);
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:PendingPrecheck - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:PendingPrecheck - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("ExportPendingPrecheck")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ExportPendingPrecheck([FromBody] PendingPrecheckRequestDto request)
        {
            _logger.LogInformation("Request received for PrecheckController:ExportPendingPrecheck {@Request}", request);

            try
            {
                var fileBytes = await _service.ExportPendingPrecheckAsync(request);

                _logger.LogInformation("PrecheckController:ExportPendingPrecheck - Generated {Bytes} bytes", fileBytes.Length);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PendingPrecheck_Download.xlsx");
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:ExportPendingPrecheck - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:ExportPendingPrecheck - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpGet("GetPrecheckStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetPrecheckStatusDetails([FromQuery] ViewPreCheckRequestDto request)
        {
            _logger.LogInformation($"Request received for PrecheckController:ViewPrecheckDetails - {request}");

            try
            {
                var response = await _service.GetPrecheckStatusDetailsService(request);

                if (response == null)
                {
                    _logger.LogWarning($"PrecheckController:ViewPrecheckDetails - Insert failed for {response}");
                    return BadRequest(new { message = "Failed ViewPrecheckDetails." });
                }

                _logger.LogInformation($"PrecheckController:ViewPrecheckDetails - Successfully Get details for {response}");
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:ViewPrecheckDetails - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:ViewPrecheckDetails - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("GetStoreAvailablComponents")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetAvailableComponents([FromBody] AvailableComponentFilterDto filter)
        {
            _logger.LogInformation($"Request received for PrecheckController:GetAvailableComponents {filter.QrCode}");
            try
            {
                var response = await _service.AvailableComponentDetailsService(filter);
                if (response == null)
                {
                    _logger.LogWarning($"PrecheckController:GetAvailableComponents - Failed for {filter.QrCode}");
                    return BadRequest(new { message = "Failed GetAvailableComponents." });
                }
                _logger.LogInformation($"PrecheckController:GetAvailableComponents - Successfully retrieved details");
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:GetAvailableComponents - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:GetAvailableComponents - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("GetAvailablComponents")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetAvailableComponentForOrder([FromBody] GetAvailableComponentsRequest request)
        {
            _logger.LogInformation($"Request received for PrecheckController:GetAvailableComponents {request}");

            try
            {
                var response = await _service.GetAvailableComponentService(request);

                if (response == null)
                {
                    _logger.LogWarning($"PrecheckController:GetAvailableComponents - Insert failed for {response}");
                    return BadRequest(new { message = "Failed GetAvailableComponents." });
                }

                _logger.LogInformation($"PrecheckController:GetAvailableComponents - Successfully Get details for {response}");
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:GetAvailableComponents - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:GetAvailableComponents - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("AddQRCodeDetails")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AddQRCodeDetailsForPrecheck([FromBody] PrecheckQRCodeRequestDto request)
        {
            _logger.LogInformation("Request received for PrecheckController:AddQRCodeDetailsForPrecheck {@Request}", request);

            try
            {
                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                request.CreatedBy = userId;

                var response = await _qrCodeService.InsertPrecheckQRCodeDetailsService(request);

                if (response == null)
                {
                    _logger.LogWarning("PrecheckController:AddQRCodeDetailsForPrecheck - Insert failed for {@Request}", request);
                    return BadRequest(new { message = "Failed to add QR code details for precheck." });
                }

                _logger.LogInformation("PrecheckController:AddQRCodeDetailsForPrecheck - Successfully inserted QR code details {@Response}", response);
                return Ok(new 
                { 
                    message = "QR code details added successfully for precheck.",
                    data = response 
                });
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "PrecheckController:AddQRCodeDetailsForPrecheck - Validation error: {Message}", vex.Message);
                return BadRequest(new { message = vex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:AddQRCodeDetailsForPrecheck - Unexpected error occurred");
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        [Authorize]
        [HttpPost("RejectAndDuplicate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RejectAndDuplicatePrecheck([FromBody] RejectPrecheckRequestDto request)
        {
            _logger.LogInformation($"Request received for PrecheckController:RejectAndDuplicatePrecheck for PrecheckDetailsId: {request.PrecheckDetailsId}");

            try
            {
                // Set CreatedBy from the authenticated user if not provided
                if (request.CreatedBy == 0)
                {
                    var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                    request.CreatedBy = userId;
                }

                var response = await _service.RejectAndDuplicatePrecheck(request);

                if (response <= 0)
                {
                    _logger.LogWarning($"PrecheckController:RejectAndDuplicatePrecheck - Operation failed for PrecheckDetailsId: {request.PrecheckDetailsId}");
                    return BadRequest(new { message = "Failed to reject and duplicate precheck." });
                }

                _logger.LogInformation($"PrecheckController:RejectAndDuplicatePrecheck - Successfully rejected and duplicated precheck for PrecheckDetailsId: {request.PrecheckDetailsId}");
                return Ok(new 
                { 
                    message = "Precheck rejected and duplicated successfully.",
                    precheckDetailsId = request.PrecheckDetailsId
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:RejectAndDuplicatePrecheck - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:RejectAndDuplicatePrecheck - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("update-quantity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateQuantity([FromQuery] string productionOrderNumber,[FromQuery] string assemblyDrawingNo,[FromBody] UpdateMaterialQuantityRequestDto request)
        {
            if (request == null)
            {
                _logger.LogWarning("UpdateQuantity: Request body is null.");
                return BadRequest(new { message = "Request body cannot be null." });
            }

            if (string.IsNullOrEmpty(productionOrderNumber) || string.IsNullOrEmpty(assemblyDrawingNo))
            {
                _logger.LogWarning("UpdateQuantity: Missing query parameters.");
                return BadRequest(new { message = "productionOrderNumber and assemblyDrawingNo are required." });
            }

            _logger.LogInformation(
                "Request received for UpdateQuantity, DrawingNumberId: {Id}",
                request.DrawingnumberId);

            try
            {
                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                var response = await _service.UpdateQuantity(productionOrderNumber, request, assemblyDrawingNo, userId);

                if (response == null)
                {
                    return BadRequest(new { message = "Failed to update quantity." });
                }

                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "UpdateQuantity - Application error");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateQuantity - Unexpected error");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [Authorize]
        [HttpPost("RemainingPrecheck")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> PrecheckForRemainingQuantity([FromBody] RejectPrecheckRequestDto request)
        {
            _logger.LogInformation($"Request received for PrecheckController:RejectAndDuplicatePrecheck for PrecheckDetailsId: {request.PrecheckDetailsId}");

            try
            {
                // Set CreatedBy from the authenticated user if not provided
                if (request.CreatedBy == 0)
                {
                    var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                    request.CreatedBy = userId;
                }

                var response = await _service.PrecheckForRemainingQuantityService(request);

                if (response <= 0)
                {
                    _logger.LogWarning($"PrecheckController:RejectAndDuplicatePrecheck - Operation failed for PrecheckDetailsId: {request.PrecheckDetailsId}");
                    return BadRequest(new { message = "Failed to reject and duplicate precheck." });
                }

                _logger.LogInformation($"PrecheckController:RejectAndDuplicatePrecheck - Successfully rejected and duplicated precheck for PrecheckDetailsId: {request.PrecheckDetailsId}");
                return Ok(new
                {
                    message = "Precheck new row added successfully.",
                    newPrecheckDetailsId = response
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:RejectAndDuplicatePrecheck - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:RejectAndDuplicatePrecheck - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }


        [Authorize]
        [HttpPost("ResetQrQuantity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ResetRemainingQuantity([FromBody] ResetRemainingQuantityDto payload)
        {
            _logger.LogInformation($"Request received for PrecheckController:ResetRemainingQuantity IdNumber: {payload.IdNumber}, DrawingNumber: {payload.DrawingNumberId}");
            try
            {
                var response = await _service.ResetRemainingQuantityService(payload);
                if (!response)
                {
                    _logger.LogWarning($"PrecheckController:ResetRemainingQuantity - Failed for IdNumber: {payload.IdNumber}");
                    return BadRequest(new { message = "Failed to reset remaining quantity." });
                }
                _logger.LogInformation($"PrecheckController:ResetRemainingQuantity - Successfully reset remaining quantity");
                return Ok(new { message = "Remaining quantity reset successfully." });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "PrecheckController:ResetRemainingQuantity - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrecheckController:ResetRemainingQuantity - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }
    }
}
