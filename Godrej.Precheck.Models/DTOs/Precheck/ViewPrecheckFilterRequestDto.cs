using System;
using System.Collections.Generic;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    /// <summary>
    /// Request body for POST /api/precheck/ViewPrecheck.
    /// </summary>
    public class ViewPrecheckFilterRequestDto
    {
        /// <summary>
        /// Free-text search matched against productionOrderNumber, drawingNumber, lnItemCode.
        /// </summary>
        public string? SearchQuery { get; set; }
        public List<string>? ProdSeries { get; set; }

        /// <summary>
        /// Precheck status labels to filter by (OR'd together): "Completed", "Pending" or "Updated".
        /// </summary>
        public List<string>? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
