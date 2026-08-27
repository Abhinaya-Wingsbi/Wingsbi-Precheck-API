using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel.Precheck;
using Godrej.Precheck.Models.DTOs.Precheck;
using Godrej.Precheck.Models.DTOs.QRCodeDetails;

namespace Godrej.Precheck.Repository.Repository.PrecheckRepository
{
    public interface IPrecheckRepository
    {
        Task<List<PrecheckTemplateResponse>> GetPrecheckTemplateResponsesAsync(string assembly);
        Task<List<PrecheckTemplateResponse>> GetPrecheckTemplateResponsesAsync(int assemblyNumber);

        Task<int> CreateProjectPrecheckDetails(ProjectPrecheckRequest precheckRequest);

        Task<int> CreateProjectDetails(MakeOrderRequest precheckRequest);

        Task<MakePrecheckRequest> UpdatePrecheckDetails(MakePrecheckRequest precheckRequest);

        Task<int> UpdateProjectStatusDetails(ViewPreCheckRequest precheckRequest, int StatusId);

        Task<MakePrecheckRequest> UpdateIdComponentConsumption(MakePrecheckRequest precheckRequest);

        Task<MakePrecheckRequest> UpdateBatchComponentConsumption(MakePrecheckRequest precheckRequest);

        Task<List<ViewPreCheckResponse>> ViewPrecheckDetails(ViewPreCheckRequest request);

        Task<List<ViewPreCheckResponse>> ViewPrecheckDetailsForProductionOrders(List<string> productionOrderNumbers);

        Task<List<ViewPreCheckResponse>> ExportViewPrecheckDetails(ViewPreCheckRequest request);

        Task<List<AvailableComponentModel>> GetAvailableComponentDetails(int DrawingId, int ProdSeriesId, DateTime? fromDate,DateTime? toDate);
        Task<ProjectPrecheckResponse> GetProjectDetails(ViewPreCheckRequest precheckRequest);
        Task<ProjectContextResult?> GetProjectContextByPoAndId(string productionOrderNumber, int idNumber, int? parentDrawingNumberId = null);
        Task<int?> GetDrawingNumberIdByName(string drawingNumber);
        Task<List<GetAvailableComponentsResponse>> GetAvailableComponentForOrder(GetAvailableComponentsRequest request);

        Task<List<ProjectDetailsResponse>> ValidateOrder(int prodSeriesId, int drawingId, string pONumber, int idNumber);

        //Get Available component Qunatity by using the Drawing Number.
        Task<int> GetAvailableComponentQunatity(int DrawingId);

        Task<int> RejectAndDuplicatePrecheck(Models.DTOs.Precheck.RejectPrecheckRequestDto request);
        Task<UpdateQuantityResponseDto> GetByProductionOrderNumberAsync(string productionOderNumber);
        Task<decimal> GetBatchTotalQuantity(UpdateMaterialQuantityRequestDto requestDto);
        Task<decimal> UpdateComponentRemaningQuantity(UpdateMaterialQuantityRequestDto qrCodeNumber, decimal? remainingQuantity);
        Task<int> UpdateQrcodeStatus(PrecheckRequestDto request);
        Task<decimal> UpdateQrcodeQuantity(string quCodeNumber, decimal newRemainingQuantity);
        Task<int> PrecheckForRemainingQuantityServiceRepo(Models.DTOs.Precheck.RejectPrecheckRequestDto request);
        Task<bool> GetDrawingNumberIdAsync(int drawingNumberId);
        Task<bool> ResetRemainingQuantity(ResetRemainingQuantityDto remainingQuantityDto);
        Task<PrecheckDetailStatusResult?> GetPrecheckDetailByProjectAndDrawing(int projectDetailsId, int drawingNumberId);
        Task<int> DeleteProjectPrecheckDetail(int id, int modifiedBy);
        Task<int> RemoveProjectPrecheckDetail(int id, int modifiedBy);
        Task<int> UpdateQRCodeStatusQuantity(PrecheckDetailStatusResult precheckDetail);
        Task<List<AssemblyProductionOrderResult>> GetAssemblyProductionOrdersByLnItemCode(string assemblyLnItemCode);
        Task<List<ProjectDetailsIdResult>> GetProjectDetailsIdsByProductionOrderNumbers(List<string> productionOrderNumbers);
        Task<AssemblyChildBomResult?> GetAssemblyChildBomDetail(int assemblyDrawingNumberId, string childLnItemCode);
        Task<int> CreateProjectPrecheckDetailWithUnit(int drawingNumberId, int prodSeriesId, int projectDetailsId, decimal quantity, string unit, string componentType, int productionOrderNumberId, int createdBy);
        Task<List<ConsumedInAssemblyResult>> GetConsumedInAssemblies(int drawingNumberId);
    }
}
