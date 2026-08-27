

using Godrej.Precheck.Models.DataModel;
using Microsoft.EntityFrameworkCore.Update.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Godrej.Precheck.Repository.Queries
{
    public static class PrecheckQueries
    {
        #region Get Precheck Template Queries

        // not useed version
        // Get user by email and password
        public static readonly string GET_PRECHECK_TEMPLATE_BY_ASSEMBLY = @"
                    SELECT 
                    asdmapping.[id]
                    ,asdmapping.[parentdrawingnumber] AS assemblynumber
                    ,parent_dw.[id] AS AssemblyNumberId
                    ,asdmapping.[drawingnumber]
                    ,dw.[id] AS DrawingNumberId
                    ,parent_dw.drawingnumber AS assemblynumber
                    ,dw.drawingnumber
                    ,nomen.nomenclature
                    ,nomen.[id] AS NomenclatureId
                    ,asdmapping.quantity
                    ,asdmapping.unit AS unitid
	                ,un.unitname as unit
                FROM tbl_assemblydrawingmapping asdmapping 
                INNER JOIN tbl_drawingnumber dw 
                    ON asdmapping.drawingnumber = dw.id 
                INNER JOIN tbl_drawingnumber parent_dw 
                    ON asdmapping.parentdrawingnumber = parent_dw.id
                INNER JOIN tbl_drawingnomenclaturemapping nomendw 
                    ON nomendw.drawingnumberid = dw.id
                INNER JOIN tbl_nomenclature nomen 
                    ON nomen.id = nomendw.nomenclatureid
                LEFT JOIN tbl_unit un 
                     ON un.id = asdmapping.unit
                WHERE parent_dw.drawingnumber = @assemblyNumber";

        #endregion

        #region Get Precheck Template Queries

        // this is used example Get user by email and password
        public static readonly string GET_PRECHECK_TEMPLATE_BY_ASSEMBLY_ID = @"
                        SELECT
                asdmapping.[id]
                ,asdmapping.[drawingnumber] as AssemblyNumber
                ,dw.[id] AS DrawingNumberId
                ,dw.drawingnumber
                ,dw.lnitemcode AS LnItemCode
                ,nomen.nomenclature
                ,nomen.[id] AS NomenclatureId
	            ,asdmapping.quantity
	            , ct.id As componenttypeid
	            ,ct.componenttype
                ,un.id AS UnitId
                ,asdmapping.unit AS Unit
            FROM tbl_drawingnumber dw
            INNER JOIN tbl_assemblydrawingmapping asdmapping
                ON asdmapping.drawingnumber = dw.id
                AND asdmapping.isactive=1
            LEFT JOIN tbl_drawingnomenclaturemapping nomendw
                ON nomendw.drawingnumberid = dw.id
                AND nomendw.isactive = 1
            LEFT JOIN tbl_nomenclature nomen
                ON nomen.id = nomendw.nomenclatureid
            LEFT JOIN tbl_unit un
                ON un.unitname = asdmapping.unit AND un.isactive = 1
            LEFT JOIN
                  tbl_drawingcomponenttypemapping ctmap
                  ON dw.id = ctmap.drawingnumberid
                  AND ctmap.isactive = 1
              LEFT JOIN
                  tbl_componenttype ct
                  ON ctmap.componenttypeid = ct.id
WHERE asdmapping.parentdrawingnumber   = @assemblyNumber and asdmapping.isactive=1";


        public static readonly string GET_AVAILABLE_QRCODE_BY_DRAWINGID =
            @"Select Count(drawingnumberid ) AS AvailableQuantity from tbl_qrcodedetails
              where  drawingnumberid=@drawingnumberid and qrcodestatusid = 1
            ";



        #endregion

        #region Update ID component consumption 


        public static readonly string GET_ID_COMPONENT_CONSUMPTION = @"
        SELECT  
            [id] ,
            [idnumber] ,
             [irnumber] ,
            [msnnumber] ,
            [consumedindrawing] ,
            [consumedindrawingid] ,
            [consumedinseriesid] ,
            [consumedinId],
            [remarks] ,
            [quantity] ,
            [unit]
        FROM [tbl_componentdrawingconsumption]
        WHERE [qrcodenumber] = @QrCodeNumber 
            AND isactive=1";
        // Get user by email and password
        public static readonly string UPDATE_ID_COMPONENT_CONSUMPTION = @"
       UPDATE tbl_componentdrawingconsumption
        SET 
            consumedindrawing = @consumedindrawing,
            consumedindrawingid = @consumedindrawingid,
            consumedinseriesid = @consumedinseriesid,
            consumedinproductionordernumber = @consumedinproductionordernumber,
            consumedinId = @consumedinId,
            remarks = @remarks,
            quantity = ISNULL(quantity, 0) + @quantity,
            unit = @unit,
            isactive = 1
        OUTPUT INSERTED.Id
        WHERE Id = (
            SELECT TOP 1 Id 
            FROM tbl_componentdrawingconsumption
            WHERE qrcodenumber = @QrCodeNumber AND isactive = 1
            ORDER BY Id
        )";


        #endregion

        #region Update Batch component consumption 

        // Get user by email and password
        public static readonly string UPDATE_BATCH_COMPONENT_CONSUMPTION = @"
       UPDATE tbl_componentdrawingconsumption
        SET 
            consumedindrawing = @consumedindrawing,
            consumedindrawingid = @consumedindrawingid,
            consumedinseriesid = @consumedinseriesid,
            consumedinId = @consumedinId,
            consumedinproductionordernumber = @consumedinproductionordernumber,
            remarks = @remarks,
            quantity = ISNULL(quantity, 0) + @quantity,
            unit = @unit
        OUTPUT INSERTED.Id
        WHERE Id = (
            SELECT TOP 1 Id 
            FROM tbl_componentdrawingconsumption
            WHERE qrcodenumber = @qrcodenumber AND isactive = 1
            ORDER BY qrcodenumber
        )";


        #endregion

        #region UpdatePreCheckDetails

        public static readonly string UPDATE_PROJECT_PRECHECK_DETAIL = @"
        WITH CTE AS (
        SELECT TOP 1 
        ppd.Id
        FROM tbl_projectprecheckdetails ppd
        INNER JOIN tbl_projectdetails pd ON ppd.projectdetailsid = pd.id
        WHERE
            ppd.isactive = 1
            AND pd.isactive = 1
            AND (
                (ppd.Id = @idnumbers)
                OR (
                    ppd.drawingnumberid = @drawingnumberid
                    AND pd.id = @consumedinId
                    AND pd.prodseriesid = @consumedinseriesid
                    AND pd.drawingnumberid = @consumedindrawingid
                    AND ppd.isprecheckcomplete = 0
                    AND ppd.oldrow=0
                )
            )
        ORDER BY ppd.modifieddate DESC
    )
    UPDATE ppd
    SET
        ppd.irnumber = @irnumber,
        ppd.msnnumber = @msnnumber,
        ppd.mrirnumber = @mrirnumber,
        ppd.username = @username,
        ppd.consumedindrawing = @consumedindrawing,
        ppd.consumedinproductionordernumber = @consumedinproductionordernumber,
        ppd.remarks = @remarks,
        ppd.quantity = ppd.quantity,
        ppd.unit = @unit,
        ppd.modifiedby = @modifiedby,
        ppd.modifieddate = @modifieddate,
        ppd.idnumber = @idnumber,
        ppd.idnumbers = @idnumbers,
        ppd.precheckdate = @precheckdate,
        ppd.isactive = 1,
        ppd.createdby=@createdby,
        ppd.remainingquantity=@remainingquantity,
        ppd.isprecheckcomplete = 
        CASE 
            WHEN @remainingquantity = 0 THEN 1 
            ELSE 0 
        END,
        ppd.qrcodeid = @qrcodeid
        OUTPUT INSERTED.Id
        FROM tbl_projectprecheckdetails ppd
        INNER JOIN CTE ON ppd.Id = CTE.Id
";

         

        #region Insertinto ProjectPrecheck Details

        public static readonly string INSERT_PROJECT_PRECHECK_DETAILS = @"
        INSERT INTO tbl_projectprecheckdetails (drawingnumberid, prodseriesid,quantity,unit,  createdby, createddate,  isactive, projectdetailsid, isprecheckcomplete, componenttype) 
        OUTPUT INSERTED.Id VALUES ( @drawingnumberid, @ProdSeriesId,@Quantity, @Unit, @createdby, @createddate, 1, @projectdetailsid, 0, @ComponentType)";

        #endregion

        #region INSERT_PROJECT_DETAILS

        public static readonly string INSERT_PROJECT_DETAILS = @"
        INSERT INTO tbl_projectdetails (idnumbers,prodseriesid, projectnumber, productionordernumber,drawingnumberid,createdby, createddate,  isactive,precheckstatus)
        OUTPUT INSERTED.Id VALUES (@IdNumbers,@ProdSeriesId, @ProjectNumber, @ProductionOrderNumber, @DrawingNumberId, @CreatedBy, @CreatedDate, 1,1)";


        #endregion

        #region GET_PROJECT_DETAILS

        public static readonly string GET_PROJECT_DETAILS = @"
       SELECT [id]
      ,[projectnumber]
      ,[productionordernumber]
      ,[assemblynumberid]
      ,[drawingnumberid]
      ,[componenttypeid]
      ,[shortdescription]
      ,[quantity]
      ,[startdate]
      ,[enddate]
      ,[createdby]
      ,[createddate]
      ,[modifiedby]
      ,[modifieddate]
      ,[isactive]
      ,[idnumbers]
      ,[prodseriesid]
      ,[expitydate]
      ,[precheckstatus] 
        FROM tbl_projectdetails
           WHERE  drawingnumberid = @DrawingNumberId
            AND prodseriesid = @ProdSeriesId
            AND (id = @IdNumbers OR idnumbers = @IdNumbers) AND isactive=1";

        #endregion

        #region GET_PROJECT_CONTEXT_BY_PO_AND_ID

        // Resolve the assembly drawing + production series for a project purely from
        // ProductionOrderNumber + IdNumber - used per Excel row when the caller only
        // supplies the QR code, production order number and id number.
        // ProductionOrderNumber is not globally unique (tbl_productionordermaster only
        // enforces uniqueness per ProductionOrderNumber+ProdSeriesId+StartIdNumber), so the
        // same PO+IdNumber pair can exist under more than one assembly drawing. When the
        // caller supplies @ParentDrawingNumberId (resolved from the Excel row's "Parent
        // Drawing" column), it disambiguates by also matching the project's own drawing.
        public static readonly string GET_PROJECT_CONTEXT_BY_PO_AND_ID = @"
        SELECT TOP 1
            id AS ProjectDetailsId,
            drawingnumberid AS DrawingNumberId,
            prodseriesid AS ProdSeriesId
        FROM tbl_projectdetails
        WHERE productionordernumber = @ProductionOrderNumber
          AND (id = @IdNumbers OR idnumbers = @IdNumbers)
          AND isactive = 1
          AND (@ParentDrawingNumberId IS NULL OR drawingnumberid = @ParentDrawingNumberId)
        ORDER BY id DESC";

        // Resolve a drawing number's text (e.g. "CK310-0800-361") to its id -
        // used to cross-check the Excel row's Drawing Number column against the QR's own drawing.
        public static readonly string GET_DRAWINGNUMBER_ID_BY_NAME = @"
        SELECT TOP 1 id
        FROM tbl_drawingnumber
        WHERE drawingnumber = @DrawingNumber
          AND isactive = 1";

        #endregion

        #region DELETE_PRECHECK_DETAILS

        // Find the tbl_projectprecheckdetails row for a given project + drawing number,
        // regardless of its current isactive status, so the caller can decide whether it's
        // already deleted before attempting the soft-delete.
        public static readonly string GET_PROJECT_PRECHECK_DETAIL_BY_PROJECT_AND_DRAWING = @"
        SELECT TOP 1
            id AS Id,
            quantity AS Quantity,
            qrcodeid AS QRCodeId,
            isactive AS IsActive,
            isprecheckcomplete AS IsPrecheckComplete
        FROM tbl_projectprecheckdetails
        WHERE projectdetailsid = @ProjectDetailsId
          AND drawingnumberid = @DrawingNumberId
          AND isDeleted is NULL";

        public static readonly string DELETE_PROJECT_PRECHECK_DETAIL = @"
        UPDATE tbl_projectprecheckdetails
        SET isactive = 0,
            isprecheckcomplete = 1,
            isDeleted=1,
            modifiedby = @ModifiedBy,
            modifieddate = @ModifiedDate
        WHERE id = @Id";

        // Reset the row back to an unconsumed state: clear every consumption-related field,
        // mark it not-complete and inactive.
        public static readonly string REMOVE_PROJECT_PRECHECK_DETAIL = @"
        UPDATE tbl_projectprecheckdetails
        SET qrcodeid = NULL,
            precheckdate = NULL,
            mrirnumber = NULL,
            consumedinproductionordernumber = NULL,
            idnumbers = NULL,
            idnumber = NULL,
            remarks = NULL,
            consumedindrawing = NULL,
            msnnumber = NULL,
            irnumber = NULL,
            isprecheckcomplete = 0,
            remainingquantity = NULL,
            isactive = 1,
            modifiedby = @ModifiedBy,
            modifieddate = @ModifiedDate
        WHERE id = @Id";

        #endregion

        #region UPDATE_PROJECT_PRECHECK_STATUS_DETAILS

        public static readonly string
            UPDATE_PROJECT_PRECHECK_STATUS_DETAILS = @"
            UPDATE tbl_projectdetails
            SET 
                precheckstatus = @precheckstatus
            OUTPUT INSERTED.Id
            WHERE Id = (
                SELECT TOP 1 Id 
                FROM tbl_projectdetails
                WHERE drawingnumberid = @DrawingNumberId
                AND prodseriesid = @ProdSeriesId
                AND (Id = @IdNumbers OR idnumbers = @IdNumbers)
                AND isactive = 1
                ORDER BY drawingnumberid
            )";


        #endregion

        #region
        public static readonly string GET_VIEW_PRECHECK_BY_PO_NUMBER = @"
    SELECT
    ppd.prodseriesid,
    ps.productionseries,
    ppd.drawingnumberid,
    dn.drawingnumber,
    nom.id AS nomenclatureid,
    nom.nomenclature,
    dn.lnitemcode,

    COALESCE(ppd.idnumber, CAST(ppd.idnumbers AS VARCHAR(50))) AS idnumber,

    ppd.irnumber,
    ppd.msnnumber,
    ppd.mrirnumber,
    ppd.consumedindrawing,
    ppd.remarks AS Remarks,
    ppd.quantity,
    adm.unit,
    ppd.mydate,
    ppd.componentcodeid,
    ppd.srnumber,
    ppd.createdby,
    ppd.createddate,
    ppd.modifiedby,
    ppd.modifieddate,
    ppd.isactive,
    pd.id AS ProjectDetailsId,
    pd.idnumbers AS StartIdNumber,
    pd.projectnumber,
    pd.productionordernumber,
    ppd.id AS PrecheckDetailsId,
    ppd.isprecheckcomplete,
    ppd.componenttype,
    ppd.precheckdate,
    tu.username AS Username,
    ppd.remainingquantity AS RemainingQuantity,
    adm.findno AS FindNo,
    CASE

    WHEN ppd.componenttype = 'ID' AND ppd.remainingquantity IS NULL AND ppd.isprecheckcomplete = 1
            THEN 'Completed'
    
        -- Batch type: remainingquantity = 0 and isprecheckcomplete = 1 → Completed
        WHEN ppd.componenttype = 'BATCH' AND ppd.remainingquantity = 0 AND ppd.isprecheckcomplete = 1 
            THEN 'Completed'
    WHEN ppd.remainingquantity IS NULL THEN 'Pending'
    WHEN ppd.remainingquantity = 0 THEN 'Completed'
    WHEN ppd.remainingquantity >= ppd.quantity THEN 'Pending'
    WHEN ppd.remainingquantity < ppd.quantity THEN 'Updated'
    END AS PrecheckStatus,
    ppd.isrejected AS IsRejected,

   CASE
    WHEN EXISTS (
       SELECT 1
        FROM tbl_material_requestion mr
        INNER JOIN tbl_productionordermaster pom 
            ON pom.productionordernumber = @productionordernumber
            AND pom.isactive = 1
        WHERE mr.rejectedcomponentdrawingnumberid = ppd.drawingnumberid
          AND mr.productionordernumberid = pom.id                    -- check PO id in mr
          AND mr.idnumber = ppd.idnumber
          AND mr.isactive = 1
          AND mr.statusid = 2
          AND mr.status <> 'Request- Deleted'
    )
    AND ppd.isprecheckcomplete=1
    AND ppd.isactive = 1
    AND ppd.isrejected = 0
THEN 1
ELSE 0
END AS readyForRejection


FROM tbl_projectdetails pd

INNER JOIN tbl_projectprecheckdetails ppd
    ON ppd.projectdetailsid = pd.id
    AND pd.productionordernumber=@productionordernumber
    AND (@idnumber IS NULL OR pd.idnumbers=@idnumber)
    AND ppd.isactive = 1

INNER JOIN tbl_productionseries ps
    ON ppd.prodseriesid = ps.id

LEFT JOIN tbl_users tu
    ON ppd.modifiedby = tu.id

INNER JOIN tbl_drawingnumber dn
    ON ppd.drawingnumberid = dn.id

LEFT JOIN (
    SELECT drawingnumberid, MIN(nomenclatureid) AS nomenclatureid
    FROM tbl_drawingnomenclaturemapping
    GROUP BY drawingnumberid
) nommap ON dn.id = nommap.drawingnumberid


LEFT JOIN tbl_nomenclature nom
    ON nommap.nomenclatureid = nom.id

OUTER APPLY (
    SELECT TOP 1 adm2.findno, adm2.unit
    FROM tbl_assemblydrawingmapping adm2
    WHERE adm2.drawingnumber = ppd.drawingnumberid
      AND adm2.parentdrawingnumber = pd.drawingnumberid
      AND adm2.isactive = 1
    ORDER BY adm2.id
) adm

WHERE
    pd.isactive = 1
    -- AND ppd.isrejected = 0
    AND (@productionordernumber IS NULL OR pd.productionordernumber = @productionordernumber)
    AND (@drawingnumberid IS NULL OR pd.drawingnumberid = @drawingnumberid)
    AND (@productionseriesid IS NULL OR pd.prodseriesid = @productionseriesid)
    AND (@idnumber IS NULL OR pd.idnumbers = @idnumber)
ORDER BY TRY_CAST(adm.findno AS INT) ASC;
";

        // Same shape as GET_VIEW_PRECHECK_BY_PO_NUMBER, but fetches every row for a whole batch of
        // production order numbers in one round-trip (used by PendingPrecheck/ExportPendingPrecheck,
        // which otherwise ran this query once per matching production order - O(N) DB round-trips
        // instead of one). The readyForRejection EXISTS check is correlated via ppd.productionordernumberid
        // (the row's own FK) instead of re-deriving it from a single @productionordernumber parameter,
        // since that parameter is now a list.
        public static readonly string GET_VIEW_PRECHECK_BY_PO_NUMBERS = @"
    SELECT
    ppd.prodseriesid,
    ps.productionseries,
    ppd.drawingnumberid,
    dn.drawingnumber,
    nom.id AS nomenclatureid,
    nom.nomenclature,
    dn.lnitemcode,

    COALESCE(ppd.idnumber, CAST(ppd.idnumbers AS VARCHAR(50))) AS idnumber,

    ppd.irnumber,
    ppd.msnnumber,
    ppd.mrirnumber,
    ppd.consumedindrawing,
    ppd.remarks AS Remarks,
    ppd.quantity,
    adm.unit,
    ppd.mydate,
    ppd.componentcodeid,
    ppd.srnumber,
    ppd.createdby,
    ppd.createddate,
    ppd.modifiedby,
    ppd.modifieddate,
    ppd.isactive,
    pd.id AS ProjectDetailsId,
    pd.idnumbers AS StartIdNumber,
    pd.projectnumber,
    pd.productionordernumber,
    ppd.id AS PrecheckDetailsId,
    ppd.isprecheckcomplete,
    ppd.componenttype,
    ppd.precheckdate,
    tu.username AS Username,
    ppd.remainingquantity AS RemainingQuantity,
    adm.findno AS FindNo,
    CASE

    WHEN ppd.componenttype = 'ID' AND ppd.remainingquantity IS NULL AND ppd.isprecheckcomplete = 1
            THEN 'Completed'

        -- Batch type: remainingquantity = 0 and isprecheckcomplete = 1 → Completed
        WHEN ppd.componenttype = 'BATCH' AND ppd.remainingquantity = 0 AND ppd.isprecheckcomplete = 1
            THEN 'Completed'
    WHEN ppd.remainingquantity IS NULL THEN 'Pending'
    WHEN ppd.remainingquantity = 0 THEN 'Completed'
    WHEN ppd.remainingquantity >= ppd.quantity THEN 'Pending'
    WHEN ppd.remainingquantity < ppd.quantity THEN 'Updated'
    END AS PrecheckStatus,
    ppd.isrejected AS IsRejected,

   CASE
    WHEN EXISTS (
       SELECT 1
        FROM tbl_material_requestion mr
        WHERE mr.rejectedcomponentdrawingnumberid = ppd.drawingnumberid
          AND mr.productionordernumberid = ppd.productionordernumberid
          AND mr.idnumber = ppd.idnumber
          AND mr.isactive = 1
          AND mr.statusid = 2
          AND mr.status <> 'Request- Deleted'
    )
    AND ppd.isprecheckcomplete=1
    AND ppd.isactive = 1
    AND ppd.isrejected = 0
THEN 1
ELSE 0
END AS readyForRejection


FROM tbl_projectdetails pd

INNER JOIN tbl_projectprecheckdetails ppd
    ON ppd.projectdetailsid = pd.id
    AND ppd.isactive = 1

INNER JOIN tbl_productionseries ps
    ON ppd.prodseriesid = ps.id

LEFT JOIN tbl_users tu
    ON ppd.modifiedby = tu.id

INNER JOIN tbl_drawingnumber dn
    ON ppd.drawingnumberid = dn.id

LEFT JOIN (
    SELECT drawingnumberid, MIN(nomenclatureid) AS nomenclatureid
    FROM tbl_drawingnomenclaturemapping
    GROUP BY drawingnumberid
) nommap ON dn.id = nommap.drawingnumberid


LEFT JOIN tbl_nomenclature nom
    ON nommap.nomenclatureid = nom.id

OUTER APPLY (
    SELECT TOP 1 adm2.findno, adm2.unit
    FROM tbl_assemblydrawingmapping adm2
    WHERE adm2.drawingnumber = ppd.drawingnumberid
      AND adm2.parentdrawingnumber = pd.drawingnumberid
      AND adm2.isactive = 1
    ORDER BY adm2.id
) adm

WHERE
    pd.isactive = 1
    AND pd.productionordernumber IN {{PO_LIST}}
ORDER BY pd.productionordernumber, TRY_CAST(adm.findno AS INT) ASC;
";

        public static readonly string GET_VIEW_PRECHECK_BY_ID_NUMBER = @"
                               SELECT
	    ppd.prodseriesid,
    ps.productionseries,
    ppd.drawingnumberid,
    dn.drawingnumber,
	nom.id as nomenclatureid,
    nom.nomenclature,
    dn.lnitemcode,
    COALESCE(ppd.idnumber, CAST(ppd.idnumbers AS VARCHAR(50))) as idnumber,
    ppd.irnumber,
    ppd.msnnumber,
    ppd.mrirnumber,
    ppd.consumedindrawing,
    ppd.remarks AS Remarks,
    ppd.quantity,
    adm.unit,
    ppd.mydate,
    ppd.componentcodeid,
    ppd.srnumber,
    tu.username AS Username,
    ppd.createdby,
    ppd.createddate,
    ppd.modifiedby,
    ppd.modifieddate,
	pd.id AS ProjectDetailsId,
    pd.projectnumber,
    pd.productionordernumber,
    ppd.Id AS PrecheckDetailsId,
    ppd.isprecheckcomplete,
    ppd.componenttype,
    ppd.precheckdate,
    ppd.remainingquantity AS RemainingQuantity,
    adm.findno AS FindNo,
    ppd.isrejected AS IsRejected,
   CASE
    WHEN ppd.isprecheckcomplete = 1
         AND ppd.isrejected = 0
         AND (
              mr.id IS NULL
              OR (mr.statusid = 2 AND mr.isactive = 1)
         )
    THEN 1
    ELSE 0
END AS ReadyForRejection,

    mr.status AS MaterialRequisitionStatus
FROM 
    tbl_projectprecheckdetails ppd
INNER JOIN 
    tbl_projectdetails pd 
ON 
    ppd.projectdetailsid = pd.id
INNER JOIN 
    tbl_productionseries ps 
ON 
    ppd.prodseriesid = ps.id
LEFT JOIN 
    tbl_users tu 
ON 
    ppd.modifiedby = tu.id 
INNER JOIN 
    tbl_drawingnumber dn 
ON 
    ppd.drawingnumberid = dn.id


LEFT JOIN 
     tbl_drawingnomenclaturemapping nommap 
     ON dn.id = nommap.drawingnumberid 
 LEFT JOIN
     tbl_nomenclature nom
     ON nommap.nomenclatureid = nom.id
LEFT JOIN
    tbl_material_requestion mr ON ppd.Id = mr.rejectedcomponentid AND mr.isactive = 1
OUTER APPLY (
    SELECT TOP 1 adm2.findno, adm2.unit
    FROM tbl_assemblydrawingmapping adm2
    WHERE adm2.drawingnumber = ppd.drawingnumberid
      AND adm2.parentdrawingnumber = pd.drawingnumberid
      AND adm2.isactive = 1
    ORDER BY adm2.id
) adm
WHERE
      pd.isactive = 1 AND ppd.isactive = 1
       AND (@drawingnumberid IS NULL OR pd.drawingnumberid = @drawingnumberid)
            AND (@productionseriesid IS NULL OR pd.prodseriesid = @productionseriesid)
            AND (@idnumber IS NULL OR (pd.id = @idnumber OR pd.idnumbers = @idnumber))
ORDER BY TRY_CAST(adm.findno AS INT) ASC";


        #endregion

        #region ExportViewPewcheck
        public static readonly string Export_View_Precheck = @"
             
SELECT 
    ppd.prodseriesid,
    ps.productionseries,
    ppd.drawingnumberid,
    dn.drawingnumber,
    nom.id AS nomenclatureid,
    nom.nomenclature,
    dn.lnitemcode,

    COALESCE(ppd.idnumber, CAST(ppd.idnumbers AS VARCHAR(50))) AS idnumber,

    ppd.irnumber,
    ppd.msnnumber,
    ppd.mrirnumber,
    ppd.consumedindrawing,
    ppd.remarks AS Remarks,
    ppd.quantity,
    adm.unit,
    ppd.mydate,
    ppd.componentcodeid,
    ppd.srnumber,
    tu.username AS Username,
    ppd.createdby,
    ppd.createddate,
    ppd.modifiedby,
    ppd.modifieddate,
    ppd.isactive,
    pd.id AS ProjectDetailsId,
    pd.projectnumber,
    pd.productionordernumber,
    ppd.id AS PrecheckDetailsId,
    ppd.isprecheckcomplete,
    ppd.componenttype,
    ppd.precheckdate,
    adm.findno AS FindNo,

    ppd.isrejected AS IsRejected,

   CASE
    WHEN EXISTS (
        SELECT 1
        FROM tbl_material_requestion mr
        WHERE mr.rejectedcomponentid = ppd.id
          AND mr.isactive = 1
          AND mr.statusid = 2
    )
    AND ppd.isprecheckcomplete = 1
    AND ppd.isactive = 1
    AND ppd.isrejected = 0
THEN 1
ELSE 0
END AS readyForRejection


FROM tbl_projectdetails pd

INNER JOIN tbl_projectprecheckdetails ppd
    ON ppd.projectdetailsid = pd.id
    AND pd.productionordernumber=@productionordernumber
    AND ppd.isactive = 1

INNER JOIN tbl_productionseries ps
    ON ppd.prodseriesid = ps.id

LEFT JOIN tbl_users tu
    ON ppd.modifiedby = tu.id

INNER JOIN tbl_drawingnumber dn
    ON ppd.drawingnumberid = dn.id

LEFT JOIN (
    SELECT drawingnumberid, MIN(nomenclatureid) AS nomenclatureid
    FROM tbl_drawingnomenclaturemapping
    GROUP BY drawingnumberid
) nommap ON dn.id = nommap.drawingnumberid


LEFT JOIN tbl_nomenclature nom
    ON nommap.nomenclatureid = nom.id

OUTER APPLY (
    SELECT TOP 1 adm2.findno, adm2.unit
    FROM tbl_assemblydrawingmapping adm2
    WHERE adm2.drawingnumber = ppd.drawingnumberid
      AND adm2.parentdrawingnumber = pd.drawingnumberid
      AND adm2.isactive = 1
    ORDER BY adm2.id
) adm

WHERE
    pd.isactive = 1

    AND    ppd.isrejected = 0
    AND (@productionordernumber IS NULL OR pd.productionordernumber = @productionordernumber)
    AND (@drawingnumberid IS NULL OR pd.drawingnumberid = @drawingnumberid)
    AND (@productionseriesid IS NULL OR pd.prodseriesid = @productionseriesid)
    AND (@idnumber IS NULL OR pd.idnumbers = @idnumber)
ORDER BY TRY_CAST(adm.findno AS INT) ASC;

        ";
        #endregion
        #region GET_Available_Components
        public static readonly string GET_Available_Components = @"
SELECT
    pd.idnumbers AS id,
    ppd.idnumber,
    ppd.quantity,
    pd.drawingnumberid,
    dn.drawingnumber,
    pd.prodseriesid,
    tps.productionseries,
    nom.nomenclature,
    pd.productionordernumber,
    pd.projectnumber,
    pd.modifiedby,
    pd.idnumbers As IdNumber,
    pd.modifieddate,
    pd.createdby,
    u.userid,
    u.username AS createdbyname,
    u.email,
    pd.createddate,
    CASE 
        WHEN (
            SELECT COUNT(*) 
            FROM tbl_projectprecheckdetails ppd2
            INNER JOIN tbl_projectdetails pd2 
                ON pd2.id = ppd2.projectdetailsid
            WHERE pd2.productionordernumber = pd.productionordernumber
              AND pd2.idnumbers = pd.idnumbers
              AND ppd2.isactive = 1
              AND ppd2.isprecheckcomplete = 1
        ) > 0 THEN 'Partial'
        ELSE 'Pending'
    END AS PrecheckStatus,
    pd.precheckstatus AS PrecheckStatusId
FROM 
    tbl_projectprecheckdetails ppd
INNER JOIN 
    tbl_projectdetails pd ON pd.id = ppd.projectdetailsid
INNER JOIN 
    tbl_drawingnumber dn ON dn.id = pd.drawingnumberid
LEFT JOIN 
    tbl_productionseries tps ON tps.id = pd.prodseriesid
LEFT JOIN 
    tbl_drawingnomenclaturemapping nommap ON pd.drawingnumberid = nommap.drawingnumberid
LEFT JOIN 
    tbl_nomenclature nom ON nommap.nomenclatureid = nom.id
LEFT JOIN 
    tbl_users u ON CAST(pd.createdby AS VARCHAR(MAX)) = u.id
WHERE 
    ppd.drawingnumberid = @drawingnumberid 
    AND ppd.prodseriesid = @productionseriesid
    AND ppd.isprecheckcomplete = 0
    AND (pd.precheckstatus IS NULL OR pd.precheckstatus != 3)
    AND (@fromDate IS NULL OR pd.createddate >= @fromDate)
    AND (@toDate IS NULL OR pd.createddate < DATEADD(DAY, 1, @toDate))
ORDER BY 
    pd.createddate DESC";
        #endregion

        #endregion

        #region Make Available Components

        public static readonly string GET_AVAILABLE_COMPONENT_ORDER = @"
            WITH RankedQRCodes AS (
   SELECT 
      q.drawingnumberid,
      d.drawingnumber,
      q.productionseriesid,
      q.idnumber,
      q.quantity,
      q.remainingquantity,
      tps.productionseries,
      stl.racklocation AS Location,
      q.qrcodenumber,
      q.expirydate,
      q.manufacturingdate,
      q.projectnumber,
      q.productionordernumber,
      qs.qrcodestatus as Status,  -- <== Added QR Code Status here
      q.refdocremarks AS Remarks,
      q.fanmannumber AS FanManNo,
      ROW_NUMBER() OVER (
           PARTITION BY q.drawingnumberid, q.productionseriesid
           ORDER BY 
               CASE 
                   WHEN d.isexpiry = 1 THEN q.expirydate 
                   ELSE q.manufacturingdate 
               END DESC
      ) AS rnk
   FROM tbl_qrcodedetails q
   INNER JOIN tbl_drawingnumber d 
       ON q.drawingnumberid = d.id
   INNER JOIN tbl_productionseries tps
       ON tps.id = q.productionseriesid
   LEFT JOIN tbl_drawingnlnitemlocationmapping l
       ON d.id = l.drawingnumberid
   LEFT JOIN tbl_storeitemlocation stl
       ON stl.id = l.racklocationid
   LEFT JOIN tbl_qrcodestatus qs  -- <== Join for QR code status
       ON q.qrcodestatusid = qs.id
   WHERE q.qrcodestatusid = 1
     AND q.drawingnumberid = @drawingnumberid
    AND q.isactive=1
)
SELECT * FROM RankedQRCodes 
ORDER BY expirydate, manufacturingdate;
 ";
        #endregion

        #region REJECT_AND_DUPLICATE_PRECHECK

        public static readonly string REJECT_AND_DUPLICATE_PRECHECK = @"
BEGIN TRANSACTION;

-- 2️⃣ Update current row → mark as rejected (keep isprecheckcomplete as is)
UPDATE tbl_projectprecheckdetails
SET 
    isrejected = 1,
    remarks = @RejectedRemarks,
    modifieddate = GETDATE(),
    modifiedby = @CreatedBy
WHERE Id = @PrecheckDetailsId;

-- 3️⃣ Insert new row → duplicate with only drawingnumberid, quantity, and nomenclatureid (set isprecheckcomplete to 0)
INSERT INTO tbl_projectprecheckdetails
(
    drawingnumberid,
    quantity,
    nomenclatureid,
    componenttype,
    remarks,
    createdby,
    createddate,
    modifiedby,
    modifieddate,
    isactive,
    prodseriesid,
    projectdetailsid,
    isprecheckcomplete,
    isrejected,
    unit
)
SELECT
    drawingnumberid,
    quantity,
    nomenclatureid,
    @ComponentType,
    @DuplicateRemarks,
    @CreatedBy,
    GETDATE(),
    @CreatedBy,
    GETDATE(),
    1,
    prodseriesid,
    projectdetailsid,
    0,
    0,
    unit
FROM tbl_projectprecheckdetails
WHERE Id = @PrecheckDetailsId;

-- 4️⃣ Update existing material requisition that triggered this rejection to 'Completed'
UPDATE tbl_material_requestion
SET status = 'Completed',
    modifieddate = GETDATE(),
    modifiedby = @CreatedBy,
    statusid=3
WHERE rejectedcomponentdrawingnumberid = @DrawingNumberId AND isactive = 1 AND statusid = 2;

COMMIT TRANSACTION;";

        #endregion

        #region update QRcode quantity

        public static readonly string FindTotalQuantity = @"
;WITH OrderedQR AS
(
    SELECT 
        id,
        ROW_NUMBER() OVER (ORDER BY createddate ASC) AS rn
    FROM tbl_qrcodedetails
    WHERE drawingnumberid = @DrawingNumberId
      AND isactive = 1
)
UPDATE qr
SET 
    qr.isactive = 0,
    qr.modifiedby = @CreatedBy,
    qr.modifieddate = GETDATE()
FROM tbl_qrcodedetails qr
INNER JOIN OrderedQR oq 
    ON qr.id = oq.id
WHERE oq.rn <= @UpdatedQuantity;

-- Remaining available QR count for the drawing number id
SELECT COUNT(*) AS AvailableQrCount
FROM tbl_qrcodedetails
WHERE drawingnumberid = @DrawingNumberId
  AND isactive = 1;";


        #endregion

        #region UPDATE_QRCODE_REMAINING_BATCH
        public static readonly string UPDATE_Componnet_REMAINING_Quantity = @"
        UPDATE ppd
        SET ppd.remainingquantity = ppd.remainingquantity-@UpdatedQuantity,
            ppd.consumedinquantity = CASE
                WHEN LOWER(@ComponentType) = 'batch' THEN ISNULL(ppd.consumedinquantity, 0) + @UpdatedQuantity
                ELSE @UpdatedQuantity
            END
        OUTPUT INSERTED.remainingquantity
        FROM tbl_projectprecheckdetails ppd

        INNER JOIN tbl_projectdetails pd
            ON ppd.projectdetailsid = pd.id

        WHERE 
            pd.productionordernumber = @ProductionOrderNumber
            AND pd.idnumbers = @IdNumber
            AND ppd.drawingnumberid = @DrawingNumberId
            AND ppd.remainingquantity>0
            AND pd.isactive = 1;
        ";

        #endregion

        #region UPDATE_QRCODE_REMAINING_BATCH
        public static readonly string UPDATE_Componnet_INITIAL_Quantity = @"
        UPDATE ppd
        SET ppd.remainingquantity = ppd.quantity
        OUTPUT INSERTED.remainingquantity
        FROM tbl_projectprecheckdetails ppd
        INNER JOIN tbl_projectdetails pd
            ON ppd.projectdetailsid = pd.id
        WHERE 
            pd.productionordernumber = @ProductionOrderNumber
            AND pd.idnumbers = @IdNumber
            AND ppd.drawingnumberid = @DrawingNumberId
            AND pd.isactive = 1
            AND ppd.remainingquantity IS NULL;   
                ";

        #endregion

        #region AddRemQuantityInQRcodeTable
        public static readonly string AddRemQuantityInQRcodeTable = @"
                   UPDATE tbl_qrcodedetails
        SET
            remainingquantity = @Quantity

        WHERE
            qrcodenumber = @QrcodeNumber
            AND componenttypeid = 2
            AND isactive = 1;
        ";
        #endregion

        #region AddRemQuantityInQRcodeTable
        public static readonly string GetRemainingQtyOfQrCode = @"
        SELECT TOP 1
            quantity,
            remainingquantity
        FROM tbl_qrcodedetails
        WHERE qrcodenumber = @QrCodeNumber
        AND remainingquantity >0
        AND isactive = 1;";
        #endregion

        #region UPDATE_QrCodeStatus
        public static readonly string UPDATE_QrCodeStatus = @"
        UPDATE tbl_qrcodedetails
        SET
            qrcodestatusid = 2,
            isactive = 0
        WHERE
            qrcodenumber = @QrCodeNumber
            AND componenttypeid = 2
            AND isactive = 1
            AND remainingquantity = 0";
        #endregion

        #region UPDATE_QrCodeRemaining_Quantity
        public static readonly string UPDATE_QrCodeRemaining_Quantity = @"
       UPDATE tbl_qrcodedetails
        SET 
            remainingquantity = @RemainingQuantity
        OUTPUT INSERTED.remainingquantity
        WHERE
            qrcodenumber = @QrCodeNumber
            AND isactive = 1";
        #endregion

        #region UPDATE_QrCodeRemaining_Quantity
        public static readonly string PRECHECK_FOR_REMAINING_QUANTITY = @"
       BEGIN TRANSACTION;

-- Insert new duplicated row
INSERT INTO tbl_projectprecheckdetails
(
    drawingnumberid,
    quantity,
    nomenclatureid,
    remarks,
    remainingquantity,
    componenttype,
    createdby,
    createddate,
    modifiedby,
    modifieddate,
    isactive,
    prodseriesid,
    projectdetailsid,
    isprecheckcomplete,
    isrejected
)
OUTPUT INSERTED.Id
SELECT
    drawingnumberid,
    quantity,
    nomenclatureid,
    @DuplicateRemarks,
    @RemainingQuantity,
    @ComponentType,
    @CreatedBy,
    GETDATE(),
    @CreatedBy,
    GETDATE(),
    1,
    prodseriesid,
    projectdetailsid,
    0,
    0
FROM tbl_projectprecheckdetails
WHERE Id = @PrecheckDetailsId
  AND isactive = 1;

COMMIT TRANSACTION;";
        #endregion

        #region Check Drawingnumber Id exists or not
        public static readonly string GET_DRAWING_NUMBER_BY_NAME = @"
        SELECT COUNT(1) 
        FROM tbl_drawingnumber
        WHERE id = @drawingnumberid
        AND isactive = 1";
        #endregion

        #region Reset remaining quantity
        public static readonly string RESET_REMAINING_QUANTITY = @"
        UPDATE ppd
        SET 
            ppd.remainingquantity = @scannedquantity+ppd.remainingquantity,
            ppd.modifieddate = @modifieddate
        FROM 
            tbl_projectprecheckdetails ppd
        INNER JOIN 
            tbl_projectdetails pd ON pd.id = ppd.projectdetailsid
        WHERE 
            pd.productionordernumber = @poNumber
            AND pd.idnumbers = @idnumber
            AND ppd.drawingnumberid = @drawingnumberid";
        #endregion

        #region Reset_QR_Quantity
        public static readonly string Reset_QR_Quantity = @"
        UPDATE  tbl_qrcodedetails
        SET remainingquantity= remainingquantity+@scannedquantity
        where qrcodenumber=@QrCodeNumber";
        #endregion

        #region UPDATE_QR_QUANTITY_AND_STATUS
        public static readonly string UPDATE_QR_QUANTITY_AND_STATUS = @"
        UPDATE tbl_qrcodedetails
        SET
            remainingquantity = remainingquantity + @Quantity,
            qrcodestatusid = CASE WHEN remainingquantity + @Quantity >= quantity THEN 1 ELSE qrcodestatusid END,
            isactive = CASE WHEN remainingquantity + @Quantity >= quantity THEN 1 ELSE isactive END
        WHERE id = @QRCodeId";
        #endregion


        #region InActive_Previous_precheckRecord
        public static readonly string InActive_Previous_precheckRecord = @"
        update tbl_projectprecheckdetails
        set oldrow=1, modifiedby=@CreatedBy, modifieddate=GetDate(), isprecheckcomplete=1
        where id=@PrecheckDetailsId
          and isactive = 1;
        ";
        #endregion


        #region AddPrecheckComponent

        // Resolve every production order (tbl_productionordermaster) that builds the given
        // assembly LnItemCode - one lnitemcode can be built under several production orders.
        public static readonly string GET_ASSEMBLY_PRODUCTION_ORDERS_BY_LNITEMCODE = @"
        SELECT DISTINCT
            pom.id AS Id,
            pom.productionordernumber AS ProductionOrderNumber,
            pom.drawingnumberid AS DrawingNumberId
        FROM tbl_productionordermaster pom
        WHERE pom.lnitemcode = @AssemblyLnItemCode
          AND pom.isactive = 1";

        // Resolve every existing tbl_projectdetails row (one per production order/unit) for the
        // given set of production order numbers, so a missing BOM component can be added to each.
        public static readonly string GET_PROJECTDETAILS_BY_PO_NUMBERS = @"
        SELECT
            id AS Id,
            prodseriesid AS ProdSeriesId,
            productionordernumber AS ProductionOrderNumber
        FROM tbl_projectdetails
        WHERE productionordernumber IN @ProductionOrderNumbers
          AND isactive = 1";

        // Resolve the child's BOM entry (quantity/componenttype) under the given assembly drawing,
        // via tbl_assemblydrawingmapping - also validates the child truly belongs to that assembly's BOM.
        public static readonly string GET_ASSEMBLY_CHILD_BOM_DETAIL = @"
        SELECT TOP 1
            dw.id AS ChildDrawingNumberId,
            asdmapping.quantity AS Quantity,
            asdmapping.unit AS Unit,
            ct.componenttype AS ComponentType
        FROM tbl_assemblydrawingmapping asdmapping
        INNER JOIN tbl_drawingnumber dw ON asdmapping.drawingnumber = dw.id AND dw.isactive = 1
        LEFT JOIN tbl_drawingcomponenttypemapping ctmap ON dw.id = ctmap.drawingnumberid AND ctmap.isactive = 1
        LEFT JOIN tbl_componenttype ct ON ctmap.componenttypeid = ct.id
        WHERE asdmapping.parentdrawingnumber = @AssemblyDrawingNumberId
          AND dw.lnitemcode = @ChildLnItemCode
          AND asdmapping.isactive = 1";

        // Resolve every assembly (parent drawing) that consumes the given child drawing number,
        // via tbl_assemblydrawingmapping - answers "which assemblies can this component be consumed in?"
        public static readonly string GET_CONSUMED_IN_ASSEMBLIES = @"
        SELECT DISTINCT
            pdw.id AS DrawingNumberId,
            pdw.drawingnumber AS DrawingNumber
        FROM tbl_assemblydrawingmapping asdmapping
        INNER JOIN tbl_drawingnumber pdw ON asdmapping.parentdrawingnumber = pdw.id
        WHERE asdmapping.drawingnumber = @DrawingNumberId
          AND asdmapping.isactive = 1
          AND pdw.isactive = 1";

        // Same shape as ProductionOrderQueries.INSERT_PROJECT_PRECHECK_DETAILS_WITH_POID, but takes the
        // component's real unit (from tbl_assemblydrawingmapping.unit) instead of hardcoding 'EA'.
        public static readonly string INSERT_PROJECT_PRECHECK_DETAILS_WITH_POID_AND_UNIT = @"
        INSERT INTO tbl_projectprecheckdetails
        (drawingnumberid, prodseriesid, projectdetailsid, quantity, unit, componenttype,
         productionordernumberid, createdby, createddate, isactive, isprecheckcomplete)
        OUTPUT INSERTED.Id
        VALUES
        (@DrawingNumberId, @ProdSeriesId, @ProjectDetailsId, @Quantity, @Unit, @ComponentType,
         @ProductionOrderNumberId, @CreatedBy, GETDATE(), 1, 0)";

        #endregion

        #region Update Precheck status
        public static readonly string UPDATE_PRECHECK_STATUS = @"
UPDATE pom
SET pom.precheckstatusid = ISNULL(psc.CalculatedStatus, 1)
FROM tbl_productionordermaster pom
LEFT JOIN (
    SELECT
        pd.productionordernumberid,
        CASE
            WHEN pom_inner.min IS NULL OR LTRIM(RTRIM(pom_inner.min)) = '' THEN 4
            WHEN COUNT(ppd.id) = SUM(CASE WHEN ppd.isprecheckcomplete = 1 THEN 1 ELSE 0 END)
                 AND COUNT(ppd.id) > 0 THEN 3
            WHEN SUM(CASE WHEN ppd.isprecheckcomplete = 1 THEN 1 ELSE 0 END) > 0 THEN 2
            ELSE 1
        END AS CalculatedStatus
    FROM tbl_projectdetails pd
    LEFT JOIN tbl_productionordermaster pom_inner ON pd.productionordernumberid = pom_inner.id
    LEFT JOIN tbl_projectprecheckdetails ppd
        ON pd.id = ppd.projectdetailsid AND ppd.isactive = 1
    WHERE pd.productionordernumberid IS NOT NULL AND pd.isactive = 1
    GROUP BY pd.productionordernumberid, pom_inner.min
) psc ON pom.id = psc.productionordernumberid
WHERE pom.isactive = 1
AND pom.productionordernumber = @PoNumber;"; 
        #endregion

    }
}
