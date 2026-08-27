namespace Godrej.Precheck.Models.DTOs.Testing
{
    public class InspectionMasterStatusDto
    {
        public int MasterId { get; set; }
        public int TotalRows { get; set; }
        public bool Stage1Completed { get; set; }
        public bool Stage2Completed { get; set; }
        public bool Stage3Completed { get; set; }
    }
}
