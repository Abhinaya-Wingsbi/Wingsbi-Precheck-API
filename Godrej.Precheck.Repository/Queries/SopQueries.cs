using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Repository.Queries
{
    public static class SopQueries
    {
        #region Get Sop Names 

        // Get user by email and password
        public static readonly string GET_SOP_NAMES = @"
           SELECT sop.[id]
      ,sop.[drawingnumberid]
      ,sop.[sopnames]
      ,sop.[version]
     , dwg.drawingnumber
  FROM tbl_sopnames sop
  inner join tbl_drawingnumber dwg on sop.drawingnumberid = dwg.id";

        #endregion

        #region GET SOP TEMPLATE

        public static readonly string GET_SOP_TEMPLATE = @"
                    WITH ComponentHierarchy AS (
    -- Base case: Level 1
    SELECT 
        id,
        parentdrawingnumber as assembly,
        drawingnumber as drawingnumberid,
        1 as level,
        CAST(
            CONCAT(
                CAST(parentdrawingnumber AS VARCHAR(50)), 
                '->', 
                CAST(drawingnumber AS VARCHAR(50))
            ) as VARCHAR(1000)
        ) as hierarchy_path,
        quantity,
        unit
    FROM tbl_assemblydrawingmapping
    WHERE parentdrawingnumber = @assemblydrawingnumber

    UNION ALL

    -- Recursive part
    SELECT 
        m.id,
        m.parentdrawingnumber,
        m.drawingnumber as drawingnumberid ,
        h.level + 1,
        CAST(
            CONCAT(
                h.hierarchy_path, 
                '->', 
                CAST(m.drawingnumber AS VARCHAR(50))
            ) as VARCHAR(1000)
        ),
        m.quantity,
        m.unit
    FROM tbl_assemblydrawingmapping m
    INNER JOIN ComponentHierarchy h ON h.drawingnumberid = m.parentdrawingnumber
    WHERE EXISTS (
        SELECT 1 
        FROM tbl_assemblydrawingmapping sub 
        WHERE sub.parentdrawingnumber = m.drawingnumber
    )
    AND h.level < 500
)

-- Final Select with drawing numbers, nomenclature, product series and quantity
SELECT 
    ch.assembly as assembly,
    pd.drawingnumber as assembly_number,
    -- Parent Product Series
    (
        SELECT STRING_AGG(dps.availableseriesid, ', ')
        FROM tbl_drawingprodseriesmapping dps
        WHERE dps.drawingnumberid = pd.id
        AND dps.isactive = 1
    ) as assembly_product_series,
    ch.drawingnumberid ,
    cd.drawingnumber as drawing_number,
    child_nom.nomenclature as drawing_nomenclature,
    comp_type.componenttypeid as drawing_component_type_id,
    ct.componenttype as drawing_component_type_name,
    -- Drawing Product Series
    (
        SELECT STRING_AGG(dps.availableseriesid, ', ')
        FROM tbl_drawingprodseriesmapping dps
        WHERE dps.drawingnumberid = cd.id
        AND dps.isactive = 1
    ) as drawing_product_series,
    ch.level,
    ch.hierarchy_path as id_hierarchy_path,
    ch.quantity,
    ch.unit
FROM ComponentHierarchy ch
LEFT JOIN tbl_drawingnumber pd 
    ON ch.assembly = pd.id
LEFT JOIN tbl_drawingnumber cd 
    ON ch.drawingnumberid = cd.id
-- Join for drawing nomenclature
LEFT JOIN tbl_drawingnomenclaturemapping child_nom_map 
    ON child_nom_map.drawingnumberid = cd.id
LEFT JOIN tbl_nomenclature child_nom 
    ON child_nom.id = child_nom_map.nomenclatureid
-- Join for drawing component type
LEFT JOIN tbl_drawingcomponenttypemapping comp_type
    ON comp_type.drawingnumberid = cd.id
    AND comp_type.isactive = 1
-- Join for component type name
LEFT JOIN tbl_componenttype ct
    ON ct.id = comp_type.componenttypeid
ORDER BY 
    ch.level, 
    pd.drawingnumber, 
    cd.drawingnumber;";
        #endregion


        #region GET ALL SOP TEMPLATE

        public static readonly string GET_ALL_SOP_TEMPLATE = @"
                    WITH ComponentHierarchy AS (
    -- Base case: Level 1
    SELECT 
        id,
        parentdrawingnumber AS assembly,
        drawingnumber       AS drawingnumberid,
        1                   AS level,
        CAST(
            CONCAT(
                CAST(parentdrawingnumber AS VARCHAR(50)), 
                '->', 
                CAST(drawingnumber AS VARCHAR(50))
            ) AS VARCHAR(1000)
        ) AS hierarchy_path,
        quantity,
        unit,
        findno
    FROM tbl_assemblydrawingmapping
    WHERE parentdrawingnumber =  @assemblydrawingnumber

    UNION ALL

    -- Recursive part
    SELECT
        m.id,
        m.parentdrawingnumber AS assembly,
        m.drawingnumber       AS drawingnumberid,
        h.level + 1           AS level,
        CAST(
            CONCAT(
                h.hierarchy_path,
                '->',
                CAST(m.drawingnumber AS VARCHAR(50))
            ) AS VARCHAR(1000)
        ) AS hierarchy_path,
        m.quantity,
        m.unit,
        m.findno
    FROM tbl_assemblydrawingmapping m
    INNER JOIN ComponentHierarchy h 
        ON h.drawingnumberid = m.parentdrawingnumber
    -- Removed the EXISTS(...) check so we include leaf nodes as well
    WHERE h.level < 500
)

-- Final Select with drawing numbers, nomenclature, product series, and quantity
SELECT 
    ch.assembly AS assembly,
    pd.drawingnumber           AS assembly_number,
    -- Parent Product Series
    (
        SELECT STRING_AGG(dps.availableseriesid, ', ')
        FROM tbl_drawingprodseriesmapping dps
        WHERE dps.drawingnumberid = pd.id
          AND dps.isactive = 1
    ) AS assembly_product_series,
    ch.drawingnumberid,
    cd.drawingnumber           AS drawing_number,
    child_nom.nomenclature     AS drawing_nomenclature,
    comp_type.componenttypeid  AS drawing_component_type_id,
    ct.componenttype           AS drawing_component_type_name,
    -- Drawing Product Series
    (
        SELECT STRING_AGG(dps.availableseriesid, ', ')
        FROM tbl_drawingprodseriesmapping dps
        WHERE dps.drawingnumberid = cd.id
          AND dps.isactive = 1
    ) AS drawing_product_series,
    ch.level,
    ch.hierarchy_path          AS id_hierarchy_path,
    ch.quantity,
    ch.unit,
    ch.findno                  AS FindNo,
    cd.lnitemcode              AS LnItemCode
FROM ComponentHierarchy ch
LEFT JOIN tbl_drawingnumber pd
       ON ch.assembly = pd.id
LEFT JOIN tbl_drawingnumber cd 
       ON ch.drawingnumberid = cd.id
-- Join for drawing nomenclature
LEFT JOIN tbl_drawingnomenclaturemapping child_nom_map
       ON child_nom_map.drawingnumberid = cd.id
      AND child_nom_map.isactive = 1
LEFT JOIN tbl_nomenclature child_nom
       ON child_nom.id = child_nom_map.nomenclatureid
      AND child_nom.isactive = 1
-- Join for drawing component type
LEFT JOIN tbl_drawingcomponenttypemapping comp_type
       ON comp_type.drawingnumberid = cd.id
      AND comp_type.isactive = 1
-- Join for component type name
LEFT JOIN tbl_componenttype ct
       ON ct.id = comp_type.componenttypeid
ORDER BY 
    ch.level, 
    pd.drawingnumber, 
    cd.drawingnumber;
";
        #endregion

        #region GET SOP CONSUMPTION DATA

        public static readonly string GET_SOP_CONSUMPTION_DATA = @"
      SELECT [Id]
      ,[Idnumber]
      ,[irnumber]
      ,[msnnumber]
      ,[consumedindrawing]
      ,[remarks]
      ,[quantity]
      ,[unit]
      ,[componentcodeid]
      ,[srnumber]
      ,[username]
      ,[drawingnumberid]
      ,[nomenclatureid]
      ,[createdby]
      ,[createddate]
      ,[modifiedby]
      ,[modifieddate]
      ,[isactive]
      ,[prodseriesid]
      ,[consumedindrawingid]
      ,[consumedinseriesid]
      ,[consumedinId]
FROM tbl_componentdrawingconsumption
WHERE drawingnumberid IN (
    SELECT CAST(value AS INT) 
    FROM STRING_SPLIT(@drawingNumbers, ',')
);";
        #endregion


        #region GET SOP PRECHECK CONSUMPTION DATA

        public static readonly string GET_SOP_PRECHECK_CONSUMPTION_DATA = @"
        SELECT  
	   child.[id] as [ProjectPrecheckDetailsId]
	   ,child.[drawingnumberid]
	  ,child.prodseriesid
      ,child.idnumbers as [Id]
      ,child.[irnumber]
      ,child.[msnnumber]
      ,child.[mrirnumber]
      ,[remarks]
      ,child.[quantity]
      ,[unit]
      ,[componentcodeid]
      ,child.[nomenclatureid]
      ,child.[createdby]
      ,child.[createddate]
      ,child.[modifiedby]
      ,child.[modifieddate]
	  ,child.idnumber  
	  ,child.[consumedindrawing]
	  ,parent.drawingnumberid as consumedindrawingnumberId
	  ,parent.prodseriesid as consumedinprodseriesid
	  ,parent.idnumbers as consumedinid
	  ,parent.id as consumedinididentity
      ,child.consumedinproductionordernumber as ConsumedinProductionOrderNumber
      ,child.remarks
      ,qd.componenttypeid
      ,qd.productionordernumber
      ,child.componenttype
      ,pom.buildnumber AS Build
      ,pom.snagsheetno AS SnagSheetNo
      ,qd_consumed.buildnumber AS QrBuildNumber
      ,qd_consumed.qrcodenumber AS ConsumedQrCodeNumber
FROM tbl_projectprecheckdetails child
inner join tbl_projectdetails parent on child.projectdetailsid = parent.id
left join tbl_irnumber ir_child on child.irnumber = ir_child.irnumber
left join tbl_qrcodedetails qd on ir_child.id = qd.irnumberid AND child.idnumbers = qd.idnumbers AND qd.isactive = 1
left join tbl_productionordermaster pom on parent.productionordernumberid = pom.id AND pom.isactive = 1
-- The specific QR code actually consumed for this component, via the row's own qrcodeid FK - this is
-- what build number is sourced from for every non-root node (see AppendFlatChildren), instead of
-- pom.buildnumber above, which is the whole production order's build number and only applies to the root.
left join tbl_qrcodedetails qd_consumed on qd_consumed.id = child.qrcodeid
WHERE child.drawingnumberid IN (
    SELECT CAST(value AS INT) 
    FROM STRING_SPLIT(@drawingNumbers, ',')
);";
        #endregion

        #region GET ROOT SOP BUILD AND SNAG
        public static readonly string GET_ROOT_SOP_BUILD_AND_SNAG = @"
            SELECT TOP 1 pom.buildnumber AS Build, pom.snagsheetno AS SnagSheetNo
            FROM tbl_productionordermaster pom
            JOIN tbl_projectdetails pd ON pom.id = pd.productionordernumberid
            WHERE pd.idnumbers = @SerielNumberId 
              AND pd.prodseriesid = @ProdSeriesId 
              AND pd.drawingnumberid = @AssemblyDrawingId
              AND pd.isactive = 1 AND pom.isactive = 1;";
        #endregion

        #region GET SUB ASSEMBLY PROJECT ID
        // tbl_irnumber/tbl_msnnumber.productionordernumber are varchar, matching tbl_projectdetails.productionordernumber
        // directly - no cast needed there. tbl_projectdetails.idnumbers is int, so @idNumbers is bound as an int
        // parameter rather than compared as a string. Previously every one of these was wrapped in
        // CAST(... AS NVARCHAR(100)) on both sides, which made the whole query non-sargable (SQL Server can't use
        // an index once a column is wrapped in a function) - this is called once per sub-assembly encountered
        // while walking a SOP's BOM tree, so that cost was paid repeatedly per request.
        public static readonly string GET_SUB_ASSEMBLY_PROJECT_ID = @"
        SELECT TOP 1 pd.id
        FROM tbl_projectdetails pd
        LEFT JOIN tbl_irnumber ir
            ON ir.productionordernumber = pd.productionordernumber
            AND ir.irnumber = @pivotIdentifier
        LEFT JOIN tbl_msnnumber msn
            ON msn.productionordernumber = pd.productionordernumber
            AND msn.msnnumber = @pivotIdentifier
        WHERE
            (ir.id IS NOT NULL OR msn.id IS NOT NULL)
        AND pd.idnumbers = @idNumbers
        AND pd.isactive = 1;";
        #endregion
    }


}
