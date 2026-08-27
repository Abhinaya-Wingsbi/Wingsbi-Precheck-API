using System;
using System.ComponentModel.DataAnnotations;

namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    public class PrecheckQRCodeRequestDto
    {
        [Required]
        public int DrawingNumberId { get; set; }
        
        [Required]
        public int ProductionSeriesId { get; set; }
        
        [Required]
        public int IdNumber { get; set; }
        
        [Required]
        [StringLength(15, MinimumLength = 15, ErrorMessage = "QR Code Number must be exactly 15 digits")]
        public string QRCodeNumber { get; set; }
        
        public int? CreatedBy { get; set; }
        
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
    }
}
