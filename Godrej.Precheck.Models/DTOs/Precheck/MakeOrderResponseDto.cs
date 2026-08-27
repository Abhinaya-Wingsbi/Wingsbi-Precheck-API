using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel.Precheck;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class MakeOrderResponseDto
    {
        public int Id { get; set; }
        public string? AssemblyNumber { get; set; }
        public int DrawingNumberId { get; set; }
        public string? DrawingNumber { get; set; }
        public string? Nomenclature { get; set; }
        public int? NomenclatureId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? TotalQuantity { get; set; }
        public int? AvailableQuantity { get; set; }
        public string lnitemcode { get; set; }
        public decimal? TotalQrQty { get; set; }
        public int? UnitId { get; set; }
        public string? Unit { get; set; }
    }
}
