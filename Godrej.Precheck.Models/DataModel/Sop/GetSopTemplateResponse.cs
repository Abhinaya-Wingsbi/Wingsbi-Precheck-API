using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel.Sop
{
    public class GetSopTemplateResponse
    {
        //parent node
        public int Assembly { get; set; }
        public string AssemblyNumber { get; set; }
        public string AssemblyProductSeries { get; set; }
        public int DrawingNumberId { get; set; } 

        public int ParentDrawingNumber { get; set; }

        public int ParentProdSeries { get; set; }

        //child node
        public string DrawingNumber { get; set; }
        public string DrawingNomenclature { get; set; }
        public int? DrawingComponentTypeId { get; set; }
        public string DrawingComponentTypeName { get; set; }
        public string DrawingProductSeries { get; set; }
        public int Level { get; set; }
        public string IdHierarchyPath { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public string FindNo { get; set; }
        public string? LnItemCode { get; set; }
    }
}
