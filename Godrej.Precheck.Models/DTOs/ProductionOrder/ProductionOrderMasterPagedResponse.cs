using System;
using System.Collections.Generic;

namespace Godrej.Precheck.Models.DTOs.ProductionOrder
{
    /// <summary>
    /// Paginated response wrapper for GET /api/ProductionOrder/GetAll
    /// </summary>
    public class ProductionOrderMasterPagedResponse
    {
        public List<ProductionOrderMasterDto> Data { get; set; } = new List<ProductionOrderMasterDto>();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalRecords / PageSize) : 0;
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }
}
