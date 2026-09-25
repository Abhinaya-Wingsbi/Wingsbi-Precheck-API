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
        /// No longer used for filtering -- GetAvailableQr always returns all QR codes (raw material and assemblies).
        /// Kept so existing clients that still send this field don't break.
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
