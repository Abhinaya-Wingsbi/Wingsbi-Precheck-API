using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel.Sop;
using Godrej.Precheck.Models.DTOs.Sop;
using Godrej.Precheck.Models.DTOs.Bom;

namespace Godrej.Precheck.Repository.Repository.SopRepository
{
    public interface ISopRepository
    {
        Task<List<SopAssemblyResponse>> GetAllAssembly();

        Task<List<GetSopTemplateResponse>> GetSopTemplate(int assemblyNumber);
        Task<List<GetSopTemplateResponse>> GetAllSopTemplate(int assemblyNumber);
        Task<List<SopConsumptionResponse>> GetSopConsumptionData(string drawingNumbers);

        Task<List<SopConsumptionResponse>> GetSopPrecheckData(string drawingNumbers);
        
        // BOM Details methods
        Task<List<BomDetailsResponseDto>> GetRecursiveBomByAssembly(string assemblyNumber);
        Task<List<AssemblySearchResponseDto>> SearchAssemblyNumbers(string searchText);
        Task<Dictionary<string, int>> GetBomComponentCountsAsync(List<string> assemblyNumbers);
        Task<int> GetSubAssemblyProjectId(string irNumber, string idNumbers);
        Task<(string Build, string SnagSheetNo)> GetRootSopBuildAndSnag(int assemblyDrawingId, int prodSeriesId, int serielNumberId);
    }
}
