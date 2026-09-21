using System.Collections.Generic;

namespace Godrej.Precheck.Models.DTOs.IdentifierReports
{
    /// <summary>
    /// Request body for POST /api/reports/ExportIrMsn.
    /// Same filters as ViewIrMsnRequestDto, plus the columns to include in the exported file.
    /// </summary>
    public class ExportIrMsnRequestDto : ViewIrMsnRequestDto
    {
        /// <summary>
        /// Names of the ViewIrMsnResponseDto columns to export, e.g. "IrNumber", "MsnNumber".
        /// When null/empty, all columns are exported.
        /// </summary>
        public List<string>? SelectedColumns { get; set; }
    }
}
