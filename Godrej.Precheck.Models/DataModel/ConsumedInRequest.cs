using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel
{
    public class ConsumedInRequest
    {
        public int ProdSeriesId { get; set; }

        public int? IdNumber { get; set; }

        public int DrawingNumberId { get; set; }

        public int? AssemblyIdNumber { get; set; }

        public string ConsumedInDrawing { get; set; }
    }
}
