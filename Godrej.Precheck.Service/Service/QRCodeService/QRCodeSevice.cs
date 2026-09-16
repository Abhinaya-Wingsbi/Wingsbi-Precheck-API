using ClosedXML.Excel;
using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DataModel.Precheck;
using Godrej.Precheck.Models.DTOs.Barcode;
using Godrej.Precheck.Models.DTOs.ConsumedIn;
using Godrej.Precheck.Models.DTOs.Precheck;
using Godrej.Precheck.Models.DTOs.QRCodeDetails;
using Godrej.Precheck.Repository.Repository.CommonRepository;
using Godrej.Precheck.Repository.Repository.PrecheckRepository;
using Godrej.Precheck.Repository.Repository.QRCodeRepository;
using Godrej.Precheck.Service.Service.CommonSevice;
using Godrej.Precheck.Service.Service.PrecheckService;
using Mapster;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Godrej.Precheck.Models.DataModel.Validation;

namespace Godrej.Precheck.Service.Service.QRCodeService
{
    public class QRCodeSevice : IQRCodeService
    {
        private readonly ILogger<QRCodeSevice> _logger;

        private readonly IQRCodeRepository _qrCodeRepository;

        private readonly ICommonRepository _commonRepository;

        private readonly ICommonService _commonService;
        private readonly IPrecheckService _precheckService;

        private readonly IPrecheckRepository _precheckRepository;
        public QRCodeSevice(ILogger<QRCodeSevice> logger, IQRCodeRepository qrCodeRepository, ICommonRepository commonRepository, ICommonService commonService, IPrecheckService precheckService, IPrecheckRepository precheckRepository)
        {
            _qrCodeRepository = qrCodeRepository;
            _logger = logger;
            _commonRepository = commonRepository;
            _precheckService = precheckService;
            _commonService = commonService;
            _precheckRepository = precheckRepository;
        }
        public async Task<List<QRCodeDetailsResponseDto?>> InsertQRCodeDetailsAsync(QRCodeDetailsDto qrCodeDetailsDto)
        {
            try
            {
                _logger.LogInformation("Starting InsertQRCodeDetailsAsync. Input: {@QRCodeDetailsDto}", qrCodeDetailsDto);

                var qrCodeDetails = qrCodeDetailsDto.Adapt<QRCodeDetails>();
                var qrCodeDetailsResponses = new List<QRCodeDetailsResponseDto?>();

                _logger.LogInformation("Fetching component type for ComponentTypeId: {ComponentTypeId}", qrCodeDetails.ComponentTypeId);

                var componentTypeResponse = await _commonRepository.GetComponentTypeByIdAsync(qrCodeDetails.ComponentTypeId);

                _logger.LogInformation("Fetching production series details for ProductionSeriesId: {ProductionSeriesId}", qrCodeDetails.ProductionSeriesId);
                var prodSeriesDetail = await _commonRepository.GetProductionSeriesById(qrCodeDetails.ProductionSeriesId);

                _logger.LogInformation("Fetching all drawing numbers.");

                var drawingDetails = await _commonService.GetAllDrawingNumberService();
                var selectedDrawingNumber = drawingDetails.FirstOrDefault(x => x.Id == qrCodeDetails.DrawingNumberId);

                if (selectedDrawingNumber == null)
                {
                    _logger.LogWarning("Drawing number not found for ID: {DrawingNumberId}", qrCodeDetails.DrawingNumberId);
                    throw new Exception("Invalid drawing number ID");
                }

                qrCodeDetails.LnItemCode = selectedDrawingNumber.LnItemCode;
                qrCodeDetails.LnItemCodeId = selectedDrawingNumber.LnItemCodeId;

                _logger.LogInformation("Processing QR codes for ComponentType: {ComponentType}", componentTypeResponse.ComponentType);
                switch (componentTypeResponse.ComponentType)
                {
                    case "ID":
                        if (!string.IsNullOrEmpty(qrCodeDetails.CustomIdRange))
                        {
                            _logger.LogInformation("Processing custom ID range: {CustomIdRange}", qrCodeDetails.CustomIdRange);

                            // Parse custom ID range (format: "2,3,4,5,6-10" or similar)
                            var customIds = ParseCustomIdRange(qrCodeDetails.CustomIdRange);
                            qrCodeDetails.Ids = customIds;
                            // Note: Quantity will be set to 1 for each individual QR code record in the loop below

                            _logger.LogInformation("Parsed custom ID range into {Count} IDs: {Ids}", customIds.Count, string.Join(",", customIds));
                        }

                        foreach (int id in qrCodeDetails.Ids)
                        {
                            //for CB components qrcode code should not be generated before precheck
                            if (componentTypeResponse.ComponentType == "ID")
                            {
                                // Raw material: LnItemCode not starting with "WJD" OR DrawingNumber starting with "RM" -> QR generation always allowed, no precheck-completeness check.
                                bool isRawMaterial = !(selectedDrawingNumber.LnItemCode?.StartsWith("WJD") ?? false)
                                    || selectedDrawingNumber.DrawingNumber.StartsWith("RM");

                                if (!isRawMaterial && selectedDrawingNumber.DrawingNumber.Contains("CB"))
                                {
                                    ViewPreCheckRequestDto viewPreCheckRequestDto = new ViewPreCheckRequestDto()
                                    {
                                        DrawingNumberId = qrCodeDetails.DrawingNumberId,
                                        ProductionSeriesId = qrCodeDetails.ProductionSeriesId,
                                        ProductionOrderNumber = qrCodeDetails.ProductionOrderNumber,
                                        Id = id
                                    };

                                    var precheckDetails = await _precheckService.GetPrecheckStatusDetailsService(viewPreCheckRequestDto);
                                    if (precheckDetails == null)
                                    {
                                        _logger.LogWarning($"Precheck details not found for ID or Order is not generated for Id : {id}");
                                        throw new ValidationException($"Order is not created for Id:{id}");
                                    }
                                    else if (precheckDetails != 3)
                                    {
                                        var preCheckRequest = new ViewPreCheckRequest()
                                        {
                                            DrawingNumberId = qrCodeDetails.DrawingNumberId,
                                            ProductionSeriesId = qrCodeDetails.ProductionSeriesId,
                                            ProductionOrderNumber = qrCodeDetails.ProductionOrderNumber,
                                            Id = id
                                        };

                                        var response = await _precheckRepository.ViewPrecheckDetails(preCheckRequest);

                                        if (response == null || !response.Any())
                                        {
                                            _logger.LogWarning($"No precheck details returned from repository for ID: {id}");
                                            throw new ValidationException($"Precheck details not found for ID: {id}");
                                        }

                                        var unsubmitedComponents = response
                                            .Where(p => p.IsPrecheckComplete == false)
                                            .Select(p => new UnsubmittedComponent
                                            {
                                                DrawingNumber = p.DrawingNumber
                                            })
                                            .ToList();

                                        if (unsubmitedComponents.Any())
                                        {
                                            var error = new Validation.PrecheckValidationError
                                            {
                                                Error = $"Precheck is not completed for the following components for Id {id}",
                                                UnsubmitedComponents = unsubmitedComponents
                                            };

                                            string jsonError = JsonSerializer.Serialize(error);
                                            _logger.LogWarning($"Incomplete precheck components found for ID: {id} - {jsonError}");
                                            throw new ValidationException(jsonError);
                                        }
                                    }
                                }
                            }
                        }
                        foreach (int id in qrCodeDetails.Ids)
                        {


                            if (componentTypeResponse.ComponentType == "ID")
                            {
                                qrCodeDetails.Quantity = 1;
                                qrCodeDetails.SrNumber = id;
                                qrCodeDetails.IdNumbers = id;
                                qrCodeDetails.RemainingQuantity = qrCodeDetails.Quantity;
                                qrCodeDetails.IdNumber = $"{prodSeriesDetail.ProductionSeries}/{id}";

                                // Reset QRCodeNumber to null so each QR code gets a unique timestamp
                                qrCodeDetails.QRCodeNumber = null;
                            }

                            var validationResponse = await _qrCodeRepository.ValidateQrCode(
                                qrCodeDetails.ProductionSeriesId,
                                qrCodeDetails.IdNumbers,
                                qrCodeDetails.DrawingNumberId,
                                qrCodeDetails.ProductionOrderNumber);

                            if (validationResponse != null)
                            {
                                _logger.LogInformation("QR code validation failed for ID: {Id}. Skipping generation.", id);
                                validationResponse.IsNewQrCode = false;
                                qrCodeDetailsResponses.Add(validationResponse);
                                continue;
                            }
                            else
                            {
                                var qrcodeResponse = await _qrCodeRepository.InsertQRCodeDetailsAsync(qrCodeDetails);

                                await _qrCodeRepository.InsertQRCodeInConsumptionAsync(qrCodeDetails);

                                var qrCodeDetailsResponse = await _qrCodeRepository.GetQRcodeDetailsAsync(qrcodeResponse.QrCodeNumber);
                                qrCodeDetailsResponse.IsNewQrCode = true;
                                qrCodeDetailsResponses.Add(qrCodeDetailsResponse);

                                _logger.LogInformation("Processed QR code details for ID: {Id}", id);
                            }
                        }
                        break;

                    case "FIM":
                    case "SI":
                        {


                            if (componentTypeResponse.ComponentType == "FIM")
                            {
                                qrCodeDetails.IdNumber = "FIM";
                            }
                            else
                            {
                                qrCodeDetails.IdNumber = "SI";
                            }

                            qrCodeDetails.RemainingQuantity = qrCodeDetails.Quantity;
                            var qrcodeResponse = await _qrCodeRepository.InsertQRCodeDetailsAsync(qrCodeDetails);

                            await _qrCodeRepository.InsertQRCodeInConsumptionAsync(qrCodeDetails);

                            var qrCodeDetailsResponse = await _qrCodeRepository.GetQRcodeDetailsAsync(qrcodeResponse.QrCodeNumber);
                            qrCodeDetailsResponse.IsNewQrCode = true;
                            qrCodeDetailsResponses.Add(qrCodeDetailsResponse);

                            _logger.LogInformation("Processed QR code details for ComponentType: {ComponentType}", componentTypeResponse.ComponentType);
                        }
                        break;

                    case "BATCH":
                        {
                            string lastIdNumber = await _qrCodeRepository.GetLatestBatchIdNumber();
                            int lastCounter = 0;

                            if (!string.IsNullOrEmpty(lastIdNumber))
                            {
                                var parts = lastIdNumber.Split('-');
                                if (parts.Length == 2)
                                    int.TryParse(parts[1], out lastCounter);
                            }

                            int batchCounter = lastCounter + 1;

                            var indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                            var indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                            string qrNumber = indianTime.AddMilliseconds(batchCounter)
                                                        .ToString("yyMMddHHmmssfff");

                            qrCodeDetails.QRCodeNumber = qrNumber;

                            qrCodeDetails.SrNumber = batchCounter;
                            qrCodeDetails.IdNumbers = batchCounter;
                            qrCodeDetails.IdNumber = $"BATCH-{batchCounter}";

                            qrCodeDetails.RemainingQuantity = qrCodeDetails.Quantity;

                            if (!string.IsNullOrEmpty(qrCodeDetailsDto.Remarks))
                            {
                                qrCodeDetails.Remarks = qrCodeDetailsDto.Remarks;
                            }

                            var qrcodeResponse =
                                await _qrCodeRepository.InsertQRCodeDetailsAsync(qrCodeDetails);

                            await _qrCodeRepository.InsertQRCodeInConsumptionAsync(qrCodeDetails);

                            var qrCodeDetailsResponse =
                                await _qrCodeRepository.GetQRcodeDetailsAsync(qrcodeResponse.QrCodeNumber);

                            qrCodeDetailsResponse.IsNewQrCode = true;

                            qrCodeDetailsResponses.Add(qrCodeDetailsResponse);

                            _logger.LogInformation("Completed processing batch QR codes.");
                        }
                        break;
                }

                _logger.LogInformation("Successfully processed QR code details: {@QRCodeDetails}", qrCodeDetails);
                return qrCodeDetailsResponses;
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "Validation error occurred while InsertQRCodeDetailsAsync:", vex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting QR code details : InsertQRCodeDetailsAsync.");
                throw;
            }
        }

        public async Task<List<StandardQRDetailsResponseDto?>> InsertStandardQRCodeDetailsAsync(StandardQRDataDto qrCodeDetailsDto)
        {
            try
            {
                _logger.LogInformation("Starting InsertQRCodeDetailsAsync. Input: {@StandardQRDataDto}", qrCodeDetailsDto);

                var qrCodeDetails = qrCodeDetailsDto.Adapt<StandardQRCodeDetails>();
                var qrCodeDetailsResponses = new List<StandardQRDetailsResponseDto?>();

                _logger.LogInformation("Fetching component type for ComponentTypeId: {ComponentTypeId}", qrCodeDetails.ComponentTypeId);

                var componentTypeResponse = await _commonRepository.GetComponentTypeByIdAsync(qrCodeDetails.ComponentTypeId);

                if (componentTypeResponse == null)
                {
                    _logger.LogWarning("Component type not found for ComponentTypeId: {ComponentTypeId}", qrCodeDetails.ComponentTypeId);
                    throw new ValidationException($"ComponentType not found for ID: {qrCodeDetails.ComponentTypeId}");
                }

                _logger.LogInformation("Fetching production series details for ProductionSeriesId: {ProductionSeriesId}", qrCodeDetails.ProductionSeriesId);

                var prodSeriesDetail = await _commonRepository.GetProductionSeriesById(qrCodeDetails.ProductionSeriesId);

                _logger.LogInformation("Fetching all drawing numbers.");

                var drawingDetails = await _commonService.GetAllDrawingNumberService();
                var selectedDrawingNumber = drawingDetails.FirstOrDefault(x => x.Id == qrCodeDetails.DrawingNumberId);

                if (selectedDrawingNumber == null)
                {
                    _logger.LogWarning("Drawing number not found for ID: {DrawingNumberId}", qrCodeDetails.DrawingNumberId);
                    throw new Exception("Invalid drawing number ID");
                }

                qrCodeDetails.LnItemCode = selectedDrawingNumber.LnItemCode;
                qrCodeDetails.LnItemCodeId = selectedDrawingNumber.LnItemCodeId;

                // Ensure UnitId is always populated - fallback to drawing's default unit
                if (!qrCodeDetails.UnitId.HasValue || qrCodeDetails.UnitId <= 0)
                {
                    qrCodeDetails.UnitId = selectedDrawingNumber.UnitId;
                }

                // Ensure production order number persists from payload
                if (string.IsNullOrWhiteSpace(qrCodeDetails.ProductionOrderNumber))
                {
                    qrCodeDetails.ProductionOrderNumber = qrCodeDetailsDto.ProductionOrderNumber;
                }

                if (string.IsNullOrWhiteSpace(qrCodeDetails.PurchaseOrderNumber))
                {
                    qrCodeDetails.PurchaseOrderNumber = qrCodeDetailsDto.PurchaseOrderNumber;
                }

                _logger.LogInformation("Processing QR codes using standard FIM/SI logic for all ComponentTypes: {ComponentType}", componentTypeResponse.ComponentType);

                var hasMatrixRows = qrCodeDetailsDto.MatrixRows != null && qrCodeDetailsDto.MatrixRows.Any();

                if (hasMatrixRows)
                {
                    int totalRows = qrCodeDetailsDto.MatrixRows.Count;
                    _logger.LogInformation("Processing {Count} matrix rows for QR code generation", totalRows);

                    // Validate: Check for duplicate (ID + MRIR + HT/BT) combinations within the matrix rows
                    var combinations = qrCodeDetailsDto.MatrixRows
                        .Where(r => !string.IsNullOrWhiteSpace(r.IdNo))
                        .Select(r => (
                            IdNo: r.IdNo!.Trim(),
                            Mirir: r.Mirir?.Trim() ?? "",
                            HtLotNo: r.HeatLotBatchNo?.Trim() ?? "",
                            LnItemCodeId: qrCodeDetailsDto.LnItemCodeId,
                            DrawingNumberId: qrCodeDetailsDto.DrawingNumberId
                        ))
                        .ToList();

                    var duplicateCombos = combinations
                        .GroupBy(c => new { c.IdNo, c.Mirir, c.HtLotNo,c.LnItemCodeId,c.DrawingNumberId })
                        .Where(g => g.Count() > 1)
                        .Select(g => $"ID: {g.Key.IdNo}, MRIR: {(string.IsNullOrEmpty(g.Key.Mirir) ? "NULL" : g.Key.Mirir)}, HT/BT: {(string.IsNullOrEmpty(g.Key.HtLotNo) ? "NULL" : g.Key.HtLotNo)}, LnItemCodeId: {g.Key.LnItemCodeId}, DrawingNumberId: {g.Key.DrawingNumberId}")
                        .ToList();

                    if (duplicateCombos.Any())
                    {
                        var duplicatesList = string.Join("; ", duplicateCombos);
                        _logger.LogWarning("Duplicate ID+MRIR+HT/BT combinations found in matrix rows: {Duplicates}", duplicatesList);
                        throw new ValidationException($"Duplicate combinations found in the matrix: {duplicatesList}. Each combination of ID Number, MRIR Number, and HT/BT must be unique.");
                    }
                    
                    var existingQrCodes = await _qrCodeRepository.GetQRCodesByIdMrirHtCombinationAsync(combinations);
                    if (existingQrCodes != null && existingQrCodes.Any())
                    {
                        var existingCombos = existingQrCodes
                            .Select(qr => $"ID: {qr.IdNumber}, MRIR: {(string.IsNullOrEmpty(qr.MRIRNumber) ? "NULL" : qr.MRIRNumber)}, HT/BT: {(string.IsNullOrEmpty(qr.HTLotNo) ? "NULL" : qr.HTLotNo)} (QR: {qr.QrCodeNumber}), LnItemCodeId: {qr.LnItemCodeId}, DrawingNumberId: {qr.DrawingNumberId}")
                            .ToList();
                        var existingList = string.Join("; ", existingCombos);
                        _logger.LogWarning("ID+MRIR+HT/BT/LnItemCodeId/DrawingNumberId combinations already exist in database: {ExistingCombos}", existingList);
                        throw new ValidationException($"The following combinations already exist in the system: {existingList}. Please use unique combinations of ID Number, MRIR Number, HT/BT, LnItemCodeId, and DrawingNumberId.");
                    }


                    for (int index = 0; index < totalRows; index++)
                    {
                        var matrixRow = qrCodeDetailsDto.MatrixRows[index];
                        var perQrCodeDetails = qrCodeDetails.Adapt<StandardQRCodeDetails>();

                        perQrCodeDetails.IdNumber = matrixRow.IdNo ?? "";
                        perQrCodeDetails.Size = matrixRow.Size ?? "";
                        perQrCodeDetails.MRIRNumber = matrixRow.Mirir ?? "";
                        perQrCodeDetails.HTLotNo = matrixRow.HeatLotBatchNo ?? "";
                        perQrCodeDetails.SrNo = matrixRow.SrNo.ToString();
                        perQrCodeDetails.SrNumber = matrixRow.SrNo;
                        // Use the quantity from the matrix row if provided, otherwise default to 1
                        perQrCodeDetails.Quantity = matrixRow.Quantity > 0 ? matrixRow.Quantity : 1;
                        perQrCodeDetails.SerialNumberOfQuantity = $"{index + 1}/{totalRows}";

                        var qrcodeResponse = await _qrCodeRepository.InsertStandardQRCodeDetailsAsync(perQrCodeDetails);
                        await _qrCodeRepository.InsertStandardQRCodeInConsumptionAsync(perQrCodeDetails);

                        // Auto store-in standard QR codes
                        await _qrCodeRepository.ComponentStoreIn(qrcodeResponse.QrCodeNumber);

                        var qrCodeDetailsResponse = await _qrCodeRepository.GetStandardQRCodeDetailsAsync(qrcodeResponse.QrCodeNumber);
                        qrCodeDetailsResponse.IsNewQrCode = true;
                        qrCodeDetailsResponse.SrNo = perQrCodeDetails.SrNo;
                        qrCodeDetailsResponse.SerialNumberOfQuantity = perQrCodeDetails.SerialNumberOfQuantity;
                        qrCodeDetailsResponse.IdNumber = perQrCodeDetails.IdNumber;
                        qrCodeDetailsResponses.Add(qrCodeDetailsResponse);
                    }

                    _logger.LogInformation("Processed {Count} matrix row QR codes for drawing {DrawingNumberId}", totalRows, qrCodeDetails.DrawingNumberId);
                    return qrCodeDetailsResponses;
                }

                var isIdComponent = string.Equals(componentTypeResponse.ComponentType, "ID", StringComparison.OrdinalIgnoreCase);
                var providedIds = qrCodeDetailsDto.Ids?
                    .Where(id => id > 0)
                    .ToList();

                if (isIdComponent && providedIds != null && providedIds.Any())
                {
                    int totalIds = providedIds.Count;

                    for (int index = 0; index < totalIds; index++)
                    {
                        var idValue = providedIds[index];
                        var perQrCodeDetails = qrCodeDetails.Adapt<StandardQRCodeDetails>();

                        perQrCodeDetails.IdNumber = idValue.ToString();
                        perQrCodeDetails.IdNumbers = idValue;
                        perQrCodeDetails.Quantity = 1;
                        perQrCodeDetails.SerialNumberOfQuantity = $"{index + 1}/{totalIds}";
                        perQrCodeDetails.SrNo = (index + 1).ToString();
                        perQrCodeDetails.SrNumber = index + 1;

                        var qrcodeResponse = await _qrCodeRepository.InsertStandardQRCodeDetailsAsync(perQrCodeDetails);
                        await _qrCodeRepository.InsertStandardQRCodeInConsumptionAsync(perQrCodeDetails);

                        // Auto store-in standard QR codes
                        await _qrCodeRepository.ComponentStoreIn(qrcodeResponse.QrCodeNumber);

                        var qrCodeDetailsResponse = await _qrCodeRepository.GetStandardQRCodeDetailsAsync(qrcodeResponse.QrCodeNumber);
                        qrCodeDetailsResponse.IsNewQrCode = true;
                        qrCodeDetailsResponse.SrNo = perQrCodeDetails.SrNo;
                        qrCodeDetailsResponse.SerialNumberOfQuantity = perQrCodeDetails.SerialNumberOfQuantity;
                        qrCodeDetailsResponse.IdNumber = perQrCodeDetails.IdNumber;
                        qrCodeDetailsResponses.Add(qrCodeDetailsResponse);
                    }

                    _logger.LogInformation("Processed {Count} ID QR codes for drawing {DrawingNumberId}", providedIds.Count, qrCodeDetails.DrawingNumberId);
                    return qrCodeDetailsResponses;
                }

                qrCodeDetails.IdNumber = componentTypeResponse.ComponentType; // Can be hardcoded to "FIM" or "SI" if desired

                var singleQrCodeResponse = await _qrCodeRepository.InsertStandardQRCodeDetailsAsync(qrCodeDetails);

                await _qrCodeRepository.InsertStandardQRCodeInConsumptionAsync(qrCodeDetails);

                // Auto store-in standard QR codes
                await _qrCodeRepository.ComponentStoreIn(singleQrCodeResponse.QrCodeNumber);

                var standardQrCodeDetailsResponse = await _qrCodeRepository.GetStandardQRCodeDetailsAsync(singleQrCodeResponse.QrCodeNumber);
                standardQrCodeDetailsResponse.IsNewQrCode = true;
                qrCodeDetailsResponses.Add(standardQrCodeDetailsResponse);

                _logger.LogInformation("Processed QR code details using standard logic for ComponentType: {ComponentType}", componentTypeResponse.ComponentType);

                _logger.LogInformation("Successfully processed QR code details: {@QRCodeDetails}", qrCodeDetails);

                return qrCodeDetailsResponses;
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "Validation error occurred while InsertQRCodeDetailsAsync: {Message}", vex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting QR code details: InsertQRCodeDetailsAsync.");
                throw;
            }
        }


        public async Task<QRCodeDetailsResponseDto> GetQRCodeDetailsService(string QRCodeNumber, int? qrCodeStatusId = null)
        {
            try
            {
                _logger.LogInformation("Fetching QR code details for code: {QRCodeNumber}", QRCodeNumber);

                var result = await _qrCodeRepository.GetQRcodeDetailsAsync(QRCodeNumber, qrCodeStatusId);

                if (result == null)
                {
                    return null;
                }

                if (result.ExpiryDate != null)
                {
                    bool batchExists = await _qrCodeRepository
                .CheckPreviousBatchExists(result.DrawingNumberId, result.IdNumbers);
                    result.BatchAvailable = batchExists;
                    _logger.LogInformation("Successfully fetched QR code details for: {QRCodeNumber}", QRCodeNumber);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching QR code details for: {QRCodeNumber}", QRCodeNumber);
                throw;
            }
        }

        public async Task<List<QRCodeDetailsResponseDto>> GetQRCodeDetailsWithParameterService(GetQRCodeRequestDto getQRCodeRequest)
        {
            try
            {
                _logger.LogInformation("Fetching QR code details for request :", getQRCodeRequest);

                var result = await _qrCodeRepository.GetQRcodeWithParameterAsync(getQRCodeRequest);

                _logger.LogInformation("Successfully fetched QR code details for request :", getQRCodeRequest);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching QR code details for request", getQRCodeRequest);
                throw;
            }
        }

        public async Task<QRCodeDetailsPagedResponse> GetBarcodeDetailsWithParametersService(
            string? searchQuery, List<string>? prodSeries, List<int>? createdBy, DateTime? fromDate, DateTime? toDate,
            int pageNumber, int? pageSize)
        {
            try
            {
                _logger.LogInformation("Fetching barcode details with parameters");

                var (items, totalCount) = await _qrCodeRepository.GetBarcodeDetailsWithParametersAsync(
                    searchQuery, prodSeries, createdBy, fromDate, toDate, pageNumber, pageSize);

                _logger.LogInformation("Successfully fetched barcode details with parameters, count: {Count}, totalCount: {TotalCount}", items.Count, totalCount);

                // Unpaginated (pageSize null) reads back as "everything, on one page"
                return new QRCodeDetailsPagedResponse
                {
                    Data = items,
                    TotalRecords = totalCount,
                    PageNumber = pageSize.HasValue ? pageNumber : 1,
                    PageSize = pageSize ?? totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching barcode details with parameters");
                throw;
            }
        }

        //same as GetQRCodeDetailsWithParameterService but restricted to consumed QR codes (qrcodestatusid = 2, isactive = 0)
        public async Task<List<QRCodeDetailsResponseDto>> GetConsumedQRCodeDetailsWithParameterService(GetQRCodeRequestDto getQRCodeRequest)
        {
            try
            {
                _logger.LogInformation("Fetching consumed QR code details for request :", getQRCodeRequest);

                var result = await _qrCodeRepository.GetConsumedQRcodeWithParameterAsync(getQRCodeRequest);

                _logger.LogInformation("Successfully fetched consumed QR code details for request :", getQRCodeRequest);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching consumed QR code details for request", getQRCodeRequest);
                throw;
            }
        }

        public async Task<QRCodeDetailsResponseDto> ComponentStoreInService(string QRCodeNumber)
        {
            try
            {
                _logger.LogInformation("Processing component store-in for QR code: {QRCodeNumber}", QRCodeNumber);

                var qrcodeInformation = await _qrCodeRepository.GetQRcodeDetailsAsync(QRCodeNumber);

                if (qrcodeInformation == null)
                {
                    _logger.LogWarning("QR code details not found for QR code: {QRCodeNumber}", QRCodeNumber);
                    throw new Exception("Invalid QR code number.");
                }

                if (string.Equals(qrcodeInformation.QrCodeStatus, "Consumed", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("QR code already consumed: {QRCodeNumber}", QRCodeNumber);
                    throw new Exception("QR code already consumed.");
                }

                var storeInResult = await _qrCodeRepository.ComponentStoreIn(QRCodeNumber);

                var componentDetails = await _qrCodeRepository.GetQRcodeDetailsAsync(QRCodeNumber);

                _logger.LogInformation("Successfully processed component store-in for QR code: {QRCodeNumber}", QRCodeNumber);

                return componentDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during component store-in for QR code: {QRCodeNumber}", QRCodeNumber);
                throw; // Let the controller handle formatting the response
            }
        }



        public async Task<List<QRCodeDetailsResponseDto>> GetComponentStoreInByDateService(StoredInQrCodeRequest storeInRequest)
        {
            try
            {

                _logger.LogInformation("Get component store-in by StoreInDate:QRCodeService {StoreInDate}");

                var componentDetails = await _qrCodeRepository.GetComponentByStoreInByDate(storeInRequest);

                if (componentDetails != null)
                {
                    foreach (var detail in componentDetails)
                    {
                        if (detail.StoreInDate.HasValue)
                        {
                            detail.StoreInDate = detail.StoreInDate.Value.Date;
                        }
                    }
                }

                _logger.LogInformation("Successfully Get component store-in by StoreInDate:QRCodeService {StoreInDate}");

                return componentDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during Get component store-in by StoreInDate:QRCodeService {StoreInDate}");
                throw;
            }
        }

        // Every exportable column for ExportQRCodeToExcel, keyed by camelCase name.
        // When selectedColumns is empty/null, all of these are exported (in this order);
        // otherwise only the requested keys are used, in the order the caller specified.
        private static readonly (string Key, string Header, Func<QRCodeDetailsResponseDto, string?> GetValue)[] QRCodeExportColumnDefinitions = new (string, string, Func<QRCodeDetailsResponseDto, string?>)[]
        {
            ("sr", "Sr. No.", item => item.SrNo),
            ("qrCodeNumber", "QRCodeNumber", item => item.QrCodeNumber),
            ("projectNumber", "Project Number", item => item.ProjectNumber),
            ("drawingNumber", "Drawing Number", item => item.DrawingNumber),
            ("productionSeries", "Production Series", item => item.ProductionSeries),
            ("nomenclature", "Nomenclature", item => item.Nomenclature),
            ("componentType", "Component Type", item => item.ComponentType),
            ("batchIdNumber", "Batch Idnumber", item => item.BatchID),
            ("unitName", "Unit Name", item => item.UnitName),
            ("idNumber", "ID Number", item => item.IdNumber),
            ("irNumber", "IR Number", item => item.IrNumber),
            ("msnNumber", "MSN Number", item => item.MsnNumber),
            ("mrirNumber", "MRIR Number", item => item.MRIRNumber),
            ("quantity", "Quantity", item => !string.IsNullOrWhiteSpace(item.BatchID) ? item.BatchID : item.Quantity?.ToString("0.####")),
            ("desposition", "Desposition", item => item.Desposition),
            ("manufacturingDate", "Manufacturing Date", item => item.ManufacturingDate?.ToString("yyyy-MM-dd")),
            ("expiryDate", "Expiry Date", item => item.ExpiryDate?.ToString("yyyy-MM-dd")),
            ("storeInDate", "Store In Date", item => item.StoreInDate?.ToString("yyyy-MM-dd HH:mm:ss")),
            ("users", "Users", item => item.Users),
            ("productionOrderNumber", "Production Order Number", item => item.ProductionOrderNumber),
            ("purchaseOrderNumber", "Purchase Order Number", item => item.PurchaseOrderNumber),
            ("rackLocation", "Rack Location", item => item.RackLocation),
            ("assemblyNumber", "Assembly Number", item => item.AssemblyNumber),
            ("lnItemCode", "LN Item Code", item => item.LnItemCode),
            ("qrCodeStatus", "QRCode Status", item => item.QrCodeStatus),
            ("consumedInDrawing", "Consumed In Drawing", item => item.ConsumedInDrawing),
            ("remark", "Remark", item => item.Remark),
            ("createdDate", "Created Date", item => item.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss")),
            ("modifiedDate", "Modified Date", item => item.ModifiedDate?.ToString("yyyy-MM-dd HH:mm:ss")),
            ("partNo", "Part No", item => item.PartNo),
            ("size", "Size", item => item.Size),
            ("shapes", "Shapes", item => item.Shapes),
            ("customerItemCode", "Customer Item Code", item => item.CustomerIC),
            ("material", "Material", item => item.Material),
            ("htLotNo", "HT Lot No", item => item.HTLotNo),
            ("fanManNumber", "FAN/MAN Number", item => item.FAN),
            ("fanManSerialNumber", "FAN/MAN Serial Number", item => item.GIC),
            ("serialNumberOfQuantity", "Serial Number of Quantity", item => item.DTD),
            ("msnIrNumber", "MSN/IR Number", item => item.IRNo),
            ("gfnNo", "GFN No", item => item.GFNNo),
            ("srNo", "Sr No", item => item.SrNo),
            ("tQty", "TQty", item => item.TQty),
            ("wc", "WC", item => item.WC),
        };

        public byte[] ExportQRCodeToExcel(List<QRCodeDetailsResponseDto> qrCodeItems, List<string>? selectedColumns = null)
        {
            try
            {
                _logger.LogInformation("Starting Excel export for {Count} QR codes", qrCodeItems.Count);

                var activeColumns = QRCodeExportColumnDefinitions;
                if (selectedColumns != null && selectedColumns.Count > 0)
                {
                    var byKey = QRCodeExportColumnDefinitions.ToDictionary(c => c.Key, StringComparer.OrdinalIgnoreCase);
                    var resolved = selectedColumns
                        .Where(k => !string.IsNullOrWhiteSpace(k) && byKey.ContainsKey(k))
                        .Select(k => byKey[k])
                        .Distinct()
                        .ToArray();

                    if (resolved.Length > 0)
                    {
                        activeColumns = resolved;
                    }
                }

                using (var workbook = new XSSFWorkbook())
                {
                    var sheet = workbook.CreateSheet("QRCodeData");

                    var headerStyle = CreateHeaderStyle(workbook);
                    var borderStyle = CreateBorderStyle(workbook);

                    WriteHeaders(sheet, headerStyle, activeColumns);

                    for (int i = 0; i < qrCodeItems.Count; i++)
                    {
                        WriteDataRow(sheet, qrCodeItems[i], borderStyle, i + 1, activeColumns, srNo: i + 1); // i + 1 because row 0 is header
                    }

                    AutoSizeColumns(sheet, activeColumns.Length);

                    using (var ms = new MemoryStream())
                    {
                        workbook.Write(ms);
                        _logger.LogInformation($"Excel export completed successfully for {qrCodeItems.Count} QR codes");
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during Excel export");
                throw;
            }
        }

        private static ICellStyle CreateHeaderStyle(IWorkbook workbook)
        {
            var style = workbook.CreateCellStyle();
            var font = workbook.CreateFont();
            font.IsBold = true;
            style.SetFont(font);
            style.FillForegroundColor = IndexedColors.Grey25Percent.Index;
            style.FillPattern = FillPattern.SolidForeground;
            style.Alignment = HorizontalAlignment.Center;
            style.VerticalAlignment = VerticalAlignment.Center;
            style.BorderTop = BorderStyle.Thin;
            style.BorderBottom = BorderStyle.Thin;
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;
            return style;
        }

        private static ICellStyle CreateBorderStyle(IWorkbook workbook)
        {
            var style = workbook.CreateCellStyle();
            style.BorderTop = BorderStyle.Thin;
            style.BorderBottom = BorderStyle.Thin;
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;
            style.Alignment = HorizontalAlignment.Center;
            style.VerticalAlignment = VerticalAlignment.Center;
            return style;
        }

        private static void WriteHeaders(ISheet sheet, ICellStyle headerStyle, (string Key, string Header, Func<QRCodeDetailsResponseDto, string?> GetValue)[] columns)
        {
            var headerRow = sheet.CreateRow(0);
            for (int i = 0; i < columns.Length; i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(columns[i].Header);
                cell.CellStyle = headerStyle;
            }
        }

        private static void WriteDataRow(ISheet sheet, QRCodeDetailsResponseDto item, ICellStyle borderStyle, int rowIndex, (string Key, string Header, Func<QRCodeDetailsResponseDto, string?> GetValue)[] columns, int srNo)
        {
            var row = sheet.CreateRow(rowIndex);
            for (int c = 0; c < columns.Length; c++)
            {
                var value = string.Equals(columns[c].Key, "sr", StringComparison.OrdinalIgnoreCase)
                    ? srNo.ToString()
                    : columns[c].GetValue(item);
                CreateCell(row, c, value, borderStyle);
            }
        }

        private static void CreateCell(IRow row, int column, string? value, ICellStyle style)
        {
            var cell = row.CreateCell(column);
            cell.SetCellValue(value ?? string.Empty); // Handle null values
            cell.CellStyle = style;
        }

        private static void AutoSizeColumns(ISheet sheet, int columnCount)
        {
            for (int i = 0; i < columnCount; i++)
            {
                sheet.AutoSizeColumn(i);
            }
        }

        public async Task<List<ConsumedInResponseDto>> ConsumedInService(ConsumedInRequestDto request)
        {
            try
            {
                _logger.LogInformation("Processing consumed-in request for {RequestType}", request.GetType().Name);

                var consumedInResponse = await _qrCodeRepository.ConsumedInRepoAsync(request);

                _logger.LogInformation("Successfully processed consumed-in request, retrieved {Count} items",
                    consumedInResponse?.Count ?? 0);

                return consumedInResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during consumed-in request processing");
                throw;
            }
        }

        public async Task<List<BatchIdResponse>> ProcessBatchService(BatchQRcodeRequestDto batchQRcodeRequest)
        {
            try
            {
                _logger.LogInformation($"Processing ProcessBatchService request for {batchQRcodeRequest}");

                var drawingDetails = await _commonService.GetAllDrawingNumberService();
                var selectedDrawingNumber = drawingDetails.FirstOrDefault(x => x.Id == batchQRcodeRequest.DrawingNumberId);

                var batchResponses = new List<BatchIdResponse>
                {
                    new BatchIdResponse
                    {
                        Quantity = batchQRcodeRequest.Quantity,
                        BatchQuantity = 1,
                        AssemblyDrawingId = batchQRcodeRequest.DrawingNumberId,
                        AssemblyNumber = selectedDrawingNumber?.AssemblyNumber?? "Custom"
                    }
                };

                _logger.LogInformation("Batch processing complete. Total batches created: {Count}", batchResponses.Count);

                return batchResponses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during batchQRcodeRequest processing in ProcessBatchService");
                throw;
            }
        }

        public async Task<QrCodeResponse> InsertPrecheckQRCodeDetailsService(PrecheckQRCodeRequestDto request)
        {
            try
            {
                _logger.LogInformation("Starting InsertPrecheckQRCodeDetailsService: {@Request}", request);

                var existingQRCode = await _qrCodeRepository.GetQRcodeDetailsAsync(request.QRCodeNumber);
                if (existingQRCode != null)
                {
                    _logger.LogWarning("QR code already exists: {QRCodeNumber}", request.QRCodeNumber);
                    throw new ValidationException($"QR code {request.QRCodeNumber} already exists in the system.");
                }

                var validationResponse = await _qrCodeRepository.ValidateQrCode(
                    request.ProductionSeriesId,
                    request.IdNumber,
                    request.DrawingNumberId,
                    null);

                if (validationResponse != null)
                {
                    _logger.LogWarning("QR code validation failed for ProductionSeriesId: {ProductionSeriesId}, IdNumber: {IdNumber}, DrawingNumberId: {DrawingNumberId}",
                        request.ProductionSeriesId, request.IdNumber, request.DrawingNumberId);
                    throw new ValidationException($"A QR code already exists for this combination of Production Series, ID Number, and Drawing Number.");
                }

                var result = await _qrCodeRepository.InsertPrecheckQRCodeDetailsAsync(request);

                _logger.LogInformation("Successfully inserted Precheck QR code details: {@Result}", result);
                return result;
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "Validation error occurred while inserting Precheck QR code details: {Message}", vex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting Precheck QR code details.");
                throw;
            }
        }

        /// <summary>
        /// Parses custom ID range string and returns a list of IDs
        /// Supports formats like: "2,3,4,5,6-10" or "1-5,7,9-12"
        /// 
        /// Test Examples:
        /// - "2,3,4,5,6-10" -> [2,3,4,5,6,7,8,9,10] (9 IDs)
        /// - "1-5,7,9-12" -> [1,2,3,4,5,7,9,10,11,12] (10 IDs)
        /// - "1,3,5" -> [1,3,5] (3 IDs)
        /// - "10-12" -> [10,11,12] (3 IDs)
        /// </summary>
        /// <param name="customIdRange">The custom ID range string</param>
        /// <returns>List of parsed IDs</returns>
        private List<int> ParseCustomIdRange(string customIdRange)
        {
            var ids = new List<int>();

            if (string.IsNullOrWhiteSpace(customIdRange))
                return ids;

            try
            {
                var parts = customIdRange.Split(',', StringSplitOptions.RemoveEmptyEntries);

                foreach (var part in parts)
                {
                    var trimmedPart = part.Trim();

                    if (trimmedPart.Contains('-'))
                    {
                        var rangeParts = trimmedPart.Split('-');
                        if (rangeParts.Length == 2 &&
                            int.TryParse(rangeParts[0].Trim(), out int start) &&
                            int.TryParse(rangeParts[1].Trim(), out int end))
                        {
                            for (int i = start; i <= end; i++)
                            {
                                ids.Add(i);
                            }
                        }
                    }
                    else
                    {
                        if (int.TryParse(trimmedPart, out int singleId))
                        {
                            ids.Add(singleId);
                        }
                    }
                }

                ids = ids.Distinct().OrderBy(x => x).ToList();

                _logger.LogInformation("Successfully parsed custom ID range '{CustomIdRange}' into {Count} unique IDs",
                    customIdRange, ids.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing custom ID range: {CustomIdRange}", customIdRange);
                throw new ValidationException($"Invalid custom ID range format: {customIdRange}. Expected format: '2,3,4,5,6-10'");
            }

            return ids;
        }

        public async Task<QRCodeDetailsResponseDto> UpdateQRCodeDetailsAsync(UpdateQRCodeDto request)
        {
            try
            {
                _logger.LogInformation("Starting UpdateQRCodeDetailsAsync for QR code: {QRCodeNumber}", request.QRCodeNumber);

                if (string.IsNullOrWhiteSpace(request.QRCodeNumber))
                {
                    throw new ApplicationException("QRCodeNumber is required.");
                }

                var existingQRCode = await _qrCodeRepository.GetQRcodeDetailsAsync(request.QRCodeNumber);
                if (existingQRCode == null)
                {
                    _logger.LogWarning("QR code not found: {QRCodeNumber}", request.QRCodeNumber);
                    throw new Exception($"QR code '{request.QRCodeNumber}' not found");
                }

                var updateSuccess = await _qrCodeRepository.UpdateQRCodeDetailsAsync(request);
                if (!updateSuccess)
                {
                    _logger.LogWarning("Failed to update QR code: {QRCodeNumber}", request.QRCodeNumber);
                    throw new Exception($"Failed to update QR code '{request.QRCodeNumber}'");
                }

                var updatedQRCode = await _qrCodeRepository.GetQRcodeDetailsAsync(request.QRCodeNumber);
                _logger.LogInformation("Successfully updated QR code: {QRCodeNumber}", request.QRCodeNumber);

                return updatedQRCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating QR code details for: {QRCodeNumber}", request.QRCodeNumber);
                throw;
            }
        }

        public async Task<string> DisableQRCodeAsync(DisableQRCodeRequestDto request)
        {
            try
            {
                _logger.LogInformation("Starting DisableQRCodeAsync for QR code: {QRCodeNumber}", request.QRCodeNumber);

                var disableSuccess = await _qrCodeRepository.DisableQRCodeAsync(request);
                if (!disableSuccess)
                {
                    _logger.LogWarning("Failed to disable QR code: {QRCodeNumber}", request.QRCodeNumber);
                    throw new Exception($"Failed to disable QR code '{request.QRCodeNumber}'");
                }

                return request.QRCodeNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while disabling QR code: {QRCodeNumber}", request.QRCodeNumber);
                throw;
            }
        }

        public async Task<StandardQRDetailsResponseDto> GetStandardQRCodeDetailsService(string qrCodeNumber)
        {
            try
            {
                _logger.LogInformation("Fetching Standard QR code details for code: {QRCodeNumber}", qrCodeNumber);

                var result = await _qrCodeRepository.GetStandardQRCodeDetailsAsync(qrCodeNumber);

                _logger.LogInformation("Successfully fetched Standard QR code details for: {QRCodeNumber}", qrCodeNumber);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching Standard QR code details for: {QRCodeNumber}", qrCodeNumber);
                throw;
            }
        }

        // Every exportable column for ExportStandardQRCodeToExcel, keyed by camelCase name (same keys as
        // QRCodeExportColumnDefinitions where they overlap). When selectedColumns is empty/null, all of
        // these are exported (in this order); otherwise only the requested keys are used, in the order
        // the caller specified. "sr" is generated at export time (row position), not read from the item.
        private static readonly (string Key, string Header, Func<StandardQRDetailsResponseDto, string?> GetValue)[] StandardQRCodeExportColumnDefinitions = new (string, string, Func<StandardQRDetailsResponseDto, string?>)[]
        {
            ("sr", "Sr. No.", item => item.SrNo),
            ("qrCodeNumber", "QRCodeNumber", item => item.QrCodeNumber),
            ("projectNumber", "Project Number", item => item.ProjectNumber),
            ("drawingNumber", "Drawing Number", item => item.DrawingNumber),
            ("productionSeries", "Production Series", item => item.ProductionSeries),
            ("nomenclature", "Nomenclature", item => item.Nomenclature),
            ("componentType", "Component Type", item => item.ComponentType),
            ("idNumber", "ID Number", item => item.IdNumber),
            ("batchIdNumber", "Batch Idnumber", item => item.BatchID),
            ("irNumber", "IR Number", item => item.IrNumber),
            ("msnNumber", "MSN Number", item => item.MsnNumber),
            ("mrirNumber", "MRIR Number", item => item.MRIRNumber),
            ("quantity", "Quantity", item => item.Quantity?.ToString("0.####")),
            ("desposition", "Desposition", item => item.Desposition),
            ("manufacturingDate", "Manufacturing Date", item => item.ManufacturingDate?.ToString("yyyy-MM-dd")),
            ("expiryDate", "Expiry Date", item => item.ExpiryDate?.ToString("yyyy-MM-dd")),
            ("storeInDate", "Store In Date", item => item.StoreInDate?.ToString("yyyy-MM-dd HH:mm:ss")),
            ("users", "Users", item => item.Users),
            ("productionOrderNumber", "Production Order Number", item => item.ProductionOrderNumber),
            ("purchaseOrderNumber", "Purchase Order Number", item => item.PurchaseOrderNumber),
            ("rackLocation", "Rack Location", item => item.RackLocation),
            ("assemblyNumber", "Assembly Number", item => item.AssemblyNumber),
            ("lnItemCode", "LN Item Code", item => item.LnItemCode),
            ("qrCodeStatus", "QRCode Status", item => item.QrCodeStatus),
            ("consumedInDrawing", "Consumed In Drawing", item => item.ConsumedInDrawing),
            ("remark", "Remark", item => item.ProjectDescription),
            ("createdDate", "Created Date", item => item.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss")),
            ("modifiedDate", "Modified Date", item => item.ModifiedDate?.ToString("yyyy-MM-dd HH:mm:ss")),
            ("partNo", "Part No", item => item.PartNo),
            ("size", "Size", item => item.Size),
            ("shapes", "Shapes", item => item.Shapes),
            ("customerItemCode", "Customer Item Code", item => item.CustomerItemCode),
            ("material", "Material", item => item.Material),
            ("htLotNo", "HT Lot No", item => item.HTLotNo),
            ("fanManNumber", "FAN/MAN Number", item => item.FanManNumber),
            ("fanManSerialNumber", "FAN/MAN Serial Number", item => item.FanManSerialNumber),
            ("serialNumberOfQuantity", "Serial Number of Quantity", item => item.SerialNumberOfQuantity),
            ("msnIrNumber", "MSN/IR Number", item => item.MsnIrNumber),
            ("gfnNo", "GFN No", item => item.GFNNo),
            ("tQty", "TQty", item => item.TQty),
            ("wc", "WC", item => item.WC),
            ("unitName", "Unit Name", item => item.UnitName),
        };

        public byte[] ExportStandardQRCodeToExcel(List<StandardQRDetailsResponseDto> qrCodeItems, List<string>? selectedColumns = null)
        {
            try
            {
                _logger.LogInformation("Starting Excel export for {Count} Standard QR codes", qrCodeItems.Count);

                var activeColumns = StandardQRCodeExportColumnDefinitions;
                if (selectedColumns != null && selectedColumns.Count > 0)
                {
                    var byKey = StandardQRCodeExportColumnDefinitions.ToDictionary(c => c.Key, StringComparer.OrdinalIgnoreCase);
                    var resolved = selectedColumns
                        .Where(k => !string.IsNullOrWhiteSpace(k) && byKey.ContainsKey(k))
                        .Select(k => byKey[k])
                        .Distinct()
                        .ToArray();

                    if (resolved.Length > 0)
                    {
                        activeColumns = resolved;
                    }
                }

                using (var workbook = new XSSFWorkbook())
                {
                    var sheet = workbook.CreateSheet("StandardQRCodeData");

                    var headerStyle = CreateHeaderStyle(workbook);
                    var borderStyle = CreateBorderStyle(workbook);

                    var headerRow = sheet.CreateRow(0);
                    for (int c = 0; c < activeColumns.Length; c++)
                    {
                        var cell = headerRow.CreateCell(c);
                        cell.SetCellValue(activeColumns[c].Header);
                        cell.CellStyle = headerStyle;
                    }

                    for (int i = 0; i < qrCodeItems.Count; i++)
                    {
                        var row = sheet.CreateRow(i + 1);
                        for (int c = 0; c < activeColumns.Length; c++)
                        {
                            var value = string.Equals(activeColumns[c].Key, "sr", StringComparison.OrdinalIgnoreCase)
                                ? (i + 1).ToString()
                                : activeColumns[c].GetValue(qrCodeItems[i]);
                            CreateCell(row, c, value, borderStyle);
                        }
                    }

                    AutoSizeColumns(sheet, activeColumns.Length);

                    using (var ms = new MemoryStream())
                    {
                        workbook.Write(ms);
                        _logger.LogInformation($"Excel export completed successfully for {qrCodeItems.Count} Standard QR codes");
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during Standard QR code Excel export");
                throw;
            }
        }
        public async Task<List<UserDto>> GetAllUsersServiceAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all active users from UserService");

                var result = await _qrCodeRepository.GetAllUsersAsync();

                _logger.LogInformation("Successfully fetched all active users. Count: {Count}", result?.Count ?? 0);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all users in GetAllUsersServiceAsync.");
                throw;
            }
        }
        public async Task<List<string>> GetDistinctBatchIdNumbersServiceAsync()
        {
            try
            {
                _logger.LogInformation("Service request: GetDistinctBatchIdNumbersServiceAsync with ProdSeriesId: {ProdSeriesId}, DrawingId: {DrawingId}");
                var result = await _qrCodeRepository.GetDistinctBatchIdNumbersAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDistinctBatchIdNumbersServiceAsync");
                throw;
            }
        }

        public async Task<List<string>> GetAllFanManSerialNumbersServiceAsync()
        {
            try
            {
                _logger.LogInformation("Service request: GetAllFanManSerialNumbersServiceAsync");
                var result = await _qrCodeRepository.GetAllFanManSerialNumbersAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllFanManSerialNumbersServiceAsync");
                throw;
            }
        }

        public async Task<byte[]> ExportConsumedInServiceAsync(ConsumedInRequestDto request)
        {
            try
            {
                _logger.LogInformation("Processing export consumed-in request for {RequestType}", request.GetType().Name);

                var consumedInResponse = await _qrCodeRepository.ExportConsumedInRepoAsync(request);

                if (consumedInResponse == null || !consumedInResponse.Any())
                {
                    _logger.LogInformation("No data found for export.");
                    return null;
                }

                using (var workbook = new XSSFWorkbook())
                {
                    var sheet = workbook.CreateSheet("ConsumedIn");

                    // Same structure as ExportQRCodeToExcel: shared style helpers + Headers array + WriteHeaders/WriteDataRow
                    var headerStyle = CreateHeaderStyle(workbook);
                    var borderStyle = CreateBorderStyle(workbook);

                    WriteConsumedInHeaders(sheet, headerStyle);

                    for (int i = 0; i < consumedInResponse.Count; i++)
                    {
                        WriteConsumedInDataRow(sheet, consumedInResponse[i], borderStyle, i + 1);
                    }

                    AutoSizeColumns(sheet, ConsumedInHeaders.Length);

                    using (var ms = new MemoryStream())
                    {
                        workbook.Write(ms);
                        _logger.LogInformation("Successfully generated Excel file with {Count} records", consumedInResponse.Count);
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting consumed-in data");
                throw;
            }
        }

        // Common fields first (same naming/order convention as the main QR export), ConsumedIn-only columns last
        private static readonly string[] ConsumedInHeaders = new string[]
        {
            "ID Number", "LN Item Code", "IR Number", "MSN Number", "Quantity",
            "Consumed In Drawing", "Consumed In Production Order Number", "Username", "Date",
            "IsRejected", "RejectionReason"
        };

        private static void WriteConsumedInHeaders(ISheet sheet, ICellStyle headerStyle)
        {
            var headerRow = sheet.CreateRow(0);
            for (int i = 0; i < ConsumedInHeaders.Length; i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(ConsumedInHeaders[i]);
                cell.CellStyle = headerStyle;
            }
        }

        private static void WriteConsumedInDataRow(ISheet sheet, ConsumedInResponseDto item, ICellStyle borderStyle, int rowIndex)
        {
            var row = sheet.CreateRow(rowIndex);
            int colIndex = 0;
            CreateCell(row, colIndex++, item.IdNumber, borderStyle);
            CreateCell(row, colIndex++, item.LnItemCode, borderStyle);
            CreateCell(row, colIndex++, item.IRNumber, borderStyle);
            CreateCell(row, colIndex++, item.MSNNumber, borderStyle);
            CreateCell(row, colIndex++, item.Quantity.ToString(), borderStyle);
            CreateCell(row, colIndex++, item.ConsumedInDrawing, borderStyle);
            CreateCell(row, colIndex++, item.ConsumedInProductionOrderNumber, borderStyle);
            CreateCell(row, colIndex++, item.Username, borderStyle);
            CreateCell(row, colIndex++, item.Date?.ToString("yyyy-MM-dd HH:mm:ss"), borderStyle);
            CreateCell(row, colIndex++, item.IsRejected.HasValue && item.IsRejected.Value ? "Yes" : "No", borderStyle);
            CreateCell(row, colIndex++, item.RejectionReason, borderStyle);
        }

        public async Task<int> BulkUpdateQRCodeService(BulkUpdateQRCodeRequestDto request)
        {
            try
            {
                _logger.LogInformation("Service: Bulk update QR codes");

                if (request.QrCodeNumbers == null || !request.QrCodeNumbers.Any())
                    throw new ValidationException("QR Code list cannot be empty");

                var result = await _qrCodeRepository.BulkUpdateQRCodeAsync(request);

                _logger.LogInformation("Service: Bulk update completed");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BulkUpdateQRCodeService");
                throw;
            }
        }


        public async Task<GetAvailableQrPagedResponse> GetAvailableQrPagedService(GetAvailableQrRequest request, int pageNumber, int pageSize)
        {
            var (items, totalRecords) = await _qrCodeRepository.GetAvailableQrPaged(request, pageNumber, pageSize);

            return new GetAvailableQrPagedResponse
            {
                Data = items,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
