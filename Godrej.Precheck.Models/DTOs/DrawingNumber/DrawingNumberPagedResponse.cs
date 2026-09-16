using System;
using System.Collections.Generic;

namespace Godrej.Precheck.Models.DTOs.DrawingNumber
{
    /// <summary>
    /// Paginated response wrapper for GET /api/Common/FetchAllDrawingNumbers
    /// </summary>
    public class DrawingNumberPagedResponse
    {
        public List<GetAllDrawingResponseDto> Data { get; set; } = new List<GetAllDrawingResponseDto>();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalRecords / PageSize) : 0;
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }
}
