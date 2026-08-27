using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel.Sop;
using Godrej.Precheck.Models.DTOs.Sop;
using Godrej.Precheck.Models.DTOs.Bom;

namespace Godrej.Precheck.Service.Service.SopService
{
    public interface ISopService
    {
        Task<List<SopAssemblyResponseDto>> GetAllAssembly();
        Task<List<GetSopResponseDto>> GetSopForAssembly(GetSopRequestDto request);
        Task<List<GetSopResponseDto>> GetSopForAssembly(GetSopRequestDto request, bool excludeRawMaterial);
        byte[] ExportToExcel(List<GetSopResponseDto> items, string projectId);
        
        // BOM Details methods
        Task<List<BomDetailsResponseDto>> GetBomDetails(string assemblyNumber);
        Task<List<AssemblySearchResponseDto>> SearchAssemblyNumbers(string searchText);
        byte[] ExportBomToExcel(List<BomDetailsResponseDto> items, string assemblyNumber);
    }
}

