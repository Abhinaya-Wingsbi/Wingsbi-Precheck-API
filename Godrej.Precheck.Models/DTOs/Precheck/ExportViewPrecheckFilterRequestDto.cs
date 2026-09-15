using System;
using System.Collections.Generic;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    /// <summary>
    /// Request body for POST /api/Precheck/ExportViewPrecheckdetails.
    /// Same filter shape as ViewPrechekByParameters, plus SelectedColumns to control which
    /// columns land in the exported workbook (empty/null = all).
    /// </summary>
    public class ExportViewPrecheckFilterRequestDto
    {
        /// <summary>
        /// Free-text search matched against productionOrderNumber, drawingNumber, lnItemCode.
        /// </summary>
        public string? SearchQuery { get; set; }
        public List<string>? ProductionSeries { get; set; }

        /// <summary>
        /// Precheck status labels to filter by (OR'd together): "Completed", "Pending" or "Updated".
        /// </summary>
        public List<string>? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public List<string>? SelectedColumns { get; set; }
    }
}
