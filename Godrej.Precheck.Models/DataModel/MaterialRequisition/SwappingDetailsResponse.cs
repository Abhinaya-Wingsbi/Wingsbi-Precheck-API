using System;

namespace Godrej.Precheck.Models.DataModel.MaterialRequisition
{
    public class SwappingDetailsResponse
    {
        public int Id { get; set; }
        public int? SwappedDrawingNumberID { get; set; }
        public string? SwappedDrawingNumber { get; set; }
        public string? FromSwappedIdNumber { get; set; }
        public string? ToSwappedIdNumber { get; set; }
        public string? SwappedFromPONumber { get; set; }
        public string? SwappedToPONumber { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
    }
}
