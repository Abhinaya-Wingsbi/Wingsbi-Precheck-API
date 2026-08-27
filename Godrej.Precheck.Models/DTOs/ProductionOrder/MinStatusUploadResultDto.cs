namespace Godrej.Precheck.Models.DTOs.ProductionOrder
{
    public class MinStatusUploadResultDto
    {
        public int TotalRows { get; set; }
        public int UpdatedRows { get; set; }
        public List<string> NotFoundProductionOrderNumbers { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}
