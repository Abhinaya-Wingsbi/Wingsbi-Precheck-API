using System.IO;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DTOs.ProductionOrder;

namespace Godrej.Precheck.Service.Service.ProductionOrderService
{
    public interface IProductionOrderService
    {
        Task<ProductionOrderUploadResultDto> UploadExcelAsync(Stream fileStream, int createdBy);
        Task<bool> UpdateProductionOrderAsync(UpdateProductionOrderDto dto, int updatedBy);
        Task<ProductionOrderMasterDto?> GetByProductionOrderNumberAsync(string productionOrderNumber);
        Task<List<ProductionOrderMasterDto>> GetAllProductionOrdersAsync(int roleId = 0);
        Task<List<ProductionOrderMasterDto>> GetAllProductionOrdersAsync(string? dateFilterType, DateTime? filterDate, DateTime? fromDate, DateTime? toDate, int? precheckStatus, string? poNumber, string? lnItemCode, int roleId = 0, string? drawingNumber = null);
        Task<ProductionOrderMasterPagedResponse> GetAllProductionOrdersPagedAsync(int roleId, int pageNumber, int pageSize);
        Task<ProductionOrderMasterPagedResponse> GetAllProductionOrdersPagedAsync(string? dateFilterType, DateTime? filterDate, DateTime? fromDate, DateTime? toDate, int? precheckStatus, string? poNumber, string? lnItemCode, int roleId, string? drawingNumber, string? searchQuery, int pageNumber, int pageSize);
        Task<ProductionOrderDetailsDto> GetProductionOrderDetailsAsync(string productionOrderNumber);
        Task<byte[]> DownloadTemplateAsync();
        Task<List<ProductionOrderMasterDto>> GetAllPONumbersAsync(string? search = null);
        Task<ProductionOrderCountsDto> GetProductionOrderCountsAsync(ProductionOrderCountFilterDto filter);
        Task<byte[]> ExportProductionOrdersAsync(string? dateFilterType, DateTime? filterDate, DateTime? fromDate, DateTime? toDate, int? precheckStatus, string? poNumber, string? lnItemCode, int roleId = 0);
        Task<MinStatusUploadResultDto> UploadMinStatusExcelAsync(Stream fileStream, int updatedBy);
        Task<bool> DeleteProductionOrderAsync(DeleteProductionOrderRequestDto request);

    }
}
