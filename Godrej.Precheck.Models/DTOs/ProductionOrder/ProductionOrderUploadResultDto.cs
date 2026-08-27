using System.Collections.Generic;

namespace Godrej.Precheck.Models.DTOs.ProductionOrder
{
    /// <summary>
    /// Response DTO for POST /api/ProductionOrder/Upload
    /// </summary>
    public class ProductionOrderUploadResultDto
    {
        public int TotalRows { get; set; }
        public int Imported { get; set; }
        public int Skipped { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
