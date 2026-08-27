using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DTOs.DrawingNumber;

namespace Godrej.Precheck.Repository.Repository.DrawingNumberRepository
{
    public interface IDrawingNumberRepository
    {
        Task<bool> CheckDrawingNumberExists(int drawingNumberId);

        // Upsert the tbl_drawingnumber row itself (by drawingnumber + lnitemcode)
        Task<int?> GetDrawingNumberIdByDrawingAndLnItemCode(string drawingNumber, string lnItemCode);
        Task<int> InsertDrawingNumber(string drawingNumber, string lnItemCode, bool isExpiry, bool isActive, int createdBy, DateTime createdDate);
        Task UpdateDrawingNumberCore(int id, bool isExpiry, bool isActive, int modifiedBy, DateTime modifiedDate);
        Task<DrawingNumberLookupDto?> GetDrawingNumberLookupByDrawingNumber(string drawingNumber);
        Task<string?> GetLnItemCodeByDrawingNumberId(int drawingNumberId);
        Task<string?> GetNomenclatureByDrawingNumberId(int drawingNumberId);
        Task<string?> GetDrawingNumberTextById(int drawingNumberId);
        Task<bool> CheckDrawingLnItemMapExists(string drawingNumber, string lnItemCode);
        Task InsertDrawingLnItemMap(string drawingNumber, string lnItemCode, int createdBy, DateTime createdDate);

        // Get master ID from mapping (by drawing number)
        Task<int?> GetNomenclatureIdFromMapping(int drawingNumberId);
        Task<int?> GetLnItemCodeIdFromMapping(int drawingNumberId);
        Task<int?> GetRackLocationIdFromMapping(int drawingNumberId);
        Task<int?> GetComponentTypeIdFromMapping(int drawingNumberId);
        Task<int?> GetDocumentTypeIdFromMapping(int drawingNumberId);
        Task<int?> GetUnitIdFromMapping(int drawingNumberId);
        Task<int?> GetProdSeriesIdFromMapping(int drawingNumberId);
        Task<string?> GetProdSeriesNameById(int prodSeriesId);

        // Re-point this drawing's own mapping row to a resolved (get-or-create'd) master id.
        // Never mutate the shared master row in place - other drawings can reference the same master id.
        Task UpdateNomenclatureMapping(int drawingNumberId, int nomenclatureId);
        Task UpdateLnItemLocationMapping(int drawingNumberId, int lnItemCodeId, int rackLocationId);
        Task UpdateComponentTypeMapping(int drawingNumberId, int componentTypeId);
        Task UpdateDocumentTypeMapping(int drawingNumberId, int documentTypeId);
        Task UpdateUnitMapping(int drawingNumberId, int unitId);
        Task UpdateProdSeriesMapping(int drawingNumberId, int availableSeriesId);

        // Insert new master entries (when no mapping exists)
        Task<int> InsertNomenclature(string nomenclature, int createdBy, DateTime createdDate);
        Task<int> InsertLnItemCode(string lnItemCode, string nomenclature, int createdBy, DateTime createdDate);
        Task<int> InsertRackLocation(string rackLocation, int createdBy, DateTime createdDate);
        Task<int> InsertComponentType(string componentType, int createdBy, DateTime createdDate);
        Task<int> InsertDocumentType(string documentType, int createdBy, DateTime createdDate);
        Task<int> InsertUnit(string unitName, int createdBy, DateTime createdDate);
        Task<int> InsertOrGetProdSeries(string availableFor, int createdBy, DateTime createdDate);

        // Mapping operations
        Task<bool> CheckNomenclatureMappingExists(int drawingNumberId);
        Task InsertNomenclatureMapping(int drawingNumberId, int nomenclatureId, int createdBy, DateTime createdDate);

        Task<bool> CheckLnItemLocationMappingExists(int drawingNumberId);
        Task InsertLnItemLocationMapping(int drawingNumberId, int lnItemCodeId, int rackLocationId, int createdBy, DateTime createdDate);

        Task<bool> CheckComponentTypeMappingExists(int drawingNumberId);
        Task InsertComponentTypeMapping(int drawingNumberId, int componentTypeId, int createdBy, DateTime createdDate);

        Task<bool> CheckDocumentTypeMappingExists(int drawingNumberId);
        Task InsertDocumentTypeMapping(int drawingNumberId, int documentTypeId, int createdBy, DateTime createdDate);

        Task<bool> CheckUnitMappingExists(int drawingNumberId);
        Task InsertUnitMapping(int drawingNumberId, int unitId, int createdBy, DateTime createdDate);

        Task InsertProdSeriesMapping(int drawingNumberId, int availableSeriesId, int createdBy, DateTime createdDate);

        // Assembly drawing mapping (parent/child)
        Task<int?> GetAssemblyDrawingMappingId(int childDrawingNumberId, int parentDrawingNumberId);
        Task<int> InsertAssemblyDrawingMapping(int childDrawingNumberId, int parentDrawingNumberId, decimal? quantity, string? unit,
            string? findNo, string? consumedProdSeriesId, string? nomenclature, string? assemblyLnItemCode, string? childLnItemCode,
            int createdBy, DateTime createdDate);
        Task UpdateAssemblyDrawingMapping(int id, decimal? quantity, string? unit, string? findNo, string? consumedProdSeriesId,
            string? nomenclature, int modifiedBy, DateTime modifiedDate);
    }
}

