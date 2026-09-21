using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    public class GetAvailableQrRequest
    {
        /// <summary>
        /// 1 = Raw Material only (LnItemCode NOT starting with 'WJD'); 2 = all except Raw Material (LnItemCode starting with 'WJD'); null = no filter.
        /// </summary>
        public int? QrType { get; set; }

        /// <summary>
        /// Free-text search matched against LnItemCode/DrawingNumber (partial match).
        /// </summary>
        public string? SearchQuery { get; set; }

        /// <summary>
        /// Production series names to filter by (OR'd together).
        /// </summary>
        public List<string>? ProdSeries { get; set; }
    }
}
