namespace Godrej.Precheck.Models.DTOs.Testing
{
    public class DrawingStageStatusDto
    {
        public int MasterId { get; set; }
        public string DrawingNumber { get; set; } = string.Empty;
        public string MsnNumber { get; set; } = string.Empty;
        public int TotalRows { get; set; }
        public bool Stage1Completed { get; set; }
        public bool Stage2Completed { get; set; }
        public bool Stage3Completed { get; set; }
        public int CurrentStage { get; set; }
        public string CurrentStageName { get; set; } = string.Empty;
    }
}
