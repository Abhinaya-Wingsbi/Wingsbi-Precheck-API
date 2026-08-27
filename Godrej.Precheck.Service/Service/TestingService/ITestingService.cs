using Godrej.Precheck.Models.DTOs.Testing;

namespace Godrej.Precheck.Service.Service.TestingService
{
    public interface ITestingService
    {
        Task<TemplateFieldsResponseDto> GetTemplateFieldsByDrawingNumberAsync(string drawingNumber, string? msnNumber, int? msnQuantity, int? stageId = null);
        Task<InsertInspectionValuesResponseDto> InsertInspectionValuesAsync(InsertInspectionValuesRequestDto request);
        Task<byte[]> ExportInspectionAsPdfAsync(string drawingNumber, string? msnNumber = null, int msnQuantity = 4);
        Task<List<PrecheckCompletedComponentDto>> GetPrecheckCompletedComponentsAsync();
        Task<List<DrawingStageStatusDto>> GetDrawingStageStatusAsync();
        Task<SaveStageDataResponseDto> SaveStageDataAsync(SaveStageDataRequestDto request);
        Task<GetStageDataResponseDto> GetStageDataAsync(string drawingNumber, string msnNumber, int stageId);
        Task<object> GetExportDebugDataAsync(string drawingNumber);
        Task<string?> GetRawTemplateHtmlAsync(string drawingNumber);
        Task<object> GetFieldNamesForExportAsync(string drawingNumber);
    }
}
