using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DataModel.Assembly;
 
namespace Godrej.Precheck.Repository.Queries
{
    public static class QRCodeQueries
    {
        #region INSERT_QRCODE_DETAILS_QUERY
 
        public static readonly string INSERT_QRCODE_DETAILS_QUERY = @"INSERT INTO tbl_qrcodedetails
        (
            qrcodenumber, drawingnumberid, productionseriesid, nomenclatureid,
            componenttypeid, idnumber, idnumbers, irnumberid, msnnumberid,
            refdocremarks, quantity, remainingquantity, desposition, expirydate, createdby, createddate,
            isactive, unitid, lnitemcodeid, racklocationid, productionordernumber, purchaseordernumber,
            operationno, qrcodestatusid, mrirnumber, manufacturingdate, projectdescription, projectnumber, buildnumber
        )
        OUTPUT INSERTED.id
        VALUES
        (
            @QRCodeNumber, @DrawingNumberId, @ProductionSeriesId, @NomenclatureId,
            @ComponentTypeId, @IdNumber, @Idnumbers, @IRNumberId, @MSNNumberId,
            @RefDocRemarks, @Quantity, @RemainingQuantity, @Desposition, @ExpiryDate, @CreatedBy,
            @CreatedDate, @IsActive, @UnitId, @LnItemCodeId, @RackLocationId, @ProductionOrderNumber, @PurchaseOrderNumber,
            @OperationNo, @QrcodeStatusId, @MRIRNumber, @ManufacturingDate, @ProjectDescription, @ProjectNumber, @BuildNumber
        )";
 
        public static readonly string INSERT_STANDARD_QRCODE_DETAILS_QUERY = @"
        INSERT INTO tbl_qrcodedetails
        (
            qrcodenumber, drawingnumberid, productionseriesid, nomenclatureid,
            componenttypeid, idnumber, idnumbers, irnumberid, msnnumberid,
            refdocremarks, quantity,remainingquantity, desposition, expirydate, createdby, createddate,
            isactive, unitid, lnitemcodeid, racklocationid, productionordernumber, purchaseordernumber,
            operationno, qrcodestatusid, mrirnumber, manufacturingdate, projectdescription, projectnumber,
            partno, size, shapeid, customeritemcode, material, htlotno,
            fanmannumber, fanmanserialnumber, serialnumberofquantity, msnirnumber, gfnno, srno, tqty, wc, togglecomponenttypeid)
        OUTPUT INSERTED.id
        VALUES(
            @QRCodeNumber, @DrawingNumberId, @ProductionSeriesId, @NomenclatureId,
            @ComponentTypeId, @IdNumber, @Idnumbers, @IRNumberId, @MSNNumberId,
            @RefDocRemarks, @Quantity, @RemainingQuantity, @Desposition, @ExpiryDate, @CreatedBy,
            @CreatedDate, @IsActive, @UnitId, @LnItemCodeId, @RackLocationId, @ProductionOrderNumber, @PurchaseOrderNumber,
            @OperationNo, @QrcodeStatusId, @MRIRNumber, @ManufacturingDate, @ProjectDescription, @ProjectNumber,
            @PartNo, @Size, @ShapeId, @CustomerItemCode, @Material, @HTLotNo,
            @FanManNumber, @FanManSerialNumber, @SerialNumberOfQuantity, @MsnIrNumber, @GFNNo, @SRNo, @TQty, @WC, @ToggleComponentTypeId
        )";
        #endregion
 
        #region INSERT_PRECHECK_QRCODE_DETAILS_QUERY
 
        public static readonly string INSERT_PRECHECK_QRCODE_DETAILS_QUERY = @"INSERT INTO tbl_qrcodedetails
        (
            qrcodenumber, drawingnumberid, productionseriesid, idnumbers,
            createdby, createddate, isactive, qrcodestatusid
        )
        OUTPUT INSERTED.id
        VALUES
        (
            @QRCodeNumber, @DrawingNumberId, @ProductionSeriesId, @IdNumbers,
            @CreatedBy, @CreatedDate, @IsActive, @QrcodeStatusId
        )";
        #endregion
 
        #region GET_QRCODE_DETAILS_QUERY
 
        public static readonly string GET_QRCODE_DETAILS_QUERY =
            @"SELECT
            qd.id,
            qd.drawingnumberid ,
            qd.productionseriesid,
            qd.nomenclatureid,
            qd.idnumber,
            qd.idnumbers,
            qd.irnumberid,
            qd.msnnumberid,
            qd.componenttypeid,
            qd.quantity,
            qd.expirydate,
            qd.racklocationid,
            qd.lnitemcodeid,
            qd.createdby,
            qd.createddate,
            qd.modifiedby,
            qd.modifieddate,
            td.drawingnumber,
            ct.componenttype,
            ir.irnumber,
            msn.msnnumber,
            n.nomenclature,
            ps.productionseries,
            qd.refdocremarks,
            qd.qrcodenumber AS QrCodeNumber,
            tu.username AS users,
            ts.racklocation,
            qd.quantity,
            qd.desposition,
            qd.productionordernumber,
            qd.purchaseordernumber,
            qd.operationno,
            tq.qrcodestatus,    
            qd.qrcodestatusid,
            qd.consumedIndrawing,
            qd.mrirnumber,
            qd.isactive,
            qd.manufacturingdate,
            qd.projectdescription AS Remark,
            qd.projectnumber,
            qd.partno,
            qd.[size],
            qd.shapeid,
            sh.materialname AS Shapes,
            qd.customeritemcode AS CustomerIC,
            qd.material,
            qd.htlotno,
            qd.fanmannumber AS FAN,
            qd.fanmanserialnumber AS GIC,
            qd.serialnumberofquantity AS DTD,
            qd.msnirnumber AS IRNo,
            qd.gfnno,
            qd.remainingquantity,
            qd.srno,
            qd.tqty,
            qd.togglecomponenttypeid,
            qd.unitid,
            qd.fanmannumber,
            qd.buildnumber,
            u.unitname,
            tln.lnitemcode,
                        (
                SELECT TOP 1 tdParent.drawingnumber
                FROM tbl_assemblydrawingmapping adm
                INNER JOIN tbl_drawingnumber tdParent
                    ON adm.parentdrawingnumber = tdParent.id
                WHERE adm.drawingnumber = qd.drawingnumberid
                ORDER BY adm.id ASC
            ) AS AssemblyNumber
        FROM tbl_qrcodedetails qd
        INNER JOIN tbl_drawingnumber td
            ON qd.drawingnumberid = td.id
        INNER JOIN tbl_productionseries ps
            ON qd.productionseriesid = ps.id
        LEFT JOIN tbl_componenttype ct
            ON qd.componenttypeid = ct.id
        LEFT JOIN tbl_irnumber ir
            ON qd.irnumberid = ir.id
        LEFT JOIN tbl_msnnumber msn
            ON qd.msnnumberid = msn.id
        LEFT JOIN tbl_drawingnomenclaturemapping dnm
            ON dnm.drawingnumberid = td.id
        LEFT JOIN tbl_nomenclature n
            ON dnm.nomenclatureid = n.id
                LEFT JOIN tbl_users tu
            ON qd.createdby = tu.id
        LEFT JOIN tbl_storeitemlocation ts
            ON qd.racklocationid = ts.id
        LEFT JOIN tbl_qrcodestatus tq
           ON qd.qrcodestatusid = tq.id
        LEFT JOIN tbl_drawingnlnitemlocationmapping tdlm
            ON tdlm.drawingnumberid = td.id
        LEFT JOIN tbl_lnitemcode tln
            ON tdlm.lnitemcodeid = tln.id
        LEFT JOIN tbl_unit u
            ON qd.unitid = u.id AND u.isactive = 1
        LEFT JOIN tbl_shapes sh
            ON qd.shapeid = sh.id
        WHERE qd.qrcodenumber = @qrcodenumber
        AND (
            (@qrcodestatusid = 2 AND qd.qrcodestatusid = 2)
            OR
            (ISNULL(@qrcodestatusid, 0) <> 2 AND qd.isactive = 1)
        );
    ";
 
        public static readonly string GET_STANDARD_QRCODE_DETAILS_QUERY = @"
            SELECT
            qd.drawingnumberid ,
            qd.productionseriesid,
            qd.nomenclatureid,
            qd.idnumber,
            qd.idnumbers,
            qd.irnumberid,
            qd.msnnumberid,
            qd.componenttypeid,
            qd.quantity,
            qd.expirydate,
            qd.racklocationid,
            qd.lnitemcodeid,
            qd.createdby,
            qd.createddate,
            qd.modifiedby,
            qd.modifieddate,
            td.drawingnumber,
            ct.componenttype,
            ir.irnumber,
            msn.msnnumber,
            n.nomenclature,
            ps.productionseries,
            qd.refdocremarks,
            qd.qrcodenumber,
            tu.username AS users,
            ts.racklocation,
            qd.quantity,
            qd.desposition,
            qd.productionordernumber,
            qd.purchaseordernumber,
            qd.operationno,
            tq.qrcodestatus,    
            qd.qrcodestatusid,
            qd.consumedIndrawing,
            qd.mrirnumber,
            qd.manufacturingdate,
            qd.projectdescription AS ProjectDescription,
            qd.projectnumber,
            qd.partno,
            qd.size,
            qd.shapeid,
            sh.materialname AS Shapes,
            qd.customeritemcode AS CustomerItemCode,
            qd.material,
            qd.htlotno,    
            qd.fanmannumber AS FanManNumber,
            qd.fanmanserialnumber AS FanManSerialNumber,
            qd.serialnumberofquantity AS SerialNumberOfQuantity,
            qd.msnirnumber AS MsnIrNumber,
            qd.gfnno,
            qd.srno,
            qd.tqty,
            qd.wc,
            tln.lnitemcode,
            tan.assemblynumber,
            qd.togglecomponenttypeid AS ToggleComponentTypeId,
            qd.storeindate AS StoreInDate,
            qd.unitid,
            u.unitname AS UnitName
        FROM tbl_qrcodedetails qd
        INNER JOIN tbl_drawingnumber td
            ON qd.drawingnumberid = td.id
        INNER JOIN tbl_productionseries ps
            ON qd.productionseriesid = ps.id
        LEFT JOIN tbl_componenttype ct
            ON qd.componenttypeid = ct.id
        LEFT JOIN tbl_irnumber ir
            ON qd.irnumberid = ir.id
        LEFT JOIN tbl_msnnumber msn
            ON qd.msnnumberid = msn.id
        LEFT JOIN tbl_drawingnomenclaturemapping dnm
            ON dnm.drawingnumberid = td.id
        LEFT JOIN tbl_nomenclature n
            ON dnm.nomenclatureid = n.id
                LEFT JOIN tbl_users tu
            ON qd.createdby = tu.id
        LEFT JOIN tbl_storeitemlocation ts
            ON qd.racklocationid = ts.id
        LEFT JOIN tbl_qrcodestatus tq
           ON qd.qrcodestatusid = tq.id
        LEFT JOIN tbl_drawingnlnitemlocationmapping tdlm
            ON tdlm.drawingnumberid = td.id
        LEFT JOIN tbl_lnitemcode tln
            ON tdlm.lnitemcodeid = tln.id
        LEFT JOIN tbl_assemblydrawingmapping tadm
            ON tadm.drawingnumber = td.id
        LEFT JOIN tbl_assemblynumber tan
            ON tadm.assemblynumber = tan.id
        LEFT JOIN tbl_shapes sh
            ON qd.shapeid = sh.id
        LEFT JOIN tbl_unit u
            ON qd.unitid = u.id AND u.isactive = 1
        WHERE qd.qrcodenumber = @qrcodenumber";
 
        public static readonly string GET_QRCODE_DETAILS_With_PARAMETER_QUERY =
    @"SELECT DISTINCT
        qd.id,
        qd.drawingnumberid,
        qd.productionseriesid,
        qd.nomenclatureid,
        qd.idnumber,
        qd.idnumbers,
        qd.irnumberid,
        qd.msnnumberid,
        qd.componenttypeid,
        qd.quantity,
        qd.expirydate,
        qd.racklocationid,
        qd.lnitemcodeid,
        qd.createdby,
        qd.createddate,
        qd.modifiedby,
        qd.modifieddate,
        qd.mydate,
        qd.sopnamesid,
        qd.unitid,
        qd.storeindate,
        qd.isactive,
        qd.partno,
        qd.[size],
        qd.shapeid,
        sh.materialname AS Shapes,
        qd.customeritemcode AS CustomerIC,
        qd.material,
        qd.htlotno,
        qd.fanmannumber AS FAN,
        qd.fanmanserialnumber AS GIC,
        qd.serialnumberofquantity AS DTD,
        qd.msnirnumber AS IRNo,
        qd.gfnno,
        qd.srno,
        qd.tqty,
        qd.wc,
        qd.togglecomponenttypeid,
        td.drawingnumber,
        ct.componenttype,
        ir.irnumber,
        msn.msnnumber,
        (SELECT TOP 1 n2.nomenclature
         FROM tbl_drawingnomenclaturemapping dnm2
         INNER JOIN tbl_nomenclature n2 ON dnm2.nomenclatureid = n2.id
         WHERE dnm2.drawingnumberid = td.id AND dnm2.isactive = 1
         ORDER BY dnm2.createddate DESC) AS nomenclature,
        ps.productionseries,
        qd.refdocremarks,
        qd.qrcodenumber,
        tu.username AS users,
        ts.racklocation,
        qd.desposition,
        qd.productionordernumber,
        qd.purchaseordernumber,
        qd.operationno,
        tq.qrcodestatus,
        qd.qrcodestatusid,
        qd.consumedIndrawing,
        qd.mrirnumber,
        qd.remainingquantity,
        qd.manufacturingdate,
        qd.projectdescription AS Remark,
        qd.projectnumber,
        (SELECT TOP 1 tln2.lnitemcode
         FROM tbl_drawingnlnitemlocationmapping tdlm2
         INNER JOIN tbl_lnitemcode tln2 ON tdlm2.lnitemcodeid = tln2.id
         WHERE tdlm2.drawingnumberid = td.id AND tdlm2.isactive = 1
         ORDER BY tdlm2.createddate DESC) AS lnitemcode,
        (
            SELECT TOP 1 tdParent.drawingnumber
            FROM tbl_assemblydrawingmapping adm
            INNER JOIN tbl_drawingnumber tdParent
                ON adm.parentdrawingnumber = tdParent.id
            WHERE adm.drawingnumber = qd.drawingnumberid
            ORDER BY adm.id ASC
        ) AS AssemblyNumber,
        u.unitname AS unitname,
        sop.sopnames AS sopnames
    FROM tbl_qrcodedetails qd
    INNER JOIN tbl_drawingnumber td
        ON qd.drawingnumberid = td.id
    INNER JOIN tbl_productionseries ps
        ON qd.productionseriesid = ps.id
    LEFT JOIN tbl_componenttype ct
        ON qd.componenttypeid = ct.id
    LEFT JOIN tbl_irnumber ir
        ON qd.irnumberid = ir.id
    LEFT JOIN tbl_msnnumber msn
        ON qd.msnnumberid = msn.id
    LEFT JOIN tbl_users tu
        ON qd.createdby = tu.id
    LEFT JOIN tbl_storeitemlocation ts
        ON qd.racklocationid = ts.id
    LEFT JOIN tbl_qrcodestatus tq
        ON qd.qrcodestatusid = tq.id
    LEFT JOIN tbl_unit u
        ON qd.unitid = u.id
    LEFT JOIN tbl_sopnames sop
        ON qd.sopnamesid = sop.id
    LEFT JOIN tbl_shapes sh
        ON qd.shapeid = sh.id
    WHERE
        qd.isactive = 1
        AND
        (
            (
                @qrcodenumber IS NOT NULL
                AND qd.qrcodenumber = @qrcodenumber
            )
            OR
            (
                @qrcodenumber IS NULL
                AND (@prodseriesid IS NULL OR qd.productionseriesid = @prodseriesid)
                AND (@drawingid IS NULL OR qd.drawingnumberid = @drawingid)
                AND (@createdby IS NULL OR qd.createdby = @createdby)
                AND (@lnitemcodeid IS NULL OR qd.lnitemcodeid = @lnitemcodeid)
                AND (@fromdate IS NULL OR CAST(qd.createddate AS DATE) >= CAST(@fromdate AS DATE))
                AND (@todate IS NULL OR CAST(qd.createddate AS DATE) <= CAST(@todate AS DATE))
                AND (@productionordernumber IS NULL OR qd.productionordernumber = @productionordernumber)
                AND (@fanmannumber IS NULL OR qd.fanmannumber = @fanmannumber)
               AND (
                    @frombatchid IS NULL
                    OR
                    (
                        -- Prefix must match
                        LEFT(qd.idnumber, LEN(qd.idnumber) - (PATINDEX('%[^0-9]%', REVERSE(qd.idnumber) + 'X') - 1))
                        =
                        LEFT(@frombatchid, LEN(@frombatchid) - (PATINDEX('%[^0-9]%', REVERSE(@frombatchid) + 'X') - 1))

                        AND

                        -- Numeric part >= from
                        TRY_CAST(
                            RIGHT(qd.idnumber, PATINDEX('%[^0-9]%', REVERSE(qd.idnumber) + 'X') - 1)
                            AS INT
                        ) >= TRY_CAST(
                            RIGHT(@frombatchid, PATINDEX('%[^0-9]%', REVERSE(@frombatchid) + 'X') - 1)
                            AS INT
                        )
                    )
                )
                AND (
                    @tobatchid IS NULL
                    OR
                    (
                        -- Prefix must match
                        LEFT(qd.idnumber, LEN(qd.idnumber) - (PATINDEX('%[^0-9]%', REVERSE(qd.idnumber) + 'X') - 1))
                        =
                        LEFT(@tobatchid, LEN(@tobatchid) - (PATINDEX('%[^0-9]%', REVERSE(@tobatchid) + 'X') - 1))

                        AND

                        -- Numeric part <= to
                        TRY_CAST(
                            RIGHT(qd.idnumber, PATINDEX('%[^0-9]%', REVERSE(qd.idnumber) + 'X') - 1)
                            AS INT
                        ) <= TRY_CAST(
                            RIGHT(@tobatchid, PATINDEX('%[^0-9]%', REVERSE(@tobatchid) + 'X') - 1)
                            AS INT
                        )
                    )
                )
                AND (
                    @frombatchid IS NULL
                    OR @tobatchid IS NULL
                    OR
                    LEFT(@frombatchid, LEN(@frombatchid) - (PATINDEX('%[^0-9]%', REVERSE(@frombatchid) + 'X') - 1))
                    =
                    LEFT(@tobatchid, LEN(@tobatchid) - (PATINDEX('%[^0-9]%', REVERSE(@tobatchid) + 'X') - 1))
                )
           )
        )
    ORDER BY qd.createddate DESC;";
        #endregion

        #region GET_CONSUMED_QRCODE_DETAILS_With_PARAMETER_QUERY
        public static readonly string GET_CONSUMED_QRCODE_DETAILS_With_PARAMETER_QUERY =
    @"SELECT DISTINCT
        qd.id,
        qd.drawingnumberid,
        qd.productionseriesid,
        qd.nomenclatureid,
        qd.idnumber,
        qd.idnumbers,
        qd.irnumberid,
        qd.msnnumberid,
        qd.componenttypeid,
        qd.quantity,
        qd.expirydate,
        qd.racklocationid,
        qd.lnitemcodeid,
        qd.createdby,
        qd.createddate,
        qd.modifiedby,
        qd.modifieddate,
        qd.mydate,
        qd.sopnamesid,
        qd.unitid,
        qd.storeindate,
        qd.isactive,
        qd.partno,
        qd.[size],
        qd.shapeid,
        sh.materialname AS Shapes,
        qd.customeritemcode AS CustomerIC,
        qd.material,
        qd.htlotno,
        qd.fanmannumber AS FAN,
        qd.fanmanserialnumber AS GIC,
        qd.serialnumberofquantity AS DTD,
        qd.msnirnumber AS IRNo,
        qd.gfnno,
        qd.srno,
        qd.tqty,
        qd.wc,
        qd.remainingquantity,
        qd.togglecomponenttypeid,
        td.drawingnumber,
        ct.componenttype,
        ir.irnumber,
        msn.msnnumber,
        (SELECT TOP 1 n2.nomenclature
         FROM tbl_drawingnomenclaturemapping dnm2
         INNER JOIN tbl_nomenclature n2 ON dnm2.nomenclatureid = n2.id
         WHERE dnm2.drawingnumberid = td.id AND dnm2.isactive = 1
         ORDER BY dnm2.createddate DESC) AS nomenclature,
        ps.productionseries,
        qd.refdocremarks,
        qd.qrcodenumber,
        tu.username AS users,
        ts.racklocation,
        qd.desposition,
        qd.productionordernumber,
        qd.purchaseordernumber,
        qd.operationno,
        tq.qrcodestatus,
        qd.qrcodestatusid,
        qd.consumedIndrawing,
        qd.mrirnumber,
        qd.manufacturingdate,
        qd.projectdescription AS Remark,
        qd.projectnumber,
        (SELECT TOP 1 tln2.lnitemcode
         FROM tbl_drawingnlnitemlocationmapping tdlm2
         INNER JOIN tbl_lnitemcode tln2 ON tdlm2.lnitemcodeid = tln2.id
         WHERE tdlm2.drawingnumberid = td.id AND tdlm2.isactive = 1
         ORDER BY tdlm2.createddate DESC) AS lnitemcode,
        (
            SELECT TOP 1 tdParent.drawingnumber
            FROM tbl_assemblydrawingmapping adm
            INNER JOIN tbl_drawingnumber tdParent
                ON adm.parentdrawingnumber = tdParent.id
            WHERE adm.drawingnumber = qd.drawingnumberid
            ORDER BY adm.id ASC
        ) AS AssemblyNumber,
        u.unitname AS unitname,
        sop.sopnames AS sopnames
    FROM tbl_qrcodedetails qd
    INNER JOIN tbl_drawingnumber td
        ON qd.drawingnumberid = td.id
    INNER JOIN tbl_productionseries ps
        ON qd.productionseriesid = ps.id
    LEFT JOIN tbl_componenttype ct
        ON qd.componenttypeid = ct.id
    LEFT JOIN tbl_irnumber ir
        ON qd.irnumberid = ir.id
    LEFT JOIN tbl_msnnumber msn
        ON qd.msnnumberid = msn.id
    LEFT JOIN tbl_users tu
        ON qd.createdby = tu.id
    LEFT JOIN tbl_storeitemlocation ts
        ON qd.racklocationid = ts.id
    LEFT JOIN tbl_qrcodestatus tq
        ON qd.qrcodestatusid = tq.id
    LEFT JOIN tbl_unit u
        ON qd.unitid = u.id
    LEFT JOIN tbl_sopnames sop
        ON qd.sopnamesid = sop.id
    LEFT JOIN tbl_shapes sh
        ON qd.shapeid = sh.id
    WHERE
        qd.qrcodestatusid = 2
        AND qd.isactive = 0
        AND
        (
            (
                @qrcodenumber IS NOT NULL
                AND qd.qrcodenumber = @qrcodenumber
            )
            OR
            (
                @qrcodenumber IS NULL
                AND (@prodseriesid IS NULL OR qd.productionseriesid = @prodseriesid)
                AND (@drawingid IS NULL OR qd.drawingnumberid = @drawingid)
                AND (@createdby IS NULL OR qd.createdby = @createdby)
                AND (@lnitemcodeid IS NULL OR qd.lnitemcodeid = @lnitemcodeid)
                AND (@fromdate IS NULL OR CAST(qd.createddate AS DATE) >= CAST(@fromdate AS DATE))
                AND (@todate IS NULL OR CAST(qd.createddate AS DATE) <= CAST(@todate AS DATE))
                AND (@productionordernumber IS NULL OR qd.productionordernumber = @productionordernumber)
                AND (@fanmannumber IS NULL OR qd.fanmannumber = @fanmannumber)
               AND (
                    @frombatchid IS NULL
                    OR
                    (
                        -- Prefix must match
                        LEFT(qd.idnumber, LEN(qd.idnumber) - (PATINDEX('%[^0-9]%', REVERSE(qd.idnumber) + 'X') - 1))
                        =
                        LEFT(@frombatchid, LEN(@frombatchid) - (PATINDEX('%[^0-9]%', REVERSE(@frombatchid) + 'X') - 1))

                        AND

                        -- Numeric part >= from
                        TRY_CAST(
                            RIGHT(qd.idnumber, PATINDEX('%[^0-9]%', REVERSE(qd.idnumber) + 'X') - 1)
                            AS INT
                        ) >= TRY_CAST(
                            RIGHT(@frombatchid, PATINDEX('%[^0-9]%', REVERSE(@frombatchid) + 'X') - 1)
                            AS INT
                        )
                    )
                )
                AND (
                    @tobatchid IS NULL
                    OR
                    (
                        -- Prefix must match
                        LEFT(qd.idnumber, LEN(qd.idnumber) - (PATINDEX('%[^0-9]%', REVERSE(qd.idnumber) + 'X') - 1))
                        =
                        LEFT(@tobatchid, LEN(@tobatchid) - (PATINDEX('%[^0-9]%', REVERSE(@tobatchid) + 'X') - 1))

                        AND

                        -- Numeric part <= to
                        TRY_CAST(
                            RIGHT(qd.idnumber, PATINDEX('%[^0-9]%', REVERSE(qd.idnumber) + 'X') - 1)
                            AS INT
                        ) <= TRY_CAST(
                            RIGHT(@tobatchid, PATINDEX('%[^0-9]%', REVERSE(@tobatchid) + 'X') - 1)
                            AS INT
                        )
                    )
                )
                AND (
                    @frombatchid IS NULL
                    OR @tobatchid IS NULL
                    OR
                    LEFT(@frombatchid, LEN(@frombatchid) - (PATINDEX('%[^0-9]%', REVERSE(@frombatchid) + 'X') - 1))
                    =
                    LEFT(@tobatchid, LEN(@tobatchid) - (PATINDEX('%[^0-9]%', REVERSE(@tobatchid) + 'X') - 1))
                )
           )
        )
    ORDER BY qd.createddate DESC;";
        #endregion

        #region GET_DISTINCT_BATCH_ID_NUMBERS_QUERY
        public static readonly string GET_DISTINCT_BATCH_ID_NUMBERS_QUERY = @"
            SELECT DISTINCT qd.idnumber
            FROM tbl_qrcodedetails qd
            WHERE qd.isactive = 1
            AND qd.qrcodestatusid!=2                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         
            ORDER BY qd.idnumber ASC";
        #endregion

        #region GET_FANMAN_SERIAL_NUMBERS_QUERY
        public static readonly string GET_FANMAN_SERIAL_NUMBERS_QUERY = @"
            SELECT DISTINCT qd.fanmannumber
            FROM tbl_qrcodedetails qd
            WHERE qd.isactive = 1
            AND qd.fanmannumber IS NOT NULL
            AND LTRIM(RTRIM(qd.fanmannumber)) <> ''
            ORDER BY qd.fanmannumber ASC";
        #endregion

        #region GET_CONSUMEDIN_QUERY
 
        public static readonly string GET_CONSUMEDIN_QUERY =
    @"
    SELECT
        tp.idnumber,
        tp.irnumber,
        tp.msnnumber,
        tp.consumedindrawing,
        tp.consumedinproductionordernumber,
        tp.lnitemcode,
        tp.lnitemcodeid,
        tu.username,  
        tp.quantity,
        tp.isrejected AS IsRejected,
        tp.remarks AS RejectionReason,
        tp.modifieddate AS date
    FROM tbl_projectprecheckdetails tp
    INNER JOIN tbl_users tu
        ON tp.modifiedby = tu.id
    INNER JOIN tbl_projectdetails pd
        ON tp.projectdetailsid = pd.id
    INNER JOIN tbl_drawingnumber adn
        ON pd.drawingnumberid = adn.id
    WHERE
        (@productionseriesid IS NULL OR tp.prodseriesid = @productionseriesid)
        AND (@idnumber IS NULL OR pd.idnumbers = @idnumber)
        AND (@drawingnumberid IS NULL OR tp.drawingnumberid = @drawingnumberid)
        AND (@assemblynumber IS NULL OR adn.drawingnumber = @assemblynumber)
        AND tp.isactive = 1
        AND tp.isprecheckcomplete=1
    ORDER BY tp.modifieddate DESC
    ";
        #endregion
 
        #region VALIDATE_QRCODE_DETAILS_QUERY
        public static readonly string VALIDATE_QRCODE_DETAILS_QUERY =
            @"SELECT
            qd.drawingnumberid ,
            qd.productionseriesid,
            qd.nomenclatureid,
            qd.idnumber,
            qd.idnumbers,
            qd.irnumberid,
            qd.msnnumberid,
            qd.componenttypeid,
            qd.quantity,
            qd.expirydate,
            qd.racklocationid,
            qd.lnitemcodeid,
            qd.createdby,
            qd.createddate,
            qd.modifiedby,
            qd.modifieddate,
            td.drawingnumber,
            ct.componenttype,
            ir.irnumber,
            msn.msnnumber,
            n.nomenclature,
            ps.productionseries,
            qd.refdocremarks,
            qd.qrcodenumber,
            tu.username AS users,
            ts.racklocation,
            qd.quantity,
            qd.desposition,
            qd.users,
            qd.productionordernumber,
            qd.purchaseordernumber,
            qd.operationno,
            tq.qrcodestatus,    
            qd.qrcodestatusid,
            qd.consumedIndrawing,
            qd.mrirnumber
        FROM tbl_qrcodedetails qd
        INNER JOIN tbl_drawingnumber td
            ON qd.drawingnumberid = td.id
        INNER JOIN tbl_productionseries ps
            ON qd.productionseriesid = ps.id
        LEFT JOIN tbl_componenttype ct
            ON qd.componenttypeid = ct.id
        LEFT JOIN tbl_irnumber ir
            ON qd.irnumberid = ir.id
        LEFT JOIN tbl_msnnumber msn
            ON qd.msnnumberid = msn.id
        LEFT JOIN tbl_drawingnomenclaturemapping dnm
            ON dnm.drawingnumberid = td.id
        LEFT JOIN tbl_nomenclature n
            ON dnm.nomenclatureid = n.id-- Changed from qd.nomenclatureid to dnm.nomenclatureid
                LEFT JOIN tbl_users tu
            ON qd.createdby = tu.id
        LEFT JOIN tbl_storeitemlocation ts
            ON qd.racklocationid = ts.id
        LEFT JOIN tbl_qrcodestatus tq
           ON qd.qrcodestatusid = tq.id
        WHERE
            qd.productionseriesid=@ProductionSeriesId AND
            qd.idnumbers=@Idnumbers AND
            qd.drawingnumberid=@DrawingNumberId AND
            (@ProductionOrderNumber IS NULL OR qd.productionordernumber=@ProductionOrderNumber) AND
            qd.isactive=1";
        #endregion
 
        #region GET_ACTIVE_QRCODE_DETAILS_QUERY
 
        public static readonly string GET_ACTIVE_QRCODE_DETAILS_QUERY =
            @"SELECT
            qd.drawingnumberid ,
            qd.productionseriesid,
            qd.nomenclatureid,
            qd.idnumber,
            qd.idnumbers,
            qd.irnumberid,
            qd.msnnumberid,
            qd.componenttypeid,
            qd.quantity,
            qd.expirydate,
            qd.racklocationid,
            qd.lnitemcodeid,
            qd.createdby,
            qd.createddate,
            qd.modifiedby,
            qd.modifieddate,
            td.drawingnumber,
            ct.componenttype,
            ir.irnumber,
            msn.msnnumber,
            n.nomenclature,
            ps.productionseries,
            qd.refdocremarks,
            qd.qrcodenumber,
            tu.username AS users,
            ts.racklocation,
            qd.quantity,
            qd.desposition,
            qd.users,
            qd.productionordernumber,
            qd.purchaseordernumber,
            qd.operationno,
            tq.qrcodestatus,    
            qd.qrcodestatusid,
            qd.consumedIndrawing
        FROM tbl_qrcodedetails qd
        INNER JOIN tbl_drawingnumber td
            ON qd.drawingnumberid = td.id
        INNER JOIN tbl_productionseries ps
            ON qd.productionseriesid = ps.id
        LEFT JOIN tbl_componenttype ct
            ON qd.componenttypeid = ct.id
        LEFT JOIN tbl_irnumber ir
            ON qd.irnumberid = ir.id
        LEFT JOIN tbl_msnnumber msn
            ON qd.msnnumberid = msn.id
        LEFT JOIN tbl_drawingnomenclaturemapping dnm
            ON dnm.drawingnumberid = td.id
        LEFT JOIN tbl_nomenclature n
            ON dnm.nomenclatureid = n.id-- Changed from qd.nomenclatureid to dnm.nomenclatureid
                LEFT JOIN tbl_users tu
            ON qd.createdby = tu.id
        LEFT JOIN tbl_storeitemlocation ts
            ON qd.racklocationid = ts.id
        LEFT JOIN tbl_qrcodestatus tq
           ON qd.qrcodestatusid = tq.id
        WHERE qd.qrcodenumber = @qrcodenumber
    AND qd.isactive = 1";
        #endregion
 
        #region INSERT_QRCODE_IN_CONSUMPTION_QUERY
 
        public static readonly string INSERT_QRCODE_IN_CONSUMPTION_QUERY = @"
        INSERT INTO tbl_componentdrawingconsumption
        (
            [irnumber],
            [msnnumber],
            [componentcodeid],
            [srnumber],
            [idnumber],
            [drawingnumberid],
            [nomenclatureid],
            [createdby],
            [createddate],
            [prodseriesid],
            [qrcodenumber],
            [productionordernumber],
            [isactive]
        )
        VALUES
        (
            @irnumber,
            @msnnumber,
            @componentcodeid,
            @srnumber,
            @idnumber,
            @drawingnumberid,
            @nomenclatureid,
            @createdby,
            @createddate,
            @prodseriesid,
            @qrcodenumber,
            @productionordernumber,
            @isactive
        )";
 
        public static readonly string INSERT_STANDARD_QRCODE_IN_CONSUMPTION_QUERY = @"
        INSERT INTO tbl_componentdrawingconsumption
        (
            [irnumber],
            [msnnumber],
            [componentcodeid],
            [srnumber],
            [idnumber],
            [drawingnumberid],
            [nomenclatureid],
            [createdby],
            [createddate],
            [prodseriesid],
            [qrcodenumber],
            [productionordernumber],
            [isactive]
        )
        VALUES
        (
            @irnumber,
            @msnnumber,
            @componentcodeid,
            @srnumber,
            @idnumber,
            @drawingnumberid,
            @nomenclatureid,
            @createdby,
            @createddate,
            @prodseriesid,
            @qrcodenumber,
            @productionordernumber,
            @isactive
        )";
        #endregion

        #region UPDATE QR CODE
        public static readonly string UPDATE_QRCODE_QUERY = @"
    UPDATE tbl_qrcodedetails
    SET
        isactive = 0,
        qrcodestatusid = 2,
        consumedIndrawing = @ConsumedInDrawing,
        modifiedby = @ModifiedBy,
        modifieddate = @ModifiedDate
    FROM tbl_qrcodedetails qr
    INNER JOIN tbl_componenttype ct ON ct.id = qr.componenttypeid
    WHERE
        qr.qrcodenumber = @QrCodeNumber
        AND qr.isactive = 1
        AND (
            (LOWER(ct.componenttype) NOT IN ('batch', 'fim'))         -- ID/Others: always deactivate
            OR (LOWER(ct.componenttype) IN ('batch', 'fim') 
                AND qr.remainingquantity = 0)                          -- Batch/FIM: only when fully consumed
        )";
        #endregion

        #region UPDATE QR CODE STGATUS
        public static readonly string UPDATE_QRCODESTATUS_QUERY = @"
        UPDATE tbl_qrcodedetails
        SET qrcodestatusid=1,
            storeindate=@StoreInDate
        WHERE qrcodenumber = @qrcodenumber and isactive = 1 ";
 
        #endregion
 
        #region UPDATE_QRCODE_DETAILS_QUERY
        public static readonly string UPDATE_QRCODE_DETAILS_QUERY = @"
        UPDATE tbl_qrcodedetails
        SET 
            drawingnumberid = COALESCE(@DrawingNumberId, drawingnumberid),
            productionseriesid = COALESCE(@ProductionSeriesId, productionseriesid),
            nomenclatureid = COALESCE(@NomenclatureId, nomenclatureid),
            componenttypeid = COALESCE(@ComponentTypeId, componenttypeid),
            idnumber = COALESCE(@IdNumber, idnumber),
            irnumberid = COALESCE(@IrNumberId, irnumberid),
            msnnumberid = COALESCE(@MsnNumberId, msnnumberid),
            quantity = COALESCE(@Quantity, quantity),
            desposition = COALESCE(@Desposition, desposition),
            mrirnumber = COALESCE(@MRIRNumber, mrirnumber),
            productionordernumber = COALESCE(@ProductionOrderNumber, productionordernumber),
            purchaseordernumber = COALESCE(@PurchaseOrderNumber, purchaseordernumber),
            projectdescription = COALESCE(@Remarks, projectdescription),
            shapeid = COALESCE(@ShapeId, shapeid),
            unitid = COALESCE(@UnitId, unitid),
            size = COALESCE(@Size, size),
            htlotno = COALESCE(@HeatLotBatch, htlotno),
            modifiedby = @ModifiedBy,
            modifieddate = @ModifiedDate
        WHERE qrcodenumber = @QRCodeNumber AND isactive = 1";
        #endregion

        #region DISABLE_QRCODE_QUERY
        public static readonly string DISABLE_QRCODE_QUERY = @"
        UPDATE tbl_qrcodedetails
        SET
            isactive = 0,
            disableremarks = COALESCE(@Remarks, disableremarks),
            modifiedby = @ModifiedBy,
            modifieddate = GetDate()
        WHERE qrcodenumber = @QRCodeNumber AND isactive = 1";
        #endregion

        #region GET_QRCODE_DETAILS_BY_NUMBER_ANY_STATUS_QUERY
        public static readonly string GET_QRCODE_DETAILS_BY_NUMBER_ANY_STATUS_QUERY = @"
        SELECT DISTINCT
            qd.id,
            qd.qrcodenumber AS QrCodeNumber,
            tq.qrcodestatus AS QrCodeStatus,
            qd.qrcodestatusid,
            qd.productionseriesid,
            qd.assemblydrawingnumberid AS AssemblyNumberId,
            qd.lnitemcodeid AS DrawingComponentLnItemCodeId,
            qd.nomenclatureid,
            qd.componenttypeid,
            qd.idnumber,
            qd.irnumberid,
            qd.msnnumberid,
            qd.refdocremarks,
            qd.quantity,
            qd.desposition,
            qd.createdby,
            qd.createddate,
            qd.modifiedby,
            qd.modifieddate,
            qd.isactive,
            qd.drawingnumberid,
            tir.irnumber,
            tmn.msnnumber,
            tn.nomenclature,
            tcp.componenttype,
            tp.productionseries,
            td.drawingnumber,
            qd.consumedindrawing,
            qd.mrirnumber AS MRIRNumber,
            qd.idnumbers,
            qd.manufacturingdate,
            qd.projectdescription AS Remark,
            qd.projectnumber,
            ad.drawingnumber AS AssemblyNumber,
            tli.lnitemcode,
            u.unitname,
            qd.storeindate,
            qd.purchaseordernumber,
            qd.remainingquantity
        FROM tbl_qrcodedetails qd
        LEFT JOIN tbl_drawingnumber td
            ON qd.drawingnumberid = td.id
        LEFT JOIN tbl_nomenclature tn
            ON qd.nomenclatureid = tn.id
        LEFT JOIN tbl_componenttype tcp
            ON qd.componenttypeid = tcp.id
        LEFT JOIN tbl_productionseries tp
            ON qd.productionseriesid = tp.id
        LEFT JOIN tbl_irnumber tir
            ON qd.irnumberid = tir.id
        LEFT JOIN tbl_msnnumber tmn
            ON qd.msnnumberid = tmn.id
        LEFT JOIN tbl_qrcodestatus tq
            ON qd.qrcodestatusid = tq.id
        LEFT JOIN tbl_drawingnumber ad
            ON qd.assemblydrawingnumberid = ad.id
        LEFT JOIN tbl_lnitemcode tli
            ON qd.lnitemcodeid = tli.id
        LEFT JOIN tbl_unit u
            ON qd.unitid = u.id
        WHERE qd.qrcodenumber = @qrcodenumber";
        #endregion


        #region GET_QRCODE_DETAILS_BY_NUMBER_ANY_STATUS_QUERY
        public static readonly string GET_QRCODE_DETAILS_BY_NUMBER = @"
        SELECT 
            qd.id,
            qd.qrcodenumber AS QrCodeNumber
            
        FROM tbl_qrcodedetails qd
        WHERE qd.qrcodenumber = @qrcodenumber
        AND qd.isactive=1";
        #endregion

        #region VERIFY_IDNUMBER_QUERY
        public static readonly string VERIFY_IDNUMBER_QUERY =
           @"SELECT          
            qd.idnumber          
        FROM tbl_qrcodedetails qd
        WHERE qd.idnumber = @idNumber
    ";
        #endregion
 
        #region LATEST_BATCHID_NUMBER
        public static readonly string LATEST_BATCHID_NUMBER =
                 @"SELECT TOP 1 qd.IdNumber
FROM tbl_qrcodedetails qd
WHERE qd.IdNumber LIKE 'BATCH-%'
ORDER BY TRY_CAST(PARSENAME(REPLACE(qd.IdNumber, '-', '.'), 1) AS INT) DESC
";
        #endregion
 
        #region BatchildComponenet
        public static readonly string GETBATCHCHILDCOMPONENT =
                 @"SELECT 
    tdn.drawingnumber AS assemblynumber,
    tadm.parentdrawingnumber AS assemblyId,
    tadm.drawingnumber,
    tadm.quantity
FROM tbl_assemblydrawingmapping tadm
INNER JOIN tbl_drawingnumber tdn
    ON tadm.parentdrawingnumber = tdn.id
WHERE tadm.drawingnumber = @DrawingNumberId";
        #endregion
 
        #region VALIDATEMAKEORDER
        public static readonly string VALIDATEMAKEORDERQUERY=
                 @"SELECT 
                    prodseriesid,
                    productionordernumber,
                    drawingnumberid,
                    idnumbers
                FROM tbl_projectdetails
                 Where
 
                productionordernumber=@pONumber
 
                AND prodseriesid= @prodSeriesId
 
                AND drawingnumberid=@drawingId
 
                AND idnumbers = @idNumber  ";
        #endregion

        #region GETSTOREINQRCODE
        public static readonly string GETSTOREINQRCODEBYDATE =
            @"SELECT
            qd.drawingnumberid ,
            qd.productionseriesid,
            qd.nomenclatureid,
            qd.idnumber,
            qd.idnumbers,
            qd.irnumberid,
            qd.msnnumberid,
            qd.componenttypeid,
            qd.quantity,
            qd.expirydate,
            qd.racklocationid,
            qd.lnitemcodeid,
            qd.createdby,
            qd.createddate,
            qd.modifiedby,
            qd.modifieddate,
            td.drawingnumber,
            ct.componenttype,
            ir.irnumber,
            msn.msnnumber,
            n.nomenclature,
            ps.productionseries,
            qd.refdocremarks,
            qd.qrcodenumber,
            tu.username AS users,
            ts.racklocation,
            qd.quantity,
            qd.desposition,
            qd.productionordernumber,
            qd.purchaseordernumber,
            qd.operationno,
            tq.qrcodestatus,
            qd.qrcodestatusid,
            qd.consumedIndrawing,
            qd.mrirnumber,
            qd.manufacturingdate,
            qd.projectnumber,
            qd.storeindate,
            tln.lnitemcode,
            tan.assemblynumber
        FROM tbl_qrcodedetails qd
        INNER JOIN tbl_drawingnumber td
            ON qd.drawingnumberid = td.id
        INNER JOIN tbl_productionseries ps
            ON qd.productionseriesid = ps.id
        LEFT JOIN tbl_componenttype ct
            ON qd.componenttypeid = ct.id
        LEFT JOIN tbl_irnumber ir
            ON qd.irnumberid = ir.id
        LEFT JOIN tbl_msnnumber msn
            ON qd.msnnumberid = msn.id
        LEFT JOIN tbl_drawingnomenclaturemapping dnm
            ON dnm.drawingnumberid = td.id
        LEFT JOIN tbl_nomenclature n
            ON dnm.nomenclatureid = n.id
                LEFT JOIN tbl_users tu
            ON qd.createdby = tu.id
        LEFT JOIN tbl_storeitemlocation ts
            ON qd.racklocationid = ts.id
        LEFT JOIN tbl_qrcodestatus tq
           ON qd.qrcodestatusid = tq.id
        LEFT JOIN tbl_drawingnlnitemlocationmapping tdlm
            ON tdlm.drawingnumberid = td.id
        LEFT JOIN tbl_lnitemcode tln
            ON tdlm.lnitemcodeid = tln.id
        LEFT JOIN tbl_assemblydrawingmapping tadm
            ON tadm.drawingnumber = td.id
        LEFT JOIN tbl_assemblynumber tan
            ON tadm.assemblynumber = tan.id
        WHERE (@storeindate IS NULL OR CAST(qd.storeindate AS DATE) = CAST(@storeindate AS DATE))
        AND qd.isactive = 1
        AND (@drawingnumber IS NULL OR td.drawingnumber = @drawingnumber)";
        #endregion

 
        #region CHECK_PREVIOUS_BATCH_EXISTS
        public static readonly string CHECK_PREVIOUS_BATCH_EXISTS = @"
        SELECT CASE 
        WHEN EXISTS (
            SELECT 1
            FROM tbl_qrcodedetails
            WHERE drawingnumberid = @drawingNumberId
            AND idnumbers < @idNumbers
            AND remainingquantity > 0
            AND qrcodestatusid = 1
            AND isactive = 1
        )
        THEN CAST(1 AS BIT)
        ELSE CAST(0 AS BIT)
        END";
        #endregion

        #region GET_ALL_USERS_QUERY
        public static readonly string GET_ALL_USERS_QUERY = @"
            SELECT 
                u.id AS Id,
                u.username AS UserName
            FROM tbl_users u
            WHERE u.isactive = 1
              AND u.username IS NOT NULL
              AND LTRIM(RTRIM(u.username)) <> ''
              AND EXISTS (
                  SELECT 1
                  FROM tbl_qrcodedetails q
                  WHERE q.createdby = u.id
              )
            ORDER BY u.username;";
        #endregion
        public static readonly string BULK_UPDATE_QRCODE_QUERY =
            @"
            UPDATE tbl_qrcodedetails
            SET 
                mrirnumber = COALESCE(@mrirnumber, mrirnumber),
                irnumberid = COALESCE(@irnumberid, irnumberid),
                msnnumberid = COALESCE(@msnnumberid, msnnumberid),
                projectnumber = COALESCE(@projectnumber, projectnumber),
                htlotno = COALESCE(@heatlotnumber, htlotno),
                size = COALESCE(@size, size),
                lnitemcodeid = COALESCE(@lnitemcodeid, lnitemcodeid),
                drawingnumberid = COALESCE(@drawingnumberid, drawingnumberid),
                productionseriesid = COALESCE(@productionseriesid, productionseriesid),
                fanmannumber = COALESCE(@fanmannumber, fanmannumber),
                fanmanserialnumber = COALESCE(@fanmanserialnumber, fanmanserialnumber),
                racklocationid = COALESCE(@racklocationid, racklocationid),
                unitid = COALESCE(@unitid, unitid),
                idnumber = COALESCE(@idnumber, idnumber),
                quantity = COALESCE(@quantity, quantity),
                modifieddate = GETDATE()
            WHERE
                qrcodenumber IN @qrcodenumbers
                AND isactive = 1
            ";

        #region GET_AVAILABLE_QR_BY_LNITEM_DRAWING
        public static readonly string GET_AVAILABLE_QR_BY_LNITEM_DRAWING = @"
            WITH RankedQRCodes AS (
   SELECT
      q.drawingnumberid,
      d.drawingnumber,
      d.lnitemcode AS LnItemCode,
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
      qs.qrcodestatus as Status,
      u.unitname AS Unit,
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
   LEFT JOIN tbl_qrcodestatus qs
       ON q.qrcodestatusid = qs.id
   LEFT JOIN tbl_unit u
       ON q.unitid = u.id
   WHERE q.qrcodestatusid = 1
     AND q.isactive = 1
     AND (@LnItemCode IS NULL OR LTRIM(RTRIM(@LnItemCode)) = '' OR d.lnitemcode = @LnItemCode)
     AND (@DrawingNumber IS NULL OR LTRIM(RTRIM(@DrawingNumber)) = '' OR d.drawingnumber = @DrawingNumber)
     AND (@ProdSeriesId IS NULL OR q.productionseriesid = @ProdSeriesId)
     AND (
           @QrType IS NULL
        OR (@QrType = 1 AND d.lnitemcode NOT LIKE 'WJD%')
        OR (@QrType = 2 AND d.lnitemcode LIKE 'WJD%')
     )
)
SELECT * FROM RankedQRCodes
ORDER BY expirydate, manufacturingdate;
 ";
        #endregion
    }
}
