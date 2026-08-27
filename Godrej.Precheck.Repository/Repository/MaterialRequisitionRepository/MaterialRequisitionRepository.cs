using Azure.Core;
using Godrej.Precheck.Models.DataModel.MaterialRequisition;
using Godrej.Precheck.Models.DTOs.MaterialRequisition;
using Godrej.Precheck.Repository.Database;
using Godrej.Precheck.Repository.Queries;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Repository.Repository.MaterialRequisitionRepository
{
    public class MaterialRequisitionRepository : IMaterialRequisitionRepository
    {
        private readonly ILogger<MaterialRequisitionRepository> _logger;
        private readonly IApplicationDbContext _db;

        public MaterialRequisitionRepository(ILogger<MaterialRequisitionRepository> logger, IApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<List<MaterialRequisitionResponse>> GetMaterialRequisitions()
        {
            _logger.LogInformation("Request for MaterialRequisitionRepository:GetMaterialRequisitions");
            try
            {
                var results = await _db.GetAll<MaterialRequisitionResponse>(
                    MaterialRequisitionQueries.GET_MATERIAL_REQUISITION,
                    new { });

                _logger.LogInformation($"Result for MaterialRequisitionRepository:GetMaterialRequisitions - Found {results.Count()} records");
                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetMaterialRequisitions.");
                throw;
            }
        }

        public async Task<List<SwappingDetailsResponse>> GetSwappingDetails()
        {
            _logger.LogInformation("Request for MaterialRequisitionRepository:GetSwappingDetails");
            try
            {
                var results = await _db.GetAll<SwappingDetailsResponse>(
                    MaterialRequisitionQueries.GET_SWAPPING_DETAILS,
                    new { });

                _logger.LogInformation("Result for MaterialRequisitionRepository:GetSwappingDetails - Found {Count} records", results.Count());
                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetSwappingDetails.");
                throw;
            }
        }

        public async Task<List<MaterialRequisitionResponse>> GetMaterialRequisitionsByStatus(string status,int statusId)
        {
            _logger.LogInformation($"Request for MaterialRequisitionRepository:GetMaterialRequisitionsByStatus with status: {status}");
            try
            {
                var results = await _db.GetAll<MaterialRequisitionResponse>(
                    MaterialRequisitionQueries.GET_MATERIAL_REQUISITION_BY_STATUS,
                    new { 
                        StatusId = statusId,
                        Status=status
                    });

                _logger.LogInformation($"Result for MaterialRequisitionRepository:GetMaterialRequisitionsByStatus - Found {results.Count()} records");
                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while GetMaterialRequisitionsByStatus for status: {status}");
                throw;
            }
        }

        public async Task<int> UpdateMaterialRequisition(UpdateMaterialRequisitionRequestDto request, int modifiedBy)
        {
            _logger.LogInformation($"Request for MaterialRequisitionRepository:UpdateMaterialRequisition for MaterialRequisitionId: {request.MaterialRequisitionId}");
            try
            {
                var result = await _db.Update(
                    MaterialRequisitionQueries.UPDATE_MATERIAL_REQUISITION,
                    new
                    {
                        MaterialRequisitionId = request.MaterialRequisitionId,
                        Remarks = request.Remarks,
                        Hwno = request.Hwno,
                        RequestOwner = request.RequestOwner,
                        Status = request.Status,
                        StatusId=request.StatusId,
                        ModifiedBy = modifiedBy
                    });

                _logger.LogInformation($"Successfully updated MaterialRequisition for MaterialRequisitionId: {request.MaterialRequisitionId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while UpdateMaterialRequisition for MaterialRequisitionId: {request.MaterialRequisitionId}.");
                throw;
            }
        }

        public async Task<int> CancelMaterialRequisition(CancelMaterialRequisitionRequestDto request, int modifiedBy)
        {
            _logger.LogInformation($"Request for MaterialRequisitionRepository:CancelMaterialRequisition for RequestId: {request.RequestId}");
            try
            {
                var result = await _db.Update(
                    MaterialRequisitionQueries.CANCEL_MATERIAL_REQUISITION,
                    new
                    {
                        RequestId = request.RequestId,
                        RequestCancleRemarks = request.RequestCancleRemarks,
                        ModifiedBy = modifiedBy
                    });

                _logger.LogInformation($"Successfully cancelled MaterialRequisition for RequestId: {request.RequestId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while CancelMaterialRequisition for RequestId: {request.RequestId}.");
                throw;
            }
        }

        public async Task<(int NewId, string RequestNumber)> CreateMaterialRequisition(CreateMaterialRequisitionRequestDto request, int createdBy)
        {
            _logger.LogInformation($"Request for MaterialRequisitionRepository:CreateMaterialRequisition - DrawingNumberId: {request.RejectedDrawingNumberId}, ProdSeriesId: {request.ProdSeriesId}, IdNumber: {request.IdNumber}");
            try
            {
                var result = await _db.GetSingle<dynamic>(
                    MaterialRequisitionQueries.CREATE_MATERIAL_REQUISITION,
                    new
                    {
                        RejectedDrawingNumberId = request.RejectedDrawingNumberId,
                        ProdSeriesId = request.ProdSeriesId,
                        IdNumber = request.IdNumber,
                        Remarks = request.Remarks,
                        Quantity=request.Quantity,
                        Nomenclature=request.Nomenclature,
                        AssemblyDrawingNumberId = request.AssemblyDrawingNumberId,
                        ProductionOrderNumber=request.ProductionOrderNumber,
                        LnItemCode = request.lnitemcode,
                        CreatedBy = createdBy ,
                        IdNumbers= request.RejectedIdNumber
                    });

                int newId = Convert.ToInt32(result.NewId);
                string requestNumber = result.RequestNumber;

                _logger.LogInformation($"Successfully created MaterialRequisition with Id: {newId}, RequestNumber: {requestNumber}");
                return (newId, requestNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while CreateMaterialRequisition. DrawingNumberId: {request.RejectedDrawingNumberId}, ProdSeriesId: {request.ProdSeriesId}, IdNumber: {request.IdNumber}");
                throw;
            }
        }

     

        public async Task<int> InActiveProjectPrecheckDetails(CreateSwappedDrawingNumberRequestDto request, int createdBy)
        {
            _logger.LogInformation(
                "Request for MaterialRequisitionRepository:CreateSwappedDrawingNumber - SwapTransactionID: {SwapTransactionID}, SwappedDrawingNumberID: {SwappedDrawingNumberID}",
                request.SwappedDrawingNumberID);
            try
            {
                //Get id from tbl_projectprecheckdetails based on swapped drawing number, id number and po number
                int id = await _db.GetSingle<int>(
                    MaterialRequisitionQueries.Get_Project_PrecheckDetailsIdTo,
                    new
                    {
                        DrawingNumberId=request.SwappedDrawingNumberID,
                        IdNumber=request.FromSwappedIdNumber,
                        PoNumber=request.SwappedFromPONumber,
                        DrawingIdNumber = request.IdNumber,
                        CreatedBy = createdBy
                    });

                //InActive record in tbl_projectprececkdetails based on id and duplicate record based on swapped drawing number, id number and po number
                await _db.ExecuteScalar<int>(MaterialRequisitionQueries.INACTIVE_PREVIOUS_AND_DUPLICATE_SOURCE_PO,
                    new
                    {
                        Id = id,
                        CreatedBy = createdBy
                    });


                //Insert record in tbl_SwappingDetails record.
                var result = await _db.Update(
                    MaterialRequisitionQueries.CREATE_SWAPPED_DRAWING_NUMBER,
                    new
                    {
                        request.SwappedDrawingNumberID,
                        request.FromSwappedIdNumber,
                        request.ToSwappedIdNumber,
                        request.SwappedFromPONumber,
                        request.SwappedToPONumber,
                        CreatedBy = createdBy
                    });


                _logger.LogInformation(
                    "Successfully created swapped drawing number for SwapTransactionID: {SwapTransactionID}"
                    );
                return id;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while CreateSwappedDrawingNumber. SwapTransactionID: {SwapTransactionID}, SwappedDrawingNumberID: {SwappedDrawingNumberID}",
                    request.SwappedDrawingNumberID);
                throw;
            }
        }


        public async Task<int> SwapNewComponentInNewAssembly(CreateSwappedDrawingNumberRequestDto request, int createdBy)
        {
            _logger.LogInformation(
               "InActive and duplicate insret in tbl_projectprececkdetails ");

            try
            {

                //Get id from tbl_projectprecheckdetails based on swapped drawing number, id number and po number
                int id = await _db.GetSingle<int>(
                    MaterialRequisitionQueries.Get_Project_PrecheckDetailsId,
                    new
                    {
                        DrawingNumberId = request.SwappedDrawingNumberID,
                        IdNumber = request.ToSwappedIdNumber,
                        PoNumber = request.SwappedToPONumber,
                     
                        CreatedBy = createdBy
                    });

                //Get projectdetails of target po and idnumber
                int targetProjectDetailsId = await _db.GetSingle<int>(
                    MaterialRequisitionQueries.Get_Project_DetailsId,
                    new
                    {
                      
                        IdNumber = request.ToSwappedIdNumber,
                        PoNumber = request.SwappedToPONumber,
                        CreatedBy = createdBy
                    });



                //Get projectprecheckdetails id of source drawing number, id number and po number
                int ProjectPrecheckDetailsId = await _db.GetSingle<int>(
                    MaterialRequisitionQueries.Get_Project_PrecheckDetailsId,
                    new
                    {
                        DrawingNumberId = request.SwappedDrawingNumberID,
                        IdNumber = request.FromSwappedIdNumber,
                        PoNumber = request.SwappedFromPONumber,
                        CreatedBy = createdBy,
                        DrawingIdNumber = request.IdNumber
                    });

                //InActive record in tbl_projectprececkdetails based on id and duplicate record based on swapped drawing number, id number and po number
                await _db.ExecuteScalar<int>(MaterialRequisitionQueries.INACTIVE_PREVIOUS_AND_DUPLICATE,
                    new
                    {
                        Id = id,
                        ProjectPrecheckDetailsId= ProjectPrecheckDetailsId,
                        TargetProjectDetailsId= targetProjectDetailsId,
                        CreatedBy = createdBy
                    });


                _logger.LogInformation(
                    "Successfully InActive and insert duplicate drawing number for SwapTransactionID: {SwapTransactionID}");

                return targetProjectDetailsId;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while CreateSwappedDrawingNumber. SwapTransactionID: {SwapTransactionID}, SwappedDrawingNumberID: {SwappedDrawingNumberID}"
                   );
                throw;
            }
        }

        public async Task<int> CheckComponentType(int drawingNumberId)
        {
            _logger.LogInformation(
              "Check componenttype of Drawingnumberid: {DrawingNumberId}", drawingNumberId);

            try
            {

                _logger.LogInformation(
                    "Successfully checked component type for Drawingnumberid: {DrawingNumberId}", drawingNumberId);

                var result= await _db.GetSingle<int>(
                    MaterialRequisitionQueries.CHECK_COMPONENT_TYPE,
                    new
                    {
                        DrawingNumberId = drawingNumberId
                    });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while checking component type for Drawingnumberid: {DrawingNumberId }"
                   );
                throw;
            }
        }
    }
}


