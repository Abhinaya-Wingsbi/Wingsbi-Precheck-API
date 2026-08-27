namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class PendingPrecheckRequestDto
    {
        public int? AssemblyDrawingNumberId { get; set; }
        public int? ProdSeriesId { get; set; }
        public string? ProductionOrderNumber { get; set; }
        public int? IdNumber { get; set; }
        public string? LnItemCode { get; set; }

        // Optional: 1 = only child rows whose own precheckStatus is "Pending", 2 = only "Updated".
        // Filters the Childs under each pending id - a unit whose Childs end up empty after this
        // filter is dropped from the response entirely.
        public int? StatusId { get; set; }
    }
}
