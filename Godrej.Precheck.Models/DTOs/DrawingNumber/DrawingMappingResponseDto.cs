using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.DrawingNumber
{
    public class DrawingMappingResponseDto
    {
        public int DrawingNumberId { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public MappingDetails Details { get; set; }
    }

    public class MappingDetails
    {
        public int? LnItemCodeId { get; set; }
        public int? NomenclatureId { get; set; }
        public int? RackLocationId { get; set; }
        public int? ComponentTypeId { get; set; }
        public int? DocumentTypeId { get; set; }
        public int? UnitId { get; set; }
        public int? ProdSeriesId { get; set; }
        public int? AssemblyDrawingMappingId { get; set; }

        public bool DrawingNumberCreated { get; set; }
        public bool DrawingNumberUpdated { get; set; }
        public bool LnItemLocationMappingCreated { get; set; }
        public bool LnItemLocationMappingUpdated { get; set; }
        public bool NomenclatureMappingCreated { get; set; }
        public bool NomenclatureMappingUpdated { get; set; }
        public bool ComponentTypeMappingCreated { get; set; }
        public bool ComponentTypeMappingUpdated { get; set; }
        public bool DocumentTypeMappingCreated { get; set; }
        public bool DocumentTypeMappingUpdated { get; set; }
        public bool UnitMappingCreated { get; set; }
        public bool UnitMappingUpdated { get; set; }
        public bool ProdSeriesMappingCreated { get; set; }
        public bool ProdSeriesMappingUpdated { get; set; }
        public bool AssemblyDrawingMappingCreated { get; set; }
        public bool AssemblyDrawingMappingUpdated { get; set; }
        public bool DrawingLnItemMapCreated { get; set; }
    }
}

