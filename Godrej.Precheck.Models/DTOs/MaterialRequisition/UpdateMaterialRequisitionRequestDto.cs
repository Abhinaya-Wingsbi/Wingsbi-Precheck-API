using System;
using System.ComponentModel.DataAnnotations;

namespace Godrej.Precheck.Models.DTOs.MaterialRequisition
{
    public class UpdateMaterialRequisitionRequestDto
    {
        [Required]
        public int? MaterialRequisitionId { get; set; }
        
        public string? Remarks { get; set; }
        
        public string? Hwno { get; set; }
        
        public string? RequestOwner { get; set; }
        
        // Store-specific fields
        public string? OutPONo { get; set; }
        
        public DateTime? MinDate { get; set; }
        
        public string? Status { get; set; }

        public int? StatusId { get; set; }
    }
}
