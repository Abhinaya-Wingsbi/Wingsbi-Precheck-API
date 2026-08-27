using Godrej.Precheck.Models.DTOs.Testing;
using Godrej.Precheck.Repository.Repository.TestingRepository;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions;

namespace Godrej.Precheck.Service.Service.TestingService
{
    public class TestingService : ITestingService
    {
        private readonly ILogger<TestingService> _logger;
        private readonly ITestingRepository _testingRepository;

        public TestingService(ILogger<TestingService> logger, ITestingRepository testingRepository)
        {
            _logger = logger;
            _testingRepository = testingRepository;
        }

        public async Task<TemplateFieldsResponseDto> GetTemplateFieldsByDrawingNumberAsync(string drawingNumber, string? msnNumber, int? msnQuantity, int? stageId = null)
        {
            _logger.LogInformation("Fetching template fields from service for DrawingNumber: {DrawingNumber}, MsnNumber: {MsnNumber}", drawingNumber, msnNumber);

            try
            {
                var flatList = await _testingRepository.GetTemplateFieldsByDrawingNumberAsync(drawingNumber, msnNumber, stageId);

                var response = new TemplateFieldsResponseDto();

                response.HeaderFields = flatList
                    .Where(f => !f.IsRowField)
                    .OrderBy(f => f.DisplayOrder)
                    .Select(f => new HeaderFieldDto
                    {
                        Id = f.Id,
                        FieldName = f.FieldName,
                        FieldLabel = f.FieldLabel,
                        FieldValue = f.FieldValue,
                        FieldType = f.FieldType,
                        DisplayOrder = f.DisplayOrder
                    })
                    .ToList();

                var grouped = flatList
                    .Where(f => f.IsRowField)
                    .GroupBy(f => new
                    {
                        f.FormulaHeaderId,
                        f.FormulaHeaderName,
                        f.FormulaHeaderValue
                    })
                    .OrderBy(g => g.Min(f => f.DisplayOrder));

                int rowCount = msnQuantity ?? 1;

                foreach (var group in grouped)
                {
                    var fields = group
                        .OrderBy(f => f.DisplayOrder)
                        .Select(f => new ColumnFieldDefinitionDto
                        {
                            Id = f.Id,
                            FieldName = f.FieldName,
                            FieldLabel = f.FieldLabel,
                            FieldType = f.FieldType,
                            DisplayOrder = f.DisplayOrder
                        })
                        .ToList();

                    var rows = Enumerable.Range(1, rowCount)
                        .Select(i =>
                        {
                            var row = new Dictionary<string, object?> { ["rowIndex"] = (object?)i };
                            foreach (var field in fields)
                                row[field.FieldName] = null;
                            return row;
                        })
                        .ToList();

                    response.ColumnGroups.Add(new ColumnGroupDto
                    {
                        FormulaHeaderId = group.Key.FormulaHeaderId ?? 0,
                        ColumnName = group.Key.FormulaHeaderName ?? string.Empty,
                        ColumnValue = group.Key.FormulaHeaderValue,
                        Fields = fields,
                        Rows = rows
                    });
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching template fields from service for DrawingNumber: {DrawingNumber}", drawingNumber);
                throw;
            }
        }

        public async Task<InsertInspectionValuesResponseDto> InsertInspectionValuesAsync(
            InsertInspectionValuesRequestDto request)
        {
            _logger.LogInformation(
                "Processing inspection values for DrawingNumber: {DrawingNumber}",
                request.DrawingNumber);

            try
            {
                if (request.Values == null || request.Values.Count == 0)
                    throw new ValidationException("At least one field value is required.");

                var templateId = await _testingRepository.GetTemplateIdByDrawingNumberAsync(request.DrawingNumber);
                if (templateId == null)
                    throw new ValidationException($"No template mapping found for drawing number '{request.DrawingNumber}'.");

                var templateFields = await _testingRepository.GetTemplateFieldsByTemplateIdAsync(templateId.Value);
                if (templateFields.Count == 0)
                    throw new ValidationException($"No fields found for template id {templateId.Value}.");

                var fieldLookup = templateFields
                    .GroupBy(x => x.FieldName, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

                var invalidFields = request.Values
                    .Where(x => string.IsNullOrWhiteSpace(x.FieldName)
                             || !fieldLookup.ContainsKey(x.FieldName))
                    .Select(x => x.FieldName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (invalidFields.Count > 0)
                    throw new ValidationException(
                        $"Invalid fieldName(s): {string.Join(", ", invalidFields)}");

                var valuesToInsert = request.Values
                    .Select(x => new TemplateFieldValueInsertDto
                    {
                        FieldId = fieldLookup[x.FieldName].Id,
                        Value = x.Value
                    })
                    .ToList();

                var inspectionMasterId = await _testingRepository.InsertInspectionValuesAsync(
                    templateId.Value,
                    request.DrawingNumber,
                    valuesToInsert);

                return new InsertInspectionValuesResponseDto
                {
                    Success = true,
                    Message = "Inspection values inserted successfully.",
                    InspectionMasterId = inspectionMasterId,
                    InsertedCount = valuesToInsert.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing inspection values for DrawingNumber: {DrawingNumber}",
                    request.DrawingNumber);
                throw;
            }
        }

        public async Task<byte[]> ExportInspectionAsPdfAsync(string drawingNumber, string? msnNumber = null, int msnQuantity = 4)
        {
            _logger.LogInformation("Exporting inspection PDF for DrawingNumber: {DrawingNumber}, MsnNumber: {MsnNumber}, MsnQuantity: {MsnQuantity}", drawingNumber, msnNumber, msnQuantity);

            try
            {
                if (string.IsNullOrWhiteSpace(drawingNumber))
                    throw new ValidationException("Drawing number is required.");

                if (msnQuantity < 1) msnQuantity = 1;

                var exportData = await _testingRepository.GetInspectionExportDataAsync(drawingNumber, msnNumber);
                if (exportData == null)
                    throw new ValidationException($"No inspection found for drawing number '{drawingNumber}'.");

                if (string.IsNullOrWhiteSpace(exportData.HtmlTemplate))
                    throw new ValidationException($"No export template found for drawing number '{drawingNumber}'.");

                var fieldValues = await _testingRepository.GetFieldValuesForExportAsync(drawingNumber, msnNumber);

                var html = exportData.HtmlTemplate;

                // Inject the row count so the JS buildRow loop runs the correct number of iterations.
                html = html.Replace("{{MSN_QUANTITY}}", msnQuantity.ToString());

                // Inject drawing number directly from export data (not a user-entered field).
                html = html.Replace("{{DRAWING_NUMBER}}", exportData.DrawingNumber ?? string.Empty);

                // Replace image placeholders ({{IMAGE_1}}, {{IMAGE_2}}, ...) with base64 data URIs from DB.
                var images = await _testingRepository.GetTemplateImagesAsync(exportData.TemplateId);
                foreach (var img in images)
                {
                    var dataUri = img.ToDataUri();
                    if (!string.IsNullOrWhiteSpace(dataUri))
                        html = html.Replace($"{{{{{img.PlaceholderKey}}}}}", dataUri);
                }

                var values = BuildExportValues(fieldValues, msnQuantity);

                // Support other {{placeholder}} substitutions (header fields, etc.)
                if (Regex.IsMatch(html, @"\{\{[^}]+\}\}", RegexOptions.IgnoreCase))
                    html = ApplyHandlebarSubstitutions(html, fieldValues, values);

                var pdfBytes = await ConvertHtmlToPdfAsync(html, values);

                _logger.LogInformation("PDF exported successfully for DrawingNumber: {DrawingNumber}", drawingNumber);
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting PDF for DrawingNumber: {DrawingNumber}", drawingNumber);
                throw;
            }
        }

        // Builds flat values dict: fixed fields keyed by "field_name", row N fields by "field_name_N".
        // Also propagates fixed field values to per-row indexed keys so templates that show
        // fixed fields inside each row (e.g. route_card_no_1, route_card_no_2) get populated.
        private static IReadOnlyDictionary<string, string> BuildExportValues(List<FieldValueExportDto> fieldValues, int msnQuantity = 4)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var fv in fieldValues.Where(x => !string.IsNullOrWhiteSpace(x.FieldName)))
            {
                var key = fv.RowNumber == 0
                    ? fv.FieldName.Trim()
                    : $"{fv.FieldName.Trim()}_{fv.RowNumber}";

                if (!values.TryGetValue(key, out var existing) ||
                    (string.IsNullOrEmpty(existing) && !string.IsNullOrEmpty(fv.FieldValue)))
                    values[key] = fv.FieldValue ?? string.Empty;
            }

            // Determine which row numbers have data; default to rows 1-4 if none found.
            var rowNums = fieldValues.Where(x => x.RowNumber > 0).Select(x => x.RowNumber)
                .Distinct().OrderBy(x => x).ToList();
            if (!rowNums.Any()) rowNums = Enumerable.Range(1, msnQuantity).ToList();

            // Propagate each fixed field value to indexed versions (field_name_1, field_name_2, ...)
            // so templates can display the same fixed value in every row without duplicate ids.
            foreach (var fieldName in fieldValues
                .Where(x => x.RowNumber == 0 && !string.IsNullOrWhiteSpace(x.FieldName))
                .Select(x => x.FieldName.Trim())
                .Distinct())
            {
                if (!values.TryGetValue(fieldName, out var fixedVal) || string.IsNullOrEmpty(fixedVal))
                    continue;
                foreach (var rowNum in rowNums)
                {
                    var indexedKey = $"{fieldName}_{rowNum}";
                    if (!values.TryGetValue(indexedKey, out var existing) || string.IsNullOrEmpty(existing))
                        values[indexedKey] = fixedVal;
                }
            }

            // Propagate P1 signature values to P2 (same signatories appear on both pages).
            // P2 fields are in the template HTML but not tracked separately in tbl_template_fields.
            foreach (var p1Key in new[] { "prepared_by_p1", "checked_by_p1", "verified_by_p1", "approved_by_p1" })
            {
                if (values.TryGetValue(p1Key, out var p1Val) && !string.IsNullOrEmpty(p1Val))
                {
                    var p2Key = p1Key.Replace("_p1", "_p2");
                    if (!values.TryGetValue(p2Key, out var existing) || string.IsNullOrEmpty(existing))
                        values[p2Key] = p1Val;
                }
            }

            return values;
        }

        // Handles legacy {{placeholder}} and {{#each rows}} template format.
        private static string ApplyHandlebarSubstitutions(
            string html,
            List<FieldValueExportDto> fieldValues,
            IReadOnlyDictionary<string, string> flatValues)
        {
            var baseValues = new Dictionary<string, string>(flatValues, StringComparer.OrdinalIgnoreCase);

            var rowGroups = fieldValues
                .Where(x => x.RowNumber > 0 && !string.IsNullOrWhiteSpace(x.FieldName))
                .GroupBy(x => x.RowNumber)
                .OrderBy(g => g.Key)
                .ToList();

            html = Regex.Replace(
                html,
                @"\{\{#each\s+rows\}\}(.*?)\{\{\/each\}\}",
                match =>
                {
                    if (!rowGroups.Any()) return string.Empty;
                    var rowBlock = match.Groups[1].Value;
                    var sb = new System.Text.StringBuilder();
                    foreach (var rowGroup in rowGroups)
                    {
                        var rowValues = new Dictionary<string, string>(baseValues, StringComparer.OrdinalIgnoreCase);
                        foreach (var fv in rowGroup)
                            rowValues[fv.FieldName.Trim()] = fv.FieldValue ?? string.Empty;
                        sb.Append(ReplacePlaceholders(rowBlock, rowValues));
                    }
                    return sb.ToString();
                },
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            html = ReplacePlaceholders(html, baseValues);
            html = Regex.Replace(html, @"\{\{[^}]+\}\}", string.Empty, RegexOptions.IgnoreCase);
            return html;
        }

        // Pre-injects a fill script into the HTML so values are embedded before Puppeteer renders.
        // The script runs synchronously after buildRow completes (both are in </body>).
        private static string InjectFillScript(string html, IReadOnlyDictionary<string, string> values)
        {
            if (values.Count == 0) return html;

            var json = System.Text.Json.JsonSerializer.Serialize(values);
            var script = $@"<script>
(function(){{
  var __v={json};
  function fill(){{
    Object.keys(__v).forEach(function(k){{
      var el=document.getElementById(k);
      if(!el)return;
      var v=__v[k];if(!v)return;
      if(el.tagName==='TEXTAREA'){{el.value=v;el.textContent=v;return;}}
      if(el.tagName==='IMG'){{el.src=v;el.style.display='block';return;}}
      if(el.tagName!=='INPUT')return;
      var cs=window.getComputedStyle(el);
      var sp=document.createElement('span');
      sp.style.cssText='display:inline-block;border-bottom:1px solid #000;min-width:'
        +Math.max(el.offsetWidth||0,40)+'px;font-size:'+(cs.fontSize||'10px')
        +';font-family:'+(cs.fontFamily||'Arial,sans-serif')
        +';vertical-align:bottom;padding:0 2px;color:#000;text-align:center;';
      sp.textContent=v;
      if(el.parentNode)el.parentNode.replaceChild(sp,el);
    }});
  }}
  if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',fill);
  else fill();
}})();
</script>";

            var idx = html.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
            return idx >= 0
                ? html.Substring(0, idx) + script + html.Substring(idx)
                : html + script;
        }

        private static async Task<byte[]> ConvertHtmlToPdfAsync(string html, IReadOnlyDictionary<string, string> values)
        {
            // Pre-inject values into HTML so the fill script runs synchronously after buildRow.
            html = InjectFillScript(html, values);

            var executablePath = GetBrowserExecutablePath();

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = executablePath,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            });

            await using var page = await browser.NewPageAsync();
            await page.SetViewportAsync(new ViewPortOptions { Width = 1400, Height = 900 });
            await page.SetContentAsync(html, new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            });

            return await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                Landscape = true,
                MarginOptions = new MarginOptions
                {
                    Top = "8mm",
                    Bottom = "8mm",
                    Left = "8mm",
                    Right = "8mm"
                }
            });
        }

        private static string GetBrowserExecutablePath()
        {
            var browserPaths = new[]
            {
                @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
            };

            var executablePath = browserPaths.FirstOrDefault(File.Exists);
            if (!string.IsNullOrWhiteSpace(executablePath))
                return executablePath;

            throw new ValidationException(
                "PDF export browser executable was not found. Please install Google Chrome or Microsoft Edge on the server.");
        }

        private static string ReplacePlaceholders(string template, IReadOnlyDictionary<string, string> values)
        {
            return Regex.Replace(
                template,
                @"\{\{(?:this\.)?([a-zA-Z0-9_]+)\}\}",
                match =>
                {
                    var key = match.Groups[1].Value;
                    if (!values.TryGetValue(key, out var rawValue))
                        return string.Empty;

                    if (key.EndsWith("_checked", StringComparison.OrdinalIgnoreCase))
                        return IsCheckedValue(rawValue) ? "checked" : string.Empty;

                    return WebUtility.HtmlEncode(rawValue);
                },
                RegexOptions.IgnoreCase);
        }

        public async Task<List<DrawingStageStatusDto>> GetDrawingStageStatusAsync()
        {
            _logger.LogInformation("Fetching drawing stage status list.");
            try
            {
                return await _testingRepository.GetDrawingStageStatusAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching drawing stage status list.");
                throw;
            }
        }

        public async Task<SaveStageDataResponseDto> SaveStageDataAsync(SaveStageDataRequestDto request)
        {
            _logger.LogInformation(
                "Saving stage {StageId} data for DrawingNumber: {DrawingNumber}",
                request.StageId, request.DrawingNumber);

            try
            {
                if (string.IsNullOrWhiteSpace(request.MsnNumber))
                    throw new ValidationException("MsnNumber is required — it identifies which inspection instance this data belongs to.");

                if (request.StageId < 1 || request.StageId > 3)
                    throw new ValidationException("StageId must be 1, 2, or 3.");

                if (request.Rows == null || request.Rows.Count == 0)
                    throw new ValidationException("At least one row is required.");

                if (request.TotalRows <= 0)
                    throw new ValidationException("TotalRows must be greater than 0.");

                var msnNumber = request.MsnNumber.Trim();

                var templateId = await _testingRepository.GetTemplateIdByDrawingNumberAsync(request.DrawingNumber);
                if (templateId == null)
                    throw new ValidationException(
                        $"No template mapped for drawing number '{request.DrawingNumber}'.");

                // Load stage row fields and fixed fields in parallel
                var stageFieldsTask = _testingRepository.GetStageFieldsAsync(templateId.Value, request.StageId);
                var fixedFieldsTask = _testingRepository.GetFixedFieldsAsync(templateId.Value);
                await Task.WhenAll(stageFieldsTask, fixedFieldsTask);

                var stageFields = stageFieldsTask.Result;
                var fixedFields = fixedFieldsTask.Result;

                if (stageFields.Count == 0)
                    throw new ValidationException(
                        $"No fields found for stage {request.StageId} of template {templateId.Value}.");

                var stageLookup = stageFields
                    .GroupBy(f => f.FieldName, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

                var fixedLookup = fixedFields
                    .GroupBy(f => f.FieldName, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

                // Validate stage row field names
                var invalidFields = request.Rows
                    .SelectMany(r => r.Fields)
                    .Where(f => !string.IsNullOrWhiteSpace(f.FieldName)
                             && !stageLookup.ContainsKey(f.FieldName))
                    .Select(f => f.FieldName)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (invalidFields.Count > 0)
                    throw new ValidationException(
                        $"Invalid field name(s) for stage {request.StageId}: {string.Join(", ", invalidFields)}");

                // Merge FixedFields + HeaderFields (frontend shape: fieldValue) + extra top-level properties
                var allFixedRequests = new List<StageFieldValueDto>(request.FixedFields);

                if (request.HeaderFields != null)
                {
                    foreach (var hf in request.HeaderFields)
                    {
                        if (!string.IsNullOrWhiteSpace(hf.FieldName))
                            allFixedRequests.Add(new StageFieldValueDto
                            {
                                FieldName = hf.FieldName,
                                Value     = hf.FieldValue
                            });
                    }
                }

                if (request.ExtraProperties != null)
                {
                    foreach (var kvp in request.ExtraProperties)
                    {
                        allFixedRequests.Add(new StageFieldValueDto
                        {
                            FieldName = kvp.Key,
                            Value     = kvp.Value.ValueKind == System.Text.Json.JsonValueKind.String
                                            ? kvp.Value.GetString()
                                            : kvp.Value.ToString()
                        });
                    }
                }

                // Build row values
                var rowValuesToInsert = request.Rows
                    .SelectMany(row => row.Fields
                        .Where(f => !string.IsNullOrWhiteSpace(f.FieldName)
                                 && stageLookup.ContainsKey(f.FieldName))
                        .Select(f => new StageRowValueInsertDto
                        {
                            FieldId   = stageLookup[f.FieldName],
                            Value     = f.Value,
                            RowNumber = row.RowNumber
                        }))
                    .ToList();

                // Build fixed field values — silently skip unknown names so row data always saves
                var fixedValuesToInsert = allFixedRequests
                    .Where(f => !string.IsNullOrWhiteSpace(f.FieldName)
                             && fixedLookup.ContainsKey(f.FieldName))
                    .Select(f => new StageRowValueInsertDto
                    {
                        FieldId   = fixedLookup[f.FieldName],
                        Value     = f.Value,
                        RowNumber = 0
                    })
                    .ToList();

                // Save fixed fields under this MSN instance's shared master (row_number=0 in the child table)
                if (fixedValuesToInsert.Count > 0)
                    await _testingRepository.SaveFixedFieldsAsync(request.DrawingNumber, msnNumber, fixedValuesToInsert);

                // Save each row's stage values under the SAME shared master (row_number=rowNumber in the child table)
                var rowGroups = rowValuesToInsert.GroupBy(v => v.RowNumber);
                int lastMasterId = 0;

                foreach (var group in rowGroups)
                {
                    lastMasterId = await _testingRepository.SaveRowDataAsync(
                        request.DrawingNumber,
                        msnNumber,
                        request.StageId,
                        group.Key,
                        request.TotalRows,
                        group.ToList());
                }

                return new SaveStageDataResponseDto
                {
                    Success   = true,
                    Message   = $"Stage {request.StageId} data saved successfully.",
                    MasterId  = lastMasterId,
                    StageId   = request.StageId,
                    RowsSaved = request.Rows.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error saving stage {StageId} data for DrawingNumber: {DrawingNumber}",
                    request.StageId, request.DrawingNumber);
                throw;
            }
        }

        public async Task<GetStageDataResponseDto> GetStageDataAsync(string drawingNumber, string msnNumber, int stageId)
        {
            _logger.LogInformation(
                "Getting stage {StageId} data for DrawingNumber: {DrawingNumber}, MsnNumber: {MsnNumber}",
                stageId, drawingNumber, msnNumber);

            try
            {
                var master = await _testingRepository.GetInspectionMasterStatusAsync(drawingNumber, msnNumber);

                if (master == null)
                {
                    var response = new GetStageDataResponseDto
                    {
                        DrawingNumber = drawingNumber,
                        MsnNumber     = msnNumber,
                        StageId       = stageId,
                        TotalRows     = 0
                    };

                    if (stageId == 2 || stageId == 3)
                    {
                        var templateId = await _testingRepository.GetTemplateIdByDrawingNumberAsync(drawingNumber);
                        if (templateId != null)
                            response.HeaderFields = await _testingRepository.GetStage10FieldsWithValuesAsync(templateId.Value, drawingNumber, msnNumber);
                    }

                    return response;
                }

                // Fetch row values, fixed field values in parallel; also header fields for stage 2/3
                var rowValuesTask   = _testingRepository.GetStageRowValuesByDrawingAsync(drawingNumber, msnNumber, stageId);
                var fixedValuesTask = _testingRepository.GetFixedFieldValuesByDrawingAsync(drawingNumber, msnNumber);
                await Task.WhenAll(rowValuesTask, fixedValuesTask);

                var rows = rowValuesTask.Result
                    .GroupBy(r => r.RowNumber)
                    .OrderBy(g => g.Key)
                    .Select(g => new StageRowDataDto
                    {
                        RowNumber = g.Key,
                        Fields    = g.Select(r => new StageFieldValueDto
                        {
                            FieldName = r.FieldName,
                            Value     = r.FieldValue
                        }).ToList()
                    })
                    .ToList();

                var fixedFields = fixedValuesTask.Result
                    .Select(r => new StageFieldValueDto
                    {
                        FieldName = r.FieldName,
                        Value     = r.FieldValue
                    })
                    .ToList();

                // stageid=0 fixed fields are "always visible" header fields — fetched for every stage now,
                // not just 2/3.
                var headerFields = new List<HeaderFieldValueDto>();
                if (stageId == 2 || stageId == 3)
                {
                    var templateId = await _testingRepository.GetTemplateIdByDrawingNumberAsync(drawingNumber);
                    if (templateId != null)
                        headerFields = await _testingRepository.GetStage10FieldsWithValuesAsync(templateId.Value, drawingNumber, msnNumber);
                }

                return new GetStageDataResponseDto
                {
                    DrawingNumber         = drawingNumber,
                    MsnNumber             = msnNumber,
                    StageId               = stageId,
                    TotalRows             = master.TotalRows,
                    Stage1Completed       = master.Stage1Completed,
                    Stage2Completed       = master.Stage2Completed,
                    Stage3Completed       = master.Stage3Completed,
                    CurrentStageCompleted = stageId switch
                    {
                        1 => master.Stage1Completed,
                        2 => master.Stage2Completed,
                        3 => master.Stage3Completed,
                        _ => false
                    },
                    HeaderFields = headerFields,
                    FixedFields  = fixedFields,
                    Rows         = rows
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting stage {StageId} data for DrawingNumber: {DrawingNumber}",
                    stageId, drawingNumber);
                throw;
            }
        }

        public async Task<List<PrecheckCompletedComponentDto>> GetPrecheckCompletedComponentsAsync()
        {
            _logger.LogInformation("Fetching precheck completed components from service.");
            try
            {
                return await _testingRepository.GetPrecheckCompletedComponentsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching precheck completed components from service.");
                throw;
            }
        }

        public async Task<string?> GetRawTemplateHtmlAsync(string drawingNumber)
        {
            var exportData = await _testingRepository.GetInspectionExportDataAsync(drawingNumber);
            return exportData?.HtmlTemplate;
        }

        public async Task<object> GetFieldNamesForExportAsync(string drawingNumber)
        {
            var fields = await _testingRepository.GetFieldNamesForExportAsync(drawingNumber);
            if (fields.Count == 0)
                return new { error = $"No template fields found for drawing number '{drawingNumber}'" };

            var exportData = await _testingRepository.GetInspectionExportDataAsync(drawingNumber);
            var templateHtml = exportData?.HtmlTemplate ?? string.Empty;

            var inputIds = Regex.Matches(templateHtml, @"<(?:input|textarea)\b[^>]*\bid=""([^""]+)""", RegexOptions.IgnoreCase)
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var savedValues = await _testingRepository.GetFieldValuesForExportAsync(drawingNumber);
            var savedLookup = savedValues
                .GroupBy(v => v.RowNumber == 0 ? v.FieldName : $"{v.FieldName}_{v.RowNumber}", StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().FieldValue, StringComparer.OrdinalIgnoreCase);

            var fixedFields = fields
                .Where(f => !f.IsRowField)
                .Select(f => new
                {
                    fieldName      = f.FieldName,
                    fieldLabel     = f.FieldLabel,
                    expectedHtmlId = f.FieldName,
                    foundInTemplate = inputIds.Contains(f.FieldName ?? string.Empty),
                    savedValue     = savedLookup.TryGetValue(f.FieldName ?? string.Empty, out var fv) ? fv : null
                })
                .ToList();

            var rowFields = fields
                .Where(f => f.IsRowField)
                .Select(f => new
                {
                    fieldName       = f.FieldName,
                    fieldLabel      = f.FieldLabel,
                    expectedHtmlId  = $"{f.FieldName}_{{i}}",
                    row1InTemplate  = inputIds.Contains($"{f.FieldName}_1"),
                    savedValueRow1  = savedLookup.TryGetValue($"{f.FieldName}_1", out var rv1) ? rv1 : null,
                    savedValueRow2  = savedLookup.TryGetValue($"{f.FieldName}_2", out var rv2) ? rv2 : null
                })
                .ToList();

            return new
            {
                drawingNumber,
                totalFields         = fields.Count,
                templateHasInputIds = inputIds.Count > 0,
                templateInputIds    = inputIds.OrderBy(x => x).ToList(),
                fixedFields,
                rowFields,
                instructions = new
                {
                    step1 = "Check 'foundInTemplate' / 'row1InTemplate' columns — false means the ID is missing from the template HTML",
                    step2 = "The 'expectedHtmlId' column shows what id= attribute the input must have",
                    step3 = "If 'savedValue' is null, no data has been saved for that field yet"
                }
            };
        }

        public async Task<object> GetExportDebugDataAsync(string drawingNumber)
        {
            var exportData = await _testingRepository.GetInspectionExportDataAsync(drawingNumber);
            if (exportData == null)
                return new { error = $"No inspection master found for drawing number '{drawingNumber}'" };

            var fieldValues = await _testingRepository.GetFieldValuesForExportAsync(drawingNumber);

            // Extract placeholder keys from the template
            var templateKeys = Regex.Matches(
                    exportData.HtmlTemplate ?? string.Empty,
                    @"\{\{(?:this\.)?([a-zA-Z0-9_]+)\}\}",
                    RegexOptions.IgnoreCase)
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(k => k)
                .ToList();

            var dbKeys = fieldValues
                .Where(x => !string.IsNullOrWhiteSpace(x.FieldName))
                .Select(x => new { x.FieldName, x.FieldValue, x.RowNumber })
                .ToList();

            var matched = templateKeys
                .Where(k => dbKeys.Any(d => string.Equals(d.FieldName?.Trim(), k, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var unmatched = templateKeys
                .Where(k => !dbKeys.Any(d => string.Equals(d.FieldName?.Trim(), k, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var templateHtml = exportData.HtmlTemplate ?? string.Empty;

            // Detect common placeholder formats in the template
            var curlyDouble = Regex.Matches(templateHtml, @"\{\{[^}]+\}\}").Count;
            var curlySingle = Regex.Matches(templateHtml, @"\{[a-zA-Z0-9_]+\}").Count;
            var squareBracket = Regex.Matches(templateHtml, @"\[[a-zA-Z0-9_]+\]").Count;
            var percent = Regex.Matches(templateHtml, @"%[a-zA-Z0-9_]+%").Count;
            var hashTag = Regex.Matches(templateHtml, @"#[a-zA-Z0-9_]+#").Count;
            var dataAttr = Regex.Matches(templateHtml, @"data-field=""([^""]+)""").Count;

            // Sample first 1000 chars of template to see its structure
            var templateSample = templateHtml.Length > 1000
                ? templateHtml.Substring(0, 1000)
                : templateHtml;

            // Extract all input/textarea name attributes to see exact naming pattern
            var inputNames = Regex.Matches(templateHtml, @"<(?:input|textarea)\b[^>]*\bname=""([^""]+)""", RegexOptions.IgnoreCase)
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ToList();

            // Extract all input id attributes
            var inputIds = Regex.Matches(templateHtml, @"<(?:input|textarea)\b[^>]*\bid=""([^""]+)""", RegexOptions.IgnoreCase)
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ToList();

            // Extract the first 5 full <input> tags to see their structure
            var sampleInputTags = Regex.Matches(templateHtml, @"<input\b[^>]*/?>", RegexOptions.IgnoreCase)
                .Cast<Match>()
                .Take(5)
                .Select(m => m.Value)
                .ToList();

            // Show a section of the template body (skip the CSS — take chars 2000-3500)
            var bodyStart = templateHtml.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
            bodyStart = bodyStart < 0 ? 1500 : bodyStart;
            var bodySample = templateHtml.Length > bodyStart + 1500
                ? templateHtml.Substring(bodyStart, 1500)
                : templateHtml.Substring(bodyStart);

            return new
            {
                drawingNumber,
                masterId = exportData.MasterId,
                templateId = exportData.TemplateId,
                templateLength = templateHtml.Length,
                templateSample,
                templateHasEachRows = templateHtml.Contains("{{#each rows}}", StringComparison.OrdinalIgnoreCase),
                detectedPlaceholderFormats = new
                {
                    doubleCurly_count = curlyDouble,
                    singleCurly_count = curlySingle,
                    squareBracket_count = squareBracket,
                    percent_count = percent,
                    hashTag_count = hashTag,
                    dataAttribute_count = dataAttr
                },
                totalFieldValuesFromDb = fieldValues.Count,
                fieldValuesFromDb = dbKeys,
                templatePlaceholderCount = templateKeys.Count,
                templatePlaceholders = templateKeys,
                matchedCount = matched.Count,
                matchedKeys = matched,
                unmatchedCount = unmatched.Count,
                unmatchedKeys = unmatched,
                allInputNamesInTemplate = inputNames,
                allInputIdsInTemplate = inputIds,
                sampleInputTags,
                bodySample
            };
        }

        private static bool IsCheckedValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var normalized = value.Trim();

            return normalized.Equals("true", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("yes", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("y", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("checked", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("1", StringComparison.OrdinalIgnoreCase);
        }
    }
}
