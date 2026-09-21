using System;
using System.Collections.Generic;

namespace Godrej.Precheck.Models.DTOs.IdentifierReports
{
    /// <summary>
    /// Request body for POST /api/reports/viewIrMsn.
    /// pageNumber/pageSize stay on the query string; every other filter is here.
    /// </summary>
    public class ViewIrMsnRequestDto
    {
        /// <summary>
        /// Free-text search matched against productionOrderNumber, drawingNumber, lnItemCode, irNumber, msnNumber.
        /// </summary>
        public string? SearchQuery { get; set; }
        public List<string>? ProductionSeries { get; set; }
        public List<int>? DepartmentTypeId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// "IR" and/or "MSN". Determines which document rows and which number field(s) are returned.
        /// </summary>
        public List<string>? DocumentType { get; set; }
    }
}
