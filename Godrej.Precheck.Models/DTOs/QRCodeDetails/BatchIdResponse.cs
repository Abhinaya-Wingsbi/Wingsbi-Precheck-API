using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    public class BatchIdResponse
    {
        public decimal Quantity { get; set; }
        public decimal BatchQuantity { get; set; }
        public int AssemblyDrawingId { get; set; }
        public string AssemblyNumber { get; set; }
    }
}

