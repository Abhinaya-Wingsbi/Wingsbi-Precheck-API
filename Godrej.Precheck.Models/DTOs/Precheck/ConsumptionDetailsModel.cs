using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class ConsumptionDetailsModel
    {

        public int Id { get; set; }

        public int ProdSeriesId { get; set; }

        public int DrawingId { get; set; }
        public string IdNumber { get; set; }
        public string Nomenclature { get; set; }
        public decimal Quantity { get; set; }
        public string IrNumber { get; set; } 
        public string MsnNumber { get; set; } 
        public string Remarks { get; set; }
        public string ProdSeries { get; set; }
        public string AssemblyNumber { get; set; }
    }
}
