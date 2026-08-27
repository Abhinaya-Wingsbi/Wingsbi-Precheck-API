using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DTOs.DrawingNumber;
using Godrej.Precheck.Repository.Repository.DrawingNumberRepository;
using Godrej.Precheck.Service.Cache;
using Godrej.Precheck.Service.Service.CommonSevice;
using Microsoft.Extensions.Logging;

namespace Godrej.Precheck.Service.Service.DrawingNumberService
{
    public class DrawingNumberService : IDrawingNumberService
    {
        private readonly ILogger<DrawingNumberService> _logger;
        private readonly IDrawingNumberRepository _drawingNumberRepository;
        private readonly ICommonService _commonService;
        private readonly ICacheService _cacheService;

        public DrawingNumberService(
            ILogger<DrawingNumberService> logger,
            IDrawingNumberRepository drawingNumberRepository,
            ICommonService commonService,
            ICacheService cacheService)
        {
            _logger = logger;
            _drawingNumberRepository = drawingNumberRepository;
            _commonService = commonService;
            _cacheService = cacheService;
        }

        private static bool ParseYesNo(string? value) =>
            string.Equals(value?.Trim(), "Yes", StringComparison.OrdinalIgnoreCase);

        private static bool ParseActiveStatus(string? value) =>
            string.IsNullOrWhiteSpace(value) || string.Equals(value.Trim(), "active", StringComparison.OrdinalIgnoreCase);

        public async Task<DrawingMappingResponseDto> InsertDrawingMappingsAsync(InsertDrawingMappingDto request)
        {
            var drawingNumberId = request.DrawingNumberId ?? 0;
            _logger.LogInformation($"Processing drawing mappings for DrawingNumberId: {drawingNumberId}, DrawingNumber: {request.DrawingNumber}");

            try
            {
                var response = new DrawingMappingResponseDto
                {
                    DrawingNumberId = drawingNumberId,
                    Success = true,
                    Details = new MappingDetails()
                };

                // Get Indian Standard Time
                var indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var createdDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);
                var createdBy = request.CreatedBy ?? 0;

                // Track if any master table was changed (to determine if cache should be cleared)
                bool masterTableChanged = false;

                var isExpiry = ParseYesNo(request.HasExpiry);
                var isActive = ParseActiveStatus(request.Status);

                // Step 1: Resolve the drawing number itself - update if it exists, insert if it doesn't
                if (drawingNumberId > 0)
                {
                    var drawingExists = await _drawingNumberRepository.CheckDrawingNumberExists(drawingNumberId);
                    if (!drawingExists)
                    {
                        _logger.LogWarning($"Drawing number ID {drawingNumberId} does not exist");
                        return new DrawingMappingResponseDto
                        {
                            DrawingNumberId = drawingNumberId,
                            Success = false,
                            Message = "Drawing number does not exist or is inactive",
                            Details = new MappingDetails()
                        };
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(request.DrawingNumber) || string.IsNullOrWhiteSpace(request.LnItemCode))
                    {
                        return new DrawingMappingResponseDto
                        {
                            DrawingNumberId = 0,
                            Success = false,
                            Message = "DrawingNumber and LnItemCode are required when DrawingNumberId is not provided.",
                            Details = new MappingDetails()
                        };
                    }

                    var existingDrawingNumberId = await _drawingNumberRepository.GetDrawingNumberIdByDrawingAndLnItemCode(
                        request.DrawingNumber, request.LnItemCode);

                    if (existingDrawingNumberId.HasValue)
                    {
                        drawingNumberId = existingDrawingNumberId.Value;
                        await _drawingNumberRepository.UpdateDrawingNumberCore(
                            drawingNumberId, isExpiry, isActive, createdBy, createdDate);
                        response.Details.DrawingNumberUpdated = true;
                        masterTableChanged = true;
                        _logger.LogInformation($"Updated existing drawing number. DrawingNumberId: {drawingNumberId}");
                    }
                    else
                    {
                        drawingNumberId = await _drawingNumberRepository.InsertDrawingNumber(
                            request.DrawingNumber, request.LnItemCode, isExpiry, isActive, createdBy, createdDate);
                        response.Details.DrawingNumberCreated = true;
                        masterTableChanged = true;
                        _logger.LogInformation($"Created new drawing number. DrawingNumberId: {drawingNumberId}");
                    }

                    response.DrawingNumberId = drawingNumberId;
                }

                // Step 2: Process LnItemCode and RackLocation (they go together in mapping)
                if (!string.IsNullOrWhiteSpace(request.LnItemCode) || !string.IsNullOrWhiteSpace(request.RackLocation))
                {
                    _logger.LogInformation($"Processing LnItemCode: {request.LnItemCode} and RackLocation: {request.RackLocation}");

                    // Get existing master IDs from mapping
                    var existingLnItemCodeId = await _drawingNumberRepository.GetLnItemCodeIdFromMapping(drawingNumberId);
                    var existingRackLocationId = await _drawingNumberRepository.GetRackLocationIdFromMapping(drawingNumberId);

                    if (existingLnItemCodeId.HasValue && existingRackLocationId.HasValue)
                    {
                        // Mapping exists - resolve (get-or-create by name) the master rows, then re-point
                        // only this drawing's own mapping row. tbl_lnitemcode/tbl_storeitemlocation rows are
                        // shared across many drawings, so never mutate the existing master row's text in place.
                        var resolvedLnItemCodeId = !string.IsNullOrWhiteSpace(request.LnItemCode)
                            ? await _drawingNumberRepository.InsertLnItemCode(request.LnItemCode, request.LnItemNomenclature ?? string.Empty, createdBy, createdDate)
                            : existingLnItemCodeId.Value;
                        var resolvedRackLocationId = !string.IsNullOrWhiteSpace(request.RackLocation)
                            ? await _drawingNumberRepository.InsertRackLocation(request.RackLocation, createdBy, createdDate)
                            : existingRackLocationId.Value;

                        if (resolvedLnItemCodeId != existingLnItemCodeId.Value || resolvedRackLocationId != existingRackLocationId.Value)
                        {
                            await _drawingNumberRepository.UpdateLnItemLocationMapping(drawingNumberId, resolvedLnItemCodeId, resolvedRackLocationId);
                        }

                        response.Details.LnItemCodeId = resolvedLnItemCodeId;
                        response.Details.RackLocationId = resolvedRackLocationId;
                        response.Details.LnItemLocationMappingUpdated = true;
                        masterTableChanged = true;
                        _logger.LogInformation($"Updated LnItem-Location mapping for DrawingNumberId: {drawingNumberId}");
                    }
                    else if (!string.IsNullOrWhiteSpace(request.LnItemCode) && !string.IsNullOrWhiteSpace(request.RackLocation))
                    {
                        // No mapping exists - INSERT new master entries and mapping
                        var newLnItemCodeId = await _drawingNumberRepository.InsertLnItemCode(
                            request.LnItemCode,
                            request.LnItemNomenclature ?? string.Empty,
                            createdBy,
                            createdDate);
                        var newRackLocationId = await _drawingNumberRepository.InsertRackLocation(
                            request.RackLocation,
                            createdBy,
                            createdDate);

                        await _drawingNumberRepository.InsertLnItemLocationMapping(
                            drawingNumberId,
                            newLnItemCodeId,
                            newRackLocationId,
                            createdBy,
                            createdDate);

                        response.Details.LnItemCodeId = newLnItemCodeId;
                        response.Details.RackLocationId = newRackLocationId;
                        response.Details.LnItemLocationMappingCreated = true;
                        masterTableChanged = true;
                        _logger.LogInformation($"Created LnItem-Location master and mapping for DrawingNumberId: {drawingNumberId}");
                    }
                }

                // Step 3: Process Nomenclature
                if (!string.IsNullOrWhiteSpace(request.Nomenclature))
                {
                    _logger.LogInformation($"Processing Nomenclature: {request.Nomenclature}");

                    // Get existing master ID from mapping
                    var existingNomenclatureId = await _drawingNumberRepository.GetNomenclatureIdFromMapping(drawingNumberId);

                    // Resolve (get-or-create by name) the master row - tbl_nomenclature rows are shared
                    // across many drawings, so never mutate an existing master row's text in place.
                    var resolvedNomenclatureId = await _drawingNumberRepository.InsertNomenclature(
                        request.Nomenclature,
                        createdBy,
                        createdDate);

                    if (existingNomenclatureId.HasValue)
                    {
                        if (resolvedNomenclatureId != existingNomenclatureId.Value)
                        {
                            await _drawingNumberRepository.UpdateNomenclatureMapping(drawingNumberId, resolvedNomenclatureId);
                        }
                        response.Details.NomenclatureId = resolvedNomenclatureId;
                        response.Details.NomenclatureMappingUpdated = true;
                        masterTableChanged = true;
                        _logger.LogInformation($"Updated Nomenclature mapping (id={resolvedNomenclatureId}) for DrawingNumberId: {drawingNumberId}");
                    }
                    else
                    {
                        await _drawingNumberRepository.InsertNomenclatureMapping(
                            drawingNumberId,
                            resolvedNomenclatureId,
                            createdBy,
                            createdDate);

                        response.Details.NomenclatureId = resolvedNomenclatureId;
                        response.Details.NomenclatureMappingCreated = true;
                        masterTableChanged = true;
                        _logger.LogInformation($"Created Nomenclature master and mapping for DrawingNumberId: {drawingNumberId}");
                    }
                }

                // Step 4: Process ComponentType
                if (!string.IsNullOrWhiteSpace(request.ComponentType))
                {
                    _logger.LogInformation($"Processing ComponentType: {request.ComponentType}");

                    // Get existing master ID from mapping
                    var existingComponentTypeId = await _drawingNumberRepository.GetComponentTypeIdFromMapping(drawingNumberId);

                    // Resolve (get-or-create by name) the master row - tbl_componenttype rows are shared
                    // across many drawings, so never mutate an existing master row's text in place.
                    var resolvedComponentTypeId = await _drawingNumberRepository.InsertComponentType(
                        request.ComponentType,
                        createdBy,
                        createdDate);

                    if (existingComponentTypeId.HasValue)
                    {
                        if (resolvedComponentTypeId != existingComponentTypeId.Value)
                        {
                            await _drawingNumberRepository.UpdateComponentTypeMapping(drawingNumberId, resolvedComponentTypeId);
                        }
                        response.Details.ComponentTypeId = resolvedComponentTypeId;
                        response.Details.ComponentTypeMappingUpdated = true;
                        masterTableChanged = true;
                        _logger.LogInformation($"Updated ComponentType mapping (id={resolvedComponentTypeId}) for DrawingNumberId: {drawingNumberId}");
                    }
                    else
                    {
                        await _drawingNumberRepository.InsertComponentTypeMapping(
                            drawingNumberId,
                            resolvedComponentTypeId,
                            createdBy,
                            createdDate);

                        response.Details.ComponentTypeId = resolvedComponentTypeId;
                        response.Details.ComponentTypeMappingCreated = true;
                        masterTableChanged = true;
                        _logger.LogInformation($"Created ComponentType master and mapping for DrawingNumberId: {drawingNumberId}");
                    }
                }

                // Step 5: Process DocumentType
                if (!string.IsNullOrWhiteSpace(request.DocumentType))
                {
                    _logger.LogInformation($"Processing DocumentType: {request.DocumentType}");

                    // Get existing master ID from mapping
                    var existingDocumentTypeId = await _drawingNumberRepository.GetDocumentTypeIdFromMapping(drawingNumberId);

                    // Resolve (get-or-create by name) the master row - tbl_documenttype rows are shared
                    // across many drawings, so never mutate an existing master row's text in place.
                    var resolvedDocumentTypeId = await _drawingNumberRepository.InsertDocumentType(
                        request.DocumentType,
                        createdBy,
                        createdDate);

                    if (existingDocumentTypeId.HasValue)
                    {
                        if (resolvedDocumentTypeId != existingDocumentTypeId.Value)
                        {
                            await _drawingNumberRepository.UpdateDocumentTypeMapping(drawingNumberId, resolvedDocumentTypeId);
                        }
                        response.Details.DocumentTypeId = resolvedDocumentTypeId;
                        response.Details.DocumentTypeMappingUpdated = true;
                        masterTableChanged = true;
                        _logger.LogInformation($"Updated DocumentType mapping (id={resolvedDocumentTypeId}) for DrawingNumberId: {drawingNumberId}");
                    }
                    else
                    {
                        await _drawingNumberRepository.InsertDocumentTypeMapping(
                            drawingNumberId,
                            resolvedDocumentTypeId,
                            createdBy,
                            createdDate);

                        response.Details.DocumentTypeId = resolvedDocumentTypeId;
                        response.Details.DocumentTypeMappingCreated = true;
                        masterTableChanged = true;
                        _logger.LogInformation($"Created DocumentType master and mapping for DrawingNumberId: {drawingNumberId}");
                    }
                }

                // Step 6: Process Unit
                if (!string.IsNullOrWhiteSpace(request.UnitName))
                {
                    _logger.LogInformation($"Processing Unit: {request.UnitName}");

                    // Get existing master ID from mapping
                    var existingUnitId = await _drawingNumberRepository.GetUnitIdFromMapping(drawingNumberId);

                    // Resolve (get-or-create by name) the master row - tbl_unit rows are shared
                    // across many drawings, so never mutate an existing master row's text in place.
                    var resolvedUnitId = await _drawingNumberRepository.InsertUnit(
                        request.UnitName,
                        createdBy,
                        createdDate);

                    if (existingUnitId.HasValue)
                    {
                        if (resolvedUnitId != existingUnitId.Value)
                        {
                            await _drawingNumberRepository.UpdateUnitMapping(drawingNumberId, resolvedUnitId);
                        }
                        response.Details.UnitId = resolvedUnitId;
                        response.Details.UnitMappingUpdated = true;
                        masterTableChanged = true;
                        _logger.LogInformation($"Updated Unit mapping (id={resolvedUnitId}) for DrawingNumberId: {drawingNumberId}");
                    }
                    else
                    {
                        await _drawingNumberRepository.InsertUnitMapping(
                            drawingNumberId,
                            resolvedUnitId,
                            createdBy,
                            createdDate);

                        response.Details.UnitId = resolvedUnitId;
                        response.Details.UnitMappingCreated = true;
                        masterTableChanged = true;
                        _logger.LogInformation($"Created Unit master and mapping for DrawingNumberId: {drawingNumberId}");
                    }
                }

                // Step 7: Process ProdSeries (AvailableFor)
                if (!string.IsNullOrWhiteSpace(request.AvailableFor))
                {
                    _logger.LogInformation($"Processing ProdSeries: {request.AvailableFor}");

                    // Get existing master ID from mapping
                    var existingProdSeriesId = await _drawingNumberRepository.GetProdSeriesIdFromMapping(drawingNumberId);

                    // Resolve (get-or-create by name) the master row - tbl_productionseries rows are shared
                    // across many drawings, so never mutate an existing master row's text in place.
                    var resolvedProdSeriesId = await _drawingNumberRepository.InsertOrGetProdSeries(
                        request.AvailableFor,
                        createdBy,
                        createdDate);

                    if (existingProdSeriesId.HasValue)
                    {
                        if (resolvedProdSeriesId != existingProdSeriesId.Value)
                        {
                            await _drawingNumberRepository.UpdateProdSeriesMapping(drawingNumberId, resolvedProdSeriesId);
                        }
                        response.Details.ProdSeriesId = resolvedProdSeriesId;
                        response.Details.ProdSeriesMappingUpdated = true;
                        masterTableChanged = true;
                        _logger.LogInformation($"Updated ProdSeries mapping (id={resolvedProdSeriesId}) for DrawingNumberId: {drawingNumberId}");
                    }
                    else
                    {
                        await _drawingNumberRepository.InsertProdSeriesMapping(
                            drawingNumberId,
                            resolvedProdSeriesId,
                            createdBy,
                            createdDate);

                        response.Details.ProdSeriesId = resolvedProdSeriesId;
                        response.Details.ProdSeriesMappingCreated = true;
                        masterTableChanged = true;
                        _logger.LogInformation($"Created ProdSeries master and mapping for DrawingNumberId: {drawingNumberId}");
                    }
                }

                // Step 8: Process ParentDrawingNumber (tbl_assemblydrawingmapping - child/parent link)
                if (!string.IsNullOrWhiteSpace(request.ParentDrawingNumber))
                {
                    _logger.LogInformation($"Processing ParentDrawingNumber: {request.ParentDrawingNumber}");

                    var parentLookup = await _drawingNumberRepository.GetDrawingNumberLookupByDrawingNumber(request.ParentDrawingNumber);
                    if (parentLookup == null)
                    {
                        _logger.LogWarning($"Parent drawing number '{request.ParentDrawingNumber}' not found or inactive");
                        return new DrawingMappingResponseDto
                        {
                            DrawingNumberId = drawingNumberId,
                            Success = false,
                            Message = $"Parent drawing number '{request.ParentDrawingNumber}' not found or inactive.",
                            Details = response.Details
                        };
                    }

                    string? consumedProdSeriesId = null;
                    if (request.AvailableSeriesId != null && request.AvailableSeriesId.Count > 0)
                    {
                        var firstAvailableSeriesId = request.AvailableSeriesId[0];
                        consumedProdSeriesId = await _drawingNumberRepository.GetProdSeriesNameById(firstAvailableSeriesId);
                        if (consumedProdSeriesId == null)
                        {
                            _logger.LogWarning($"AvailableSeriesId '{firstAvailableSeriesId}' not found or inactive");
                            return new DrawingMappingResponseDto
                            {
                                DrawingNumberId = drawingNumberId,
                                Success = false,
                                Message = $"Production series with id '{firstAvailableSeriesId}' not found or inactive.",
                                Details = response.Details
                            };
                        }
                    }

                    var existingAssemblyMappingId = await _drawingNumberRepository.GetAssemblyDrawingMappingId(
                        drawingNumberId, parentLookup.Id);

                    var childLnItemCode = await _drawingNumberRepository.GetLnItemCodeByDrawingNumberId(drawingNumberId);
                    var childNomenclature = await _drawingNumberRepository.GetNomenclatureByDrawingNumberId(drawingNumberId);

                    if (existingAssemblyMappingId.HasValue)
                    {
                        await _drawingNumberRepository.UpdateAssemblyDrawingMapping(
                            existingAssemblyMappingId.Value,
                            request.Quantity,
                            request.UnitName,
                            request.FindNo,
                            consumedProdSeriesId,
                            childNomenclature,
                            createdBy,
                            createdDate);
                        response.Details.AssemblyDrawingMappingId = existingAssemblyMappingId.Value;
                        response.Details.AssemblyDrawingMappingUpdated = true;
                        masterTableChanged = true;
                        _logger.LogInformation($"Updated assembly drawing mapping (id={existingAssemblyMappingId.Value}) for DrawingNumberId: {drawingNumberId}");
                    }
                    else
                    {
                        var newAssemblyMappingId = await _drawingNumberRepository.InsertAssemblyDrawingMapping(
                            drawingNumberId,
                            parentLookup.Id,
                            request.Quantity,
                            request.UnitName,
                            request.FindNo,
                            consumedProdSeriesId,
                            childNomenclature,
                            parentLookup.LnItemCode,
                            childLnItemCode,
                            createdBy,
                            createdDate);
                        response.Details.AssemblyDrawingMappingId = newAssemblyMappingId;
                        response.Details.AssemblyDrawingMappingCreated = true;
                        masterTableChanged = true;
                        _logger.LogInformation($"Created assembly drawing mapping for DrawingNumberId: {drawingNumberId}");
                    }
                }

                // Step 9: Ensure tbl_drawing_lnitem_map has this drawing's (drawingnumber, lnitemcode) pair.
                // This table is separate from tbl_drawingnumber's own denormalized lnitemcode column - it's
                // what ProductionOrder/Upload actually queries to resolve a drawing from an item code, and
                // nothing else in this method keeps it in sync, so a drawing created/updated here without
                // this step would silently fail that upload's lookup.
                var lnItemCodeForMap = await _drawingNumberRepository.GetLnItemCodeByDrawingNumberId(drawingNumberId);
                var drawingNumberTextForMap = !string.IsNullOrWhiteSpace(request.DrawingNumber)
                    ? request.DrawingNumber
                    : await _drawingNumberRepository.GetDrawingNumberTextById(drawingNumberId);

                if (!string.IsNullOrWhiteSpace(drawingNumberTextForMap) && !string.IsNullOrWhiteSpace(lnItemCodeForMap))
                {
                    var mapExists = await _drawingNumberRepository.CheckDrawingLnItemMapExists(drawingNumberTextForMap, lnItemCodeForMap);
                    if (!mapExists)
                    {
                        await _drawingNumberRepository.InsertDrawingLnItemMap(drawingNumberTextForMap, lnItemCodeForMap, createdBy, createdDate);
                        response.Details.DrawingLnItemMapCreated = true;
                        masterTableChanged = true;
                        _logger.LogInformation($"Created tbl_drawing_lnitem_map entry for DrawingNumberId: {drawingNumberId} ({drawingNumberTextForMap} -> {lnItemCodeForMap})");
                    }
                }

                // Generate success message
                var createdMappings = new List<string>();
                if (response.Details.DrawingNumberCreated) createdMappings.Add("DrawingNumber");
                if (response.Details.LnItemLocationMappingCreated) createdMappings.Add("LnItem-Location");
                if (response.Details.NomenclatureMappingCreated) createdMappings.Add("Nomenclature");
                if (response.Details.ComponentTypeMappingCreated) createdMappings.Add("ComponentType");
                if (response.Details.DocumentTypeMappingCreated) createdMappings.Add("DocumentType");
                if (response.Details.UnitMappingCreated) createdMappings.Add("Unit");
                if (response.Details.ProdSeriesMappingCreated) createdMappings.Add("ProdSeries");
                if (response.Details.DrawingLnItemMapCreated) createdMappings.Add("DrawingLnItemMap");
                if (response.Details.AssemblyDrawingMappingCreated) createdMappings.Add("AssemblyDrawingMapping");

                var updatedMappings = new List<string>();
                if (response.Details.DrawingNumberUpdated) updatedMappings.Add("DrawingNumber");
                if (response.Details.LnItemLocationMappingUpdated) updatedMappings.Add("LnItem-Location");
                if (response.Details.NomenclatureMappingUpdated) updatedMappings.Add("Nomenclature");
                if (response.Details.ComponentTypeMappingUpdated) updatedMappings.Add("ComponentType");
                if (response.Details.DocumentTypeMappingUpdated) updatedMappings.Add("DocumentType");
                if (response.Details.UnitMappingUpdated) updatedMappings.Add("Unit");
                if (response.Details.ProdSeriesMappingUpdated) updatedMappings.Add("ProdSeries");
                if (response.Details.AssemblyDrawingMappingUpdated) updatedMappings.Add("AssemblyDrawingMapping");

                var statusMessages = new List<string>();
                if (createdMappings.Any())
                {
                    statusMessages.Add($"Created: {string.Join(", ", createdMappings)}");
                }
                if (updatedMappings.Any())
                {
                    statusMessages.Add($"Updated: {string.Join(", ", updatedMappings)}");
                }

                response.Message = statusMessages.Any()
                    ? string.Join(" | ", statusMessages)
                    : "No changes made. Items not found in master tables or mappings already exist.";

                // Clear the drawing numbers cache ONLY if master tables were actually changed
                if (masterTableChanged)
                {
                    _cacheService.Remove(CacheSettings.DrawingNumbersCacheKey);
                    _logger.LogInformation("Cleared DrawingNumbers cache after master table update");
                }

                _logger.LogInformation($"Successfully processed drawing mappings for DrawingNumberId: {drawingNumberId}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing drawing mappings for DrawingNumberId: {drawingNumberId}");
                return new DrawingMappingResponseDto
                {
                    DrawingNumberId = drawingNumberId,
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Details = new MappingDetails()
                };
            }
        }

        public async Task<GetDrawingMappingDto> GetDrawingMappingsAsync(int drawingNumberId)
        {
            _logger.LogInformation($"Getting drawing mappings for DrawingNumberId: {drawingNumberId}");

            try
            {
                // Get all drawing numbers and find the one matching the ID
                var allDrawings = await _commonService.GetAllDrawingNumberService(null);
                var drawing = allDrawings?.FirstOrDefault(d => d.Id == drawingNumberId);

                if (drawing == null)
                {
                    _logger.LogWarning($"Drawing number not found for ID: {drawingNumberId}");
                    return null;
                }

                var mapping = new GetDrawingMappingDto
                {
                    DrawingNumberId = drawing.Id,
                    DrawingNumber = drawing.DrawingNumber,
                    LnItemCode = drawing.LnItemCode,
                    Nomenclature = drawing.Nomenclature,
                    RackLocation = drawing.Location,
                    ComponentType = drawing.ComponentType,
                    DocumentType = drawing.DocumentType,
                    UnitName = drawing.UnitName
                };

                _logger.LogInformation($"Successfully retrieved mappings for DrawingNumberId: {drawingNumberId}");
                return mapping;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting drawing mappings for DrawingNumberId: {drawingNumberId}");
                throw;
            }
        }
    }
}

