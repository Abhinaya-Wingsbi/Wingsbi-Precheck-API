using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Precheck
{
    public class UpdateQuantityResponseDto
    {
        public decimal? RemainingQuantity { get; set; }
        public decimal? AvailableQuantity {get;set;}

        public int? DrawingnumebrID { get; set; }
    }
}
