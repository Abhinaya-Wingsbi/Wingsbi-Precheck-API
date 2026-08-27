using System.ComponentModel.DataAnnotations;

namespace Godrej.Precheck.Models.DTOs.User
{
    public class UploadSignatureRequestDto
    {
     
        public int? UserId { get; set; }

        public string? Signature { get; set; }  // Base64 encoded signature image
        public int? ModifiedBy { get; set; }
    }
}
