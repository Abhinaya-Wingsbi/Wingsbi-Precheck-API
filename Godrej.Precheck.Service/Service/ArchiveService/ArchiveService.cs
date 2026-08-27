using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel.Archive;
using Godrej.Precheck.Models.DTOs.Archive;
using Godrej.Precheck.Repository.Repository.ArchiveRepository;
using Microsoft.Extensions.Logging;

namespace Godrej.Precheck.Service.Service.ArchiveService
{
    public class ArchiveService : IArchiveService
    {
        private readonly ILogger<ArchiveService> _logger;
        private readonly IArchiveRepository _archiveRepository;

        public ArchiveService(ILogger<ArchiveService> logger, IArchiveRepository archiveRepository)
        {
            _logger = logger;
            _archiveRepository = archiveRepository;
        }

        public async Task<List<CompDataResponse>> GetCompDataAsync(int productionSeriesId, int assemblyNumberId, string componentId)
        {
            try
            {
                _logger.LogInformation($"ArchiveService: Scanning ALL data for optimal results - ProductionSeriesId: {productionSeriesId}, AssemblyNumberId: {assemblyNumberId}, ComponentId: {componentId}");

                var request = new ArchiveFilterRequest
                {
                    ProductionSeriesId = productionSeriesId,
                    AssemblyNumberId = assemblyNumberId,
                    IdNumber = componentId
                };

                // Direct SQL query with no pagination - scans all 600K+ rows efficiently
                var allData = await _archiveRepository.GetAllArchiveDataAsync(request);
                
                // Convert to CompDataResponse format (matches precheck view)
                var compData = allData.Select(x => new CompDataResponse
                {
                    Id = x.Id,
                    DrawingNumber = x.DrawingNumber,
                    ChildDrawingNumberId = x.PONumber ?? "---", // Using PONumber which contains IDNos (drawing number ID)
                    Nomenclature = x.Nomenclature ?? "---", // Component nomenclature (e.g., Wire, Split Pin, etc.)
                    IrNumber = x.IRNumber,
                    MsnNumber = x.MSNNumber,
                    Quantity = x.Quantity ?? "---",
                    ConsumedIn = x.ConsumedIn,
                    Remarks = x.Remarks,
                    UserName = x.UserName,
                    CreatedDate = x.CreatedDate,
                    AssemblyNumber = x.AssemblyNumber,
                    ProductionSeries = x.ProductionSeries
                }).ToList();

                _logger.LogInformation($"ArchiveService: Successfully scanned and retrieved {compData.Count} matching records from large dataset");
                return compData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ArchiveService: Error scanning data: {ex.Message}");
                throw;
            }
        }

    }
}