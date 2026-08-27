using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel.Archive;
using Godrej.Precheck.Models.DTOs.Archive;

namespace Godrej.Precheck.Service.Service.ArchiveService
{
    public interface IArchiveService
    {
        /// <summary>
        /// Get COMP data with simple filtering - Single API endpoint
        /// </summary>
        /// <param name="productionSeriesId">Production series ID from tbl_productionseries</param>
        /// <param name="assemblyNumberId">Assembly number ID from tbl_assemblynumber</param>
        /// <param name="componentId">Component ID number (just the number)</param>
        /// <returns>COMP data in precheck view format</returns>
        Task<List<CompDataResponse>> GetCompDataAsync(int productionSeriesId, int assemblyNumberId, string componentId);
    }
}