using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    /// <summary>
    /// Represents a single row in the QR code matrix table from the UI
    /// </summary>
    public class QrMatrixRowDto
    {
        /// <summary>
        /// Serial number of the row
        /// </summary>
        public int SrNo { get; set; }

        /// <summary>
        /// ID Number for this specific item
        /// </summary>
        public string? IdNo { get; set; }

        /// <summary>
        /// Size of the item
        /// </summary>
        public string? Size { get; set; }

        /// <summary>
        /// MRIR number for this item
        /// </summary>
        public string? Mirir { get; set; }

        /// <summary>
        /// Heat/Lot/Batch number for this item
        /// </summary>
        public string? HeatLotBatchNo { get; set; }

        /// <summary>
        /// Quantity for this specific item row
        /// </summary>
        public decimal Quantity { get; set; }
    }
}
