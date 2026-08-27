using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel.Precheck
{
    public class MakePrecheckRequest
    {
        public string? ConsumedDrawingNo { get; set; }
        public int ConsumedInDrawingNumberID { get; set; }
        public int ConsumedInProdSeriesID { get; set; }
        public int ConsumedInId { get; set; }
        public int? DrawingNumberId { get; set; }
        public int Id { get; set; }
        public int? ProductionSeriesId { get; set; }
        public string? Remarks { get; set; }
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public string? IrNumber { get; set; }
        public string? MsnNumber { get; set; }
        public string? MrirNumber { get; set; }
        public string? QrCodeNumber { get; set; }
        public string ComponentType { get; set; }
        public int MyProperty { get; set; }
        public string ProductionOrderNumber { get; set; }
        public string IdNumbers { get; set; }
        public int CreatedBy { get; set; }
        public string UserName { get; set; }
        public int? LnItemCodeId { get; set; }
        public string? LnItemCode { get; set; }
        public string? PrecheckStatus { get; set; }
        public int? PrecheckStatusId { get; set; }
        public decimal? RemainingQuantity { get; set; }
        public int QrCodeId { get; set; }
    }
}
