using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DataModel.Assembly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Godrej.Precheck.Repository.Queries
{
    public static class BomQueries
    {
        #region GET_RECURSIVE_BOM_BY_ASSEMBLY_NUMBER

        /// <summary>
        /// Recursively fetches the complete BOM tree for a given assembly number.
        /// Uses CTE to traverse all levels of child components.
        /// </summary>
        public static readonly string GET_RECURSIVE_BOM_BY_ASSEMBLY_NUMBER = @"
            -- First, get the assembly's drawing number ID
          DECLARE @assemblyDrawingId INT;

  SELECT @assemblyDrawingId = id FROM tbl_drawingnumber WHERE drawingnumber = @assemblyNumber;
 
  IF @assemblyDrawingId IS NULL

  BEGIN

      -- Return empty result if assembly not found

      SELECT 

          0 AS Level,

          0 AS ParentDrawingId,

          0 AS ChildDrawingId,

          '' AS ParentDrawingNumber,

          '' AS ChildDrawingNumber,

          '' AS Nomenclature,

          '' AS ComponentType,

          '' AS LnItemCode,

          0 AS Quantity,

          '' AS Unit,

          '' AS AssemblyNumber,

          '' AS IdNumber,

          '' AS IrNumber,

          '' AS MsnNumber,

          '' AS Remarks

      WHERE 1 = 0;

      RETURN;

  END
 
  ;WITH RecursiveAssembly AS (

      -- Anchor: Get direct children of the assembly

      SELECT 

          adm.drawingnumber AS child_id,

          adm.parentdrawingnumber AS parent_id,

          0 AS level,

          adm.quantity,

          adm.unit,

          adm.findno

      FROM tbl_assemblydrawingmapping adm

      WHERE adm.parentdrawingnumber = @assemblyDrawingId and adm.isactive=1
 
      UNION ALL
 
      -- Recursive: Get children of children

      SELECT 

          c.drawingnumber AS child_id,

          c.parentdrawingnumber AS parent_id,

          p.level + 1 AS level,

          c.quantity,

          c.unit,

          c.findno

      FROM tbl_assemblydrawingmapping c

      INNER JOIN RecursiveAssembly p ON c.parentdrawingnumber = p.child_id

      WHERE p.level < 50
      AND c.isactive = 1

  )

  SELECT DISTINCT

      ra.level AS Level,

      ra.parent_id AS ParentDrawingId,

      ra.child_id AS ChildDrawingId,

      pdn.drawingnumber AS ParentDrawingNumber,

      cdn.drawingnumber AS ChildDrawingNumber,

      COALESCE(n.nomenclature, '') AS Nomenclature,

      COALESCE(ct.componenttype, '') AS ComponentType,

      COALESCE(dlm.lnitemcode, '') AS LnItemCode,

      COALESCE(ra.quantity, 0) AS Quantity,

      COALESCE(ra.unit, '') AS Unit,

      COALESCE(ra.findno, '') AS FindNo,

      @assemblyNumber AS AssemblyNumber,

      '' AS IdNumber,

      '' AS IrNumber,

      '' AS MsnNumber,

      '' AS Remarks

  FROM RecursiveAssembly ra

  LEFT JOIN tbl_drawingnumber pdn ON ra.parent_id = pdn.id

  LEFT JOIN tbl_drawingnumber cdn ON ra.child_id = cdn.id

  OUTER APPLY (

      SELECT TOP 1 n.nomenclature

      FROM tbl_drawingnomenclaturemapping dnm

      INNER JOIN tbl_nomenclature n 

          ON dnm.nomenclatureid = n.id

      WHERE dnm.drawingnumberid = cdn.id

        AND dnm.isactive = 1

      ORDER BY dnm.id DESC   -- or isprimary desc

  ) n

  LEFT JOIN tbl_drawingcomponenttypemapping dctm ON cdn.id = dctm.drawingnumberid AND dctm.isactive = 1

  LEFT JOIN tbl_componenttype ct ON dctm.componenttypeid = ct.id

  LEFT JOIN tbl_drawing_lnitem_map dlm ON cdn.drawingnumber = dlm.drawingnumber AND dlm.isactive = 1

  ORDER BY ra.level, pdn.drawingnumber, cdn.drawingnumber";

        #endregion

        #region GET_ASSEMBLY_NUMBERS_SEARCH

        /// <summary>
        /// Search for assembly numbers by partial match.
        /// Returns assembly drawing numbers that have child components.
        /// </summary>
        public static readonly string GET_ASSEMBLY_NUMBERS_SEARCH = @"
            SELECT DISTINCT TOP 50
                dn.id AS Id,
                dn.drawingnumber AS DrawingNumber,
                dn.lnitemcode AS LnItemCode,
                COALESCE(n.nomenclature, '') AS Nomenclature
            FROM tbl_drawingnumber dn
            INNER JOIN tbl_assemblydrawingmapping adm ON dn.id = adm.parentdrawingnumber
            LEFT JOIN tbl_drawingnomenclaturemapping dnm ON dn.id = dnm.drawingnumberid AND dnm.isactive = 1
            LEFT JOIN tbl_nomenclature n ON dnm.nomenclatureid = n.id
            WHERE (dn.drawingnumber LIKE '%' + @searchText + '%' OR dn.lnitemcode LIKE '%' + @searchText + '%')
            AND dn.isactive = 1
            ORDER BY dn.drawingnumber";

        #endregion

        #region EXPORT_BOM_QUERY

        /// <summary>
        /// Export BOM data with all details for Excel export.
        /// </summary>
        public static readonly string EXPORT_BOM_QUERY = @"
             DECLARE @assemblyDrawingId INT;
  SELECT @assemblyDrawingId = id FROM tbl_drawingnumber WHERE drawingnumber = @assemblyNumber;

  IF @assemblyDrawingId IS NULL
  BEGIN
      -- Return empty result if assembly not found
      SELECT 
          0 AS Level,
          0 AS ParentDrawingId,
          0 AS ChildDrawingId,
          '' AS ParentDrawingNumber,
          '' AS ChildDrawingNumber,
          '' AS Nomenclature,
          '' AS ComponentType,
          '' AS LnItemCode,
          0 AS Quantity,
          '' AS Unit,
          '' AS AssemblyNumber,
          '' AS IdNumber,
          '' AS IrNumber,
          '' AS MsnNumber,
          '' AS Remarks
      WHERE 1 = 0;
      RETURN;
  END

  ;WITH RecursiveAssembly AS (
      -- Anchor: Get direct children of the assembly
      SELECT 
          adm.drawingnumber AS child_id,
          adm.parentdrawingnumber AS parent_id,
          0 AS level,
          adm.quantity,
          adm.unit,
          adm.findno
      FROM tbl_assemblydrawingmapping adm
      WHERE adm.parentdrawingnumber = @assemblyDrawingId and adm.isactive =1

      UNION ALL

      -- Recursive: Get children of children
      SELECT 
          c.drawingnumber AS child_id,
          c.parentdrawingnumber AS parent_id,
          p.level + 1 AS level,
          c.quantity,
          c.unit,
          c.findno
      FROM tbl_assemblydrawingmapping c
      INNER JOIN RecursiveAssembly p ON c.parentdrawingnumber = p.child_id
      WHERE p.level < 50
  )
  SELECT DISTINCT
      ra.level AS Level,
      ra.parent_id AS ParentDrawingId,
      ra.child_id AS ChildDrawingId,
      pdn.drawingnumber AS ParentDrawingNumber,
      cdn.drawingnumber AS ChildDrawingNumber,
      COALESCE(n.nomenclature, '') AS Nomenclature,
      COALESCE(ct.componenttype, '') AS ComponentType,
      COALESCE(dlm.lnitemcode, '') AS LnItemCode,
      COALESCE(ra.quantity, 0) AS Quantity,
      COALESCE(ra.unit, '') AS Unit,
      COALESCE(ra.findno, '') AS FindNo,
      @assemblyNumber AS AssemblyNumber,
      '' AS IdNumber,
      '' AS IrNumber,
      '' AS MsnNumber,
      '' AS Remarks
  FROM RecursiveAssembly ra
  LEFT JOIN tbl_drawingnumber pdn ON ra.parent_id = pdn.id
  LEFT JOIN tbl_drawingnumber cdn ON ra.child_id = cdn.id
  OUTER APPLY (
      SELECT TOP 1 n.nomenclature
      FROM tbl_drawingnomenclaturemapping dnm
      INNER JOIN tbl_nomenclature n 
          ON dnm.nomenclatureid = n.id
      WHERE dnm.drawingnumberid = cdn.id
        AND dnm.isactive = 1
      ORDER BY dnm.id DESC   -- or isprimary desc
  ) n
  LEFT JOIN tbl_drawingcomponenttypemapping dctm ON cdn.id = dctm.drawingnumberid AND dctm.isactive = 1
  LEFT JOIN tbl_componenttype ct ON dctm.componenttypeid = ct.id
  LEFT JOIN tbl_drawing_lnitem_map dlm ON cdn.drawingnumber = dlm.drawingnumber AND dlm.isactive = 1
  ORDER BY ra.level, pdn.drawingnumber, cdn.drawingnumber
 ";

        #endregion

        #region GET_BOM_COUNTS_BY_ASSEMBLY_NUMBERS
        public static readonly string GET_BOM_COUNTS_BY_ASSEMBLY_NUMBERS = @"
        WITH AssemblyIds AS(
    SELECT dn.id, dn.drawingnumber
    FROM tbl_drawingnumber dn
    WHERE dn.drawingnumber IN @assemblyNumbers  -- Dapper IN clause
),
RecursiveAssembly AS(
    SELECT
        ai.drawingnumber AS RootAssembly,
        adm.drawingnumber AS child_id,
        0 AS level
    FROM tbl_assemblydrawingmapping adm
    JOIN AssemblyIds ai ON adm.parentdrawingnumber = ai.id

    UNION ALL

    SELECT
        p.RootAssembly,
        c.drawingnumber AS child_id,
        p.level + 1
    FROM tbl_assemblydrawingmapping c
    INNER JOIN RecursiveAssembly p ON c.parentdrawingnumber = p.child_id
    WHERE p.level< 50
)
SELECT
    RootAssembly AS AssemblyNumber,
    COUNT(DISTINCT child_id) AS ComponentCount
FROM RecursiveAssembly
GROUP BY RootAssembly";
        #endregion
    }
}
