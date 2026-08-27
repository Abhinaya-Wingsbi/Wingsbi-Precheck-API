using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel
{
    public class IRNumbers
    {
        public int? Id { get; set; }     
        public string? IrNumber { get; set; }
        public int? DrawingNumberId { get; set; }
        public string? Stage { get; set; } // Keep for backward compatibility/display
        public int? StageId { get; set; }
        public string? ProductionOrderNumber { get; set; }
        public string? PurchaseOrderNumber { get; set; }
        public int? NomenclatureId { get; set; }
        public int? ComponentTypeId { get; set; }
        public int? Quantity { get; set; }
        public string? Remark { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string?ProjectNumber { get; set; }
        public string? Supplier { get; set; }
        public bool? IsActive { get; set; }
        public string? DrawingNumberIdName { get; set; } 
        public string? ProductionSeriesName { get; set; }
        public string? Nomenclature { get; set; }
        public string? ComponentType { get; set; }
        public int? ProdSeriesId { get; set; }
        public int? IdNumberStart { get; set; }
        public int? IdNumberEnd { get; set; }
        public string? UserName { get; set; }
        public int? DepartmentId { get; set; }

        public string? IdNumberRange { get; set; }

        public int? SequenceNo { get; set; }

        public string? GeneratedBy { get; set; }

        public string? DepartmentName { get; set; }

        // Purchase Item fields
        public string? ItemDescription { get; set; }
        public string? LnItemCode { get; set; }

        public string? OperationNumber { get; set; }
        public string? BuildNumber { get; set; }
    }
}
