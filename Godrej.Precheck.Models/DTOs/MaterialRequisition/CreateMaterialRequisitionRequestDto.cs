using System;
using System.ComponentModel.DataAnnotations;

namespace Godrej.Precheck.Models.DTOs.MaterialRequisition
{
    /// <summary>
    /// DTO for creating new material requisition (used by Planner role)
    /// User provides: Project Number, Drawing/LN Item Code, Production Series, ID Number
    /// System looks up matching row in tbl_projectprecheckdetails and copies to tbl_material_requestion
    /// </summary>
    public class CreateMaterialRequisitionRequestDto
    {
        [Required]
        public int RejectedDrawingNumberId { get; set; }
        
        [Required]
        public int ProdSeriesId { get; set; }
        
        [Required]
        public string? IdNumber { get; set; }
        
        public string? Remarks { get; set; }

        public decimal Quantity { get; set; }

        public string? Nomenclature { get; set; }

        public int AssemblyDrawingNumberId { get; set; } 

        public string lnitemcode { get; set; }

        public string? ProductionOrderNumber { get; set; }
        public string? RejectedIdNumber { get; set; }
    }
}

