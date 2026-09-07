namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class AddPrecheckComponentDto
    {
        public string? AssemblyLnItemCode { get; set; }
        public string? ChildLnItemCode { get; set; }
        public string? ComponentType { get; set; }

        // true: add this component to every existing production order of AssemblyLnItemCode.
        // false: skip existing production orders entirely (no tbl_projectprecheckdetails rows written).
        public bool UserInput { get; set; }
    }
}
