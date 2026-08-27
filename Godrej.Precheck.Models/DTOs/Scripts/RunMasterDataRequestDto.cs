using System.ComponentModel.DataAnnotations;

namespace Godrej.Precheck.Models.DTOs.Scripts
{
    public class RunMasterDataRequestDto
    {
        public List<string> FileName { get; set; } = new();
    }
}
