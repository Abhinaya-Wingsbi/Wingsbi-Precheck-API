using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.QRCodeDetails
{
    public class ConsumedInRequestDto
    {
        public int ProdSeriesId { get; set; }

        public int? IdNumber { get; set; }

        public int DrawingNumberId { get; set; }

        public string? AssemblyNumber { get; set; }

    }
}
