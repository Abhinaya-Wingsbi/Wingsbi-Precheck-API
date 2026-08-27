namespace Godrej.Precheck.Models.DTOs.Identifier
{  
    public class DownloadMSNMemoDto
    {
        public string MsnNumber { get; set; } = "";
        public int CreatedBy { get; set; }
        public int DepartmentId { get; set; }
        public string DocumentType { get; set; } = "";
        public int DrawingNumberId { get; set; }
        public string IdNumberRange { get; set; } = "";
        public string IdRange { get; set; } = "";
        public bool IsStandard { get; set; }
        public string LnItemCode { get; set; } = "";
        public string OperationNumber { get; set; } = "";
        public int ProdSeriesId { get; set; }
        public string ProductionOrderNumber { get; set; } = "";
        public string ProjectNumber { get; set; } = "";
        public string PurchaseOrderNumber { get; set; } = "";
        public int Quantity { get; set; }
        public string Remark { get; set; } = "";
        public int StageId { get; set; }
        public string Supplier { get; set; } = "";
        public string? UserName { get; set; }
    }
}


