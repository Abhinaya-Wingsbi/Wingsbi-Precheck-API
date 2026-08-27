using System.Collections.Generic;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class PrecheckExcelImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<PrecheckExcelRowResultDto> Results { get; set; } = new();
    }

    public class PrecheckExcelRowResultDto
    {
        public string QrCodeNumber { get; set; }
        public string ProductionOrderNumber { get; set; }
        public int IdNumber { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
