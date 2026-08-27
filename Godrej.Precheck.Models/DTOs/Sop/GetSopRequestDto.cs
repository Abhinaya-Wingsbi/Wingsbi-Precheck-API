using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Sop
{
    public class GetSopRequestDto
    {
        public int AssemblyDrawingId { get; set; }
        public int ProdSeriesId { get; set; }
        public int SerielNumberId { get; set; }
        public string AssemblyDrawing { get; set; }
    }
}
