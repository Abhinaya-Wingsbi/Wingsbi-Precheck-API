using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    public class StandardQRDataDto
    {
        public int ProductionSeriesId { get; set; }
        public int? LnItemCodeId { get; set; }
        public int? RackLocationId { get; set; }
        public string? LnItemCode { get; set; }
        public int? NomenclatureId { get; set; }
        public int? ComponentTypeId { get; set; }
        public string? IdNumber { get; set; }
        public int? IrNumberId { get; set; }
        public int? MsnNumberId { get; set; }
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
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; }
        public int Id { get; set; }
        public int DrawingNumberId { get; set; }
        public string? MRIRNumber { get; set; }
        public int? UnitId { get; set; }
        public DateTime? ManufacturingDate { get; set; }
        public string? Remark { get; set; }
        public string? ProjectNumber { get; set; }
        public List<BatchInfo>? BatchIds { get; set; }
        public string? Remarks { get; set; }

        // Optional: add individual fields from remark for separate tracking if needed
        public string? ProjectDescription { get; set; }
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

        /// <summary>
        /// Matrix table rows from the UI containing ID, Size, MRIR, and Heat/Lot/Batch data
        /// </summary>
        public List<QrMatrixRowDto>? MatrixRows { get; set; }
        public string? PurchaseOrderNumber { get; set; }
    }
}
