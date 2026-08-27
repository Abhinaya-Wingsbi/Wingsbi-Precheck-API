
using Dapper;
using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DataModel.Precheck;
using Godrej.Precheck.Models.DTOs.Precheck;
using Godrej.Precheck.Models.DTOs.ProductionOrder;
using Godrej.Precheck.Models.DTOs.QRCodeDetails;
using Godrej.Precheck.Repository.Database;
using Godrej.Precheck.Repository.Queries;
using Godrej.Precheck.Repository.Repository.ProductionOrderRepository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Godrej.Precheck.Repository.Repository.PrecheckRepository
{
    public class PrecheckRepository : IPrecheckRepository
    {
        private readonly ILogger<PrecheckRepository> _logger;
        private readonly IApplicationDbContext _db;
        private readonly IProductionOrderRepository _productionOrderRepository;

        public PrecheckRepository(ILogger<PrecheckRepository> logger, IApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<List<PrecheckTemplateResponse>> GetPrecheckTemplateResponsesAsync(string assemblyNumber)
        {
            _logger.LogInformation($"Request for PrecheckRepository:GetPrecheckTemplateResponsesAsync : {assemblyNumber}");
            var results = await _db.GetAll<PrecheckTemplateResponse>(
            PrecheckQueries.GET_PRECHECK_TEMPLATE_BY_ASSEMBLY,
            new { assemblyNumber = assemblyNumber });
            _logger.LogInformation($"Result for PrecheckRepository:GetPrecheckTemplateResponsesAsync:{results}");
            return results.ToList();
        }

        public async Task<List<PrecheckTemplateResponse>> GetPrecheckTemplateResponsesAsync(int assemblyNumber)
        {
            try
            {

                _logger.LogInformation($"Request for PrecheckRepository:GetPrecheckTemplateResponsesAsync: {assemblyNumber}");
                var results = await _db.GetAll<PrecheckTemplateResponse>(
                PrecheckQueries.GET_PRECHECK_TEMPLATE_BY_ASSEMBLY_ID,
                new { assemblyNumber = assemblyNumber });
                _logger.LogInformation($"Result for PrecheckRepository:GetPrecheckTemplateResponsesAsync{results}");
                return results.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public async Task<MakePrecheckRequest> UpdateIdComponentConsumption(MakePrecheckRequest precheckRequest)
        {

            _logger.LogInformation($"Request for PrecheckRepository:Upadate UpdateIdComponentConsumption: {precheckRequest}");
            try
            {


                var updatedId = await _db.UpdateAsync(

                    PrecheckQueries.UPDATE_ID_COMPONENT_CONSUMPTION,
                    new
                    {

                        consumedindrawing = precheckRequest.ConsumedDrawingNo,
                        consumedinproductionordernumber = precheckRequest.ProductionOrderNumber,
                        consumedindrawingid = precheckRequest.ConsumedInDrawingNumberID,
                        consumedinseriesid = precheckRequest.ConsumedInProdSeriesID,
                        consumedinId = precheckRequest.ConsumedInId,
                        remarks = precheckRequest.Remarks,
                        quantity = precheckRequest.Quantity,
                        unit = precheckRequest.Unit,
                        drawingnumberid = precheckRequest.DrawingNumberId,
                        Idnumber = precheckRequest.Id,
                        ProdSeriesId = precheckRequest.ProductionSeriesId,
                        qrCodeNumber = precheckRequest.QrCodeNumber,
                        modifiedby = precheckRequest.CreatedBy,
                        modifieddate = DateTime.Now,
                        isactive = 1
                    });



                _logger.LogInformation($"Successfully updated UpdateIdComponentConsumption: for {precheckRequest.ConsumedDrawingNo} ");



                return precheckRequest;
            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Error occurred while UpdateIdComponentConsumption.");
                return precheckRequest;
                //  throw;

            }
        }

        public async Task<MakePrecheckRequest> UpdateBatchComponentConsumption(MakePrecheckRequest precheckRequest)
        {

            _logger.LogInformation($"Request for PrecheckRepository:Upadate UpdateBatchComponentConsumption: {precheckRequest}");
            try
            {
                var response = await _db.GetSingle<MakePrecheckRequest>(PrecheckQueries.GET_ID_COMPONENT_CONSUMPTION, new { qrCodeNumber = precheckRequest.QrCodeNumber });

                if (response == null)
                {
                    var updatedId = await _db.UpdateAsync(

                    PrecheckQueries.UPDATE_BATCH_COMPONENT_CONSUMPTION,
                    new
                    {

                        consumedindrawing = precheckRequest.ConsumedDrawingNo,
                        consumedindrawingid = precheckRequest.ConsumedInDrawingNumberID,
                        consumedinseriesid = precheckRequest.ConsumedInProdSeriesID,
                        consumedinId = precheckRequest.ConsumedInId,
                        consumedinproductionordernumber = precheckRequest.ProductionOrderNumber,
                        remarks = precheckRequest.Remarks,
                        quantity = precheckRequest.Quantity,
                        unit = precheckRequest.Unit,
                        qrcodenumber = precheckRequest.QrCodeNumber,
                        modifiedby = precheckRequest.CreatedBy,
                        modifieddate = DateTime.Now,
                        isactive = 1
                    });

                    _logger.LogInformation($"Successfully updated precheck details: for {precheckRequest.ConsumedDrawingNo} ");

                }

                return precheckRequest;
            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Error occurred while UpdateBatchComponentConsumption.");

                // throw;
                return precheckRequest;

            }
        }


        public async Task<MakePrecheckRequest> UpdatePrecheckDetails(MakePrecheckRequest precheckRequest)
        {

            _logger.LogInformation($"Request for PrecheckRepository::Upadate UpdatePrecheckDetails: {precheckRequest}");
            try
            {
                var indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);
                var updatedId = await _db.UpdateAsync(

                    PrecheckQueries.UPDATE_PROJECT_PRECHECK_DETAIL,
                    new
                    {
                        irnumber = precheckRequest.IrNumber,
                        msnnumber = precheckRequest.MsnNumber,
                        mrirnumber = precheckRequest.MrirNumber,
                        consumedindrawing = precheckRequest.ConsumedDrawingNo,
                        consumedindrawingid = precheckRequest.ConsumedInDrawingNumberID,
                        consumedinseriesid = precheckRequest.ConsumedInProdSeriesID,
                        componenttype = precheckRequest.ComponentType,
                        consumedinId = precheckRequest.ConsumedInId,
                        consumedinproductionordernumber = precheckRequest.ProductionOrderNumber,
                        remarks = precheckRequest.Remarks,
                        quantity = precheckRequest.Quantity,
                        unit = precheckRequest.Unit,
                        drawingnumberid = precheckRequest.DrawingNumberId,
                        idnumbers = precheckRequest.Id,
                        idnumber = precheckRequest.IdNumbers,
                        prodSeriesId = precheckRequest.ProductionSeriesId,
                        productionordernumber = precheckRequest.ProductionOrderNumber,
                        modifiedby = precheckRequest.CreatedBy,
                        modifieddate = indianTime,
                        precheckdate = indianTime,
                        username = precheckRequest.UserName,
                        qrcodenumber = precheckRequest.QrCodeNumber,
                        remainingquantity = precheckRequest.RemainingQuantity,
                        createdby=precheckRequest.CreatedBy,
                        qrcodeid = precheckRequest.QrCodeId
                    });

                _logger.LogInformation($"Successfully Upadated UpdatePrecheckDetails : {precheckRequest.ConsumedDrawingNo}");


                return precheckRequest;
            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Error occurred while UpdatePrecheckDetails.");

                throw;

            }
        }

        public async Task<int> CreateProjectPrecheckDetails(ProjectPrecheckRequest precheckRequest)
        {
            _logger.LogInformation($"Request for PrecheckRepository:Inserting CreateProjectPrecheckDetails: {precheckRequest}");
            try
            {

                var insertedId = await _db.ExecuteScalar<int>(
                    PrecheckQueries.INSERT_PROJECT_PRECHECK_DETAILS,
                    new
                    {

                        drawingnumberid = precheckRequest.DrawingNumberId,
                        ProdSeriesId = precheckRequest.ProductionSeriesId,
                        projectdetailsid = precheckRequest.ProjectDetailsId,
                        Quantity = precheckRequest.Quantity,
                        unit = 1,
                        ComponentType = precheckRequest.ComponentType,
                        createdby = precheckRequest.CreatedBy,
                        createddate = DateTime.Now

                    });

                _logger.LogInformation($"Inserted ID: {insertedId}");


                return insertedId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting CreateProjectPrecheckDetails.");
                throw;
            }
        }

        public async Task<int> CreateProjectDetails(MakeOrderRequest precheckRequest)
        {
            _logger.LogInformation($"Request for PrecheckRepository:Inserting CreateProjectDetails: {precheckRequest}");
            try
            {
                var insertedId = await _db.ExecuteScalar<int>(

                    PrecheckQueries.INSERT_PROJECT_DETAILS,
                    new
                    {
                        ProjectNumber = precheckRequest.ProductionOrderNumber,
                        ProductionOrderNumber = precheckRequest.ProductionOrderNumber,
                        ProdSeriesId = precheckRequest.ProductionSeriesId,
                        IdNumbers = precheckRequest.Id,
                        DrawingNumberId = precheckRequest.DrawingNumberId,
                        CreatedBy = precheckRequest.CreatedBy,
                        CreatedDate = DateTime.Now

                    });

                _logger.LogInformation($"Successfully inserted CreateProjectDetails: {insertedId}");

                return insertedId;
            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Error occurred while inserting CreateProjectDetails.");

                throw;

            }
        }

        public async Task<int> UpdateProjectStatusDetails(ViewPreCheckRequest precheckRequest, int StatusId)
        {
            _logger.LogInformation($"Request for PrecheckRepository:UpdateProjectStatusDetails: {precheckRequest}");
            try
            {
                var insertedId = await _db.ExecuteScalar<int>(

                    PrecheckQueries.UPDATE_PROJECT_PRECHECK_STATUS_DETAILS,
                    new
                    {

                        ProdSeriesId = precheckRequest.ProductionSeriesId,
                        IdNumbers = precheckRequest.Id,
                        DrawingNumberId = precheckRequest.DrawingNumberId,
                        precheckstatus = StatusId

                    });

                _logger.LogInformation($"Successfully UpdateProjectStatusDetails: {insertedId}");
                //precheckRequest.ProjectDetailsId = insertedId;
                return insertedId;
            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Error occurred while UpdateProjectStatusDetails.");

                throw;

            }
        }

        public async Task<List<ViewPreCheckResponse>> ViewPrecheckDetails(ViewPreCheckRequest request)
        {
            _logger.LogInformation($"Request for PrecheckRepository:ViewPrecheckDetails {request}");

            if (request.ProductionOrderNumber != null)
            {
                var results = await _db.GetAll<ViewPreCheckResponse>(
               PrecheckQueries.GET_VIEW_PRECHECK_BY_PO_NUMBER,
               new
               {
                   productionordernumber = request.ProductionOrderNumber,
                   drawingnumberid = request.DrawingNumberId.HasValue && request.DrawingNumberId > 0 ? request.DrawingNumberId : null,
                   productionseriesid = request.ProductionSeriesId,
                   idnumber = request.Id,
                   rejectedDrawingNumberId = request.DrawingNumberId
               });
                _logger.LogInformation($"Result for PrecheckRepository:ViewPrecheckDetails{results}");
                return results.ToList();

            }
            else
            {
                var results = await _db.GetAll<ViewPreCheckResponse>(
                PrecheckQueries.GET_VIEW_PRECHECK_BY_ID_NUMBER,
                new
                {
                    drawingnumberid = request.DrawingNumberId,
                    productionseriesid = request.ProductionSeriesId,
                    idnumber = request.Id,
                    rejectedDrawingNumberId = request.DrawingNumberId
                });
                _logger.LogInformation($"Result for PrecheckRepository:ViewPrecheckDetails{results}");
                return results.ToList();
            }
        }

        public async Task<List<ViewPreCheckResponse>> ViewPrecheckDetailsForProductionOrders(List<string> productionOrderNumbers)
        {
            _logger.LogInformation($"Request for PrecheckRepository:ViewPrecheckDetailsForProductionOrders, {productionOrderNumbers.Count} PO(s)");

            if (productionOrderNumbers == null || productionOrderNumbers.Count == 0)
            {
                return new List<ViewPreCheckResponse>();
            }

            // SQL Server caps a single query at 2100 parameters, and Dapper expands an IN-clause list
            // into one parameter per item - so a batch of thousands of production order numbers has to
            // be split into chunks under that limit. This still turns what used to be one query per
            // production order into a small, fixed number of queries instead of thousands.
            const int chunkSize = 2000;
            var allResults = new List<ViewPreCheckResponse>();

            foreach (var chunk in productionOrderNumbers.Chunk(chunkSize))
            {
                // tbl_projectdetails.productionordernumber is varchar, but Dapper types a plain C# string
                // parameter as nvarchar by default. That mismatch is cheap for a single "=" comparison but
                // devastating for "IN (...)" against an unindexed column - SQL Server has to convert every
                // row before it can compare, once per list item. Dapper's automatic IN-list expansion
                // doesn't let a DbType be attached, so the parameter list and the (@p0,@p1,...) clause are
                // built by hand here, each one explicitly typed as AnsiString to avoid the conversion.
                var parameters = new DynamicParameters();
                var paramNames = new string[chunk.Length];
                for (int i = 0; i < chunk.Length; i++)
                {
                    paramNames[i] = $"@po{i}";
                    parameters.Add($"po{i}", chunk[i], DbType.AnsiString);
                }

                var query = PrecheckQueries.GET_VIEW_PRECHECK_BY_PO_NUMBERS
                    .Replace("{{PO_LIST}}", $"({string.Join(",", paramNames)})");

                var chunkResults = await _db.GetAll<ViewPreCheckResponse>(query, parameters);

                allResults.AddRange(chunkResults);
            }

            _logger.LogInformation($"Result for PrecheckRepository:ViewPrecheckDetailsForProductionOrders{allResults}");
            return allResults;
        }

        public async Task<List<ViewPreCheckResponse>> ExportViewPrecheckDetails(ViewPreCheckRequest request)
        {
            _logger.LogInformation($"Request for PrecheckRepository:ViewPrecheckDetails {request}");

            if (request.ProductionOrderNumber != null)
            {
                var results = await _db.GetAll<ViewPreCheckResponse>(
               PrecheckQueries.Export_View_Precheck,
               new
               {
                   productionordernumber = request.ProductionOrderNumber,
                   drawingnumberid = request.DrawingNumberId.HasValue && request.DrawingNumberId > 0 ? request.DrawingNumberId : null,
                   productionseriesid = request.ProductionSeriesId,
                   idnumber = request.Id,
                   rejectedDrawingNumberId = request.DrawingNumberId
               });
                _logger.LogInformation($"Result for PrecheckRepository:ViewPrecheckDetails{results}");
                return results.ToList();

            }
            else
            {
                var results = await _db.GetAll<ViewPreCheckResponse>(
                PrecheckQueries.GET_VIEW_PRECHECK_BY_ID_NUMBER,
                new
                {
                    drawingnumberid = request.DrawingNumberId,
                    productionseriesid = request.ProductionSeriesId,
                    idnumber = request.Id,
                    rejectedDrawingNumberId = request.DrawingNumberId
                });
                _logger.LogInformation($"Result for PrecheckRepository:ViewPrecheckDetails{results}");
                return results.ToList();
            }
        }

        public async Task<ProjectPrecheckResponse> GetProjectDetails(ViewPreCheckRequest precheckRequest)
        {
            _logger.LogInformation($"Request for PrecheckRepository:fetching GetProjectDetails : {precheckRequest}");
            try
            {
                var response = await _db.GetSingle<ProjectPrecheckResponse>(

                    PrecheckQueries.GET_PROJECT_DETAILS,
                   new
                   {

                       ProdSeriesId = precheckRequest.ProductionSeriesId,
                       IdNumbers = precheckRequest.Id,
                       DrawingNumberId = precheckRequest.DrawingNumberId


                   });

                _logger.LogInformation($"Response for PrecheckRepository : GetProjectDetails {response}");
                //precheckRequest.ProjectDetailsId = insertedId;
                return response;
            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Error occurred while fetching GetProjectDetails.");

                throw;

            }
        }

        public async Task<ProjectContextResult?> GetProjectContextByPoAndId(string productionOrderNumber, int idNumber, int? parentDrawingNumberId = null)
        {
            _logger.LogInformation($"Request for PrecheckRepository:GetProjectContextByPoAndId, PO: {productionOrderNumber}, IdNumber: {idNumber}, ParentDrawingNumberId: {parentDrawingNumberId}");
            try
            {
                var response = await _db.GetSingle<ProjectContextResult?>(
                    PrecheckQueries.GET_PROJECT_CONTEXT_BY_PO_AND_ID,
                    new
                    {
                        ProductionOrderNumber = productionOrderNumber,
                        IdNumbers = idNumber,
                        ParentDrawingNumberId = parentDrawingNumberId
                    });

                _logger.LogInformation($"Response for PrecheckRepository:GetProjectContextByPoAndId: {response}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching GetProjectContextByPoAndId.");
                throw;
            }
        }

        public async Task<int?> GetDrawingNumberIdByName(string drawingNumber)
        {
            _logger.LogInformation($"Request for PrecheckRepository:GetDrawingNumberIdByName, DrawingNumber: {drawingNumber}");
            try
            {
                var response = await _db.GetSingle<int?>(
                    PrecheckQueries.GET_DRAWINGNUMBER_ID_BY_NAME,
                    new { DrawingNumber = drawingNumber });

                _logger.LogInformation($"Response for PrecheckRepository:GetDrawingNumberIdByName: {response}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching GetDrawingNumberIdByName.");
                throw;
            }
        }

        public async Task<PrecheckDetailStatusResult?> GetPrecheckDetailByProjectAndDrawing(int projectDetailsId, int drawingNumberId)
        {
            _logger.LogInformation($"Request for PrecheckRepository:GetPrecheckDetailByProjectAndDrawing, ProjectDetailsId: {projectDetailsId}, DrawingNumberId: {drawingNumberId}");
            try
            {
                var response = await _db.GetSingle<PrecheckDetailStatusResult?>(
                    PrecheckQueries.GET_PROJECT_PRECHECK_DETAIL_BY_PROJECT_AND_DRAWING,
                    new { ProjectDetailsId = projectDetailsId, DrawingNumberId = drawingNumberId });

                _logger.LogInformation($"Response for PrecheckRepository:GetPrecheckDetailByProjectAndDrawing: {response}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching GetPrecheckDetailByProjectAndDrawing.");
                throw;
            }
        }

        public async Task<int> DeleteProjectPrecheckDetail(int id, int modifiedBy)
        {
            _logger.LogInformation($"Request for PrecheckRepository:DeleteProjectPrecheckDetail, Id: {id}");
            try
            {
                var indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                var rowsAffected = await _db.Update(
                    PrecheckQueries.DELETE_PROJECT_PRECHECK_DETAIL,
                    new { Id = id, ModifiedBy = modifiedBy, ModifiedDate = indianTime });

                _logger.LogInformation($"Response for PrecheckRepository:DeleteProjectPrecheckDetail, RowsAffected: {rowsAffected}");
                return rowsAffected;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while executing DeleteProjectPrecheckDetail.");
                throw;
            }
        }

        public async Task<int> RemoveProjectPrecheckDetail(int id, int modifiedBy)
        {
            _logger.LogInformation($"Request for PrecheckRepository:RemoveProjectPrecheckDetail, Id: {id}");
            try
            {
                var indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                var rowsAffected = await _db.Update(
                    PrecheckQueries.REMOVE_PROJECT_PRECHECK_DETAIL,
                    new { Id = id, ModifiedBy = modifiedBy, ModifiedDate = indianTime });

                _logger.LogInformation($"Response for PrecheckRepository:RemoveProjectPrecheckDetail, RowsAffected: {rowsAffected}");
                return rowsAffected;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while executing RemoveProjectPrecheckDetail.");
                throw;
            }
        }

        public async Task<List<AvailableComponentModel>> GetAvailableComponentDetails(int DrawingId,int ProdSeriesId,DateTime? fromDate,DateTime? toDate)
        {
            _logger.LogInformation($"Request for PrecheckRepository:GetAvailableComponentDetails DrawingId: {DrawingId}, ProdSeriesId: {ProdSeriesId}, FromDate: {fromDate}, ToDate: {toDate}");

            var results = await _db.GetAll<AvailableComponentModel>(
                PrecheckQueries.GET_Available_Components,
                new
                {
                    drawingnumberid = DrawingId,
                    productionseriesid = ProdSeriesId,
                    fromDate = fromDate,
                    toDate = toDate.HasValue ? toDate.Value.Date.AddDays(1).AddTicks(-1) : (DateTime?)null
                    // ^^^ makes toDate inclusive of the full day (up to 23:59:59.999)
                });

            _logger.LogInformation($"Result for PrecheckRepository:GetAvailableComponentDetails count: {results?.Count()}");
            return results.ToList();
        }


        //get avaialable quantity 

        public async Task<int> GetAvailableComponentQunatity(int DrawingId)
        {
            _logger.LogInformation($"Request for PrecheckRepository:GetAvailableComponentQunatity by DrawingId {DrawingId}");
            var quantity = await _db.GetSingle<int>(
            PrecheckQueries.GET_AVAILABLE_QRCODE_BY_DRAWINGID,
            new
            {
                drawingnumberid = DrawingId

            });
            _logger.LogInformation($"Result for PrecheckRepository:GetAvailableComponentQunatity{quantity}");
            return quantity;
        }

        public async Task<List<GetAvailableComponentsResponse>> GetAvailableComponentForOrder(GetAvailableComponentsRequest request)
        {
            _logger.LogInformation($"Request for PrecheckRepository:GetAvailableComponentForOrder{request}");
            var results = await _db.GetAll<GetAvailableComponentsResponse>(
            PrecheckQueries.GET_AVAILABLE_COMPONENT_ORDER,
            new
            {
                drawingnumberid = request.DrawingNumberId,
                // productionseriesid = request.ProdSeriesId,
                quantity = request.Quantity
            });
            _logger.LogInformation($"Result for PrecheckRepository:GetAvailableComponentsResponse{results}");
            return results.ToList();
        }

        public async Task<List<ProjectDetailsResponse>> ValidateOrder(int prodSeriesId, int drawingId, string pONumber, int idNumber)
        {
            _logger.LogInformation($"Request for QRCodeRepository:ValidateOrder{pONumber}");

            try
            {
                var results = await _db.GetAll<ProjectDetailsResponse>(
                    QRCodeQueries.VALIDATEMAKEORDERQUERY,
                    new
                    {
                        drawingId = drawingId,
                        prodSeriesId = prodSeriesId,
                        pONumber = pONumber,
                        idNumber = idNumber
                    });

                _logger.LogInformation("Successfully retrieved ValidateOrder", results);

                return results.ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while ValidateOrder.");
                throw;
            }
        }

        public async Task<int> RejectAndDuplicatePrecheck(Models.DTOs.Precheck.RejectPrecheckRequestDto request)
        {
            _logger.LogInformation($"Request for PrecheckRepository:RejectAndDuplicatePrecheck for PrecheckDetailsId: {request.PrecheckDetailsId}");
            try
            {
                var result = await _db.ExecuteTransaction(
                    PrecheckQueries.REJECT_AND_DUPLICATE_PRECHECK,
                    new
                    {
                        DrawingNumberId=request.DrawingNumberId,
                        PrecheckDetailsId = request.PrecheckDetailsId,
                        RejectedRemarks = request.RejectedRemarks,
                        DuplicateRemarks = request.DuplicateRemarks,
                        ComponentType = request.ComponentType,
                        CreatedBy = request.CreatedBy
                    });

                _logger.LogInformation($"Successfully rejected and duplicated precheck for PrecheckDetailsId: {request.PrecheckDetailsId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while RejectAndDuplicatePrecheck for PrecheckDetailsId: {request.PrecheckDetailsId}.");
                throw;
            }
        }


        public async Task<UpdateQuantityResponseDto> GetByProductionOrderNumberAsync(string productionOrderNumber)
        {
            _logger.LogInformation("Fetching ProductionOrderMaster by PO: {PO}", productionOrderNumber);
            try
            {
                var result = await _db.GetSingle<UpdateQuantityResponseDto>(
                    ProductionOrderQueries.GET_BY_PO_NUMBER,
                    new { ProductionOrderNumber = productionOrderNumber });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ProductionOrderMaster by PO");
                throw;
            }
        }

        public async Task<decimal> GetBatchTotalQuantity(UpdateMaterialQuantityRequestDto requestDto)
        {
            try
            {
                var ComponentDetails = await _db.GetSingle<dynamic>(
                         PrecheckQueries.GetRemainingQtyOfQrCode,
                         new
                         {
                             DrawingNumber = requestDto.DrawingnumberId,
                             ProductionOrderNumber = requestDto.ProductionOrderNumber,
                             IdNumber=requestDto.Idnumber,
                             QrCodeNumber=requestDto.QrCodeNumber,
                         });

                if (ComponentDetails == null)
                {
                    throw new ApplicationException(
                        $"QR Code with DrawingNumber {requestDto.DrawingnumberId} not found or inactive.");
                }

                // If remainingquantity is NULL or 0, return original quantity
                if (ComponentDetails.remainingquantity == null || ComponentDetails.remainingquantity == 0)
                {
                    return (decimal)ComponentDetails.quantity;
                }

                // Otherwise return remaining quantity
                return (decimal)ComponentDetails.remainingquantity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error fetching Component Details quantity for");
                throw;
            }
        }


        public async Task<decimal> UpdateComponentRemaningQuantity(UpdateMaterialQuantityRequestDto requestDto, decimal? remainingQuantity)
        {
            try
            {

                 await _db.GetSingle<decimal>(
                    PrecheckQueries.UPDATE_Componnet_INITIAL_Quantity,
                    new
                    {
                        DrawingNumberId = requestDto.DrawingnumberId,
                        ParentDrawingNumberId = requestDto.ParentDrawingNumber,
                        ProductionOrderNumber = requestDto.ProductionOrderNumber,
                        IdNumber = requestDto.Idnumber,
                        RemainingQuantity = remainingQuantity
                    });

                return await _db.GetSingle<decimal>(
                    PrecheckQueries.UPDATE_Componnet_REMAINING_Quantity,
                    new
                    {
                        DrawingNumberId = requestDto.DrawingnumberId,
                        ParentDrawingNumberId = requestDto.ParentDrawingNumber,
                        ProductionOrderNumber = requestDto.ProductionOrderNumber,
                        IdNumber=requestDto.Idnumber,
                        UpdatedQuantity =requestDto.UpdatedQuantity,
                        ComponentType = requestDto.ComponentType
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error fetching batch total quantity. AssemblyId: {AssemblyId}, DrawingId: {DrawingId}");
                throw;
            }
        }

        public async Task<int> UpdateQrcodeStatus(PrecheckRequestDto requestDto)
        {

            try
            {
                return await _db.GetSingle<int>(
                    PrecheckQueries.UPDATE_QrCodeStatus,
                    new
                    {
                        QrCodeNumber = requestDto.QrCodeNumber,
                        UpdatedQuantity=requestDto.UpdatedQuantity,
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error fetching batch total quantity. AssemblyId: {AssemblyId}, DrawingId: {DrawingId}");
                throw;
            }
        }

        public async Task<decimal> UpdateQrcodeQuantity(string qrCodeNumber, decimal newRemainingQuantity)
        {
            try
            {
                return await _db.GetSingle<decimal>(
                    PrecheckQueries.UPDATE_QrCodeRemaining_Quantity,
                    new
                    {
                        QrCodeNumber = qrCodeNumber,
                        RemainingQuantity = newRemainingQuantity

                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error fetching batch total quantity. AssemblyId: {AssemblyId}, DrawingId: {DrawingId}");
                throw;
            }
        }

        public async Task<int> PrecheckForRemainingQuantityServiceRepo(RejectPrecheckRequestDto request)
        {
            _logger.LogInformation($"Request for PrecheckRepository:RejectAndDuplicatePrecheck for PrecheckDetailsId: {request.PrecheckDetailsId}");
            try
            {
                var updatePreviousPrecheckRecord = await _db.GetSingle<int>(
                    PrecheckQueries.InActive_Previous_precheckRecord,
                    new
                    {

                        PrecheckDetailsId = request.PrecheckDetailsId,
                        CreatedBy = request.CreatedBy
                    });

                var newId = await _db.GetSingle<int>(
                      PrecheckQueries.PRECHECK_FOR_REMAINING_QUANTITY,
                      new
                      {
                          DrawingNumberId = request.DrawingNumberId,
                          PrecheckDetailsId = request.PrecheckDetailsId,
                          RejectedRemarks = request.RejectedRemarks,
                          DuplicateRemarks = request.DuplicateRemarks,
                          RemainingQuantity = request.RemainingQuantity,
                          ComponentType = request.ComponentType,
                          IdNumber=request.IdNumber,
                          CreatedBy = request.CreatedBy
                      });

                
                _logger.LogInformation($"Successfully rejected and duplicated precheck for PrecheckDetailsId: {request.PrecheckDetailsId}");
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while RejectAndDuplicatePrecheck for PrecheckDetailsId: {request.PrecheckDetailsId}.");
                throw;
            }
        }

        public async Task<bool> GetDrawingNumberIdAsync(int drawingNumberId)
        {
            _logger.LogInformation($"Request for PrecheckRepository:GetDrawingNumberIdAsync DrawingNumber: {drawingNumberId}");

            var result = await _db.ExecuteScalar<bool>(
                PrecheckQueries.GET_DRAWING_NUMBER_BY_NAME,
                new { drawingnumberid = drawingNumberId });

            _logger.LogInformation($"Result for PrecheckRepository:GetDrawingNumberIdAsync found: {result != null}");
            return result;
        }

        public async Task<bool> ResetRemainingQuantity(ResetRemainingQuantityDto remainingQuantityDto)
        {
            _logger.LogInformation($"Request for PrecheckRepository:ResetRemainingQuantity AssemblyNumber: {remainingQuantityDto.PONumber}, DrawingNumberId: {remainingQuantityDto.DrawingNumberId}, IdNumber: {remainingQuantityDto.IdNumber}, ScannedQuantity: {remainingQuantityDto.ScannedQuantity}");

            var result = await _db.Execute(
                PrecheckQueries.RESET_REMAINING_QUANTITY,
                new
                {
                    poNumber = remainingQuantityDto.PONumber,
                    drawingnumberid = remainingQuantityDto.DrawingNumberId,
                    idnumber = remainingQuantityDto.IdNumber,
                    scannedquantity = remainingQuantityDto.ScannedQuantity,
                    modifieddate = DateTime.UtcNow
                });

            var ResetQrQuantity = await _db.Execute(
               PrecheckQueries.Reset_QR_Quantity,
               new
               {
                   QrCodeNumber = remainingQuantityDto.QrCodeNumber,
                   scannedquantity = remainingQuantityDto.ScannedQuantity,
               });

            _logger.LogInformation($"Result for PrecheckRepository:ResetRemainingQuantity rowsAffected: {result}");
            return result > 0;
        }

        public async Task<int> UpdateQRCodeStatusQuantity(PrecheckDetailStatusResult precheckDetail)
        {
            _logger.LogInformation($"Request for PrecheckRepository:UpdateQRCodeStatusQuantity QRCodeId: {precheckDetail.QRCodeId}, Quantity: {precheckDetail.Quantity}");

            var result = await _db.Execute(
                PrecheckQueries.UPDATE_QR_QUANTITY_AND_STATUS,
                new
                {
                    QRCodeId = precheckDetail.QRCodeId,
                    Quantity = precheckDetail.Quantity,
                });

            _logger.LogInformation($"Result for PrecheckRepository:UpdateQRCodeStatusQuantity rowsAffected: {result}");
            return result;
        }

        public async Task<List<AssemblyProductionOrderResult>> GetAssemblyProductionOrdersByLnItemCode(string assemblyLnItemCode)
        {
            _logger.LogInformation($"Request for PrecheckRepository:GetAssemblyProductionOrdersByLnItemCode, AssemblyLnItemCode: {assemblyLnItemCode}");
            try
            {
                var results = await _db.GetAll<AssemblyProductionOrderResult>(
                    PrecheckQueries.GET_ASSEMBLY_PRODUCTION_ORDERS_BY_LNITEMCODE,
                    new { AssemblyLnItemCode = assemblyLnItemCode });

                _logger.LogInformation($"Response for PrecheckRepository:GetAssemblyProductionOrdersByLnItemCode: {results}");
                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching GetAssemblyProductionOrdersByLnItemCode.");
                throw;
            }
        }

        public async Task<List<ProjectDetailsIdResult>> GetProjectDetailsIdsByProductionOrderNumbers(List<string> productionOrderNumbers)
        {
            _logger.LogInformation($"Request for PrecheckRepository:GetProjectDetailsIdsByProductionOrderNumbers, Count: {productionOrderNumbers?.Count}");
            try
            {
                var results = await _db.GetAll<ProjectDetailsIdResult>(
                    PrecheckQueries.GET_PROJECTDETAILS_BY_PO_NUMBERS,
                    new { ProductionOrderNumbers = productionOrderNumbers });

                _logger.LogInformation($"Response for PrecheckRepository:GetProjectDetailsIdsByProductionOrderNumbers: {results}");
                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching GetProjectDetailsIdsByProductionOrderNumbers.");
                throw;
            }
        }

        public async Task<int> CreateProjectPrecheckDetailWithUnit(int drawingNumberId, int prodSeriesId, int projectDetailsId, decimal quantity, string unit, string componentType, int productionOrderNumberId, int createdBy)
        {
            _logger.LogInformation($"Request for PrecheckRepository:CreateProjectPrecheckDetailWithUnit, DrawingNumberId: {drawingNumberId}, ProjectDetailsId: {projectDetailsId}, Unit: {unit}");
            try
            {
                var insertedId = await _db.ExecuteScalar<int>(
                    PrecheckQueries.INSERT_PROJECT_PRECHECK_DETAILS_WITH_POID_AND_UNIT,
                    new
                    {
                        DrawingNumberId = drawingNumberId,
                        ProdSeriesId = prodSeriesId,
                        ProjectDetailsId = projectDetailsId,
                        Quantity = quantity,
                        Unit = unit,
                        ComponentType = componentType,
                        ProductionOrderNumberId = productionOrderNumberId,
                        CreatedBy = createdBy
                    });

                _logger.LogInformation($"Successfully inserted CreateProjectPrecheckDetailWithUnit: {insertedId}");
                return insertedId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting CreateProjectPrecheckDetailWithUnit.");
                throw;
            }
        }

        public async Task<List<ConsumedInAssemblyResult>> GetConsumedInAssemblies(int drawingNumberId)
        {
            _logger.LogInformation($"Request for PrecheckRepository:GetConsumedInAssemblies, DrawingNumberId: {drawingNumberId}");
            try
            {
                var results = await _db.GetAll<ConsumedInAssemblyResult>(
                    PrecheckQueries.GET_CONSUMED_IN_ASSEMBLIES,
                    new { DrawingNumberId = drawingNumberId });

                _logger.LogInformation($"Response for PrecheckRepository:GetConsumedInAssemblies: {results}");
                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching GetConsumedInAssemblies.");
                throw;
            }
        }

        public async Task<AssemblyChildBomResult?> GetAssemblyChildBomDetail(int assemblyDrawingNumberId, string childLnItemCode)
        {
            _logger.LogInformation($"Request for PrecheckRepository:GetAssemblyChildBomDetail, AssemblyDrawingNumberId: {assemblyDrawingNumberId}, ChildLnItemCode: {childLnItemCode}");
            try
            {
                var response = await _db.GetSingle<AssemblyChildBomResult?>(
                    PrecheckQueries.GET_ASSEMBLY_CHILD_BOM_DETAIL,
                    new { AssemblyDrawingNumberId = assemblyDrawingNumberId, ChildLnItemCode = childLnItemCode });

                _logger.LogInformation($"Response for PrecheckRepository:GetAssemblyChildBomDetail: {response}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching GetAssemblyChildBomDetail.");
                throw;
            }
        }
    }
}
