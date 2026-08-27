using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    public class StandardQRCodeDetails
    {
        public string? QRCodeNumber { get; set; }
        public int QRCodeId { get; set; }
        public int ProductionSeriesId { get; set; }
        public int QrcodeStatusId { get; set; }
        public int? NomenclatureId { get; set; }
        public int? LnItemCodeId { get; set; }
        public string? LnItemCode { get; set; }
        public int? RackLocationId { get; set; }
        public int ComponentTypeId { get; set; }
        public string? IdNumber { get; set; }
        public int IdNumbers { get; set; }
        public int? IrNumberId { get; set; }
        public int? MsnNumberId { get; set; }
        public int SrNumber { get; set; }
        public string? RefDocRemarks { get; set; }
        public List<int>? Ids { get; set; }
        public decimal Quantity { get; set; }
        public string? Desposition { get; set; }
        public DateTime? MyDate { get; set; }
        public string? Users { get; set; }
        public string? ProductionOrderNumber { get; set; }
        public string? RackLocation { get; set; }
        public string? OperationNo { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Id { get; set; }
        public int DrawingNumberId { get; set; }
        public string? MRIRNumber { get; set; }
        public int? UnitId { get; set; }
        public DateTime? ManufacturingDate { get; set; }
        public string? ProjectDescription { get; set; }

        public string? ProjectNumber { get; set; }

        public List<BatchInfo> BatchIds { get; set; }

        public string? PartNo { get; set; }
        public string? Size { get; set; }
        public int? ShapeId { get; set; }
        public string? CustomerItemCode { get; set; }
        public string? Material { get; set; }
        public string? HTLotNo { get; set; }
        public string? FanManNumber { get; set; }
        public string? FanManSerialNumber { get; set; }
        public string? SerialNumberOfQuantity { get; set; }
        public string? MsnIrNumber { get; set; }
        public string? GFNNo { get; set; }
        public string? SrNo { get; set; }
        public string? TQty { get; set; }
        public string? WC { get; set; }
        public int ToggleComponentTypeId { get; set; }
        public string? Remarks { get; set; }

        /// <summary>
        /// PO Number / Project Order Number
        /// </summary>
        public string? PoNumber { get; set; }

        /// <summary>
        /// Project name
        /// </summary>
        public string? Project { get; set; }

        /// <summary>
        /// Purchase Order Number (for purchase items)
        /// </summary>
        public string? PurchaseOrderNumber { get; set; }
    }
}
