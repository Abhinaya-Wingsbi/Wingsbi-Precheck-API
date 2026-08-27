using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Repository.Database;
using Godrej.Precheck.Repository.Queries;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;

namespace Godrej.Precheck.Repository.Repository.IdentifierRepository
{
    public class IdentifierRepository : IIdentifierRepository
    {
        private readonly ILogger<IdentifierRepository> _logger;
        private readonly IApplicationDbContext _db;

        public IdentifierRepository(ILogger<IdentifierRepository> logger, IApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IRNumbers> InsertIRNumberAsync(IRNumbers irNumber)
        {
            _logger.LogInformation($"Request for IndentifierRepository:Inserting IRNumber details", irNumber);
            try
            {
                var CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                 TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                var InsertedId = await _db.ExecuteScalar<int>(
                    IdentifierQueries.InsertIRNumberQuery,
                    new
                    {
                        IrNumber=irNumber.IrNumber,
                        ProdSeriesId=irNumber.ProdSeriesId,
                        DrawingNumberId=irNumber.DrawingNumberId,
                        NomenclatureId = irNumber.NomenclatureId,
                        ComponentTypeId = irNumber.ComponentTypeId,
                        IdNumberStart=irNumber.IdNumberStart,
                        IdNumberEnd=irNumber.IdNumberEnd,
                        Quantity =irNumber.Quantity,
                        Remark=irNumber.Remark,
                        CreatedBy=irNumber.CreatedBy,
                        CreatedDate= CreatedDate,
                        ProductionOrderNumber=irNumber.ProductionOrderNumber,
                        PurchaseOrderNumber=irNumber.PurchaseOrderNumber,
                        Stage=irNumber.Stage,
                        StageId=irNumber.StageId,
                        ProjectNumber=irNumber.ProjectNumber,
                        Supplier=irNumber.Supplier,
                        Departmentid = irNumber.DepartmentId,
                        Idnumber = irNumber.IdNumberRange,
                        Sequenceno = irNumber.SequenceNo,
                        LnItemCode = irNumber.LnItemCode,
                        ItemDescription = irNumber.ItemDescription,
                        OperationNumber = irNumber.OperationNumber,
                        BuildNumber = irNumber.BuildNumber
                    });

                _logger.LogInformation($"Successfully inserted IRNumber details: {irNumber}");
                irNumber.Id = InsertedId;
                return irNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting IRNumber details.");
                throw;
            }
        }

        public async Task<MSNNumbers> InsertMSNNumberAsync(MSNNumbers msnNumber)
        {
            _logger.LogInformation($"Request for IndentifierRepository:Inserting MSNNumber details", msnNumber);
            try
            {
                var CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                var InsertedId = await _db.ExecuteScalar<int>(
                    IdentifierQueries.InsertMSNNumberQuery,
                    new
                    {
                        MSNNumber = msnNumber.MsnNumber,
                        ProdSeriesId=msnNumber.ProdSeriesId,
                        DrawingNumberId=msnNumber.DrawingNumberId,
                        NomenclatureId = msnNumber.NomenclatureId,
                        ComponentTypeId = msnNumber.ComponentTypeId,
                        IdNumberStart = msnNumber.IdNumberStart,
                        IdNumberEnd = msnNumber.IdNumberEnd,
                        Quantity = msnNumber.Quantity,
                        Remark = msnNumber.Remark,
                        CreatedBy = msnNumber.CreatedBy,
                        CreatedDate = CreatedDate,
                        ProductionOrderNumber = msnNumber.ProductionOrderNumber,
                        PurchaseOrderNumber = msnNumber.PurchaseOrderNumber,
                        Stage = msnNumber.Stage,
                        StageId = msnNumber.StageId,
                        ProjectNumber = msnNumber.ProjectNumber,
                        Supplier = msnNumber.Supplier,
                        Departmentid = msnNumber.DepartmentId,
                        Idnumber = msnNumber.IdNumberRange,
                        Sequenceno = msnNumber.SequenceNo,
                        LnItemCode = msnNumber.LnItemCode,
                        ItemDescription = msnNumber.ItemDescription,
                        OperationNumber = msnNumber.OperationNumber,
                        BuildNumber = msnNumber.BuildNumber
                    });

                _logger.LogInformation($"Successfully inserted MSNNumber details: {msnNumber}", msnNumber);
                msnNumber.Id = InsertedId;
                return msnNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting MSNNumber details.");
                throw;
            }
        }


        public async Task<IRNumbers> UpdateIRNumberAsync(IRNumbers irNumber)
        {
            _logger.LogInformation($"Request for IndentifierRepository:updating IRNumber details", irNumber);
            try
            {
                var ModifiedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                await _db.Update(
                    IdentifierQueries.UpdateIRNumberQuery,
                    new
                    {
                        IrNumber = irNumber.IrNumber,
                        DrawingNumberId = irNumber.DrawingNumberId,
                        LnItemCode = irNumber.LnItemCode,
                        IdNumberStart = irNumber.IdNumberStart,
                        IdNumberEnd = irNumber.IdNumberEnd,
                        IdNumberRange = irNumber.IdNumberRange,
                        Quantity = irNumber.Quantity,
                        Remark = irNumber.Remark,
                        Stage = irNumber.Stage,
                        StageId = irNumber.StageId,
                        Supplier = irNumber.Supplier,
                        ModifiedBy = irNumber.ModifiedBy,
                        ModifiedDate = ModifiedDate,
                        NomenclatureId = irNumber.NomenclatureId,
                        ComponentTypeId = irNumber.ComponentTypeId,
                        OperationNumber=irNumber.OperationNumber,
                    });

                _logger.LogInformation($"Successfully updated IRNumber details: {irNumber}");
                return irNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating IRNumber details.");
                throw;
            }
        }

        public async Task<MSNNumbers> UpdateMSNNumberAsync(MSNNumbers msnNumber)
        {
            _logger.LogInformation($"Request for IndentifierRepository:Inserting MSNNumber details", msnNumber);
            try
            {
                var ModifiedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                await _db.Update(
                    IdentifierQueries.UpdateMSNNumberQuery,
                    new
                    {
                        MSNNumber = msnNumber.MsnNumber,
                        DrawingNumberId = msnNumber.DrawingNumberId,
                        LnItemCode = msnNumber.LnItemCode,
                        IdNumberStart = msnNumber.IdNumberStart,
                        IdNumberEnd = msnNumber.IdNumberEnd,
                        IdNumberRange = msnNumber.IdNumberRange,
                        Quantity = msnNumber.Quantity,
                        Remark = msnNumber.Remark,
                        Stage = msnNumber.Stage,
                        StageId = msnNumber.StageId,
                        Supplier = msnNumber.Supplier,
                        ModifiedBy = msnNumber.ModifiedBy,
                        ModifiedDate = ModifiedDate,
                        NomenclatureId = msnNumber.NomenclatureId,
                        ComponentTypeId = msnNumber.ComponentTypeId,
                        OperationNumber=msnNumber.OperationNumber,
                    });

                _logger.LogInformation($"Successfully Updated MSNNumber details: {msnNumber}", msnNumber);
                return msnNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while Updating MSNNumber details.");
                throw;
            }
        }

        public async Task<bool> ExistsIrIdConflictAsync(int prodSeriesId, int drawingNumberId, int idStart, int idEnd, int departmentId, string operationNumber, int stageId)
        {
            _logger.LogInformation("Checking IR ID conflict for ProdSeriesId={ProdSeriesId}, DrawingNumberId={DrawingNumberId}, Range={IdStart}-{IdEnd}, DepartmentId={DepartmentId}, OperationNumber={OperationNumber}, StageId={StageId}", prodSeriesId, drawingNumberId, idStart, idEnd, departmentId, operationNumber, stageId);
            try
            {
                var result = await _db.GetSingle<int?>(
                    IdentifierQueries.VALIDATE_IR_ID_CONFLICT,
                    new
                    {
                        ProdSeriesId = prodSeriesId,
                        DrawingNumberId = drawingNumberId,
                        IdStart = idStart,
                        IdEnd = idEnd,
                        DepartmentId = departmentId,
                        OperationNumber = operationNumber,
                        StageId = stageId
                    });
                return result.HasValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking IR ID conflict.");
                throw;
            }
        }

        public async Task<bool> ExistsMsnIdConflictAsync(int prodSeriesId, int drawingNumberId, int idStart, int idEnd, int departmentId, string operationNumber, int stageId)
        {
            _logger.LogInformation("Checking MSN ID conflict for ProdSeriesId={ProdSeriesId}, DrawingNumberId={DrawingNumberId}, Range={IdStart}-{IdEnd}, DepartmentId={DepartmentId}, OperationNumber={OperationNumber}, StageId={StageId}", prodSeriesId, drawingNumberId, idStart, idEnd, departmentId, operationNumber, stageId);
            try
            {
                var result = await _db.GetSingle<int?>(
                    IdentifierQueries.VALIDATE_MSN_ID_CONFLICT,
                    new
                    {
                        ProdSeriesId = prodSeriesId,
                        DrawingNumberId = drawingNumberId,
                        IdStart = idStart,
                        IdEnd = idEnd,
                        DepartmentId = departmentId,
                        OperationNumber = operationNumber,
                        StageId = stageId
                    });
                return result.HasValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking MSN ID conflict.");
                throw;
            }
        }

        public async Task<string?> ExistsIrIdNumberAsync(int prodSeriesId, int drawingNumberId, string idNumber, int departmentId, string operationNumber, int stageId)
        {
            _logger.LogInformation("Checking IR ID number uniqueness for ProdSeriesId={ProdSeriesId}, DrawingNumberId={DrawingNumberId}, IdNumber={IdNumber}, DepartmentId={DepartmentId}, OperationNumber={OperationNumber}, StageId={StageId}", prodSeriesId, drawingNumberId, idNumber, departmentId, operationNumber, stageId);
            try
            {
                var result = await _db.GetSingle<string?>(
                    IdentifierQueries.VALIDATE_IR_ID_NUMBER_UNIQUENESS,
                    new
                    {
                        ProdSeriesId = prodSeriesId,
                        DrawingNumberId = drawingNumberId,
                        IdNumber = idNumber,
                        DepartmentId = departmentId,
                        OperationNumber = operationNumber,
                        StageId = stageId
                    });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking IR ID number uniqueness.");
                throw;
            }
        }

        public async Task<bool> ExistsMsnIdNumberAsync(int prodSeriesId, int drawingNumberId, string idNumber, int departmentId, string operationNumber, int stageId)
        {
            _logger.LogInformation("Checking MSN ID number uniqueness for ProdSeriesId={ProdSeriesId}, DrawingNumberId={DrawingNumberId}, IdNumber={IdNumber}, DepartmentId={DepartmentId}, OperationNumber={OperationNumber}, StageId={StageId}", prodSeriesId, drawingNumberId, idNumber, departmentId, operationNumber, stageId);
            try
            {
                var result = await _db.GetSingle<int?>(
                    IdentifierQueries.VALIDATE_MSN_ID_NUMBER_UNIQUENESS,
                    new
                    {
                        ProdSeriesId = prodSeriesId,
                        DrawingNumberId = drawingNumberId,
                        IdNumber = idNumber,
                        DepartmentId = departmentId,
                        OperationNumber = operationNumber,
                        StageId = stageId
                    });
                return result.HasValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking MSN ID number uniqueness.");
                throw;
            }
        }

        // Update Validation Methods
        public async Task<bool> ExistsIrIdConflictUpdateAsync(int prodSeriesId, int drawingNumberId, int idStart, int idEnd, int excludeId, int departmentId, string operationNumber, int stageId)
        {
            _logger.LogInformation("Checking IR ID conflict (Update) for ProdSeriesId={ProdSeriesId}, DrawingNumberId={DrawingNumberId}, Range={IdStart}-{IdEnd}, ExcludeId={ExcludeId}, DepartmentId={DepartmentId}, OperationNumber={OperationNumber}, StageId={StageId}", prodSeriesId, drawingNumberId, idStart, idEnd, excludeId, departmentId, operationNumber, stageId);
            try
            {
                var result = await _db.GetSingle<int?>(
                    IdentifierQueries.VALIDATE_IR_ID_CONFLICT_UPDATE,
                    new
                    {
                        ProdSeriesId = prodSeriesId,
                        DrawingNumberId = drawingNumberId,
                        IdStart = idStart,
                        IdEnd = idEnd,
                        ExcludeId = excludeId,
                        DepartmentId = departmentId,
                        OperationNumber = operationNumber,
                        StageId = stageId
                    });
                return result.HasValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking IR ID conflict (Update).");
                throw;
            }
        }

        public async Task<bool> ExistsMsnIdConflictUpdateAsync(int prodSeriesId, int drawingNumberId, int idStart, int idEnd, int excludeId, int departmentId, string operationNumber, int stageId)
        {
            _logger.LogInformation("Checking MSN ID conflict (Update) for ProdSeriesId={ProdSeriesId}, DrawingNumberId={DrawingNumberId}, Range={IdStart}-{IdEnd}, ExcludeId={ExcludeId}, DepartmentId={DepartmentId}, OperationNumber={OperationNumber}, StageId={StageId}", prodSeriesId, drawingNumberId, idStart, idEnd, excludeId, departmentId, operationNumber, stageId);
            try
            {
                var result = await _db.GetSingle<int?>(
                    IdentifierQueries.VALIDATE_MSN_ID_CONFLICT_UPDATE,
                    new
                    {
                        ProdSeriesId = prodSeriesId,
                        DrawingNumberId = drawingNumberId,
                        IdStart = idStart,
                        IdEnd = idEnd,
                        ExcludeId = excludeId,
                        DepartmentId = departmentId,
                        OperationNumber = operationNumber,
                        StageId = stageId
                    });
                return result.HasValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking MSN ID conflict (Update).");
                throw;
            }
        }

        public async Task<bool> ExistsIrIdNumberUpdateAsync(int prodSeriesId, int drawingNumberId, string idNumber, int excludeId, int departmentId, string operationNumber, int stageId)
        {
            _logger.LogInformation("Checking IR ID number uniqueness (Update) for ProdSeriesId={ProdSeriesId}, DrawingNumberId={DrawingNumberId}, IdNumber={IdNumber}, ExcludeId={ExcludeId}, DepartmentId={DepartmentId}, OperationNumber={OperationNumber}, StageId={StageId}", prodSeriesId, drawingNumberId, idNumber, excludeId, departmentId, operationNumber, stageId);
            try
            {
                var result = await _db.GetSingle<int?>(
                    IdentifierQueries.VALIDATE_IR_ID_NUMBER_UNIQUENESS_UPDATE,
                    new
                    {
                        ProdSeriesId = prodSeriesId,
                        DrawingNumberId = drawingNumberId,
                        IdNumber = idNumber,
                        ExcludeId = excludeId,
                        DepartmentId = departmentId,
                        OperationNumber = operationNumber,
                        StageId = stageId
                    });
                return result.HasValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking IR ID number uniqueness (Update).");
                throw;
            }
        }

        public async Task<bool> ExistsMsnIdNumberUpdateAsync(int prodSeriesId, int drawingNumberId, string idNumber, int excludeId, int departmentId, string operationNumber, int stageId)
        {
            _logger.LogInformation("Checking MSN ID number uniqueness (Update) for ProdSeriesId={ProdSeriesId}, DrawingNumberId={DrawingNumberId}, IdNumber={IdNumber}, ExcludeId={ExcludeId}, DepartmentId={DepartmentId}, OperationNumber={OperationNumber}, StageId={StageId}", prodSeriesId, drawingNumberId, idNumber, excludeId, departmentId, operationNumber, stageId);
            try
            {
                var result = await _db.GetSingle<int?>(
                    IdentifierQueries.VALIDATE_MSN_ID_NUMBER_UNIQUENESS_UPDATE,
                    new
                    {
                        ProdSeriesId = prodSeriesId,
                        DrawingNumberId = drawingNumberId,
                        IdNumber = idNumber,
                        ExcludeId = excludeId,
                        DepartmentId = departmentId,
                        OperationNumber = operationNumber,
                        StageId = stageId
                    });
                return result.HasValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking MSN ID number uniqueness (Update).");
                throw;
            }
        }

        public async Task<IRNumbers> InsertStandardIRNumberAsync(IRNumbers irNumber)
        {
            _logger.LogInformation($"Request for IdentifierRepository:Inserting Standard IRNumber details", irNumber);
            try
            {
                var CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                 TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                var InsertedId = await _db.ExecuteScalar<int>(
                    IdentifierQueries.InsertStandardIRNumberQuery,
                    new
                    {
                        IrNumber = irNumber.IrNumber,
                        ProdSeriesId = irNumber.ProdSeriesId,
                        DrawingNumberId = irNumber.DrawingNumberId,
                        NomenclatureId = irNumber.NomenclatureId,
                        ComponentTypeId = irNumber.ComponentTypeId,
                        Quantity = irNumber.Quantity,
                        ProjectNumber = irNumber.ProjectNumber,
                        ProductionOrderNumber = irNumber.ProductionOrderNumber,
                        PurchaseOrderNumber = irNumber.PurchaseOrderNumber,
                        Supplier = irNumber.Supplier,
                        Remark = irNumber.Remark,
                        ItemDescription = irNumber.ItemDescription,
                        LnItemCode = irNumber.LnItemCode,
                        IdNumber = irNumber.IdNumberRange,
                        StageId = irNumber.StageId,
                        Stage = irNumber.Stage,
                        CreatedBy = irNumber.CreatedBy,
                        Departmentid = irNumber.DepartmentId,
                        Sequenceno = irNumber.SequenceNo,
                        OperationNumber = irNumber.OperationNumber
                    });

                _logger.LogInformation($"Successfully inserted Standard IRNumber details: {irNumber}");
                irNumber.Id = InsertedId;
                return irNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting Standard IRNumber details.");
                throw;
            }
        }

        public async Task<MSNNumbers> InsertStandardMSNNumberAsync(MSNNumbers msnNumber)
        {
            _logger.LogInformation($"Request for IdentifierRepository:Inserting Standard MSNNumber details", msnNumber);
            try
            {
                var CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                var InsertedId = await _db.ExecuteScalar<int>(
                    IdentifierQueries.InsertStandardMSNNumberQuery,
                    new
                    {
                        MSNNumber = msnNumber.MsnNumber,
                        ProdSeriesId = msnNumber.ProdSeriesId,
                        DrawingNumberId = msnNumber.DrawingNumberId,
                        NomenclatureId = msnNumber.NomenclatureId,
                        ComponentTypeId = msnNumber.ComponentTypeId,
                        Quantity = msnNumber.Quantity,
                        ProjectNumber = msnNumber.ProjectNumber,
                        ProductionOrderNumber = msnNumber.ProductionOrderNumber,
                        PurchaseOrderNumber = msnNumber.PurchaseOrderNumber,
                        Supplier = msnNumber.Supplier,
                        Remark = msnNumber.Remark,
                        ItemDescription = msnNumber.ItemDescription,
                        LnItemCode = msnNumber.LnItemCode,
                        IdNumber = msnNumber.IdNumberRange,
                        StageId = msnNumber.StageId,
                        Stage = msnNumber.Stage,
                        CreatedBy = msnNumber.CreatedBy,
                        DepartmentId = msnNumber.DepartmentId,
                        Sequenceno = msnNumber.SequenceNo,
                        OperationNumber = msnNumber.OperationNumber
                    });

                _logger.LogInformation($"Successfully inserted Standard MSNNumber details: {msnNumber}", msnNumber);
                msnNumber.Id = InsertedId;
                return msnNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting Standard MSNNumber details.");
                throw;
            }
        }
        public async Task<string> GetDrawingNumberByID(int drawingNumberId)
        {
            try
            {
                var drawingNumber = await _db.GetSingle<string>(
                   IdentifierQueries.GET_DRAWING_NUMBER,
                    new { Id = drawingNumberId }
                );

                return drawingNumber ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting drawing number by ID: {DrawingNumberId}", drawingNumberId);
                throw;
            }
        }
    }
}
