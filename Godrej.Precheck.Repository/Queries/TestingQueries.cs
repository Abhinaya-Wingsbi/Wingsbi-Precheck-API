using System.Text;

namespace Godrej.Precheck.Repository.Queries
{
    public static class TestingQueries
    {
        public static readonly string CHECK_TEMPLATE_EXISTS = @"
        DECLARE @Sql NVARCHAR(MAX);

        IF OBJECT_ID('tbl_templates') IS NULL
        BEGIN
            SELECT CAST(0 AS INT);
            RETURN;
        END

        SET @Sql = N'SELECT TOP 1 1
                     FROM tbl_templates
                     WHERE ' +
                     CASE
                        WHEN COL_LENGTH('tbl_templates', 'id') IS NOT NULL THEN N'id = @TemplateId'
                        WHEN COL_LENGTH('tbl_templates', 'template_id') IS NOT NULL THEN N'template_id = @TemplateId'
                        ELSE N'1 = 0'
                     END;

        IF COL_LENGTH('tbl_templates', 'isactive') IS NOT NULL
            SET @Sql += N' AND isactive = 1';

        EXEC sp_executesql
            @Sql,
            N'@TemplateId INT',
            @TemplateId = @TemplateId;";

        public static readonly string GET_TEMPLATE_FIELDS_BY_TEMPLATE_ID = @"
        DECLARE @Sql NVARCHAR(MAX);
        DECLARE @FieldNameColumn NVARCHAR(128);
        DECLARE @FieldLabelColumn NVARCHAR(128);
        DECLARE @FieldTypeColumn NVARCHAR(128);
        DECLARE @FieldIdColumn NVARCHAR(128);
        DECLARE @TemplateIdColumn NVARCHAR(128);

        IF OBJECT_ID('tbl_template_fields') IS NULL
        BEGIN
            SELECT TOP 0
                CAST(NULL AS INT) AS Id,
                CAST(NULL AS NVARCHAR(255)) AS FieldName,
                CAST(NULL AS NVARCHAR(255)) AS FieldLabel,
                CAST(NULL AS NVARCHAR(255)) AS FieldType;
            RETURN;
        END

        SET @FieldIdColumn =
            CASE
                WHEN COL_LENGTH('tbl_template_fields', 'id') IS NOT NULL THEN 'id'
                WHEN COL_LENGTH('tbl_template_fields', 'field_id') IS NOT NULL THEN 'field_id'
                ELSE NULL
            END;

        SET @FieldNameColumn =
            CASE
                WHEN COL_LENGTH('tbl_template_fields', 'fieldname') IS NOT NULL THEN 'fieldname'
                WHEN COL_LENGTH('tbl_template_fields', 'field_name') IS NOT NULL THEN 'field_name'
                WHEN COL_LENGTH('tbl_template_fields', 'name') IS NOT NULL THEN 'name'
                WHEN COL_LENGTH('tbl_template_fields', 'fieldkey') IS NOT NULL THEN 'fieldkey'
                WHEN COL_LENGTH('tbl_template_fields', 'field_key') IS NOT NULL THEN 'field_key'
                ELSE NULL
            END;

        SET @FieldLabelColumn =
            CASE
                WHEN COL_LENGTH('tbl_template_fields', 'fieldlabel') IS NOT NULL THEN 'fieldlabel'
                WHEN COL_LENGTH('tbl_template_fields', 'field_label') IS NOT NULL THEN 'field_label'
                WHEN COL_LENGTH('tbl_template_fields', 'label') IS NOT NULL THEN 'label'
                ELSE NULL
            END;

        SET @FieldTypeColumn =
            CASE
                WHEN COL_LENGTH('tbl_template_fields', 'fieldtype') IS NOT NULL THEN 'fieldtype'
                WHEN COL_LENGTH('tbl_template_fields', 'field_type') IS NOT NULL THEN 'field_type'
                WHEN COL_LENGTH('tbl_template_fields', 'type') IS NOT NULL THEN 'type'
                ELSE NULL
            END;

        SET @TemplateIdColumn =
            CASE
                WHEN COL_LENGTH('tbl_template_fields', 'template_id') IS NOT NULL THEN 'template_id'
                WHEN COL_LENGTH('tbl_template_fields', 'templateid') IS NOT NULL THEN 'templateid'
                ELSE NULL
            END;

        IF @FieldIdColumn IS NULL OR @FieldNameColumn IS NULL OR @TemplateIdColumn IS NULL
        BEGIN
            SELECT TOP 0
                CAST(NULL AS INT) AS Id,
                CAST(NULL AS NVARCHAR(255)) AS FieldName,
                CAST(NULL AS NVARCHAR(255)) AS FieldLabel,
                CAST(NULL AS NVARCHAR(255)) AS FieldType;
            RETURN;
        END

        SET @Sql = N'SELECT
                        CAST(' + QUOTENAME(@FieldIdColumn) + N' AS INT) AS Id,
                        CAST(' + QUOTENAME(@FieldNameColumn) + N' AS NVARCHAR(255)) AS FieldName,
                        ' + CASE WHEN @FieldLabelColumn IS NOT NULL
                                 THEN N'CAST(' + QUOTENAME(@FieldLabelColumn) + N' AS NVARCHAR(255))'
                                 ELSE N'CAST(NULL AS NVARCHAR(255))' END + N' AS FieldLabel,
                        ' + CASE WHEN @FieldTypeColumn IS NOT NULL
                                 THEN N'CAST(' + QUOTENAME(@FieldTypeColumn) + N' AS NVARCHAR(255))'
                                 ELSE N'CAST(NULL AS NVARCHAR(255))' END + N' AS FieldType
                    FROM tbl_template_fields
                    WHERE ' + QUOTENAME(@TemplateIdColumn) + N' = @TemplateId';

        IF COL_LENGTH('tbl_template_fields', 'isactive') IS NOT NULL
            SET @Sql += N' AND isactive = 1';

        IF COL_LENGTH('tbl_template_fields', 'displayorder') IS NOT NULL
            SET @Sql += N' ORDER BY displayorder';
        ELSE IF COL_LENGTH('tbl_template_fields', 'sequence') IS NOT NULL
            SET @Sql += N' ORDER BY sequence';
        ELSE IF COL_LENGTH('tbl_template_fields', 'id') IS NOT NULL
            SET @Sql += N' ORDER BY id';

        EXEC sp_executesql
            @Sql,
            N'@TemplateId INT',
            @TemplateId = @TemplateId;";

        public static readonly string GET_TEMPLATE_ID_BY_DRAWING_NUMBER = @"
    SELECT TOP 1 dtm.template_id
    FROM tbl_drawingtemplatemapping dtm
    INNER JOIN tbl_drawingnumber dn ON dn.id = dtm.drawingnumberid
    WHERE dn.drawingnumber = @DrawingNumber
    AND dtm.isactive = 1
    AND dn.isactive = 1
    ORDER BY dtm.id DESC";

        public static readonly string GET_DRAWING_ID = @"
    SELECT TOP 1 id 
    FROM tbl_drawingnumber 
    WHERE drawingnumber = @DrawingNumber 
    AND isactive = 1";

        public static readonly string GET_TEMPLATE_FIELDS_WITH_VALUES_BY_DRAWING_NUMBER = @"
    DECLARE @TemplateId INT;
    DECLARE @MasterId INT;
    DECLARE @DrawingId INT;

    -- =============================================
    -- STEP 1: GET DRAWING ID
    -- =============================================
    SELECT TOP 1 @DrawingId = id
    FROM tbl_drawingnumber
    WHERE drawingnumber = @DrawingNumber
    AND isactive = 1;

    IF @DrawingId IS NULL
    BEGIN
        SELECT TOP 0 
            CAST(NULL AS INT)          AS FieldId,
            CAST(NULL AS NVARCHAR(255)) AS FieldName,
            CAST(NULL AS NVARCHAR(255)) AS FieldLabel,
            CAST(NULL AS NVARCHAR(255)) AS FieldType,
            CAST(NULL AS NVARCHAR(MAX)) AS FieldValue;
        RETURN;
    END

    -- =============================================
    -- STEP 2: GET TEMPLATE ID FROM MAPPING
    -- =============================================
    SELECT TOP 1 @TemplateId = dtm.template_id
    FROM tbl_drawingtemplatemapping dtm
    INNER JOIN tbl_drawingnumber dn ON dn.id = dtm.drawingnumberid
    WHERE dn.drawingnumber = @DrawingNumber
    AND dtm.isactive = 1
    AND dn.isactive = 1
    ORDER BY dtm.id DESC;

    IF @TemplateId IS NULL
    BEGIN
        SELECT TOP 0 
            CAST(NULL AS INT)          AS FieldId,
            CAST(NULL AS NVARCHAR(255)) AS FieldName,
            CAST(NULL AS NVARCHAR(255)) AS FieldLabel,
            CAST(NULL AS NVARCHAR(255)) AS FieldType,
            CAST(NULL AS NVARCHAR(MAX)) AS FieldValue;
        RETURN;
    END

    -- =============================================
    -- STEP 3: GET THE MASTER FOR THIS (DRAWING, MSN) INSPECTION INSTANCE
    -- =============================================
    SELECT TOP 1 @MasterId = id
    FROM tbl_inspection_master
    WHERE template_id = @TemplateId
    AND drawing_id = @DrawingId
    AND msn_number = @MsnNumber
    AND isactive = 1
    ORDER BY id DESC;

    -- NOTE: @MasterId may be NULL here (no submission yet, or MsnNumber not
    -- yet chosen). The LEFT JOIN below handles that gracefully: all fields
    -- are returned with FieldValue = NULL when no inspection exists.

    -- =============================================
    -- STEP 4: GET FIELDS WITH VALUES AND FORMULA HEADER INFO
    -- =============================================
    SELECT
        tf.id                AS Id,
        tf.field_name        AS FieldName,
        tf.field_label       AS FieldLabel,
        tf.field_type        AS FieldType,
        tf.stageid           AS StageId,
        tf.display_order     AS DisplayOrder,
        tf.is_row_field      AS IsRowField,
        rv.field_value       AS FieldValue,
        tf.formulaheaderid   AS FormulaHeaderId,
        tfv.field_name       AS FormulaHeaderName,
        tfv.field_value      AS FormulaHeaderValue
    FROM tbl_template_fields tf
    LEFT JOIN tbl_inspection_row_values rv
        ON rv.fieldId = tf.id
        AND rv.row_master_id = @MasterId
        AND rv.row_number = 0
        AND rv.isactive = 1
    LEFT JOIN tbl_template_formula_values tfv
        ON tfv.id = tf.formulaheaderid
        AND tfv.isactive = 1
    WHERE tf.template_id = @TemplateId
    AND tf.isactive = 1
    AND (tf.stageid <> 20 OR tf.stageid IS NULL OR @StageId = 3)
    AND (
        @StageId IS NULL
        OR tf.is_row_field = 0
        OR (tf.formulaheaderid IS NOT NULL AND tfv.stageid = @StageId)
        OR tf.stageid = 10
    )
    ORDER BY tf.display_order;";



        public static readonly string GET_FIELDS_BY_TEMPLATE_ID = @"
        SELECT id, field_name, field_label, field_type, is_row_field, is_required, display_order
        FROM tbl_template_fields
        WHERE template_id = @TemplateId
        AND isactive = 1
        ORDER BY display_order";

        // @MsnNumber is optional: pass it to target one specific inspection instance (the
        // normal, correct case). Debug/dev tooling may omit it, which falls back to "latest
        // instance for this drawing" — fine for ad-hoc inspection, not for real save/export flows.
        public static readonly string GET_INSPECTION_FOR_EXPORT = @"
    SELECT TOP 1
        im.id               AS MasterId,
        im.drawing_number   AS DrawingNumber,
        im.template_id      AS TemplateId,
        t.template          AS HtmlTemplate
    FROM tbl_inspection_master im
    INNER JOIN tbl_templates t ON t.id = im.template_id
    WHERE im.drawing_number = @DrawingNumber
    AND (@MsnNumber IS NULL OR im.msn_number = @MsnNumber)
    AND im.isactive = 1
    ORDER BY im.id DESC";

        // Fetches all field values (fixed + per-row) for one (drawing, MSN) inspection instance.
        // RowNumber = 0 for fixed fields, 1/2/3... for per-row fields.
        // @MsnNumber optional — see note on GET_INSPECTION_FOR_EXPORT above.
        public static readonly string GET_FIELD_VALUES_FOR_EXPORT = @"
    SELECT
        tf.field_name       AS FieldName,
        rv.field_value      AS FieldValue,
        rv.row_number       AS RowNumber
    FROM tbl_inspection_master im
    INNER JOIN tbl_inspection_row_values rv ON rv.row_master_id = im.id AND rv.isactive = 1
    INNER JOIN tbl_template_fields tf ON tf.id = rv.fieldId
    WHERE im.drawing_number = @DrawingNumber
    AND (@MsnNumber IS NULL OR im.msn_number = @MsnNumber)
    AND im.isactive = 1
    ORDER BY rv.row_number, tf.display_order";

        public static readonly string GET_MASTER_ID_BY_DRAWING = @"
    SELECT TOP 1 id
    FROM tbl_inspection_master
    WHERE drawing_number = @DrawingNumber
    AND isactive = 1
    ORDER BY id DESC";

        // One row per (drawing, MSN) inspection instance and its stage-completion status.
        public static readonly string GET_DRAWING_STAGE_STATUS = @"
    SELECT
        im.id                   AS MasterId,
        im.drawing_number       AS DrawingNumber,
        im.msn_number           AS MsnNumber,
        im.total_rows           AS TotalRows,
        im.stage1_completed     AS Stage1Completed,
        im.stage2_completed     AS Stage2Completed,
        im.stage3_completed     AS Stage3Completed,
        CASE
            WHEN im.stage1_completed = 0 THEN 1
            WHEN im.stage2_completed = 0 THEN 2
            ELSE 3
        END                     AS CurrentStage,
        CASE
            WHEN im.stage1_completed = 1
             AND im.stage2_completed = 1
             AND im.stage3_completed = 1 THEN 'Completed'
            WHEN im.stage1_completed = 1
             AND im.stage2_completed = 1 THEN 'Stage 3'
            WHEN im.stage1_completed = 1 THEN 'Stage 2'
            ELSE 'Stage 1'
        END                     AS CurrentStageName
    FROM tbl_inspection_master im
    WHERE im.isactive = 1
    ORDER BY im.id DESC";

        public static readonly string GET_FIXED_FIELDS_BY_TEMPLATE = @"
    SELECT id AS Id, field_name AS FieldName
    FROM tbl_template_fields
    WHERE template_id = @TemplateId
    AND is_row_field = 0
    AND isactive = 1
    ORDER BY display_order";

        // Fixed field values for one (drawing, MSN) inspection instance (row_number=0 in the child table)
        public static readonly string GET_FIXED_FIELD_VALUES_BY_DRAWING = @"
    SELECT
        tf.field_name   AS FieldName,
        rv.field_value  AS FieldValue
    FROM tbl_inspection_master im
    INNER JOIN tbl_inspection_row_values rv ON rv.row_master_id = im.id AND rv.isactive = 1
    INNER JOIN tbl_template_fields tf ON tf.id = rv.fieldId
    WHERE im.drawing_number = @DrawingNumber
    AND im.msn_number = @MsnNumber
    AND im.isactive = 1
    AND rv.row_number = 0
    AND tf.is_row_field = 0
    ORDER BY tf.display_order";

        // Stage row values for one (drawing, MSN) inspection instance, across all its data rows
        public static readonly string GET_STAGE_ROW_VALUES_BY_DRAWING = @"
    SELECT
        rv.row_number   AS RowNumber,
        tf.field_name   AS FieldName,
        rv.field_value  AS FieldValue
    FROM tbl_inspection_master im
    INNER JOIN tbl_inspection_row_values rv ON rv.row_master_id = im.id AND rv.isactive = 1
    INNER JOIN tbl_template_fields tf ON tf.id = rv.fieldId
    LEFT JOIN tbl_template_formula_values tfv
        ON tfv.id = tf.formulaheaderid AND tfv.isactive = 1
    WHERE im.drawing_number = @DrawingNumber
    AND im.msn_number = @MsnNumber
    AND rv.row_number > 0
    AND im.isactive = 1
    AND tf.is_row_field = 1
    AND (tf.stageid <> 20 OR tf.stageid IS NULL OR @StageId = 3)
    AND (
        tf.stageid = 10
        OR (tf.formulaheaderid IS NOT NULL AND tfv.stageid = @StageId)
    )
    ORDER BY rv.row_number, tf.display_order";

        // Stage-10 (common) fields with saved values — reads row 1's values under this MSN's master
        public static readonly string GET_STAGE10_FIELDS_WITH_VALUES = @"
    DECLARE @RowMasterId INT;
    SELECT TOP 1 @RowMasterId = id
    FROM tbl_inspection_master
    WHERE drawing_number = @DrawingNumber
    AND msn_number = @MsnNumber
    AND isactive = 1;

    SELECT
        tf.id           AS Id,
        tf.field_name   AS FieldName,
        tf.field_label  AS FieldLabel,
        tf.field_type   AS FieldType,
        rv.field_value  AS Value
    FROM tbl_template_fields tf
    LEFT JOIN tbl_inspection_row_values rv
        ON rv.fieldId = tf.id
        AND rv.row_master_id = @RowMasterId
        AND rv.row_number = 1
        AND rv.isactive = 1
    WHERE tf.template_id = @TemplateId
    AND tf.stageid = 0
    AND tf.isactive = 1
    ORDER BY tf.display_order";

        public static readonly string GET_STAGE_FIELDS_BY_TEMPLATE_STAGE = @"
    SELECT tf.id AS Id, tf.field_name AS FieldName
    FROM tbl_template_fields tf
    LEFT JOIN tbl_template_formula_values tfv
        ON tfv.id = tf.formulaheaderid
        AND tfv.isactive = 1
    WHERE tf.template_id = @TemplateId
    AND tf.is_row_field = 1
    AND tf.isactive = 1
    AND (tf.stageid <> 20 OR tf.stageid IS NULL OR @StageId = 3)
    AND (
        tf.stageid = 10
        OR (tf.formulaheaderid IS NOT NULL AND tfv.stageid = @StageId)
    )
    ORDER BY tf.display_order";

        // One master row per (drawing, MSN) instance now, so this is a direct read — no aggregation needed.
        public static readonly string GET_INSPECTION_MASTER_STATUS = @"
    SELECT TOP 1
        id                  AS MasterId,
        total_rows          AS TotalRows,
        stage1_completed    AS Stage1Completed,
        stage2_completed    AS Stage2Completed,
        stage3_completed    AS Stage3Completed
    FROM tbl_inspection_master
    WHERE drawing_number = @DrawingNumber
    AND msn_number = @MsnNumber
    AND isactive = 1
    ORDER BY id DESC";

        public static readonly string GET_STAGE_ROW_VALUES = @"
    SELECT
        rv.row_number   AS RowNumber,
        tf.field_name   AS FieldName,
        rv.field_value  AS FieldValue
    FROM tbl_inspection_row_values rv
    INNER JOIN tbl_template_fields tf ON tf.id = rv.fieldId
    WHERE rv.row_master_id = @MasterId
    AND tf.stageid = @StageId
    AND rv.isactive = 1
    ORDER BY rv.row_number, tf.display_order";

        // Returns all active field names for the template mapped to a drawing number.
        // Use this to discover exact DB field_name values so template IDs can match.
        public static readonly string GET_FIELD_NAMES_FOR_EXPORT = @"
    SELECT
        tf.field_name    AS FieldName,
        tf.field_label   AS FieldLabel,
        tf.is_row_field  AS IsRowField,
        tf.display_order AS DisplayOrder
    FROM tbl_template_fields tf
    WHERE tf.template_id = (
        SELECT TOP 1 dtm.template_id
        FROM tbl_drawingtemplatemapping dtm
        INNER JOIN tbl_drawingnumber dn ON dn.id = dtm.drawingnumberid
        WHERE dn.drawingnumber = @DrawingNumber
        AND dtm.isactive = 1
        AND dn.isactive = 1
        ORDER BY dtm.id DESC
    )
    AND tf.isactive = 1
    ORDER BY tf.is_row_field, tf.display_order";

        public static readonly string GET_PRECHECK_COMPLETED_COMPONENTS = @"
    WITH PrecheckStatusCalc AS (
        SELECT
            pom.id AS productionordernumberid,
            CASE
                WHEN pom.min IS NULL OR LTRIM(RTRIM(pom.min)) = '' THEN 4
                WHEN COUNT(ppd.id) = SUM(CASE WHEN ppd.isprecheckcomplete = 1 THEN 1 ELSE 0 END)
                     AND COUNT(ppd.id) > 0 THEN 3
                WHEN SUM(CASE WHEN ppd.isprecheckcomplete = 1 THEN 1 ELSE 0 END) > 0 THEN 2
                ELSE 1
            END AS CalculatedStatus,
            MAX(ppd.modifieddate) AS LastModifiedDate
        FROM tbl_productionordermaster pom
        LEFT JOIN tbl_projectdetails pd
            ON pom.id = pd.productionordernumberid AND pd.isactive = 1
        LEFT JOIN tbl_projectprecheckdetails ppd
            ON pd.id = ppd.projectdetailsid AND ppd.isactive = 1
        WHERE pom.isactive = 1
        GROUP BY pom.id, pom.min
    )
    SELECT
        pom.id                      AS Id,
        pom.productionordernumber   AS ProductionOrderNumber,
        pom.projectnumber           AS ProjectNumber,
        pom.projectdescription      AS ProjectDescription,
        pom.lnitemcode              AS LnItemCode,
        pom.itemdescription         AS ItemDescription,
        dn.drawingnumber            AS DrawingNumber,
        pom.min                     AS Min,
        pom.buildnumber             AS BuildNumber,
        pom.quantity                AS Quantity,
        pom.mrirnumber              AS MrirNumber,
        3                           AS PrecheckStatus,
        'Completed'                 AS PrecheckStatusName,
        psc.LastModifiedDate        AS LastModifiedDate,
        msn.msnnumber               AS MsnNumber,
        msn.quantity                AS MsnQuantity
    FROM tbl_productionordermaster pom
    INNER JOIN PrecheckStatusCalc psc ON pom.id = psc.productionordernumberid
    LEFT JOIN tbl_drawingnumber dn ON pom.drawingnumberid = dn.id AND dn.isactive = 1
    LEFT JOIN tbl_msnnumber msn
        ON CAST(msn.productionordernumber AS NVARCHAR(100)) = CAST(pom.productionordernumber AS NVARCHAR(100))
        AND msn.isactive = 1
    WHERE pom.isactive = 1
      AND psc.CalculatedStatus = 3
    ORDER BY psc.LastModifiedDate DESC";

        // Fetch images for a template by template_id.
        // tbl_template_images columns: id, template_id, placeholder_key, image_data (base64 or data URI), mime_type, isactive
        public static readonly string GET_TEMPLATE_IMAGES = @"
    SELECT
        placeholder_key AS PlaceholderKey,
        image_data      AS ImageData,
        ISNULL(mime_type, 'image/png') AS MimeType
    FROM tbl_template_images
    WHERE template_id = @TemplateId
      AND isactive = 1";
    }
}
