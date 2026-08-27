using System.Collections.Generic;
using Godrej.Precheck.Models.DTOs.Precheck;

namespace Godrej.Precheck.Models.DTOs.ProductionOrder
{
    public class ProductionOrderDetailsDto
    {
        public ProductionOrderMasterDto Master { get; set; } = new();
        public List<MakeOrderResponseDto> BomItems { get; set; } = new();
    }
}
