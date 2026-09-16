using System;
using System.Collections.Generic;
using Godrej.Precheck.Models.DataModel.Precheck;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    /// <summary>
    /// Paginated response wrapper for GET /api/precheck/ViewPrechekByParameters.
    /// </summary>
    public class ViewPrecheckByParametersPagedResponse
    {
        public List<ViewPreCheckResponse> Data { get; set; } = new List<ViewPreCheckResponse>();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalRecords / PageSize) : 0;
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }
}
