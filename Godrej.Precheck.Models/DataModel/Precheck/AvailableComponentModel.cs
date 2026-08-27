using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel.Precheck
{
    public class AvailableComponentModel
    {
        public int Id { get; set; }
        public string IdNumber { get; set; }
        public decimal Quantity { get; set; }
        public int? DrawingnumberId { get; set; }
        public string DrawingNumber { get; set; }
        public int? Prodseriesid { get; set; }
        public string ProductionSeries { get; set; }
        public string Nomenclature { get; set; }
        public string ProductionOrderNumber { get; set; }
        public int? Modifiedby { get; set; }
        public DateTime? Modifieddate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedByName { get; set; }
        public string ProjectNumber { get; set; }
        public string FanManNumber { get; set; }
        public int PrecheckStatusId { get; set; }
        public  string PrecheckStatus { get; set; } 
    }
}
