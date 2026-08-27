using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Repository.Queries
{
    public static class ArchiveQueries
    {
        #region GET ARCHIVE DATA WITH FILTERS

        public static readonly string GET_ARCHIVE_DATA = @"
            WITH FilteredData AS (
                SELECT 
                    cd.Id,
                    cd.IDNos,
                    cd.IRNos,
                    cd.MSNNos,
                    cd.ConsumedIn,
                    cd.Remarks,
                    cd.Quantity,
                    cd.MyDate,
                    cd.UserName,
                    cd.AssemblyId,
                    cd.ProductionSeries,
                    cd.AssemblyNumber,
                    cd.ComponentId,
                    cd.CreatedDate,
                    dn.drawingnumber,
                    ISNULL(nom.nomenclature, 'N/A') as nomenclature, -- Safely handle missing nomenclature
                    cdi.CompTableName
                FROM tbl_comp_data cd
                LEFT JOIN tbl_comp_data_info cdi ON cd.CompInfoId = cdi.Id
                LEFT JOIN tbl_drawingnumber dn ON cdi.DrawingNumberId = dn.Id
                LEFT JOIN tbl_drawingnomenclaturemapping nommap ON dn.Id = nommap.drawingnumberid
                LEFT JOIN tbl_nomenclature nom ON nommap.nomenclatureid = nom.Id
                WHERE cd.IsActive = 1 
                    AND (cdi.IsActive = 1 OR cdi.IsActive IS NULL)
                    AND (dn.isactive = 1 OR dn.isactive IS NULL)
                    AND (@AssemblyNumber IS NULL OR cd.AssemblyNumber LIKE '%' + @AssemblyNumber + '%')
                    AND (@ProductionSeries IS NULL OR cd.ProductionSeries = @ProductionSeries)
                    AND (@IdNumber IS NULL OR cd.ComponentId LIKE '%' + @IdNumber + '%')
                    AND (@DrawingNumberId IS NULL OR cdi.DrawingNumberId = @DrawingNumberId)
                    AND (@ProductionSeriesId IS NULL OR cd.ProductionSeriesId = @ProductionSeriesId)
                    AND (@AssemblyNumberId IS NULL OR cd.AssemblyNumberId = @AssemblyNumberId)
            )
            SELECT 
                Id,
                IDNos as PONumber,
                drawingnumber as DrawingNumber,
                nomenclature as Nomenclature,
                Quantity,
                ComponentId as IDNumber,
                IRNos as IRNumber,
                MSNNos as MSNNumber,
                'Consumed' as Status, -- Default status, can be enhanced based on business logic
                COALESCE(MyDate, CreatedDate) as CreatedDate,
                AssemblyNumber,
                ProductionSeries,
                ConsumedIn,
                Remarks,
                UserName,
                ROW_NUMBER() OVER (ORDER BY COALESCE(MyDate, CreatedDate) DESC) as RowNum
            FROM FilteredData
            ORDER BY COALESCE(MyDate, CreatedDate) DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        #endregion

        #region GET ALL ARCHIVE DATA (WITHOUT PAGINATION)

        public static readonly string GET_ALL_ARCHIVE_DATA = @"
            -- Optimized query for large dataset scanning (600K+ rows)
            -- Uses indexes on ProductionSeriesId, AssemblyNumberId, ComponentId for fast filtering
            SELECT 
                cd.Id,
                cd.IDNos as PONumber,
                dn.drawingnumber as DrawingNumber,
                ISNULL(nom.nomenclature, 'N/A') as Nomenclature,
                ISNULL(cd.Quantity, '---') as Quantity,
                cd.ComponentId as IDNumber,
                cd.IRNos as IRNumber,
                cd.MSNNos as MSNNumber,
                'Consumed' as Status,
                COALESCE(cd.MyDate, cd.CreatedDate) as CreatedDate,
                cd.AssemblyNumber,
                cd.ProductionSeries,
                cd.ConsumedIn,
                cd.Remarks,
                cd.UserName
            FROM tbl_comp_data cd WITH (NOLOCK) -- Read uncommitted for performance on large scans
            LEFT JOIN tbl_comp_data_info cdi ON cd.CompInfoId = cdi.Id
            LEFT JOIN tbl_drawingnumber dn ON cdi.DrawingNumberId = dn.Id
            LEFT JOIN tbl_drawingnomenclaturemapping nommap ON dn.Id = nommap.drawingnumberid
            LEFT JOIN tbl_nomenclature nom ON nommap.nomenclatureid = nom.Id
            WHERE cd.IsActive = 1 
                AND (cdi.IsActive = 1 OR cdi.IsActive IS NULL)
                AND (dn.isactive = 1 OR dn.isactive IS NULL)
                -- Optimized filters using exact matches on indexed columns
                AND (@ProductionSeriesId IS NULL OR cd.ProductionSeriesId = @ProductionSeriesId)
                AND (@AssemblyNumberId IS NULL OR cd.AssemblyNumberId = @AssemblyNumberId)
                AND (@IdNumber IS NULL OR cd.ComponentId = @IdNumber) -- Changed from LIKE to exact match for performance
                AND (@AssemblyNumber IS NULL OR cd.AssemblyNumber LIKE '%' + @AssemblyNumber + '%')
                AND (@ProductionSeries IS NULL OR cd.ProductionSeries = @ProductionSeries)
                AND (@DrawingNumberId IS NULL OR cdi.DrawingNumberId = @DrawingNumberId)
            ORDER BY cd.CreatedDate DESC -- Simplified ordering for performance";

        #endregion

        #region GET ARCHIVE DATA COUNT

        public static readonly string GET_ARCHIVE_DATA_COUNT = @"
            SELECT COUNT(cd.Id) as TotalCount
            FROM tbl_comp_data cd
            LEFT JOIN tbl_comp_data_info cdi ON cd.CompInfoId = cdi.Id
            LEFT JOIN tbl_drawingnumber dn ON cdi.DrawingNumberId = dn.Id
            WHERE cd.IsActive = 1 
                AND (cdi.IsActive = 1 OR cdi.IsActive IS NULL)
                AND (dn.isactive = 1 OR dn.isactive IS NULL)
                AND (@AssemblyNumber IS NULL OR cd.AssemblyNumber LIKE '%' + @AssemblyNumber + '%')
                AND (@ProductionSeries IS NULL OR cd.ProductionSeries = @ProductionSeries)
                AND (@IdNumber IS NULL OR cd.ComponentId LIKE '%' + @IdNumber + '%')
                AND (@DrawingNumberId IS NULL OR cdi.DrawingNumberId = @DrawingNumberId)
                AND (@ProductionSeriesId IS NULL OR cd.ProductionSeriesId = @ProductionSeriesId)
                AND (@AssemblyNumberId IS NULL OR cd.AssemblyNumberId = @AssemblyNumberId)";

        #endregion

        #region GET DROPDOWN OPTIONS

        public static readonly string GET_ASSEMBLY_NUMBERS = @"
            SELECT DISTINCT AssemblyNumber
            FROM tbl_comp_data 
            WHERE IsActive = 1 
                AND AssemblyNumber IS NOT NULL 
                AND AssemblyNumber != ''
            ORDER BY AssemblyNumber";

        public static readonly string GET_PRODUCTION_SERIES = @"
            SELECT DISTINCT ProductionSeries
            FROM tbl_comp_data 
            WHERE IsActive = 1 
                AND ProductionSeries IS NOT NULL 
                AND ProductionSeries != ''
            ORDER BY ProductionSeries";

        #endregion

        #region GET COMPONENT DETAILS BY DRAWING NUMBER

        public static readonly string GET_COMPONENTS_BY_DRAWING_NUMBER = @"
            SELECT 
                cd.Id,
                cd.IDNos,
                cd.IRNos,
                cd.MSNNos,
                cd.ConsumedIn,
                cd.Remarks,
                cd.Quantity,
                cd.MyDate,
                cd.UserName,
                cd.AssemblyId,
                cd.ProductionSeries,
                cd.AssemblyNumber,
                cd.ComponentId,
                dn.drawingnumber,
                ISNULL(nom.nomenclature, 'N/A') as nomenclature,
                cdi.CompTableName
            FROM tbl_comp_data cd
            LEFT JOIN tbl_comp_data_info cdi ON cd.CompInfoId = cdi.Id
            LEFT JOIN tbl_drawingnumber dn ON cdi.DrawingNumberId = dn.Id
            LEFT JOIN tbl_drawingnomenclaturemapping nommap ON dn.Id = nommap.drawingnumberid
            LEFT JOIN tbl_nomenclature nom ON nommap.nomenclatureid = nom.Id
            WHERE cd.IsActive = 1 
                AND (cdi.IsActive = 1 OR cdi.IsActive IS NULL)
                AND (dn.isactive = 1 OR dn.isactive IS NULL)
                AND dn.drawingnumber = @DrawingNumber
            ORDER BY cd.ConsumedIn, cd.ComponentId";

        #endregion

        #region GET CONSUMPTION DETAILS BY ASSEMBLY

        public static readonly string GET_CONSUMPTION_BY_ASSEMBLY = @"
            SELECT 
                cd.AssemblyNumber,
                cd.ProductionSeries,
                cd.ComponentId,
                COUNT(cd.Id) as ComponentCount,
                SUM(cd.Quantity) as TotalQuantity,
                STRING_AGG(DISTINCT dn.drawingnumber, ', ') as DrawingNumbers
            FROM tbl_comp_data cd
            INNER JOIN tbl_comp_data_info cdi ON cd.CompInfoId = cdi.Id
            INNER JOIN tbl_drawingnumber dn ON cdi.DrawingNumberId = dn.Id
            WHERE cd.IsActive = 1 
                AND cdi.IsActive = 1 
                AND dn.isactive = 1
                AND (@AssemblyPattern IS NULL OR cd.AssemblyNumber LIKE '%' + @AssemblyPattern + '%')
                AND (@ProductionSeries IS NULL OR cd.ProductionSeries = @ProductionSeries)
            GROUP BY cd.AssemblyNumber, cd.ProductionSeries, cd.ComponentId
            ORDER BY cd.AssemblyNumber, cd.ProductionSeries, cd.ComponentId";

        #endregion

        #region EXPORT QUERIES

        public static readonly string GET_ARCHIVE_DATA_FOR_EXPORT = @"
            SELECT 
                cd.IDNos as 'PO Number',
                dn.drawingnumber as 'Drawing Number',
                ISNULL(nom.nomenclature, 'N/A') as 'Nomenclature',
                cd.Quantity,
                cd.ComponentId as 'ID Number',
                cd.IRNos as 'IR Number',
                cd.MSNNos as 'MSN Number',
                'Consumed' as Status,
                FORMAT(COALESCE(cd.MyDate, cd.CreatedDate), 'dd-MM-yyyy HH:mm:ss') as 'Created Date',
                cd.AssemblyNumber as 'Assembly Number',
                cd.ProductionSeries as 'Production Series',
                cd.ConsumedIn as 'Consumed In',
                cd.Remarks,
                cd.UserName as 'User Name'
            FROM tbl_comp_data cd
            LEFT JOIN tbl_comp_data_info cdi ON cd.CompInfoId = cdi.Id
            LEFT JOIN tbl_drawingnumber dn ON cdi.DrawingNumberId = dn.Id
            LEFT JOIN tbl_drawingnomenclaturemapping nommap ON dn.Id = nommap.drawingnumberid
            LEFT JOIN tbl_nomenclature nom ON nommap.nomenclatureid = nom.Id
            WHERE cd.IsActive = 1 
                AND (cdi.IsActive = 1 OR cdi.IsActive IS NULL)
                AND (dn.isactive = 1 OR dn.isactive IS NULL)
                AND (@AssemblyNumber IS NULL OR cd.AssemblyNumber LIKE '%' + @AssemblyNumber + '%')
                AND (@ProductionSeries IS NULL OR cd.ProductionSeries = @ProductionSeries)
                AND (@IdNumber IS NULL OR cd.ComponentId LIKE '%' + @IdNumber + '%')
                AND (@DrawingNumberId IS NULL OR cdi.DrawingNumberId = @DrawingNumberId)
            ORDER BY COALESCE(cd.MyDate, cd.CreatedDate) DESC";

        #endregion

        #region STATISTICS QUERIES

        public static readonly string GET_ARCHIVE_STATISTICS = @"
            SELECT 
                COUNT(DISTINCT cdi.CompTableName) as TotalCompTables,
                COUNT(DISTINCT cdi.DrawingNumberId) as TotalDrawingNumbers,
                COUNT(cd.Id) as TotalRecords,
                COUNT(DISTINCT cd.ProductionSeries) as TotalProductionSeries,
                COUNT(DISTINCT cd.AssemblyNumber) as TotalAssemblyNumbers,
                MIN(cd.MyDate) as EarliestRecord,
                MAX(cd.MyDate) as LatestRecord
            FROM tbl_comp_data cd
            INNER JOIN tbl_comp_data_info cdi ON cd.CompInfoId = cdi.Id
            WHERE cd.IsActive = 1 AND cdi.IsActive = 1";

        #endregion
    }
}
