using System;
using System.ComponentModel.DataAnnotations;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class RejectPrecheckRequestDto
    {
        [Required]
        public int PrecheckDetailsId { get; set; }
        
        [Required]
        public int DrawingNumberId { get; set; }
        
        [Required]
        public int ProductionSeriesId { get; set; }
        
        [Required]
        public string IdNumber { get; set; }
        
        public string? QrCodeNumber { get; set; }
        
        
        public string? RejectedRemarks { get; set; }
        
      
        public string? DuplicateRemarks { get; set; }
        
   
        public int? CreatedBy { get; set; }
        public decimal? RemainingQuantity { get; set; }
        public string? ComponentType { get; set; }
    }
}
