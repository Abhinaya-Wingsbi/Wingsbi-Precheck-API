using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class StoredInQrCodeRequest
    {
        public DateTime? StoreInDate { get; set; }
        public string? DrawingNumber { get; set; }
    }
}
