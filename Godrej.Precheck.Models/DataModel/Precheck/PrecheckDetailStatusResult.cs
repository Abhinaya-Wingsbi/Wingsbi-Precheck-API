namespace Godrej.Precheck.Models.DataModel.Precheck
{
    public class PrecheckDetailStatusResult
    {
        public int Id { get; set; }
        public int? IsActive { get; set; }
        public bool IsPrecheckComplete { get; set; }
        public int QRCodeId { get; set; }
        public decimal Quantity { get; set; }
    }
}
