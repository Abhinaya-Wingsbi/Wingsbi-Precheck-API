using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class BulkPrecheckRequestDto
    {
        public int FromId { get; set; }
        public int ToId { get; set; }

        public string? QrCodeNumber { get; set; }
        public decimal QtyToBeConsume { get; set; }

        public string? ConsumedDrawingNo { get; set; }
        public int ConsumedInDrawingNumberID { get; set; }
        public int ConsumedInProdSeriesID { get; set; }

        public int? DrawingNumberId { get; set; }
        public int? ProductionSeriesId { get; set; }
        public string? Remarks { get; set; }
        public string? Unit { get; set; }
        public string? IrNumber { get; set; }
        public string? MsnNumber { get; set; }
        public string? MrirNumber { get; set; }
        public string ComponentType { get; set; }
        public string ProductionOrderNumber { get; set; }
        public int CreatedBy { get; set; }
        public int? LnItemCodeId { get; set; }
        public string? LnItemCode { get; set; }
        public string? AssemblyDrawingNo { get; set; }
    }
}
