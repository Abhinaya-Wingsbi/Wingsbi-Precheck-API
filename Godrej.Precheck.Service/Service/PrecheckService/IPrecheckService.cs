using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel.Precheck;
using Godrej.Precheck.Models.DTOs.Precheck;

namespace Godrej.Precheck.Service.Service.PrecheckService
{
    public interface IPrecheckService
    {
        Task<List<PrecheckTemplateResponseDto>> GetPrecheckAssemblyTemplate(string assemblyNumber);
       
        Task<List<ViewPreCheckResponse>> MakePrecheck(List<PrecheckRequestDto> request);

        Task<List<ViewPreCheckResponse>> BulkPrecheck(BulkPrecheckRequestDto request);

        Task<List<MakeOrderResponseDto>> MakeOrder(MakeOrderRequestDto request);
        Task<List<ViewPreCheckResponse>> ViewPrecheckDetailsService(ViewPreCheckRequestDto request);
        Task<List<AvailableComponentModel>> AvailableComponentDetailsService(AvailableComponentFilterDto qrCode);

        Task<byte[]> GeneratePrecheckPdfAsync(List<ViewPreCheckResponse> preCheckResponses, ViewPreCheckRequestDto request);

        Task<int?> GetPrecheckStatusDetailsService(ViewPreCheckRequestDto request);
        Task<List<GetAvailableComponentsResponse>> GetAvailableComponentService(GetAvailableComponentsRequest request);
        Task<int> RejectAndDuplicatePrecheck(RejectPrecheckRequestDto request);
        Task<UpdateQuantityResponseDto> UpdateQuantity(string productionOrderNumber,UpdateMaterialQuantityRequestDto request, string assemblyDrawingNo,int userId);
        Task<List<ViewPreCheckResponse>> ExportViewPrecheckDetailsService(ViewPreCheckRequestDto request);
        Task<int> PrecheckForRemainingQuantityService(RejectPrecheckRequestDto request);
        Task<bool> ResetRemainingQuantityService(ResetRemainingQuantityDto payload);
        Task<PrecheckExcelImportResultDto> MakePrecheckFromExcelAsync(System.IO.Stream fileStream, int createdBy);
        Task<byte[]> DownloadPrecheckExcelTemplateAsync();
        Task DeletePrecheckDetailsAsync(DeletePrecheckDetailsRequestDto request, int modifiedBy);
        Task RemovePrecheckDetailsAsync(DeletePrecheckDetailsRequestDto request, int modifiedBy);
        Task<AddPrecheckComponentResponseDto> AddPrecheckComponentAsync(AddPrecheckComponentDto request, int createdBy);
        Task<List<ConsumedInComponentsResponseDto>> GetConsumedInComponentsAsync(int drawingNumberId);
        Task<List<PendingPrecheckResponseDto>> GetPendingPrecheckAsync(PendingPrecheckRequestDto request);
        Task<byte[]> ExportPendingPrecheckAsync(PendingPrecheckRequestDto request);
    }
}
