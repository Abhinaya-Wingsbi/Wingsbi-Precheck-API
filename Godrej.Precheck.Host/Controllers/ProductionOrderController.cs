using System;
using System.Threading.Tasks;
using Godrej.Precheck.Service.Service.ProductionOrderService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Godrej.Precheck.Models.DTOs.ProductionOrder;
using Microsoft.IdentityModel.Tokens;
using Godrej.Precheck.Service.Service.PrecheckService;

namespace QRCodeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionOrderController : ControllerBase
    {
        private readonly ILogger<ProductionOrderController> _logger;
        private readonly IProductionOrderService _productionOrderService;
       

        public ProductionOrderController(
            ILogger<ProductionOrderController> logger,
            IProductionOrderService productionOrderService)
        {
            _logger = logger;
            _productionOrderService = productionOrderService;
        }

        /// <summary>
        /// Upload Excel file to import Production Orders
        /// </summary>
        [Authorize]
        [HttpPost("Upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            _logger.LogInformation("Request received for ProductionOrderController:Upload");

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
                var userId = Convert.ToInt32(User.FindFirst("id")?.Value ?? "0");

                using var stream = file.OpenReadStream();
                var result = await _productionOrderService.UploadExcelAsync(stream, userId);

                _logger.LogInformation("Upload complete: {Imported}/{Total} rows imported", result.Imported, result.TotalRows);

                if (result.Errors.Count > 0)
                {
                    return Ok(new
                    {
                        success = result.Imported > 0,
                        message = $"Imported {result.Imported} of {result.TotalRows} rows",
                        result
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = $"Successfully imported {result.Imported} rows",
                    result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading Production Order Excel");
                return StatusCode(500, new { message = "An unexpected error occurred while processing the file" });
            }
        }

        /// <summary>
        /// Upload Excel file to update Min and Status of existing Production Orders
        /// </summary>
        [Authorize]
        [HttpPost("UpdateMinStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateMinStatusExcel(IFormFile file)
        {
            _logger.LogInformation("Request received for ProductionOrderController:UpdateMinStatusExcel");

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
                var userId = Convert.ToInt32(User.FindFirst("id")?.Value ?? "0");

                using var stream = file.OpenReadStream();
                var result = await _productionOrderService.UploadMinStatusExcelAsync(stream, userId);

                _logger.LogInformation("Upload complete: {Updated}/{TotalRows} rows updated. {NotFoundCount} missing.", 
                    result.UpdatedRows, result.TotalRows, result.NotFoundProductionOrderNumbers.Count);

                return Ok(new
                {
                    success = result.UpdatedRows > 0 || result.TotalRows == 0,
                    message = $"Processed {result.TotalRows} rows. Updated {result.UpdatedRows}. Not found: {result.NotFoundProductionOrderNumbers.Count}",
                    result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading Min/Status Excel");
                return StatusCode(500, new { message = "An unexpected error occurred while processing the file" });
            }
        }

        /// <summary>
        /// Update an existing Production Order
        /// </summary>
        [Authorize]
        [HttpPost("Update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProductionOrder([FromBody] UpdateProductionOrderDto request)
        {
            _logger.LogInformation("Request received for ProductionOrderController:Update for '{PO}'", request.ProductionOrderNumber);

            if (request == null || string.IsNullOrWhiteSpace(request.ProductionOrderNumber))
            {
                return BadRequest(new { message = "Invalid request or missing Production Order Number" });
            }

            try
            {
                var userId = Convert.ToInt32(User.FindFirst("id")?.Value ?? "0");

                var success = await _productionOrderService.UpdateProductionOrderAsync(request, userId);
                if (success)
                {
                    return Ok(new { success = true, message = $"Successfully updated '{request.ProductionOrderNumber}'" });
                }

                return BadRequest(new { message = "Failed to update Production Order" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Production Order details");
                if (ex.Message.Contains("does not exist") || ex.Message.Contains("not found"))
                {
                    return NotFound(new { message = ex.Message });
                }
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get Production Order details by PO Number (for auto-fill)
        /// </summary>
        [Authorize]
        [HttpGet("GetByPONumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByPONumber([FromQuery] string productionOrderNumber)
        {
            _logger.LogInformation("Request received for ProductionOrderController:GetByPONumber {PO}", productionOrderNumber);

            if (string.IsNullOrWhiteSpace(productionOrderNumber))
            {
                return BadRequest(new { message = "Production Order Number is required" });
            }

            try
            {
                var result = await _productionOrderService.GetByProductionOrderNumberAsync(productionOrderNumber);

                if (result == null)
                {
                    return NotFound(new { message = $"Production Order '{productionOrderNumber}' not found" });
                }

                _logger.LogInformation("Found Production Order: {PO}", productionOrderNumber);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Production Order by PO Number");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [Authorize]
        [HttpGet("GetProductionOrderDetails")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductionOrderDetails([FromQuery] string productionOrderNumber)
        {
            _logger.LogInformation("Request for GetProductionOrderDetails: {PO}", productionOrderNumber);

            try
            {
                var result = await _productionOrderService.GetProductionOrderDetailsAsync(productionOrderNumber);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching production order details for {PO}", productionOrderNumber);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all Production Orders (for admin/management view)
        /// Supports optional filtering by date and precheck status
        /// </summary>
        [Authorize]
        [HttpGet("GetAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProductionOrder(
            [FromQuery] string? dateFilterType = null,
            [FromQuery] DateTime? filterDate = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? precheckStatus = null,
            [FromQuery] string? poNumber = null,
            [FromQuery] string? lnItemCode = null,
            [FromQuery] string? role = null,
            [FromQuery] string? drawingNumber = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20
            )
        {
            _logger.LogInformation("Request received for ProductionOrderController:GetAll with filters, page {PageNumber} size {PageSize}", pageNumber, pageSize);

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            try
            {
                var roleIdStr = User.FindFirst("roleid")?.Value 
                             ?? User.FindFirst("roleid")?.Value
                             ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                             ?? "0";

                int.TryParse(roleIdStr, out int roleid);

                if (!string.IsNullOrEmpty(role))
                {
                    if (role.Equals("QC", StringComparison.OrdinalIgnoreCase)) roleid = 2;
                    else if (role.Equals("Store", StringComparison.OrdinalIgnoreCase)) roleid = 3;
                    else if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || role.Equals("Planner", StringComparison.OrdinalIgnoreCase)) roleid = 1;
                }
                else if (roleid == 0 && !string.IsNullOrEmpty(roleIdStr))
                {
                    if (roleIdStr.Equals("QC", StringComparison.OrdinalIgnoreCase)) roleid = 2;
                    else if (roleIdStr.Equals("Store", StringComparison.OrdinalIgnoreCase)) roleid = 3;
                    else if (roleIdStr.Equals("Admin", StringComparison.OrdinalIgnoreCase) || roleIdStr.Equals("Planner", StringComparison.OrdinalIgnoreCase)) roleid = 1;
                }

                ProductionOrderMasterPagedResponse results;

                // Check if any filters are applied
                if (!string.IsNullOrEmpty(dateFilterType) || precheckStatus.HasValue|| !string.IsNullOrEmpty(poNumber) ||!string.IsNullOrEmpty(lnItemCode)|| !string.IsNullOrEmpty(drawingNumber))
                {
                    results = await _productionOrderService.GetAllProductionOrdersPagedAsync(
                        dateFilterType, filterDate, fromDate, toDate, precheckStatus, poNumber,
                        lnItemCode, roleid, drawingNumber, pageNumber, pageSize);
                }
                else
                {
                    results = await _productionOrderService.GetAllProductionOrdersPagedAsync(roleid, pageNumber, pageSize);
                }

                _logger.LogInformation("Found {Count} Production Orders (page {PageNumber} of {TotalPages})", results.Data.Count, results.PageNumber, results.TotalPages);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all Production Orders");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Download the official Production Order Import Template
        /// </summary>
        [Authorize]
        [HttpGet("DownloadTemplate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DownloadTemplate()
        {
            _logger.LogInformation("Request received for ProductionOrderController:DownloadTemplate");

            try
            {
                var fileBytes = await _productionOrderService.DownloadTemplateAsync();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Production_Order_Template.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading Production Order Template");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Get all Production Order Numbers with details (for dropdown and auto-fill)
        /// Uses existing GetByPONumber infrastructure
        /// </summary>
        [Authorize]
        [HttpGet("GetAllPONumbers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPONumbers([FromQuery] string? search = null)
        {
            _logger.LogInformation("Request received for ProductionOrderController:GetAllPONumbers");

            try
            {
                var results = await _productionOrderService.GetAllPONumbersAsync(search);
                _logger.LogInformation("Found {Count} PO Numbers", results.Count);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all PO Numbers");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Get Counts of Production Orders (Total, Completed, Partial)
        /// </summary>
        [Authorize]
        [HttpGet("GetCounts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductionOrderCounts(
            [FromQuery] string? dateFilterType=null,
            [FromQuery] DateTime? filterDate=null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? precheckStatus = null,
            [FromQuery] string? poNumber = null,
            [FromQuery] string? lnItemCode = null,
            [FromQuery] string? role = null,
            [FromQuery] int roleid = 0,
            [FromQuery] string? drawingnumber = null)
        {
            if (roleid == 0)
            {
                var roleIdStr = User.FindFirst("roleid")?.Value 
                             ?? User.FindFirst("roleid")?.Value
                             ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                             ?? "0";

                int.TryParse(roleIdStr, out roleid);

                if (!string.IsNullOrEmpty(role))
                {
                    if (role.Equals("QC", StringComparison.OrdinalIgnoreCase)) roleid = 2;
                    else if (role.Equals("Store", StringComparison.OrdinalIgnoreCase)) roleid = 3;
                    else if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || role.Equals("Planner", StringComparison.OrdinalIgnoreCase)) roleid = 1;
                }
                else if (roleid == 0 && !string.IsNullOrEmpty(roleIdStr))
                {
                    if (roleIdStr.Equals("QC", StringComparison.OrdinalIgnoreCase)) roleid = 2;
                    else if (roleIdStr.Equals("Store", StringComparison.OrdinalIgnoreCase)) roleid = 3;
                    else if (roleIdStr.Equals("Admin", StringComparison.OrdinalIgnoreCase) || roleIdStr.Equals("Planner", StringComparison.OrdinalIgnoreCase)) roleid = 1;
                }
            }
            var filter = new ProductionOrderCountFilterDto
            {
                DateFilterType = dateFilterType,
                FilterDate = filterDate,
                FromDate = fromDate,
                ToDate = toDate,
                PrecheckStatus = precheckStatus,
                PoNumber = poNumber,
                LnItemCode = lnItemCode,
                RoleId = roleid,
                DrawingNumber= drawingnumber
            };
          
            try
            {
                var result = await _productionOrderService.GetProductionOrderCountsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching production order counts");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Export Production Orders to Excel with Summary Counts
        /// </summary>
        [Authorize]
        [HttpGet("Export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Export(
            [FromQuery] string? dateFilterType = null,
            [FromQuery] DateTime? filterDate = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? precheckStatus = null, 
            [FromQuery] string? poNumber = null,
            [FromQuery] string? lnItemCode = null,
            [FromQuery] string? role = null,
            [FromQuery] int roleid = 0)
        {
            _logger.LogInformation("Request received for ProductionOrderController:Export");

            try
            {
                if (roleid == 0)
                {
                    var roleIdStr = User.FindFirst("roleid")?.Value 
                                 ?? User.FindFirst("roleid")?.Value
                                 ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                                 ?? "0";

                    int.TryParse(roleIdStr, out roleid);

                    if (!string.IsNullOrEmpty(role))
                    {
                        if (role.Equals("QC", StringComparison.OrdinalIgnoreCase)) roleid = 2;
                        else if (role.Equals("Store", StringComparison.OrdinalIgnoreCase)) roleid = 3;
                        else if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || role.Equals("Planner", StringComparison.OrdinalIgnoreCase)) roleid = 1;
                    }
                    else if (roleid == 0 && !string.IsNullOrEmpty(roleIdStr))
                    {
                        if (roleIdStr.Equals("QC", StringComparison.OrdinalIgnoreCase)) roleid = 2;
                        else if (roleIdStr.Equals("Store", StringComparison.OrdinalIgnoreCase)) roleid = 3;
                        else if (roleIdStr.Equals("Admin", StringComparison.OrdinalIgnoreCase) || roleIdStr.Equals("Planner", StringComparison.OrdinalIgnoreCase)) roleid = 1;
                    }
                }

                var fileBytes = await _productionOrderService.ExportProductionOrdersAsync(
                    dateFilterType, filterDate, fromDate, toDate, precheckStatus, poNumber, lnItemCode, roleid);

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ProductionOrder_Download.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting Production Orders");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }


        [Authorize]
        [HttpGet("GetAllPo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProductionOrderNumbers(
           [FromQuery] string? dateFilterType = null,
           [FromQuery] DateTime? filterDate = null,
           [FromQuery] DateTime? fromDate = null,
           [FromQuery] DateTime? toDate = null,
           [FromQuery] int? precheckStatus = null,
           [FromQuery] string? poNumber = null,
           [FromQuery] string? lnItemCode = null,
           [FromQuery] int roleid=0,
           [FromQuery] string? role = null,
           [FromQuery] string? drawingNumber = null
           )
        {
            _logger.LogInformation("Request received for ProductionOrderController:GetAll with filters");

            try
            {
                if (roleid == 0)
                {
                    var roleIdStr = User.FindFirst("roleid")?.Value 
                                 ?? User.FindFirst("roleid")?.Value
                                 ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                                 ?? "0";

                    int.TryParse(roleIdStr, out roleid);

                    if (!string.IsNullOrEmpty(role))
                    {
                        if (role.Equals("QC", StringComparison.OrdinalIgnoreCase)) roleid = 2;
                        else if (role.Equals("Store", StringComparison.OrdinalIgnoreCase)) roleid = 3;
                        else if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || role.Equals("Planner", StringComparison.OrdinalIgnoreCase)) roleid = 1;
                    }
                    else if (roleid == 0 && !string.IsNullOrEmpty(roleIdStr))
                    {
                        if (roleIdStr.Equals("QC", StringComparison.OrdinalIgnoreCase)) roleid = 2;
                        else if (roleIdStr.Equals("Store", StringComparison.OrdinalIgnoreCase)) roleid = 3;
                        else if (roleIdStr.Equals("Admin", StringComparison.OrdinalIgnoreCase) || roleIdStr.Equals("Planner", StringComparison.OrdinalIgnoreCase)) roleid = 1;
                    }
                }
               
                List<ProductionOrderMasterDto> results;

                // Check if any filters are applied
                if (!string.IsNullOrEmpty(dateFilterType) || precheckStatus.HasValue || !string.IsNullOrEmpty(poNumber) || !string.IsNullOrEmpty(lnItemCode) || !string.IsNullOrEmpty(drawingNumber))
                {
                    results = await _productionOrderService.GetAllProductionOrdersAsync(
                        dateFilterType, filterDate, fromDate, toDate, precheckStatus, poNumber,
            lnItemCode, roleid, drawingNumber);
                }
                else
                {
                    results = await _productionOrderService.GetAllProductionOrdersAsync(roleid);
                }

                _logger.LogInformation("Found {Count} Production Orders", results.Count);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all Production Orders");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }


        /// <summary>
        /// Delete Production Order by PO Number and ID Number
        /// </summary>
        [Authorize]
        [HttpPost("DeleteProductionOrder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteProductionOrder([FromBody] DeleteProductionOrderRequestDto request)
        {
            _logger.LogInformation("Request received for ProductionOrderController:DeleteProductionOrder PO: {PO}, IDNumber: {IDNumber}",
                request.ProductionOrderNumber, request.IdNumber);

            if (string.IsNullOrWhiteSpace(request.ProductionOrderNumber) || request.IdNumber <= 0)
            {
                return BadRequest(new { message = "Valid Production Order Number and ID Number are required" });
            }
            try
            {
                var isDeleted = await _productionOrderService.DeleteProductionOrderAsync(request);
                if (!isDeleted)
                {
                    return NotFound(new { message = $"Production Order '{request.ProductionOrderNumber}' with ID '{request.IdNumber}' not found or already inactive" });
                }

                _logger.LogInformation("Successfully deleted Production Order: {PO}, IDNumber: {IDNumber}",
                    request.ProductionOrderNumber, request.IdNumber);
                return Ok(new { message = $"Production Order '{request.ProductionOrderNumber}' deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Production Order");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }



    }
}
