using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class AvailableComponentFilterDto
    {
        public string QrCode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Free-text search matched against ProductionOrderNumber and IdNumber. Ignored when QrCode is supplied.
        /// </summary>
        public string? SearchQuery { get; set; }

        /// <summary>
        /// Drawing number to filter by (resolved to tbl_drawingnumber.id). Ignored when QrCode is supplied.
        /// </summary>
        public string? DrawingNumber { get; set; }

        /// <summary>
        /// Production series names to filter by. Ignored when QrCode is supplied.
        /// </summary>
        public List<string>? ProdSeries { get; set; }

        /// <summary>
        /// Precheck status label to filter by: "Partial" or "Pending".
        /// </summary>
        public string? Status { get; set; }
    }
}
