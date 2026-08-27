using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    public class BatchQRcodeResponse
    {
        public string? AssemblyNumber { get; set; }
        public int? AssemblyId { get; set; }
        public int? DrawingNumber { get; set; }
        public decimal? Quantity { get; set; }
        public int PossibleBatchQty { get; set; }
        public int CustomQty { get; set; }

    }
}
