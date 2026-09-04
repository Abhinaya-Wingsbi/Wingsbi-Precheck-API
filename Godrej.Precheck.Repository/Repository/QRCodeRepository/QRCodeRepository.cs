using Azure;
using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DTOs.ConsumedIn;
using Godrej.Precheck.Models.DTOs.Precheck;
using Godrej.Precheck.Models.DTOs.QRCodeDetails;
using Godrej.Precheck.Repository.Database;
using Godrej.Precheck.Repository.Queries;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Repository.Repository.QRCodeRepository
{
    public class QRCodeRepository : IQRCodeRepository
    {
        private readonly ILogger<QRCodeRepository> _logger;

        private readonly IApplicationDbContext _db;

        public QRCodeRepository(ILogger<QRCodeRepository> logger, IApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<QrCodeResponse> InsertQRCodeDetailsAsync(QRCodeDetails qrCodeDetails)
        {
            _logger.LogInformation($"Request for QRCodeRepository:Inserting QR code details: {qrCodeDetails}");
            try
            {
                var indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                if (string.IsNullOrEmpty(qrCodeDetails.QRCodeNumber))
                {
                    qrCodeDetails.QRCodeNumber = indianTime.ToString("yyMMddHHmmssfff");
                }
                
                qrCodeDetails.CreatedDate = indianTime;

                var insertedId = await _db.ExecuteScalar<int>(
                     QRCodeQueries.INSERT_QRCODE_DETAILS_QUERY,
                     new
                     {
                         QRCodeNumber = qrCodeDetails.QRCodeNumber,
                         ProductionSeriesId = qrCodeDetails.ProductionSeriesId,
                         NomenclatureId = qrCodeDetails.NomenclatureId > 0 ? qrCodeDetails.NomenclatureId : (int?)null,
                         ComponentTypeId = qrCodeDetails.ComponentTypeId,
                         DrawingNumberId = qrCodeDetails.DrawingNumberId,
                         IdNumber = qrCodeDetails.IdNumber,
                         Idnumbers = qrCodeDetails.IdNumbers,
                         IRNumberId = qrCodeDetails.IrNumberId,
                         MSNNumberId = qrCodeDetails.MsnNumberId,
                         RefDocRemarks = qrCodeDetails.RefDocRemarks,
                         Quantity = qrCodeDetails.Quantity,
                         RemainingQuantity = qrCodeDetails.Quantity,
                         Desposition = qrCodeDetails.Desposition,
                         ExpiryDate = qrCodeDetails.ExpiryDate,
                         CreatedBy = qrCodeDetails.CreatedBy,
                         CreatedDate = qrCodeDetails.CreatedDate,
                         //ModifiedBy = qrCodeDetails.ModifiedBy,
                         //ModifiedDate = qrCodeDetails.ModifiedDate,
                         UnitId = qrCodeDetails.UnitId > 0 ? qrCodeDetails.UnitId : (int?)null,
                         LnItemCodeId = qrCodeDetails.LnItemCodeId > 0 ? qrCodeDetails.LnItemCodeId : (int?)null,
                         RackLocationId = qrCodeDetails.RackLocationId > 0 ? qrCodeDetails.RackLocationId : (int?)null,
                         OperationNo = qrCodeDetails.OperationNo,
                         ProductionOrderNumber = qrCodeDetails.ProductionOrderNumber,
                         PurchaseOrderNumber = qrCodeDetails.PurchaseOrderNumber,
                         IsActive = 1,
                         QrcodeStatusId = 3,
                         MRIRNumber = qrCodeDetails.MRIRNumber,
                         ManufacturingDate = qrCodeDetails.ManufacturingDate,
                         ProjectDescription = !string.IsNullOrWhiteSpace(qrCodeDetails.Remarks) ? qrCodeDetails.Remarks : qrCodeDetails.Remark,
                         ProjectNumber = qrCodeDetails.ProjectNumber,
                         BuildNumber = qrCodeDetails.BuildNumber

                     });


                _logger.LogInformation("Successfully inserted QR code details: {@QRCodeDetails}", qrCodeDetails);
                return new QrCodeResponse
                {
                    QrCodeId = insertedId,
                    QrCodeNumber = qrCodeDetails.QRCodeNumber
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting QR code details.");
                throw;
            }
        }

        public async Task<QrCodeResponse> InsertStandardQRCodeDetailsAsync(StandardQRCodeDetails qrCodeDetails)
        {
            _logger.LogInformation($"Request for QRCodeRepository:Inserting Standard QR code details: {qrCodeDetails}");
            try
            {
                var indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                qrCodeDetails.QRCodeNumber = indianTime.ToString("yyMMddHHmmssfff");
                qrCodeDetails.CreatedDate = indianTime;

                var insertedId = await _db.ExecuteScalar<int>(
                    QRCodeQueries.INSERT_STANDARD_QRCODE_DETAILS_QUERY,
                    new
                    {
                        QRCodeNumber = qrCodeDetails.QRCodeNumber,
                        ProductionSeriesId = qrCodeDetails.ProductionSeriesId,
                        NomenclatureId = qrCodeDetails.NomenclatureId > 0 ? qrCodeDetails.NomenclatureId : (int?)null,
                        ComponentTypeId = qrCodeDetails.ComponentTypeId,
                        DrawingNumberId = qrCodeDetails.DrawingNumberId,
                        IdNumber = qrCodeDetails.IdNumber,
                        Idnumbers = qrCodeDetails.IdNumbers,
                        IRNumberId = qrCodeDetails.IrNumberId,
                        MSNNumberId = qrCodeDetails.MsnNumberId,
                        RefDocRemarks = qrCodeDetails.RefDocRemarks,
                        Quantity = qrCodeDetails.Quantity,
                        RemainingQuantity = qrCodeDetails.Quantity,
                        Desposition = qrCodeDetails.Desposition,
                        ExpiryDate = qrCodeDetails.ExpiryDate,
                        CreatedBy = qrCodeDetails.CreatedBy,
                        CreatedDate = qrCodeDetails.CreatedDate,
                        UnitId = qrCodeDetails.UnitId > 0 ? qrCodeDetails.UnitId : (int?)null,
                        LnItemCodeId = qrCodeDetails.LnItemCodeId > 0 ? qrCodeDetails.LnItemCodeId : (int?)null,
                        RackLocationId = qrCodeDetails.RackLocationId > 0 ? qrCodeDetails.RackLocationId : (int?)null,
                        OperationNo = qrCodeDetails.OperationNo,
                        ProductionOrderNumber = qrCodeDetails.ProductionOrderNumber,
                        PurchaseOrderNumber = qrCodeDetails.PurchaseOrderNumber,
                        IsActive = 1,
                        QrcodeStatusId = 3,
                        MRIRNumber = qrCodeDetails.MRIRNumber,
                        ManufacturingDate = qrCodeDetails.ManufacturingDate,
                        ProjectDescription = !string.IsNullOrWhiteSpace(qrCodeDetails.Remarks) ? qrCodeDetails.Remarks : qrCodeDetails.ProjectDescription,
                        ProjectNumber = qrCodeDetails.ProjectNumber,
                        PartNo = qrCodeDetails.PartNo,
                        Size = qrCodeDetails.Size,
                        ShapeId = qrCodeDetails.ShapeId,
                        CustomerItemCode = qrCodeDetails.CustomerItemCode,
                        Material = qrCodeDetails.Material,
                        HTLotNo = qrCodeDetails.HTLotNo,
                        FanManNumber = qrCodeDetails.FanManNumber,
                        FanManSerialNumber = qrCodeDetails.FanManSerialNumber,
                        SerialNumberOfQuantity = qrCodeDetails.SerialNumberOfQuantity,
                        MsnIrNumber = qrCodeDetails.MsnIrNumber,
                        GFNNo = qrCodeDetails.GFNNo,
                        SRNo = qrCodeDetails.SrNo,
                        TQty = qrCodeDetails.TQty,
                        WC = qrCodeDetails.WC,
                        ToggleComponentTypeId = qrCodeDetails.ToggleComponentTypeId

                    });

                _logger.LogInformation("Successfully inserted Standard QR code details: {@QRCodeDetails}", qrCodeDetails);
                return new QrCodeResponse
                {
                    QrCodeId = insertedId,
                    QrCodeNumber = qrCodeDetails.QRCodeNumber
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting Standard QR code details.");
                throw;
            }
        }

        public async Task<QrCodeResponse> InsertPrecheckQRCodeDetailsAsync(PrecheckQRCodeRequestDto request)
        {
            try
            {
                _logger.LogInformation("Inserting Precheck QR code details: {@Request}", request);

                TimeZoneInfo indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                var insertedId = await _db.ExecuteScalar<int>(
                    QRCodeQueries.INSERT_PRECHECK_QRCODE_DETAILS_QUERY,
                    new
                    {
                        QRCodeNumber = request.QRCodeNumber,
                        DrawingNumberId = request.DrawingNumberId,
                        ProductionSeriesId = request.ProductionSeriesId,
                        IdNumbers = request.IdNumber,
                        CreatedBy = request.CreatedBy,
                        CreatedDate = indianTime,
                        IsActive = request.IsActive,
                        QrcodeStatusId = 3 // Default status for new QR codes
                    });

                _logger.LogInformation("Successfully inserted Precheck QR code details with ID: {InsertedId}", insertedId);

                return new QrCodeResponse
                {
                    QrCodeId = insertedId,
                    QrCodeNumber = request.QRCodeNumber
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting Precheck QR code details.");
                throw;
            }
        }

        public async Task<QRCodeDetailsResponseDto> GetQRcodeDetailsAsync(string QRCodeNumber, int? qrCodeStatusId = null)
        {
            _logger.LogInformation("Request for QRCodeRepository:GetQRcodeDetailsAsync", QRCodeNumber);

            try
            {
                var results = await _db.GetSingle<QRCodeDetailsResponseDto>(
                    QRCodeQueries.GET_QRCODE_DETAILS_QUERY,
                    new { qrcodenumber = QRCodeNumber, qrcodestatusid = qrCodeStatusId });

                _logger.LogInformation("Successfully retrieved QRCode details", results);

                return results;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetQRcodeDetailsAsync.");
                throw;
            }
        }

        public async Task<StandardQRDetailsResponseDto> GetStandardQRCodeDetailsAsync(string QRCodeNumber)
        {
            _logger.LogInformation("Request for QRCodeRepository:GetStandardQRCodeDetailsAsync", QRCodeNumber);
            try
            {
                var results = await _db.GetSingle<StandardQRDetailsResponseDto>(
                    QRCodeQueries.GET_STANDARD_QRCODE_DETAILS_QUERY,
                    new { qrcodenumber = QRCodeNumber });
                _logger.LogInformation("Successfully retrieved Standard QRCode details", results);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetStandardQRCodeDetailsAsync.");
                throw;
            }
        }

        //Get Componenet storeinByDate

        public async Task<List<QRCodeDetailsResponseDto>> GetComponentByStorInByDate(StoredInQrCodeRequest storeindate)
        {
            _logger.LogInformation("Request for QRCodeRepository:GetComponentByStorInByDate", storeindate);
            try
            {
                var results = await _db.GetAll<QRCodeDetailsResponseDto>(
                    QRCodeQueries.GETSTOREINQRCODEBYDATE,
                    new
                    {
                        storeindate = storeindate.StoreInDate,
                        drawingnumber = storeindate.DrawingNumber // Add this
                    });
                _logger.LogInformation("Successfully retrieved storein qrcodes by date", results);
                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while QRCodeRepository: GetComponentByStorInByDate.");
                throw;
            }
        }


        //GET QRCODE DETAILS WITH PARAMETERS
        public async Task<List<QRCodeDetailsResponseDto>> GetQRcodeWithParameterAsync(GetQRCodeRequestDto getQRCodeRequestDto)
        {
            _logger.LogInformation("Request for QRCodeRepository:GetQRcodeDetailsAsync", getQRCodeRequestDto);

            try
            {
                var results = await _db.GetAll<QRCodeDetailsResponseDto>(
                    QRCodeQueries.GET_QRCODE_DETAILS_With_PARAMETER_QUERY,
                    new { qrcodenumber = getQRCodeRequestDto.QRCodeNumber,
                          prodseriesid= getQRCodeRequestDto.ProdSeriesId,
                          drawingid=getQRCodeRequestDto.DrawingNumberId,
                          createdby= getQRCodeRequestDto.CreatedBy,
                          lnitemcodeid = getQRCodeRequestDto.LnItemCodeId,
                          fromdate = getQRCodeRequestDto.FromDate,
                          todate = getQRCodeRequestDto.ToDate,
                          productionordernumber = getQRCodeRequestDto.ProductionOrderNumber,
                          frombatchid = getQRCodeRequestDto.FromBatchId,
                          tobatchid = getQRCodeRequestDto.ToBatchId,
                          fanmannumber = getQRCodeRequestDto.FanManNumber
                        });


                _logger.LogInformation("Successfully retrieved QRCode details", results);

                return results.ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetQRcodeDetailsAsync.");
                throw;
            }
        }

        public async Task<(List<QRCodeDetailsResponseDto> Items, int TotalCount)> GetBarcodeDetailsWithParametersAsync(
            BarcodeSearchQueryDto? searchQuery, List<string>? prodSeries, int? createdBy, DateTime? fromDate, DateTime? toDate,
            int pageNumber, int pageSize)
        {
            _logger.LogInformation("Request for QRCodeRepository:GetBarcodeDetailsWithParametersAsync");

            try
            {
                var qrFilter = " AND 1=1";
                var drawingFilter = " AND 1=1";
                var itemFilter = " AND 1=1";
                var idNumbersFilter = " AND 1=1";
                var seriesFilter = " AND 1=1";
                var createdByFilter = " AND 1=1";
                var dateFilter = " AND 1=1";

                if (!string.IsNullOrWhiteSpace(searchQuery?.QRCodeNumber))
                {
                    qrFilter = " AND qd.qrcodenumber = @QRCodeNumber";
                }

                if (!string.IsNullOrWhiteSpace(searchQuery?.DrawingNumber))
                {
                    drawingFilter = " AND td.drawingnumber LIKE '%' + @DrawingNumber + '%'";
                }

                if (!string.IsNullOrWhiteSpace(searchQuery?.LineItemCode))
                {
                    itemFilter = " AND li.lnitemcode LIKE '%' + @LineItemCode + '%'";
                }

                var idNumbers = searchQuery?.IdNumbers;
                if (idNumbers != null && idNumbers.Count > 0)
                {
                    idNumbersFilter = " AND qd.idnumber IN @IdNumbers";
                }

                if (prodSeries != null && prodSeries.Count > 0)
                {
                    seriesFilter = " AND ps.productionseries IN @ProdSeries";
                }

                if (createdBy.HasValue)
                {
                    createdByFilter = " AND qd.createdby = @CreatedBy";
                }

                if (fromDate.HasValue || toDate.HasValue)
                {
                    dateFilter = @" AND (@FromDate IS NULL OR CAST(qd.createddate AS DATE) >= CAST(@FromDate AS DATE))
                                    AND (@ToDate IS NULL OR CAST(qd.createddate AS DATE) <= CAST(@ToDate AS DATE))";
                }

                var countQuery = QRCodeQueries.GET_BARCODE_DETAILS_WITH_PARAMETERS_COUNT_QUERY
                    .Replace("{QR_FILTER}", qrFilter)
                    .Replace("{DRAWING_FILTER}", drawingFilter)
                    .Replace("{ITEM_FILTER}", itemFilter)
                    .Replace("{ID_NUMBERS_FILTER}", idNumbersFilter)
                    .Replace("{SERIES_FILTER}", seriesFilter)
                    .Replace("{CREATEDBY_FILTER}", createdByFilter)
                    .Replace("{DATE_FILTER}", dateFilter);

                var pagedQuery = QRCodeQueries.GET_BARCODE_DETAILS_WITH_PARAMETERS_PAGED_QUERY
                    .Replace("{QR_FILTER}", qrFilter)
                    .Replace("{DRAWING_FILTER}", drawingFilter)
                    .Replace("{ITEM_FILTER}", itemFilter)
                    .Replace("{ID_NUMBERS_FILTER}", idNumbersFilter)
                    .Replace("{SERIES_FILTER}", seriesFilter)
                    .Replace("{CREATEDBY_FILTER}", createdByFilter)
                    .Replace("{DATE_FILTER}", dateFilter);

                var queryParams = new
                {
                    QRCodeNumber = searchQuery?.QRCodeNumber,
                    DrawingNumber = searchQuery?.DrawingNumber,
                    LineItemCode = searchQuery?.LineItemCode,
                    IdNumbers = idNumbers,
                    ProdSeries = prodSeries,
                    CreatedBy = createdBy,
                    FromDate = fromDate,
                    ToDate = toDate
                };

                var totalCount = await _db.ExecuteScalar<int>(countQuery, queryParams);

                var offset = (pageNumber - 1) * pageSize;
                var results = await _db.GetAll<QRCodeDetailsResponseDto>(
                    pagedQuery,
                    new
                    {
                        QRCodeNumber = searchQuery?.QRCodeNumber,
                        DrawingNumber = searchQuery?.DrawingNumber,
                        LineItemCode = searchQuery?.LineItemCode,
                        IdNumbers = idNumbers,
                        ProdSeries = prodSeries,
                        CreatedBy = createdBy,
                        FromDate = fromDate,
                        ToDate = toDate,
                        Offset = offset,
                        PageSize = pageSize
                    });

                _logger.LogInformation("Successfully retrieved barcode details, count: {Count}, totalCount: {TotalCount}", results.Count(), totalCount);

                return (results.ToList(), totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetBarcodeDetailsWithParametersAsync.");
                throw;
            }
        }

        public async Task<List<QRCodeDetailsResponseDto>> GetConsumedQRcodeWithParameterAsync(GetQRCodeRequestDto getQRCodeRequestDto)
        {
            _logger.LogInformation("Request for QRCodeRepository:GetConsumedQRcodeWithParameterAsync", getQRCodeRequestDto);

            try
            {
                var results = await _db.GetAll<QRCodeDetailsResponseDto>(
                    QRCodeQueries.GET_CONSUMED_QRCODE_DETAILS_With_PARAMETER_QUERY,
                    new { qrcodenumber = getQRCodeRequestDto.QRCodeNumber,
                          prodseriesid= getQRCodeRequestDto.ProdSeriesId,
                          drawingid=getQRCodeRequestDto.DrawingNumberId,
                          createdby= getQRCodeRequestDto.CreatedBy,
                          lnitemcodeid = getQRCodeRequestDto.LnItemCodeId,
                          fromdate = getQRCodeRequestDto.FromDate,
                          todate = getQRCodeRequestDto.ToDate,
                          productionordernumber = getQRCodeRequestDto.ProductionOrderNumber,
                          frombatchid = getQRCodeRequestDto.FromBatchId,
                          tobatchid = getQRCodeRequestDto.ToBatchId,
                          fanmannumber = getQRCodeRequestDto.FanManNumber
                        });

                _logger.LogInformation("Successfully retrieved consumed QRCode details", results);

                return results.ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetConsumedQRcodeWithParameterAsync.");
                throw;
            }
        }

        public async Task<QRCodeDetailsResponseDto?> GetActiveQRcodeDetailsAsync(string QRCodeNumber)
        {
            _logger.LogInformation("Request for QRCodeRepository:GetActiveQRcodeDetailsAsync", QRCodeNumber);

            try
            {
                var results = await _db.GetSingle<QRCodeDetailsResponseDto?>(
                    QRCodeQueries.GET_QRCODE_DETAILS_QUERY,
                    new { qrcodenumber = QRCodeNumber, qrcodestatusid = (int?)null });

                _logger.LogInformation("Successfully retrieved ActiveQRcode details", results);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving GetActiveQRcodeDetailsAsync");
                throw;
            }
        }

        public async Task<QRCodeDetailsResponseDto?> GetQRcodeDetailsAnyStatusAsync(string QRCodeNumber)
        {
            _logger.LogInformation("Request for QRCodeRepository:GetQRcodeDetailsAnyStatusAsync for QR code: {QRCodeNumber}", QRCodeNumber);

            try
            {
                var results = await _db.GetSingle<QRCodeDetailsResponseDto?>(
                    QRCodeQueries.GET_QRCODE_DETAILS_BY_NUMBER_ANY_STATUS_QUERY,
                    new { qrcodenumber = QRCodeNumber });

                _logger.LogInformation("Successfully retrieved QRCode details including inactive for: {QRCodeNumber}", QRCodeNumber);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetQRcodeDetailsAnyStatusAsync for: {QRCodeNumber}", QRCodeNumber);
                throw;
            }
        }


        public async Task<QRCodeDetailsResponseDto?> GetQRcodeDetails(string QRCodeNumber)
        {
            _logger.LogInformation("Request for QRCodeRepository:GetQRcodeDetailsAnyStatusAsync for QR code: {QRCodeNumber}", QRCodeNumber);

            try
            {
                var results = await _db.GetSingle<QRCodeDetailsResponseDto?>(
                    QRCodeQueries.GET_QRCODE_DETAILS_BY_NUMBER,
                    new { qrcodenumber = QRCodeNumber });

                _logger.LogInformation("Successfully retrieved QRCode details including inactive for: {QRCodeNumber}", QRCodeNumber);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetQRcodeDetailsAnyStatusAsync for: {QRCodeNumber}", QRCodeNumber);
                throw;
            }
        }


        public async Task<bool> InsertQRCodeInConsumptionAsync(QRCodeDetails qrCodeDetails)
        {
            _logger.LogInformation("Request for QRCodeRepository:Inserting InsertQRCodeInConsumptionAsync", qrCodeDetails);
            try
            {
                await _db.Execute(

                    QRCodeQueries.INSERT_QRCODE_IN_CONSUMPTION_QUERY,
                    new
                    {
                        irnumber = qrCodeDetails.IrNumberId,
                        msnnumber = qrCodeDetails.MsnNumberId,
                        componentcodeid = qrCodeDetails.ComponentTypeId,
                        srnumber = qrCodeDetails.SrNumber,
                        idnumber = qrCodeDetails.IdNumber,
                        drawingnumberid = qrCodeDetails.DrawingNumberId,
                        nomenclatureid = qrCodeDetails.NomenclatureId,
                        createdby = qrCodeDetails.CreatedBy,
                        createddate = qrCodeDetails.CreatedDate,
                        //modifiedby = qrCodeDetails.ModifiedBy,
                        //modifieddate = qrCodeDetails.ModifiedDate,
                        prodseriesid = qrCodeDetails.ProductionSeriesId,
                        qrcodenumber = qrCodeDetails.QRCodeNumber,
                        productionordernumber = qrCodeDetails.ProductionOrderNumber,
                        purchaseordernumber = qrCodeDetails.PurchaseOrderNumber,
                        isactive = 1
                    });

                _logger.LogInformation("Successfully inserted QR code details:InsertQRCodeInConsumptionAsync", qrCodeDetails);
                return true;
            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Error occurred while inserting InsertQRCodeInConsumptionAsync");

                throw;

            }

        }

        public async Task<bool> InsertStandardQRCodeInConsumptionAsync(StandardQRCodeDetails qrCodeDetails)
        {
            _logger.LogInformation("Request for QRCodeRepository:Inserting InsertStandardQRCodeInConsumptionAsync", qrCodeDetails);
            try
            {
                await _db.Execute(
                    QRCodeQueries.INSERT_STANDARD_QRCODE_IN_CONSUMPTION_QUERY,
                    new
                    {
                        irnumber = qrCodeDetails.IrNumberId,
                        msnnumber = qrCodeDetails.MsnNumberId,
                        componentcodeid = qrCodeDetails.ComponentTypeId,
                        srnumber = qrCodeDetails.SrNumber,
                        idnumber = qrCodeDetails.IdNumber,
                        drawingnumberid = qrCodeDetails.DrawingNumberId,
                        nomenclatureid = qrCodeDetails.NomenclatureId,
                        createdby = qrCodeDetails.CreatedBy,
                        createddate = qrCodeDetails.CreatedDate,
                        //modifiedby = qrCodeDetails.ModifiedBy,
                        //modifieddate = qrCodeDetails.ModifiedDate,
                        prodseriesid = qrCodeDetails.ProductionSeriesId,
                        qrcodenumber = qrCodeDetails.QRCodeNumber,
                        productionordernumber = qrCodeDetails.ProductionOrderNumber,
                        purchaseordernumber = qrCodeDetails.PurchaseOrderNumber,
                        isactive = 1
                    });
                _logger.LogInformation("Successfully inserted Standard QR code details:InsertStandardQRCodeInConsumptionAsync", qrCodeDetails);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting InsertStandardQRCodeInConsumptionAsync");
                throw;
            }
        }

        public async Task<bool> UpdateQrCodeDetails(string qrCode, string consumedInDrawing, decimal? quantity)
        {

            _logger.LogInformation($"Request for QRCodeRepository:Upadate UpdateQrCodeDetails: {qrCode}");
            try
            {
                var updatedId = await _db.Update(

                     QRCodeQueries.UPDATE_QRCODE_QUERY,
                    new
                    {
                        QrCodeNumber = qrCode,
                        ModifiedBy = 1,
                        ModifiedDate = DateTime.Now,
                        ConsumedInDrawing = consumedInDrawing,
                        Quantity= quantity
                    });

                _logger.LogInformation($"successfully Upadated UpdateQrCodeDetails: {qrCode}");

                return true;
            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Error occurred while inserting QR code details.");

                throw;

            }
        }

        public async Task<bool> ComponentStoreIn(string QRCodeNumber)
        {
            _logger.LogInformation("Request for QRCodeRepository:ComponentStoreIn", QRCodeNumber);
            try
            {
                var indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                var updatedId = await _db.Update(

                     QRCodeQueries.UPDATE_QRCODESTATUS_QUERY,
                    new
                    {
                        qrcodenumber = QRCodeNumber,
                        StoreInDate = indianTime,
                    });

                _logger.LogInformation($"successfully Upadated UpdateQrCodeDetails: {QRCodeNumber}");

                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while ComponentStoreIn.");
                throw;
            }
        }
        public async Task<QRCodeDetailsResponseDto> ValiadateQrCode(int productionseriesid, int idnumbers, int drawingnumberid, string? productionOrderNumber)
        {
            _logger.LogInformation("Request for QRCodeRepository:ValiadateQrCode", productionseriesid, idnumbers, drawingnumberid, productionOrderNumber);

            try
            {
                var results = await _db.GetSingle<QRCodeDetailsResponseDto>(
                    QRCodeQueries.VALIDATE_QRCODE_DETAILS_QUERY,
                    new {
                        ProductionSeriesId = productionseriesid,
                        Idnumbers = idnumbers,
                        DrawingNumberId = drawingnumberid,
                        ProductionOrderNumber = productionOrderNumber
                    });

                _logger.LogInformation("Successfully retrieved QRCode details", results);

                return results;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while ValiadateQrCode.");
                throw;
            }
        }

        public async Task<List<ConsumedInResponseDto>> ConsumedInRepoAsync(ConsumedInRequestDto request)
        {
            _logger.LogInformation("Request for QRCodeRepository:ConsumedInRepoAsync", request);

            try
            {
    
                    var result = await _db.GetAll<ConsumedInResponseDto>(
                        QRCodeQueries.GET_CONSUMEDIN_QUERY,
                        new
                        {
                            productionseriesid = request.ProdSeriesId,
                            idnumber = request.IdNumber,
                            drawingnumberid = request.DrawingNumberId,
                            assemblynumber = request.AssemblyNumber
                        });
                    _logger.LogInformation($"Successfully retrieved ConsumedIn details", result);

                    return result.ToList();
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving QRCodeRepository details:ConsumedInServiceAsync.");
                throw;
            }
        }

        public async Task<string> VerifyIdNumber(string idNumber)
        {
            _logger.LogInformation("Request for QRCodeRepository:VerifyIdNumber", idNumber);

            try
            {
                var results = await _db.GetSingle<string>( 
                    QRCodeQueries.VERIFY_IDNUMBER_QUERY,
                    new { idNumber = idNumber });

                _logger.LogInformation("Successfully VerifyIdNumber", results);

                return results;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while VerifyIdNumber.");
                throw;
            }
        }

        public async Task<string> GetLatestBatchIdNumber()
        {
            _logger.LogInformation("Request for QRCodeRepository:GetLatestBatchIdNumber");
            
            try
            {
                var results = await _db.GetSingle<string>(
                    QRCodeQueries.LATEST_BATCHID_NUMBER,
                    new {  });

                _logger.LogInformation("Successfully retrieved LatestBatchIdNumbers", results);

                return results;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while LatestBatchIdNumbers.");
                throw;
            }
        }
        
        public async Task<List<BatchQRcodeResponse>> GetChildComponenetforAssembly(int DrawingId)
        {
            _logger.LogInformation($"Request for QRCodeRepository:GetChildComponenetforAssembly{DrawingId}");

            try
            {
                var results = await _db.GetAll<BatchQRcodeResponse>(
                    QRCodeQueries.GETBATCHCHILDCOMPONENT,
                    new {
                        DrawingNumberId = DrawingId
                    });

                _logger.LogInformation("Successfully retrieved ChildComponenetforAssembly", results);

                return results.ToList();
                 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while LatestBatchIdNumbers.");
                throw;
            }
        }

        public async Task<List<StandardQRDetailsResponseDto>> GetQRCodesByIdNumbersAsync(List<string> idNumbers)
        {
            _logger.LogInformation("Checking for existing QR codes with ID numbers: {IdNumbers}", string.Join(", ", idNumbers));
            try
            {
                if (idNumbers == null || !idNumbers.Any())
                {
                    return new List<StandardQRDetailsResponseDto>();
                }

                var query = @"
                    SELECT 
                        qd.id,
                        qd.qrcodenumber AS QrCodeNumber,
                        qd.idnumber AS IdNumber,
                        qd.drawingnumberid AS DrawingNumberId,
                        td.drawingnumber AS DrawingNumber
                    FROM tbl_qrcodedetails qd
                    LEFT JOIN tbl_drawingnumber td ON qd.drawingnumberid = td.id
                    WHERE qd.idnumber IN @IdNumbers
                    AND qd.isactive = 1";

                var results = await _db.GetAll<StandardQRDetailsResponseDto>(query, new { IdNumbers = idNumbers });
                
                _logger.LogInformation("Found {Count} existing QR codes with matching ID numbers", results.Count());
                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking for duplicate ID numbers.");
                throw;
            }
        }

        public async Task<List<StandardQRDetailsResponseDto>> GetQRCodesByIdMrirHtCombinationAsync(List<(string IdNo, string Mirir, string HtLotNo,int? LnItemCodeId,int DrawingNumberId)> combinations)
        {
            _logger.LogInformation("Checking for existing QR codes with {Count} ID+MRIR+HT combinations", combinations.Count);
            try
            {
                if (combinations == null || !combinations.Any())
                {
                    return new List<StandardQRDetailsResponseDto>();
                }

                // Build dynamic query to check for each combination
                var query = @"
                    SELECT 
                        qd.id,
                        qd.qrcodenumber AS QrCodeNumber,
                        qd.idnumber AS IdNumber,
                        qd.mrirnumber AS MRIRNumber,
                        qd.htlotno AS HTLotNo,
                        qd.lnitemcodeid AS LnItemCodeId,
                        qd.drawingnumberid AS DrawingNumberId,
                        td.drawingnumber AS DrawingNumber
                    FROM tbl_qrcodedetails qd
                    LEFT JOIN tbl_drawingnumber td ON qd.drawingnumberid = td.id
                    WHERE qd.isactive = 1
                    AND (";

                var conditions = new List<string>();
                var parameters = new Dictionary<string, object>();

                for (int i = 0; i < combinations.Count; i++)
                {
                    var combo = combinations[i];
                    var idParam = $"@IdNo{i}";
                    var mrirParam = $"@Mirir{i}";
                    var htParam = $"@HtLotNo{i}";
                    var lnItemCodeParam = $"@LnItemCodeId{i}";
                    var drawingNumberParam = $"@DrawingNumberId{i}";

                    // Handle NULL values properly in the condition
                    string condition;
                    if (string.IsNullOrWhiteSpace(combo.Mirir) && string.IsNullOrWhiteSpace(combo.HtLotNo))
                    {
                        condition = $"(qd.idnumber = {idParam} AND qd.mrirnumber IS NULL AND qd.htlotno IS NULL AND qd.lnitemcodeid = {lnItemCodeParam} AND qd.drawingnumberid = {drawingNumberParam})";
                        parameters.Add(idParam.TrimStart('@'), combo.IdNo);
                        parameters.Add(lnItemCodeParam.TrimStart('@'), combo.LnItemCodeId);
                        parameters.Add(drawingNumberParam.TrimStart('@'), combo.DrawingNumberId);
                    }
                    else if (string.IsNullOrWhiteSpace(combo.Mirir))
                    {
                        condition = $"(qd.idnumber = {idParam} AND qd.mrirnumber IS NULL AND qd.htlotno = {htParam} AND qd.lnitemcodeid = {lnItemCodeParam} AND qd.drawingnumberid = {drawingNumberParam})";
                        parameters.Add(idParam.TrimStart('@'), combo.IdNo);
                        parameters.Add(htParam.TrimStart('@'), combo.HtLotNo);
                        parameters.Add(lnItemCodeParam.TrimStart('@'), combo.LnItemCodeId);
                        parameters.Add(drawingNumberParam.TrimStart('@'), combo.DrawingNumberId);
                    }
                    else if (string.IsNullOrWhiteSpace(combo.HtLotNo))
                    {
                        condition = $"(qd.idnumber = {idParam} AND qd.mrirnumber = {mrirParam} AND qd.htlotno IS NULL AND qd.lnitemcodeid = {lnItemCodeParam} AND qd.drawingnumberid = {drawingNumberParam})";
                        parameters.Add(idParam.TrimStart('@'), combo.IdNo);
                        parameters.Add(mrirParam.TrimStart('@'), combo.Mirir);
                        parameters.Add(lnItemCodeParam.TrimStart('@'), combo.LnItemCodeId);
                        parameters.Add(drawingNumberParam.TrimStart('@'), combo.DrawingNumberId);
                    }
                    else
                    {
                        condition = $"(qd.idnumber = {idParam} AND qd.mrirnumber = {mrirParam} AND qd.htlotno = {htParam} AND qd.lnitemcodeid = {lnItemCodeParam} AND qd.drawingnumberid = {drawingNumberParam})";
                        parameters.Add(idParam.TrimStart('@'), combo.IdNo);
                        parameters.Add(mrirParam.TrimStart('@'), combo.Mirir);
                        parameters.Add(htParam.TrimStart('@'), combo.HtLotNo);
                        parameters.Add(lnItemCodeParam.TrimStart('@'), combo.LnItemCodeId);
                        parameters.Add(drawingNumberParam.TrimStart('@'), combo.DrawingNumberId);
                    }

                    conditions.Add(condition);
                }

                query += string.Join(" OR ", conditions) + ")";

                var results = await _db.GetAll<StandardQRDetailsResponseDto>(query, parameters);
                
                _logger.LogInformation("Found {Count} existing QR codes with matching ID+MRIR+HT combinations", results.Count());
                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking for duplicate ID+MRIR+HT combinations.");
                throw;
            }
        }

        public async Task<bool> UpdateQRCodeDetailsAsync(UpdateQRCodeDto request)
        {
            _logger.LogInformation("Request for QRCodeRepository:UpdateQRCodeDetailsAsync for QR code: {QRCodeNumber}", request.QRCodeNumber);
            try
            {
                var indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                var rowsAffected = await _db.Update(
                    QRCodeQueries.UPDATE_QRCODE_DETAILS_QUERY,
                    new
                    {
                        QRCodeNumber = request.QRCodeNumber,
                        DrawingNumberId = request.DrawingNumberId,
                        ProductionSeriesId = request.ProductionSeriesId,
                        NomenclatureId = request.NomenclatureId,
                        ComponentTypeId = request.ComponentTypeId,
                        IdNumber = request.IdNumber,
                        IrNumberId = request.IrNumberId,
                        MsnNumberId = request.MsnNumberId,
                        Quantity = request.Quantity,
                        Desposition = request.Desposition,
                        MRIRNumber = request.MRIRNumber,
                        ProductionOrderNumber = request.ProductionOrderNumber,
                        PurchaseOrderNumber = request.PurchaseOrderNumber,
                        Remarks = request.Remarks,
                        ShapeId = request.ShapeId,
                        UnitId = request.UnitId,
                        Size = request.Size,
                        HeatLotBatch = request.HeatLotBatch,
                        ModifiedBy = request.ModifiedBy,
                        ModifiedDate = indianTime
                    });

                _logger.LogInformation("Successfully updated QR code details for: {QRCodeNumber}, rows affected: {RowsAffected}", request.QRCodeNumber, rowsAffected);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating QR code details for: {QRCodeNumber}", request.QRCodeNumber);
                throw;
            }
        }

        public async Task<bool> DisableQRCodeAsync(DisableQRCodeRequestDto request)
        {
            _logger.LogInformation("Request for QRCodeRepository:DisableQRCodeAsync for QR code: {QRCodeNumber}", request.QRCodeNumber);
            try
            {
                var qrCodeAnyStatus = await GetQRcodeDetails(request.QRCodeNumber);

                
                if (qrCodeAnyStatus == null)
                {
                    _logger.LogWarning("QR code does not exist: {QRCodeNumber}", request.QRCodeNumber);
                    throw new ApplicationException($"QR code '{request.QRCodeNumber}' does not exist or already disable.");
                }

                var indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                var rowsAffected = await _db.Update(
                    QRCodeQueries.DISABLE_QRCODE_QUERY,
                    new
                    {
                        QRCodeNumber = request.QRCodeNumber,
                        Remarks = request.Remarks,
                        ModifiedBy = request.ModifiedBy,
                        ModifiedDate = indianTime
                    });

                _logger.LogInformation("Successfully disabled QR code: {QRCodeNumber}, rows affected: {RowsAffected}", request.QRCodeNumber, rowsAffected);

                if (rowsAffected <= 0)
                {
                    throw new ApplicationException($"Failed to disable QR code '{request.QRCodeNumber}'.");
                }

                return rowsAffected > 0;
            }
            catch (ApplicationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while disabling QR code: {QRCodeNumber}", request.QRCodeNumber);
                throw;
            }
        }

        public async Task<bool> IsStandardQRCode(string qrCodeNumber)
        {
            _logger.LogInformation("Request for QRCodeRepository:IsStandardQRCode for QR code: {QRCodeNumber}", qrCodeNumber);
            try
            {
                // Check if QR code has Standard-specific fields populated
                // Standard QR codes have togglecomponenttypeid, shapeid, or partno fields populated
                var query = @"
                    SELECT 
                        CASE 
                            WHEN togglecomponenttypeid IS NOT NULL 
                                OR shapeid IS NOT NULL 
                                OR partno IS NOT NULL 
                            THEN 1 
                            ELSE 0 
                        END AS IsStandard
                    FROM tbl_qrcodedetails 
                    WHERE qrcodenumber = @QRCodeNumber AND isactive = 1";

                var result = await _db.GetSingle<int>(query, new { QRCodeNumber = qrCodeNumber });
                
                bool isStandard = result == 1;
                _logger.LogInformation("QR code {QRCodeNumber} is {Type} type", qrCodeNumber, isStandard ? "Standard" : "Manufacturing");
                
                return isStandard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if QR code is Standard type: {QRCodeNumber}", qrCodeNumber);
                throw;
            }
        }

        public async Task<bool> CheckPreviousBatchExists(int drawingNumberId, int idNumbers)
        {
            var result = await _db.GetSingle<bool>(
                QRCodeQueries.CHECK_PREVIOUS_BATCH_EXISTS,
                new
                {
                    drawingNumberId = drawingNumberId,
                    idNumbers = idNumbers
                });

            return result;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            _logger.LogInformation("Request for UserRepository:GetAllUsersAsync");

            try
            {
                var results = await _db.GetAll<UserDto>(
                    QRCodeQueries.GET_ALL_USERS_QUERY,
                    new { });

               // _logger.LogInformation("Successfully retrieved all active users. Count: {Count}", results.Count);

                return results?.ToList() ?? new List<UserDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving GetAllUsersAsync");
                throw;
            }
        }

        public async Task<List<string>> GetDistinctBatchIdNumbersAsync()
        {
            _logger.LogInformation("Request for QRCodeRepository:GetDistinctBatchIdNumbersAsync");

            try
            {
                var results = await _db.GetAll<string>(
                    QRCodeQueries.GET_DISTINCT_BATCH_ID_NUMBERS_QUERY, new { });

                _logger.LogInformation("Successfully retrieved {Count} distinct batch ID numbers", results.Count());

                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetDistinctBatchIdNumbersAsync.");
                throw;
            }
        }

        public async Task<List<string>> GetAllFanManSerialNumbersAsync()
        {
            _logger.LogInformation("Request for QRCodeRepository:GetAllFanManSerialNumbersAsync");

            try
            {
                var results = await _db.GetAll<string>(
                    QRCodeQueries.GET_FANMAN_SERIAL_NUMBERS_QUERY, new { });

                _logger.LogInformation("Successfully retrieved {Count} distinct fan man serial numbers", results.Count());

                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetAllFanManSerialNumbersAsync.");
                throw;
            }
        }

        public async Task<List<ConsumedInResponseDto>> ExportConsumedInRepoAsync(ConsumedInRequestDto request)
        {
            _logger.LogInformation("Request for QRCodeRepository: ExportConsumedInRepoAsync");

            try
            {
                var result = await _db.GetAll<ConsumedInResponseDto>(
                    QRCodeQueries.GET_CONSUMEDIN_QUERY,
                    new
                    {
                        productionseriesid = request.ProdSeriesId,
                        idnumber = request.IdNumber,
                        drawingnumberid = request.DrawingNumberId,
                        assemblynumber = request.AssemblyNumber
                    });

                _logger.LogInformation("Successfully retrieved {Count} records for export", result?.Count() ?? 0);

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting ConsumedIn data from repository.");
                throw;
            }
        }

        public async Task<int> BulkUpdateQRCodeAsync(BulkUpdateQRCodeRequestDto request)
        {
            _logger.LogInformation("Request for QRCodeRepository: BulkUpdateQRCodeRepoAsync");

            try
            {
                var result = await _db.Execute(
                    QRCodeQueries.BULK_UPDATE_QRCODE_QUERY,
                    new
                    {
                        qrcodenumbers = request.QrCodeNumbers,
                        mrirnumber = request.MRIRNumber,
                        irnumberid = request.IRNumberId,
                        msnnumberid = request.MSNNumberId,
                        projectnumber = request.ProjectNumber,
                        size = request.Size,
                        heatlotnumber = request.HeatLotNumber,
                        lnitemcodeid = request.LnItemCodeId,
                        drawingnumberid = request.DrawingNumberId,
                        productionseriesid = request.ProductionSeriesId,
                        fanmannumber = request.FanManNumber,
                        fanmanserialnumber = request.FanManSerialNumber,
                        racklocationid = request.RackLocationId,
                        unitid = request.UnitId,
                        idnumber = request.IdNumber,
                        quantity = request.Quantity
                    });

                _logger.LogInformation("Successfully updated {Count} QR Codes", result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while bulk updating QR Codes.");
                throw;
            }
        }

        public async Task<List<GetAvailableComponentsResponse>> GetAvailableQr(GetAvailableQrRequest request)
        {
            _logger.LogInformation($"Request for QRCodeRepository:GetAvailableQr LnItemCode: {request.LnItemCode}, DrawingNumber: {request.DrawingNumber}");

            try
            {
                var results = await _db.GetAll<GetAvailableComponentsResponse>(
                    QRCodeQueries.GET_AVAILABLE_QR_BY_LNITEM_DRAWING,
                    new
                    {
                        LnItemCode = request.LnItemCode,
                        DrawingNumber = request.DrawingNumber,
                        ProdSeriesId = request.ProdSeriesId,
                        QrType = request.QrType
                    });

                _logger.LogInformation("Successfully retrieved GetAvailableQr", results);

                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetAvailableQr.");
                throw;
            }
        }
    }
}
