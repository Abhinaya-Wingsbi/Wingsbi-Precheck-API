using System.Data;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel.ProductionOrder;
using Godrej.Precheck.Models.DTOs.ProductionOrder;
using Godrej.Precheck.Models.DTOs.Precheck;

namespace Godrej.Precheck.Repository.Repository.ProductionOrderRepository
{
    public interface IProductionOrderRepository
    {
        Task<int> InsertProductionOrderMasterAsync(ProductionOrderMaster master);
        Task<int> UpdateProductionOrderMasterAsync(ProductionOrderMaster master, int updatedBy);
        Task<ProductionOrderMasterDto?> GetByProductionOrderNumberAsync(string productionOrderNumber);
        Task<ProductionOrderMasterDto?> GetByProductionOrderNumberUpdatePOAsync(string productionOrderNumber,int? Id);

        Task<List<ProductionOrderMasterDto>> GetAllProductionOrdersAsync();
        Task<List<ProductionOrderMasterDto>> GetAllProductionOrdersAsync(string? dateFilterType, DateTime? filterDate, DateTime? fromDate, DateTime? toDate, int? precheckStatus, string? poNumber, string? lnItemCode,string? drawingnumber);
       // Task<List<ProductionOrderMasterDto>> GetAllProductionOrdersAsync(string? dateFilterType, DateTime? filterDate, DateTime? fromDate, DateTime? toDate, int? precheckStatus,string? poNumber, string? lnItemCode);

        Task<List<ProductionOrderMasterDto>> GetAllPONumbersAsync(string? search = null);
        Task<(int? DrawingNumberId, int? LnItemCodeId, string? DrawingNumber, string? Nomenclature)> LookupDrawingByLnItemCodeAsync(string lnItemCode);
        Task<(int? ProdSeriesId, string? ProductionSeries)> LookupProdSeriesByPrefixAsync(string prefix);
        Task<int> InsertProjectDetailsWithPOIdAsync(int idNumbers, int prodSeriesId, string projectNumber, string productionOrderNumber, int drawingNumberId, int productionOrderNumberId, int createdBy, IDbConnection connection = null);
        Task<int> InsertProjectPrecheckDetailsWithPOIdAsync(int drawingNumberId, int prodSeriesId, int projectDetailsId, decimal quantity, string componentType, int productionOrderNumberId, int createdBy, IDbConnection connection = null);
        Task<IDbConnection> CreateOpenConnectionAsync();
        Task<int> DeleteProjectDetailsWithPOIdAsync(int productionOrderNumberId);
        Task<bool> CheckPOExistsAsync(string productionOrderNumber, int prodSeriesId, int startIdNumber);
        Task<(bool HasOverlap, int? MaxEndIdNumber)> CheckProdSeriesStartIdOverlapAsync(int prodSeriesId, int lnItemCodeId, int startIdNumber, int quantity);
        Task<ProductionOrderCountsDto> GetProductionOrderCountsAsync(ProductionOrderCountFilterDto filter);
        Task<MinStatusUploadResultDto> UpdateMinStatusAsync(List<MinStatusUploadRowDto> poList);
        Task<bool> DeleteProductionOrderAsync(DeleteProductionOrderRequestDto request);
        Task<List<PendingPrecheckResponseDto>> GetProductionOrdersForPendingPrecheckAsync(int? assemblyDrawingNumberId, int? prodSeriesId, string? productionOrderNumber, string? lnItemCode);
    }
}
