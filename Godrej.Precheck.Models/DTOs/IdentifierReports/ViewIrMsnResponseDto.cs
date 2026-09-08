using System;

namespace Godrej.Precheck.Models.DTOs.IdentifierReports
{
    public class ViewIrMsnResponseDto
    {
        public int? Id { get; set; }
        public string? DocumentType { get; set; }
        public string? IrNumber { get; set; }
        public string? MsnNumber { get; set; }
        public string? ProductionOrderNumber { get; set; }
        public string? DrawingNumber { get; set; }
        public string? LnItemCode { get; set; }
        public string? ProductionSeriesName { get; set; }
        public string? DepartmentName { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
