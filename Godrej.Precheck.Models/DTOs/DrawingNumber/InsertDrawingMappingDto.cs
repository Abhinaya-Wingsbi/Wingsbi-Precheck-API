using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.DrawingNumber
{
    public class InsertDrawingMappingDto
    {
        // Null or 0 means "resolve or create" via DrawingNumber + LnItemCode below
        public int? DrawingNumberId { get; set; }

        // Used only when DrawingNumberId is 0, to look up or create the tbl_drawingnumber row
        public string? DrawingNumber { get; set; }

        // LnItemCode related
        public string? LnItemCode { get; set; }
        public string? LnItemNomenclature { get; set; }

        // Nomenclature
        public string? Nomenclature { get; set; }

        // Rack Location (Store Item Location)
        public string? RackLocation { get; set; }

        // Component Type
        public string? ComponentType { get; set; }

        // Document Type
        public string? DocumentType { get; set; }

        // Unit
        public string? UnitName { get; set; }

        // Production series this drawing is available for (e.g. "H")
        public string? AvailableFor { get; set; }

        // "Yes"/"No" -> tbl_drawingnumber.isexpiry
        public string? HasExpiry { get; set; }

        // "active"/"inactive" -> tbl_drawingnumber.isactive
        public string? Status { get; set; }

        // tbl_assemblydrawingmapping: links this drawing as a child of the given parent drawing
        public string? ParentDrawingNumber { get; set; }
        public decimal? Quantity { get; set; }
        public string? FindNo { get; set; }

        // First id is resolved against tbl_productionseries (isactive = 1) -> tbl_assemblydrawingmapping.consumedprodseriesid
        public List<int>? AvailableSeriesId { get; set; }

        public int? CreatedBy { get; set; }
    }
}

