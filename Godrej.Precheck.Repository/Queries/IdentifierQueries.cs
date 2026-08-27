using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Repository.Queries
{
    public static class IdentifierQueries
    {

        public static readonly string InsertIRNumberQuery = @"
            INSERT INTO tbl_IRNumber (ProdSeriesId, DrawingNumberId, NomenclatureId, ComponentTypeId, IrNumber, IdNumberStart, IdNumberEnd, Quantity, Remark, CreatedBy, CreatedDate,  isactive, ProductionOrderNumber, PurchaseOrderNumber, Stage, StageId, ProjectNumber, Supplier ,Departmentid, Idnumber,Sequenceno, LnItemCode, ItemDescription, OperationNumber, BuildNumber)
           OUTPUT INSERTED.Id VALUES (@ProdSeriesId, @DrawingNumberId, @NomenclatureId, @ComponentTypeId, @IrNumber, @IdNumberStart, @IdNumberEnd, @Quantity, @Remark, @CreatedBy, @CreatedDate, 1, @ProductionOrderNumber, @PurchaseOrderNumber, @Stage, @StageId, @ProjectNumber, @Supplier,@Departmentid, @Idnumber, @Sequenceno, @LnItemCode, @ItemDescription, @OperationNumber, @BuildNumber)";

        public const string InsertMSNNumberQuery = @"
           INSERT INTO tbl_MSNNumber (ProdSeriesId, DrawingNumberId, NomenclatureId, ComponentTypeId, MSNNumber, IdNumberStart, IdNumberEnd, Quantity, Remark, CreatedBy, CreatedDate,  isactive, ProductionOrderNumber, PurchaseOrderNumber, Stage, StageId, ProjectNumber, Supplier, Departmentid , Idnumber,Sequenceno, LnItemCode, ItemDescription, OperationNumber, BuildNumber)
           OUTPUT INSERTED.Id VALUES (@ProdSeriesId, @DrawingNumberId, @NomenclatureId, @ComponentTypeId, @MSNNumber, @IdNumberStart, @IdNumberEnd, @Quantity, @Remark, @CreatedBy, @CreatedDate,  1, @ProductionOrderNumber, @PurchaseOrderNumber, @Stage, @StageId, @ProjectNumber, @Supplier, @Departmentid, @Idnumber,@Sequenceno, @LnItemCode, @ItemDescription, @OperationNumber, @BuildNumber)";

        public static readonly string UpdateIRNumberQuery = @"UPDATE tbl_IRNumber
        SET  DrawingNumberId=@DrawingNumberId, LnItemCode=@LnItemCode, IdNumberStart=@IdNumberStart, IdNumberEnd=@IdNumberEnd, Quantity=@Quantity, Remark=@Remark,  Stage=@Stage, StageId=@StageId, Supplier=@Supplier, ModifiedBy=@ModifiedBy ,  ModifiedDate=@ModifiedDate , IdNumber=@IdNumberRange, NomenclatureId=@NomenclatureId, ComponentTypeId=@ComponentTypeId, operationnumber=@OperationNumber
        WHERE IRNumber=@IRNumber";

        public static readonly string UpdateMSNNumberQuery = @"UPDATE tbl_MSNNumber
        SET  DrawingNumberId=@DrawingNumberId, LnItemCode=@LnItemCode, IdNumberStart=@IdNumberStart, IdNumberEnd=@IdNumberEnd, Quantity=@Quantity, Remark=@Remark,  Stage=@Stage, StageId=@StageId, Supplier=@Supplier,ModifiedBy=@ModifiedBy ,  ModifiedDate=@ModifiedDate , IdNumber=@IdNumberRange, NomenclatureId=@NomenclatureId, ComponentTypeId=@ComponentTypeId,OperationNumber=@OperationNumber
        WHERE MSNNumber=@MSNNumber";

        // Validation: ensure no overlapping ID ranges exist for given ProdSeriesId + DrawingNumberId
        public static readonly string VALIDATE_IR_ID_CONFLICT = @"
            SELECT TOP 1 1
            FROM tbl_IRNumber ir
            WHERE ir.isactive = 1
              AND ir.ProdSeriesId = @ProdSeriesId
              AND ir.DrawingNumberId = @DrawingNumberId
              AND ir.Departmentid = @DepartmentId
              AND ir.OperationNumber = @OperationNumber
              AND ir.StageId = @StageId
              AND (
                    -- Overlap if either endpoint falls within existing range
                    (@IdStart BETWEEN ir.IdNumberStart AND ir.IdNumberEnd)
                    OR (@IdEnd BETWEEN ir.IdNumberStart AND ir.IdNumberEnd)
                    OR (ir.IdNumberStart BETWEEN @IdStart AND @IdEnd)
                    OR (ir.IdNumberEnd BETWEEN @IdStart AND @IdEnd)
                  )";

        public static readonly string VALIDATE_MSN_ID_CONFLICT = @"
            SELECT TOP 1 1
            FROM tbl_MSNNumber msn
            WHERE msn.isactive = 1
              AND msn.ProdSeriesId = @ProdSeriesId
              AND msn.DrawingNumberId = @DrawingNumberId
              AND msn.Departmentid = @DepartmentId
              AND msn.OperationNumber = @OperationNumber
              AND msn.StageId = @StageId
              AND (
                    (@IdStart BETWEEN msn.IdNumberStart AND msn.IdNumberEnd)
                    OR (@IdEnd BETWEEN msn.IdNumberStart AND msn.IdNumberEnd)
                    OR (msn.IdNumberStart BETWEEN @IdStart AND @IdEnd)
                    OR (msn.IdNumberEnd BETWEEN @IdStart AND @IdEnd)
                  )";

        // Enhanced validation: check for duplicate IR ID numbers by combination of ProdSeriesId, DrawingNumberId, and DocumentType
        public static readonly string VALIDATE_IR_ID_NUMBER_UNIQUENESS = @"
            SELECT TOP 1 ir.irnumber
            FROM tbl_IRNumber ir
            WHERE ir.isactive = 1
              AND ir.ProdSeriesId = @ProdSeriesId
              AND ir.DrawingNumberId = @DrawingNumberId
              AND ir.idnumber = @IdNumber
              AND ir.Departmentid = @DepartmentId
              AND ir.OperationNumber = @OperationNumber
              AND ir.StageId = @StageId";

        // Enhanced validation: check for duplicate MSN ID numbers by combination of ProdSeriesId, DrawingNumberId, and DocumentType
        public static readonly string VALIDATE_MSN_ID_NUMBER_UNIQUENESS = @"
            SELECT TOP 1 1
            FROM tbl_MSNNumber msn
            WHERE msn.isactive = 1
              AND msn.ProdSeriesId = @ProdSeriesId
              AND msn.DrawingNumberId = @DrawingNumberId
              AND msn.idnumber = @IdNumber
              AND msn.Departmentid = @DepartmentId
              AND msn.OperationNumber = @OperationNumber
              AND msn.StageId = @StageId";

        // Update Validation: ensure no overlapping ID ranges exist for given ProdSeriesId + DrawingNumberId, excluding current ID
        public static readonly string VALIDATE_IR_ID_CONFLICT_UPDATE = @"
            SELECT TOP 1 1
            FROM tbl_IRNumber ir
            WHERE ir.isactive = 1
              AND ir.ProdSeriesId = @ProdSeriesId
              AND ir.DrawingNumberId = @DrawingNumberId
              AND ir.Departmentid = @DepartmentId
              AND ir.OperationNumber = @OperationNumber
              AND ir.StageId = @StageId
              AND ir.Id != @ExcludeId
              AND (
                    (@IdStart BETWEEN ir.IdNumberStart AND ir.IdNumberEnd)
                    OR (@IdEnd BETWEEN ir.IdNumberStart AND ir.IdNumberEnd)
                    OR (ir.IdNumberStart BETWEEN @IdStart AND @IdEnd)
                    OR (ir.IdNumberEnd BETWEEN @IdStart AND @IdEnd)
                  )";

        public static readonly string VALIDATE_MSN_ID_CONFLICT_UPDATE = @"
            SELECT TOP 1 1
            FROM tbl_MSNNumber msn
            WHERE msn.isactive = 1
              AND msn.ProdSeriesId = @ProdSeriesId
              AND msn.DrawingNumberId = @DrawingNumberId
              AND msn.Departmentid = @DepartmentId
              AND msn.OperationNumber = @OperationNumber
              AND msn.StageId = @StageId
              AND msn.Id != @ExcludeId
              AND (
                    (@IdStart BETWEEN msn.IdNumberStart AND msn.IdNumberEnd)
                    OR (@IdEnd BETWEEN msn.IdNumberStart AND msn.IdNumberEnd)
                    OR (msn.IdNumberStart BETWEEN @IdStart AND @IdEnd)
                    OR (msn.IdNumberEnd BETWEEN @IdStart AND @IdEnd)
                  )";

        public static readonly string VALIDATE_IR_ID_NUMBER_UNIQUENESS_UPDATE = @"
            SELECT TOP 1 1
            FROM tbl_IRNumber ir
            WHERE ir.isactive = 1
              AND ir.ProdSeriesId = @ProdSeriesId
              AND ir.DrawingNumberId = @DrawingNumberId
              AND ir.Id != @ExcludeId
              AND ir.idnumber = @IdNumber
              AND ir.Departmentid = @DepartmentId
              AND ir.OperationNumber = @OperationNumber
              AND ir.StageId = @StageId";

        public static readonly string VALIDATE_MSN_ID_NUMBER_UNIQUENESS_UPDATE = @"
            SELECT TOP 1 1
            FROM tbl_MSNNumber msn
            WHERE msn.isactive = 1
              AND msn.ProdSeriesId = @ProdSeriesId
              AND msn.DrawingNumberId = @DrawingNumberId
              AND msn.Id != @ExcludeId
              AND msn.idnumber = @IdNumber
              AND msn.Departmentid = @DepartmentId
              AND msn.OperationNumber = @OperationNumber
              AND msn.StageId = @StageId";

        // Purchase Item (Standard) IR Number insertion
        public static readonly string InsertStandardIRNumberQuery = @"
            INSERT INTO tbl_IRNumber (
                ProdSeriesId, DrawingNumberId, NomenclatureId, ComponentTypeId, IrNumber, Quantity, 
                ProjectNumber, ProductionOrderNumber, PurchaseOrderNumber, Supplier, Remark,
                ItemDescription, LnItemCode, IdNumber, StageId, Stage,
                CreatedBy, CreatedDate, isactive, Departmentid, Sequenceno, OperationNumber
            )
            OUTPUT INSERTED.Id 
            VALUES (
                @ProdSeriesId, @DrawingNumberId, @NomenclatureId, @ComponentTypeId, @IrNumber, @Quantity,
                @ProjectNumber, @ProductionOrderNumber, @PurchaseOrderNumber, @Supplier, @Remark,
                @ItemDescription, @LnItemCode, @IdNumber, @StageId, @Stage,
                @CreatedBy, GETDATE(), 1, @Departmentid, @Sequenceno, @OperationNumber
            )";

        // Purchase Item (Standard) MSN Number insertion
        public static readonly string InsertStandardMSNNumberQuery = @"
            INSERT INTO tbl_MSNNumber (
                ProdSeriesId, DrawingNumberId, NomenclatureId, ComponentTypeId, MSNNumber, Quantity,
                ProjectNumber, ProductionOrderNumber, PurchaseOrderNumber, Supplier, Remark,
                ItemDescription, LnItemCode, IdNumber, StageId, Stage,
                CreatedBy, CreatedDate, isactive, Departmentid, Sequenceno, OperationNumber
            )
            OUTPUT INSERTED.Id 
            VALUES (
                @ProdSeriesId, @DrawingNumberId, @NomenclatureId, @ComponentTypeId, @MSNNumber, @Quantity,
                @ProjectNumber, @ProductionOrderNumber, @PurchaseOrderNumber, @Supplier, @Remark,
                @ItemDescription, @LnItemCode, @IdNumber, @StageId, @Stage,
                @CreatedBy, GETDATE(), 1, @Departmentid, @Sequenceno, @OperationNumber
            )";

        public static readonly string GET_DRAWING_NUMBER = @"SELECT drawingnumber FROM tbl_drawingnumber WHERE Id = @Id;";
    }
}
