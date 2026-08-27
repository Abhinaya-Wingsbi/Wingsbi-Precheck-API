using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    public class StandardQRDetailsResponseDto
    {
        public string QrCodeNumber { get; set; }
        public string QrCodeStatus { get; set; }
        public int QrCodeStatusId { get; set; }
        public int ProductionSeriesId { get; set; }
        public int? AssemblyNumberId { get; set; }
        public int? DrawingComponentLnItemCodeId { get; set; }
        public int? NomenclatureId { get; set; }
        public int? ComponentTypeId { get; set; }
        public string? IdNumber { get; set; }
        public int? IrNumberId { get; set; }
        public int? MsnNumberId { get; set; }
        public string? RefDocRemarks { get; set; }
        public decimal? Quantity { get; set; }
        public int LnItemCodeId { get; set; }

        public string? Desposition { get; set; }
        public DateTime? MyDate { get; set; }
        public string? Users { get; set; }
        public string? ProductionOrderNumber { get; set; }
        public string? RackLocation { get; set; }
        public string? OperationNo { get; set; }
        public int? SopNamesId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public int Id { get; set; }
        public int DrawingNumberId { get; set; }

        //extra       
        public string IrNumber { get; set; }
        public string MsnNumber { get; set; }
        public string Nomenclature { get; set; }
        public string ComponentType { get; set; }
        public string ProductionSeries { get; set; }
        public string DrawingNumber { get; set; }
        public int? UnitId { get; set; }
        public string ConsumedInDrawing { get; set; }
        public string? MRIRNumber { get; set; }
        public int IdNumbers { get; set; }
        public bool IsNewQrCode { get; set; }
        public DateTime? ManufacturingDate { get; set; }
        public string? ProjectDescription { get; set; }
        public string? ProjectNumber { get; set; }
        public string? AssemblyNumber { get; set; }
        public string? LnItemCode { get; set; }

        public string? PartNo { get; set; }
        public string? Size { get; set; }
        public string? Shapes { get; set; }
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
        public string? PurchaseOrderNumber { get; set; }
        public DateTime? StoreInDate { get; set; }
        public string? UnitName { get; set; }
        public string? BatchID { get; set; }
    }
}
