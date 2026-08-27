using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.ProductionOrder
{
    public class DeleteProductionOrderRequestDto
    {
        public string ProductionOrderNumber { get; set; } = string.Empty;
        public int IdNumber { get; set; }
        public int Quantity { get; set; }
    }
}
