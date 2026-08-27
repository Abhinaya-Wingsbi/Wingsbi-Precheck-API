using System;

namespace Godrej.Precheck.Models.DTOs.IRNumber
{
    /// <summary>
    /// DTO for Purchase Item IR Number generation
    /// </summary>
    public class StandardIRNumberDto
    {
        public int? DrawingNumberId { get; set; }
        public int? NomenclatureId { get; set; }
        public int? ComponentTypeId { get; set; }
        public string? ItemDescription { get; set; }
        public string? LnItemCode { get; set; }
        public string? ProjectNumber { get; set; }
        public string? ProductionOrderNumber { get; set; }
        public string? PurchaseOrderNumber { get; set; }
        public int ProdSeriesId { get; set; }
        public string? IdNumberRange { get; set; }
        public int? IdNumberStart { get; set; }
        public int? IdNumberEnd { get; set; }
        public int Quantity { get; set; }
        public int? StageId { get; set; }
        public string? Supplier { get; set; }
        public string? Remark { get; set; }
        
        // Auto-populated by controller from JWT claims
        public int? CreatedBy { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? OperationNumber { get; set; }
    }
}
