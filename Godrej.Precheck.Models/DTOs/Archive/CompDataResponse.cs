using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Archive
{
    /// <summary>
    /// Response DTO for simple COMP data API - matches precheck view format
    /// </summary>
    public class CompDataResponse
    {
        public int Id { get; set; }
        public string DrawingNumber { get; set; }
        public string ChildDrawingNumberId { get; set; }  // IDNos field - ID of the drawing number being consumed (e.g., "FIM")
        public string Nomenclature { get; set; }  // Nomenclature of the component (e.g., "Wire", "Split Pin", "Cotter Pin")
        public string IrNumber { get; set; }
        public string MsnNumber { get; set; }
        public string Quantity { get; set; }
        public string ConsumedIn { get; set; }
        public string Remarks { get; set; }
        public string UserName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string AssemblyNumber { get; set; }
        public string ProductionSeries { get; set; }
    }
}
