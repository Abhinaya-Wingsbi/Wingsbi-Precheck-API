using System.ComponentModel.DataAnnotations;

namespace Godrej.Precheck.Models.DTOs.MaterialRequisition
{
    public class CancelMaterialRequisitionRequestDto
    {
        [Required]
        public int RequestId { get; set; }

        // Spelling matches the tbl_material_requestion column name (requestcancleremarks)
        public string? RequestCancleRemarks { get; set; }
    }
}
