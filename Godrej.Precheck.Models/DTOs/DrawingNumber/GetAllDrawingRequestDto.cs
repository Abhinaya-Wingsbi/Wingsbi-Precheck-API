using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.DrawingNumber
{
    public class GetAllDrawingRequestDto
    {
        public string? ComponentType { get; set; }
        public string? Search { get; set; }

        /// <summary>
        /// Production series names to filter by (matches if the drawing is available for any of these series).
        /// </summary>
        public List<string>? ProdSeries { get; set; }

        /// <summary>
        /// Unit names to filter by.
        /// </summary>
        public List<string>? Unit { get; set; }
    }
}
