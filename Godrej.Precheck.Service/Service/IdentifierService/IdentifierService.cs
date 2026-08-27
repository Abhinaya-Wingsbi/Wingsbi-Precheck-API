using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DTOs.Identifier;
using Godrej.Precheck.Models.DTOs.IRNumber;
using Godrej.Precheck.Models.DTOs.MSNNumber;
using Godrej.Precheck.Repository.Repository.CommonRepository;
using Godrej.Precheck.Repository.Repository.IdentifierRepository;
using Mapster;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Godrej.Precheck.Service.Service.IdentifierService
{
    public class IdentifierService : IIdentifierService
    {
        private readonly ILogger<IdentifierService> _logger;
        private readonly IIdentifierRepository _repository;
        private readonly ICommonRepository _commonRepository;

        public IdentifierService(ILogger<IdentifierService> logger, IIdentifierRepository repository, ICommonRepository commonRepository)
        {
            _logger = logger;
            _repository = repository;
            _commonRepository = commonRepository;
        }

        public async Task<IRNumbers> InsertIRNumberAsync(IRNumberDto irNumberDto)
        {
            _logger.LogInformation("Starting InsertIRNumberAsync with data: {@IRNumberDto}", irNumberDto);
            try
            {
                var irNumber = irNumberDto.Adapt<IRNumbers>();
                _logger.LogDebug("Mapped IRNumberDto to IRNumbers model");

                var prodSeriesResponse = await _commonRepository.GetProductionSeriesById(irNumberDto.ProdSeriesId);
                _logger.LogDebug("Retrieved production series with ID {ProdSeriesId}: {ProductionSeries}",
                    irNumberDto.ProdSeriesId, prodSeriesResponse.ProductionSeries);

                var prodSeries = prodSeriesResponse.ProductionSeries;
                int userid = irNumberDto.CreatedBy.Value;

                int lastSequenceNo = await _commonRepository.GetLastSequenceNoIrNumberTable();
                _logger.LogDebug("Retrieved last sequence number: {LastSequenceNo}", lastSequenceNo);

                int finalSequenceNo = lastSequenceNo + 1;
                var generatedIRNumber = $"IR/{irNumberDto.DepartmentName}/{DateTime.Now.Year}/{prodSeries}-{finalSequenceNo}";
                _logger.LogDebug("Generated IR number: {GeneratedIRNumber}", generatedIRNumber);

                var validateIR = await _commonRepository.GetSingleIRnumber(generatedIRNumber);
                _logger.LogDebug("Validation check for existing IR number returned: {ValidateIR}", validateIR != null ? "Exists" : "Does not exist");

                irNumber.SequenceNo = finalSequenceNo;

                // Enhanced validation: Check for duplicate ID numbers
                // Check for duplicate ID number if provided
                if (!string.IsNullOrEmpty(irNumberDto.IdNumberRange))
                {
                    var result = await _repository.ExistsIrIdNumberAsync(irNumberDto.ProdSeriesId, irNumberDto.DrawingNumberId ?? 0, irNumberDto.IdNumberRange, irNumberDto.DepartmentId ?? 0, irNumberDto.OperationNumber ?? string.Empty, irNumberDto.StageId ?? 0);
                    if (result != null)
                    {
                        _logger.LogWarning("Duplicate IR ID number for ProdSeriesId {ProdSeriesId}, DrawingNumberId {DrawingNumberId}, IdNumberRange {IdNumberRange}, Dept {Dept}, Op {Op}, Stage {Stage}", irNumberDto.ProdSeriesId, irNumberDto.DrawingNumberId, irNumberDto.IdNumberRange, irNumberDto.DepartmentId, irNumberDto.OperationNumber, irNumberDto.StageId);
                        throw new ValidationException($"{result} IR number already exists for this combination of Production Series, Drawing Number, ID Number, Department, Operation, and Stage.");
                    }
                }

                // Check for overlapping ID number ranges
                int idStart = irNumberDto.IdNumberStart ?? 0;
                int idEnd = irNumberDto.IdNumberEnd ?? 0;
                if (idStart > 0 && idEnd > 0)
                {
                    if (await _repository.ExistsIrIdConflictAsync(irNumberDto.ProdSeriesId, irNumberDto.DrawingNumberId ?? 0, idStart, idEnd, irNumberDto.DepartmentId ?? 0, irNumberDto.OperationNumber ?? string.Empty, irNumberDto.StageId ?? 0))
                    {
                        _logger.LogWarning("Overlapping IR ID range for ProdSeriesId {ProdSeriesId}, DrawingNumberId {DrawingNumberId}, Range {IdStart}-{IdEnd}, Dept {Dept}, Op {Op}, Stage {Stage}", irNumberDto.ProdSeriesId, irNumberDto.DrawingNumberId, idStart, idEnd, irNumberDto.DepartmentId, irNumberDto.OperationNumber, irNumberDto.StageId);
                        throw new ValidationException("Overlapping ID range for this specific combination of Production Series, Drawing Number, Department, Operation, and Stage.");
                    }
                }
                if (validateIR != null)
                {
                    _logger.LogWarning("Attempted to create duplicate IR number: {GeneratedIRNumber}", generatedIRNumber);
                    throw new ValidationException($"{generatedIRNumber} IRNumber is already generated.");
                }
                irNumber.IrNumber = generatedIRNumber;

                // Populate stage name from stageid if stageid is provided
                if (irNumber.StageId.HasValue && irNumber.StageId.Value > 0)
                {
                    var stage = await _commonRepository.GetStageById(irNumber.StageId.Value);
                    if (stage != null)
                    {
                        irNumber.Stage = stage.StageName;
                    }
                }

                _logger.LogDebug("Attempting to insert IR number record to database");
                var insertedIRNumber = await _repository.InsertIRNumberAsync(irNumber);

                _logger.LogInformation("Successfully inserted IRNumber details: {@IRNumber}", insertedIRNumber);
                return insertedIRNumber;
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "Validation error occurred while inserting IRNumber: {ErrorMessage}", vex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting IRNumber details for department {Department} and production series {ProdSeries}",
                    irNumberDto.DepartmentName, irNumberDto.ProdSeriesId);
                throw;
            }
        }

        public async Task<MSNNumbers> InsertMSNNumberAsync(MSNNumberDto msnNumberDto)
        {
            _logger.LogInformation("Starting InsertMSNNumberAsync with data: {@MSNNumberDto}", msnNumberDto);
            try
            {
                var msnNumber = msnNumberDto.Adapt<MSNNumbers>();
                _logger.LogDebug("Mapped MSNNumberDto to MSNNumbers model");

                var prodSeriesResponse = await _commonRepository.GetProductionSeriesById(msnNumberDto.ProdSeriesId);
                _logger.LogDebug("Retrieved production series with ID {ProdSeriesId}: {ProductionSeries}",
                    msnNumberDto.ProdSeriesId, prodSeriesResponse.ProductionSeries);

                var prodSeries = prodSeriesResponse.ProductionSeries;

                int lastSequenceNo = await _commonRepository.GetLastSequenceNoMSNNumberTable();
                _logger.LogDebug("Retrieved last sequence number: {LastSequenceNo}", lastSequenceNo);

                int finalSequenceNo = lastSequenceNo + 1;
                var generatedMSNNumber = $"MSN/{msnNumberDto.DepartmentName}/{DateTime.Now.Year}/{prodSeries}-{finalSequenceNo}";
                _logger.LogDebug("Generated MSN number: {GeneratedMSNNumber}", generatedMSNNumber);

                var validateMSN = await _commonRepository.GetSingleMSNNuber(generatedMSNNumber);
                _logger.LogDebug("Validation check for existing MSN number returned: {ValidateMSN}", validateMSN != null ? "Exists" : "Does not exist");

                msnNumber.SequenceNo = finalSequenceNo;

                // Enhanced validation: Check for duplicate MSN ID numbers
                // Check for duplicate ID number if provided
                if (!string.IsNullOrEmpty(msnNumberDto.IdNumberRange))
                {
                    if (await _repository.ExistsMsnIdNumberAsync(msnNumberDto.ProdSeriesId, msnNumberDto.DrawingNumberId ?? 0, msnNumberDto.IdNumberRange, msnNumberDto.DepartmentId ?? 0, msnNumberDto.OperationNumber ?? string.Empty, msnNumberDto.StageId ?? 0))
                    {
                        _logger.LogWarning("Duplicate MSN ID number for ProdSeriesId {ProdSeriesId}, DrawingNumberId {DrawingNumberId}, IdNumberRange {IdNumberRange}, Dept {Dept}, Op {Op}, Stage {Stage}", msnNumberDto.ProdSeriesId, msnNumberDto.DrawingNumberId, msnNumberDto.IdNumberRange, msnNumberDto.DepartmentId, msnNumberDto.OperationNumber, msnNumberDto.StageId);
                        throw new ValidationException("MSN number already exists for this combination of Production Series, Drawing Number, ID Number, Department, Operation, and Stage.");
                    }
                }

                // Check for overlapping ID number ranges
                int msnStart = msnNumberDto.IdNumberStart ?? 0;
                int msnEnd = msnNumberDto.IdNumberEnd ?? 0;
                if (msnStart > 0 && msnEnd > 0)
                {
                    if (await _repository.ExistsMsnIdConflictAsync(msnNumberDto.ProdSeriesId, msnNumberDto.DrawingNumberId ?? 0, msnStart, msnEnd, msnNumberDto.DepartmentId ?? 0, msnNumberDto.OperationNumber ?? string.Empty, msnNumberDto.StageId ?? 0))
                    {
                        _logger.LogWarning("Overlapping MSN ID range for ProdSeriesId {ProdSeriesId}, DrawingNumberId {DrawingNumberId}, Range {IdStart}-{IdEnd}, Dept {Dept}, Op {Op}, Stage {Stage}", msnNumberDto.ProdSeriesId, msnNumberDto.DrawingNumberId, msnStart, msnEnd, msnNumberDto.DepartmentId, msnNumberDto.OperationNumber, msnNumberDto.StageId);
                        throw new ValidationException("Overlapping ID range for this specific combination of Production Series, Drawing Number, Department, Operation, and Stage.");
                    }
                }

                if (validateMSN != null)
                {
                    _logger.LogWarning("Attempted to create duplicate MSN number: {GeneratedMSNNumber}", generatedMSNNumber);
                    throw new ValidationException("MSNNumber is already generated.");
                }

                msnNumber.MsnNumber = generatedMSNNumber;

                // Populate stage name from stageid if stageid is provided
                if (msnNumber.StageId.HasValue && msnNumber.StageId.Value > 0)
                {
                    var stage = await _commonRepository.GetStageById(msnNumber.StageId.Value);
                    if (stage != null)
                    {
                        msnNumber.Stage = stage.StageName;
                    }
                }

                _logger.LogDebug("Attempting to insert MSN number record to database");
                var insertedMSNNumber = await _repository.InsertMSNNumberAsync(msnNumber);

                _logger.LogInformation("Successfully inserted MSNNumber details: {@MSNNumber}", insertedMSNNumber);
                return insertedMSNNumber;
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "Validation error occurred while inserting MSNNumber: {ErrorMessage}", vex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting MSNNumber details for department {Department} and production series {ProdSeries}",
                    msnNumberDto.DepartmentName, msnNumberDto.ProdSeriesId);
                throw;
            }
        }

        public async Task<IRNumbers> UpadateIRNumberAsync(UpdateIRDto updateIR)
        {
            _logger.LogInformation("Starting UpdateIRNumberAsync with data: {@UpdateIRDto}", updateIR);
            try
            {
                // Fetch the existing record to get ProdSeriesId and DrawingNumberId and current ID
                var existingRecord = await _commonRepository.GetSingleIRnumber(updateIR.IrNumber);
                if (existingRecord == null)
                {
                     throw new Exception($"IR Number {updateIR.IrNumber} not found.");
                }

                // Parse range if provided
                if (!string.IsNullOrEmpty(updateIR.IdNumberRange) && (!updateIR.IdNumberStart.HasValue || !updateIR.IdNumberEnd.HasValue))
                {
                   var parts = updateIR.IdNumberRange.Split('-');
                   if (parts.Length == 2 && int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
                   {
                        updateIR.IdNumberStart = start;
                        updateIR.IdNumberEnd = end;
                   }
                   else if (parts.Length == 1 && int.TryParse(parts[0], out int single))
                   {
                        updateIR.IdNumberStart = single;
                        updateIR.IdNumberEnd = single;
                   }
                }

                // Validation logic (only if ID range is being updated)
                if (!string.IsNullOrEmpty(updateIR.IdNumberRange))
                {
                     // Check for duplicate ID number
                     if (await _repository.ExistsIrIdNumberUpdateAsync(
                         existingRecord.ProdSeriesId ?? 0,
                         existingRecord.DrawingNumberId ?? 0,
                         updateIR.IdNumberRange,
                         existingRecord.Id ?? 0,
                         existingRecord.DepartmentId ?? 0,
                         updateIR.OperationNumber ?? existingRecord.OperationNumber ?? string.Empty,
                         updateIR.StageId ?? existingRecord.StageId ?? 0))
                     {
                        _logger.LogWarning("Duplicate IR ID number (Update) for ProdSeriesId {ProdSeriesId}, DrawingNumberId {DrawingNumberId}, IdNumberRange {IdNumberRange}, ExcludeId {Id}", existingRecord.ProdSeriesId, existingRecord.DrawingNumberId, updateIR.IdNumberRange, existingRecord.Id);
                        throw new ValidationException("IR number already exists for this combination of Production Series, Drawing Number, Department, Operation, and Stage.");
                     }

                     // Check for overlaps
                     int updIdStart = updateIR.IdNumberStart ?? 0;
                     int updIdEnd = updateIR.IdNumberEnd ?? 0;
                     if (updIdStart > 0 && updIdEnd > 0)
                     {
                        if (await _repository.ExistsIrIdConflictUpdateAsync(
                            existingRecord.ProdSeriesId ?? 0,
                            existingRecord.DrawingNumberId ?? 0,
                            updIdStart,
                            updIdEnd,
                            existingRecord.Id ?? 0,
                            existingRecord.DepartmentId ?? 0,
                            updateIR.OperationNumber ?? existingRecord.OperationNumber ?? string.Empty,
                            updateIR.StageId ?? existingRecord.StageId ?? 0))
                        {
                            _logger.LogWarning("Overlapping IR ID range (Update) for ProdSeriesId {ProdSeriesId}, DrawingNumberId {DrawingNumberId}, Range {IdStart}-{IdEnd}, ExcludeId {Id}", existingRecord.ProdSeriesId, existingRecord.DrawingNumberId, existingRecord.Id);
                            throw new ValidationException("Overlapping ID range for this specific combination of Production Series, Drawing Number, Department, Operation, and Stage.");
                        }
                     }
                }

                var updateNumber = updateIR.Adapt<IRNumbers>();
                _logger.LogDebug("Mapped UpdateIRDto to IRNumbers model");

                // Populate stage name from stageid if stageid is provided
                if (updateNumber.StageId.HasValue && updateNumber.StageId.Value > 0)
                {
                    var stage = await _commonRepository.GetStageById(updateNumber.StageId.Value);
                    if (stage != null)
                    {
                        updateNumber.Stage = stage.StageName;
                    }
                }

                _logger.LogDebug("Attempting to update IR number in database with ID: {IRNumberId}", updateNumber.Id);
                var updateIRNumber = await _repository.UpdateIRNumberAsync(updateNumber);

                _logger.LogInformation("Successfully updated IRNumber details: {@IRNumber}", updateIRNumber);
                return updateIRNumber;
            }
            catch (ValidationException vex)
            {
                 _logger.LogWarning(vex, "Validation error occurred while updating IRNumber: {ErrorMessage}", vex.Message);
                 throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating IRNumber details for ID: {IRNumber}", updateIR.IrNumber);
                throw;
            }
        }

        public async Task<MSNNumbers> UpadateMSNNumber(UpdateMSNDto updateMSN)
        {
            _logger.LogInformation("Starting UpdateMSNNumber with data: {@UpdateMSNDto}", updateMSN);
            try
            {
                // Fetch the existing record to get ProdSeriesId and DrawingNumberId and current ID
                var existingRecord = await _commonRepository.GetSingleMSNNuber(updateMSN.MsnNumber);
                if (existingRecord == null)
                {
                     throw new Exception($"MSN Number {updateMSN.MsnNumber} not found.");
                }

                // Parse range if provided
                if (!string.IsNullOrEmpty(updateMSN.IdNumberRange) && (!updateMSN.IdNumberStart.HasValue || !updateMSN.IdNumberEnd.HasValue))
                {
                   var parts = updateMSN.IdNumberRange.Split('-');
                   if (parts.Length == 2 && int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
                   {
                        updateMSN.IdNumberStart = start;
                        updateMSN.IdNumberEnd = end;
                   }
                   else if (parts.Length == 1 && int.TryParse(parts[0], out int single))
                   {
                        updateMSN.IdNumberStart = single;
                        updateMSN.IdNumberEnd = single;
                   }
                }

                // Validation logic (only if ID range is being updated)
                 if (!string.IsNullOrEmpty(updateMSN.IdNumberRange))
                 {
                      // Check for duplicate ID number
                      if (await _repository.ExistsMsnIdNumberUpdateAsync(
                          existingRecord.ProdSeriesId ?? 0,
                          existingRecord.DrawingNumberId ?? 0,
                          updateMSN.IdNumberRange,
                          existingRecord.Id ?? 0,
                          existingRecord.DepartmentId ?? 0,
                          updateMSN.OperationNumber ?? existingRecord.OperationNumber ?? string.Empty,
                          updateMSN.StageId ?? existingRecord.StageId ?? 0))
                      {
                         _logger.LogWarning("Duplicate MSN ID number (Update) for ProdSeriesId {ProdSeriesId}, DrawingNumberId {DrawingNumberId}, IdNumberRange {IdNumberRange}, ExcludeId {Id}", existingRecord.ProdSeriesId, existingRecord.DrawingNumberId, updateMSN.IdNumberRange, existingRecord.Id);
                         throw new ValidationException("MSN number already exists for this combination of Production Series, Drawing Number, Department, Operation, and Stage.");
                      }

                      // Check for overlaps
                      int msnUpdStart = updateMSN.IdNumberStart ?? 0;
                      int msnUpdEnd = updateMSN.IdNumberEnd ?? 0;
                      if (msnUpdStart > 0 && msnUpdEnd > 0)
                      {
                         if (await _repository.ExistsMsnIdConflictUpdateAsync(
                             existingRecord.ProdSeriesId ?? 0,
                             existingRecord.DrawingNumberId ?? 0,
                             msnUpdStart,
                             msnUpdEnd,
                             existingRecord.Id ?? 0,
                             existingRecord.DepartmentId ?? 0,
                             updateMSN.OperationNumber ?? existingRecord.OperationNumber ?? string.Empty,
                             updateMSN.StageId ?? existingRecord.StageId ?? 0))
                         {
                             _logger.LogWarning("Overlapping MSN ID range (Update) for ProdSeriesId {ProdSeriesId}, DrawingNumberId {DrawingNumberId}, Range {msnUpdStart}-{msnUpdEnd}, ExcludeId {Id}", existingRecord.ProdSeriesId, existingRecord.DrawingNumberId, msnUpdStart, msnUpdEnd, existingRecord.Id);
                             throw new ValidationException("Overlapping ID range for this specific combination of Production Series, Drawing Number, Department, Operation, and Stage.");
                         }
                      }
                 }

                var updateNumber = updateMSN.Adapt<MSNNumbers>();
                _logger.LogDebug("Mapped UpdateMSNDto to MSNNumbers model");

                // Populate stage name from stageid if stageid is provided
                if (updateNumber.StageId.HasValue && updateNumber.StageId.Value > 0)
                {
                    var stage = await _commonRepository.GetStageById(updateNumber.StageId.Value);
                    if (stage != null)
                    {
                        updateNumber.Stage = stage.StageName;
                    }
                }

                _logger.LogDebug("Attempting to update MSN number in database with ID: {MSNNumberId}", updateNumber.Id);
                var updateMSNNumber = await _repository.UpdateMSNNumberAsync(updateNumber);

                _logger.LogInformation("Successfully updated MSNNumber details: {@MSNNumber}", updateMSNNumber);
                return updateMSNNumber;
            }
            catch (ValidationException vex)
            {
                 _logger.LogWarning(vex, "Validation error occurred while updating MSNNumber: {ErrorMessage}", vex.Message);
                 throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating MSNNumber details for ID: {MSNNumber}", updateMSN.MsnNumber);
                throw;
            }
        }

        public async Task<IRNumbers> InsertStandardIRNumberAsync(StandardIRNumberDto standardIRNumberDto)
        {
            _logger.LogInformation("Starting InsertStandardIRNumberAsync with data: {@StandardIRNumberDto}", standardIRNumberDto);
            try
            {
                // Helper to parse range
                if (!string.IsNullOrEmpty(standardIRNumberDto.IdNumberRange) && (!standardIRNumberDto.IdNumberStart.HasValue || !standardIRNumberDto.IdNumberEnd.HasValue))
                {
                   var parts = standardIRNumberDto.IdNumberRange.Split('-');
                   if (parts.Length == 2 && int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
                   {
                        standardIRNumberDto.IdNumberStart = start;
                        standardIRNumberDto.IdNumberEnd = end;
                   }
                   else if (parts.Length == 1 && int.TryParse(parts[0], out int single))
                   {
                        standardIRNumberDto.IdNumberStart = single;
                        standardIRNumberDto.IdNumberEnd = single;
                   }
                }
                
                var irNumber = standardIRNumberDto.Adapt<IRNumbers>();
                _logger.LogDebug("Mapped StandardIRNumberDto to IRNumbers model with ID Range: {Start}-{End}", standardIRNumberDto.IdNumberStart, standardIRNumberDto.IdNumberEnd);

                var prodSeriesResponse = await _commonRepository.GetProductionSeriesById(standardIRNumberDto.ProdSeriesId);
                _logger.LogDebug("Retrieved production series with ID {ProdSeriesId}: {ProductionSeries}",
                    standardIRNumberDto.ProdSeriesId, prodSeriesResponse.ProductionSeries);

                var prodSeries = prodSeriesResponse.ProductionSeries;
                int userid = standardIRNumberDto.CreatedBy.Value;

                int lastSequenceNo = await _commonRepository.GetLastSequenceNoIrNumberTable();
                _logger.LogDebug("Retrieved last sequence number: {LastSequenceNo}", lastSequenceNo);

                int finalSequenceNo = lastSequenceNo + 1;
                var generatedIRNumber = $"IR/{standardIRNumberDto.DepartmentName}/{DateTime.Now.Year}/{prodSeries}-{finalSequenceNo}";
                _logger.LogDebug("Generated IR number: {GeneratedIRNumber}", generatedIRNumber);

                var validateIR = await _commonRepository.GetSingleIRnumber(generatedIRNumber);
                _logger.LogDebug("Validation check for existing IR number returned: {ValidateIR}", validateIR != null ? "Exists" : "Does not exist");

                irNumber.SequenceNo = finalSequenceNo;

                // Enhanced validation: Check for duplicate ID numbers
                // Check for duplicate ID number if provided
                if (!string.IsNullOrEmpty(standardIRNumberDto.IdNumberRange))
                {
                    var existingIrNumber = await _repository.ExistsIrIdNumberAsync(standardIRNumberDto.ProdSeriesId, standardIRNumberDto.DrawingNumberId ?? 0, standardIRNumberDto.IdNumberRange, standardIRNumberDto.DepartmentId ?? 0, standardIRNumberDto.OperationNumber ?? string.Empty, standardIRNumberDto.StageId ?? 0);
                    if (existingIrNumber!=null)
                    {
                        _logger.LogWarning("Duplicate Standard IR ID number for ProdSeriesId {ProdSeriesId}, DrawingNumberId {DrawingNumberId}, IdNumberRange {IdNumberRange}, Dept {Dept}, Op {Op}, Stage {Stage}", standardIRNumberDto.ProdSeriesId, standardIRNumberDto.DrawingNumberId, standardIRNumberDto.IdNumberRange, standardIRNumberDto.DepartmentId, standardIRNumberDto.OperationNumber, standardIRNumberDto.StageId);
                        throw new ValidationException("IR number already exists for this combination of Production Series, Drawing Number, ID Number.");
                    }
                }

                // Check for overlapping ID number ranges
                int standIdStart = standardIRNumberDto.IdNumberStart ?? 0;
                int standIdEnd = standardIRNumberDto.IdNumberEnd ?? 0;
                if (standIdStart > 0 && standIdEnd > 0)
                {
                    if (await _repository.ExistsIrIdConflictAsync(standardIRNumberDto.ProdSeriesId, standardIRNumberDto.DrawingNumberId ?? 0, standIdStart, standIdEnd, standardIRNumberDto.DepartmentId ?? 0, standardIRNumberDto.OperationNumber ?? string.Empty, standardIRNumberDto.StageId ?? 0))
                    {
                        _logger.LogWarning("Overlapping Standard IR ID range for ProdSeriesId {ProdSeriesId}, DrawingNumberId {DrawingNumberId}, Range {IdStart}-{IdEnd}, Dept {Dept}, Op {Op}, Stage {Stage}", standardIRNumberDto.ProdSeriesId, standardIRNumberDto.DrawingNumberId, standIdStart, standIdEnd, standardIRNumberDto.DepartmentId, standardIRNumberDto.OperationNumber, standardIRNumberDto.StageId);
                        throw new ValidationException("Overlapping ID range for this specific combination of Production Series, Drawing Number, Department, Operation, and Stage.");
                    }
                }

                if (validateIR != null)
                {
                    _logger.LogWarning("Attempted to create duplicate IR number: {GeneratedIRNumber}", generatedIRNumber);
                    throw new ValidationException("IRNumber is already generated.");
                }

                irNumber.IrNumber = generatedIRNumber;

                // Populate stage name from stageid if stageid is provided
                if (irNumber.StageId.HasValue && irNumber.StageId.Value > 0)
                {
                    var stage = await _commonRepository.GetStageById(irNumber.StageId.Value);
                    if (stage != null)
                    {
                        irNumber.Stage = stage.StageName;
                    }
                }

                _logger.LogDebug("Attempting to insert Standard IR number record to database");
                var insertedIRNumber = await _repository.InsertStandardIRNumberAsync(irNumber);

                _logger.LogInformation("Successfully inserted Standard IRNumber details: {@IRNumber}", insertedIRNumber);
                return insertedIRNumber;
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "Validation error occurred while inserting Standard IRNumber: {ErrorMessage}", vex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting Standard IRNumber details for department {Department} and production series {ProdSeries}",
                    standardIRNumberDto.DepartmentName, standardIRNumberDto.ProdSeriesId);
                throw;
            }
        }

        public async Task<MSNNumbers> InsertStandardMSNNumberAsync(StandardMSNNumberDto standardMSNNumberDto)
        {
            _logger.LogInformation("Starting InsertStandardMSNNumberAsync with data: {@StandardMSNNumberDto}", standardMSNNumberDto);
            try
            {
                // Helper to parse range
                if (!string.IsNullOrEmpty(standardMSNNumberDto.IdNumberRange) && (!standardMSNNumberDto.IdNumberStart.HasValue || !standardMSNNumberDto.IdNumberEnd.HasValue))
                {
                   var parts = standardMSNNumberDto.IdNumberRange.Split('-');
                   if (parts.Length == 2 && int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
                   {
                        standardMSNNumberDto.IdNumberStart = start;
                        standardMSNNumberDto.IdNumberEnd = end;
                   }
                   else if (parts.Length == 1 && int.TryParse(parts[0], out int single))
                   {
                        standardMSNNumberDto.IdNumberStart = single;
                        standardMSNNumberDto.IdNumberEnd = single;
                   }
                }

                var msnNumber = standardMSNNumberDto.Adapt<MSNNumbers>();
                _logger.LogDebug("Mapped StandardMSNNumberDto to MSNNumbers model with ID Range: {Start}-{End}", standardMSNNumberDto.IdNumberStart, standardMSNNumberDto.IdNumberEnd);

                var prodSeriesResponse = await _commonRepository.GetProductionSeriesById(standardMSNNumberDto.ProdSeriesId);
                _logger.LogDebug("Retrieved production series with ID {ProdSeriesId}: {ProductionSeries}",
                    standardMSNNumberDto.ProdSeriesId, prodSeriesResponse.ProductionSeries);

                var prodSeries = prodSeriesResponse.ProductionSeries;

                int lastSequenceNo = await _commonRepository.GetLastSequenceNoMSNNumberTable();
                _logger.LogDebug("Retrieved last sequence number: {LastSequenceNo}", lastSequenceNo);

                int finalSequenceNo = lastSequenceNo + 1;
                var generatedMSNNumber = $"MSN/{standardMSNNumberDto.DepartmentName}/{DateTime.Now.Year}/{prodSeries}-{finalSequenceNo}";
                _logger.LogDebug("Generated MSN number: {GeneratedMSNNumber}", generatedMSNNumber);

                var validateMSN = await _commonRepository.GetSingleMSNNuber(generatedMSNNumber);
                _logger.LogDebug("Validation check for existing MSN number returned: {ValidateMSN}", validateMSN != null ? "Exists" : "Does not exist");

                msnNumber.SequenceNo = finalSequenceNo;

                // Enhanced validation: Check for duplicate MSN ID numbers
                // Check for duplicate ID number if provided
                if (!string.IsNullOrEmpty(standardMSNNumberDto.IdNumberRange))
                {
                    if (await _repository.ExistsMsnIdNumberAsync(standardMSNNumberDto.ProdSeriesId, standardMSNNumberDto.DrawingNumberId ?? 0, standardMSNNumberDto.IdNumberRange, standardMSNNumberDto.DepartmentId ?? 0, standardMSNNumberDto.OperationNumber ?? string.Empty, standardMSNNumberDto.StageId ?? 0))
                    {
                        _logger.LogWarning("Duplicate Standard MSN ID number for ProdSeriesId {ProdSeriesId}, DrawingNumberId {DrawingNumberId}, IdNumberRange {IdNumberRange}, Dept {Dept}, Op {Op}, Stage {Stage}", standardMSNNumberDto.ProdSeriesId, standardMSNNumberDto.DrawingNumberId, standardMSNNumberDto.IdNumberRange, standardMSNNumberDto.DepartmentId, standardMSNNumberDto.OperationNumber, standardMSNNumberDto.StageId);
                        throw new ValidationException("MSN number already exists for this combination of Production Series, Drawing Number, ID Number, Department, Operation, and Stage.");
                    }
                }

                // Check for overlapping ID number ranges
                int msnStandStart = standardMSNNumberDto.IdNumberStart ?? 0;
                int msnStandEnd = standardMSNNumberDto.IdNumberEnd ?? 0;
                if (msnStandStart > 0 && msnStandEnd > 0)
                {
                    if (await _repository.ExistsMsnIdConflictAsync(standardMSNNumberDto.ProdSeriesId, standardMSNNumberDto.DrawingNumberId ?? 0, msnStandStart, msnStandEnd, standardMSNNumberDto.DepartmentId ?? 0, standardMSNNumberDto.OperationNumber ?? string.Empty, standardMSNNumberDto.StageId ?? 0))
                    {
                        _logger.LogWarning("Overlapping Standard MSN ID range for ProdSeriesId {ProdSeriesId}, DrawingNumberId {DrawingNumberId}, Range {IdStart}-{IdEnd}, Dept {Dept}, Op {Op}, Stage {Stage}", standardMSNNumberDto.ProdSeriesId, standardMSNNumberDto.DrawingNumberId, msnStandStart, msnStandEnd, standardMSNNumberDto.DepartmentId, standardMSNNumberDto.OperationNumber, standardMSNNumberDto.StageId);
                        throw new ValidationException("Overlapping ID range for this specific combination of Production Series, Drawing Number, Department, Operation, and Stage.");
                    }
                }

                if (validateMSN != null)
                {
                    _logger.LogWarning("Attempted to create duplicate MSN number: {GeneratedMSNNumber}", generatedMSNNumber);
                    throw new ValidationException("MSNNumber is already generated.");
                }

                msnNumber.MsnNumber = generatedMSNNumber;

                // Populate stage name from stageid if stageid is provided
                if (msnNumber.StageId.HasValue && msnNumber.StageId.Value > 0)
                {
                    var stage = await _commonRepository.GetStageById(msnNumber.StageId.Value);
                    if (stage != null)
                    {
                        msnNumber.Stage = stage.StageName;
                    }
                }

                _logger.LogDebug("Attempting to insert Standard MSN number record to database");
                var insertedMSNNumber = await _repository.InsertStandardMSNNumberAsync(msnNumber);

                _logger.LogInformation("Successfully inserted Standard MSNNumber details: {@MSNNumber}", insertedMSNNumber);
                return insertedMSNNumber;
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "Validation error occurred while inserting Standard MSNNumber: {ErrorMessage}", vex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while inserting Standard MSNNumber details for department {Department} and production series {ProdSeries}",
                    standardMSNNumberDto.DepartmentName, standardMSNNumberDto.ProdSeriesId);
                throw;
            }
        }

        public byte[] GenerateDownloadMSNMemoPdf(DownloadMSNMemoDto request)
        {
            try
            {
                request ??= new DownloadMSNMemoDto();
                QuestPDF.Settings.License = LicenseType.Community;

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(24);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Content().Column(column =>
                        {
                            column.Spacing(10);

                            column.Item().Border(1).Padding(10).Column(header =>
                            {
                                header.Item().AlignCenter().Text("MSN Memo").SemiBold().FontSize(16);
                                header.Item().PaddingTop(5).Row(row =>
                                {
                                    row.RelativeItem().Text($"Contract No: {request.ProductionOrderNumber}");
                                    row.RelativeItem().AlignRight().Text($"Date: {DateTime.Now:dd/MM/yyyy}");
                                });
                                header.Item().Row(row =>
                                {
                                    row.RelativeItem().Text($"MSN: {request.MsnNumber}");
                                    row.RelativeItem().AlignRight().Text($"Project: {request.ProjectNumber}");
                                });
                                header.Item().Row(row =>
                                {
                                    row.RelativeItem().Text($"Operation No: {request.OperationNumber}");
                                    row.RelativeItem().AlignRight().Text($"Qty: {request.Quantity}");
                                });
                            });

                            column.Item().Border(1).Padding(10).Column(addresses =>
                            {
                                addresses.Spacing(6);
                                addresses.Item().Text("From: Quality Control, Godrej Aerospace Division");
                                addresses.Item().Text("To: Officer Incharge, Resident MSQAA Cell, Godrej Mumbai");
                            });

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    string[] headers =
                                    {
                                "Sr No", "Item Code", "ID Range",
                                "Stage", "Total Qty", "Accepted Qty",
                                "Rejected Qty", "Remarks"
                            };
                                    foreach (var title in headers)
                                    {
                                        header.Cell().Border(1)
                                            .Background(Colors.Grey.Lighten2)
                                            .Padding(4).Text(title).SemiBold();
                                    }
                                });

                                string[] values =
                                {
                            "1",
                            request.LnItemCode,
                            request.IdNumberRange,
                            request.StageId.ToString(),
                            request.Quantity.ToString(),
                            "",
                            "",
                            request.Remark
                        };

                                foreach (var value in values)
                                {
                                    table.Cell().Border(1).Padding(4).Text(value ?? string.Empty);
                                }
                            });

                            column.Item().Border(1).Padding(10).Column(details =>
                            {
                                details.Spacing(6);
                                details.Item().Text($"Production Order: {request.ProductionOrderNumber}");
                                details.Item().Text($"Drawing Number ID: {request.DrawingNumberId}");
                                details.Item().Text($"Observations: {request.Remark}");
                            });

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Border(1).Background(Colors.Grey.Lighten2)
                                        .Padding(4).Text("QC").SemiBold();
                                    header.Cell().Border(1).Background(Colors.Grey.Lighten2)
                                        .Padding(4).Text("Production").SemiBold();
                                });

                                table.Cell().Border(1).Padding(8).Column(qc =>
                                {
                                    qc.Item().Text("Name: ");
                                    qc.Item().Text($"Date: {DateTime.Now:dd/MM/yyyy}");
                                });

                                table.Cell().Border(1).Padding(8).Column(prod =>
                                {
                                    prod.Item().Text("Name: ");
                                    prod.Item().Text($"Date: {DateTime.Now:dd/MM/yyyy}");
                                });
                            });
                        });
                    });
                });

                using var stream = new MemoryStream();
                document.GeneratePdf(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for MSN Memo");
                throw;
            }
        }

        public async Task<string> GenerateDownloadMSNMemoHtml(DownloadMSNMemoDto request)
        {
            try
            {
                request ??= new DownloadMSNMemoDto();
                string drawingNumber = _repository.GetDrawingNumberByID(request.DrawingNumberId).Result;

                // Fetch user signature from DB if CreatedBy is set
                string signatureImgTag = "";
                if (request.CreatedBy > 0)
                {
                    var signatureBase64 = await _commonRepository.GetUserSignatureAsync(request.CreatedBy);
                    if (!string.IsNullOrWhiteSpace(signatureBase64))
                    {
                        // Step 1: Trim whitespace
                        var cleaned = signatureBase64.Trim();

                        // Step 2: Remove data URI prefix if present
                        if (cleaned.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                        {
                            var commaIndex = cleaned.IndexOf(',');
                            if (commaIndex >= 0)
                                cleaned = cleaned.Substring(commaIndex + 1).Trim();
                        }

                        // Step 3: Remove ALL whitespace/newlines that corrupt base64
                        cleaned = cleaned
                            .Replace("\r\n", "")
                            .Replace("\r", "")
                            .Replace("\n", "")
                            .Replace("\t", "")
                            .Replace(" ", "");

                        // Step 4: Validate and rebuild clean base64
                        try
                        {
                            var bytes = Convert.FromBase64String(cleaned);
                            var validBase64 = Convert.ToBase64String(bytes); // guaranteed clean single-line
                            signatureImgTag = $"<img src=\"data:image/png;base64,{validBase64}\" " +
                                              $"style=\"max-height:55px; max-width:160px; display:block; margin-top:5px;\" " +
                                              $"alt=\"Signature\" />";
                        }
                        catch (FormatException)
                        {
                            _logger.LogWarning("Invalid base64 signature for user {UserId}", request.CreatedBy);
                            // signatureImgTag stays empty string — no crash
                        }
                    }
                }

                string htmlTemplate = GetMSNMemoTemplate();

                var placeholders = new Dictionary<string, string>
                {
                    ["{{contractNo}}"] = request.ProductionOrderNumber,
                    ["{{dtd}}"] = DateTime.Now.ToString("dd/MM/yyyy"),
                    ["{{formatNo}}"] = "",
                    ["{{pageNo}}"] = "1",
                    ["{{revision}}"] = "",
                    ["{{wef}}"] = "",
                    ["{{fromName}}"] = "Quality Control, Godrej Aerospace Division",
                    ["{{toName}}"] = "Officer Incharge, Resident MSQAA Cell, Godrej Mumbai",
                    ["{{project}}"] = request.ProjectNumber,
                    ["{{date}}"] = DateTime.Now.ToString("dd/MM/yyyy"),
                    ["{{msn}}"] = request.MsnNumber,
                    ["{{srNo}}"] = "1",
                    ["{{itemName}}"] = request.LnItemCode,
                    ["{{identificationNo}}"] = request.IdNumberRange,
                    ["{{qty}}"] = request.Quantity.ToString(),
                    ["{{godrejRemarks}}"] = request.Remark,
                    ["{{msqaaRemarks}}"] = "",
                    ["{{drawingNumber}}"] = drawingNumber.ToString(),
                    ["{{rcNos}}"] = request.OperationNumber,
                    ["{{itemRevision}}"] = "",
                    ["{{anodisation}}"] = "",
                    ["{{material}}"] = "",
                    ["{{painting}}"] = "",
                    ["{{rtcNo}}"] = "",
                    ["{{irRef}}"] = "",
                    ["{{qcName}}"] = request.UserName,
                    ["{{msqaa}}"] = "",
                    ["{{qcDate}}"] = DateTime.Now.ToString("dd/MM/yyyy"),
                    ["{{prodName}}"] = "",
                    ["{{prodDate}}"] = DateTime.Now.ToString("dd/MM/yyyy"),
                    ["{{drgNo1}}"] = "",
                    ["{{drgNo2}}"] = "",
                    ["{{drgNo3}}"] = "",
                    ["{{stage}}"] = request.StageId.ToString(),
                    ["{{totalQty}}"] = request.Quantity.ToString(),
                    ["{{acceptedQty}}"] = "",
                    ["{{rejectedQty}}"] = "",
                    ["{{reworkQty}}"] = "",
                    ["{{observations}}"] = request.Remark,
                    ["{{signatureImage}}"] = signatureImgTag   // injected signature
                };

                foreach (var placeholder in placeholders)
                {
                    // signatureImage contains raw HTML — do NOT HTML-encode it
                    if (placeholder.Key == "{{signatureImage}}")
                        htmlTemplate = htmlTemplate.Replace(placeholder.Key, placeholder.Value);
                    else
                        htmlTemplate = htmlTemplate.Replace(placeholder.Key, HtmlEncode(placeholder.Value));
                }

                return htmlTemplate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating HTML template for MSN Memo");
                throw;
            }
        }






        private static string HtmlEncode(string? value)
            => System.Net.WebUtility.HtmlEncode(value ?? string.Empty);

        private static string GetMSNMemoTemplate()
        {
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Content", "Templates", "MSNMemo.html");
            if (File.Exists(templatePath))
            {
                return File.ReadAllText(templatePath);
            }

            // Option A: return the raw string as before (fine for now)
            // Option B (recommended): read from a .html file in wwwroot/Templates/
            //   return File.ReadAllText("wwwroot/Templates/MSNMemo.html");
            return @"<!DOCTYPE html>
<html lang=""en"">

<head>
  <meta charset=""UTF-8"">
  <title>Inspection Memo Stage Template</title>
  <style>
    /* Global Print and Layout Settings */
    @page {
      size: A4 portrait;
      margin: 12mm 10mm 12mm 10mm;
    }

    @media print {
      body {
        margin: 0;
        padding: 0;
        background-color: #fff;
        -webkit-print-color-adjust: exact;
        print-color-adjust: exact;
      }

      .page-container {
        border: none !important;
        box-shadow: none !important;
        padding: 0 !important;
        margin: 0 !important;
        width: 100% !important;
        height: auto !important;
      }
    }

    body {
      font-family: ""Helvetica Neue"", Helvetica, Arial, sans-serif;
      font-size: 11px;
      line-height: 1.35;
      color: #000;
      margin: 0;
      padding: 20px;
      background-color: #f0f2f5;
      display: flex;
      justify-content: center;
    }

    /* A4 Sheet Container */
    .page-container {
      background-color: #fff;
      width: 190mm;
      /* Adjusted A4 printable width to fit margins */
      min-height: 272mm;
      padding: 10mm;
      box-sizing: border-box;
      border: 1px solid #d3d3d3;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
      position: relative;
    }

    /* Internal Classification Header */
    .classification-header {
      text-align: center;
      font-size: 9px;
      text-transform: uppercase;
      letter-spacing: 1.5px;
      margin-bottom: 6px;
      color: #555;
      font-weight: 500;
    }

    /* Outer Table Border */
    .main-border-box {
      border: 1.5px solid #000;
      background-color: #fff;
    }

    /* Top Grid Header styling */
    .header-table {
      width: 100%;
      border-collapse: collapse;
      border-bottom: 1.5px solid #000;
    }

    .header-table td {
      border-right: 1px solid #000;
      padding: 6px 8px;
      vertical-align: top;
    }

    .header-table td:last-child {
      border-right: none;
    }

    /* Left Logo Cell */
    .logo-cell {
      width: 25%;
      text-align: center;
      vertical-align: middle !important;
      padding: 10px 4px !important;
    }

    .logo-container {
      display: inline-block;
      text-align: left;
    }

    .logo-brand {
      font-family: ""Georgia"", serif;
      font-style: italic;
      font-size: 22px;
      font-weight: 900;
      color: #000;
      letter-spacing: -0.5px;
      line-height: 1;
    }

    .logo-division {
      font-size: 7.5px;
      letter-spacing: 2px;
      text-transform: uppercase;
      font-weight: 800;
      display: block;
      margin-top: 1px;
      border-top: 1px solid #000;
      padding-top: 1px;
    }

    /* Center Cell */
    .center-header-cell {
      width: 50%;
      text-align: center;
      padding-top: 8px !important;
    }

    .company-title {
      font-size: 13px;
      font-weight: bold;
      margin: 0;
      letter-spacing: 0.2px;
    }

    .division-title {
      font-size: 12px;
      font-weight: bold;
      margin: 2px 0 1px 0;
    }

    .memo-title {
      font-size: 14px;
      font-weight: 800;
      text-transform: uppercase;
      margin: 4px 0;
      letter-spacing: 0.5px;
    }

    .contract-info {
      font-size: 10px;
      margin: 4px 0 0 0;
    }

    /* Right Block Cell */
    .right-header-cell {
      width: 25%;
      font-size: 10px;
      padding: 6px 10px !important;
    }

    .right-header-cell table {
      width: 100%;
      border-collapse: collapse;
    }

    .right-header-cell table td {
      border: none;
      padding: 2.5px 0;
      font-size: 10px;
    }

    .right-header-cell .label {
      width: 55%;
      color: #333;
    }

    .right-header-cell .colon {
      width: 10%;
      text-align: center;
    }

    .right-header-cell .value {
      width: 35%;
      font-weight: bold;
      text-align: left;
    }

    .handshake-icon {
      text-align: center;
      margin-top: 8px;
    }

    /* From/To Metadata Table */
    .metadata-table {
      width: 100%;
      border-collapse: collapse;
      border-bottom: 1.5px solid #000;
    }

    .metadata-table td {
      border-bottom: 1px solid #000;
      padding: 6px 10px;
      vertical-align: middle;
      font-size: 10.5px;
    }

    .metadata-table tr:last-child td {
      border-bottom: none;
    }

    .meta-col-left {
      width: 55%;
      border-right: 1px solid #000;
    }

    .meta-col-right {
      width: 45%;
    }

    /* Intro Text block */
    .instruction-text {
      padding: 8px 10px;
      font-size: 10px;
      border-bottom: 1.5px solid #000;
      background-color: #fafafa;
      line-height: 1.4;
    }

    /* Main Table Grid styling */
    .details-table {
      width: 100%;
      border-collapse: collapse;
    }

    .details-table th {
      border-right: 1px solid #000;
      border-bottom: 1.5px solid #000;
      padding: 6px 4px;
      font-size: 9.5px;
      font-weight: bold;
      text-align: center;
      vertical-align: middle;
      background-color: #f7f7f7;
    }

    .details-table td {
      border-right: 1px solid #000;
      border-bottom: 1px solid #000;
      padding: 6px;
      vertical-align: top;
      font-size: 9.5px;
    }

    /* Sub-rows/cells styling in columns 2 and 5 */
    .sub-border-bottom {
      border-bottom: 1px solid #000;
      padding-bottom: 5px;
      margin-bottom: 5px;
      min-height: 16px;
    }

    .sub-border-bottom:last-child {
      border-bottom: none;
      padding-bottom: 0;
      margin-bottom: 0;
    }

    /* Signature area table */
    .signature-area {
      width: 100%;
      border-collapse: collapse;
      border-top: 1.5px solid #000;
      background-color: #fff;
    }

    .signature-area td {
      border: none;
      padding: 12px 15px;
      vertical-align: top;
      width: 50%;
    }

    .signature-title {
      font-weight: bold;
      font-size: 11px;
      margin-bottom: 35px;
    }

    .signature-details {
      font-size: 10px;
      line-height: 1.5;
    }

    .signature-details table {
      width: 100%;
      border-collapse: collapse;
    }

    .signature-details td {
      padding: 2px 0;
      border: none !important;
    }

    /* CSS Stamps & Signatures Styling */
    .stamp-container {
      position: relative;
      min-height: 65px;
      margin-bottom: 8px;
    }

    .signature-line {
      border-bottom: 1px dashed #666;
      width: 140px;
      margin-top: 25px;
      font-style: italic;
      color: #555;
      font-size: 9px;
      padding-bottom: 2px;
    }
  </style>
</head>

<body>
  <!-- Classification header above the printable page box -->
  <div class=""classification-header""></div>

  <div class=""page-container"">
    <div class=""main-border-box"">

      <!-- HEADER PART -->
      <table class=""header-table"">
        <tr>
          <!-- Logo Cell (Left) -->
          <td class=""logo-cell"">
            <img src=""data:image/png;base64,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"" alt=""Godrej Aerospace"" style=""max-width: 140px; max-height: 40px; display: block; margin: 0 auto;"" />
          </td>

          <!-- Center Header Info -->
          <td class=""center-header-cell"">
            <div class=""company-title"">Godrej & Boyce Mfg. Co. Ltd.</div>
            <div class=""division-title"">Godrej Aerospace Division</div>
            <div class=""memo-title"">Inspection Memo Stage</div>
            <div class=""contract-info"">Contract No: <strong>{{contractNo}}</strong> Dtd: <strong>{{dtd}}</strong></div>
          </td>

          <!-- Right Stats Cell -->
          <td class=""right-header-cell"">
            <table>
              <tr>
                <td class=""label"">Format No</td>
                <td class=""colon"">:</td>
                <td class=""value"">{{formatNo}}</td>
              </tr>
              <tr>
                <td class=""label"">Page No</td>
                <td class=""colon"">:</td>
                <td class=""value"">{{pageNo}}</td>
              </tr>
              <tr>
                <td class=""label"">Revision</td>
                <td class=""colon"">:</td>
                <td class=""value"">{{revision}}</td>
              </tr>
              <tr>
                <td class=""label"">W.E.F.</td>
                <td class=""colon"">:</td>
                <td class=""value"">{{wef}}</td>
              </tr>
            </table>

            <!-- Handshake Icon -->
            <div class=""handshake-icon"">
              <img src=""data:image/png;base64,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"" alt=""Handshake"" style=""height: 18px; width: auto; display: block; margin: 8px auto 0 auto;"" />
            </div>
          </td>
        </tr>
      </table>

      <!-- FROM / TO DETAILS BLOCK -->
      <table class=""metadata-table"">
        <tr>
          <td class=""meta-col-left"">
            From: Quality Control, Godrej Aerospace Division
          </td>
          <td class=""meta-col-right"">
            Memo Stage No (MSN): <strong>{{msn}}</strong>
          </td>
        </tr>
        <tr>
          <td class=""meta-col-left"">
            To: Officer Incharge, Resident MSQAA Cell, Godrej Mumbai
          </td>
          <td class=""meta-col-right"">
            Date: <strong>{{date}}</strong>
          </td>
        </tr>
      </table>

      <!-- GENERAL INSTRUCTION LINE -->
      <div class=""instruction-text"">
        Following Raw Material / Components / Sub-Assembly /Assembly are ready for your Inspection & all relevant documents are available.
      </div>

      <!-- MAIN DYNAMIC TABLE GRID -->
      <table class=""details-table"">
        <thead>
          <tr>
            <th style=""width: 5%;"">Sr. No.</th>
            <th style=""width: 30%;"">Name of the R Mat. /Comp/<br>Sub-Assy/Assy/Operation</th>
            <th style=""width: 15%;"">Identification No</th>
            <th style=""width: 8%;"">Qty</th>
            <th style=""width: 25%;"">Inspection Report Details</th>
            <th style=""width: 10%;"">Remarks By Godrej</th>
            <th style=""width: 17%; border-right: none;"">Remarks by MSQAA</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <!-- Col 1: Sr. No. (Row 1) -->
            <td style=""width: 5%; text-align: center; font-weight: bold; vertical-align: top; padding: 6px;"">
              {{srNo}}
            </td>

            <!-- Col 2: Name of the R Mat... (Row 1) -->
            <td style=""width: 30%; vertical-align: top; padding: 6px;"">
              <strong>{{itemName}}</strong>
            </td>

            <!-- Col 3: Identification No (Row 1) -->
            <td style=""width: 15%; text-align: center; vertical-align: top; padding: 6px;"">
              {{identificationNo}}
            </td>

            <!-- Col 4: Qty (Row 1) -->
            <td style=""width: 8%; text-align: center; font-weight: bold; vertical-align: top; padding: 6px;"">
              {{qty}}
            </td>

            <!-- Col 5: Inspection Report Details (Row 1) -->
            <td style=""width: 25%; vertical-align: top; font-size: 8.5px; line-height: 1.25; padding: 6px;"">
              Components delegated for all stages upto Surface treatment to Godrej QC as per letter no. RMC(MUM)/103 dtd: {{date}}
            </td>

            <!-- Col 6: Remarks By Godrej (rowspan 5) -->
            <td rowspan=""5"" style=""width: 10%; text-align: center; vertical-align: middle; padding: 6px;"">
              {{godrejRemarks}}
            </td>

            <!-- Col 7: Remarks by MSQAA (rowspan 5) -->
            <td rowspan=""5"" style=""width: 17%; vertical-align: middle; line-height: 1.35; padding: 8px; border-right: none;"">
              {{msqaaRemarks}}
            </td>
          </tr>
          <tr>
            <!-- Col 1: Sr. No. (Row 2) -->
            <td style=""text-align: center; vertical-align: top; padding: 6px;"">
              &nbsp;
            </td>

            <!-- Col 2: Name of the R Mat... (Row 2) -->
            <td style=""vertical-align: top; padding: 6px;"">
              {{drawingNumber}}
            </td>

            <!-- Col 3: Identification No (Row 2) -->
            <td style=""text-align: center; vertical-align: top; padding: 6px;"">
              &nbsp;
            </td>

            <!-- Col 4: Qty (Row 2) -->
            <td style=""text-align: center; vertical-align: top; padding: 6px;"">
              &nbsp;
            </td>

            <!-- Col 5: Inspection Report Details (Row 2) -->
            <td style=""vertical-align: top; font-size: 8.5px; line-height: 1.25; padding: 6px;"">
              01. R/C Nos.: {{rcNos}}
            </td>
          </tr>
          <tr>
            <!-- Col 1: Sr. No. (Row 3) -->
            <td style=""text-align: center; vertical-align: top; padding: 6px;"">
              &nbsp;
            </td>

            <!-- Col 2: Name of the R Mat... (Row 3) -->
            <td style=""vertical-align: top; padding: 6px;"">
              Rev.no: {{itemRevision}}
            </td>

            <!-- Col 3: Identification No (Row 3) -->
            <td style=""text-align: center; vertical-align: top; padding: 6px;"">
              &nbsp;
            </td>

            <!-- Col 4: Qty (Row 3) -->
            <td style=""text-align: center; vertical-align: top; padding: 6px;"">
              &nbsp;
            </td>

            <!-- Col 5: Inspection Report Details (Row 3) -->
            <td style=""vertical-align: top; font-size: 8.5px; line-height: 1.25; padding: 6px;"">
              02. Anodisation: {{anodisation}}
            </td>
          </tr>
          <tr>
            <!-- Col 1: Sr. No. (Row 4) -->
            <td style=""text-align: center; vertical-align: top; padding: 6px;"">
              &nbsp;
            </td>

            <!-- Col 2: Name of the R Mat... (Row 4) -->
            <td style=""vertical-align: top; padding: 6px;"">
              MATL: {{material}}
            </td>

            <!-- Col 3: Identification No (Row 4) -->
            <td style=""text-align: center; vertical-align: top; padding: 6px;"">
              &nbsp;
            </td>

            <!-- Col 4: Qty (Row 4) -->
            <td style=""text-align: center; vertical-align: top; padding: 6px;"">
              &nbsp;
            </td>

            <!-- Col 5: Inspection Report Details (Row 4) -->
            <td style=""vertical-align: top; font-size: 8.5px; line-height: 1.25; padding: 6px;"">
              03. Painting: {{painting}}
            </td>
          </tr>
          <tr>
            <!-- Col 1: Sr. No. (Row 5) -->
            <td style=""text-align: center; vertical-align: top; padding: 6px;"">
              &nbsp;
            </td>

            <!-- Col 2: Name of the R Mat... (Row 5) -->
            <td style=""vertical-align: top; padding: 6px;"">
              RTC No: {{rtcNo}}
            </td>

            <!-- Col 3: Identification No (Row 5) -->
            <td style=""text-align: center; vertical-align: top; padding: 6px;"">
              &nbsp;
            </td>

            <!-- Col 4: Qty (Row 5) -->
            <td style=""text-align: center; vertical-align: top; padding: 6px;"">
              &nbsp;
            </td>

            <!-- Col 5: Inspection Report Details (Row 5) -->
            <td style=""vertical-align: top; font-size: 8.5px; line-height: 1.25; padding: 6px;"">
              04. IR: {{irRef}} (for reference)
            </td>
          </tr>
        </tbody>
      </table>

      <!-- SIGNATURE AND APPROVAL FOOTER -->
      <table class=""signature-area"">
        <tr>
          <!-- Left: Godrej QC Signature Section -->
          <td style=""padding-bottom: 20px;"">
            <div class=""signature-title"">Godrej QC:</div>

            <div class=""stamp-container"">
              <!-- Signature image (injected from tbl_user_signatures) or blank line -->
              {{signatureImage}}
              <div class=""signature-line""></div>
            </div>

            <div class=""signature-details"">
              <table style=""width: 100%;"">
                <tr>
                  <td style=""width: 75px; color: #555;"">Name</td>
                  <td style=""width: 10px;"">:</td>
                  <td style=""font-weight: bold;"">{{qcName}}</td>
                </tr>
                <tr>
                  <td style=""color: #555;"">Designation</td>
                  <td>:</td>
                  <td>Assistant Manager</td>
                </tr>
              </table>
            </div>
          </td>

          <!-- Right: MSQAA Officer Signature Section -->
          <td style=""padding-bottom: 20px;"">
            <div class=""signature-title"" style=""display: flex; justify-content: space-between; align-items: center; margin-bottom: 6px;"">
              <span>MSQAA:</span>
              <span style=""font-weight: bold; font-size: 11px; padding-right: 15px;"">{{msqaa}}</span>
            </div>

            <div class=""signature-details"">
              <div style=""color: #555; font-weight: normal; font-size: 10px;"">(Officer)</div>
            </div>
          </td>
        </tr>
      </table>

    </div>
  </div>
</body>

</html>"; // your existing template here
        }
    }
}
