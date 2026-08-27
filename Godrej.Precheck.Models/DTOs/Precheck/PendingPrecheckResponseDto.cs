using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godrej.Precheck.Models.DataModel.Precheck;
using Godrej.Precheck.Models.DTOs.ProductionOrder;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    // Same shape as ProductionOrderController's GetAll response (ProductionOrderMasterDto),
    // with one addition: PendingIdNumbers, one entry per unit id (within StartIdNumber..EndIdNumber)
    // whose precheck is not fully complete, each carrying its own tbl_projectprecheckdetails child rows.
    public class PendingPrecheckResponseDto : ProductionOrderMasterDto
    {
        // System.Text.Json otherwise serializes a derived class's own properties before the
        // inherited base-class ones, which put PendingIdNumbers ahead of every GetAll field.
        // Forcing a high explicit order puts it last, after the production order details.
        [JsonPropertyOrder(1000)]
        public List<PendingPrecheckIdDto> PendingIdNumbers { get; set; } = new List<PendingPrecheckIdDto>();
    }

    public class PendingPrecheckIdDto
    {
        public int IdNumber { get; set; }

        // The tbl_projectprecheckdetails rows for this specific unit id - same shape ViewPrecheck
        // returns. Empty when the unit hasn't been started at all (no precheck rows exist yet).
        public List<ViewPreCheckResponse> Childs { get; set; } = new List<ViewPreCheckResponse>();
    }
}
