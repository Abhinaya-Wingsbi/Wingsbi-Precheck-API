using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel.Precheck
{
    public class PrecheckTemplateResponse
    {
        public int Id { get; set; }
        public string? AssemblyNumber { get; set; }
        public int DrawingNumberId { get; set; }
        public string? DrawingNumber { get; set; }
        public string? Nomenclature { get; set; }

        public int? NomenclatureId { get; set; }
       
        public decimal? Quantity { get; set; }

        public int? ComponentTypeId { get; set; }

        public string ComponentType { get; set; }
        public int? UnitId { get; set; }
        public string? Unit { get; set; }
        public string lnitemcode { get; set; }

    }
}
