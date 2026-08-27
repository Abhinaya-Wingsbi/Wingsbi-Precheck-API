using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.Archive
{
    /// <summary>
    /// Response DTO for backup database COMP data API
    /// Enhanced with separated consumed_in fields and additional metadata
    /// </summary>
    public class BackupCompDataResponse
    {
        public long Id { get; set; }
        public string DrawingNumber { get; set; }
        public string PONumber { get; set; } // IDNos field
        public string Nomenclature { get; set; }
        public string Quantity { get; set; }
        public string IDNumber { get; set; } // ConsumedInId field
        public string IRNumber { get; set; }
        public string MSNNumber { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Separated ConsumedIn fields
        public string AssemblyNumber { get; set; } // ConsumedInAssembly
        public string ProductionSeries { get; set; } // ConsumedInProdSeries
        public string ConsumedIn { get; set; } // Original ConsumedIn field
        
        public string Remarks { get; set; }
        public string UserName { get; set; }
        
        // Additional fields from backup database
        public string CompTableName { get; set; } // Source COMP table
        public string ComponentId { get; set; }
        public string ItemCode { get; set; }
        public string ComponentType { get; set; }
    }

    /// <summary>
    /// Simplified response for backward compatibility with existing API
    /// Maps backup database fields to expected format
    /// </summary>
    public class SimpleCompDataResponse
    {
        public long Id { get; set; }
        public string DrawingNumber { get; set; }
        public string ChildDrawingNumberId { get; set; }  // PONumber (IDNos)
        public string Nomenclature { get; set; }
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

    /// <summary>
    /// Paginated response wrapper
    /// </summary>
    public class BackupArchiveDataPagedResponse
    {
        public List<BackupCompDataResponse> Data { get; set; }
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public bool HasNext => PageNumber < TotalPages;
        public bool HasPrevious => PageNumber > 1;
    }

    /// <summary>
    /// Dropdown options for filtering
    /// </summary>
    public class BackupArchiveDropdownResponse
    {
        public List<string> ProductionSeries { get; set; }
        public List<string> AssemblyNumbers { get; set; }
        public List<string> DrawingNumbers { get; set; }
        public List<string> Nomenclatures { get; set; }
        public List<string> ComponentTypes { get; set; }
    }

    /// <summary>
    /// Statistics response
    /// </summary>
    public class BackupArchiveStatisticsResponse
    {
        public int TotalCompTables { get; set; }
        public int TotalDrawingNumbers { get; set; }
        public long TotalRecords { get; set; }
        public int TotalProductionSeries { get; set; }
        public int TotalAssemblyNumbers { get; set; }
        public int TotalNomenclatures { get; set; }
        public DateTime? EarliestRecord { get; set; }
        public DateTime? LatestRecord { get; set; }
    }

    /// <summary>
    /// Drawing number to COMP table mapping response
    /// </summary>
    public class DrawingCompMappingResponse
    {
        public int Id { get; set; }
        public string DrawingNumber { get; set; }
        public string CompTableName { get; set; }
        public string Nomenclature { get; set; }
        public string ItemCode { get; set; }
        public string ComponentType { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Consumption summary by assembly
    /// </summary>
    public class BackupConsumptionSummaryResponse
    {
        public string AssemblyNumber { get; set; }
        public string ProductionSeries { get; set; }
        public string ComponentId { get; set; }
        public int ComponentCount { get; set; }
        public string DrawingNumbers { get; set; }
        public string Nomenclatures { get; set; }
        public string CompTables { get; set; }
    }
}
