using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel.Sop
{
    public class SopConsumptionResponse
    {
     
        public int ProjectPrecheckDetailsId { get; set; }
        public int? DrawingNumberId { get; set; }

        public int? ProdSeriesId { get; set; }

        public string Id { get; set; }


        public string IrNumber { get; set; }
        public string MsnNumber { get; set; }
        public string MrirNumber { get; set; }

       
        public string Remarks { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
      
        public int? ComponentCodeId { get; set; }

    
        public int? NomenclatureId { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsActive { get; set; }
        public string IdNumber { get; set; }

        public string ConsumedInDrawing { get; set; }
        //parent node
        public int ConsumedinDrawingNumberId { get; set; }
        public int ConsumedinProdSeriesId { get; set; }

        public string ConsumedinId { get; set; }
        public int ConsumedinIdIdentity { get; set; }

        public string ComponentType { get; set; }
        public int ComponentTypeId { get; set; }
        public string ProductionOrderNumber { get; set; } // add if not present
        public string? Build { get; set; }
        public string? SnagSheetNo { get; set; }
        public string? ConsumedinProductionOrderNumber { get; set; }

        // The build number recorded against the specific QR code actually consumed for this component
        // (tbl_qrcodedetails.buildnumber, joined via tbl_projectprecheckdetails.qrcodeid) - distinct from
        // Build above, which comes from the production order instead.
        public string? QrBuildNumber { get; set; }
        public string? ConsumedQrCodeNumber { get; set; }
    }
}
