using System;
using System.Collections.Generic;
using Godrej.Precheck.Models.DataModel.Precheck;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    /// <summary>
    /// Paginated response wrapper for POST /api/precheck/GetStoreAvailablComponents.
    /// </summary>
    public class AvailableComponentPagedResponse
    {
        public List<AvailableComponentModel> Data { get; set; } = new List<AvailableComponentModel>();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalRecords / PageSize) : 0;
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }
}
