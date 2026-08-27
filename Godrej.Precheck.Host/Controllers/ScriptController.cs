using Godrej.Precheck.Models.DTOs.Scripts;
using Godrej.Precheck.Service.Service.ScriptService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Godrej.Precheck.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScriptController : ControllerBase
    {
        private readonly ILogger<ScriptController> _logger;
        private readonly string _uploadPath;
        private readonly string _stdQRScriptPath;
        private readonly string _qrImportScriptPath;
        private readonly string _masterDataScriptPath;

        public ScriptController(ILogger<ScriptController> logger, IConfiguration config)
        {
            _logger = logger;
            _uploadPath = config["UploadPath"]!;
            _stdQRScriptPath = config["ScriptPaths:STDQRGeneration"]!;
            _qrImportScriptPath = config["ScriptPaths:QRCodeImport"]!;
            _masterDataScriptPath = config["ScriptPaths:MasterData"]!;

            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);
        }

        [Authorize]
        [HttpPost("UploadExcel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("ScriptController:UploadExcel - No file received.");
                return BadRequest(new { message = "No file received." });
            }

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("ScriptController:UploadExcel - Invalid file type received.");
                return BadRequest(new { message = "Only .xlsx files are accepted." });
            }

            _logger.LogInformation("Request received for ScriptController:UploadExcel");
            try
            {
                // Unique name so multiple users don't overwrite each other
                var fileName = $"{Guid.NewGuid()}_uploaded.xlsx";
                var savePath = Path.Combine(_uploadPath, fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation($"ScriptController:UploadExcel - File saved at {savePath}");

                return Ok(new
                {
                    success = true,
                    message = "File uploaded successfully.",
                    fileName = fileName    // ← send this back to React, needed for Run API
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "ScriptController:UploadExcel - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ScriptController:UploadExcel - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("RunSTDQRGeneration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult RunSTDQRGeneration([FromBody] RunScriptRequestDto request)
        {
            if (request == null || request.FileName == null || request.FileName.Count == 0 || string.IsNullOrWhiteSpace(request.FileName[0]))
            {
                _logger.LogWarning("ScriptController:RunSTDQRGeneration - FileName is null or empty.");
                return BadRequest(new { message = "FileName cannot be empty." });
            }

            _logger.LogInformation($"Request received for ScriptController:RunSTDQRGeneration - {request.FileName[0]}");
            try
            {
                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                var excelPath = Path.Combine(_uploadPath, request.FileName[0]);
                return ExecuteScript(_stdQRScriptPath, excelPath, "RunSTDQRGeneration", null, userId.ToString());
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "ScriptController:RunSTDQRGeneration - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ScriptController:RunSTDQRGeneration - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("RunQRCodeImport")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult RunQRCodeImport([FromBody] RunScriptRequestDto request)
        {
            if (request == null || request.FileName == null || request.FileName.Count == 0 || string.IsNullOrWhiteSpace(request.FileName[0]))
            {
                _logger.LogWarning("ScriptController:RunQRCodeImport - FileName is null or empty.");
                return BadRequest(new { message = "FileName cannot be empty." });
            }

            _logger.LogInformation($"Request received for ScriptController:RunQRCodeImport - {request.FileName[0]}");
            try
            {
                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                var excelPath = Path.Combine(_uploadPath, request.FileName[0]);
                return ExecuteScript(_qrImportScriptPath, excelPath, "RunQRCodeImport", null, userId.ToString());
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "ScriptController:RunQRCodeImport - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ScriptController:RunQRCodeImport - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("UploadMasterDataExcel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UploadMasterDataExcel(IFormFile file1, IFormFile file2)
        {
            if (file1 == null || file1.Length == 0)
            {
                _logger.LogWarning("ScriptController:UploadMasterDataExcel - file1 is missing.");
                return BadRequest(new { message = "First file (file1) is required." });
            }
            if (file2 == null || file2.Length == 0)
            {
                _logger.LogWarning("ScriptController:UploadMasterDataExcel - file2 is missing.");
                return BadRequest(new { message = "Second file (file2) is required." });
            }
            if (!file1.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "file1: Only .xlsx files are accepted." });
            if (!file2.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "file2: Only .xlsx files are accepted." });

            _logger.LogInformation("Request received for ScriptController:UploadMasterDataExcel");
            try
            {
                var fileName1 = $"{Guid.NewGuid()}_masterdata1.xlsx";
                var fileName2 = $"{Guid.NewGuid()}_masterdata2.xlsx";

                var savePath1 = Path.Combine(_uploadPath, fileName1);
                var savePath2 = Path.Combine(_uploadPath, fileName2);

                using (var stream = new FileStream(savePath1, FileMode.Create))
                    await file1.CopyToAsync(stream);

                using (var stream = new FileStream(savePath2, FileMode.Create))
                    await file2.CopyToAsync(stream);

                _logger.LogInformation($"ScriptController:UploadMasterDataExcel - Files saved: {savePath1}, {savePath2}");

                return Ok(new
                {
                    success = true,
                    message = "Both files uploaded successfully.",
                    fileName1 = fileName1,  // ← pass to RunMasterData
                    fileName2 = fileName2   // ← pass to RunMasterData
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "ScriptController:UploadMasterDataExcel - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ScriptController:UploadMasterDataExcel - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize]
        [HttpPost("RunMasterData")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult RunMasterData([FromBody] RunMasterDataRequestDto request)
        {
            if (request == null || request.FileName == null || request.FileName.Count < 2)
            {
                _logger.LogWarning("ScriptController:RunMasterData - FileName array must contain at least 2 entries.");
                return BadRequest(new { message = "FileName array must contain at least 2 file names." });
            }

            if (request.FileName.Any(string.IsNullOrWhiteSpace))
            {
                _logger.LogWarning("ScriptController:RunMasterData - One or more entries in FileName are empty.");
                return BadRequest(new { message = "FileName array must not contain empty entries." });
            }

            _logger.LogInformation($"Request received for ScriptController:RunMasterData - {string.Join(", ", request.FileName)}");

            var scriptDir = Path.GetDirectoryName(_masterDataScriptPath)!;
            string? scriptDirCopy1 = null;
            string? scriptDirCopy2 = null;

            try
            {
                var userId = Convert.ToInt32(User.FindFirst("id")?.Value);
                var excelPath1 = Path.Combine(_uploadPath, request.FileName[0]);
                var excelPath2 = Path.Combine(_uploadPath, request.FileName[1]);

                // Copy uploaded files into the script's own folder so the script can find them
                scriptDirCopy1 = Path.Combine(scriptDir, request.FileName[0]);
                scriptDirCopy2 = Path.Combine(scriptDir, request.FileName[1]);
                System.IO.File.Copy(excelPath1, scriptDirCopy1, overwrite: true);
                System.IO.File.Copy(excelPath2, scriptDirCopy2, overwrite: true);
                _logger.LogInformation($"[RunMasterData] Copied files to script folder: {scriptDirCopy1}, {scriptDirCopy2}");

                var result = ExecuteScript(_masterDataScriptPath, scriptDirCopy1, "RunMasterData", scriptDirCopy2, userId.ToString());

                // Clean up original uploads after script completes
                TryDeleteFile(excelPath1, "RunMasterData", "uploaded file1");
                TryDeleteFile(excelPath2, "RunMasterData", "uploaded file2");

                return result;
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "ScriptController:RunMasterData - Application error occurred");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ScriptController:RunMasterData - Unexpected error occurred");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
            finally
            {
                // Ensure script-folder copies are always cleaned up
                if (scriptDirCopy1 != null) TryDeleteFile(scriptDirCopy1, "RunMasterData", "script folder copy1");
                if (scriptDirCopy2 != null) TryDeleteFile(scriptDirCopy2, "RunMasterData", "script folder copy2");
            }
        }


        [Authorize] 
        [HttpGet("DownloadTemplate/{scriptType}")]
        public ActionResult DownloadTemplate(string scriptType)
        {
            _logger.LogInformation($"Request received for ScriptController:DownloadTemplate - {scriptType}");

            var templatePath = scriptType.ToLower() switch
            {
                "stdqrgeneration" => Path.Combine(Path.GetDirectoryName(_stdQRScriptPath)!, "STDqrcodesample.xlsx"),
                "qrcodeimport" => Path.Combine(Path.GetDirectoryName(_qrImportScriptPath)!, "qrcodesample.xlsx"),
                "masterdata1" => Path.Combine(Path.GetDirectoryName(_masterDataScriptPath)!, "masterdata-drawing-assembly.xlsx"),
                "masterdata2" => Path.Combine(Path.GetDirectoryName(_masterDataScriptPath)!, "masterdata-drawing.xlsx"),
                _ => null
            };

            if (templatePath == null)
                return BadRequest(new { message = "Invalid script type." });

            if (!System.IO.File.Exists(templatePath))
            {
                _logger.LogWarning($"ScriptController:DownloadTemplate - Template not found at {templatePath}");
                return NotFound(new { message = "Template file not found on server." });
            }

            byte[] fileBytes;
            using (var fs = new FileStream(templatePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var ms = new MemoryStream())
            {
                fs.CopyTo(ms);
                fileBytes = ms.ToArray();
            }
            var fileName = Path.GetFileName(templatePath);

            _logger.LogInformation($"ScriptController:DownloadTemplate - Sending {fileName}");

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

       
        #region

        // Single-file variant
        private ActionResult ExecuteScript(string scriptPath, string excelPath, string apiName)
            => ExecuteScript(scriptPath, excelPath, apiName, null);

        // Two-file variant, optionally followed by an extra trailing argument (e.g. logged-in user id)
        private ActionResult ExecuteScript(string scriptPath, string excelPath, string apiName, string? excelPath2 = null, string? extraArg = null)
        {
            if (!System.IO.File.Exists(scriptPath))
                return NotFound(new { message = "Script not found on server." });

            if (!System.IO.File.Exists(excelPath))
                return BadRequest(new { message = "Uploaded Excel (file1) not found." });

            if (excelPath2 != null && !System.IO.File.Exists(excelPath2))
                return BadRequest(new { message = "Uploaded Excel (file2) not found." });

            // Build arguments with absolute paths
            var argParts = new List<string> { $"\"{excelPath}\"" };
            if (excelPath2 != null)
                argParts.Add($"\"{excelPath2}\"");
            if (!string.IsNullOrEmpty(extraArg))
                argParts.Add(extraArg);
            var arguments = string.Join(" ", argParts);

            _logger.LogInformation($"[{apiName}] Starting process: {scriptPath}");
            _logger.LogInformation($"[{apiName}] Arguments: {arguments}");

            var psi = new ProcessStartInfo
            {
                FileName = scriptPath,
                Arguments = arguments,
                WorkingDirectory = Path.GetDirectoryName(scriptPath)!, // ← original script folder
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };

            psi.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

            using var process = new Process { StartInfo = psi };

            var stdoutBuilder = new System.Text.StringBuilder();
            var stderrBuilder = new System.Text.StringBuilder();

            process.OutputDataReceived += (s, e) => { if (e.Data != null) stdoutBuilder.AppendLine(e.Data); };
            process.ErrorDataReceived += (s, e) => { if (e.Data != null) stderrBuilder.AppendLine(e.Data); };

            process.Start();
            _logger.LogInformation($"[{apiName}] Process started PID: {process.Id}");

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
            _logger.LogInformation($"[{apiName}] Process exited: {process.ExitCode}");

            string stdout = stdoutBuilder.ToString();
            string stderr = stderrBuilder.ToString();

            _logger.LogInformation($"[{apiName}] STDOUT: {stdout}");
            _logger.LogInformation($"[{apiName}] STDERR: {stderr}");

            try
            {
                // Cleanup uploaded files after script finishes
                TryDeleteFile(excelPath, apiName, "uploaded file1");
                if (excelPath2 != null)
                    TryDeleteFile(excelPath2, apiName, "uploaded file2");
            }
            catch { }

            // Merge stderr into output so all script messages are visible on the frontend
            var combinedOutput = string.IsNullOrWhiteSpace(stderr)
                ? stdout
                : stdout + "\n--- STDERR ---\n" + stderr;

            // Always return 200 so the frontend receives the full output body.
            // Use success/exitCode flags to let the UI distinguish full success vs partial errors.
            if (process.ExitCode == 0)
                return Ok(new { success = true, message = "Script executed successfully.", output = combinedOutput, exitCode = 0 });
            else
                return Ok(new { success = false, message = "Script execution completed with errors.", output = combinedOutput, exitCode = process.ExitCode });
        }

        private void TryDeleteDirectory(string dirPath, string apiName)
        {
            try
            {
                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, recursive: true);
                    _logger.LogInformation($"[{apiName}] Deleted working directory: {dirPath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"[{apiName}] Could not delete working directory: {dirPath}");
            }
        }

        private void TryDeleteFile(string filePath, string apiName, string label)
        {
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    _logger.LogInformation($"[{apiName}] Deleted {label}: {filePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"[{apiName}] Could not delete {label}: {filePath}");
            }
        }

        #endregion
    }
}
