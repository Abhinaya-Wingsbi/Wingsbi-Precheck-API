using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel
{
    public class DrawingNumbers
    {
        public int Id { get; set; }
        public string DrawingNumber { get; set; }
        public string ComponentCode { get; set; }
        public string LnItemCode { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string Location { get; set; }
        public string Nomenclature { get; set; }
        public string ComponentType { get; set; }
        public string DocumentType { get; set; }
         public int? LnItemCodeId { get; set; }

        //// Newly Added Properties (as per query)
        public int? RackLocationId { get; set; } 
        public int? NomenclatureId { get; set; } 
        public int? ComponentTypeId { get; set; } 
        public int? DocumentTypeId { get; set; } 
        public int? AssemblyId { get; set; }
        public string AssemblyNumber { get; set; }
        // Comma-separated list (a drawing number can map to multiple production series / parent
        // drawing numbers); parsed back into lists in CommonService.GetAllDrawingNumberService.
        public string AvailableSeriesId { get; set; }
        public string AvailableSeries { get; set; }
        public bool? IsExpiry { get; set; }
        public int? UnitId { get; set; }
        public string UnitName { get; set; }
        public string ParentDrawingNumberId { get; set; }
        public string ParentDrawingNumber { get; set; }
    }
}
