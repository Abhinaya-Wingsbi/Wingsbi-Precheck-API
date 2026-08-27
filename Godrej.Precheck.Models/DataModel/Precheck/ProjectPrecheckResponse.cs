using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel.Precheck
{
    public class ProjectPrecheckResponse
    {
        public int Id { get; set; }
        public string ProjectNumber { get; set; }
        public string ProductionOrderNumber { get; set; }
        public int DrawingNumberId { get; set; }
        public int IdNumbers { get; set; }
        public int ProdSeriesId { get; set; }
        public int PrecheckStatus { get; set; }
    }
}
