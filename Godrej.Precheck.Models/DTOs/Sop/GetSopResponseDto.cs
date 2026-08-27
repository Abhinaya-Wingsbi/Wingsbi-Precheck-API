using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Sop
{
    public class GetSopResponseDto
    {
        public int SerialNumber { get; set; }
        public int ProdSeriesId { get; set; }
        public string Id { get; set; }
        public int DrawingNumberId { get; set; }
        public string DrawingNumber { get; set; }
        public string Nomenclature { get; set; }
        public string IdNumber { get; set; }
        public string Quantity { get; set; }
        public string IrNumber { get; set; }
        public string MsnNumber { get; set; }
        public string MrirNumber { get; set; }
        public string Remarks { get; set; }
        public string AssemblyNumber { get; set; }
        public string Unit { get; set; }
        public int Level { get; set; }
        public string Build { get; set; }
        public string Snag_Sheet_No { get; set; }
        public string FindNo { get; set; }

        //for tree representation
        public List<GetSopResponseDto> Children { get; set; } = new List<GetSopResponseDto>();
    }
}
