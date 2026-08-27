using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel
{
    public class GetIRNumberByDrawingNumberRequest
    {

        public string? DrawingNumber { get; set; }

        public string? Stage { get; set; }
        public string? Productionseries { get; set; }
        public int? DepartmentTypeId { get; set; }
        public string? LnItemCode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? IRNumeberId { get; set; }
    }
}
