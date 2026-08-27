using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class ResetRemainingQuantityDto
    {
        public int DrawingNumberId { get; set; }
        public int IdNumber { get; set; }
        public decimal ScannedQuantity { get; set; }
        public string PONumber { get; set; }
        public string QrCodeNumber { get; set; }
    }
}
