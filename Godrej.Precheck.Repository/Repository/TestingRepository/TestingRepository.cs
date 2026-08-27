using Dapper;
using Godrej.Precheck.Models.DTOs.Testing;
using Godrej.Precheck.Repository.Database;
using Godrej.Precheck.Repository.Queries;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Godrej.Precheck.Repository.Repository.TestingRepository
{
    public class TestingRepository : ITestingRepository
    {
        private readonly ILogger<TestingRepository> _logger;
        private readonly IApplicationDbContext _db;

        public TestingRepository(ILogger<TestingRepository> logger, IApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<List<TemplateFieldDto>> GetTemplateFieldsByDrawingNumberAsync(string drawingNumber, string? msnNumber, int? stageId = null)
        {
            _logger.LogInformation("Getting template fields for DrawingNumber: {DrawingNumber}, MsnNumber: {MsnNumber}, StageId: {StageId}", drawingNumber, msnNumber, stageId);

            try
            {
                var result = await _db.QueryAsync<TemplateFieldDto>(
                    TestingQueries.GET_TEMPLATE_FIELDS_WITH_VALUES_BY_DRAWING_NUMBER,
                    new { DrawingNumber = drawingNumber, MsnNumber = msnNumber, StageId = stageId });

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting template fields for DrawingNumber: {DrawingNumber}", drawingNumber);
                throw;
            }
        }

        public async Task<bool> CheckTemplateExistsAsync(int templateId)
        {
            _logger.LogInformation("Checking if template exists for TemplateId: {TemplateId}", templateId);

            try
            {
                var result = await _db.GetSingle<int?>(
                    TestingQueries.CHECK_TEMPLATE_EXISTS,
                    new { TemplateId = templateId });

                return result.HasValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking template existence for TemplateId: {TemplateId}", templateId);
                throw;
            }
        }

        public async Task<List<TemplateFieldDto>> GetTemplateFieldsByTemplateIdAsync(int templateId)
        {
            _logger.LogInformation("Getting template fields for TemplateId: {TemplateId}", templateId);

            try
            {
                var result = await _db.QueryAsync<TemplateFieldDto>(
                    TestingQueries.GET_TEMPLATE_FIELDS_BY_TEMPLATE_ID,
                    new { TemplateId = templateId });

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting template fields for TemplateId: {TemplateId}", templateId);
                throw;
            }
        }

        public async Task<int?> GetDrawingIdByDrawingNumberAsync(string drawingNumber)
        {
            _logger.LogInformation("Getting drawing id for DrawingNumber: {DrawingNumber}", drawingNumber);

            try
            {
                return await _db.ExecuteScalar<int?>(
                    TestingQueries.GET_DRAWING_ID,
                    new { DrawingNumber = drawingNumber });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drawing id for DrawingNumber: {DrawingNumber}", drawingNumber);
                throw;
            }
        }

        public async Task<int> InsertInspectionValuesAsync(
            int templateId,
            string drawingNumber,
            List<TemplateFieldValueInsertDto> values)
        {
            _logger.LogInformation(
                "Inserting inspection values for TemplateId: {TemplateId}, DrawingNumber: {DrawingNumber}, Count: {Count}",
                templateId, drawingNumber, values.Count);

            try
            {

                var drawingId = await _db.ExecuteScalar<int?>(
                    TestingQueries.GET_DRAWING_ID,
                    new { DrawingNumber = drawingNumber });

                if (drawingId == null)
                    throw new ValidationException($"Drawing number '{drawingNumber}' was not found.");

                var sql = BuildInsertSql(values.Count);

                var parameters = new DynamicParameters();
                parameters.Add("@DrawingId", drawingId.Value);   
                parameters.Add("@TemplateId", templateId);
                parameters.Add("@DrawingNumber", drawingNumber);

                for (var i = 0; i < values.Count; i++)
                {
                    parameters.Add($"@FieldId{i}", values[i].FieldId);
                    parameters.Add($"@FieldValue{i}", values[i].Value);
                }

                return await _db.ExecuteScalar<int>(sql, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error inserting inspection values for TemplateId: {TemplateId}, DrawingNumber: {DrawingNumber}",
                    templateId, drawingNumber);
                throw;
            }
        }

        public async Task<int?> GetTemplateIdByDrawingNumberAsync(string drawingNumber)
        {
            return await _db.ExecuteScalar<int?>(
                TestingQueries.GET_TEMPLATE_ID_BY_DRAWING_NUMBER,
                new { DrawingNumber = drawingNumber });
        }

        public static string BuildInsertSql(int valueCount)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DECLARE @MasterId INT;");
            sb.AppendLine();
            sb.AppendLine("BEGIN TRY");
            sb.AppendLine("    BEGIN TRANSACTION;");
            sb.AppendLine();

            // Insert into tbl_inspection_master
            sb.AppendLine("    INSERT INTO tbl_inspection_master");
            sb.AppendLine("        (drawing_id, template_id, drawing_number, isactive, createddate)");
            sb.AppendLine("    VALUES");
            sb.AppendLine("        (@DrawingId, @TemplateId, @DrawingNumber, 1, GETDATE());");
            sb.AppendLine();
            sb.AppendLine("    SET @MasterId = CAST(SCOPE_IDENTITY() AS INT);");
            sb.AppendLine();
            sb.AppendLine("    IF @MasterId IS NULL");
            sb.AppendLine("        THROW 50004, 'Inspection master insert failed.', 1;");
            sb.AppendLine();

            // Insert each field value directly linked to master
            for (var i = 0; i < valueCount; i++)
            {
                sb.AppendLine("    INSERT INTO tbl_inspection_row_values");
                sb.AppendLine("        (row_master_id, fieldId, field_value, isactive, createddate)");
                sb.AppendLine("    VALUES");
                sb.AppendLine($"        (@MasterId, @FieldId{i}, @FieldValue{i}, 1, GETDATE());");
                sb.AppendLine();
            }

            sb.AppendLine("    COMMIT TRANSACTION;");
            sb.AppendLine("    SELECT @MasterId;");
            sb.AppendLine("END TRY");
            sb.AppendLine("BEGIN CATCH");
            sb.AppendLine("    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;");
            sb.AppendLine("    THROW;");
            sb.AppendLine("END CATCH;");

            return sb.ToString();
        }


        public async Task<int?> GetMasterIdByDrawingNumberAsync(string drawingNumber)
        {
            _logger.LogInformation("Getting master id for DrawingNumber: {DrawingNumber}", drawingNumber);
            try
            {
                return await _db.ExecuteScalar<int?>(
                    TestingQueries.GET_MASTER_ID_BY_DRAWING,
                    new { DrawingNumber = drawingNumber });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting master id for DrawingNumber: {DrawingNumber}", drawingNumber);
                throw;
            }
        }

        public async Task<InspectionExportDataDto?> GetInspectionExportDataAsync(string drawingNumber, string? msnNumber = null)
        {
            _logger.LogInformation("Getting export data for DrawingNumber: {DrawingNumber}, MsnNumber: {MsnNumber}", drawingNumber, msnNumber);
            try
            {
                var result = await _db.QueryAsync<InspectionExportDataDto>(
                    TestingQueries.GET_INSPECTION_FOR_EXPORT,
                    new { DrawingNumber = drawingNumber, MsnNumber = msnNumber });
                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting export data for DrawingNumber: {DrawingNumber}", drawingNumber);
                throw;
            }
        }

        public async Task<List<FieldValueExportDto>> GetFieldValuesForExportAsync(string drawingNumber, string? msnNumber = null)
        {
            _logger.LogInformation("Getting field values for DrawingNumber: {DrawingNumber}, MsnNumber: {MsnNumber}", drawingNumber, msnNumber);
            try
            {
                var result = await _db.QueryAsync<FieldValueExportDto>(
                    TestingQueries.GET_FIELD_VALUES_FOR_EXPORT,
                    new { DrawingNumber = drawingNumber, MsnNumber = msnNumber });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting field values for DrawingNumber: {DrawingNumber}", drawingNumber);
                throw;
            }
        }

        public async Task<List<DrawingStageStatusDto>> GetDrawingStageStatusAsync()
        {
            _logger.LogInformation("Getting drawing stage status list.");
            try
            {
                var result = await _db.QueryAsync<DrawingStageStatusDto>(
                    TestingQueries.GET_DRAWING_STAGE_STATUS, new { });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drawing stage status list.");
                throw;
            }
        }

        public async Task<List<TemplateFieldDto>> GetFixedFieldsAsync(int templateId)
        {
            _logger.LogInformation("Getting fixed fields for TemplateId: {TemplateId}", templateId);
            try
            {
                var result = await _db.QueryAsync<TemplateFieldDto>(
                    TestingQueries.GET_FIXED_FIELDS_BY_TEMPLATE,
                    new { TemplateId = templateId });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fixed fields for TemplateId: {TemplateId}", templateId);
                throw;
            }
        }

        public async Task<List<StageRowValueRawDto>> GetFixedFieldValuesByDrawingAsync(string drawingNumber, string msnNumber)
        {
            _logger.LogInformation("Getting fixed field values for DrawingNumber: {DrawingNumber}, MsnNumber: {MsnNumber}", drawingNumber, msnNumber);
            try
            {
                var result = await _db.QueryAsync<StageRowValueRawDto>(
                    TestingQueries.GET_FIXED_FIELD_VALUES_BY_DRAWING,
                    new { DrawingNumber = drawingNumber, MsnNumber = msnNumber });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fixed field values for DrawingNumber: {DrawingNumber}", drawingNumber);
                throw;
            }
        }

        public async Task<List<StageRowValueRawDto>> GetStageRowValuesByDrawingAsync(string drawingNumber, string msnNumber, int stageId)
        {
            _logger.LogInformation(
                "Getting stage row values for DrawingNumber: {DrawingNumber}, MsnNumber: {MsnNumber}, StageId: {StageId}",
                drawingNumber, msnNumber, stageId);
            try
            {
                var result = await _db.QueryAsync<StageRowValueRawDto>(
                    TestingQueries.GET_STAGE_ROW_VALUES_BY_DRAWING,
                    new { DrawingNumber = drawingNumber, MsnNumber = msnNumber, StageId = stageId });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting stage row values for DrawingNumber: {DrawingNumber}, StageId: {StageId}",
                    drawingNumber, stageId);
                throw;
            }
        }

        public async Task<List<TemplateFieldDto>> GetStageFieldsAsync(int templateId, int stageId)
        {
            _logger.LogInformation(
                "Getting stage fields for TemplateId: {TemplateId}, StageId: {StageId}",
                templateId, stageId);
            try
            {
                var result = await _db.QueryAsync<TemplateFieldDto>(
                    TestingQueries.GET_STAGE_FIELDS_BY_TEMPLATE_STAGE,
                    new { TemplateId = templateId, StageId = stageId });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting stage fields for TemplateId: {TemplateId}, StageId: {StageId}",
                    templateId, stageId);
                throw;
            }
        }

        public async Task<int> SaveFixedFieldsAsync(string drawingNumber, string msnNumber, List<StageRowValueInsertDto> fixedValues)
        {
            _logger.LogInformation(
                "Saving fixed fields for DrawingNumber: {DrawingNumber}, MsnNumber: {MsnNumber}, Count: {Count}",
                drawingNumber, msnNumber, fixedValues.Count);
            try
            {
                var sql = BuildSaveFixedFieldsSql(fixedValues.Count);
                var parameters = new DynamicParameters();
                parameters.Add("@DrawingNumber", drawingNumber);
                parameters.Add("@MsnNumber", msnNumber);

                for (var i = 0; i < fixedValues.Count; i++)
                {
                    parameters.Add($"@FixedFieldId{i}", fixedValues[i].FieldId);
                    parameters.Add($"@FixedValue{i}", fixedValues[i].Value);
                }

                return await _db.ExecuteScalar<int>(sql, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving fixed fields for DrawingNumber: {DrawingNumber}", drawingNumber);
                throw;
            }
        }

        public async Task<int> SaveRowDataAsync(
            string drawingNumber,
            string msnNumber,
            int stageId,
            int rowNumber,
            int totalRows,
            List<StageRowValueInsertDto> fieldValues)
        {
            _logger.LogInformation(
                "Saving row {RowNumber} for DrawingNumber: {DrawingNumber}, MsnNumber: {MsnNumber}, StageId: {StageId}",
                rowNumber, drawingNumber, msnNumber, stageId);
            try
            {
                var sql = BuildSaveRowDataSql(fieldValues.Count);
                var parameters = new DynamicParameters();
                parameters.Add("@DrawingNumber", drawingNumber);
                parameters.Add("@MsnNumber", msnNumber);
                parameters.Add("@StageId", stageId);
                parameters.Add("@RowNumber", rowNumber);
                parameters.Add("@TotalRows", totalRows);

                for (var i = 0; i < fieldValues.Count; i++)
                {
                    parameters.Add($"@FieldId{i}", fieldValues[i].FieldId);
                    parameters.Add($"@FieldValue{i}", fieldValues[i].Value);
                }

                return await _db.ExecuteScalar<int>(sql, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error saving row {RowNumber} for DrawingNumber: {DrawingNumber}",
                    rowNumber, drawingNumber);
                throw;
            }
        }

        public async Task<InspectionMasterStatusDto?> GetInspectionMasterStatusAsync(string drawingNumber, string msnNumber)
        {
            _logger.LogInformation(
                "Getting inspection master status for DrawingNumber: {DrawingNumber}, MsnNumber: {MsnNumber}", drawingNumber, msnNumber);
            try
            {
                var result = await _db.QueryAsync<InspectionMasterStatusDto>(
                    TestingQueries.GET_INSPECTION_MASTER_STATUS,
                    new { DrawingNumber = drawingNumber, MsnNumber = msnNumber });
                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting inspection master status for DrawingNumber: {DrawingNumber}", drawingNumber);
                throw;
            }
        }

        public static string BuildSaveFixedFieldsSql(int fixedValueCount)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DECLARE @MasterId INT;");
            sb.AppendLine("DECLARE @TemplateId INT;");
            sb.AppendLine("DECLARE @DrawingId INT;");
            sb.AppendLine();
            sb.AppendLine("BEGIN TRY");
            sb.AppendLine("    BEGIN TRANSACTION;");
            sb.AppendLine();
            sb.AppendLine("    SELECT TOP 1 @DrawingId = id FROM tbl_drawingnumber WHERE drawingnumber = @DrawingNumber AND isactive = 1;");
            sb.AppendLine("    IF @DrawingId IS NULL THROW 50001, 'Drawing number not found.', 1;");
            sb.AppendLine();
            sb.AppendLine("    SELECT TOP 1 @TemplateId = dtm.template_id FROM tbl_drawingtemplatemapping dtm WHERE dtm.drawingnumberid = @DrawingId AND dtm.isactive = 1 ORDER BY dtm.id DESC;");
            sb.AppendLine("    IF @TemplateId IS NULL THROW 50002, 'No template mapped for this drawing number.', 1;");
            sb.AppendLine();
            sb.AppendLine("    -- Find or create the ONE master row for this (drawing, MSN) inspection instance");
            sb.AppendLine("    SELECT TOP 1 @MasterId = id FROM tbl_inspection_master WHERE drawing_number = @DrawingNumber AND msn_number = @MsnNumber AND isactive = 1 ORDER BY id DESC;");
            sb.AppendLine();
            sb.AppendLine("    IF @MasterId IS NULL");
            sb.AppendLine("    BEGIN");
            sb.AppendLine("        INSERT INTO tbl_inspection_master (template_id, drawing_id, drawing_number, msn_number, total_rows, stage1_completed, stage2_completed, stage3_completed, isactive, createddate)");
            sb.AppendLine("        VALUES (@TemplateId, @DrawingId, @DrawingNumber, @MsnNumber, 0, 0, 0, 0, 1, GETDATE());");
            sb.AppendLine("        SET @MasterId = CAST(SCOPE_IDENTITY() AS INT);");
            sb.AppendLine("    END");
            sb.AppendLine();
            sb.AppendLine("    -- Delete and re-insert fixed fields (row_number = 0 within the child table)");
            sb.AppendLine("    DELETE rv FROM tbl_inspection_row_values rv");
            sb.AppendLine("    INNER JOIN tbl_template_fields tf ON tf.id = rv.fieldId");
            sb.AppendLine("    WHERE rv.row_master_id = @MasterId AND tf.is_row_field = 0 AND rv.row_number = 0;");
            sb.AppendLine();

            for (var i = 0; i < fixedValueCount; i++)
            {
                sb.AppendLine("    INSERT INTO tbl_inspection_row_values (row_master_id, fieldId, field_value, row_number, isactive, createddate)");
                sb.AppendLine($"    VALUES (@MasterId, @FixedFieldId{i}, @FixedValue{i}, 0, 1, GETDATE());");
            }

            sb.AppendLine();
            sb.AppendLine("    COMMIT TRANSACTION;");
            sb.AppendLine("    SELECT @MasterId;");
            sb.AppendLine("END TRY");
            sb.AppendLine("BEGIN CATCH");
            sb.AppendLine("    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;");
            sb.AppendLine("    THROW;");
            sb.AppendLine("END CATCH;");
            return sb.ToString();
        }

        public static string BuildSaveRowDataSql(int fieldCount)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DECLARE @MasterId INT;");
            sb.AppendLine("DECLARE @TemplateId INT;");
            sb.AppendLine("DECLARE @DrawingId INT;");
            sb.AppendLine();
            sb.AppendLine("BEGIN TRY");
            sb.AppendLine("    BEGIN TRANSACTION;");
            sb.AppendLine();
            sb.AppendLine("    SELECT TOP 1 @DrawingId = id FROM tbl_drawingnumber WHERE drawingnumber = @DrawingNumber AND isactive = 1;");
            sb.AppendLine("    IF @DrawingId IS NULL THROW 50001, 'Drawing number not found.', 1;");
            sb.AppendLine();
            sb.AppendLine("    SELECT TOP 1 @TemplateId = dtm.template_id FROM tbl_drawingtemplatemapping dtm WHERE dtm.drawingnumberid = @DrawingId AND dtm.isactive = 1 ORDER BY dtm.id DESC;");
            sb.AppendLine("    IF @TemplateId IS NULL THROW 50002, 'No template mapped for this drawing number.', 1;");
            sb.AppendLine();
            sb.AppendLine("    -- Find or create the ONE master row for this (drawing, MSN) inspection instance");
            sb.AppendLine("    -- (shared across every row and every stage of the same MSN submission)");
            sb.AppendLine("    SELECT TOP 1 @MasterId = id FROM tbl_inspection_master WHERE drawing_number = @DrawingNumber AND msn_number = @MsnNumber AND isactive = 1 ORDER BY id DESC;");
            sb.AppendLine();
            sb.AppendLine("    IF @MasterId IS NULL");
            sb.AppendLine("    BEGIN");
            sb.AppendLine("        INSERT INTO tbl_inspection_master (template_id, drawing_id, drawing_number, msn_number, total_rows, stage1_completed, stage2_completed, stage3_completed, isactive, createddate)");
            sb.AppendLine("        VALUES (@TemplateId, @DrawingId, @DrawingNumber, @MsnNumber, @TotalRows, 0, 0, 0, 1, GETDATE());");
            sb.AppendLine("        SET @MasterId = CAST(SCOPE_IDENTITY() AS INT);");
            sb.AppendLine("    END");
            sb.AppendLine("    ELSE IF @TotalRows > 0");
            sb.AppendLine("    BEGIN");
            sb.AppendLine("        UPDATE tbl_inspection_master SET total_rows = @TotalRows WHERE id = @MasterId;");
            sb.AppendLine("    END");
            sb.AppendLine();
            sb.AppendLine("    -- Delete existing stage values for THIS row only (row_number = @RowNumber) —");
            sb.AppendLine("    -- other rows under the same master must be left untouched.");
            sb.AppendLine("    DELETE rv FROM tbl_inspection_row_values rv");
            sb.AppendLine("    INNER JOIN tbl_template_fields tf ON tf.id = rv.fieldId");
            sb.AppendLine("    LEFT JOIN tbl_template_formula_values tfv ON tfv.id = tf.formulaheaderid AND tfv.isactive = 1");
            sb.AppendLine("    WHERE rv.row_master_id = @MasterId");
            sb.AppendLine("    AND rv.row_number = @RowNumber");
            sb.AppendLine("    AND tf.is_row_field = 1");
            sb.AppendLine("    AND (tf.stageid <> 20 OR tf.stageid IS NULL OR @StageId = 3)");
            sb.AppendLine("    AND (tf.stageid = 10 OR (tf.formulaheaderid IS NOT NULL AND tfv.stageid = @StageId));");
            sb.AppendLine();

            for (var i = 0; i < fieldCount; i++)
            {
                sb.AppendLine("    INSERT INTO tbl_inspection_row_values (row_master_id, fieldId, field_value, row_number, isactive, createddate)");
                sb.AppendLine($"    VALUES (@MasterId, @FieldId{i}, @FieldValue{i}, @RowNumber, 1, GETDATE());");
            }

            sb.AppendLine();
            sb.AppendLine("    UPDATE tbl_inspection_master");
            sb.AppendLine("    SET stage1_completed = CASE WHEN @StageId = 1 THEN 1 ELSE stage1_completed END,");
            sb.AppendLine("        stage2_completed = CASE WHEN @StageId = 2 THEN 1 ELSE stage2_completed END,");
            sb.AppendLine("        stage3_completed = CASE WHEN @StageId = 3 THEN 1 ELSE stage3_completed END");
            sb.AppendLine("    WHERE id = @MasterId;");
            sb.AppendLine();
            sb.AppendLine("    COMMIT TRANSACTION;");
            sb.AppendLine("    SELECT @MasterId;");
            sb.AppendLine("END TRY");
            sb.AppendLine("BEGIN CATCH");
            sb.AppendLine("    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;");
            sb.AppendLine("    THROW;");
            sb.AppendLine("END CATCH;");
            return sb.ToString();
        }

        public static string BuildSaveStageDataSql(int rowValueCount, int fixedValueCount)
        {
            var sb = new StringBuilder();

            sb.AppendLine("DECLARE @MasterId INT;");
            sb.AppendLine("DECLARE @TemplateId INT;");
            sb.AppendLine("DECLARE @DrawingId INT;");
            sb.AppendLine();
            sb.AppendLine("BEGIN TRY");
            sb.AppendLine("    BEGIN TRANSACTION;");
            sb.AppendLine();

            sb.AppendLine("    SELECT TOP 1 @DrawingId = id");
            sb.AppendLine("    FROM tbl_drawingnumber");
            sb.AppendLine("    WHERE drawingnumber = @DrawingNumber AND isactive = 1;");
            sb.AppendLine();
            sb.AppendLine("    IF @DrawingId IS NULL");
            sb.AppendLine("        THROW 50001, 'Drawing number not found.', 1;");
            sb.AppendLine();

            sb.AppendLine("    SELECT TOP 1 @TemplateId = dtm.template_id");
            sb.AppendLine("    FROM tbl_drawingtemplatemapping dtm");
            sb.AppendLine("    WHERE dtm.drawingnumberid = @DrawingId AND dtm.isactive = 1");
            sb.AppendLine("    ORDER BY dtm.id DESC;");
            sb.AppendLine();
            sb.AppendLine("    IF @TemplateId IS NULL");
            sb.AppendLine("        THROW 50002, 'No template mapped for this drawing number.', 1;");
            sb.AppendLine();

            sb.AppendLine("    SELECT TOP 1 @MasterId = id");
            sb.AppendLine("    FROM tbl_inspection_master");
            sb.AppendLine("    WHERE drawing_number = @DrawingNumber AND isactive = 1");
            sb.AppendLine("    ORDER BY id DESC;");
            sb.AppendLine();

            sb.AppendLine("    IF @MasterId IS NULL");
            sb.AppendLine("    BEGIN");
            sb.AppendLine("        INSERT INTO tbl_inspection_master");
            sb.AppendLine("            (template_id, drawing_id, drawing_number, total_rows,");
            sb.AppendLine("             stage1_completed, stage2_completed, stage3_completed,");
            sb.AppendLine("             isactive, createddate)");
            sb.AppendLine("        VALUES");
            sb.AppendLine("            (@TemplateId, @DrawingId, @DrawingNumber, @TotalRows,");
            sb.AppendLine("             0, 0, 0, 1, GETDATE());");
            sb.AppendLine("        SET @MasterId = CAST(SCOPE_IDENTITY() AS INT);");
            sb.AppendLine("    END");
            sb.AppendLine("    ELSE");
            sb.AppendLine("    BEGIN");
            sb.AppendLine("        UPDATE tbl_inspection_master");
            sb.AppendLine("        SET total_rows = @TotalRows");
            sb.AppendLine("        WHERE id = @MasterId;");
            sb.AppendLine("    END");
            sb.AppendLine();

            // Delete existing stage-specific row values (mirrors GET_STAGE_FIELDS_BY_TEMPLATE_STAGE logic)
            sb.AppendLine("    DELETE rv");
            sb.AppendLine("    FROM tbl_inspection_row_values rv");
            sb.AppendLine("    INNER JOIN tbl_template_fields tf ON tf.id = rv.fieldId");
            sb.AppendLine("    LEFT JOIN tbl_template_formula_values tfv");
            sb.AppendLine("        ON tfv.id = tf.formulaheaderid AND tfv.isactive = 1");
            sb.AppendLine("    WHERE rv.row_master_id = @MasterId");
            sb.AppendLine("    AND tf.is_row_field = 1");
            sb.AppendLine("    AND (tf.stageid <> 20 OR tf.stageid IS NULL OR @StageId = 3)");
            sb.AppendLine("    AND (");
            sb.AppendLine("        tf.stageid = 10");
            sb.AppendLine("        OR (tf.formulaheaderid IS NOT NULL AND tfv.stageid = @StageId)");
            sb.AppendLine("    );");
            sb.AppendLine();

            // Insert row values
            for (var i = 0; i < rowValueCount; i++)
            {
                sb.AppendLine("    INSERT INTO tbl_inspection_row_values");
                sb.AppendLine("        (row_master_id, fieldId, field_value, row_number, isactive, createddate)");
                sb.AppendLine("    VALUES");
                sb.AppendLine($"        (@MasterId, @FieldId{i}, @FieldValue{i}, @RowNumber{i}, 1, GETDATE());");
                sb.AppendLine();
            }

            // Delete and re-insert fixed fields if provided
            if (fixedValueCount > 0)
            {
                sb.AppendLine("    DELETE rv");
                sb.AppendLine("    FROM tbl_inspection_row_values rv");
                sb.AppendLine("    INNER JOIN tbl_template_fields tf ON tf.id = rv.fieldId");
                sb.AppendLine("    WHERE rv.row_master_id = @MasterId");
                sb.AppendLine("    AND tf.is_row_field = 0;");
                sb.AppendLine();

                for (var i = 0; i < fixedValueCount; i++)
                {
                    sb.AppendLine("    INSERT INTO tbl_inspection_row_values");
                    sb.AppendLine("        (row_master_id, fieldId, field_value, row_number, isactive, createddate)");
                    sb.AppendLine("    VALUES");
                    sb.AppendLine($"        (@MasterId, @FixedFieldId{i}, @FixedValue{i}, 0, 1, GETDATE());");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("    UPDATE tbl_inspection_master");
            sb.AppendLine("    SET stage1_completed = CASE WHEN @StageId = 1 THEN 1 ELSE stage1_completed END,");
            sb.AppendLine("        stage2_completed = CASE WHEN @StageId = 2 THEN 1 ELSE stage2_completed END,");
            sb.AppendLine("        stage3_completed = CASE WHEN @StageId = 3 THEN 1 ELSE stage3_completed END");
            sb.AppendLine("    WHERE id = @MasterId;");
            sb.AppendLine();

            sb.AppendLine("    COMMIT TRANSACTION;");
            sb.AppendLine("    SELECT @MasterId;");
            sb.AppendLine("END TRY");
            sb.AppendLine("BEGIN CATCH");
            sb.AppendLine("    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;");
            sb.AppendLine("    THROW;");
            sb.AppendLine("END CATCH;");

            return sb.ToString();
        }

        public async Task<List<PrecheckCompletedComponentDto>> GetPrecheckCompletedComponentsAsync()
        {
            _logger.LogInformation("Getting all precheck completed components.");
            try
            {
                var result = await _db.QueryAsync<PrecheckCompletedComponentDto>(
                    TestingQueries.GET_PRECHECK_COMPLETED_COMPONENTS,
                    new { });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting precheck completed components.");
                throw;
            }
        }

        public async Task<List<HeaderFieldValueDto>> GetStage10FieldsWithValuesAsync(int templateId, string drawingNumber, string msnNumber)
        {
            _logger.LogInformation(
                "Getting stage-10 fields for TemplateId: {TemplateId}, DrawingNumber: {DrawingNumber}, MsnNumber: {MsnNumber}",
                templateId, drawingNumber, msnNumber);
            try
            {
                var result = await _db.QueryAsync<HeaderFieldValueDto>(
                    TestingQueries.GET_STAGE10_FIELDS_WITH_VALUES,
                    new { TemplateId = templateId, DrawingNumber = drawingNumber, MsnNumber = msnNumber });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting stage-10 fields for TemplateId: {TemplateId}", templateId);
                throw;
            }
        }

        public async Task<List<TemplateFieldDto>> GetFieldNamesForExportAsync(string drawingNumber)
        {
            _logger.LogInformation("Getting field names for export for DrawingNumber: {DrawingNumber}", drawingNumber);
            try
            {
                var result = await _db.QueryAsync<TemplateFieldDto>(
                    TestingQueries.GET_FIELD_NAMES_FOR_EXPORT,
                    new { DrawingNumber = drawingNumber });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting field names for DrawingNumber: {DrawingNumber}", drawingNumber);
                throw;
            }
        }

        public async Task<List<TemplateImageDto>> GetTemplateImagesAsync(int templateId)
        {
            _logger.LogInformation("Getting template images for TemplateId: {TemplateId}", templateId);
            try
            {
                var result = await _db.QueryAsync<TemplateImageDto>(
                    TestingQueries.GET_TEMPLATE_IMAGES,
                    new { TemplateId = templateId });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting template images for TemplateId: {TemplateId}", templateId);
                throw;
            }
        }
    }
}
