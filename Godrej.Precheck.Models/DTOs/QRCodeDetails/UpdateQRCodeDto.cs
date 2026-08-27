using System;

namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    public class UpdateQRCodeDto
    {
        public string? QRCodeNumber { get; set; }
        public int? DrawingNumberId { get; set; }
        public int? ProductionSeriesId { get; set; }
        public int? NomenclatureId { get; set; }
        public int? ComponentTypeId { get; set; }
        public string? IdNumber { get; set; }
        public int? IrNumberId { get; set; }
        public int? MsnNumberId { get; set; }
        public decimal? Quantity { get; set; }
        public string? Desposition { get; set; }
        public string? MRIRNumber { get; set; }
        public string? ProductionOrderNumber { get; set; }
        public string? PurchaseOrderNumber { get; set; }
        public string? Remarks { get; set; }
        public int? ShapeId { get; set; }
        public int? UnitId { get; set; }
        public string? Size { get; set; }
        public string? HeatLotBatch { get; set; }
        public int? ModifiedBy { get; set; }
    }
}
