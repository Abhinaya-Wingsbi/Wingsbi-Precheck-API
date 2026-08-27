using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Repository.Queries
{
    public static class MaterialRequisitionQueries
    {
        #region GET_MATERIAL_REQUISITION

        public static readonly string GET_MATERIAL_REQUISITION = @"
SELECT  
    tmr.prodseriesid,
    ps.productionseries,
    tmr.drawingnumberid,
    dn.drawingnumber,
    tmr.nomenclatureid,
    nom.nomenclature,
    tmr.lnitemcode,
    tmr.idnumber,
    tmr.irnumber,
    tmr.msnnumber,
    tmr.mrirnumber,
    tmr.consumedindrawing,
    tmr.remarks AS Remarks,
    tmr.quantity,
    tmr.unit,
    tmr.mydate,
    tmr.componentcodeid,
    tmr.srnumber,
    tmr.username,
    tmr.createdby,
    tmr.createddate,
    tmr.modifiedby,
    tmr.modifieddate,
    pd.id AS ProjectDetailsId,
    pd.projectnumber,
    pd.productionordernumber,
    ppd.Id AS PrecheckDetailsId,
    tmr.idnumbers,
    tmr.isprecheckcomplete,
    tmr.componenttype,
    tmr.precheckdate,
    ppd.isrejected AS IsRejected,
    tmr.id AS MaterialRequisitionId,
    tmr.requestnumber AS RequestNumber,
    tmr.rejectedcomponentid AS RejectedComponentId,
    tmr.hwno AS Hwno,
    tmr.request_owner AS RequestOwner,
    tmr.consumedinproductionordernumber,
    tmr.consumedinquantity,
    tmr.productionordernumberid,
    tmr.lnitemcodeid,
    tmr.status,
    tmr.statusid
FROM tbl_material_requestion tmr 

LEFT JOIN tbl_projectdetails pd 
    ON tmr.projectdetailsid = pd.id

LEFT JOIN tbl_productionseries ps 
    ON tmr.prodseriesid = ps.id

LEFT JOIN tbl_users tu 
    ON tmr.createdby = tu.id 

LEFT JOIN tbl_drawingnumber dn 
    ON tmr.rejectedcomponentdrawingnumberid = dn.id

OUTER APPLY (
    SELECT TOP 1 p.id, p.isrejected
    FROM tbl_projectprecheckdetails p
    WHERE p.drawingnumberid = tmr.rejectedcomponentdrawingnumberid
    ORDER BY p.id DESC
) ppd

OUTER APPLY (
    SELECT TOP 1 n.nomenclature
    FROM tbl_drawingnomenclaturemapping nm
    JOIN tbl_nomenclature n 
        ON n.id = nm.nomenclatureid
    WHERE nm.drawingnumberid = dn.id
    ORDER BY n.id
) nom

WHERE tmr.isactive = 1";


        #endregion

        #region GET_MATERIAL_REQUISITION_BY_STATUS

        public static readonly string GET_MATERIAL_REQUISITION_BY_STATUS = @"
SELECT  
    tmr.prodseriesid,
    ps.productionseries,
    tmr.drawingnumberid,
    dn.drawingnumber,
    tmr.nomenclatureid,
    nom.nomenclature,
    tmr.lnitemcode,
    tmr.idnumber,
    tmr.irnumber,
    tmr.msnnumber,
    tmr.mrirnumber,
    tmr.consumedindrawing,
    tmr.remarks AS Remarks,
    tmr.quantity,
    tmr.unit,
    tmr.mydate,
    tmr.componentcodeid,
    tmr.srnumber,
    tmr.username,
    tmr.createdby,
    tmr.createddate,
    tmr.modifiedby,
    tmr.modifieddate,
    pd.id AS ProjectDetailsId,
    pd.projectnumber,
    pd.productionordernumber,
    ppd.Id AS PrecheckDetailsId,
    tmr.idnumbers,
    tmr.isprecheckcomplete,
    tmr.componenttype,
    tmr.precheckdate,
    ppd.isrejected AS IsRejected,
    tmr.id AS MaterialRequisitionId,
    tmr.requestnumber AS RequestNumber,
    tmr.rejectedcomponentid AS RejectedComponentId,
    tmr.hwno AS Hwno,
    tmr.request_owner AS RequestOwner,
    tmr.consumedinproductionordernumber,
    tmr.consumedinquantity,
    tmr.productionordernumberid,
    tmr.lnitemcodeid,
    tmr.status,
    tmr.statusid
FROM tbl_material_requestion tmr 
LEFT JOIN tbl_projectprecheckdetails ppd
    ON tmr.rejectedcomponentid = ppd.id
INNER JOIN 
    tbl_projectdetails pd 
ON 
    tmr.projectdetailsid = pd.id
INNER JOIN 
    tbl_productionseries ps 
ON 
    tmr.prodseriesid = ps.id
INNER JOIN 
    tbl_users tu 
ON 
    tmr.createdby = tu.id 
INNER JOIN 
    tbl_drawingnumber dn 
ON 
    tmr.drawingnumberid = dn.id
LEFT JOIN 
     tbl_drawingnomenclaturemapping nommap 
     ON dn.id = nommap.drawingnumberid and nommap.isactive=1
LEFT JOIN 
     tbl_nomenclature nom 
     ON nommap.nomenclatureid = nom.id
WHERE tmr.isactive = 1 AND tmr.status = @Status";

        #endregion

        #region GET_SWAPPING_DETAILS

        public static readonly string GET_SWAPPING_DETAILS = @"
        SELECT
            sd.id,
            sd.swappedDrawingNumberID,
            dn.drawingnumber AS SwappedDrawingNumber,
            sd.fromSwappedIdNumber,
            sd.toSwappedIdNumber,
            sd.swappedFromPONumber,
            sd.swappedToPONumber,
            sd.createdDate,
            sd.modifiedDate,
            sd.createdBy,
            sd.modifiedBy,
            sd.isActive
        FROM tbl_swappingdetails sd
        LEFT JOIN tbl_drawingnumber dn
            ON sd.swappedDrawingNumberID = dn.id
        WHERE sd.isActive = 1
        ORDER BY sd.id DESC;";

        #endregion

        #region UPDATE_MATERIAL_REQUISITION

        public static readonly string UPDATE_MATERIAL_REQUISITION = @"
UPDATE tbl_material_requestion
SET 
    remarks = COALESCE(@Remarks, remarks),
    hwno = @Hwno,
    request_owner = @RequestOwner,
    status = CASE
        WHEN @StatusId = 1 THEN 'Pending-Store'
        WHEN @StatusId = 2   THEN 'Completed'
        ELSE status
    END,

    statusid = CASE
        WHEN @StatusId = 1 THEN 2
        WHEN @StatusId = 2   THEN 3
        ELSE statusid
    END,
    modifieddate = GETDATE(),
    modifiedby = @ModifiedBy
WHERE id = @MaterialRequisitionId";

        #endregion

        #region CANCEL_MATERIAL_REQUISITION

        public static readonly string CANCEL_MATERIAL_REQUISITION = @"
UPDATE tbl_material_requestion
SET
    isactive = 0,
    status = 'Request- Deleted',
    requestcancleremarks = @RequestCancleRemarks,
    modifieddate = GETDATE(),
    modifiedby = @ModifiedBy
WHERE id = @RequestId";

        #endregion

        #region CREATE_MATERIAL_REQUISITION

        public static readonly string CREATE_MATERIAL_REQUISITION = @"
DECLARE @NextRequestNumber VARCHAR(100);
DECLARE @MaxNumber INT;
DECLARE @NewId INT;
DECLARE @ProjectDetailsId INT;
DECLARE @PrecheckId INT;

SELECT @ProjectDetailsId = id
FROM tbl_projectdetails
WHERE idnumbers = @IdNumber
  AND drawingnumberid=@AssemblyDrawingNumberId
  AND prodseriesid = @ProdSeriesId
    AND productionordernumber=@ProductionOrderNumber
  AND isactive = 1;

IF @ProjectDetailsId IS NULL
BEGIN
    RAISERROR('No matching record found in project details.', 16, 1);
    RETURN;
END

    SELECT @PrecheckId = id
    FROM tbl_projectprecheckdetails
    WHERE projectdetailsid = @ProjectDetailsId
      AND drawingnumberid = @RejectedDrawingNumberId
      AND isprecheckcomplete=1
      AND prodseriesid = @ProdSeriesId
      AND idnumber=@IdNumbers
      AND isactive = 1

IF @PrecheckId IS NULL
BEGIN
    RAISERROR('No matching record found in project precheck details.', 16, 1);
    RETURN;
END

IF EXISTS (
    SELECT 1
    FROM tbl_material_requestion
    WHERE rejectedcomponentdrawingnumberid = @RejectedDrawingNumberId
      AND prodseriesid = @ProdSeriesId
      AND idnumber = @IdNumber
      AND isactive = 1
      AND statusid <> 3
)
BEGIN
    RAISERROR('Already requested.', 16, 1);
    RETURN;
END

SELECT 
    @MaxNumber = ISNULL(
        MAX(CAST(SUBSTRING(requestnumber, 4, LEN(requestnumber) - 3) AS INT)), 
        0
    )
FROM tbl_material_requestion
WHERE requestnumber LIKE 'REQ%';

SET @NextRequestNumber =
    'REQ' + RIGHT('000' + CAST(@MaxNumber + 1 AS VARCHAR(10)), 3);

INSERT INTO tbl_material_requestion (
    requestnumber, projectdetailsid, drawingnumberid, prodseriesid,
    quantity, unit, remarks, rejectedcomponentid, idnumber, irnumber,
    msnnumber, consumedindrawing, mydate, componenttype, srnumber,
    username, nomenclatureid, idnumbers, isprecheckcomplete,
    consumedinproductionordernumber, consumedinquantity, componentcodeid,
    mrirnumber, precheckdate, productionordernumberid, lnitemcodeid,
    lnitemcode, statusid, status, assemblydrawingnumberid,
    rejectedcomponentdrawingnumberid, isactive, createdby, createddate
)
SELECT TOP 1
    @NextRequestNumber,
    @ProjectDetailsId,
    ppd.drawingnumberid,
    ppd.prodseriesid,
    @Quantity,
    ppd.unit,
    COALESCE(@Remarks, ppd.remarks),
    @PrecheckId,
    ppd.idnumber,
    ppd.irnumber,
    ppd.msnnumber,
    ppd.consumedindrawing,
    ppd.mydate,
    ppd.componenttype,
    ppd.srnumber,
    ppd.username,
    nom.id,
    @IdNumber,
    0,
    ppd.consumedinproductionordernumber,
    ppd.consumedinquantity,
    ppd.componentcodeid,
    ppd.mrirnumber,
    ppd.precheckdate,
    ppd.productionordernumberid,
    ln_correct.id,
    COALESCE(map.lnitemcode, dn.lnitemcode),
    1,
    'Pending-Planner',
    @AssemblyDrawingNumberId,
    @RejectedDrawingNumberId,
    1,
    @CreatedBy,
    GETDATE()
FROM tbl_projectprecheckdetails ppd
INNER JOIN tbl_drawingnumber dn 
    ON ppd.drawingnumberid = dn.id
LEFT JOIN tbl_drawing_lnitem_map map 
    ON dn.drawingnumber = map.drawingnumber 
    AND map.isactive = 1
    AND map.lnitemcode = @LnItemCode
LEFT JOIN tbl_lnitemcode ln_correct 
    ON map.lnitemcode = ln_correct.lnitemcode 
    AND ln_correct.isactive = 1
LEFT JOIN tbl_drawingnomenclaturemapping nommap 
    ON dn.id = nommap.drawingnumberid
LEFT JOIN tbl_nomenclature nom 
    ON nommap.nomenclatureid = nom.id
    AND nom.nomenclature = @Nomenclature
WHERE ppd.projectdetailsid = @ProjectDetailsId
  AND ppd.drawingnumberid = @RejectedDrawingNumberId
  AND ppd.prodseriesid = @ProdSeriesId
  AND ppd.isactive = 1;

SET @NewId = SCOPE_IDENTITY();

SELECT 
    @NewId AS NewId,
    @NextRequestNumber AS RequestNumber;";


        #endregion

        #region GET_NEXT_REQUEST_NUMBER

        public static readonly string GET_NEXT_REQUEST_NUMBER = @"
DECLARE @MaxNumber INT;
SELECT @MaxNumber = ISNULL(MAX(CAST(SUBSTRING(requestnumber, 4, LEN(requestnumber) - 3) AS INT)), 0)
FROM tbl_material_requestion
WHERE requestnumber LIKE 'REQ%';
SELECT 'REQ' + RIGHT('000' + CAST(@MaxNumber + 1 AS VARCHAR(10)), 3) AS NextRequestNumber;";

        #endregion

        #region CHECK_PRECHECK_DETAILS_EXISTS

        public static readonly string CHECK_PRECHECK_DETAILS_EXISTS = @"
SELECT TOP 1 
    ppd.Id AS PrecheckDetailsId,
    ppd.quantity,
    ppd.unit,
    dn.drawingnumber,
    ps.productionseries,
    pd.projectnumber,
    pd.productionordernumber,
    nom.nomenclature
FROM tbl_projectprecheckdetails ppd
INNER JOIN tbl_projectdetails pd ON ppd.projectdetailsid = pd.id
INNER JOIN tbl_drawingnumber dn ON ppd.drawingnumberid = dn.id
INNER JOIN tbl_productionseries ps ON ppd.prodseriesid = ps.id
LEFT JOIN tbl_drawingnomenclaturemapping nommap ON dn.id = nommap.drawingnumberid 
LEFT JOIN tbl_nomenclature nom ON nommap.nomenclatureid = nom.id
WHERE pd.idnumbers = @IdNumber
  AND ppd.drawingnumberid = @DrawingNumberId
  AND ppd.prodseriesid = @ProdSeriesId
  AND ppd.isactive = 1
  AND pd.isactive = 1;";

        #endregion

        #region CREATE_SWAPPED_DRAWING_NUMBER

        public static readonly string CREATE_SWAPPED_DRAWING_NUMBER = @"
INSERT INTO tbl_swappingdetails (
    swappedDrawingNumberID,
    fromSwappedIdNumber,
    toSwappedIdNumber,
    swappedFromPONumber,
    swappedToPONumber,
    createdby,
    createddate,
    isactive
)
VALUES (
    @SwappedDrawingNumberID,
    @FromSwappedIdNumber,
    @ToSwappedIdNumber,
    @SwappedFromPONumber,
    @SwappedToPONumber,
    @CreatedBy,
    GETDATE(),
    1
);";

        #endregion


        #region Get_Project_PrecheckDetailsId

        public static readonly string Get_Project_PrecheckDetailsIdTo = @"
        Select 
        ppd.id 
        from tbl_projectprecheckdetails ppd
        Inner Join tbl_projectdetails pd
        on ppd.projectdetailsid=pd.id
        where pd.idnumbers=@IdNumber
        and pd.productionordernumber=@PoNumber
        and ppd.drawingnumberid=@DrawingNumberId 
        and ppd.idnumber=@DrawingIdNumber
        and ppd.isactive=1 
        and pd.isactive=1
        ;";

        #endregion


        #region Get_Project_PrecheckDetailsId

        public static readonly string Get_Project_PrecheckDetailsId = @"
        Select 
        ppd.id 
        from tbl_projectprecheckdetails ppd
        Inner Join tbl_projectdetails pd
        on ppd.projectdetailsid=pd.id
        where pd.idnumbers=@IdNumber
        and pd.productionordernumber=@PoNumber
        and ppd.drawingnumberid=@DrawingNumberId 
        and ppd.isactive=1 
        and pd.isactive=1
        ;";

        #endregion
        #region Get_Project_PrecheckDetailsId

        public static readonly string Get_Project_DetailsId = @"
        Select 
        id 
        from tbl_projectdetails
        where idnumbers=@IdNumber
        and productionordernumber=@PoNumber
        and isactive=1
        ;";

        #endregion



        #region INACTIVE_AND_DUPLICATE

        public static readonly string INACTIVE_PREVIOUS_AND_DUPLICATE = @"
        -----------------------------------------------------------------------------------------------------
        --InACtive previous record
        UPDATE tbl_projectprecheckdetails
        SET isactive = 0
        WHERE id = @Id
        AND isactive = 1;

        -----------------------------------------------------------------------------------------------------

        -- Step 2: Insert duplicate of the same row with IsActive = 1 (new active record)
        INSERT INTO tbl_projectprecheckdetails
        (
            -- list all your columns except the identity/primary key
            projectdetailsid,
            drawingnumberid,
            isprecheckcomplete,
            quantity,
            unit,
            idnumber,
            irnumber,
            msnnumber,
            idnumbers,
            componenttype,
            prodseriesid,
            CreatedBy,
            CreatedDate,
            ModifiedBy,
            ModifiedDate,
            IsActive
            -- add all other columns from your table here
        )
        SELECT
            @TargetProjectDetailsId,
            drawingnumberid,
            1,
            quantity,
            unit,
            idnumber,
            irnumber,
            msnnumber,
            idnumbers,
            componenttype,
            prodseriesid,
            CreatedBy,
            CreatedDate,
            ModifiedBy,
            ModifiedDate,
            1 AS IsActive   -- new row is active
            -- mirror all other columns here
        FROM tbl_projectprecheckdetails
        WHERE id = @ProjectPrecheckDetailsId;";

        #endregion

        #region INACTIVE_AND_DUPLICATE

        public static readonly string INACTIVE_PREVIOUS_AND_DUPLICATE_SOURCE_PO = @"
        -----------------------------------------------------------------------------------------------------
        --InACtive previous record
        UPDATE tbl_projectprecheckdetails
        SET isactive = 0
        WHERE id = @Id
        AND isactive = 1;

        -----------------------------------------------------------------------------------------------------

        -- Step 2: Insert duplicate of the same row with IsActive = 1 (new active record)
        INSERT INTO tbl_projectprecheckdetails
        (
            -- list all your columns except the identity/primary key
            projectdetailsid,
            drawingnumberid,
            isprecheckcomplete,
            quantity,
            unit,
            componenttype,
            prodseriesid,
            CreatedBy,
            CreatedDate,
            ModifiedDate,
            precheckdate,
            IsActive
            -- add all other columns from your table here
        )
        SELECT
            projectdetailsid,
            drawingnumberid,
            0,
            quantity,
            unit,
            componenttype,
            prodseriesid,
            CreatedBy,
            CreatedDate,
            ModifiedDate,
            GetDate(),
            1 AS IsActive   -- new row is active
            -- mirror all other columns here
        FROM tbl_projectprecheckdetails
        WHERE id = @id;";

        #endregion

        #region Get_Project_PrecheckDetailsId

        public static readonly string CHECK_COMPONENT_TYPE = @"
        Select 
        dct.componenttypeid 
        from tbl_drawingnumber dn
        Inner join tbl_drawingcomponenttypemapping dct
        on dn.id = dct.drawingnumberid
        where dct.drawingnumberid=@DrawingNumberId
        and dn.isactive=1
        ;";

        #endregion

    }
}
