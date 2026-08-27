using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Repository.Queries
{
    public static class DrawingNumberQueries
    {
        #region Get Master ID from Mapping (by Drawing Number)

        public static readonly string GET_NOMENCLATURE_ID_FROM_MAPPING = @"
            SELECT nomenclatureid FROM tbl_drawingnomenclaturemapping 
            WHERE drawingnumberid = @DrawingNumberId AND isactive = 1";

        public static readonly string GET_LNITEM_ID_FROM_MAPPING = @"
            SELECT lnitemcodeid FROM tbl_drawingnlnitemlocationmapping 
            WHERE drawingnumberid = @DrawingNumberId AND isactive = 1";

        public static readonly string GET_RACKLOCATION_ID_FROM_MAPPING = @"
            SELECT racklocationid FROM tbl_drawingnlnitemlocationmapping 
            WHERE drawingnumberid = @DrawingNumberId AND isactive = 1";

        public static readonly string GET_COMPONENTTYPE_ID_FROM_MAPPING = @"
            SELECT componenttypeid FROM tbl_drawingcomponenttypemapping 
            WHERE drawingnumberid = @DrawingNumberId AND isactive = 1";

        public static readonly string GET_DOCUMENTTYPE_ID_FROM_MAPPING = @"
            SELECT documenttypeid FROM tbl_drawingdocumenttypemapping 
            WHERE drawingnumberid = @DrawingNumberId AND isactive = 1";

        public static readonly string GET_UNIT_ID_FROM_MAPPING = @"
            SELECT unitid FROM tbl_drawingunitmapping
            WHERE drawingnumberid = @DrawingNumberId AND isactive = 1";

        public static readonly string GET_PRODSERIES_ID_FROM_MAPPING = @"
            SELECT availableseriesid FROM tbl_drawingprodseriesmapping
            WHERE drawingnumberid = @DrawingNumberId AND isactive = 1";

        // Resolve a production series id (payload's prodSeriesId) to its name, for tbl_assemblydrawingmapping.consumedprodseriesid
        public static readonly string GET_PRODSERIES_NAME_BY_ID = @"
            SELECT TOP 1 productionseries
            FROM tbl_productionseries
            WHERE id = @ProdSeriesId
              AND isactive = 1";

        #endregion

        #region Drawing Number Upsert

        // Find an active tbl_drawingnumber row by drawing number + ln item code
        public static readonly string GET_DRAWINGNUMBER_ID_BY_DRAWING_AND_LNITEM = @"
            SELECT TOP 1 id
            FROM tbl_drawingnumber
            WHERE drawingnumber = @DrawingNumber
              AND lnitemcode    = @LnItemCode
              AND isactive      = 1";

        public static readonly string INSERT_DRAWING_NUMBER = @"
            INSERT INTO tbl_drawingnumber (drawingnumber, lnitemcode, isexpiry, isactive, createdby, createddate)
            VALUES (@DrawingNumber, @LnItemCode, @IsExpiry, @IsActive, @CreatedBy, @CreatedDate);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        public static readonly string UPDATE_DRAWING_NUMBER_CORE = @"
            UPDATE tbl_drawingnumber
            SET isexpiry     = @IsExpiry,
                isactive     = @IsActive,
                modifiedby   = @ModifiedBy,
                modifieddate = @ModifiedDate
            WHERE id = @Id";

        // Resolve a parent drawing number string to its id + lnitemcode
        public static readonly string GET_DRAWINGNUMBER_LOOKUP_BY_DRAWING_NUMBER = @"
            SELECT TOP 1 id, lnitemcode AS LnItemCode
            FROM tbl_drawingnumber
            WHERE drawingnumber = @DrawingNumber
              AND isactive      = 1";

        // Resolve the child drawing number's own lnitemcode by its id
        public static readonly string GET_LNITEMCODE_BY_DRAWINGNUMBER_ID = @"
            SELECT TOP 1 lnitemcode
            FROM tbl_drawingnumber
            WHERE id = @DrawingNumberId
              AND isactive = 1";

        // Resolve the drawing number's own text by its id - used to keep tbl_drawing_lnitem_map in sync
        public static readonly string GET_DRAWINGNUMBER_TEXT_BY_ID = @"
            SELECT TOP 1 drawingnumber
            FROM tbl_drawingnumber
            WHERE id = @DrawingNumberId";

        // tbl_drawing_lnitem_map is what ProductionOrder/Upload's LOOKUP_DRAWING_BY_LNITEMCODE query
        // actually reads to resolve a drawing from an item code - distinct from tbl_drawingnumber's own
        // (drawingnumber, lnitemcode) columns, which InsertDrawingMappings doesn't otherwise touch.
        public static readonly string CHECK_DRAWING_LNITEM_MAP_EXISTS = @"
            SELECT TOP 1 1
            FROM tbl_drawing_lnitem_map
            WHERE drawingnumber = @DrawingNumber
              AND lnitemcode = @LnItemCode
              AND isactive = 1";

        public static readonly string INSERT_DRAWING_LNITEM_MAP = @"
            INSERT INTO tbl_drawing_lnitem_map (drawingnumber, lnitemcode, createdby, createddate, isactive)
            VALUES (@DrawingNumber, @LnItemCode, @CreatedBy, @CreatedDate, 1)";

        // Resolve the child drawing number's mapped nomenclature by its id
        public static readonly string GET_NOMENCLATURE_BY_DRAWINGNUMBER_ID = @"
            SELECT TOP 1 n.nomenclature
            FROM tbl_drawingnomenclaturemapping dnm
            INNER JOIN tbl_nomenclature n ON dnm.nomenclatureid = n.id
            WHERE dnm.drawingnumberid = @DrawingNumberId
              AND dnm.isactive = 1
            ORDER BY dnm.createddate DESC";

        #endregion

        #region Assembly Drawing Mapping (parent/child)

        // tbl_assemblydrawingmapping.id IS an IDENTITY column
        public static readonly string GET_ASSEMBLY_DRAWING_MAPPING_ID = @"
            SELECT TOP 1 id
            FROM tbl_assemblydrawingmapping
            WHERE drawingnumber       = @ChildDrawingNumberId
              AND parentdrawingnumber = @ParentDrawingNumberId
              AND isactive            = 1";

        public static readonly string INSERT_ASSEMBLY_DRAWING_MAPPING = @"
            INSERT INTO tbl_assemblydrawingmapping
                (drawingnumber, parentdrawingnumber, createdby, createddate, isactive,
                 quantity, unit, findno, consumedprodseriesid, nomenclature, assembly_lnitemcode, child_lnitemcode)
            VALUES
                (@ChildDrawingNumberId, @ParentDrawingNumberId, @CreatedBy, @CreatedDate, 1,
                 @Quantity, @Unit, @FindNo, @ConsumedProdSeriesId, @Nomenclature, @AssemblyLnItemCode, @ChildLnItemCode);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        public static readonly string UPDATE_ASSEMBLY_DRAWING_MAPPING = @"
            UPDATE tbl_assemblydrawingmapping
            SET quantity             = @Quantity,
                unit                 = @Unit,
                findno               = @FindNo,
                consumedprodseriesid = @ConsumedProdSeriesId,
                nomenclature         = @Nomenclature,
                modifiedby           = @ModifiedBy,
                modifieddate         = @ModifiedDate
            WHERE id = @Id";

        #endregion


        #region Insert New Master Entries

        public static readonly string INSERT_NOMENCLATURE = @"
            INSERT INTO tbl_nomenclature (nomenclature, createdby, createddate, isactive)
            VALUES (@Nomenclature, @CreatedBy, @CreatedDate, 1);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        public static readonly string INSERT_LNITEMCODE = @"
            INSERT INTO tbl_lnitemcode (lnitemcode, nomenclature, createdby, createddate, isactive)
            VALUES (@LnItemCode, @Nomenclature, @CreatedBy, @CreatedDate, 1);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        public static readonly string INSERT_RACKLOCATION = @"
            INSERT INTO tbl_storeitemlocation (racklocation, createdby, createddate, isactive)
            VALUES (@RackLocation, @CreatedBy, @CreatedDate, 1);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        public static readonly string INSERT_COMPONENTTYPE = @"
            INSERT INTO tbl_componenttype (componenttype, createdby, createddate, isactive)
            VALUES (@ComponentType, @CreatedBy, @CreatedDate, 1);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        public static readonly string INSERT_DOCUMENTTYPE = @"
            INSERT INTO tbl_documenttype (documenttype, createdby, createddate, isactive)
            VALUES (@DocumentType, @CreatedBy, @CreatedDate, 1);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        public static readonly string INSERT_UNIT = @"
            INSERT INTO tbl_unit (unitname, createdby, createddate, isactive)
            VALUES (@UnitName, @CreatedBy, @CreatedDate, 1);
            SELECT SCOPE_IDENTITY();";

        #endregion

        #region Master Table Inserts (Legacy - InsertOrGet)

        // Insert or Get LnItemCode (tbl_lnitemcode.id is NOT an IDENTITY column)
        public static readonly string INSERT_OR_GET_LNITEMCODE = @"
            DECLARE @ExistingId INT;
            SELECT @ExistingId = id
            FROM tbl_lnitemcode
            WHERE lnitemcode = @LnItemCode AND isactive = 1;

            IF @ExistingId IS NOT NULL
            BEGIN
                SELECT @ExistingId;
            END
            ELSE
            BEGIN
                INSERT INTO tbl_lnitemcode (lnitemcode, nomenclature, createdby, createddate, isactive)
                VALUES ( @LnItemCode, @Nomenclature, @CreatedBy, @CreatedDate, 1);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            END";

        // Insert or Get Nomenclature (tbl_nomenclature.id IS an IDENTITY column)
        public static readonly string INSERT_OR_GET_NOMENCLATURE = @"
            DECLARE @ExistingId INT;
            SELECT @ExistingId = id
            FROM tbl_nomenclature
            WHERE nomenclature = @Nomenclature AND isactive = 1;

            IF @ExistingId IS NOT NULL
            BEGIN
                SELECT @ExistingId;
            END
            ELSE
            BEGIN
                INSERT INTO tbl_nomenclature (nomenclature, createdby, createddate, isactive)
                VALUES (@Nomenclature, @CreatedBy, @CreatedDate, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
            END";

        // Insert or Get Rack Location (Store Item Location) (tbl_storeitemlocation.id is NOT an IDENTITY column)
        public static readonly string INSERT_OR_GET_RACKLOCATION = @"
            DECLARE @ExistingId INT;
            SELECT @ExistingId = id
            FROM tbl_storeitemlocation
            WHERE racklocation = @RackLocation AND isactive = 1;

            IF @ExistingId IS NOT NULL
            BEGIN
                SELECT @ExistingId;
            END
            ELSE
            BEGIN
                INSERT INTO tbl_storeitemlocation ( racklocation, createdby, createddate, isactive)
                VALUES ( @RackLocation, @CreatedBy, @CreatedDate, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
            END";

        // Insert or Get Component Type (tbl_componenttype.id IS an IDENTITY column)
        public static readonly string INSERT_OR_GET_COMPONENTTYPE = @"
            DECLARE @ExistingId INT;
            SELECT @ExistingId = id
            FROM tbl_componenttype
            WHERE componenttype = @ComponentType AND isactive = 1;

            IF @ExistingId IS NOT NULL
            BEGIN
                SELECT @ExistingId;
            END
            ELSE
            BEGIN
                INSERT INTO tbl_componenttype (componenttype, createdby, createddate, isactive)
                VALUES (@ComponentType, @CreatedBy, @CreatedDate, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
            END";

        // Insert or Get Document Type (tbl_documenttype.id IS an IDENTITY column)
        public static readonly string INSERT_OR_GET_DOCUMENTTYPE = @"
            DECLARE @ExistingId INT;
            SELECT @ExistingId = id
            FROM tbl_documenttype
            WHERE documenttype = @DocumentType AND isactive = 1;

            IF @ExistingId IS NOT NULL
            BEGIN
                SELECT @ExistingId;
            END
            ELSE
            BEGIN
                INSERT INTO tbl_documenttype (documenttype, createdby, createddate, isactive)
                VALUES (@DocumentType, @CreatedBy, @CreatedDate, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
            END";

        // Insert or Get Unit (tbl_unit.id IS an IDENTITY column)
        public static readonly string INSERT_OR_GET_UNIT = @"
            DECLARE @ExistingId INT;
            SELECT @ExistingId = id
            FROM tbl_unit
            WHERE unitname = @UnitName AND isactive = 1;

            IF @ExistingId IS NOT NULL
            BEGIN
                SELECT @ExistingId;
            END
            ELSE
            BEGIN
                INSERT INTO tbl_unit (unitname, createdby, createddate, isactive)
                VALUES (@UnitName, @CreatedBy, @CreatedDate, 1);

                SELECT CAST(SCOPE_IDENTITY() AS INT);
            END";

        // Insert or Get Production Series (tbl_productionseries.id is NOT an IDENTITY column)
        public static readonly string INSERT_OR_GET_PRODSERIES = @"
            DECLARE @ExistingId INT;
            SELECT @ExistingId = id
            FROM tbl_productionseries
            WHERE productionseries = @AvailableFor AND isactive = 1;

            IF @ExistingId IS NOT NULL
            BEGIN
                SELECT @ExistingId;
            END
            ELSE
            BEGIN
                INSERT INTO tbl_productionseries ( productionseries, createdby, createddate, isactive)
                VALUES ( @AvailableFor, @CreatedBy, @CreatedDate, 1);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            END";

        #endregion

        #region Mapping Table Operations

        // Check if nomenclature mapping exists
        public static readonly string CHECK_NOMENCLATURE_MAPPING = @"
            SELECT TOP 1 1 
            FROM tbl_drawingnomenclaturemapping 
            WHERE drawingnumberid = @DrawingNumberId 
              AND isactive = 1";

        // Insert nomenclature mapping (tbl_drawingnomenclaturemapping.id is NOT an IDENTITY column)
        public static readonly string INSERT_NOMENCLATURE_MAPPING = @"
            INSERT INTO tbl_drawingnomenclaturemapping
                (drawingnumberid, nomenclatureid, createdby, createddate, isactive)
            VALUES ( @DrawingNumberId, @NomenclatureId, @CreatedBy, @CreatedDate, 1)";

        public static readonly string UPDATE_NOMENCLATURE_MAPPING = @"
            UPDATE tbl_drawingnomenclaturemapping
            SET nomenclatureid = @NomenclatureId
            WHERE drawingnumberid = @DrawingNumberId AND isactive = 1";

        // Check if ln item location mapping exists
        public static readonly string CHECK_LNITEM_LOCATION_MAPPING = @"
            SELECT TOP 1 1 
            FROM tbl_drawingnlnitemlocationmapping 
            WHERE drawingnumberid = @DrawingNumberId 
              AND isactive = 1";

        // Insert ln item location mapping (tbl_drawingnlnitemlocationmapping.id is NOT an IDENTITY column)
        public static readonly string INSERT_LNITEM_LOCATION_MAPPING = @"
INSERT INTO tbl_drawingnlnitemlocationmapping
(drawingnumberid, lnitemcodeid, racklocationid, createdby, createddate, isactive)
VALUES
( @DrawingNumberId, @LnItemCodeId, @RackLocationId, @CreatedBy, @CreatedDate, 1)";

        public static readonly string UPDATE_LNITEM_LOCATION_MAPPING = @"
            UPDATE tbl_drawingnlnitemlocationmapping
            SET lnitemcodeid = @LnItemCodeId,
                racklocationid = @RackLocationId
            WHERE drawingnumberid = @DrawingNumberId AND isactive = 1";

        // Check if component type mapping exists
        public static readonly string CHECK_COMPONENTTYPE_MAPPING = @"
            SELECT TOP 1 1 
            FROM tbl_drawingcomponenttypemapping 
            WHERE drawingnumberid = @DrawingNumberId 
              AND isactive = 1";

        // Insert component type mapping (tbl_drawingcomponenttypemapping.id is NOT an IDENTITY column)
        public static readonly string INSERT_COMPONENTTYPE_MAPPING = @"
            INSERT INTO tbl_drawingcomponenttypemapping
                (drawingnumberid, componenttypeid, createdby, createddate, isactive)
            VALUES ( @DrawingNumberId, @ComponentTypeId, @CreatedBy, @CreatedDate, 1)";

        public static readonly string UPDATE_COMPONENTTYPE_MAPPING = @"
            UPDATE tbl_drawingcomponenttypemapping
            SET componenttypeid = @ComponentTypeId
            WHERE drawingnumberid = @DrawingNumberId AND isactive = 1";

        // Check if document type mapping exists
        public static readonly string CHECK_DOCUMENTTYPE_MAPPING = @"
            SELECT TOP 1 1 
            FROM tbl_drawingdocumenttypemapping 
            WHERE drawingnumberid = @DrawingNumberId 
              AND isactive = 1";

        // Insert document type mapping (tbl_drawingdocumenttypemapping.id is NOT an IDENTITY column)
        public static readonly string INSERT_DOCUMENTTYPE_MAPPING = @"
            INSERT INTO tbl_drawingdocumenttypemapping
                (drawingnumberid, documenttypeid, createdby, createddate, isactive)
            VALUES (@DrawingNumberId, @DocumentTypeId, @CreatedBy, @CreatedDate, 1)";

        public static readonly string UPDATE_DOCUMENTTYPE_MAPPING = @"
            UPDATE tbl_drawingdocumenttypemapping
            SET documenttypeid = @DocumentTypeId
            WHERE drawingnumberid = @DrawingNumberId AND isactive = 1";

        // Check if unit mapping exists
        public static readonly string CHECK_UNIT_MAPPING = @"
            SELECT TOP 1 1 
            FROM tbl_drawingunitmapping 
            WHERE drawingnumberid = @DrawingNumberId 
              AND isactive = 1";

        // Insert unit mapping (tbl_drawingunitmapping.id is NOT an IDENTITY column)
        public static readonly string INSERT_UNIT_MAPPING = @"
            INSERT INTO tbl_drawingunitmapping
                (drawingnumberid, unitid, createdby, createddate, isactive)
            VALUES (@DrawingNumberId, @UnitId, @CreatedBy, @CreatedDate, 1)";

        public static readonly string UPDATE_UNIT_MAPPING = @"
            UPDATE tbl_drawingunitmapping
            SET unitid = @UnitId
            WHERE drawingnumberid = @DrawingNumberId AND isactive = 1";

        // Insert prod series mapping (tbl_drawingprodseriesmapping.id is NOT an IDENTITY column)
        public static readonly string INSERT_PRODSERIES_MAPPING = @"
            INSERT INTO tbl_drawingprodseriesmapping
                ( drawingnumberid, availableseriesid, createdby, createddate, isactive)
            VALUES ( @DrawingNumberId, @AvailableSeriesId, @CreatedBy, @CreatedDate, 1)";

        public static readonly string UPDATE_PRODSERIES_MAPPING = @"
            UPDATE tbl_drawingprodseriesmapping
            SET availableseriesid = @AvailableSeriesId
            WHERE drawingnumberid = @DrawingNumberId AND isactive = 1";

        #endregion

        #region Validation Queries

        // Check if drawing number exists and is active
        public static readonly string CHECK_DRAWING_NUMBER_EXISTS = @"
            SELECT TOP 1 1 
            FROM tbl_drawingnumber 
            WHERE id = @DrawingNumberId AND isactive = 1";

        #endregion
    }
}

