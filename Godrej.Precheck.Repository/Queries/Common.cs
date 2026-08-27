using System;
using Godrej.Precheck.Models.DataModel;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Globalization;

namespace Godrej.Precheck.Repository.Queries
{
    public static class Common
    {
        #region Common

        public static readonly string GET_PRECHECK_MODULE_QUERY = @"
            SELECT id, 
                   modulename, 
                   moduledescription
            FROM tbl_precheckmodules ";

        public static readonly string GET_SECURITY_QUESTION_MODULE_QUERY = @"
            SELECT id, 
                   securityquestion, 
                   createdby, createddate, modifiedby, modifieddate, isactive
            FROM tbl_securityquestion 
            WHERE isactive = 1";


        public static readonly string GET_COMPONENT_TYPE_QUERY = @"
            SELECT  id
          ,componenttype
          ,createdby
          ,createddate
          ,modifiedby
          ,modifieddate
          ,isactive
             FROM tbl_componenttype
            WHERE isactive = 1";

        public static readonly string GET_COMPONENT_TYPE_BY_NAME_QUERY = @"
            SELECT  id
          ,componenttype
          ,createdby
          ,createddate
          ,modifiedby
          ,modifieddate
          ,isactive
          FROM tbl_componenttype
          WHERE isactive = 1 and componenttype = @query ";

        public static readonly string GET_COMPONENT_TYPE_BY_Id_QUERY = @"
            SELECT  id
          ,componenttype
          ,createdby
          ,createddate
          ,modifiedby
          ,modifieddate
          ,isactive
          FROM tbl_componenttype
          WHERE isactive = 1 and id = @Id ";

        public static readonly string GET_Production_Series_Query = @"
            SELECT id, 
                   productionseries,
                   rccolour, 
                   createdby, 
                   createddate, 
                   modifiedby, 
                   modifieddate, 
                   isactive
            FROM tbl_productionseries 
            WHERE isactive = 1";


        public static readonly string ADD_NEW_ROLE_PERMISSIONS = @"
    INSERT INTO tbl_page_role_access (roleid, fullaccess, noaccess, pageid, createdby, createddate)
    SELECT @RoleId, fullaccess, noaccess, pageid, @CreatedBy, GETDATE()
    FROM tbl_page_role_access 
    WHERE roleid = (SELECT TOP 1 id FROM tbl_userroles WHERE LOWER(role) = 'planner' AND isactive = 1 ORDER BY id DESC);
    SELECT SCOPE_IDENTITY();";


        //ProdSeries by Name
        public static readonly string GET_ProductionSeriesByName_Query = @"
            SELECT id, 
                   productionseries,
                   rccolour, 
                   createdby, 
                   createddate, 
                   modifiedby, 
                   modifieddate, 
                   isactive
            FROM tbl_productionseries 
            WHERE productionseries=@query and isactive = 1";

        //ProdSeries by Id
        public static readonly string GET_ProductionSeriesById_Query = @"
            SELECT id, 
                   productionseries,
                   rccolour, 
                   createdby, 
                   createddate, 
                   modifiedby, 
                   modifieddate, 
                   isactive
            FROM tbl_productionseries 
            WHERE id=@Id and isactive = 1";

        public static readonly string GET_IRNUMBER_Query = @"
                SELECT 
                ir.id,
                ir.irnumber,
                td.drawingnumber as drawingnumberidName  ,
                ps.productionseries as productionseriesName ,          
                nc.nomenclature,           
                ct.componenttype ,          
                ir.idnumberstart, 
                ir.idnumberend,
                ir.idnumber as idnumberrange,
                ir.quantity,                  
                ir.remark,
                ir.createdby,
                ir.createddate,
                ir.modifiedby,
                ir.modifieddate,
                ir.isactive,
                ir.productionordernumber as ProductionOrderNumber,
                ir.stage,
                ir.stageid,
                ir.projectnumber,
                ir.supplier,
                ir.itemdescription,
                ir.lnitemcode,
                ir.operationnumber,
                ir.purchaseordernumber as PurchaseOrderNumber,
                u.id AS createdby,
                u.username AS generatedby
            FROM 
                tbl_irnumber ir
            INNER JOIN 
                tbl_productionseries ps ON ir.prodseriesid = ps.id
            INNER JOIN 
                tbl_drawingnumber td  ON ir.drawingnumberid = td.id
            LEFT JOIN 
                tbl_nomenclature nc ON ir.nomenclatureid = nc.id
            LEFT JOIN 
                tbl_componenttype ct ON ir.componenttypeid = ct.id
            LEFT JOIN
                tbl_stage s ON ir.stageid = s.id
            INNER JOIN 
                tbl_users u ON ir.createdby = u.id 
            WHERE ir.isactive = 1 and ir.departmentid = @departmentid
        AND ir.irnumber LIKE '%' + @query + '%'";

        public static readonly string GET_SINGLE_IRNUMBER_Query = @"SELECT ir.id, ir.irnumber, ir.prodseriesid, ir.drawingnumberid, ir.nomenclatureid, ir.componenttypeid, ir.idnumberstart, ir.idnumberend, ir.quantity, ir.remark, ir.productionordernumber as ProductionOrderNumber, ir.purchaseordernumber as PurchaseOrderNumber, ir.itemdescription, ir.lnitemcode, ir.operationnumber, ir.stage, ir.stageid, ir.projectnumber, ir.supplier, ir.createdby, ir.createddate, ir.modifiedby, ir.modifieddate, ir.isactive
        FROM tbl_irnumber ir Where ir.irnumber=@query";

        public static readonly string GET_IRNUMBERByDrawing_Query = @"
           SELECT DISTINCT
    ir.id,
    ir.irnumber,
    td.drawingnumber AS drawingnumberidName,
    ps.productionseries AS productionseriesName,
    nc.nomenclature,
    ct.componenttype,
    ir.idnumberstart, 
    ir.idnumberend,
    ir.idnumber as idnumberrange,
    ir.quantity,
    ir.remark,
    ir.createdby,
    ir.createddate,
    ir.modifiedby,
    ir.modifieddate,
    ir.isactive,
    ir.productionordernumber as ProductionOrderNumber,
    ir.stage,
    ir.stageid,
    ir.projectnumber,
    ir.supplier,
    ir.operationnumber,
    ir.itemdescription,
    COALESCE(ir.lnitemcode, map.lnitemcode) as lnitemcode,
    ir.purchaseordernumber as PurchaseOrderNumber,
    ir.buildnumber,
    tu.username,
    d.name AS departmentname
FROM
    tbl_irnumber ir
LEFT JOIN 
    tbl_productionseries ps ON ir.prodseriesid = ps.id
LEFT JOIN 
    tbl_drawingnumber td ON ir.drawingnumberid = td.id
LEFT JOIN 
    tbl_nomenclature nc ON ir.nomenclatureid = nc.id
LEFT JOIN 
    tbl_componenttype ct ON ir.componenttypeid = ct.id
LEFT JOIN
    tbl_stage s ON ir.stageid = s.id
LEFT JOIN
    tbl_Users tu ON ir.createdby = tu.id
LEFT JOIN
    tbl_department d ON ir.departmentid = d.id
LEFT JOIN 
    tbl_drawing_lnitem_map map ON td.drawingnumber = map.drawingnumber
WHERE 
    ir.isactive = 1     
AND (@query IS NULL OR td.drawingnumber LIKE '%' + @query + '%')
AND (@productionseries IS NULL OR ps.productionseries = @productionseries)
AND (@DepartmentID IS NULL OR ir.departmentid = @DepartmentID)
AND (@Stage IS NULL OR ir.stage = @Stage OR s.stage = @Stage)
AND (@LnItemCode IS NULL OR ir.lnitemcode LIKE '%' + @LnItemCode + '%')
AND (@FromDate IS NULL OR CAST(ir.createddate AS DATE) >= CAST(@FromDate AS DATE))
AND (@ToDate IS NULL OR CAST(ir.createddate AS DATE) <= CAST(@ToDate AS DATE))
AND (@IRNumberId IS NULL OR ir.id = @IRNumberId)
ORDER BY
    ir.createddate DESC";


        public static readonly string GET_MSNNUMBERByDrawing_Query = @"
    SELECT DISTINCT
        msn.id,
        msn.msnnumber,
            td.drawingnumber as drawingnumberidName,
            ps.productionseries as productionseriesName ,                        
        nc.nomenclature,           
        ct.componenttype,          
        msn.idnumberstart, 
        msn.idnumberend,
        msn.idnumber as idnumberrange,
        msn.quantity,
        msn.remark,
        msn.createdby,
        msn.createddate,
        msn.modifiedby,
        msn.modifieddate,
        msn.isactive,
        msn.productionordernumber as ProductionOrderNumber,
        msn.stage,
        msn.stageid,
        msn.projectnumber,
        msn.operationnumber,
        msn.supplier,
        msn.itemdescription,
        msn.lnitemcode,
        msn.purchaseordernumber as PurchaseOrderNumber,
        msn.buildnumber,
        tu.username,
        d.name AS departmentname
    FROM
        tbl_msnnumber msn
    LEFT JOIN 
        tbl_productionseries ps ON msn.prodseriesid = ps.id
    LEFT JOIN 
        tbl_drawingnumber td  ON msn.drawingnumberid = td.id
    LEFT JOIN 
        tbl_nomenclature nc ON msn.nomenclatureid = nc.id
    LEFT JOIN 
        tbl_componenttype ct ON msn.componenttypeid = ct.id
    LEFT JOIN
        tbl_stage s ON msn.stageid = s.id
    LEFT JOIN
            tbl_Users tu ON msn.createdby =tu.id
    LEFT JOIN
        tbl_department d ON msn.departmentid = d.id
    WHERE
        msn.isactive = 1
        AND (@query IS NULL OR td.drawingnumber LIKE '%' + @query + '%')
        AND (@productionseries IS NULL OR ps.productionseries = @productionseries)
        AND (@DepartmentID IS NULL OR msn.departmentid = @DepartmentID)
        AND (@Stage IS NULL OR msn.stage = @Stage OR s.stage = @Stage)
        AND (@LnItemCode IS NULL OR msn.lnitemcode LIKE '%' + @LnItemCode + '%')
        AND (@FromDate IS NULL OR CAST(msn.createddate AS DATE) >= CAST(@FromDate AS DATE))
        AND (@ToDate IS NULL OR CAST(msn.createddate AS DATE) <= CAST(@ToDate AS DATE))
    AND (@MSNNumberId IS NULL OR msn.id = @MSNNumberId)
    ORDER BY 
        msn.createddate DESC;
";

        public static readonly string GET_SINGLE_MSNNUMBER_Query = @"SELECT msn.id, msn.msnnumber, msn.prodseriesid, msn.drawingnumberid, msn.nomenclatureid, msn.componenttypeid, msn.idnumberstart, msn.idnumberend, msn.quantity, msn.remark, msn.productionordernumber as ProductionOrderNumber, msn.purchaseordernumber as PurchaseOrderNumber, msn.itemdescription, msn.lnitemcode, msn.stage, msn.stageid, msn.projectnumber, msn.supplier, msn.createdby, msn.createddate, msn.modifiedby, msn.modifieddate, msn.isactive
        FROM tbl_msnnumber msn Where msn.msnnumber=@query";

        public static readonly string GET_MSNNUMBER_Query = @"
            SELECT 
            msn.id,
            msn.msnnumber,
            td.drawingnumber as drawingnumberidName ,
            ps.productionseries as productionseriesName ,                        
            nc.nomenclature,           
            ct.componenttype,          
            msn.idnumberstart, 
            msn.idnumberend,
            msn.idnumber as idnumberrange,
            msn.quantity,
            msn.remark,
            msn.createdby,
            msn.createddate,
            msn.modifiedby,
            msn.modifieddate,
            msn.isactive,
            msn.productionordernumber as ProductionOrderNumber,
            msn.stage,
            msn.stageid,
            msn.projectnumber,
            msn.supplier,
            msn.itemdescription,
            msn.lnitemcode,
            msn.purchaseordernumber as PurchaseOrderNumber,
            msn.createdby,
            u.username As generatedby
        FROM 
            tbl_msnnumber msn
        INNER JOIN 
            tbl_productionseries ps ON msn.prodseriesid = ps.id
        INNER JOIN 
        tbl_drawingnumber td  ON msn.drawingnumberid = td.id
        LEFT JOIN
            tbl_stage s ON msn.stageid = s.id
        LEFT JOIN 
            tbl_nomenclature nc ON msn.nomenclatureid = nc.id
        LEFT JOIN 
            tbl_componenttype ct ON msn.componenttypeid = ct.id 
        INNER JOIN 
            tbl_users u ON msn.createdby = u.id 

                  WHERE msn.isactive = 1 AND  msn.departmentid = @departmentid  And msn.msnnumber LIKE '%' + @query + '%'";


        public static readonly string GET_ALL_LNITEMCODE = @"
            -- Use ROW_NUMBER to eliminate duplicates while maintaining proper ordering
            SELECT TOP 100 lnitemcode
            FROM (
                SELECT 
                    lnitemcode,
                    priority,
                    sort_order,
                    ROW_NUMBER() OVER (PARTITION BY lnitemcode ORDER BY priority, sort_order) AS rn
                FROM (
                    SELECT 
                        lnitemcode,
                        priority,
                        CASE 
                            WHEN @search IS NULL OR @search = '' THEN lnitemcode
                            WHEN lnitemcode = @search THEN '0' + lnitemcode
                            WHEN lnitemcode LIKE @search + '%' THEN '1' + lnitemcode
                            ELSE '2' + lnitemcode
                        END AS sort_order
                    FROM (
                        -- First try mapping table
                        SELECT lnitemcode, 1 AS priority
                        FROM tbl_drawing_lnitem_map 
                        WHERE isactive = 1 
                          AND (@search IS NULL OR @search = '' OR lnitemcode LIKE @search + '%')
                        
                        UNION ALL
                        
                        -- Fallback to tbl_lnitemcode only if mapping table has no results
                        SELECT lnitemcode, 2 AS priority
                        FROM tbl_lnitemcode 
                        WHERE isactive = 1 
                          AND (@search IS NULL OR @search = '' OR lnitemcode LIKE @search + '%')
                          AND NOT EXISTS (SELECT 1 FROM tbl_drawing_lnitem_map WHERE isactive = 1)
                    ) AS combined
                ) AS with_sort
            ) AS deduplicated
            WHERE rn = 1
            ORDER BY priority, sort_order";

        public static readonly string GET_DrawingNumber_Query =
     @" SELECT 
        dn.id, 
        dn.drawingnumber, 
        dn.createdby, 
        dn.createddate, 
        dn.modifiedby, 
        dn.modifieddate, 
        dn.isactive,
        loc.racklocationid,
        sil.racklocation As location, 
        nom.nomenclature, 
        nom.id As nomenclatureid,
        ct.id As componenttypeid,
        ln_correct.id As lnitemcodeid,
        doc.id As documenttypeid,
        ta.id As assemblyid, 
        ta.assemblynumber,
        ct.componenttype,
        map.lnitemcode, 
        doc.documentType,
        tdpsmap.availableseriesid,
        dn.isexpiry,
        tps.productionseries as availableseries,
        NULL AS unitid,
        tassmap.unit AS unitname,
        tassmap.parentdrawingnumber AS parentdrawingnumberid,
        parentdw.drawingnumber AS parentdrawingnumber
    FROM 
        tbl_drawingnumber dn
    LEFT JOIN 
        tbl_drawing_lnitem_map map
        ON dn.drawingnumber = map.drawingnumber
    LEFT JOIN 
        tbl_lnitemcode ln_correct 
        ON map.lnitemcode = ln_correct.lnitemcode AND ln_correct.isactive = 1
    LEFT JOIN 
        tbl_drawingnlnitemlocationmapping loc 
        ON dn.id = loc.drawingnumberid AND loc.isactive = 1
    LEFT JOIN 
        tbl_storeitemlocation sil 
        ON loc.racklocationid = sil.id AND sil.isactive = 1
    LEFT JOIN 
        tbl_drawingnomenclaturemapping nommap 
        ON dn.id = nommap.drawingnumberid AND nommap.isactive = 1
    LEFT JOIN 
        tbl_nomenclature nom 
        ON nommap.nomenclatureid = nom.id AND nom.isactive = 1
    LEFT JOIN 
        tbl_drawingcomponenttypemapping ctmap 
        ON dn.id = ctmap.drawingnumberid AND ctmap.isactive = 1
    LEFT JOIN 
        tbl_componenttype ct 
        ON ctmap.componenttypeid = ct.id AND ct.isactive = 1
    LEFT JOIN 
        tbl_lnitemcode ln 
        ON loc.lnitemcodeid = ln.id AND ln.isactive = 1
    LEFT JOIN 
        tbl_drawingdocumenttypemapping docmap 
        ON dn.id = docmap.drawingnumberid AND docmap.isactive = 1
    LEFT JOIN 
        tbl_documenttype doc 
        ON docmap.documenttypeid = doc.id AND doc.isactive = 1
    LEFT JOIN 
        tbl_assemblydrawingmapping tassmap
        ON dn.id = tassmap.drawingnumber AND tassmap.isactive = 1
    LEFT JOIN 
        tbl_assemblynumber ta 
        ON tassmap.assemblynumber = ta.id AND ta.isactive = 1
    LEFT JOIN 
        tbl_drawingnumber parentdw
        ON tassmap.parentdrawingnumber = parentdw.id AND parentdw.isactive = 1
    LEFT JOIN 
        tbl_drawingprodseriesmapping tdpsmap
        ON tdpsmap.drawingnumberid = dn.id AND tdpsmap.isactive = 1
    LEFT JOIN 
        tbl_productionseries tps
        ON tdpsmap.availableseriesid = tps.id AND tps.isactive = 1
    WHERE 
        dn.isactive = 1";
        //   AND dn.drawingnumber LIKE '%' + @query + '%'";

        public static readonly string GET_DrawingNumberById_Query = @"
        SELECT id, drawingnumber, createdby, createddate, modifiedby, modifieddate, isactive, nomenclature
        FROM tbl_drawingnumber Where id=@Id";




        public static readonly string DELETE_DRAWING_NUMBER_QUERY = @"
            DECLARE @RecordId INT;

            SELECT @RecordId = id
            FROM tbl_drawingnumber
            WHERE drawingnumber = @DrawingNumber
              AND lnitemcode    = @LnItemCode;

            IF @RecordId IS NULL
                THROW 50001, 'Drawing number with the given lnitemcode not found.', 1;

            UPDATE tbl_drawingnumber
            SET isactive     = 0,
                modifiedby   = @ModifiedBy,
                modifieddate = GETDATE()
            WHERE id = @RecordId;

            SELECT @RecordId AS DeletedRecordId;";

        public static readonly string GET_DocumentType_QUERY = @"SELECT * FROM tbl_documenttype Where isactive=1";

        public static readonly string GET_DocumentTypeByName_QUERY = @"SELECT * FROM tbl_documenttype Where isactive=1 And documenttype LIKE '%'+@query+'%'";

        public static readonly string GET_Stage_ByType_QUERY = @"SELECT id, [stage] AS StageName, stagetype AS StageType, createdby AS CreatedBy, createddate AS CreatedDate, modifiedby AS ModifiedBy, modifieddate AS ModifiedDate, isactive AS IsActive FROM tbl_stage WHERE isactive=1 AND stagetype = @stageType ORDER BY [stage]";

        public static readonly string GET_All_Stage_QUERY = @"SELECT id, [stage] AS StageName, stagetype AS StageType, createdby AS CreatedBy, createddate AS CreatedDate, modifiedby AS ModifiedBy, modifieddate AS ModifiedDate, isactive AS IsActive FROM tbl_stage WHERE isactive=1 ORDER BY stagetype, [stage]";

        public static readonly string GET_Stage_ById_QUERY = @"SELECT id, [stage] AS StageName, stagetype AS StageType, createdby AS CreatedBy, createddate AS CreatedDate, modifiedby AS ModifiedBy, modifieddate AS ModifiedDate, isactive AS IsActive FROM tbl_stage WHERE id = @stageId AND isactive=1";

        public static readonly string GET_AssemblyBy_Id_Query = @"
        SELECT id, assemblynumber, createdby, createddate, modifiedby, modifieddate, isactive
        FROM tbl_assemblynumber where id=@assemblyId";

        public static readonly string GET_ALL_Assembly_Query = @"
        SELECT id, assemblynumber, createdby, createddate, modifiedby, modifieddate, isactive
        FROM tbl_assemblynumber where isactive=1 ORDER BY assemblynumber";

        public static readonly string GET_Nomenclature_QUERY = @"SELECT id, nomenclature, createdby, createddate, modifiedby, modifieddate, isactive FROM tbl_nomenclature where isactive=1 And nomenclature LIKE '%'+@query+'%'";

        public static readonly string GET_User_QUERY = @"
            SELECT
            u.id,
            u.email,
            u.username,
            u.userid,
            u.plantid,
            u.lastloginat,
            u.createdby,
            u.createddate,
            u.modifiedby,
            u.modifieddate,
            u.isactive,
            ur.role ,
            u.departmentid
        FROM
            tbl_users u
        LEFT JOIN
            tbl_userroles ur ON u.userroleid = ur.id
        WHERE
            u.isactive = 1 And u.id=@UserId";



        public static readonly string GET_UserByName = @"
            SELECT
            u.id,
            u.email,
            u.username,
            u.userid,
            u.plantid,
            u.lastloginat,
            u.createdby,
            u.createddate,
            u.modifiedby,
            u.modifieddate,
            u.isactive,
            ur.role 
        FROM
            tbl_users u
        LEFT JOIN
            tbl_userroles ur ON u.userroleid = ur.id
        WHERE
            u.isactive = 1 And u.username=@Name";



        public static readonly string GET_ProductionOrderByName_QUERY = @"
        SELECT id, productionordernumber, createdby, createddate, modifiedby, modifieddate, isactive
        FROM tbl_productionordernumber Where isactive =1 and productionordernumber = @ProductionOrder
        ";

        public static readonly string GET_UnitByName_QUERY = @"
        SELECT id, unitname, createdby, createddate, modifiedby, modifieddate, isactive
        FROM tbl_unit Where isactive=1
            ";

        public static readonly string GET_All_Shapes_QUERY = @"
        SELECT id, materialname AS MaterialName, createdby, createddate, modifiedby, modifieddate, isactive
        FROM tbl_shapes 
        WHERE isactive = 1
            ";


        public static readonly string GET_DEPARTMENT_BY_ID = @"SELECT * FROM tbl_department where id = @DepartmentId";


        public static readonly string GET_LAST_SEQUENCE_IRNUMBER = @"SELECT ISNULL(MAX(sequenceno), 0) AS LastSequenceNo
        FROM tbl_irnumber";

        public static readonly string GET_LAST_SEQUENCE_MSNNUMBER = @"SELECT ISNULL(MAX(sequenceno), 0) AS LastSequenceNo
        FROM tbl_msnnumber";


        public static readonly string GET_ALL_DEPARTMENT = @"SELECT * FROM tbl_department where isactive = 1";

        public static readonly string GET_ALL_USERROLES = @"SELECT * FROM tbl_userroles where isactive=1";

        public static readonly string GET_ALL_PLANTS = @"SELECT * FROM tbl_plant";

        public static readonly string INSERT_USER_ROLE_QUERY = @"
            INSERT INTO tbl_userroles (role, description, createdby, createddate, isactive)
            VALUES (@Role, @Description, @CreatedBy, GETDATE(), 1);
            SELECT CAST(SCOPE_IDENTITY() as int)";

        public static readonly string UPDATE_USER_ROLE_QUERY = @"
            UPDATE tbl_userroles
            SET role = @Role,
                description = @Description,
                modifiedby = @ModifiedBy,
                modifieddate = GETDATE(),
                isactive = @IsActive
            WHERE id = @Id";

        public static readonly string DELETE_USER_ROLE_QUERY = @"
            UPDATE tbl_userroles
            SET isactive = 0,
                modifiedby = @ModifiedBy,
                modifieddate = GETDATE()
            WHERE id = @Id";

        public static readonly string GET_ALL_USERS_QUERY = @"
            SELECT 
                u.id,
                u.email,
                u.username,
                u.userid,
                u.plantid,
                p.plantname,
                u.lastloginat,
                u.createdby,
                u.createddate,
                u.modifiedby,
                u.modifieddate,
                u.isactive,
                u.userroleid,
                ur.role,
                u.departmentid,
                d.name AS departmentname,
                u.securityquestionid
            FROM tbl_users u
            LEFT JOIN tbl_userroles ur ON u.userroleid = ur.id
            LEFT JOIN tbl_department d ON u.departmentid = d.id
            LEFT JOIN tbl_plant p ON u.plantid = p.id
            ORDER BY u.createddate DESC";

        public static readonly string UPDATE_USER_QUERY = @"
            UPDATE tbl_users
            SET 
                email = COALESCE(@Email, email),
                username = COALESCE(@UserName, username),
                departmentid = COALESCE(@DepartmentId, departmentid),
                userroleid = COALESCE(@UserRoleId, userroleid),
                plantid=@PlantId,
                securityquestionid = COALESCE(@SecurityQuestionId, securityquestionid),
                securityanswer = COALESCE(@SecurityAnswer, securityanswer),
                modifiedby = @ModifiedBy,
                modifieddate = @ModifiedDate
            WHERE id = @Id AND isactive = 1";

        public static readonly string UPDATE_USER_STATUS_QUERY = @"
            UPDATE tbl_users
            SET isactive = @IsActive,
                modifiedby = @ModifiedBy,
                modifieddate = GETDATE()
            WHERE id = @Id";
        #endregion

        #region IR_DISTINCT
        public static readonly string DISTINCT_VALUES_IRNUMBER_QUERY = @"
            SELECT DISTINCT irnumber ,id
            FROM tbl_irnumber
            WHERE isactive = 1
            ORDER BY id DESC";
        #endregion

        #region
        public static readonly string GET_ALL_MSN_NUMBER_QUERY = @"
    SELECT DISTINCT 
        id AS Id,
        msnnumber AS MSNNumber
    FROM tbl_msnnumber
    WHERE msnnumber IS NOT NULL
    ORDER BY id DESC";
        #endregion

        #region GET_ALL_PAGE_ROLE_ACCESS_QUERY 
        public static readonly string GET_ALL_PAGE_ROLE_ACCESS_QUERY = @"
    SELECT 
        p.id,
        p.pagename AS PageName,
        p.displayorder AS DisplayOrder,
        p.isactive AS IsActive,
        p.createdby AS CreatedBy,
        p.createddate AS CreatedDate,
        p.modifiedby AS ModifiedBy,
        p.modifieddate AS ModifiedDate,
        p.parentid AS ParentId,
        ISNULL(pra.fullaccess, 0) AS FullAccess,
        ISNULL(pra.noaccess, 0) AS NoAccess
    FROM tbl_page p
    LEFT JOIN tbl_page_role_access pra 
        ON pra.pageid = p.id 
        AND pra.roleid = @RoleId         
        AND pra.isactive = 1
    WHERE p.isactive = 1
    ORDER BY p.parentid, p.displayorder";
        #endregion

        #region UPDATE_PAGE_ROLE_ACCESS_QUERY
        public static readonly string UPDATE_PAGE_ROLE_ACCESS_QUERY = @"
        MERGE tbl_page_role_access AS target
        USING (SELECT @RoleId AS RoleId, @PageId AS PageId) AS src
            ON target.roleid = src.RoleId AND target.pageid = src.PageId
        WHEN MATCHED THEN
            UPDATE SET
                fullaccess = @FullAccess,
                noaccess = @NoAccess,
                modifiedby = @ModifiedBy,
                modifieddate = GETDATE(),
                isactive = 1
        WHEN NOT MATCHED THEN
            INSERT (roleid, pageid, fullaccess, noaccess, createdby, createddate, isactive)
            VALUES (@RoleId, @PageId, @FullAccess, @NoAccess, @ModifiedBy, GETDATE(), 1);";
        #endregion

        #region ADD_DEPARTMENT_QUERY
        public static readonly string ADD_DEPARTMENT_QUERY = @"
        INSERT INTO tbl_department (name, isactive, createddate, createdby)
        VALUES (@DepartmentName, 1, GETDATE(), @CreatedBy)";
        #endregion
        #region ADD_UNIT_QUERY
        public static readonly string ADD_UNIT_QUERY = @"
        INSERT INTO tbl_unit (unitname, isactive, createddate, createdby)
        VALUES (@UnitName, 1, GETDATE(), @CreatedBy)";
        #endregion

        #region ADD_SHAPE_QUERY
        public static readonly string ADD_SHAPE_QUERY = @"
        INSERT INTO tbl_shapes (materialname, isactive, createddate, createdby)
        VALUES (@ShapeName, 1, GETDATE(), @CreatedBy)";
        #endregion


        #region ADD_STAGE_QUERY
        public static readonly string ADD_STAGE_QUERY = @"
        INSERT INTO tbl_stage (stage,stagetype, isactive, createddate, createdby)
        VALUES (@StageName,@StageType, 1, GETDATE(), @CreatedBy)";
        #endregion

        #region UPDATE_UNIT_QUERY
        public static readonly string UPDATE_UNIT_QUERY = @"
UPDATE tbl_unit 
SET unitname = @UnitName, 
    modifieddate = GETDATE(), 
    modifiedby = @ModifiedBy
WHERE id = @Id AND isactive = 1";
        #endregion

        #region UPDATE_SHAPE_QUERY
        public static readonly string UPDATE_SHAPE_QUERY = @"
UPDATE tbl_shapes 
SET materialname = @ShapeName, 
    modifieddate = GETDATE(), 
    modifiedby = @ModifiedBy
WHERE id = @Id AND isactive = 1";
        #endregion

        #region UPDATE_STAGE_QUERY
        public static readonly string UPDATE_STAGE_QUERY = @"
UPDATE tbl_stage 
SET stage = @StageName, 
    stagetype = @StageType,
    modifieddate = GETDATE(), 
    modifiedby = @ModifiedBy
WHERE id = @Id AND isactive = 1";
        #endregion


        #region DELETE_UNIT_QUERY
        public static readonly string DELETE_UNIT_QUERY = @"
UPDATE tbl_unit 
SET isactive = 0, 
    modifieddate = GETDATE(), 
    modifiedby = @ModifiedBy
WHERE id = @Id AND isactive = 1";
        #endregion

        #region DELETE_SHAPE_QUERY
        public static readonly string DELETE_SHAPE_QUERY = @"
UPDATE tbl_shapes 
SET isactive = 0, 
    modifieddate = GETDATE(), 
    modifiedby = @ModifiedBy
WHERE id = @Id AND isactive = 1";
        #endregion

        #region DELETE_STAGE_QUERY
        public static readonly string DELETE_STAGE_QUERY = @"
UPDATE tbl_stage 
SET isactive = 0, 
    modifieddate = GETDATE(), 
    modifiedby = @ModifiedBy
WHERE id = @Id AND isactive = 1";
        #endregion

        #region UPDATE_DEPARTMENT_QUERY
        public static readonly string UPDATE_DEPARTMENT_QUERY = @"
UPDATE tbl_department 
SET name = @DepartmentName, 
    modifieddate = GETDATE(), 
    modifiedby = @ModifiedBy
WHERE id = @Id AND isactive = 1";
        #endregion

        #region DELETE_DEPARTMENT_QUERY
        public static readonly string DELETE_DEPARTMENT_QUERY = @"
UPDATE tbl_department 
SET isactive = 0, 
    modifieddate = GETDATE(), 
    modifiedby = @ModifiedBy
WHERE id = @Id AND isactive = 1";
        #endregion

        #region CHECK_USER_EXISTS_QUERY
        public static readonly string CHECK_USER_EXISTS_QUERY = @"
    SELECT COUNT(1) 
    FROM tbl_users 
    WHERE userid   = @UserId 
       OR username = @UserName";
        #endregion

        #region ADD_USER_QUERY
        public static readonly string ADD_USER_QUERY = @"
    INSERT INTO tbl_users 
        (userid, username, userroleid, passwordhash,securitystamp,departmentid, createdby, createddate)
    VALUES 
        (@UserId, @UserName, @RoleId, @Password,@SecurityStamp,@DepartmentId, @CreatedBy, @CreatedDate);
    SELECT SCOPE_IDENTITY();";   
        #endregion

        #region GET_USER_BY_ID_QUERY
public static readonly string GET_USER_BY_ID_QUERY = @"
    SELECT 
        id,
        userid,
        username,
        userroleid,
        createdby,
        createddate
    FROM tbl_users
    WHERE id = @Id";
        #endregion


        #region ADD_PROD_SERIES_QUERY
        public static readonly string ADD_PROD_SERIES_QUERY = @"
    INSERT INTO tbl_productionseries 
    (id, productionseries, createdby, createddate, isactive)
    VALUES 
    (
        (SELECT ISNULL(MAX(id), 0) + 1 FROM tbl_productionseries),
        @ProductionSeries, 
        @CreatedBy, 
        GETDATE(), 
        1
    )";
        #endregion
        #region UPDATE_PROD_SERIES_QUERY
        public static readonly string UPDATE_PROD_SERIES_QUERY = @"
    UPDATE tbl_productionseries SET
        productionseries = @ProductionSeries,
        modifiedby = @ModifiedBy,
        modifieddate = GETDATE()
    WHERE id = @Id
    AND isactive = 1";
        #endregion

        #region DELETE_PROD_SERIES_QUERY
        public static readonly string DELETE_PROD_SERIES_QUERY = @"
    UPDATE tbl_productionseries SET
        isactive = 0,
        modifiedby = @DeletedBy,
        modifieddate = GETDATE()
    WHERE id = @Id
    AND isactive = 1";
        #endregion

        public static readonly string REMOVE_CHILD_DRAWING_QUERY = @"
            DECLARE @ChildId    INT;
            DECLARE @AssemblyId INT;
            DECLARE @ExistingId INT;

            -- Resolve drawing numbers to IDs
            SELECT @ChildId    = id FROM tbl_drawingnumber WHERE drawingnumber = @ChildDrawingNumber    AND isactive = 1;
            SELECT @AssemblyId = id FROM tbl_drawingnumber WHERE drawingnumber = @AssemblyDrawingNumber AND isactive = 1;

            IF @ChildId IS NULL
                THROW 50001, 'Child drawing number not found or inactive.', 1;

            IF @AssemblyId IS NULL
                THROW 50002, 'Assembly drawing number not found or inactive.', 1;

            -- Find the existing active mapping
            SELECT TOP 1 @ExistingId = id
            FROM tbl_assemblydrawingmapping
            WHERE drawingnumber       = @ChildId
              AND parentdrawingnumber = @AssemblyId
              AND assembly_lnitemcode = @AssemblyLnItemCode
              AND child_lnitemcode    = @ChildLnItemCode
              AND isactive            = 1;

            IF @ExistingId IS NULL
                THROW 50003, 'No active mapping found for the given child and assembly.', 1;

            -- Soft delete
            UPDATE tbl_assemblydrawingmapping
            SET isactive     = 0,
                modifiedby   = @ModifiedBy,
                modifieddate = GETDATE()
            WHERE id = @ExistingId;

            SELECT @ExistingId AS RemovedRecordId;";

        public static readonly string REASSIGN_PARENT_DRAWING_QUERY = @"
            DECLARE @ExistingId INT;

            -- Find the existing active mapping by child/assembly ln item codes and find no
            -- (the same child part can appear more than once under the same parent at different find numbers)
            SELECT TOP 1 @ExistingId = id
            FROM tbl_assemblydrawingmapping
            WHERE child_lnitemcode    = @ChildLnItemCode
              AND assembly_lnitemcode = @ParentLnItemCode
              AND findno              = @FindNo
              AND isactive            = 1
            ORDER BY id;

            IF @ExistingId IS NULL
                THROW 50001, 'No active mapping found for the given drawing, parent drawing and find no.', 1;

            -- Only overwrite findno/quantity when a real value was supplied
            UPDATE tbl_assemblydrawingmapping
            SET findno       = CASE WHEN @FindNo IS NOT NULL AND @FindNo <> '' THEN @FindNo ELSE findno END,
                quantity     = CASE WHEN @Quantity IS NOT NULL AND @Quantity <> 0 THEN @Quantity ELSE quantity END,
                modifiedby   = @ModifiedBy,
                modifieddate = GETDATE()
            WHERE id = @ExistingId;

            SELECT @ExistingId AS UpdatedRecordId;";

        public static readonly string ADD_ASSEMBLY_DRAWING_MAPPING_QUERY = @"
            DECLARE @ChildId    INT;
            DECLARE @ParentId   INT;
            DECLARE @NewId      INT;

            -- Resolve drawing number strings to IDs
            SELECT @ChildId  = id FROM tbl_drawingnumber WHERE drawingnumber = @DrawingNumber       AND isactive = 1;
            SELECT @ParentId = id FROM tbl_drawingnumber WHERE drawingnumber = @ParentDrawingNumber AND isactive = 1;

            IF @ChildId IS NULL
                THROW 50001, 'Child drawing number not found or inactive.', 1;

            IF @ParentId IS NULL
                THROW 50002, 'Parent drawing number not found or inactive.', 1;

            INSERT INTO tbl_assemblydrawingmapping
                (drawingnumber, parentdrawingnumber, createdby, createddate, modifiedby, modifieddate,
                 isactive, quantity, assembly_lnitemcode, child_lnitemcode, findno, consumedprodseriesid,nomenclature,unit)
            VALUES
                (@ChildId, @ParentId, @CreatedBy, GETDATE(), NULL, NULL,
                 1, @Quantity, @AssemblyLnItemCode, @ChildLnItemCode, @FindNo, @ConsumedProdSeriesId, @Nomenclature,@Unit);

            SELECT @NewId = SCOPE_IDENTITY();

            SELECT @NewId AS NewRecordId;";

        public static readonly string GET_ASSEMBLY_DRAWING_MAPPING_QUERY = @"
    SELECT
        adm.assemblynumber          AS AssemblyNumber,
        dn.drawingnumber            AS DrawingNumber,
        dn.isactive                 AS DrawingNumberStatus,
        adm.createdby               AS CreatedBy,
        adm.createddate             AS CreatedDate,
        adm.modifiedby              AS ModifiedBy,
        adm.modifieddate            AS ModifiedDate,
        adm.isactive                AS IsActive,
        adm.quantity                AS Quantity,
        adm.unit                    AS Unit,
        pdn.drawingnumber           AS ParentDrawingNumber,
        adm.consumedprodseriesid    AS ConsumedProdSeriesId,
        adm.findno                  AS FindNo,
        adm.nomenclature            AS Nomenclature,
        adm.assembly_lnitemcode     AS AssemblyLnItemCode,
        adm.child_lnitemcode        AS ChildLnItemCode,
        ct.componenttype            AS ComponentType
    FROM tbl_assemblydrawingmapping adm
    INNER JOIN tbl_drawingnumber dn  ON dn.id  = adm.drawingnumber
    INNER JOIN tbl_drawingnumber pdn ON pdn.id = adm.parentdrawingnumber
    LEFT  JOIN tbl_drawingcomponenttypemapping dctm ON dctm.drawingnumberid = dn.id AND dctm.isactive = 1
    LEFT  JOIN tbl_componenttype ct ON ct.id = dctm.componenttypeid AND ct.isactive = 1
    WHERE adm.isactive = 1
      AND (@SearchQuery IS NULL OR @SearchQuery = ''
           OR adm.assembly_lnitemcode    LIKE '%' + @SearchQuery + '%')";
    }
}
