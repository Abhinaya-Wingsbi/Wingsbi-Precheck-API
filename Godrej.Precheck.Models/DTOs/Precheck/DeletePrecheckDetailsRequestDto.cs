namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class DeletePrecheckDetailsRequestDto
    {
        public string ProductionOrderNumber { get; set; }
        public int IdNumber { get; set; }
        public int DrawingNumberId { get; set; }
    }
}
