using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Repository.Queries
{
    /// <summary>
    /// SQL queries for backup database archive functionality
    /// Queries the unified tbl_archive_comp_data table in PrecheckDB_QA
    /// </summary>
    public static class BackupArchiveQueries
    {
        #region GET ARCHIVE DATA FROM BACKUP DATABASE

        public static readonly string GET_BACKUP_ARCHIVE_DATA = @"
            -- Optimized query for backup database archive data
            -- Uses tbl_archive_comp_data with separated consumed_in fields
            SELECT 
                acd.Id,
                acd.DrawingNumber,
                acd.IDNos as PONumber,
                acd.Nomenclature,
                ISNULL(acd.Quantity, '---') as Quantity,
                acd.ConsumedInId as IDNumber,
                acd.IRNos as IRNumber,
                acd.MSNNos as MSNNumber,
                ISNULL(acd.Status, 'Consumed') as Status,
                acd.CreatedDate,
                acd.ConsumedInAssembly as AssemblyNumber,
                acd.ConsumedInProdSeries as ProductionSeries,
                acd.ConsumedInOriginal as ConsumedIn,
                acd.Remarks,
                acd.UserName,
                acd.CompTableName,
                acd.ComponentId,
                acd.ItemCode
            FROM tbl_archive_comp_data acd WITH (NOLOCK)
            WHERE acd.IsActive = 1
                AND (@ConsumedInProdSeries IS NULL OR acd.ConsumedInProdSeries = @ConsumedInProdSeries)
                AND (@ConsumedInAssembly IS NULL OR acd.ConsumedInAssembly LIKE '%' + @ConsumedInAssembly + '%')
                AND (@ConsumedInId IS NULL OR acd.ConsumedInId = @ConsumedInId)
                AND (@DrawingNumber IS NULL OR acd.DrawingNumber = @DrawingNumber)
                AND (@ComponentId IS NULL OR acd.ComponentId = @ComponentId)
                AND (@Nomenclature IS NULL OR acd.Nomenclature LIKE '%' + @Nomenclature + '%')
                AND (@IDNos IS NULL OR acd.IDNos = @IDNos)
                AND (@CompTableName IS NULL OR acd.CompTableName = @CompTableName)
            ORDER BY acd.CreatedDate DESC";

        #endregion

        #region GET ARCHIVE DATA COUNT FROM BACKUP DATABASE

        public static readonly string GET_BACKUP_ARCHIVE_DATA_COUNT = @"
            SELECT COUNT(acd.Id) as TotalCount
            FROM tbl_archive_comp_data acd
            WHERE acd.IsActive = 1
                AND (@ConsumedInProdSeries IS NULL OR acd.ConsumedInProdSeries = @ConsumedInProdSeries)
                AND (@ConsumedInAssembly IS NULL OR acd.ConsumedInAssembly LIKE '%' + @ConsumedInAssembly + '%')
                AND (@ConsumedInId IS NULL OR acd.ConsumedInId = @ConsumedInId)
                AND (@DrawingNumber IS NULL OR acd.DrawingNumber = @DrawingNumber)
                AND (@ComponentId IS NULL OR acd.ComponentId = @ComponentId)
                AND (@Nomenclature IS NULL OR acd.Nomenclature LIKE '%' + @Nomenclature + '%')
                AND (@IDNos IS NULL OR acd.IDNos = @IDNos)
                AND (@CompTableName IS NULL OR acd.CompTableName = @CompTableName)";

        #endregion

        #region GET DROPDOWN OPTIONS FROM BACKUP DATABASE

        public static readonly string GET_BACKUP_PRODUCTION_SERIES = @"
            SELECT DISTINCT ConsumedInProdSeries as ProductionSeries
            FROM tbl_archive_comp_data 
            WHERE IsActive = 1 
                AND ConsumedInProdSeries IS NOT NULL 
                AND ConsumedInProdSeries != ''
            ORDER BY ConsumedInProdSeries";

        public static readonly string GET_BACKUP_ASSEMBLY_NUMBERS = @"
            SELECT DISTINCT ConsumedInAssembly as AssemblyNumber
            FROM tbl_archive_comp_data 
            WHERE IsActive = 1 
                AND ConsumedInAssembly IS NOT NULL 
                AND ConsumedInAssembly != ''
            ORDER BY ConsumedInAssembly";

        public static readonly string GET_BACKUP_DRAWING_NUMBERS = @"
            SELECT DISTINCT DrawingNumber
            FROM tbl_archive_comp_data 
            WHERE IsActive = 1 
                AND DrawingNumber IS NOT NULL 
                AND DrawingNumber != ''
            ORDER BY DrawingNumber";

        public static readonly string GET_BACKUP_NOMENCLATURES = @"
            SELECT DISTINCT Nomenclature
            FROM tbl_archive_comp_data 
            WHERE IsActive = 1 
                AND Nomenclature IS NOT NULL 
                AND Nomenclature != ''
                AND Nomenclature != 'Unknown'
            ORDER BY Nomenclature";

        #endregion

        #region GET COMPONENT DETAILS BY DRAWING NUMBER FROM BACKUP

        public static readonly string GET_BACKUP_COMPONENTS_BY_DRAWING_NUMBER = @"
            SELECT 
                acd.Id,
                acd.DrawingNumber,
                acd.IDNos,
                acd.IRNos,
                acd.MSNNos,
                acd.ConsumedInOriginal as ConsumedIn,
                acd.ConsumedInAssembly,
                acd.ConsumedInProdSeries,
                acd.ConsumedInId,
                acd.Remarks,
                acd.Quantity,
                acd.MyDate,
                acd.UserName,
                acd.ComponentId,
                acd.Nomenclature,
                acd.CompTableName,
                acd.ItemCode,
                acd.ComponentType
            FROM tbl_archive_comp_data acd
            WHERE acd.IsActive = 1 
                AND acd.DrawingNumber = @DrawingNumber
            ORDER BY acd.ConsumedInOriginal, acd.ComponentId";

        #endregion

        #region GET CONSUMPTION DETAILS BY ASSEMBLY FROM BACKUP

        public static readonly string GET_BACKUP_CONSUMPTION_BY_ASSEMBLY = @"
            SELECT 
                acd.ConsumedInAssembly as AssemblyNumber,
                acd.ConsumedInProdSeries as ProductionSeries,
                acd.ConsumedInId as ComponentId,
                COUNT(acd.Id) as ComponentCount,
                STRING_AGG(DISTINCT acd.DrawingNumber, ', ') as DrawingNumbers,
                STRING_AGG(DISTINCT acd.Nomenclature, ', ') as Nomenclatures,
                STRING_AGG(DISTINCT acd.CompTableName, ', ') as CompTables
            FROM tbl_archive_comp_data acd
            WHERE acd.IsActive = 1 
                AND (@AssemblyPattern IS NULL OR acd.ConsumedInAssembly LIKE '%' + @AssemblyPattern + '%')
                AND (@ProductionSeries IS NULL OR acd.ConsumedInProdSeries = @ProductionSeries)
            GROUP BY acd.ConsumedInAssembly, acd.ConsumedInProdSeries, acd.ConsumedInId
            ORDER BY acd.ConsumedInAssembly, acd.ConsumedInProdSeries, acd.ConsumedInId";

        #endregion

        #region EXPORT QUERIES FOR BACKUP DATABASE

        public static readonly string GET_BACKUP_ARCHIVE_DATA_FOR_EXPORT = @"
            SELECT 
                acd.IDNos as 'PO Number',
                acd.DrawingNumber as 'Drawing Number',
                ISNULL(acd.Nomenclature, 'N/A') as 'Nomenclature',
                acd.Quantity,
                acd.ConsumedInId as 'ID Number',
                acd.IRNos as 'IR Number',
                acd.MSNNos as 'MSN Number',
                ISNULL(acd.Status, 'Consumed') as Status,
                FORMAT(acd.CreatedDate, 'dd-MM-yyyy HH:mm:ss') as 'Created Date',
                acd.ConsumedInAssembly as 'Assembly Number',
                acd.ConsumedInProdSeries as 'Production Series',
                acd.ConsumedInOriginal as 'Consumed In',
                acd.Remarks,
                acd.UserName as 'User Name',
                acd.CompTableName as 'Source Table',
                acd.ItemCode as 'Item Code',
                acd.ComponentType as 'Component Type'
            FROM tbl_archive_comp_data acd
            WHERE acd.IsActive = 1 
                AND (@ConsumedInProdSeries IS NULL OR acd.ConsumedInProdSeries = @ConsumedInProdSeries)
                AND (@ConsumedInAssembly IS NULL OR acd.ConsumedInAssembly LIKE '%' + @ConsumedInAssembly + '%')
                AND (@ConsumedInId IS NULL OR acd.ConsumedInId = @ConsumedInId)
                AND (@DrawingNumber IS NULL OR acd.DrawingNumber = @DrawingNumber)
            ORDER BY acd.CreatedDate DESC";

        #endregion

        #region STATISTICS QUERIES FOR BACKUP DATABASE

        public static readonly string GET_BACKUP_ARCHIVE_STATISTICS = @"
            SELECT 
                COUNT(DISTINCT acd.CompTableName) as TotalCompTables,
                COUNT(DISTINCT acd.DrawingNumber) as TotalDrawingNumbers,
                COUNT(acd.Id) as TotalRecords,
                COUNT(DISTINCT acd.ConsumedInProdSeries) as TotalProductionSeries,
                COUNT(DISTINCT acd.ConsumedInAssembly) as TotalAssemblyNumbers,
                COUNT(DISTINCT acd.Nomenclature) as TotalNomenclatures,
                MIN(acd.CreatedDate) as EarliestRecord,
                MAX(acd.CreatedDate) as LatestRecord
            FROM tbl_archive_comp_data acd
            WHERE acd.IsActive = 1";

        #endregion

        #region MAPPING QUERIES

        public static readonly string GET_DRAWING_COMP_MAPPINGS = @"
            SELECT 
                dcm.Id,
                dcm.DrawingNumber,
                dcm.CompTableName,
                dcm.Nomenclature,
                dcm.ItemCode,
                dcm.ComponentType,
                dcm.CreatedDate
            FROM tbl_drawing_comp_mapping dcm
            WHERE dcm.IsActive = 1
            ORDER BY dcm.DrawingNumber, dcm.CompTableName";

        public static readonly string GET_MAPPING_BY_DRAWING_NUMBER = @"
            SELECT 
                dcm.Id,
                dcm.DrawingNumber,
                dcm.CompTableName,
                dcm.Nomenclature,
                dcm.ItemCode,
                dcm.ComponentType
            FROM tbl_drawing_comp_mapping dcm
            WHERE dcm.IsActive = 1 
                AND dcm.DrawingNumber = @DrawingNumber
            ORDER BY dcm.CompTableName";

        #endregion

        #region ADVANCED SEARCH QUERIES

        public static readonly string SEARCH_ARCHIVE_DATA = @"
            SELECT 
                acd.Id,
                acd.DrawingNumber,
                acd.IDNos as PONumber,
                acd.Nomenclature,
                acd.Quantity,
                acd.ConsumedInId as IDNumber,
                acd.IRNos as IRNumber,
                acd.MSNNos as MSNNumber,
                acd.Status,
                acd.CreatedDate,
                acd.ConsumedInAssembly as AssemblyNumber,
                acd.ConsumedInProdSeries as ProductionSeries,
                acd.ConsumedInOriginal as ConsumedIn,
                acd.Remarks,
                acd.UserName,
                acd.CompTableName
            FROM tbl_archive_comp_data acd
            WHERE acd.IsActive = 1
                AND (
                    @SearchTerm IS NULL 
                    OR acd.DrawingNumber LIKE '%' + @SearchTerm + '%'
                    OR acd.Nomenclature LIKE '%' + @SearchTerm + '%'
                    OR acd.IDNos LIKE '%' + @SearchTerm + '%'
                    OR acd.ConsumedInAssembly LIKE '%' + @SearchTerm + '%'
                    OR acd.ConsumedInId LIKE '%' + @SearchTerm + '%'
                    OR acd.CompTableName LIKE '%' + @SearchTerm + '%'
                )
            ORDER BY acd.CreatedDate DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        /// <summary>
        /// Search for drawing numbers consumed in specific assembly
        /// Based on ConsumedIn pattern like "D/K324-0000-000CB/321"
        /// Returns IDNos as ChildDrawingNumberId (e.g., "FIM")
        /// Returns MyDate as CreatedDate (e.g., "30-Oct-19 11:54:45")
        /// </summary>
        public static readonly string SEARCH_BY_CONSUMED_IN = @"
            SELECT 
                acd.Id,
                tdcm.DrawingNumber,
                acd.IDNos as ChildDrawingNumberId,
                acd.Nomenclature,
                acd.IRNos as IRNumber,
                acd.MSNNos as MSNNumber,
                acd.Quantity,
                acd.ConsumedInOriginal as ConsumedIn,
                acd.Remarks,
                acd.UserName,
                acd.MyDate as CreatedDate,
                acd.ConsumedInAssembly as AssemblyNumber,
                acd.ConsumedInProdSeries as ProductionSeries
            FROM tbl_archive_comp_data acd
            INNER JOIN tbl_drawing_comp_mapping tdcm ON acd.DrawingCompMappingId = tdcm.Id
            WHERE acd.IsActive = 1
                AND tdcm.IsActive = 1
                AND acd.ConsumedInProdSeries = @ProductionSeries
                AND acd.ConsumedInAssembly = @AssemblyNumber
                AND acd.ConsumedInId = @ComponentId
            ORDER BY tdcm.DrawingNumber, acd.CreatedDate DESC";

        #endregion
    }
}
