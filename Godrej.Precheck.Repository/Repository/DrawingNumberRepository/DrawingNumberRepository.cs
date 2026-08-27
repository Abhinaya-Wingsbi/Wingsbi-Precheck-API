using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DTOs.DrawingNumber;
using Godrej.Precheck.Repository.Database;
using Godrej.Precheck.Repository.Queries;
using Microsoft.Extensions.Logging;

namespace Godrej.Precheck.Repository.Repository.DrawingNumberRepository
{
    public class DrawingNumberRepository : IDrawingNumberRepository
    {
        private readonly ILogger<DrawingNumberRepository> _logger;
        private readonly IApplicationDbContext _db;

        public DrawingNumberRepository(ILogger<DrawingNumberRepository> logger, IApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<bool> CheckDrawingNumberExists(int drawingNumberId)
        {
            _logger.LogInformation($"Checking if drawing number exists: {drawingNumberId}");
            try
            {
                var result = await _db.GetSingle<int?>(
                    DrawingNumberQueries.CHECK_DRAWING_NUMBER_EXISTS,
                    new { DrawingNumberId = drawingNumberId });
                return result.HasValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking drawing number existence for ID: {drawingNumberId}");
                throw;
            }
        }

        #region Drawing Number Upsert

        public async Task<int?> GetDrawingNumberIdByDrawingAndLnItemCode(string drawingNumber, string lnItemCode)
        {
            _logger.LogInformation($"Looking up drawing number: {drawingNumber}, LnItemCode: {lnItemCode}");
            try
            {
                var result = await _db.GetSingle<int?>(
                    DrawingNumberQueries.GET_DRAWINGNUMBER_ID_BY_DRAWING_AND_LNITEM,
                    new { DrawingNumber = drawingNumber, LnItemCode = lnItemCode });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error looking up drawing number: {drawingNumber}, LnItemCode: {lnItemCode}");
                throw;
            }
        }

        public async Task<int> InsertDrawingNumber(string drawingNumber, string lnItemCode, bool isExpiry, bool isActive, int createdBy, DateTime createdDate)
        {
            _logger.LogInformation($"Inserting drawing number: {drawingNumber}, LnItemCode: {lnItemCode}");
            try
            {
                var result = await _db.ExecuteScalar<int>(
                    DrawingNumberQueries.INSERT_DRAWING_NUMBER,
                    new
                    {
                        DrawingNumber = drawingNumber,
                        LnItemCode = lnItemCode,
                        IsExpiry = isExpiry,
                        IsActive = isActive,
                        CreatedBy = createdBy,
                        CreatedDate = createdDate
                    });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inserting drawing number: {drawingNumber}, LnItemCode: {lnItemCode}");
                throw;
            }
        }

        public async Task UpdateDrawingNumberCore(int id, bool isExpiry, bool isActive, int modifiedBy, DateTime modifiedDate)
        {
            _logger.LogInformation($"Updating drawing number core: Id={id}");
            try
            {
                await _db.Execute(
                    DrawingNumberQueries.UPDATE_DRAWING_NUMBER_CORE,
                    new { Id = id, IsExpiry = isExpiry, IsActive = isActive, ModifiedBy = modifiedBy, ModifiedDate = modifiedDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating drawing number core: Id={id}");
                throw;
            }
        }

        public async Task<DrawingNumberLookupDto?> GetDrawingNumberLookupByDrawingNumber(string drawingNumber)
        {
            _logger.LogInformation($"Looking up drawing number lookup: {drawingNumber}");
            try
            {
                var result = await _db.GetSingle<DrawingNumberLookupDto?>(
                    DrawingNumberQueries.GET_DRAWINGNUMBER_LOOKUP_BY_DRAWING_NUMBER,
                    new { DrawingNumber = drawingNumber });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error looking up drawing number: {drawingNumber}");
                throw;
            }
        }

        public async Task<string?> GetLnItemCodeByDrawingNumberId(int drawingNumberId)
        {
            _logger.LogInformation($"Looking up lnitemcode by DrawingNumberId: {drawingNumberId}");
            try
            {
                var result = await _db.GetSingle<string?>(
                    DrawingNumberQueries.GET_LNITEMCODE_BY_DRAWINGNUMBER_ID,
                    new { DrawingNumberId = drawingNumberId });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error looking up lnitemcode by DrawingNumberId: {drawingNumberId}");
                throw;
            }
        }

        public async Task<string?> GetNomenclatureByDrawingNumberId(int drawingNumberId)
        {
            _logger.LogInformation($"Looking up nomenclature by DrawingNumberId: {drawingNumberId}");
            try
            {
                var result = await _db.GetSingle<string?>(
                    DrawingNumberQueries.GET_NOMENCLATURE_BY_DRAWINGNUMBER_ID,
                    new { DrawingNumberId = drawingNumberId });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error looking up nomenclature by DrawingNumberId: {drawingNumberId}");
                throw;
            }
        }

        public async Task<string?> GetDrawingNumberTextById(int drawingNumberId)
        {
            _logger.LogInformation($"Looking up drawing number text by DrawingNumberId: {drawingNumberId}");
            try
            {
                var result = await _db.GetSingle<string?>(
                    DrawingNumberQueries.GET_DRAWINGNUMBER_TEXT_BY_ID,
                    new { DrawingNumberId = drawingNumberId });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error looking up drawing number text by DrawingNumberId: {drawingNumberId}");
                throw;
            }
        }

        public async Task<bool> CheckDrawingLnItemMapExists(string drawingNumber, string lnItemCode)
        {
            _logger.LogInformation($"Checking tbl_drawing_lnitem_map: DrawingNumber={drawingNumber}, LnItemCode={lnItemCode}");
            try
            {
                var result = await _db.GetSingle<int?>(
                    DrawingNumberQueries.CHECK_DRAWING_LNITEM_MAP_EXISTS,
                    new { DrawingNumber = drawingNumber, LnItemCode = lnItemCode });
                return result.HasValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking tbl_drawing_lnitem_map");
                throw;
            }
        }

        public async Task InsertDrawingLnItemMap(string drawingNumber, string lnItemCode, int createdBy, DateTime createdDate)
        {
            _logger.LogInformation($"Inserting tbl_drawing_lnitem_map: DrawingNumber={drawingNumber}, LnItemCode={lnItemCode}");
            try
            {
                await _db.Execute(
                    DrawingNumberQueries.INSERT_DRAWING_LNITEM_MAP,
                    new
                    {
                        DrawingNumber = drawingNumber,
                        LnItemCode = lnItemCode,
                        CreatedBy = createdBy,
                        CreatedDate = createdDate
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting tbl_drawing_lnitem_map");
                throw;
            }
        }

        #endregion

        #region Get Master ID from Mapping

        public async Task<int?> GetNomenclatureIdFromMapping(int drawingNumberId)
        {
            _logger.LogInformation($"Getting nomenclature ID from mapping: DrawingId={drawingNumberId}");
            try
            {
                var result = await _db.GetSingle<int?>(
                    DrawingNumberQueries.GET_NOMENCLATURE_ID_FROM_MAPPING,
                    new { DrawingNumberId = drawingNumberId });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting nomenclature ID from mapping");
                throw;
            }
        }

        public async Task<int?> GetLnItemCodeIdFromMapping(int drawingNumberId)
        {
            _logger.LogInformation($"Getting LnItemCode ID from mapping: DrawingId={drawingNumberId}");
            try
            {
                var result = await _db.GetSingle<int?>(
                    DrawingNumberQueries.GET_LNITEM_ID_FROM_MAPPING,
                    new { DrawingNumberId = drawingNumberId });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting LnItemCode ID from mapping");
                throw;
            }
        }

        public async Task<int?> GetRackLocationIdFromMapping(int drawingNumberId)
        {
            _logger.LogInformation($"Getting RackLocation ID from mapping: DrawingId={drawingNumberId}");
            try
            {
                var result = await _db.GetSingle<int?>(
                    DrawingNumberQueries.GET_RACKLOCATION_ID_FROM_MAPPING,
                    new { DrawingNumberId = drawingNumberId });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting RackLocation ID from mapping");
                throw;
            }
        }

        public async Task<int?> GetComponentTypeIdFromMapping(int drawingNumberId)
        {
            _logger.LogInformation($"Getting ComponentType ID from mapping: DrawingId={drawingNumberId}");
            try
            {
                var result = await _db.GetSingle<int?>(
                    DrawingNumberQueries.GET_COMPONENTTYPE_ID_FROM_MAPPING,
                    new { DrawingNumberId = drawingNumberId });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ComponentType ID from mapping");
                throw;
            }
        }

        public async Task<int?> GetDocumentTypeIdFromMapping(int drawingNumberId)
        {
            _logger.LogInformation($"Getting DocumentType ID from mapping: DrawingId={drawingNumberId}");
            try
            {
                var result = await _db.GetSingle<int?>(
                    DrawingNumberQueries.GET_DOCUMENTTYPE_ID_FROM_MAPPING,
                    new { DrawingNumberId = drawingNumberId });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting DocumentType ID from mapping");
                throw;
            }
        }

        public async Task<int?> GetUnitIdFromMapping(int drawingNumberId)
        {
            _logger.LogInformation($"Getting Unit ID from mapping: DrawingId={drawingNumberId}");
            try
            {
                var result = await _db.GetSingle<int?>(
                    DrawingNumberQueries.GET_UNIT_ID_FROM_MAPPING,
                    new { DrawingNumberId = drawingNumberId });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Unit ID from mapping");
                throw;
            }
        }

        public async Task<int?> GetProdSeriesIdFromMapping(int drawingNumberId)
        {
            _logger.LogInformation($"Getting ProdSeries ID from mapping: DrawingId={drawingNumberId}");
            try
            {
                var result = await _db.GetSingle<int?>(
                    DrawingNumberQueries.GET_PRODSERIES_ID_FROM_MAPPING,
                    new { DrawingNumberId = drawingNumberId });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ProdSeries ID from mapping");
                throw;
            }
        }

        public async Task<string?> GetProdSeriesNameById(int prodSeriesId)
        {
            _logger.LogInformation($"Looking up production series name: Id={prodSeriesId}");
            try
            {
                var result = await _db.GetSingle<string?>(
                    DrawingNumberQueries.GET_PRODSERIES_NAME_BY_ID,
                    new { ProdSeriesId = prodSeriesId });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up production series name");
                throw;
            }
        }

        #endregion

        #region Re-point Drawing Mapping to Resolved Master Id

        public async Task UpdateNomenclatureMapping(int drawingNumberId, int nomenclatureId)
        {
            _logger.LogInformation($"Re-pointing nomenclature mapping: DrawingId={drawingNumberId}, NomenclatureId={nomenclatureId}");
            try
            {
                await _db.Execute(
                    DrawingNumberQueries.UPDATE_NOMENCLATURE_MAPPING,
                    new { DrawingNumberId = drawingNumberId, NomenclatureId = nomenclatureId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error re-pointing nomenclature mapping");
                throw;
            }
        }

        public async Task UpdateLnItemLocationMapping(int drawingNumberId, int lnItemCodeId, int rackLocationId)
        {
            _logger.LogInformation($"Re-pointing LnItem-Location mapping: DrawingId={drawingNumberId}, LnItemCodeId={lnItemCodeId}, RackLocationId={rackLocationId}");
            try
            {
                await _db.Execute(
                    DrawingNumberQueries.UPDATE_LNITEM_LOCATION_MAPPING,
                    new { DrawingNumberId = drawingNumberId, LnItemCodeId = lnItemCodeId, RackLocationId = rackLocationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error re-pointing LnItem-Location mapping");
                throw;
            }
        }

        public async Task UpdateComponentTypeMapping(int drawingNumberId, int componentTypeId)
        {
            _logger.LogInformation($"Re-pointing ComponentType mapping: DrawingId={drawingNumberId}, ComponentTypeId={componentTypeId}");
            try
            {
                await _db.Execute(
                    DrawingNumberQueries.UPDATE_COMPONENTTYPE_MAPPING,
                    new { DrawingNumberId = drawingNumberId, ComponentTypeId = componentTypeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error re-pointing ComponentType mapping");
                throw;
            }
        }

        public async Task UpdateDocumentTypeMapping(int drawingNumberId, int documentTypeId)
        {
            _logger.LogInformation($"Re-pointing DocumentType mapping: DrawingId={drawingNumberId}, DocumentTypeId={documentTypeId}");
            try
            {
                await _db.Execute(
                    DrawingNumberQueries.UPDATE_DOCUMENTTYPE_MAPPING,
                    new { DrawingNumberId = drawingNumberId, DocumentTypeId = documentTypeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error re-pointing DocumentType mapping");
                throw;
            }
        }

        public async Task UpdateUnitMapping(int drawingNumberId, int unitId)
        {
            _logger.LogInformation($"Re-pointing Unit mapping: DrawingId={drawingNumberId}, UnitId={unitId}");
            try
            {
                await _db.Execute(
                    DrawingNumberQueries.UPDATE_UNIT_MAPPING,
                    new { DrawingNumberId = drawingNumberId, UnitId = unitId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error re-pointing Unit mapping");
                throw;
            }
        }

        public async Task UpdateProdSeriesMapping(int drawingNumberId, int availableSeriesId)
        {
            _logger.LogInformation($"Re-pointing ProdSeries mapping: DrawingId={drawingNumberId}, AvailableSeriesId={availableSeriesId}");
            try
            {
                await _db.Execute(
                    DrawingNumberQueries.UPDATE_PRODSERIES_MAPPING,
                    new { DrawingNumberId = drawingNumberId, AvailableSeriesId = availableSeriesId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error re-pointing ProdSeries mapping");
                throw;
            }
        }

        #endregion

        #region Insert New Master Entries

        public async Task<int> InsertNomenclature(string nomenclature, int createdBy, DateTime createdDate)
        {
            _logger.LogInformation($"Inserting or getting nomenclature: {nomenclature}");
            try
            {
                var result = await _db.ExecuteScalar<int>(
                    DrawingNumberQueries.INSERT_OR_GET_NOMENCLATURE,
                    new { Nomenclature = nomenclature, CreatedBy = createdBy, CreatedDate = createdDate });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inserting/getting nomenclature: {nomenclature}");
                throw;
            }
        }

        public async Task<int> InsertLnItemCode(string lnItemCode, string nomenclature, int createdBy, DateTime createdDate)
        {
            _logger.LogInformation($"Inserting or getting LnItemCode: {lnItemCode}");
            try
            {
                var result = await _db.ExecuteScalar<int>(
                    DrawingNumberQueries.INSERT_OR_GET_LNITEMCODE,
                    new { LnItemCode = lnItemCode, Nomenclature = nomenclature, CreatedBy = createdBy, CreatedDate = createdDate });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inserting/getting LnItemCode: {lnItemCode}");
                throw;
            }
        }

        public async Task<int> InsertRackLocation(string rackLocation, int createdBy, DateTime createdDate)
        {
            _logger.LogInformation($"Inserting or getting RackLocation: {rackLocation}");
            try
            {
                var result = await _db.ExecuteScalar<int>(
                    DrawingNumberQueries.INSERT_OR_GET_RACKLOCATION,
                    new { RackLocation = rackLocation, CreatedBy = createdBy, CreatedDate = createdDate });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inserting/getting RackLocation: {rackLocation}");
                throw;
            }
        }

        public async Task<int> InsertComponentType(string componentType, int createdBy, DateTime createdDate)
        {
            _logger.LogInformation($"Inserting or getting ComponentType: {componentType}");
            try
            {
                var result = await _db.ExecuteScalar<int>(
                    DrawingNumberQueries.INSERT_OR_GET_COMPONENTTYPE,
                    new { ComponentType = componentType, CreatedBy = createdBy, CreatedDate = createdDate });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inserting/getting ComponentType: {componentType}");
                throw;
            }
        }

        public async Task<int> InsertDocumentType(string documentType, int createdBy, DateTime createdDate)
        {
            _logger.LogInformation($"Inserting or getting DocumentType: {documentType}");
            try
            {
                var result = await _db.ExecuteScalar<int>(
                    DrawingNumberQueries.INSERT_OR_GET_DOCUMENTTYPE,
                    new { DocumentType = documentType, CreatedBy = createdBy, CreatedDate = createdDate });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inserting/getting DocumentType: {documentType}");
                throw;
            }
        }

        public async Task<int> InsertUnit(string unitName, int createdBy, DateTime createdDate)
        {
            _logger.LogInformation($"Inserting or getting Unit: {unitName}");
            try
            {
                var result = await _db.ExecuteScalar<int>(
                    DrawingNumberQueries.INSERT_OR_GET_UNIT,
                    new { UnitName = unitName, CreatedBy = createdBy, CreatedDate = createdDate });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inserting/getting Unit: {unitName}");
                throw;
            }
        }

        public async Task<int> InsertOrGetProdSeries(string availableFor, int createdBy, DateTime createdDate)
        {
            _logger.LogInformation($"Inserting or getting ProdSeries: {availableFor}");
            try
            {
                var result = await _db.ExecuteScalar<int>(
                    DrawingNumberQueries.INSERT_OR_GET_PRODSERIES,
                    new { AvailableFor = availableFor, CreatedBy = createdBy, CreatedDate = createdDate });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inserting/getting ProdSeries: {availableFor}");
                throw;
            }
        }

        #endregion

        #region Mapping Operations

        public async Task<bool> CheckNomenclatureMappingExists(int drawingNumberId)
        {
            _logger.LogInformation($"Checking nomenclature mapping: DrawingId={drawingNumberId}");
            try
            {
                var result = await _db.GetSingle<int?>(
                    DrawingNumberQueries.CHECK_NOMENCLATURE_MAPPING,
                    new { DrawingNumberId = drawingNumberId });
                return result.HasValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking nomenclature mapping");
                throw;
            }
        }

        public async Task InsertNomenclatureMapping(int drawingNumberId, int nomenclatureId, int createdBy, DateTime createdDate)
        {
            _logger.LogInformation($"Inserting nomenclature mapping: DrawingId={drawingNumberId}, NomenclatureId={nomenclatureId}");
            try
            {
                await _db.Execute(
                    DrawingNumberQueries.INSERT_NOMENCLATURE_MAPPING,
                    new
                    {
                        DrawingNumberId = drawingNumberId,
                        NomenclatureId = nomenclatureId,
                        CreatedBy = createdBy,
                        CreatedDate = createdDate
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting nomenclature mapping");
                throw;
            }
        }

        public async Task<bool> CheckLnItemLocationMappingExists(int drawingNumberId)
        {
            _logger.LogInformation($"Checking ln item location mapping: DrawingId={drawingNumberId}");
            try
            {
                var result = await _db.GetSingle<int?>(
                    DrawingNumberQueries.CHECK_LNITEM_LOCATION_MAPPING,
                    new { DrawingNumberId = drawingNumberId });
                return result.HasValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking ln item location mapping");
                throw;
            }
        }

        public async Task InsertLnItemLocationMapping(int drawingNumberId, int lnItemCodeId, int rackLocationId, int createdBy, DateTime createdDate)
        {
            _logger.LogInformation($"Inserting ln item location mapping: DrawingId={drawingNumberId}, LnItemCodeId={lnItemCodeId}, RackLocationId={rackLocationId}");
            try
            {
                await _db.Execute(
                    DrawingNumberQueries.INSERT_LNITEM_LOCATION_MAPPING,
                    new
                    {
                        DrawingNumberId = drawingNumberId,
                        LnItemCodeId = lnItemCodeId,
                        RackLocationId = rackLocationId,
                        CreatedBy = createdBy,
                        CreatedDate = createdDate
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting ln item location mapping");
                throw;
            }
        }

        public async Task<bool> CheckComponentTypeMappingExists(int drawingNumberId)
        {
            _logger.LogInformation($"Checking component type mapping: DrawingId={drawingNumberId}");
            try
            {
                var result = await _db.GetSingle<int?>(
                    DrawingNumberQueries.CHECK_COMPONENTTYPE_MAPPING,
                    new { DrawingNumberId = drawingNumberId });
                return result.HasValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking component type mapping");
                throw;
            }
        }

        public async Task InsertComponentTypeMapping(int drawingNumberId, int componentTypeId, int createdBy, DateTime createdDate)
        {
            _logger.LogInformation($"Inserting component type mapping: DrawingId={drawingNumberId}, ComponentTypeId={componentTypeId}");
            try
            {
                await _db.Execute(
                    DrawingNumberQueries.INSERT_COMPONENTTYPE_MAPPING,
                    new
                    {
                        DrawingNumberId = drawingNumberId,
                        ComponentTypeId = componentTypeId,
                        CreatedBy = createdBy,
                        CreatedDate = createdDate
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting component type mapping");
                throw;
            }
        }

        public async Task<bool> CheckDocumentTypeMappingExists(int drawingNumberId)
        {
            _logger.LogInformation($"Checking document type mapping: DrawingId={drawingNumberId}");
            try
            {
                var result = await _db.GetSingle<int?>(
                    DrawingNumberQueries.CHECK_DOCUMENTTYPE_MAPPING,
                    new { DrawingNumberId = drawingNumberId });
                return result.HasValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking document type mapping");
                throw;
            }
        }

        public async Task InsertDocumentTypeMapping(int drawingNumberId, int documentTypeId, int createdBy, DateTime createdDate)
        {
            _logger.LogInformation($"Inserting document type mapping: DrawingId={drawingNumberId}, DocumentTypeId={documentTypeId}");
            try
            {
                await _db.Execute(
                    DrawingNumberQueries.INSERT_DOCUMENTTYPE_MAPPING,
                    new
                    {
                        DrawingNumberId = drawingNumberId,
                        DocumentTypeId = documentTypeId,
                        CreatedBy = createdBy,
                        CreatedDate = createdDate
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting document type mapping");
                throw;
            }
        }

        public async Task<bool> CheckUnitMappingExists(int drawingNumberId)
        {
            _logger.LogInformation($"Checking unit mapping: DrawingId={drawingNumberId}");
            try
            {
                var result = await _db.GetSingle<int?>(
                    DrawingNumberQueries.CHECK_UNIT_MAPPING,
                    new { DrawingNumberId = drawingNumberId });
                return result.HasValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking unit mapping");
                throw;
            }
        }

        public async Task InsertUnitMapping(int drawingNumberId, int unitId, int createdBy, DateTime createdDate)
        {
            _logger.LogInformation($"Inserting unit mapping: DrawingId={drawingNumberId}, UnitId={unitId}");
            try
            {
                await _db.Execute(
                    DrawingNumberQueries.INSERT_UNIT_MAPPING,
                    new
                    {
                        DrawingNumberId = drawingNumberId,
                        UnitId = unitId,
                        CreatedBy = createdBy,
                        CreatedDate = createdDate
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting unit mapping");
                throw;
            }
        }

        public async Task InsertProdSeriesMapping(int drawingNumberId, int availableSeriesId, int createdBy, DateTime createdDate)
        {
            _logger.LogInformation($"Inserting ProdSeries mapping: DrawingId={drawingNumberId}, AvailableSeriesId={availableSeriesId}");
            try
            {
                await _db.Execute(
                    DrawingNumberQueries.INSERT_PRODSERIES_MAPPING,
                    new
                    {
                        DrawingNumberId = drawingNumberId,
                        AvailableSeriesId = availableSeriesId,
                        CreatedBy = createdBy,
                        CreatedDate = createdDate
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting ProdSeries mapping");
                throw;
            }
        }

        #endregion

        #region Assembly Drawing Mapping (parent/child)

        public async Task<int?> GetAssemblyDrawingMappingId(int childDrawingNumberId, int parentDrawingNumberId)
        {
            _logger.LogInformation($"Getting assembly drawing mapping: ChildId={childDrawingNumberId}, ParentId={parentDrawingNumberId}");
            try
            {
                var result = await _db.GetSingle<int?>(
                    DrawingNumberQueries.GET_ASSEMBLY_DRAWING_MAPPING_ID,
                    new { ChildDrawingNumberId = childDrawingNumberId, ParentDrawingNumberId = parentDrawingNumberId });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assembly drawing mapping");
                throw;
            }
        }

        public async Task<int> InsertAssemblyDrawingMapping(int childDrawingNumberId, int parentDrawingNumberId, decimal? quantity, string? unit,
            string? findNo, string? consumedProdSeriesId, string? nomenclature, string? assemblyLnItemCode, string? childLnItemCode,
            int createdBy, DateTime createdDate)
        {
            _logger.LogInformation($"Inserting assembly drawing mapping: ChildId={childDrawingNumberId}, ParentId={parentDrawingNumberId}");
            try
            {
                var result = await _db.ExecuteScalar<int>(
                    DrawingNumberQueries.INSERT_ASSEMBLY_DRAWING_MAPPING,
                    new
                    {
                        ChildDrawingNumberId = childDrawingNumberId,
                        ParentDrawingNumberId = parentDrawingNumberId,
                        Quantity = quantity,
                        Unit = unit,
                        FindNo = findNo,
                        ConsumedProdSeriesId = consumedProdSeriesId,
                        Nomenclature = nomenclature,
                        AssemblyLnItemCode = assemblyLnItemCode,
                        ChildLnItemCode = childLnItemCode,
                        CreatedBy = createdBy,
                        CreatedDate = createdDate
                    });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting assembly drawing mapping");
                throw;
            }
        }

        public async Task UpdateAssemblyDrawingMapping(int id, decimal? quantity, string? unit, string? findNo, string? consumedProdSeriesId,
            string? nomenclature, int modifiedBy, DateTime modifiedDate)
        {
            _logger.LogInformation($"Updating assembly drawing mapping: Id={id}");
            try
            {
                await _db.Execute(
                    DrawingNumberQueries.UPDATE_ASSEMBLY_DRAWING_MAPPING,
                    new
                    {
                        Id = id,
                        Quantity = quantity,
                        Unit = unit,
                        FindNo = findNo,
                        ConsumedProdSeriesId = consumedProdSeriesId,
                        Nomenclature = nomenclature,
                        ModifiedBy = modifiedBy,
                        ModifiedDate = modifiedDate
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assembly drawing mapping");
                throw;
            }
        }

        #endregion
    }
}
