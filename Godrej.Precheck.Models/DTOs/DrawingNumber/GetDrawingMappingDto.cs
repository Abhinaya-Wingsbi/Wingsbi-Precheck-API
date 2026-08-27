using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.DrawingNumber
{
    public class GetDrawingMappingDto
    {
        public int DrawingNumberId { get; set; }
        public string DrawingNumber { get; set; }
        public string? LnItemCode { get; set; }
        public string? Nomenclature { get; set; }
        public string? RackLocation { get; set; }
        public string? ComponentType { get; set; }
        public string? DocumentType { get; set; }
        public string? UnitName { get; set; }
    }
}

