using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.User
{
    public class AddProdSeriesRequestDto
    {
        public string ProductionSeries { get; set; }
        public int CreatedBy { get; set; }
    }
}
