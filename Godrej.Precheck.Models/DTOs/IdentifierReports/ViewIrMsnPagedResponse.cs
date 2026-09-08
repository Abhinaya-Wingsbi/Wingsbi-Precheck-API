using System;
using System.Collections.Generic;

namespace Godrej.Precheck.Models.DTOs.IdentifierReports
{
    /// <summary>
    /// Paginated response wrapper for POST /api/reports/viewIrMsn.
    /// </summary>
    public class ViewIrMsnPagedResponse
    {
        public List<ViewIrMsnResponseDto> Data { get; set; } = new List<ViewIrMsnResponseDto>();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalRecords / PageSize) : 0;
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }
}
