using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.User
{
    public class UpdateProdSeriesRequestDto
    {
        public int Id { get; set; }
        public string ProductionSeries { get; set; }
        public int ModifiedBy { get; set; }
    }
}
