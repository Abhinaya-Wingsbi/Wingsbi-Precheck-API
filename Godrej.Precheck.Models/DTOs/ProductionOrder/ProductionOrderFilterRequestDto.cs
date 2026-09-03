using System;
using System.Collections.Generic;

namespace Godrej.Precheck.Models.DTOs.ProductionOrder
{
    /// <summary>
    /// Request body for POST /api/ProductionOrder/GetAll.
    /// pageNumber/pageSize stay on the query string; every filter is supplied here.
    /// </summary>
    public class ProductionOrderFilterRequestDto
    {
        public string? DateFilterType { get; set; }
        public DateTime? FilterDate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<int>? PrecheckStatus { get; set; }
        public string? PoNumber { get; set; }
        public string? LnItemCode { get; set; }
        public string? Role { get; set; }
        public string? DrawingNumber { get; set; }
        public string? SearchQuery { get; set; }
        public List<string>? ProductionSeries { get; set; }
    }
}
