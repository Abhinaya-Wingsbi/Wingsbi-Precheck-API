namespace Godrej.Precheck.Repository.Queries
{
    public static class ProductionOrderQueries
    {
        #region INSERT_PRODUCTION_ORDER_MASTER

        public static readonly string INSERT_PRODUCTION_ORDER_MASTER = @"
            INSERT INTO tbl_productionordermaster 
            (productionordernumber, projectnumber, projectdescription, lnitemcode, itemdescription,
             prodseriesid, startidnumber,endidnumber,quantity, drawingnumberid, lnitemcodeid, createdby, createddate, isactive,mrirnumber, min, status, buildnumber, snagsheetno)
            OUTPUT INSERTED.id
            VALUES 
            (@ProductionOrderNumber, @ProjectNumber, @ProjectDescription, @LnItemCode, @ItemDescription,
             @ProdSeriesId, @StartIdNumber,(@StartIdNumber + @Quantity)-1, @Quantity, @DrawingNumberId, @LnItemCodeId, @CreatedBy, GETDATE(), 1, @MRIRNumber, @MIN, @Status, @BuildNumber, @SnagSheetNo)";

        #endregion

        #region GET_PRODUCTION_ORDERS_FOR_PENDING_PRECHECK

        // Used by PendingPrecheck. Same column/join shape as GET_FILTERED_PRODUCTION_ORDERS (so the
        // response matches ProductionOrderController's GetAll one-for-one), but filtered by exact-match
        // optional criteria (AssemblyDrawingNumberId / ProdSeriesId / ProductionOrderNumber, AND'd together
        // when more than one is supplied) instead of GetAll's partial-match/date/status filters.
        // Per-unit precheck completeness (PendingIdNumbers) is computed separately by the caller using the
        // same logic as the ViewPrecheck API - PrecheckStatus/PrecheckStatusName here are the same
        // whole-order 4-state calculation GetAll already shows, kept for shape parity with GetAll.
        public static readonly string GET_PRODUCTION_ORDERS_FOR_PENDING_PRECHECK = @"
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
                LEFT JOIN tbl_projectdetails pd ON pom.id = pd.productionordernumberid AND pd.isactive = 1
                LEFT JOIN tbl_projectprecheckdetails ppd
                    ON pd.id = ppd.projectdetailsid
                    AND ppd.isactive = 1
                WHERE pom.isactive = 1
                GROUP BY pom.id, pom.min
            )
            SELECT
                pom.id,
                pom.productionordernumber,
                pom.projectnumber,
                pom.projectdescription,
                pom.lnitemcode,
                pom.itemdescription,
                pom.prodseriesid,
                ps.productionseries,
                pom.startidnumber,
                pom.quantity,
                pom.drawingnumberid,
                dn.drawingnumber,
                nom.nomenclature,
                ct.componenttype,
                sl.racklocation,
                pom.lnitemcodeid,
                pom.createddate,
                pom.mrirnumber AS MRIRNumber,
                pom.min As Min,
                pom.buildnumber AS BuildNumber,
                pom.snagsheetno AS SnagSheetNo,
                pom.endidnumber AS EndIdNumber,
                psc.CalculatedStatus AS PrecheckStatus,
                CASE
                    WHEN psc.CalculatedStatus = 1 THEN 'Pending'
                    WHEN psc.CalculatedStatus = 2 THEN 'Partial'
                    WHEN psc.CalculatedStatus = 3 THEN 'Completed'
                    WHEN psc.CalculatedStatus = 4 THEN 'Pending-Planner'
                    ELSE 'Unknown'
                END AS PrecheckStatusName,
                pom.modifieddate        AS ModifiedDate,
                psc.LastModifiedDate    AS LastModifiedDate
            FROM tbl_productionordermaster pom
            LEFT JOIN PrecheckStatusCalc psc ON pom.id = psc.productionordernumberid
            LEFT JOIN tbl_productionseries ps ON pom.prodseriesid = ps.id
            LEFT JOIN tbl_drawingnumber dn ON pom.drawingnumberid = dn.id
            LEFT JOIN (
                SELECT
                    dnm.drawingnumberid,
                    STRING_AGG(nom.nomenclature, ', ') AS nomenclature
                FROM tbl_drawingnomenclaturemapping dnm
                JOIN tbl_nomenclature nom
                    ON dnm.nomenclatureid = nom.id
                WHERE dnm.isactive = 1
                  AND nom.isactive = 1
                GROUP BY dnm.drawingnumberid
            ) nom
                ON dn.id = nom.drawingnumberid
            LEFT JOIN (
                SELECT
                    dctm.drawingnumberid,
                    STRING_AGG(ct.componenttype, ', ') AS componenttype
                FROM tbl_drawingcomponenttypemapping dctm
                JOIN tbl_componenttype ct
                    ON dctm.componenttypeid = ct.id
                WHERE dctm.isactive = 1
                  AND ct.isactive = 1
                GROUP BY dctm.drawingnumberid
            ) ct
                ON dn.id = ct.drawingnumberid
            LEFT JOIN (
                SELECT
                    dlm.drawingnumberid,
                    STRING_AGG(sl.racklocation, ', ') AS racklocation
                FROM tbl_drawingnlnitemlocationmapping dlm
                JOIN tbl_storeitemlocation sl
                    ON dlm.racklocationid = sl.id
                WHERE dlm.isactive = 1
                  AND sl.isactive = 1
                GROUP BY dlm.drawingnumberid
            ) sl
                ON dn.id = sl.drawingnumberid
            WHERE pom.isactive = 1
              AND (@AssemblyDrawingNumberId IS NULL OR pom.drawingnumberid = @AssemblyDrawingNumberId)
              AND (@ProdSeriesId IS NULL OR pom.prodseriesid = @ProdSeriesId)
              AND (@ProductionOrderNumber IS NULL OR pom.productionordernumber = @ProductionOrderNumber)
              AND (@LnItemCode IS NULL OR pom.lnitemcode = @LnItemCode)
            ORDER BY pom.createddate DESC";

        #endregion

        #region GET_BY_PO_NUMBER

        public static readonly string GET_BY_PO_NUMBER = @"
            SELECT
                pom.id,
                pom.productionordernumber,
                pom.projectnumber,
                pom.projectdescription,
                pom.lnitemcode,
                pom.itemdescription,
                pom.prodseriesid,
                ps.productionseries,
                pom.startidnumber,
                pom.quantity,
                pom.drawingnumberid,
                dn.drawingnumber,
                nom.nomenclature,
                ct.componenttype,
                sl.racklocation,
                pom.lnitemcodeid,
                pom.buildnumber,
                pom.snagsheetno,
                dum.unitid,
                un.unitname
            FROM tbl_productionordermaster pom
            LEFT JOIN tbl_productionseries ps ON pom.prodseriesid = ps.id
            LEFT JOIN tbl_drawingnumber dn ON pom.drawingnumberid = dn.id
            LEFT JOIN tbl_drawingnomenclaturemapping dnm ON dn.id = dnm.drawingnumberid AND dnm.isactive = 1
            LEFT JOIN tbl_nomenclature nom ON dnm.nomenclatureid = nom.id AND nom.isactive = 1
            LEFT JOIN tbl_drawingcomponenttypemapping dctm ON dn.id = dctm.drawingnumberid AND dctm.isactive = 1
            LEFT JOIN tbl_componenttype ct ON dctm.componenttypeid = ct.id AND ct.isactive = 1
            LEFT JOIN tbl_drawingnlnitemlocationmapping dlm ON dn.id = dlm.drawingnumberid AND dlm.isactive = 1
            LEFT JOIN tbl_storeitemlocation sl ON dlm.racklocationid = sl.id AND sl.isactive = 1
            LEFT JOIN tbl_drawingunitmapping dum ON dn.id = dum.drawingnumberid AND dum.isactive = 1
            LEFT JOIN tbl_unit un ON dum.unitid = un.id AND un.isactive = 1
            WHERE pom.productionordernumber = @ProductionOrderNumber
              AND pom.isactive = 1";

        #endregion

        #region GET_BY_PO_NUMBER

        public static readonly string GET_BY_PO_NUMBER_UPDATE_PO = @"
            SELECT 
                pom.id,
                pom.productionordernumber,
                pom.projectnumber,
                pom.projectdescription,
                pom.lnitemcode,
                pom.itemdescription,
                pom.prodseriesid,
                ps.productionseries,
                pom.startidnumber,
                pom.quantity,
                pom.drawingnumberid,
                dn.drawingnumber,
                nom.nomenclature,
                ct.componenttype,
                sl.racklocation,
                pom.lnitemcodeid,
                pom.mrirnumber,
                pom.buildnumber,
                pom.snagsheetno
            FROM tbl_productionordermaster pom
            LEFT JOIN tbl_productionseries ps ON pom.prodseriesid = ps.id
            LEFT JOIN tbl_drawingnumber dn ON pom.drawingnumberid = dn.id
            LEFT JOIN tbl_drawingnomenclaturemapping dnm ON dn.id = dnm.drawingnumberid AND dnm.isactive = 1
            LEFT JOIN tbl_nomenclature nom ON dnm.nomenclatureid = nom.id AND nom.isactive = 1
            LEFT JOIN tbl_drawingcomponenttypemapping dctm ON dn.id = dctm.drawingnumberid AND dctm.isactive = 1
            LEFT JOIN tbl_componenttype ct ON dctm.componenttypeid = ct.id AND ct.isactive = 1
            LEFT JOIN tbl_drawingnlnitemlocationmapping dlm ON dn.id = dlm.drawingnumberid AND dlm.isactive = 1
            LEFT JOIN tbl_storeitemlocation sl ON dlm.racklocationid = sl.id AND sl.isactive = 1
            WHERE pom.productionordernumber = @ProductionOrderNumber
            AND pom.id=@Id
              AND pom.isactive = 1";

        #endregion

        #region LOOKUP_DRAWING_BY_LNITEMCODE

        public static readonly string LOOKUP_DRAWING_BY_LNITEMCODE = @"
            SELECT TOP 1 
                dn.id AS DrawingNumberId,
                ln.id AS LnItemCodeId,
                dn.drawingnumber AS DrawingNumber,
                nom.nomenclature AS Nomenclature
            FROM tbl_drawing_lnitem_map map
            JOIN tbl_drawingnumber dn ON dn.drawingnumber = map.drawingnumber AND dn.isactive = 1
            JOIN tbl_lnitemcode ln ON ln.lnitemcode = map.lnitemcode AND ln.isactive = 1
            LEFT JOIN tbl_drawingnomenclaturemapping nommap ON dn.id = nommap.drawingnumberid AND nommap.isactive = 1
            LEFT JOIN tbl_nomenclature nom ON nommap.nomenclatureid = nom.id AND nom.isactive = 1
            WHERE map.lnitemcode = @LnItemCode AND map.isactive = 1";

        #endregion

        #region LOOKUP_PRODSERIES_BY_PREFIX

        public static readonly string LOOKUP_PRODSERIES_BY_PREFIX = @"
            SELECT TOP 1 id, productionseries
            FROM tbl_productionseries
            WHERE productionseries = @Prefix AND isactive = 1";

        #endregion

        #region INSERT_PROJECT_DETAILS_WITH_POID

        public static readonly string INSERT_PROJECT_DETAILS_WITH_POID = @"
            INSERT INTO tbl_projectdetails 
            (idnumbers, prodseriesid, projectnumber, productionordernumber, drawingnumberid, 
             productionordernumberid, createdby, createddate, isactive, precheckstatus)
            OUTPUT INSERTED.id 
            VALUES 
            (@IdNumbers, @ProdSeriesId, @ProjectNumber, @ProductionOrderNumber, @DrawingNumberId,
             @ProductionOrderNumberId, @CreatedBy, GETDATE(), 1, (
                SELECT 
                    CASE 
                        WHEN pom.min IS NOT NULL AND LTRIM(RTRIM(pom.min)) <> '' 
                            THEN 1   -- Pending
                        ELSE 4       -- Pending-Planner
                    END
                FROM dbo.tbl_productionordermaster pom
                WHERE pom.productionordernumber = @ProductionOrderNumber
                AND startidnumber=@IdNumbers
                  AND pom.isactive = 1
            ))";

        #endregion

        #region INSERT_PROJECT_PRECHECK_DETAILS_WITH_POID

        public static readonly string INSERT_PROJECT_PRECHECK_DETAILS_WITH_POID = @"
            INSERT INTO tbl_projectprecheckdetails 
            (drawingnumberid, prodseriesid, projectdetailsid, quantity, unit, componenttype, 
             productionordernumberid, createdby, createddate, isactive, isprecheckcomplete)
            OUTPUT INSERTED.Id
            VALUES 
            (@DrawingNumberId, @ProdSeriesId, @ProjectDetailsId, @Quantity, 'EA', @ComponentType,
             @ProductionOrderNumberId, @CreatedBy, GETDATE(), 1, 0)";

        #endregion

        #region CHECK_PO_EXISTS

        public static readonly string CHECK_PO_EXISTS = @"
            SELECT COUNT(1) FROM tbl_productionordermaster
            WHERE productionordernumber = @ProductionOrderNumber
              AND prodseriesid = @ProdSeriesId

              AND isactive = 1";

        #endregion

        #region CHECK_PRODSERIES_STARTID_OVERLAP

        // Overlap test between the new [StartIdNumber, StartIdNumber+Quantity-1] range and every
        // existing active range under the same LnItemCode + ProdSeries pair. Same ProdSeries with a
        // different LnItemCode, or same LnItemCode with a different ProdSeries, is not a collision.
        // The suggested next Start ID Number uses the same pair, reflecting where that item's own
        // range under that series left off.
        public static readonly string CHECK_PRODSERIES_STARTID_OVERLAP = @"
            SELECT
                CASE WHEN EXISTS (
                    SELECT 1 FROM tbl_productionordermaster
                    WHERE prodseriesid = @ProdSeriesId
                      AND lnitemcodeid = @LnItemCodeId
                      AND isactive = 1
                      AND @StartIdNumber <= (startidnumber + quantity - 1)
                      AND startidnumber <= (@StartIdNumber + @Quantity - 1)
                ) THEN 1 ELSE 0 END AS HasOverlap,
                (
                    SELECT MAX(startidnumber + quantity - 1)
                    FROM tbl_productionordermaster
                    WHERE prodseriesid = @ProdSeriesId
                      AND lnitemcodeid = @LnItemCodeId
                      AND isactive = 1
                ) AS MaxEndIdNumber";

        #endregion

        #region GET_ALL_PRODUCTION_ORDERS

        public static readonly string GET_ALL_PRODUCTION_ORDERS = @"
         
    SELECT
        id,
        productionordernumber,
        projectnumber,
        projectdescription,
        lnitemcode,
        itemdescription,
        prodseriesid,
        productionseries,
        startidnumber,
        quantity,
        drawingnumberid,
        drawingnumber,
        nomenclature,
        componenttype,
        racklocation,
        lnitemcodeid,
        createddate,
        MRIRNumber,
        PrecheckStatus,
        PrecheckStatusName,
        min,  
        buildnumber AS BuildNumber,
        snagsheetno AS SnagSheetNo,
        endidnumber AS EndIdNumber,
        ModifiedDate
    FROM vw_production_order_summary
    WHERE isactive = 1
    ORDER BY createddate DESC";

        #endregion

        #region GET_FILTERED_PRODUCTION_ORDERS

        public static readonly string GET_FILTERED_PRODUCTION_ORDERS = @"
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
    LEFT JOIN tbl_projectdetails pd ON pom.id = pd.productionordernumberid AND pd.isactive = 1
    LEFT JOIN tbl_projectprecheckdetails ppd 
        ON pd.id = ppd.projectdetailsid 
        AND ppd.isactive = 1
    WHERE pom.isactive = 1
    GROUP BY pom.id, pom.min
)
            SELECT 
                pom.id,
                pom.productionordernumber,
                pom.projectnumber,
                pom.projectdescription,
                pom.lnitemcode,
                pom.itemdescription,
                pom.prodseriesid,
                ps.productionseries,
                pom.startidnumber,
                pom.quantity,
                pom.drawingnumberid,
                dn.drawingnumber,
                nom.nomenclature,
                ct.componenttype,
                sl.racklocation,
                pom.lnitemcodeid,
                pom.createddate,
                pom.mrirnumber AS MRIRNumber,
                pom.min As Min,
                pom.buildnumber AS BuildNumber,
                pom.snagsheetno AS SnagSheetNo,
                pom.endidnumber AS EndIdNumber,
                psc.CalculatedStatus AS PrecheckStatus,
                CASE
                    WHEN psc.CalculatedStatus = 1 THEN 'Pending'
                    WHEN psc.CalculatedStatus = 2 THEN 'Partial'
                    WHEN psc.CalculatedStatus = 3 THEN 'Completed'
                    WHEN psc.CalculatedStatus = 4 THEN 'Pending-Planner'
                    ELSE 'Unknown'
                END AS PrecheckStatusName,
                pom.modifieddate        AS ModifiedDate,       -- PO master modified date
                psc.LastModifiedDate    AS LastModifiedDate
            FROM tbl_productionordermaster pom
            LEFT JOIN PrecheckStatusCalc psc ON pom.id = psc.productionordernumberid
            LEFT JOIN tbl_productionseries ps ON pom.prodseriesid = ps.id
            LEFT JOIN tbl_drawingnumber dn ON pom.drawingnumberid = dn.id
            LEFT JOIN (
    SELECT 
        dnm.drawingnumberid,
        STRING_AGG(nom.nomenclature, ', ') AS nomenclature
    FROM tbl_drawingnomenclaturemapping dnm
    JOIN tbl_nomenclature nom 
        ON dnm.nomenclatureid = nom.id
    WHERE dnm.isactive = 1 
      AND nom.isactive = 1
    GROUP BY dnm.drawingnumberid
) nom 
    ON dn.id = nom.drawingnumberid
-- 🔹 Aggregated Component Type
LEFT JOIN (
    SELECT 
        dctm.drawingnumberid,
        STRING_AGG(ct.componenttype, ', ') AS componenttype
    FROM tbl_drawingcomponenttypemapping dctm
    JOIN tbl_componenttype ct 
        ON dctm.componenttypeid = ct.id
    WHERE dctm.isactive = 1 
      AND ct.isactive = 1
    GROUP BY dctm.drawingnumberid
) ct 
    ON dn.id = ct.drawingnumberid
-- 🔹 Aggregated Rack Location
LEFT JOIN (
    SELECT 
        dlm.drawingnumberid,
        STRING_AGG(sl.racklocation, ', ') AS racklocation
    FROM tbl_drawingnlnitemlocationmapping dlm
    JOIN tbl_storeitemlocation sl 
        ON dlm.racklocationid = sl.id
    WHERE dlm.isactive = 1 
      AND sl.isactive = 1
    GROUP BY dlm.drawingnumberid
) sl 
    ON dn.id = sl.drawingnumberid
            WHERE pom.isactive = 1
            {DATE_FILTER}
            {STATUS_FILTER}
            {PO_FILTER}
            {LNITEM_FILTER}
            {DRAWING_FILTER}       
            ORDER BY pom.createddate DESC";

        #endregion

        #region GET_PRODUCTION_ORDER_COUNTS

        public static readonly string GET_PRODUCTION_ORDER_COUNTS = @"
           WITH PrecheckStatusCalc AS (
    SELECT 
        pom.id AS productionordernumberid,
        CASE 
            WHEN pom.min IS NULL OR LTRIM(RTRIM(pom.min)) = '' THEN 4 
            WHEN COUNT(ppd.id) = SUM(CASE WHEN ppd.isprecheckcomplete = 1 THEN 1 ELSE 0 END) 
                 AND COUNT(ppd.id) > 0 THEN 3
            WHEN SUM(CASE WHEN ppd.isprecheckcomplete = 1 THEN 1 ELSE 0 END) > 0 THEN 2
            ELSE 1
        END AS CalculatedStatus
    FROM tbl_productionordermaster pom
    LEFT JOIN tbl_projectdetails pd ON pom.id = pd.productionordernumberid AND pd.isactive = 1
    LEFT JOIN tbl_projectprecheckdetails ppd 
        ON pd.id = ppd.projectdetailsid 
        AND ppd.isactive = 1
    WHERE pom.isactive = 1
    GROUP BY pom.id, pom.min
)
            SELECT 
                COUNT(pom.id) AS TotalCount,
                SUM(CASE WHEN psc.CalculatedStatus = 3 THEN 1 ELSE 0 END) AS CompletedCount,
                SUM(CASE WHEN psc.CalculatedStatus = 2 THEN 1 ELSE 0 END) AS PartialCount,
                SUM(CASE WHEN psc.CalculatedStatus = 1 THEN 1 ELSE 0 END) AS PendingCount,
                SUM(CASE WHEN psc.CalculatedStatus = 4 THEN 1 ELSE 0 END) AS UploadedCount
            FROM tbl_productionordermaster pom
            LEFT JOIN PrecheckStatusCalc psc ON pom.id = psc.productionordernumberid
            WHERE pom.isactive = 1
            {DATE_FILTER}
            {STATUS_FILTER}
            {OTHER_FILTERS}
";

        #endregion

        #region UPDATE_PRODUCTION_ORDER_MASTER

        public static readonly string UPDATE_PRODUCTION_ORDER_MASTER = @"
            UPDATE tbl_productionordermaster 
            SET projectnumber = @ProjectNumber,
                projectdescription = @ProjectDescription,
                lnitemcode = @LnItemCode,
                itemdescription = @ItemDescription,
                prodseriesid = @ProdSeriesId,
                startidnumber = @StartIdNumber,
                endidnumber = (@StartIdNumber + @Quantity) - 1,
                quantity = @Quantity,
                drawingnumberid = @DrawingNumberId,
                lnitemcodeid = @LnItemCodeId,
                mrirnumber = @MRIRNumber,
                modifieddate=GetDate(),
                min = @Min,
                buildnumber = @BuildNumber,
                snagsheetno = @SnagSheetNo
            WHERE productionordernumber = @ProductionOrderNumber AND id=@Id AND isactive = 1";

        #endregion

        #region DELETE_PROJECT_DETAILS_WITH_POID

        public static readonly string DELETE_PROJECT_DETAILS_WITH_POID = @"
            UPDATE tbl_projectprecheckdetails SET isactive = 0 WHERE productionordernumberid = @ProductionOrderNumberId;
            UPDATE tbl_projectdetails SET isactive = 0 WHERE productionordernumberid = @ProductionOrderNumberId;";

        #endregion

        #region UPDATE_PO_MIN_STATUS

        public static readonly string UPDATE_PO_MIN_STATUS = @"
            UPDATE tbl_productionordermaster 
            SET min = @Min, status = @Status 
            WHERE productionordernumber = @ProductionOrderNumber AND isactive = 1";

        #endregion


        #region Delete PO

        public static readonly string DELETE_PRECHECK_DETAILS = @"
    UPDATE tbl_projectprecheckdetails
    SET isactive = 0,
        modifieddate = GETDATE()
    WHERE projectdetailsid IN (
        SELECT id
        FROM tbl_projectdetails
        WHERE productionordernumber = @ProductionOrderNumber
          AND idnumbers IN @IdNumbers
          AND isactive = 1
    )
    AND isactive = 1;";


        public static readonly string DELETE_PROJECT_DETAILS = @"
    UPDATE tbl_projectdetails
    SET isactive = 0,
        modifieddate = GETDATE()
    WHERE productionordernumber = @ProductionOrderNumber
      AND idnumbers IN @IdNumbers
      AND isactive = 1;";


        public static readonly string DELETE_PRODUCTION_ORDER_MASTER = @"
    UPDATE tbl_productionordermaster
    SET isactive = 0
    WHERE productionordernumber = @ProductionOrderNumber
      AND startidnumber = @IdNumber
      AND isactive = 1;";
#endregion



    }
}


