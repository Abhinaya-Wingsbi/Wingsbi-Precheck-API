



using Azure;
using ClosedXML.Excel;
using EnumsNET;
using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DataModel.Precheck;
using Godrej.Precheck.Models.DTOs.Barcode;
using Godrej.Precheck.Models.DTOs.Precheck;
using Godrej.Precheck.Models.DTOs.QRCodeDetails;
using Godrej.Precheck.Repository.Repository.CommonRepository;
using Godrej.Precheck.Repository.Repository.PrecheckRepository;
using Godrej.Precheck.Repository.Repository.ProductionOrderRepository;
using Godrej.Precheck.Repository.Repository.QRCodeRepository;
using Godrej.Precheck.Repository.Repository.UserRepository;
using Godrej.Precheck.Service.Helper;
using Godrej.Precheck.Service.Service.CommonSevice;
using Mapster;
using MathNet.Numerics.RootFinding;
using Microsoft.Extensions.Logging;
using MigraDoc.Rendering;
using NPOI.SS.UserModel;
using Org.BouncyCastle.Asn1.Pkcs;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SixLabors.Fonts;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Web;

namespace Godrej.Precheck.Service.Service.PrecheckService
{
    public class PrecheckService : IPrecheckService
    {
        private readonly IPrecheckRepository _precheckRepository;
        private readonly IQRCodeRepository _qRCodeRepository;
        private readonly ILogger<PrecheckService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IProductionOrderRepository _productionOrderRepository;
        private readonly ICommonRepository _commonRepository;

        // LN Item Codes that are prechecked under production series "H" but must be printed as "SH" on the precheck export
        private static readonly HashSet<string> LnItemCodesPrintedAsSh = new(StringComparer.OrdinalIgnoreCase)
        {
            "WJD000000001813",
            "WJD000000001814",
            "WJD000000002734",
            "WJD000000002777",
            "WJD000000002742",
            "WJD000000002746",
            "WJD000000002752",
            "WJD000000002736",
            "WJD000000002778",
            "WJD000000002743",
            "WJD000000002747",
            "WJD000000002753",
            "WJD000000001848",
            "WJD000000003756",
            "WJD000000003757",
            "WJD000000008552",
            "WJD000000008551",
            "WJD000000008553",
            "WJD000000008554"
        };

        public PrecheckService(IPrecheckRepository precheckRepository, IQRCodeRepository qRCodeRepository, ILogger<PrecheckService> logger, IUserRepository userRepository, IProductionOrderRepository productionOrderRepository, ICommonRepository commonRepository)
        {
            _precheckRepository = precheckRepository;
            _qRCodeRepository = qRCodeRepository;
            _logger = logger;
            _userRepository = userRepository;
            _productionOrderRepository = productionOrderRepository;
            _commonRepository = commonRepository;
        }

        public async Task<List<PrecheckTemplateResponseDto>> GetPrecheckAssemblyTemplate(string assemblyNumber)
        {
            var result = await _precheckRepository.GetPrecheckTemplateResponsesAsync(assemblyNumber);

            var response = result.Adapt<List<PrecheckTemplateResponseDto>>();

            return response;
        }

        public async Task<List<ViewPreCheckResponse>> MakePrecheck(List<PrecheckRequestDto> requestDto)
        {
            try
            {
                foreach (var request in requestDto)
                {

                    if (request == null)
                        continue;

                    await ProcessSinglePrecheckItem(request);

                }

                var lastrequest = requestDto.LastOrDefault();
                var viewLastPreCheckRequest = CreateViewPreCheckRequest(lastrequest);
                // 5. Get and return updated precheck details
                var response = await _precheckRepository.ViewPrecheckDetails(viewLastPreCheckRequest);
                // var response = await _precheckRepository.PrecheckDetails(viewLastPreCheckRequest);
                //update the precheck status based on the view precheck
                var status = GetPrecheckStatus(response);

                await _precheckRepository.UpdateProjectStatusDetails(viewLastPreCheckRequest, status);


                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task ProcessSinglePrecheckItem(PrecheckRequestDto request)
        {
            // 1. Validate and get QR code details
            var qrCodeDetails = await ValidateAndGetQRCodeDetails(request.QrCodeNumber);
            decimal? remainingQtyAfterConsume = request.RemainingQuantity;

            var viewPreCheckRequest = CreateViewPreCheckRequest(request);
            var preCheckResponses = await _precheckRepository.ViewPrecheckDetails(viewPreCheckRequest);
            if (preCheckResponses == null)
            {
                throw new ApplicationException("Precheck details does not exist");
            }

            await ValidateComponentDrawing(preCheckResponses, request.DrawingNumberId.Value);

            // 3. Prepare precheck request with QR code details
            //var precheckRequest = PreparePrecheckRequest(request, qrCodeDetails);
            var precheckRequest = request.Adapt<MakePrecheckRequest>();
            precheckRequest.QrCodeId = qrCodeDetails.Id;
            //Added UserName
            User UserDetails = await _userRepository.GetUserByIdAsync(request.CreatedBy);

            precheckRequest.RemainingQuantity = request.RemainingQuantity;

            if (qrCodeDetails.ComponentTypeId == 2)
            {
                precheckRequest.PrecheckStatusId =
                    remainingQtyAfterConsume == 0
                        ? 2
                        : 1;
            }

            //Call update- quantity service
            await _precheckRepository.UpdateQrcodeStatus(request);
            var updateQuantityResult = await UpdateQuantity(
                request.ConsumeInProductionOrderNumber,        // string productionOrderNumber
                new UpdateMaterialQuantityRequestDto
                {
                    DrawingnumberId = request.DrawingNumberId,
                    QrCodeNumber = request.QrCodeNumber,
                    UpdatedQuantity = request.UpdatedQuantity,
                    ParentDrawingNumber = request.ConsumedInDrawingNumberID,
                    Idnumber = request.ConsumedInId,
                    ComponentType = request.ComponentType
                },
                request.AssemblyDrawingNo,            // string assemblyDrawingNo
                request.CreatedBy                     // int userId
            );

            // 4. Process based on component type
            await ProcessComponentType(precheckRequest);
        }

        public async Task<List<ViewPreCheckResponse>> BulkPrecheck(BulkPrecheckRequestDto request)
        {
            if (request == null)
            {
                throw new ApplicationException("Request cannot be null.");
            }

            if (request.FromId > request.ToId)
            {
                throw new ApplicationException("FromId must be less than or equal to ToId.");
            }

            var componentType = request.ComponentType?.ToUpper();
            if (componentType != "BATCH" && componentType != "FIM" && componentType != "SI")
            {
                throw new ApplicationException($"Component type {request.ComponentType} is not supported for bulk precheck.");
            }

            try
            {
                var responses = new List<ViewPreCheckResponse>();

                for (var id = request.FromId; id <= request.ToId; id++)
                {
                    var precheckRequestDto = request.Adapt<PrecheckRequestDto>();
                    precheckRequestDto.ConsumedInId = id;
                    precheckRequestDto.Quantity = request.QtyToBeConsume;
                    precheckRequestDto.UpdatedQuantity = request.QtyToBeConsume;
                    precheckRequestDto.ConsumeInProductionOrderNumber = request.ProductionOrderNumber;

                    var viewPreCheckRequest = CreateViewPreCheckRequest(precheckRequestDto);
                    var existingPreCheckDetails = await _precheckRepository.ViewPrecheckDetails(viewPreCheckRequest);
                    if (existingPreCheckDetails == null)
                    {
                        throw new ApplicationException("Precheck details does not exist");
                    }

                    var targetPreCheckDetail = existingPreCheckDetails
                        .Find(x => x.DrawingNumberId == request.DrawingNumberId && !x.IsPrecheckComplete);

                    if (targetPreCheckDetail == null)
                    {
                        throw new ApplicationException($"Drawing ID {request.DrawingNumberId} is not valid for Id {id}");
                    }

                    var availableQuantity = targetPreCheckDetail.RemainingQuantity ?? targetPreCheckDetail.Quantity ?? 0;
                    precheckRequestDto.RemainingQuantity = availableQuantity - request.QtyToBeConsume;

                    await ProcessSinglePrecheckItem(precheckRequestDto);

                    var updatedPreCheckDetails = await _precheckRepository.ViewPrecheckDetails(viewPreCheckRequest);
                    var status = GetPrecheckStatus(updatedPreCheckDetails);
                    await _precheckRepository.UpdateProjectStatusDetails(viewPreCheckRequest, status);

                    if (updatedPreCheckDetails != null)
                    {
                        responses.AddRange(updatedPreCheckDetails);
                    }
                }

                return responses;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<PrecheckExcelImportResultDto> MakePrecheckFromExcelAsync(Stream fileStream, int createdBy)
        {
            // Expected layout (first worksheet):
            // Row 1: title (e.g. "BOM Details for Assembly: ...") - not read
            // Row 2: blank
            // Row 3: header labels - Level | Drawing Number | Nomenclature | LN Item Code | Component Type | Qty | Parent Drawing | Find No | ProductionOrderNumber | IdNumber | QRCodeNumber
            // Row 4+: one component per row. Level/Nomenclature/LN Item Code/Component Type/Find No are reference-only columns and are not read.
            // Qty is read and used as the quantity to consume. Parent Drawing, when supplied, disambiguates
            // the project lookup by assembly drawing (see step 0 below) - it's optional for backward compatibility.
            // Each row is fully self-contained and can belong to a different project.

            using var workbook = new XLWorkbook(fileStream); // open the uploaded file as an Excel workbook
            var worksheet = workbook.Worksheets.First(); // work off the first sheet in the file

            var rows = new List<(string QrCodeNumber, string ProductionOrderNumber, string DrawingNumber, string IdNumberText, decimal? Quantity, string ParentDrawing)>(); // holds every parsed data row before processing begins
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0; // find the last row that actually has data
            for (int row = 4; row <= lastRow; row++) // walk every data row, starting after the title/blank/header rows
            {
                var drawingNumber = worksheet.Cell(row, 2).GetString()?.Trim(); // column 2: Drawing Number
                var qtyText = worksheet.Cell(row, 6).GetString()?.Trim(); // column 6: Qty, read as text first so it can be validated
                var parentDrawing = worksheet.Cell(row, 7).GetString()?.Trim(); // column 7: Parent Drawing - identifies which assembly this row's project belongs to
                var po = worksheet.Cell(row, 9).GetString()?.Trim(); // column 9: ProductionOrderNumber
                var idText = worksheet.Cell(row, 10).GetString()?.Trim(); // column 10: IdNumber, read as text first so it can be validated
                var qr = worksheet.Cell(row, 11).GetString()?.Trim(); // column 11: QRCodeNumber
                if (string.IsNullOrWhiteSpace(qr) && string.IsNullOrWhiteSpace(po) && string.IsNullOrWhiteSpace(drawingNumber) && string.IsNullOrWhiteSpace(idText))
                {
                    continue; // skip fully blank rows
                }

                decimal? quantity = decimal.TryParse(qtyText, out var parsedQuantity) ? parsedQuantity : null; // parse Qty to a decimal, or null if it's blank/not a number

                rows.Add((qr, po, drawingNumber, idText, quantity, parentDrawing)); // queue this row for processing
            }

            if (!rows.Any()) // no usable rows were found anywhere in the file
            {
                throw new ApplicationException("No data rows found starting at row 2."); // fail the whole upload up front
            }

            var result = new PrecheckExcelImportResultDto // the response object returned to the caller
            {
                TotalRows = rows.Count // record how many rows were found, for the summary counts
            };

            // Track every distinct project touched so we can refresh its status once at the end
            var touchedProjects = new Dictionary<(string Po, int IdNumber), ViewPreCheckRequest>(); // one entry per unique PO+IdNumber seen across all rows

            foreach (var row in rows) // process each row independently - one row's failure doesn't stop the rest
            {
                var qrCodeNumber = row.QrCodeNumber; // this row's QR code, used below and in the catch block for error reporting
                var productionOrderNumber = row.ProductionOrderNumber; // this row's production order number, used below and in the catch block
                

                try
                {
                    if (string.IsNullOrWhiteSpace(qrCodeNumber) || string.IsNullOrWhiteSpace(productionOrderNumber) ||
                        string.IsNullOrWhiteSpace(row.DrawingNumber) || string.IsNullOrWhiteSpace(row.IdNumberText)) // any of the four required columns is missing
                    {
                        throw new ApplicationException("QRCodeNumber, ProductionOrderNumber, Drawing Number and IdNumber are all required on every row.");
                    }

                    if (!int.TryParse(row.IdNumberText, out var idNumber)) // IdNumber column must be a whole number
                    {
                        throw new ApplicationException($"IdNumber '{row.IdNumberText}' is not a valid number.");
                    }

                    // 0. Resolve the Parent Drawing column, if supplied, so the project lookup below can be
                    // disambiguated by assembly drawing - ProductionOrderNumber is not globally unique
                    // (tbl_productionordermaster only enforces uniqueness per PO+ProdSeriesId+StartIdNumber),
                    // so the same PO+IdNumber pair can otherwise resolve to the wrong assembly's project.
                    int? parentDrawingNumberId = null;
                    if (!string.IsNullOrWhiteSpace(row.ParentDrawing))
                    {
                        parentDrawingNumberId = await _precheckRepository.GetDrawingNumberIdByName(row.ParentDrawing);
                        if (parentDrawingNumberId == null)
                        {
                            throw new ApplicationException($"Parent Drawing '{row.ParentDrawing}' not found or inactive.");
                        }
                    }

                    // 1. Resolve the project (tbl_projectdetails) by ProductionOrderNumber + IdNumber (+ Parent Drawing when supplied), isactive=1
                    var projectContext = await _precheckRepository.GetProjectContextByPoAndId(productionOrderNumber, idNumber, parentDrawingNumberId); // look up the active project this row belongs to
                    if (projectContext == null) // no active project matches this PO + IdNumber (+ Parent Drawing)
                    {
                        throw new ApplicationException($"No active project found for ProductionOrderNumber '{productionOrderNumber}', IdNumber {idNumber}.");
                    }

                    var viewPreCheckRequest = new ViewPreCheckRequest // the filter used to fetch/refresh this project's precheck rows
                    {
                        Id = idNumber, // the project's unit/ID number
                        ProductionSeriesId = projectContext.ProdSeriesId, // the project's production series
                        DrawingNumberId = projectContext.DrawingNumberId, // the project's own (assembly) drawing
                        CreatedBy = createdBy // the user performing this import
                    };

                    // 2. Resolve the component rows (tbl_projectprecheckdetails) under that project, isactive=1
                    var existingPreCheckDetails = await _precheckRepository.ViewPrecheckDetails(viewPreCheckRequest); // fetch every BOM component row for this project
                    if (existingPreCheckDetails == null || !existingPreCheckDetails.Any()) // the project has no precheck rows at all
                    {
                        throw new ApplicationException("Precheck details does not exist for this project.");
                    }

                    // 3. Resolve the QR code (tbl_qrcodedetails), isactive/status=1
                    var qrCodeDetails = await ValidateAndGetQRCodeDetails(qrCodeNumber); // validate the QR exists, is active, has quantity left, and is ready for consumption
                    if (qrCodeDetails.DrawingNumberId <= 0) // the QR isn't linked to any drawing at all
                    {
                        throw new ApplicationException($"QR Code {qrCodeNumber} has no drawing number.");
                    }

                    // 3b. Cross-check the row's Drawing Number column against the QR's own drawing
                    var rowDrawingNumberId = await _precheckRepository.GetDrawingNumberIdByName(row.DrawingNumber); // resolve the Excel row's Drawing Number text to its id
                    if (rowDrawingNumberId == null) // that drawing number text doesn't exist (or is inactive) in the system
                    {
                        throw new ApplicationException($"Drawing Number '{row.DrawingNumber}' not found or inactive.");
                    }

                    if (rowDrawingNumberId.Value != qrCodeDetails.DrawingNumberId) // the Excel row's drawing doesn't match what the QR is actually tagged for
                    {
                        throw new ApplicationException($"Drawing Number '{row.DrawingNumber}' does not match with QR Code's {qrCodeNumber} drawing number.");
                    }

                    // 4. Match the QR's own drawing number against the component rows for this project.
                    // A prior shortfall can leave two rows for the same drawing under this project -
                    // the closed original and a freshly duplicated open one - so prefer whichever
                    // matching row is still open rather than just the first match found.
                    var matchingDrawingDetails = existingPreCheckDetails
                        .FindAll(x => x.DrawingNumberId == qrCodeDetails.DrawingNumberId); // every precheck row for this project that matches the QR's drawing (there can be more than one)

                    if (matchingDrawingDetails.Count == 0) // this drawing isn't part of the project's BOM at all
                    {
                        throw new ApplicationException($"No component matches drawing ID {qrCodeDetails.DrawingNumberId} for QR Code {qrCodeNumber} under project (PO '{productionOrderNumber}', IdNumber {idNumber}).");
                    }

                    var targetPreCheckDetail = matchingDrawingDetails.Find(x => !x.IsPrecheckComplete); // pick whichever matching row is still open

                    if (targetPreCheckDetail == null) // every matching row for this drawing is already complete
                    {
                        throw new ApplicationException($"Precheck is already complete for this Production Order '{productionOrderNumber}' and IdNumber {idNumber}.");
                    }

                    var availableQuantity = targetPreCheckDetail.RemainingQuantity ?? targetPreCheckDetail.Quantity ?? 0; // how much this row still actually needs
                    var qtyToConsume = row.Quantity ?? qrCodeDetails.Quantity ?? 1; // how much is being supplied - Excel's Qty column, else the QR's own quantity, else 1

                    // If the Excel/QR quantity only partially covers what this component still
                    // needs, only that amount is consumed now and the shortfall is carried forward
                    // to a new row afterwards instead of being silently treated as fully complete.
                    // Never consume more than the row's actual outstanding amount (availableQuantity) -
                    // targetPreCheckDetail.Quantity still holds the component's original total even on
                    // a duplicated row further down a shortfall chain, so it must not be used here.
                    var isShortfall = qtyToConsume < availableQuantity; // true when the supplied quantity doesn't cover what's still needed
                    var consumedThisStep = isShortfall ? qtyToConsume : availableQuantity; // the amount actually consumed this step
                    var leftoverQuantity = isShortfall ? availableQuantity - qtyToConsume : 0m; // whatever is still outstanding after this step

                    // 5. Fill tbl_projectprecheckdetails for that specific row
                    var precheckRequestDto = new PrecheckRequestDto // the request that actually records this consumption
                    {
                        Id = targetPreCheckDetail.PrecheckDetailsId, // the specific precheck row being updated
                        QrCodeNumber = qrCodeNumber, // the QR being consumed
                        ConsumedDrawingNo = qrCodeDetails.DrawingNumber, // the drawing text to record as consumed
                        DrawingNumberId = qrCodeDetails.DrawingNumberId, // the drawing id being consumed
                        ComponentType = qrCodeDetails.ComponentType, // ID/BATCH/FIM/SI etc., drives how consumption is recorded
                        Quantity = consumedThisStep, // the amount consumed this step
                        UpdatedQuantity = consumedThisStep, // same amount, used to decrement the QR's own balance
                        RemainingQuantity = leftoverQuantity, // what's left outstanding on this row after this step
                        Unit = qrCodeDetails.UnitName, // unit of measure, copied from the QR
                        IrNumber = qrCodeDetails.IrNumber, // QR's IR number, recorded onto the precheck row for traceability
                        MsnNumber = qrCodeDetails.MsnNumber, // QR's MSN number, recorded onto the precheck row for traceability
                        MrirNumber = qrCodeDetails.MRIRNumber, // QR's MRIR number, recorded onto the precheck row for traceability
                        LnItemCode = qrCodeDetails.LnItemCode, // QR's LN item code
                        IdNumbers = qrCodeDetails.IdNumber, // QR's own id number field
                        ProductionOrderNumber = productionOrderNumber, // the PO this consumption belongs to
                        ConsumeInProductionOrderNumber = productionOrderNumber, // same PO, used by the downstream QR-balance update
                        ConsumedInId = idNumber, // the unit/ID number this component is being consumed into
                        ConsumedInProdSeriesID = projectContext.ProdSeriesId, // the project's production series
                        ConsumedInDrawingNumberID = projectContext.DrawingNumberId, // the project's own assembly drawing
                        CreatedBy = createdBy // the user performing this import
                    };

                    await ProcessSinglePrecheckItem(precheckRequestDto); // actually apply the consumption: update the precheck row, the QR's balance, and the consumption ledger

                    if (isShortfall && leftoverQuantity > 0) // this step only partially covered the row, so there's still an outstanding amount
                    {
                        // Close out this row (its IR/MSN/MRIR audit fields now belong to this QR)
                        // and open a fresh duplicate row carrying the leftover quantity forward -
                        // mirroring what the frontend does via RemainingPrecheck when a user
                        // manually adds a row after an under-covering scan. Excel import has no
                        // equivalent manual step, so it must be done here automatically.
                        await _precheckRepository.PrecheckForRemainingQuantityServiceRepo(new RejectPrecheckRequestDto
                        {
                            PrecheckDetailsId = targetPreCheckDetail.PrecheckDetailsId, // the row being closed out
                            DrawingNumberId = qrCodeDetails.DrawingNumberId, // the drawing the new duplicate row should carry
                            ProductionSeriesId = projectContext.ProdSeriesId, // the project's production series
                            IdNumber = idNumber.ToString(), // the unit/ID number, as text
                            ComponentType = qrCodeDetails.ComponentType, // component type carried onto the new row
                            RemainingQuantity = leftoverQuantity, // the outstanding amount the new row should open with
                            DuplicateRemarks = "Auto-duplicated: remaining quantity carried forward from Excel import", // audit note explaining why the new row exists
                            CreatedBy = createdBy // the user performing this import
                        });
                    }

                    touchedProjects[(productionOrderNumber, idNumber)] = viewPreCheckRequest; // remember this project so its overall status gets refreshed after the loop

                    result.Results.Add(new PrecheckExcelRowResultDto // record this row as a success in the response
                    {
                        QrCodeNumber = qrCodeNumber,
                        ProductionOrderNumber = productionOrderNumber,
                        IdNumber = idNumber,
                        Success = true,
                        Message = "Precheck completed successfully."
                    });
                    result.SuccessCount++; // bump the overall success counter
                }
                catch (Exception ex) // this row failed - log it and record it, then move on to the next row
                {
                    _logger.LogWarning(ex, "Error processing row (QR {QrCodeNumber}, PO {ProductionOrderNumber}) from Excel precheck import", qrCodeNumber, productionOrderNumber); // log the failure for diagnostics
                    result.Results.Add(new PrecheckExcelRowResultDto // record this row as a failure in the response
                    {
                        QrCodeNumber = qrCodeNumber,
                        ProductionOrderNumber = productionOrderNumber,
                        IdNumber = int.TryParse(row.IdNumberText, out var failedIdNumber) ? failedIdNumber : 0, // best-effort IdNumber for the error entry, even if it failed to parse
                        Success = false,
                        Message = ex.Message
                    });
                    result.FailedCount++; // bump the overall failure counter
                }
            }

            // Refresh overall status once per distinct project touched, after processing every row
            foreach (var viewPreCheckRequest in touchedProjects.Values) // for every project that had at least one row processed
            {
                var updatedPreCheckDetails = await _precheckRepository.ViewPrecheckDetails(viewPreCheckRequest); // re-fetch that project's current precheck rows
                var overallStatus = GetPrecheckStatus(updatedPreCheckDetails); // compute NotStarted/InProgress/Complete from those rows
                await _precheckRepository.UpdateProjectStatusDetails(viewPreCheckRequest, overallStatus); // persist the recomputed status
            }

            return result; // hand back the per-row results and summary counts to the caller
        }

        public Task<byte[]> DownloadPrecheckExcelTemplateAsync()
        {
            // Must mirror the layout expected by MakePrecheckFromExcelAsync:
            // Row 1: title, Row 2: blank, Row 3: header labels, Row 4+: data
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Precheck");

            worksheet.Cell(1, 1).Value = "BOM Details for Assembly";
            worksheet.Range(1, 1, 1, 11).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;

            var headers = new[]
            {
                "Level", "Drawing Number", "Nomenclature", "LN Item Code", "Component Type",
                "Qty", "Parent Drawing", "Find No", "ProductionOrderNumber", "IdNumber", "QRCodeNumber"
            };

            for (int col = 0; col < headers.Length; col++)
            {
                worksheet.Cell(3, col + 1).Value = headers[col];
            }

            var headerRow = worksheet.Row(3);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#E11584"); // Godrej Pink
            headerRow.Style.Font.FontColor = XLColor.White;
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }

        public async Task DeletePrecheckDetailsAsync(DeletePrecheckDetailsRequestDto request, int modifiedBy)
        {
            _logger.LogInformation(
                "Starting DeletePrecheckDetailsAsync, ProductionOrderNumber: {ProductionOrderNumber}, IdNumber: {IdNumber}, DrawingNumberId: {DrawingNumberId}",
                request.ProductionOrderNumber, request.IdNumber, request.DrawingNumberId);

            if (string.IsNullOrWhiteSpace(request.ProductionOrderNumber))
            {
                throw new ApplicationException("ProductionOrderNumber is required.");
            }

            // 1. Resolve the project (tbl_projectdetails) by ProductionOrderNumber + IdNumber, isactive=1
            var projectContext = await _precheckRepository.GetProjectContextByPoAndId(request.ProductionOrderNumber, request.IdNumber);
            if (projectContext == null)
            {
                throw new ApplicationException($"No active project found for ProductionOrderNumber '{request.ProductionOrderNumber}', IdNumber {request.IdNumber}.");
            }

            // 2. Find the precheck detail (tbl_projectprecheckdetails) for that project + drawing number
            var precheckDetail = await _precheckRepository.GetPrecheckDetailByProjectAndDrawing(projectContext.ProjectDetailsId, request.DrawingNumberId);
            if (precheckDetail == null)
            {
                throw new ApplicationException($"No precheck detail found for DrawingNumberId {request.DrawingNumberId} under this project.");
            }

            // 3. Refuse if it's already inactive
            if ((precheckDetail.IsActive ?? 0) != 1)
            {
                throw new ApplicationException("This precheck detail is already inactive.");
            }

            // 4. Soft-delete: isactive=0, isprecheckcomplete=1, isDeleted=1
            await _precheckRepository.DeleteProjectPrecheckDetail(precheckDetail.Id, modifiedBy);

            _logger.LogInformation("Successfully deleted precheck detail Id: {Id}", precheckDetail.Id);
        }

        public async Task RemovePrecheckDetailsAsync(DeletePrecheckDetailsRequestDto request, int modifiedBy)
        {
            _logger.LogInformation(
                "Starting RemovePrecheckDetailsAsync, ProductionOrderNumber: {ProductionOrderNumber}, IdNumber: {IdNumber}, DrawingNumberId: {DrawingNumberId}",
                request.ProductionOrderNumber, request.IdNumber, request.DrawingNumberId);

            if (string.IsNullOrWhiteSpace(request.ProductionOrderNumber))
            {
                throw new ApplicationException("ProductionOrderNumber is required.");
            }

            // 1. Resolve the project (tbl_projectdetails) by ProductionOrderNumber + IdNumber, isactive=1
            var projectContext = await _precheckRepository.GetProjectContextByPoAndId(request.ProductionOrderNumber, request.IdNumber);
            if (projectContext == null)
            {
                throw new ApplicationException($"No active project found for ProductionOrderNumber '{request.ProductionOrderNumber}', IdNumber {request.IdNumber}.");
            }

            // 2. Find the precheck detail (tbl_projectprecheckdetails) for that project + drawing number
            var precheckDetail = await _precheckRepository.GetPrecheckDetailByProjectAndDrawing(projectContext.ProjectDetailsId, request.DrawingNumberId);
            if (precheckDetail == null)
            {
                throw new ApplicationException($"No precheck detail found for DrawingNumberId {request.DrawingNumberId} under this project.");
            }

            // 3. Refuse if it's already inactive
            if ((precheckDetail.IsActive ?? 0) != 1)
            {
                throw new ApplicationException("This precheck detail is already inactive.");
            }

            // 4. Clear every consumption-related field and mark not-complete/inactive
            await _precheckRepository.RemoveProjectPrecheckDetail(precheckDetail.Id, modifiedBy);

            // 5. update QR status and quantity
            await _precheckRepository.UpdateQRCodeStatusQuantity(precheckDetail);

            _logger.LogInformation("Successfully removed precheck detail Id: {Id}", precheckDetail.Id);
        }

        public async Task<AddPrecheckComponentResponseDto> AddPrecheckComponentAsync(AddPrecheckComponentDto request, int createdBy)
        {
            _logger.LogInformation(
                "Starting AddPrecheckComponentAsync, AssemblyLnItemCode: {AssemblyLnItemCode}, ChildLnItemCode: {ChildLnItemCode}",
                request.AssemblyLnItemCode, request.ChildLnItemCode);

            if (string.IsNullOrWhiteSpace(request.AssemblyLnItemCode))
            {
                throw new ApplicationException("AssemblyLnItemCode is required.");
            }
            if (string.IsNullOrWhiteSpace(request.ChildLnItemCode))
            {
                throw new ApplicationException("ChildLnItemCode is required.");
            }

            // 1. Resolve every production order (tbl_productionordermaster) building this assembly
            var assemblyOrders = await _precheckRepository.GetAssemblyProductionOrdersByLnItemCode(request.AssemblyLnItemCode);
            if (assemblyOrders == null || assemblyOrders.Count == 0)
            {
                throw new ApplicationException($"No production order found for Assembly LnItemCode '{request.AssemblyLnItemCode}'.");
            }

            var assemblyDrawingNumberId = assemblyOrders.First().DrawingNumberId;
            var productionOrderNumbers = assemblyOrders.Select(a => a.ProductionOrderNumber).Distinct().ToList();
            var productionOrderIdByNumber = assemblyOrders.ToDictionary(a => a.ProductionOrderNumber, a => a.Id, StringComparer.OrdinalIgnoreCase);

            // 2. Resolve the child's BOM entry (quantity/componenttype) under this assembly -
            //    also validates the child truly belongs to the assembly's BOM.
            var bomDetail = await _precheckRepository.GetAssemblyChildBomDetail(assemblyDrawingNumberId, request.ChildLnItemCode);
            if (bomDetail == null)
            {
                throw new ApplicationException($"Child LnItemCode '{request.ChildLnItemCode}' was not found in Assembly '{request.AssemblyLnItemCode}' BOM.");
            }

            // 3. Resolve every existing project (tbl_projectdetails) for those production orders
            var projects = await _precheckRepository.GetProjectDetailsIdsByProductionOrderNumbers(productionOrderNumbers);
            if (projects == null || projects.Count == 0)
            {
                throw new ApplicationException($"No active project found for Assembly LnItemCode '{request.AssemblyLnItemCode}'.");
            }

            int addedCount = 0;
            int skippedCount = 0;

            foreach (var project in projects)
            {
                // 4. Skip if this child is already present under this project (regardless of isactive) -
                //    unless the caller explicitly marks this as an "ID" component, in which case another
                //    row is always allowed (ID components are individually serialized, not deduplicated).
                var existing = await _precheckRepository.GetPrecheckDetailByProjectAndDrawing(project.Id, bomDetail.ChildDrawingNumberId);
                if (existing != null && !string.Equals(request.ComponentType, "ID", StringComparison.OrdinalIgnoreCase))
                {
                    skippedCount++;
                    continue;
                }

                var productionOrderNumberId = productionOrderIdByNumber[project.ProductionOrderNumber];

                if (bomDetail.ComponentType == "ID")
                {
                    foreach (var _ in Enumerable.Range(1, (int)(bomDetail.Quantity ?? 0)))
                    {
                        await _precheckRepository.CreateProjectPrecheckDetailWithUnit(
                            bomDetail.ChildDrawingNumberId,
                            project.ProdSeriesId ?? 0,
                            project.Id,
                            1,
                            bomDetail.Unit,
                            bomDetail.ComponentType,
                            productionOrderNumberId,
                            createdBy);
                    }
                }
                else
                {
                    await _precheckRepository.CreateProjectPrecheckDetailWithUnit(
                        bomDetail.ChildDrawingNumberId,
                        project.ProdSeriesId ?? 0,
                        project.Id,
                        bomDetail.Quantity ?? 0,
                        bomDetail.Unit,
                        bomDetail.ComponentType,
                        productionOrderNumberId,
                        createdBy);
                }

                addedCount++;
            }

            _logger.LogInformation(
                "Completed AddPrecheckComponentAsync: ProjectsChecked={ProjectsChecked}, Added={Added}, Skipped={Skipped}",
                projects.Count, addedCount, skippedCount);

            return new AddPrecheckComponentResponseDto
            {
                ProjectsChecked = projects.Count,
                ComponentsAdded = addedCount,
                AlreadyPresentSkipped = skippedCount
            };
        }

        public async Task<List<ConsumedInComponentsResponseDto>> GetConsumedInComponentsAsync(int drawingNumberId)
        {
            _logger.LogInformation("Starting GetConsumedInComponentsAsync, DrawingNumberId: {DrawingNumberId}", drawingNumberId);

            if (drawingNumberId <= 0)
            {
                throw new ApplicationException("DrawingNumberId is required.");
            }

            var result = await _precheckRepository.GetConsumedInAssemblies(drawingNumberId);

            return result.Adapt<List<ConsumedInComponentsResponseDto>>();
        }

        private async Task<QRCodeDetailsResponseDto> ValidateAndGetQRCodeDetails(string qrCodeNumber)
        {
            var qrCodeDetails = await _qRCodeRepository.GetActiveQRcodeDetailsAsync(qrCodeNumber);
            if (qrCodeDetails == null)
            {
                throw new Exception($"QR Code {qrCodeNumber} is inactive or already consumed.");
            }

            if (qrCodeDetails.RemainingQuantity == 0)
            {
                throw new Exception($"QR Code {qrCodeNumber} is already consumed.");
            }

            if (qrCodeDetails.QrCodeStatusId != 1)
            {
                throw new ApplicationException($"QR Code {qrCodeNumber} is not ready for consumption.");
            }
            return qrCodeDetails;
        }

        private async Task ValidateComponentDrawing(List<ViewPreCheckResponse> preCheckResponses, int drawingNumberId)
        {
            var componentId = preCheckResponses.Find(x => x.IsPrecheckComplete == false);

            if (componentId == null)
            {
                throw new ApplicationException($"Drawing ID {drawingNumberId} is not valid");
            }
        }
        public int GetPrecheckStatus(List<ViewPreCheckResponse> precheckResponses)
        {
            if (precheckResponses == null || !precheckResponses.Any())
            {
                return 1;  //NotStarted;
            }

            // Check if any precheck is completed
            bool hasAnyCompleted = precheckResponses.Any(x => x.IsPrecheckComplete);

            // Check if all prechecks are completed
            bool areAllCompleted = precheckResponses.All(x => x.IsPrecheckComplete);

            if (!hasAnyCompleted)
            {
                return 1; //NotStarted;
            }
            else if (hasAnyCompleted && !areAllCompleted)
            {
                return 2; //InProgress;
            }
            else // all are completed
            {
                return 3;//Complete;
            }
        }

        public async Task<List<PendingPrecheckResponseDto>> GetPendingPrecheckAsync(PendingPrecheckRequestDto request)
        {
            // 1. Resolve every active production order matching whichever optional filters were supplied
            // (AssemblyDrawingNumberId / ProdSeriesId / ProductionOrderNumber all AND together when present).
            var matchingProductionOrders = await _productionOrderRepository.GetProductionOrdersForPendingPrecheckAsync(
                request.AssemblyDrawingNumberId,
                request.ProdSeriesId,
                request.ProductionOrderNumber,
                request.LnItemCode);

            var result = new List<PendingPrecheckResponseDto>();

            if (matchingProductionOrders.Count == 0)
            {
                return result;
            }

            // 2. Fetch every precheck row for every matching production order in a single round-trip,
            // instead of one query per order - the loop below used to call ViewPrecheckDetails once per
            // matching production order, which meant a broad filter (matching hundreds of orders) turned
            // into hundreds of sequential DB round-trips. Grouping by ProductionOrderNumber up front lets
            // the per-order logic below stay exactly the same, just against pre-fetched data.
            var allPrecheckRows = await _precheckRepository.ViewPrecheckDetailsForProductionOrders(
                matchingProductionOrders.Select(o => o.ProductionOrderNumber).ToList());

            var rowsByProductionOrder = allPrecheckRows
                .Where(x => x.ProductionOrderNumber != null)
                .GroupBy(x => x.ProductionOrderNumber!)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var productionOrder in matchingProductionOrders)
            {
                // 3. Group this order's rows by the specific unit id they belong to, so each unit id can be
                // judged on its own - a unit id with no rows at all (never started) simply won't have a group.
                var precheckRows = rowsByProductionOrder.TryGetValue(productionOrder.ProductionOrderNumber, out var poRows)
                    ? poRows
                    : new List<ViewPreCheckResponse>();

                var rowsByUnitId = precheckRows
                    .Where(x => x.StartIdNumber.HasValue)
                    .GroupBy(x => x.StartIdNumber!.Value)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // 4. Walk the production order's declared id range - or, when the optional IdNumber filter is
                // supplied, just that single unit - and keep only the unit ids that are not fully complete,
                // either partially done (some rows complete, not all) or not started at all - carrying each
                // unit's own child precheck rows alongside it.
                var pendingIdNumbers = new List<PendingPrecheckIdDto>();
                if (productionOrder.StartIdNumber.HasValue)
                {
                    var rangeStart = productionOrder.StartIdNumber.Value;
                    var rangeEnd = productionOrder.EndIdNumber ?? productionOrder.StartIdNumber.Value;

                    if (request.IdNumber.HasValue)
                    {
                        // Only consider this production order if the requested id actually falls within its
                        // declared range - otherwise skip it entirely (rangeStart > rangeEnd, no iterations).
                        if (request.IdNumber.Value < rangeStart || request.IdNumber.Value > rangeEnd)
                        {
                            rangeStart = 1;
                            rangeEnd = 0;
                        }
                        else
                        {
                            rangeStart = request.IdNumber.Value;
                            rangeEnd = request.IdNumber.Value;
                        }
                    }

                    // The already-computed per-row PrecheckStatus ("Pending"/"Completed"/"Updated") that
                    // request.StatusId (1/2) maps onto - null means the caller didn't ask for a status filter.
                    string? wantedRowStatus = request.StatusId switch
                    {
                        1 => "Pending",
                        2 => "Updated",
                        null => null,
                        _ => string.Empty // any other value matches nothing
                    };

                    for (var unitId = rangeStart; unitId <= rangeEnd; unitId++)
                    {
                        if (!rowsByUnitId.TryGetValue(unitId, out var unitRows))
                        {
                            // No precheck rows at all for this unit - not started, which only ever
                            // satisfies a "Pending" filter (there's nothing here that can be "Updated").
                            if (!request.StatusId.HasValue || wantedRowStatus == "Pending")
                            {
                                pendingIdNumbers.Add(new PendingPrecheckIdDto { IdNumber = unitId, Childs = new List<ViewPreCheckResponse>() });
                            }
                            continue;
                        }

                        // Judge completion by the same PrecheckStatus string shown in each row, not the
                        // isprecheckcomplete flag - a row can have isprecheckcomplete=0 while its computed
                        // status is already "Completed" (e.g. remainingquantity reached 0 without the flag
                        // being set), and that must not surface as a pending/updated component.
                        bool unitFullyComplete = unitRows.All(x => x.PrecheckStatus == "Completed");
                        if (!unitFullyComplete)
                        {
                            // Even on a partially-complete unit, only surface the components that are still
                            // Pending or Updated - components whose status is already "Completed" are left out.
                            var incompleteChilds = unitRows.Where(x => x.PrecheckStatus == "Pending" || x.PrecheckStatus == "Updated");
                            if (wantedRowStatus != null)
                            {
                                incompleteChilds = incompleteChilds.Where(x => x.PrecheckStatus == wantedRowStatus);
                            }
                            var finalChilds = incompleteChilds.ToList();

                            // When a status filter is supplied, a unit with nothing matching that status
                            // is dropped from the response rather than showing up with an empty Childs list.
                            if (!request.StatusId.HasValue || finalChilds.Count > 0)
                            {
                                pendingIdNumbers.Add(new PendingPrecheckIdDto { IdNumber = unitId, Childs = finalChilds });
                            }
                        }
                    }
                }

                // 5. Only include this production order if at least one of its units is still pending.
                if (pendingIdNumbers.Count > 0)
                {
                    productionOrder.PendingIdNumbers = pendingIdNumbers;
                    result.Add(productionOrder);
                }
            }

            return result;
        }

        public async Task<byte[]> ExportPendingPrecheckAsync(PendingPrecheckRequestDto request)
        {
            // Reuse the exact same filtering/status logic as PendingPrecheck - the Excel is just
            // that same response flattened to one row per pending component (per unit, per child).
            var orders = await GetPendingPrecheckAsync(request);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Pending Precheck");

            int headerRow = 1;
            var headers = new[]
            {
                "Sr No", "Assembly PO Number", "Assembly ID No", "Drawing Number", "LN Item Code",
                "Prod Series", "Qty", "Remaining Qty", "Created Date", "Status"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(headerRow, i + 1).Value = headers[i];
            }
            worksheet.SheetView.FreezeRows(1);

            int row = headerRow + 1;
            int srNo = 1;
            foreach (var order in orders)
            {
                foreach (var unit in order.PendingIdNumbers)
                {
                    foreach (var child in unit.Childs)
                    {
                        worksheet.Cell(row, 1).Value = srNo++;
                        worksheet.Cell(row, 2).Value = order.ProductionOrderNumber;
                        worksheet.Cell(row, 3).Value = unit.IdNumber;
                        worksheet.Cell(row, 4).Value = child.DrawingNumber;
                        worksheet.Cell(row, 5).Value = child.LnItemCode;
                        worksheet.Cell(row, 6).Value = order.ProductionSeries;
                        worksheet.Cell(row, 7).Value = child.Quantity;
                        worksheet.Cell(row, 8).Value = child.RemainingQuantity;

                        worksheet.Cell(row, 9).Value = child.CreatedDate;
                        worksheet.Cell(row, 9).Style.DateFormat.Format = "dd-MM-yyyy HH:mm";

                        worksheet.Cell(row, 10).Value = child.PrecheckStatus;

                        row++;
                    }
                }
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private ViewPreCheckRequest CreateViewPreCheckRequest(PrecheckRequestDto request)
        {
            return new ViewPreCheckRequest
            {
                Id = request.ConsumedInId,
                ProductionSeriesId = request.ConsumedInProdSeriesID,
                DrawingNumberId = request.ConsumedInDrawingNumberID,
                CreatedBy = request.CreatedBy
                //  ProductionOrderNumber = request.ProductionOrderNumber
            };
        }

        private async Task ProcessComponentType(
            MakePrecheckRequest precheckRequest
          )
        {
            switch (precheckRequest.ComponentType.ToUpper())
            {

                case "ID":
                    await ProcessIdComponent(precheckRequest);
                    break;

                case "BATCH":
                    await ProcessBatchComponent(precheckRequest);
                    break;

                case "FIM":
                case "SI":
                    await ProcessOtherComponent(precheckRequest);
                    break;

                default:
                    throw new ApplicationException(
                        $"Component type {precheckRequest.ComponentType} is not supported");
            }
        }

        private async Task ProcessIdComponent(MakePrecheckRequest precheckRequest)
        {
            // Update component consumption
            await _precheckRepository.UpdateIdComponentConsumption(precheckRequest);

            // Update precheck details
            await _precheckRepository.UpdatePrecheckDetails(precheckRequest);

            // Disable QR code
            await _qRCodeRepository.UpdateQrCodeDetails(precheckRequest.QrCodeNumber, precheckRequest.ConsumedDrawingNo, precheckRequest.RemainingQuantity);
        }

        private async Task ProcessBatchComponent(MakePrecheckRequest precheckRequest)
        {
            //Set IdNumber
            //precheckRequest.IdNumbers = ($"Batch-{precheckRequest.Id}");
            // Update component consumption
            await _precheckRepository.UpdateIdComponentConsumption(precheckRequest);

            // Update precheck details
            await _precheckRepository.UpdatePrecheckDetails(precheckRequest);

            // Disable QR code
            await _qRCodeRepository.UpdateQrCodeDetails(precheckRequest.QrCodeNumber, precheckRequest.ConsumedDrawingNo, precheckRequest.RemainingQuantity);
        }

        private async Task ProcessOtherComponent(MakePrecheckRequest precheckRequest)
        {
            //Set IdNumber
            //var Id = precheckRequest.ComponentType == "FIM" ? "FIM" : "SI";

            //precheckRequest.IdNumbers = Id;
            // Update component consumption
            await _precheckRepository.UpdateBatchComponentConsumption(precheckRequest);

            // Update precheck details
            await _precheckRepository.UpdatePrecheckDetails(precheckRequest);

            // Disable QR code
            await _qRCodeRepository.UpdateQrCodeDetails(precheckRequest.QrCodeNumber, precheckRequest.ConsumedDrawingNo, precheckRequest.RemainingQuantity);
        }


        //MakeOrder
        public async Task<List<MakeOrderResponseDto>> MakeOrder(MakeOrderRequestDto request)
        {
            List<MakeOrderResponseDto> response = new();

            try
            {
                var assemblyResponses = await _precheckRepository.GetPrecheckTemplateResponsesAsync(request.DrawingNumberId);

                foreach (var item in request.Ids)
                {
                    var orderDetails = await _precheckRepository.ValidateOrder(
                        (int)request.ProductionSeriesId,
                        request.DrawingNumberId,
                        request.ProductionOrderNumber,
                        item
                    );

                    if (orderDetails.Count != 0)
                    {
                        throw new ValidationException($"Order already generated for ID: {item}");
                    }

                    var projectDetails = await _precheckRepository.GetProjectDetails(new ViewPreCheckRequest
                    {
                        Id = item,
                        DrawingNumberId = request.DrawingNumberId,
                        ProductionSeriesId = request.ProductionSeriesId
                    });

                    if (projectDetails != null)
                    {
                        continue;
                    }

                    var precheckRequest = request.Adapt<MakeOrderRequest>();
                    precheckRequest.Id = item;

                    var projectdetails = await _precheckRepository.CreateProjectDetails(precheckRequest);

                    foreach (var assembly in assemblyResponses)
                    {
                        if (assembly.ComponentType == "ID")
                        {
                            foreach (var i in Enumerable.Range(1, (int)(assembly.Quantity ?? 0)))
                            {
                                var projectPrecheckRequest = new ProjectPrecheckRequest
                                {
                                    DrawingNumberId = assembly.DrawingNumberId,
                                    ProductionSeriesId = request.ProductionSeriesId,
                                    ProjectDetailsId = projectdetails,
                                    Quantity = 1,
                                    CreatedBy = request.CreatedBy,
                                    ComponentType = assembly.ComponentType
                                };

                                await _precheckRepository.CreateProjectPrecheckDetails(projectPrecheckRequest);
                            }
                        }
                        else
                        {
                            var projectPrecheckRequest = new ProjectPrecheckRequest
                            {
                                DrawingNumberId = assembly.DrawingNumberId,
                                ProductionSeriesId = request.ProductionSeriesId,
                                ProjectDetailsId = projectdetails,
                                Quantity = assembly.Quantity ?? 0,
                                CreatedBy = request.CreatedBy,
                                ComponentType = assembly.ComponentType
                            };

                            await _precheckRepository.CreateProjectPrecheckDetails(projectPrecheckRequest);
                        }
                    }
                }

                response = assemblyResponses.Adapt<List<MakeOrderResponseDto>>();

                // 1. Multiply TotalQuantity = Quantity * request.Ids.Count
                response.ForEach(r => r.TotalQuantity = r.Quantity * request.Ids.Count);

                // 2. Set AvailableQuantity using DrawingNumberId
                foreach (var responseDto in response)
                {
                    int quantity = await _precheckRepository.GetAvailableComponentQunatity(responseDto.DrawingNumberId);
                    responseDto.AvailableQuantity = quantity;
                }

                return response;
            }
            catch (ValidationException vex)
            {

                throw new ValidationException(vex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while generating the order.", ex);
            }
        }



        //view precheck API

        public async Task<List<ViewPreCheckResponse>> ViewPrecheckDetailsService(ViewPreCheckRequestDto request)
        {

            var precheckRequest = request.Adapt<ViewPreCheckRequest>();
            var result = await _precheckRepository.ViewPrecheckDetails(precheckRequest);

             
            return result;
        }

        public async Task<int?> GetPrecheckStatusDetailsService(ViewPreCheckRequestDto request)
        {

            var precheckRequest = request.Adapt<ViewPreCheckRequest>();

            var response = await _precheckRepository.GetProjectDetails(precheckRequest);

            if (response != null)
            {
                return response.PrecheckStatus;
            }

            return null;

        }

        public async Task<List<ViewPreCheckResponse>> ExportViewPrecheckDetailsService(ViewPreCheckRequestDto request)
        {

            var precheckRequest = request.Adapt<ViewPreCheckRequest>();
            var result = await _precheckRepository.ExportViewPrecheckDetails(precheckRequest);

            if (request.RemainingPrecheck==true)
            {
                result = result.Where(x => x.IsPrecheckComplete==false).ToList();
            }

            return result;
        }

        //Avaible precheck API

        public async Task<List<AvailableComponentModel>> AvailableComponentDetailsService(AvailableComponentFilterDto filter)
        {
            var qrCodeDetails = await _qRCodeRepository.GetActiveQRcodeDetailsAsync(filter.QrCode);
            if (qrCodeDetails == null)
            {
                throw new ApplicationException($"QR Code {filter.QrCode} is not active");
            }
            if (qrCodeDetails.QrCodeStatusId != 1)
            {
                throw new ApplicationException($"QR Code {filter.QrCode} is not ready for consumption");
            }

            var result = await _precheckRepository.GetAvailableComponentDetails(
                qrCodeDetails.DrawingNumberId,
                qrCodeDetails.ProductionSeriesId,
                filter.FromDate,
                filter.ToDate
            );
            return result;
        }

        public async Task<byte[]> GeneratePrecheckPdfAsync(List<ViewPreCheckResponse> preCheckResponses, ViewPreCheckRequestDto request)
        {
            if (preCheckResponses == null || preCheckResponses.Count == 0)
                throw new ArgumentNullException(nameof(preCheckResponses));

            QuestPDF.Settings.License = LicenseType.Community;

            var now = DateTime.Now;
            string drawingNumber = null;
            string drawingLnItemCode = null;

            if (request?.DrawingNumberId.HasValue == true && request.DrawingNumberId > 0)
            {
                var drawingDetails = await _commonRepository.GetDrawingNumberById(request.DrawingNumberId.Value);
                drawingNumber = drawingDetails?.DrawingNumber;
                drawingLnItemCode = drawingDetails?.LnItemCode;
            }

            drawingNumber ??= preCheckResponses
                .FirstOrDefault(x => !string.IsNullOrEmpty(x?.DrawingNumber))?.DrawingNumber ?? "-";

            string productionSeriesName = null;
            if (request?.ProductionSeriesId.HasValue == true && request.ProductionSeriesId > 0)
            {
                var productionSeriesDetails = await _commonRepository.GetProductionSeriesById(request.ProductionSeriesId.Value);
                productionSeriesName = productionSeriesDetails?.ProductionSeries;
            }

            // These LN Item Codes are prechecked under series "H", but must be printed as "SH" on the export
            if (string.Equals(productionSeriesName, "H", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(drawingLnItemCode)
                && LnItemCodesPrintedAsSh.Contains(drawingLnItemCode))
            {
                productionSeriesName = "SH";
            }

            var consumedInDrawing = (request?.Id.HasValue == true && request.Id > 0)
                ? $"{drawingNumber}/{request.Id}"
                : drawingNumber;

            consumedInDrawing = !string.IsNullOrEmpty(productionSeriesName)
                ? $"{productionSeriesName}/{consumedInDrawing}"
                : consumedInDrawing;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    // ---- Header
                    page.Header().Column(header =>
                    {
                        header.Item().Text($"Pre-check List for : {consumedInDrawing}")
                            .SemiBold().FontSize(11).AlignCenter();

                        header.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Printed on: {now:dd MMMM yy/ HH:mm tt}").FontSize(8);
                            row.RelativeItem().AlignRight().Text("ANNEXURE-I").FontSize(8);
                        });

                        header.Item().PaddingBottom(10);
                    });

                    // ---- Content Table
                    page.Content().Table(table =>
                    {
                        string[] headers = {
                "Sr. No.", "Drawing No.", "Nomenclature", "Qty", "Unit", "ID No.",
                "IR No.", "MSN No.", "MRIR No", "Find Number", "Status", "Remarks", "UserName", "Date"
            };

                        float[] columnWidths = {
                25, 80, 60, 35, 35, 60, 70, 70, 55, 55, 50, 55, 55, 75
            };

                        table.ColumnsDefinition(columns =>
                        {
                            foreach (var width in columnWidths)
                                columns.ConstantColumn(width);
                        });

                        // ---- Table Header Row
                        table.Header(header =>
                        {
                            foreach (var title in headers)
                            {
                                header.Cell().Background("#E0E0E0").Border(1).Padding(4).Text(title).Bold();
                            }
                        });

                        // ---- Data Rows
                        int srNo = 1;
                        foreach (var item in preCheckResponses)
                        {
                            string[] cells = {
                    srNo.ToString(),
                    item.DrawingNumber ?? "",
                    item.Nomenclature ?? "",
                    item.Quantity?.ToString() ?? "",
                    item.Unit ?? "",
                    item.IdNumber ?? "",
                    item.IrNumber ?? "",
                    item.MsnNumber ?? "",
                    item.MrirNumber ?? "",
                    item.FindNo ?? "",
                    (item.IsRejected ?? false) ? "Rejected" : "Active",
                    item.Remarks ?? "",
                    item.Username ?? "",
                    item.ModifiedDate?.ToString("dd/MM/yyyy HH:mm:ss") ?? ""
                };

                            foreach (var cell in cells)
                            {
                                table.Cell()
                                    .Border(1)
                                    .Padding(4)
                                    .Text(cell)
                                    .WrapAnywhere();
                            }

                            srNo++;
                        }
                    });

                    // ---- Footer Row
                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text("Sign of QC representative");
                        row.RelativeItem().AlignCenter().Text("Sign of QC representative");
                        row.RelativeItem().AlignRight().Text("Sign of QA / MSQAA Representative");
                    });
                });
            });



            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }

        public Task<List<GetAvailableComponentsResponse>> GetAvailableComponentService(GetAvailableComponentsRequest request)
        {
            var response = _precheckRepository.GetAvailableComponentForOrder(request);
            return response;
        }

        public async Task<int> RejectAndDuplicatePrecheck(RejectPrecheckRequestDto request)
        {
            _logger.LogInformation($"Request for PrecheckService:RejectAndDuplicatePrecheck for PrecheckDetailsId: {request.PrecheckDetailsId}");
            try
            {
                var result = await _precheckRepository.RejectAndDuplicatePrecheck(request);
                _logger.LogInformation($"Successfully rejected and duplicated precheck for PrecheckDetailsId: {request.PrecheckDetailsId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in PrecheckService:RejectAndDuplicatePrecheck for PrecheckDetailsId: {request.PrecheckDetailsId}");
                throw;
            }
        }

        public async Task<UpdateQuantityResponseDto> UpdateQuantity(
     string productionOrderNumber,
     UpdateMaterialQuantityRequestDto request,
     string assemblyDrawingNo,
     int userId)
        {
            _logger.LogInformation(
                "Request for UpdateQuantity, DrawingNumberId: {Id}",
                request.DrawingnumberId);

            try
            {
                // 1️⃣ Validate input
                if (request.UpdatedQuantity <= 0)
                {
                    throw new ApplicationException("Updated quantity must be greater than zero.");
                }

                // 2️⃣ Get CURRENT remaining quantity from DB
                decimal? currentRemaining;
                try
                {
                    currentRemaining = await _precheckRepository
                        .GetBatchTotalQuantity(request);
                }
                catch (Exception)
                {
                    throw new ApplicationException(
                        "Failed to retrieve current remaining quantity.");
                }

                // 3️⃣ Check if enough quantity is available
                if (request.UpdatedQuantity > currentRemaining)
                {
                    throw new ApplicationException(
                        $"Entered quantity exceeds available balance ({currentRemaining}).");
                }

                // 4️⃣ Subtract from remaining
                decimal? newRemainingQuantity = (currentRemaining ?? 0) - request.UpdatedQuantity;

                //  Update remianing qty in qrcodedetails DB
                decimal remainingComponentQuantity=await _precheckRepository.UpdateComponentRemaningQuantity(
                    request,
                    request.UpdatedQuantity?? 0);

                await _precheckRepository.UpdateQrcodeQuantity(request.QrCodeNumber, newRemainingQuantity?? 0);
                
               
                // 6️⃣ Return response
                return new UpdateQuantityResponseDto
                {
                    RemainingQuantity = remainingComponentQuantity,
                    DrawingnumebrID = request.DrawingnumberId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in PrecheckService:UpdateQuantity");
                throw;
            }
        }

        public async Task<int> PrecheckForRemainingQuantityService(RejectPrecheckRequestDto request)
        {
            _logger.LogInformation($"Request for PrecheckService:RejectAndDuplicatePrecheck for PrecheckDetailsId: {request.PrecheckDetailsId}");
            try
            {
                var result = await _precheckRepository.PrecheckForRemainingQuantityServiceRepo(request);
                _logger.LogInformation($"Successfully rejected and duplicated precheck for PrecheckDetailsId: {request.PrecheckDetailsId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in PrecheckService:RejectAndDuplicatePrecheck for PrecheckDetailsId: {request.PrecheckDetailsId}");
                throw;
            }
        }

        public async Task<bool> ResetRemainingQuantityService(ResetRemainingQuantityDto remainingQuantityDto)
        {
            // Validate DrawingNumber exists
            var drawingDetails = await _precheckRepository.GetDrawingNumberIdAsync(remainingQuantityDto.DrawingNumberId);
            if (!drawingDetails)
            {
                throw new ApplicationException($"Drawing Number Id {remainingQuantityDto.DrawingNumberId} does not exist.");
            }

            var result = await _precheckRepository.ResetRemainingQuantity(
                remainingQuantityDto
            );

            return result;
        }
    }
}