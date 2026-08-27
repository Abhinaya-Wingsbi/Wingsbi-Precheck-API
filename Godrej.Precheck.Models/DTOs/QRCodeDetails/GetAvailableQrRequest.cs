using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    public class GetAvailableQrRequest
    {
        public string LnItemCode { get; set; }
        public string DrawingNumber { get; set; }
        public int? ProdSeriesId { get; set; }

        /// <summary>
        /// 1 = Raw Material only (LnItemCode NOT starting with 'WJD'); 2 = all except Raw Material (LnItemCode starting with 'WJD'); null = no filter.
        /// </summary>
        public int? QrType { get; set; }
    }
}
