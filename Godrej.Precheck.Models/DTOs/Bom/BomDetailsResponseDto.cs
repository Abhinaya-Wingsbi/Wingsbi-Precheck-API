using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Bom
{
    /// <summary>
    /// Response DTO for BOM details - matches View SOP fields
    /// </summary>
    public class BomDetailsResponseDto
    {
        public int Level { get; set; }
        public int ParentDrawingId { get; set; }
        public int ChildDrawingId { get; set; }
        public string? ParentDrawingNumber { get; set; }
        public string? ChildDrawingNumber { get; set; }
        public string? Nomenclature { get; set; }
        public string? ComponentType { get; set; }
        public string? LnItemCode { get; set; }
        public decimal? Quantity { get; set; }
        public string? Unit { get; set; }
        public string? AssemblyNumber { get; set; }

        public string? FindNo { get; set; }
        
        // Additional fields for display (matching View SOP)
        public string? IdNumber { get; set; }
        public string? IrNumber { get; set; }
        public string? MsnNumber { get; set; }
        public string? Remarks { get; set; }
        
        // Tree display properties
        public string? Id => ChildDrawingId.ToString();
        public string? ParentId => Level == 0 ? null : ParentDrawingId.ToString();
        public bool HasChildren { get; set; }
        public bool IsExpanded { get; set; }
    }

    /// <summary>
    /// Request DTO for BOM search
    /// </summary>
    public class GetBomRequestDto
    {
        public string? AssemblyNumber { get; set; }
    }

    /// <summary>
    /// Response DTO for assembly number search/autocomplete
    /// </summary>
    public class AssemblySearchResponseDto
    {
        public int Id { get; set; }
        public string? DrawingNumber { get; set; }
        public string? Nomenclature { get; set; }
        public string? LnItemCode { get; set; }
    }
}
