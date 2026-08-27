using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DataModel.Precheck;
using Godrej.Precheck.Models.DataModel.Sop;
using Godrej.Precheck.Models.DTOs.Bom;
using Godrej.Precheck.Repository.Database;
using Godrej.Precheck.Repository.Queries;
using Microsoft.Extensions.Logging;

namespace Godrej.Precheck.Repository.Repository.SopRepository
{
    public class SopRepository : ISopRepository
    {
        private readonly ILogger<SopRepository> _logger;
        private readonly IApplicationDbContext _db;

        public SopRepository(ILogger<SopRepository> logger,IApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<List<SopAssemblyResponse>> GetAllAssembly()
        {
            _logger.LogInformation($"Request for SopRepository:GetAllAssembly");
            var results = await _db.GetAll<SopAssemblyResponse>(
            SopQueries.GET_SOP_NAMES,
            new {});
            _logger.LogInformation($"Result for SopRepository:GetAllAssembly, {results}");
            return results.ToList();
        }

        public async Task<List<GetSopTemplateResponse>> GetSopTemplate(int assemblyNumber)
        {
            _logger.LogInformation($"Request for SopRepository:GetSopTemplate {assemblyNumber}");
            var results = await _db.GetAll<GetSopTemplateResponse>(
            SopQueries.GET_SOP_TEMPLATE,
            new { assemblydrawingnumber = assemblyNumber });
            _logger.LogInformation($"Result for SopRepository:GetSopTemplates", results);
            return results.ToList();
        }


        public async Task<List<GetSopTemplateResponse>> GetAllSopTemplate(int assemblyNumber)
        {
            _logger.LogInformation($"Request for SopRepository:GetSopTemplate");
            var results = await _db.GetAll<GetSopTemplateResponse>(
            SopQueries.GET_ALL_SOP_TEMPLATE,
            new { assemblydrawingnumber = assemblyNumber });
            _logger.LogInformation($"Result for SopRepository:GetSopTemplates");
            return results.ToList();
        }

        public async Task<List<SopConsumptionResponse>> GetSopConsumptionData(string drawingNumbers)
        {
            _logger.LogInformation($"Request for SopRepository:GetSopConsumptionData {drawingNumbers}");
            var parameters = new DynamicParameters();
            parameters.Add("@drawingNumbers", drawingNumbers);
            var results = await _db.GetAll<SopConsumptionResponse>(
            SopQueries.GET_SOP_CONSUMPTION_DATA,
            parameters);
            _logger.LogInformation($"Result for SopRepository:GetSopConsumptionData {results}");
            return results.ToList();
        }


        public async Task<List<SopConsumptionResponse>> GetSopPrecheckData(string drawingNumbers)
        {
            _logger.LogInformation($"Request for SopRepository:GetSopConsumptionData");
            var parameters = new DynamicParameters();
            parameters.Add("@drawingNumbers", drawingNumbers);
            var results = await _db.GetAll<SopConsumptionResponse>(
            SopQueries.GET_SOP_PRECHECK_CONSUMPTION_DATA,
            parameters);
            _logger.LogInformation($"Result for SopRepository:GetSopConsumptionData");
            return results.ToList();
        }

        /// <summary>
        /// Get recursive BOM by assembly drawing number.
        /// Returns complete hierarchy of child components.
        /// </summary>
        public async Task<List<BomDetailsResponseDto>> GetRecursiveBomByAssembly(string assemblyNumber)
        {
            _logger.LogInformation($"Request for SopRepository:GetRecursiveBomByAssembly {assemblyNumber}");
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@assemblyNumber", assemblyNumber);
                var results = await _db.GetAll<BomDetailsResponseDto>(
                    BomQueries.GET_RECURSIVE_BOM_BY_ASSEMBLY_NUMBER,
                    parameters);
                _logger.LogInformation($"Result for SopRepository:GetRecursiveBomByAssembly - {results.Count()} records");
                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetRecursiveBomByAssembly for {assemblyNumber}");
                throw;
            }
        }

        // In SopRepository — add this new method
        public async Task<Dictionary<string, int>> GetBomComponentCountsAsync(List<string> assemblyNumbers)
        {
            if (!assemblyNumbers.Any()) return new Dictionary<string, int>();

            var parameters = new DynamicParameters();
            parameters.Add("@assemblyNumbers", assemblyNumbers); // pass as TVP or join

            var results = await _db.GetAll<(string AssemblyNumber, int ComponentCount)>(
                BomQueries.GET_BOM_COUNTS_BY_ASSEMBLY_NUMBERS,
                parameters);

            return results.ToDictionary(x => x.AssemblyNumber, x => x.ComponentCount);
        }

        /// <summary>
        /// Search for assembly numbers by partial match.
        /// </summary>
        public async Task<List<AssemblySearchResponseDto>> SearchAssemblyNumbers(string searchText)
        {
            _logger.LogInformation($"Request for SopRepository:SearchAssemblyNumbers {searchText}");
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@searchText", searchText ?? "");
                var results = await _db.GetAll<AssemblySearchResponseDto>(
                    BomQueries.GET_ASSEMBLY_NUMBERS_SEARCH,
                    parameters);
                _logger.LogInformation($"Result for SopRepository:SearchAssemblyNumbers - {results.Count()} records");
                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in SearchAssemblyNumbers for {searchText}");
                throw;
            }
        }

        public async Task<int> GetSubAssemblyProjectId(string pivotIdentifier, string idNumbers)
        {
            _logger.LogInformation($"Request for SopRepository:GetSubAssemblyProjectId pivotIdentifier: {pivotIdentifier}, idNumbers: {idNumbers}");

            // tbl_projectdetails.idnumbers is an int column - if idNumbers isn't a valid number, it can never
            // match, same outcome as the old CAST-to-string comparison would give for non-numeric input.
            if (!int.TryParse(idNumbers, out var idNumbersInt))
            {
                return 0;
            }

            var parameters = new DynamicParameters();
            parameters.Add("@pivotIdentifier", pivotIdentifier, DbType.AnsiString);
            parameters.Add("@idNumbers", idNumbersInt);
            var result = await _db.GetSingle<int>(
                SopQueries.GET_SUB_ASSEMBLY_PROJECT_ID,
                parameters);
            _logger.LogInformation($"Result for SopRepository:GetSubAssemblyProjectId: {result}");
            return result;
        }

        public async Task<(string Build, string SnagSheetNo)> GetRootSopBuildAndSnag(int assemblyDrawingId, int prodSeriesId, int serielNumberId)
        {
            _logger.LogInformation($"Request for SopRepository:GetRootSopBuildAndSnag - assemblyDrawingId: {assemblyDrawingId}, prodSeriesId: {prodSeriesId}, serielNumberId: {serielNumberId}");
            var parameters = new DynamicParameters();
            parameters.Add("@AssemblyDrawingId", assemblyDrawingId);
            parameters.Add("@ProdSeriesId", prodSeriesId);
            parameters.Add("@SerielNumberId", serielNumberId);
            var result = await _db.GetSingle<dynamic>(
                SopQueries.GET_ROOT_SOP_BUILD_AND_SNAG,
                parameters);

            if (result == null)
                return (null, null);

            return ((string)result.Build, (string)result.SnagSheetNo);
        }
    }
}

