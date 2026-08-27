using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class UpdateMaterialQuantityRequestDto
    {
        public int? DrawingnumberId {  get; set; }
        public int ParentDrawingNumber { get; set; }
        public decimal? UpdatedQuantity { get; set; }
        public string? LnItemCode { get; set; }
        public int? CreatedBy { get; set; }
        public string QrCodeNumber { get; set; }
        public string? ProductionOrderNumber { get; set;}
        public int? Idnumber { get; set; }
        public string? ComponentType { get; set; }
    }
}
