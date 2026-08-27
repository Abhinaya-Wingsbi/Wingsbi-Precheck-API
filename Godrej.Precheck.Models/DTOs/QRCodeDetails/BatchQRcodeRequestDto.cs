using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    public  class BatchQRcodeRequestDto
    {
        public int DrawingNumberId { get; set; }
        public decimal Quantity { get; set; }
        public string? Remarks { get; set; }
    }
}
