namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class AddPrecheckComponentResponseDto
    {
        public int ProjectsChecked { get; set; }
        public int ComponentsAdded { get; set; }
        public int AlreadyPresentSkipped { get; set; }
    }
}
