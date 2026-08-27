using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel.MaterialRequisition
{
    public class MaterialRequisitionResponse
    {
        public int MaterialRequisitionId { get; set; }
        public string? RequestNumber { get; set; }
        public int? RejectedComponentId { get; set; }
        public int PrecheckDetailsId { get; set; }
        public string? IdNumber { get; set; }
        public string? IrNumber { get; set; }
        public string? MsnNumber { get; set; }
        public string? MrirNumber { get; set; }
        public string? ConsumedInDrawing { get; set; }
        public string? Remarks { get; set; }
        public float? Quantity { get; set; }
        public string? Unit { get; set; }
        public DateTime? MyDate { get; set; }
        public int? ComponentCodeId { get; set; }
        public string? ComponentType { get; set; }
        public int? SrNumber { get; set; }
        public string? Username { get; set; }
        public int? NomenclatureId { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? PrecheckDate { get; set; }
        public int? IsActive { get; set; }
        public int? ProdSeriesId { get; set; }
        public int? DrawingNumberId { get; set; }
        public int? ProjectDetailsId { get; set; }
        public string? ProjectNumber { get; set; }
        public string? ProductionOrderNumber { get; set; }
        public string? LnItemCode { get; set; }
        public string? Nomenclature { get; set; }
        public string? ProductionSeries { get; set; }
        public string? DrawingNumber { get; set; }
        public bool? IsPrecheckComplete { get; set; }
        public bool? IsRejected { get; set; }
        public string? Hwno { get; set; }
        public string? RequestOwner { get; set; }
        public string? Status { get; set; }
    }
}
