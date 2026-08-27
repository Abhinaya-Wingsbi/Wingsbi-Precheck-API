using System.ComponentModel.DataAnnotations;
using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DTOs.Assembly;
using Godrej.Precheck.Models.DTOs.ComponentType;
using Godrej.Precheck.Models.DTOs.DocumentType;
using Godrej.Precheck.Models.DTOs.DrawingNumber;
using Godrej.Precheck.Models.DTOs.IRNumber;
using Godrej.Precheck.Models.DTOs.MSNNumber;
using Godrej.Precheck.Models.DTOs.Precheck;
using Godrej.Precheck.Models.DTOs.ProductionSeries;
using Godrej.Precheck.Models.DTOs.Stage;
using Godrej.Precheck.Service.Service.CommonSevice;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Godrej.Precheck.Host.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CommonController : ControllerBase
    {
        private readonly ILogger<CommonController> _logger;
        private readonly ICommonService _commonService;

        public CommonController(ILogger<CommonController> logger, ICommonService commonService)
        {
            _logger = logger;
            _commonService = commonService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetAllModules")]
        public async Task<IActionResult> GetAllModuleAsync()
        {
            try
            {
                _logger.LogInformation("Request for CommonController:GetAllModules method");

                var result = await _commonService.GetAllModules();

                if (result == null)
                {
                    _logger.LogInformation($"Response for CommonController:GetAllModules method:No modules found.");

                    return NotFound();
                }

                var precheckModuleResponse = result.Adapt<List<ModulesInfo>>();

                _logger.LogInformation($"Response for CommonController:GetAllModules method: {precheckModuleResponse}");
                return Ok(precheckModuleResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetAllModules: {ex}");
                return BadRequest(ex);

            }
        }

        //Get componenttypes

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetAllComponenttype")]
        public async Task<IActionResult> GetAllComponenttype()
        {
            try
            {
                _logger.LogInformation("Request for CommonController:GetAllComponenttype method");

                var result = await _commonService.ComponentTypeService();

                if (result == null)
                {
                    _logger.LogInformation("Response for CommonController:GetAllComponenttype method: No Componenttype found.");

                    return NotFound();
                }

                var componentTypeResponse = result.Adapt<List<ComponentTypeResponseDto>>();

                _logger.LogInformation($"Response for CommonController:GetAllComponenttype method: {componentTypeResponse}");
                return Ok(componentTypeResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetAllComponenttype: {ex}");
                return BadRequest(ex);

            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetAllDrawingNumber")]
        public async Task<IActionResult> GetDrawingNumberAsync([FromQuery]GetAllDrawingRequestDto? request)
        {
            try
            {
                _logger.LogInformation($"Request for CommonController:GetDrawingNumberAsync method {request}");

   
                 var result = await _commonService.GetAllDrawingNumberService(request);

                if (result == null)
                {
                    _logger.LogInformation("Response for CommonController:GetDrawingNumberAsync method: No DrawingNumber found.");

                    return NotFound();
                }

                //var DrawingNumberResponse = result.Adapt<DrawingNumberResponseDto>();

                _logger.LogInformation($"Response for CommonController:GetDrawingNumberAsync method: {result}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetDrawingNumberAsync: {ex}");
                return BadRequest(ex);

            }
        }


        //Fetch all Drawing Number Witoutany paramter

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("FetchAllDrawingNumbers")]
        public async Task<IActionResult> GetAllDrawingNumberAsync()
        {
            try
            {
                _logger.LogInformation("Request for CommonController:GetAllDrawingNumberAsync method (no parameters)");

                var result = await _commonService.GetAllDrawingNumberService(null);

                if (result == null || result.Count == 0)
                {
                    _logger.LogInformation("No DrawingNumber found.");
                    return NotFound();
                }

                _logger.LogInformation($"Retrieved {result.Count} drawing numbers.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetAllDrawingNumberAsync: {ex}");
                return BadRequest(ex);
            }
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetAllDocumentType")]
        public async Task<IActionResult> GetAllDocumnetTypeAsync()
        {
            try
            {
                _logger.LogInformation("Request for CommonController:GetDocumnetTypeAsync method");

                var result = await _commonService.DocumentTypeService();

                if (result == null)
                {
                    _logger.LogInformation("Response for CommonController:GetDocumnetTypeAsync method: No DocumnetType found.");

                    return NotFound();
                }

                var DocumentmentTypeResponse = result.Adapt<List<DocumentTypeResponseDto>>();

                _logger.LogInformation($"Response for CommonController:GetDocumnetTypeAsync method: {DocumentmentTypeResponse}");
                return Ok(DocumentmentTypeResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetDocumnetTypeAsync: {ex}");
                return BadRequest(ex);

            }
        }

        //GetComponentTypeByName
        [NonAction]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetComponenttypeByName")]
        public async Task<IActionResult> GetComponenttypeByName([FromQuery] string componentType)
        {
            try
            {
                _logger.LogInformation($"Request for CommonController:GetComponenttypeByName method {componentType}");

                var result = await _commonService.ComponentTypeByNameService(componentType);

                if (result == null)
                {
                    _logger.LogInformation("Response for CommonController:GetComponenttypeByName method: No Componenttype found.");

                    return NotFound();
                }

                var componentTypeResponse = result.Adapt<ComponentTypeResponseDto>();

                _logger.LogInformation($"Response for CommonController:GetComponenttypeByName method: {componentTypeResponse}");
                return Ok(componentTypeResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetComponenttypeByName: {ex}");
                return BadRequest(ex);

            }
        }


        //GetComponentTypeById
        [NonAction]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetComponenttypeById")]
        public async Task<IActionResult> GetComponenttypeById([FromQuery] int Id)
        {
            try
            {
                _logger.LogInformation($"Request for CommonController:GetComponenttypeById method {Id}");

                var result = await _commonService.ComponentTypeByIdService(Id);

                if (result == null)
                {
                    _logger.LogInformation("Response for CommonController:GetComponenttypeById method: No Componenttype found.");

                    return NotFound();
                }

                var componentTypeResponse = result.Adapt<ComponentTypeResponseDto>();

                _logger.LogInformation($"Response for CommonController:GetComponenttypeById method: {componentTypeResponse}");
                return Ok(componentTypeResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController: GetComponenttypeById: {ex}");
                return BadRequest(ex);

            }
        }


        //Get Prod Series
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetAllProductionSeries")]
        public async Task<IActionResult> GetAllProductionSeries()
        {
            try
            {
                _logger.LogInformation("Request for CommonController:GetAllProductionSeries method");

                var result = await _commonService.ProductionSeriesService();

                if (result == null || !result.Any())
                {
                    _logger.LogInformation("Response for CommonController:GetAllProductionSeries method: No Componenttype found.");

                    return NotFound();
                }

                var productionSeriesResponse = result.Adapt<List<ProductionSeriesResponseDto>>();

                _logger.LogInformation($"Response for CommonController:GetAllProductionSeries method: {productionSeriesResponse}");
                return Ok(productionSeriesResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetAllProductionSeries: {ex}");
                return BadRequest(ex);

            }
        }

        [NonAction]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetProductionSeriesByName")]
        public async Task<IActionResult> GetProductionSeriesByName([FromQuery] string query)
        {
            try
            {
                _logger.LogInformation($"Request for CommonController:GetProductionSeriesByName method: {query}");

                var result = await _commonService.ProductionSeriesByNameService(query);

                if (result == null)
                {
                    _logger.LogInformation("Response for CommonController:GetProductionSeriesByName method: No ProductionSeries found.");

                    return NotFound();
                }

                var productionSeriesResponse = result.Adapt<ProductionSeriesResponseDto>();

                _logger.LogInformation($"Response for CommonController:GetAllProductionSeries method: {productionSeriesResponse}");
                return Ok(productionSeriesResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetAllProductionSeries: {ex}");
                return BadRequest(ex);

            }
        }

        //GetProdSeriesById
        [NonAction]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetProductionSeriesById")]
        public async Task<IActionResult> GetProductionSeriesById([FromQuery] int Id)
        {
            try
            {
                _logger.LogInformation($"Request for CommonController:GetProductionSeriesById method:{Id}");

                var result = await _commonService.ProductionSeriesByIdService(Id);

                if (result == null)
                {
                    _logger.LogInformation("Response for CommonController:GetProductionSeriesById method: No ProductionSeries found.");

                    return NotFound();
                }

                var productionSeriesResponse = result.Adapt<ProductionSeriesResponseDto>();

                _logger.LogInformation($"Response for CommonController:GetProductionSeriesById method: {productionSeriesResponse}");
                return Ok(productionSeriesResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetProductionSeriesById: {ex}");
                return BadRequest(ex);

            }
        }

      
       

        [NonAction]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetDocumentTypeByName")]
        public async Task<IActionResult> GetDocumnetTypeByName([FromQuery] string query)
        {
            try
            {
                _logger.LogInformation($"Request for CommonController:GetDocumnetTypeByName method: {query}");

                var result = await _commonService.DocumentTypeByNameService(query);

                if (result == null)
                {
                    _logger.LogInformation("Response for CommonController:GetDocumnetTypeByName method: No DocumnetType found.");

                    return NotFound();
                }

                _logger.LogInformation($"Response for CommonController:GetDocumnetTypeByName method: {result}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetDocumnetTypeByName: {ex}");
                return BadRequest(ex);

            }
        }

        [NonAction]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetNomenclatureByName")]
        public async Task<IActionResult> GetNomenclature([FromQuery] string query)
        {
            try
            {
                _logger.LogInformation($"Request for CommonController:GetNomenclature method: {query}");

                var result = await _commonService.NomenclatureService(query);

                if (result == null)
                {
                    _logger.LogInformation("Response for CommonController:GetNomenclature method: No Nomenclature found.");

                    return NotFound();
                }

                _logger.LogInformation($"Response for CommonController:GetNomenclature method: {result}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetNomenclature method {ex}");
                return BadRequest(ex);

            }
        }

       

        [NonAction]
        //GetProductionOrderByName
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetProductionOrderByName")]
        public async Task<IActionResult> GetProductionOrderByName([FromQuery] string ProductionOrder)
        {
            try
            {
                _logger.LogInformation($"Request for CommonController:GetProductionOrderByName method:{ProductionOrder}");

                var result = await _commonService.ProductionOrderByNameService(ProductionOrder);

                if (result == null)
                {
                    _logger.LogInformation("Response for CommonController:GetProductionOrderByName method: No ProductionOrder found.");

                    return NotFound();
                }

                _logger.LogInformation($"Response for CommonController:GetProductionOrderByName method: {result}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetProductionOrderByName: {ex}");
                return BadRequest(ex);

            }
        }

        //GetUnitByName
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetAllUnit")]
        public async Task<IActionResult> GetAllUnit()
        {
            try
            {
                _logger.LogInformation("Request for CommonController:GetUnitByName method");

                var result = await _commonService.UnitByName();

                if (result == null)
                {
                    _logger.LogInformation("Response for CommonController:GetUnitByName method: No Unit found.");

                    return NotFound();
                }

                _logger.LogInformation($"Response for CommonController:GetUnitByName method: {result}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetUnitByName: {ex}");
                return BadRequest(ex);

            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetAllShapes")]
        public async Task<IActionResult> GetAllShapes()
        {
            try
            {
                _logger.LogInformation("Request for CommonController:GetAllShapes method");

                var result = await _commonService.GetAllShapes();

                if (result == null || !result.Any())
                {
                    _logger.LogInformation("Response for CommonController:GetAllShapes method: No Shapes found.");

                    return NotFound();
                }

                _logger.LogInformation($"Response for CommonController:GetAllShapes method: Retrieved {result.Count} shapes");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetAllShapes: {ex}");
                return BadRequest(ex);

            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetAllAssemblyDrawingMappings")]
        public async Task<IActionResult> GetAllAssemblyDrawingMappings([FromBody] GetAssemblyDrawingMappingRequestDto request)
        {
            try
            {
                _logger.LogInformation("Request for CommonController:GetAllAssemblyDrawingMappings method, LnItemCode: '{LnItemCode}'", request.LnItemCode);

                var result = await _commonService.GetAllAssemblyDrawingMappingsAsync(request.LnItemCode);

                if (result == null || !result.Any())
                {
                    _logger.LogInformation("Response for CommonController:GetAllAssemblyDrawingMappings method: No mappings found.");
                    return NotFound();
                }

                _logger.LogInformation("Response for CommonController:GetAllAssemblyDrawingMappings method: Retrieved {Count} mappings", result.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetAllAssemblyDrawingMappings: {ex}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ReassignParentDrawing")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReassignParentDrawing([FromBody] ReassignParentDrawingRequestDto request)
        {
            try
            {
                _logger.LogInformation("Request for CommonController:ReassignParentDrawing, ChildLnItemCode: {ChildLnItemCode}, ParentLnItemCode: {ParentLnItemCode}",
                    request.DrawingNumberLnitemcode, request.ParentDrawingNumberLnitemcode);

                var modifiedBy = Convert.ToInt32(User.FindFirst("id")?.Value);

                var updatedRecordId = await _commonService.ReassignParentDrawingAsync(request, modifiedBy);

                _logger.LogInformation("Response for CommonController:ReassignParentDrawing: UpdatedRecordId {UpdatedRecordId}", updatedRecordId);
                return Ok(new
                {
                    statusCode = StatusCodes.Status200OK,
                    message    = "Assembly drawing mapping updated successfully.",
                    updatedRecordId
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "ReassignParentDrawing business error: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception Error for CommonController:ReassignParentDrawing");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("RemoveChildDrawing")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveChildDrawing([FromBody] RemoveChildDrawingRequestDto request)
        {
            try
            {
                _logger.LogInformation("Request for CommonController:RemoveChildDrawing, Assembly: {Assembly}, Child: {Child}",
                    request.AssemblyDrawingNumber, request.ChildDrawingNumber);

                var modifiedBy = Convert.ToInt32(User.FindFirst("id")?.Value);

                var removedId = await _commonService.RemoveChildDrawingAsync(request, modifiedBy);

                _logger.LogInformation("Response for CommonController:RemoveChildDrawing: RemovedRecordId {Id}", removedId);
                return Ok(new
                {
                    statusCode = StatusCodes.Status200OK,
                    message    = "Child drawing removed from assembly successfully.",
                    removedRecordId = removedId
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "RemoveChildDrawing business error: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception Error for CommonController:RemoveChildDrawing");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("DeleteDrawingNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteDrawingNumber([FromBody] DeleteDrawingNumberRequestDto request)
        {
            try
            {
                _logger.LogInformation("Request for CommonController:DeleteDrawingNumber, Drawing: {Drawing}, LnItemCode: {LnItemCode}",
                    request.DrawingNumber, request.LnItemCode);

                var modifiedBy = Convert.ToInt32(User.FindFirst("id")?.Value);

                var deletedId = await _commonService.DeleteDrawingNumberAsync(request, modifiedBy);

                _logger.LogInformation("Response for CommonController:DeleteDrawingNumber: DeletedRecordId {Id}", deletedId);
                return Ok(new
                {
                    statusCode = StatusCodes.Status200OK,
                    message    = "Drawing number deleted successfully.",
                    deletedRecordId = deletedId
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "DeleteDrawingNumber business error: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception Error for CommonController:DeleteDrawingNumber");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("AddAssemblyDrawingMapping")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAssemblyDrawingMapping([FromBody] AddAssemblyDrawingMappingRequestDto request)
        {
            try
            {
                _logger.LogInformation("Request for CommonController:AddAssemblyDrawingMapping, Drawing: {Drawing}, Parent: {Parent}",
                    request.DrawingNumber, request.ParentDrawingNumber);

                var createdBy = Convert.ToInt32(User.FindFirst("id")?.Value);

                var newId = await _commonService.AddAssemblyDrawingMappingAsync(request, createdBy);

                _logger.LogInformation("Response for CommonController:AddAssemblyDrawingMapping: NewId {Id}", newId);
                return Ok(new
                {
                    statusCode = StatusCodes.Status200OK,
                    message    = "Assembly drawing mapping added successfully.",
                    newRecordId = newId
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "AddAssemblyDrawingMapping business error: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception Error for CommonController:AddAssemblyDrawingMapping");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        //Get All Assembly
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetAllAssembly")]
        public async Task<IActionResult> GetAllAssembly()
        {
            try
            {
                _logger.LogInformation("Request for CommonController:GetAllAssembly method");

                var result = await _commonService.GetAllAssembly();

                if (result == null || !result.Any())
                {
                    _logger.LogInformation("Response for CommonController:GetAllAssembly method: No Assembly found.");

                    return NotFound();
                }

                var assemblyResponse = result.Adapt<List<AssemblyResponseDto>>();

                _logger.LogInformation($"Response for CommonController:GetAllAssembly method: Retrieved {assemblyResponse.Count} assemblies");
                return Ok(assemblyResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetAllAssembly: {ex}");
                return BadRequest(ex.Message);

            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetIRStages")]
        public async Task<IActionResult> GetIRStagesAsync()
        {
            try
            {
                _logger.LogInformation("Request for CommonController:GetIRStagesAsync method");

                var result = await _commonService.GetIRStagesService();

                if (result == null || !result.Any())
                {
                    _logger.LogInformation("Response for CommonController:GetIRStagesAsync method: No IR stages found.");
                    return NotFound();
                }

                var stageResponse = result.Adapt<List<StageResponseDto>>();

                _logger.LogInformation($"Response for CommonController:GetIRStagesAsync method: Retrieved {stageResponse.Count} IR stages");
                return Ok(stageResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetIRStagesAsync: {ex}");
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetMSNStages")]
        public async Task<IActionResult> GetMSNStagesAsync()
        {
            try
            {
                _logger.LogInformation("Request for CommonController:GetMSNStagesAsync method");

                var result = await _commonService.GetMSNStagesService();

                if (result == null || !result.Any())
                {
                    _logger.LogInformation("Response for CommonController:GetMSNStagesAsync method: No MSN stages found.");
                    return NotFound();
                }

                var stageResponse = result.Adapt<List<StageResponseDto>>();

                _logger.LogInformation($"Response for CommonController:GetMSNStagesAsync method: Retrieved {stageResponse.Count} MSN stages");
                return Ok(stageResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetMSNStagesAsync: {ex}");
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("GetAllLnItemCode")]
        public async Task<IActionResult> GetAllLnItemCode([FromQuery] string search = null)
        {
            try
            {
                _logger.LogInformation($"Request for CommonController:GetAllLnItemCode with search: '{search}'");

                var result = await _commonService.GetAllLnItemCode(search);

                if (result == null || result.Count == 0)
                {
                    _logger.LogInformation("Response for CommonController:GetAllLnItemCode: No LnItemCode found.");
                    return Ok(new List<string>());
                }

                _logger.LogInformation($"Response for CommonController:GetAllLnItemCode: Retrieved {result.Count} codes");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception Error for CommonController:GetAllLnItemCode: {ex}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
