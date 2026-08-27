using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DataModel.Assembly;
using Godrej.Precheck.Models.DataModel.Common;
using Godrej.Precheck.Models.DataModel.Precheck;
using Godrej.Precheck.Models.DTOs.Assembly;
using Godrej.Precheck.Models.DTOs.DrawingNumber;
using Godrej.Precheck.Models.DTOs.IRNumber;
using Godrej.Precheck.Models.DTOs.MSNNumber;
using Godrej.Precheck.Models.DTOs.Precheck;
using Godrej.Precheck.Models.DTOs.Stage;
using Godrej.Precheck.Models.DTOs.User;
using Godrej.Precheck.Repository.Repository.CommonRepository;
using Godrej.Precheck.Service.Cache;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Service.Service.CommonSevice
{
    public class CommonService : ICommonService
    {
        private readonly ICommonRepository _commonRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CommonService> _logger;


        private const int SaltSize = 128 / 8;
        private const int Iterations = 100000;
        private const int HashSize = 256 / 8;

        public CommonService(ICommonRepository commonRepository,ICacheService cacheService,ILogger<CommonService> logger)
        {
            _commonRepository = commonRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<List<SecurityQuestionsInfo>> GetAllSecurityQuestions()
        {
            _logger.LogInformation("Starting GetAllModules");
            try
            {
                _logger.LogDebug("Attempting to get precheck modules from cache: {CacheKey}", CacheSettings.PrecheckModulesCacheKey);
                // Get all precheck modules from cache or repository
                var allModules = await _cacheService.GetOrSetAsync(
                    CacheSettings.PrecheckModulesCacheKey,
                    async () => {
                        _logger.LogDebug("Cache miss for precheck modules, fetching from repository");
                        return await _commonRepository.GetAllSecurityQuestionModule();
                    },
                    CacheSettings.PrecheckModulesCacheDuration
                );

                _logger.LogInformation("Successfully retrieved {Count} precheck modules", allModules?.Count ?? 0);
                return allModules;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving precheck modules: {ErrorMessage}", ex.Message);
                throw;
            }
        }
        public async Task<AddUserResponseDto?> AddUserAsync(AddUserRequestDto request, int createdBy)
        {
            try
            {
                _logger.LogInformation("Starting AddUserAsync for user: {UserName}", request.UserName);

                // ✅ Use same HashPassword method as RegisterAsync
                if (!string.IsNullOrEmpty(request.Password))
                {
                    var (hash, securityStamp) = HashPassword(request.Password);
                    request.Password = hash;           // ✅ BCrypt hash
                    request.SecurityStamp = securityStamp;  // ✅ SecurityStamp from same password
                }

                var result = await _commonRepository.AddUserAsync(request, createdBy);
                if (result == null)
                {
                    _logger.LogWarning("Failed to add user: {UserName}", request.UserName);
                    return null;
                }
                _logger.LogInformation("Successfully added user: {UserName}", request.UserName);
                return result;
            }
            catch (ApplicationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding user: {UserName}", request.UserName);
                throw;
            }
        }

        // Updated secure password hashing method
        private (string Hash, string SecurityStamp) HashPassword(string password)
        {
            // Generate a cryptographically secure random salt
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password using PBKDF2 with HMAC-SHA256
            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: HashSize
            );

            // Store both hash and salt as Base64 strings
            string hashString = Convert.ToBase64String(hash);
            string saltString = Convert.ToBase64String(salt);

            return (Hash: hashString, SecurityStamp: saltString);
        }
        public async Task<List<PageRoleAccessResponseDto>> GetAllPageRoleAccessAsync(int roleId)
        {
            try
            {
                _logger.LogInformation("Starting GetAllPageRoleAccessAsync");
                var result = await _commonRepository.GetAllPageRoleAccessAsync(roleId);
                if (result == null || !result.Any())
                {
                    _logger.LogWarning("No page role access records found");
                    return new List<PageRoleAccessResponseDto>();
                }
                _logger.LogInformation("Retrieved {Count} page role access records", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllPageRoleAccessAsync");
                throw;
            }
        }
        public async Task<List<PrecheckModule>> GetAllModules()
        {
            _logger.LogInformation("Starting GetAllModules");
            try
            {
                _logger.LogDebug("Attempting to get precheck modules from cache: {CacheKey}", CacheSettings.PrecheckModulesCacheKey);
                // Get all precheck modules from cache or repository
                var allModules = await _cacheService.GetOrSetAsync(
                    CacheSettings.PrecheckModulesCacheKey,
                    async () => {
                        _logger.LogDebug("Cache miss for precheck modules, fetching from repository");
                        return await _commonRepository.GetAllPrecheckModule();
                    },
                    CacheSettings.PrecheckModulesCacheDuration
                );

                _logger.LogInformation("Successfully retrieved {Count} precheck modules", allModules?.Count ?? 0);
                return allModules;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving precheck modules: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<List<ComponentsType>> ComponentTypeService()
        {
            _logger.LogInformation("Starting ComponentTypeService");
            try
            {
                _logger.LogDebug("Attempting to get component types from cache: {CacheKey}", CacheSettings.ComponentTypesCacheKey);
                // Get all component types from cache or repository
                var allComponentTypes = await _cacheService.GetOrSetAsync(
                    CacheSettings.ComponentTypesCacheKey,
                    async () => {
                        _logger.LogDebug("Cache miss for component types, fetching from repository");
                        return await _commonRepository.GetAllComponentType();
                    },
                    CacheSettings.ComponentTypesCacheDuration
                );

                _logger.LogInformation("Successfully retrieved {Count} component types", allComponentTypes?.Count ?? 0);
                return allComponentTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving component types: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<ComponentsType> ComponentTypeByNameService(string query)
        {
            _logger.LogInformation("Starting ComponentTypeByNameService with query: {Query}", query);
            try
            {
                _logger.LogDebug("Fetching component type by name: {Query}", query);
                var result = await _commonRepository.GetComponentTypeByNameAsync(query);

                if (result != null)
                {
                    _logger.LogInformation("Component type found for query: {Query}, Id: {Id}", query, result);
                }
                else
                {
                    _logger.LogWarning("No component type found for query: {Query}", query);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving component type by name: {Query}. Error: {ErrorMessage}", query, ex.Message);
                throw;
            }
        }

        public async Task<ProductionOrderModel> ProductionOrderByNameService(string query)
        {
            _logger.LogInformation("Starting ProductionOrderByNameService with query: {Query}", query);
            try
            {
                _logger.LogDebug("Fetching production order by name: {Query}", query);
                var result = await _commonRepository.GetProductionOrderByName(query);

                if (result != null)
                {
                    _logger.LogInformation("Production order found for query: {Query}, Id: {Id}", query, result.Id);
                }
                else
                {
                    _logger.LogWarning("No production order found for query: {Query}", query);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving production order by name: {Query}. Error: {ErrorMessage}", query, ex.Message);
                throw;
            }
        }

        public async Task<List<UnitModel>> UnitByName()
        {
            _logger.LogInformation("Starting UnitByName");
            try
            {
                _logger.LogDebug("Attempting to get units from cache: {CacheKey}", CacheSettings.UnitsCacheKey);
                // Get all units from cache or repository
                var allUnits = await _cacheService.GetOrSetAsync(
                    CacheSettings.UnitsCacheKey,
                    async () =>
                    {
                        _logger.LogDebug("Cache miss for units, fetching from repository");
                        return await _commonRepository.GetUnitByName();
                    },
                    CacheSettings.UnitsCacheDuration
                );

                _logger.LogInformation("Successfully retrieved {Count} units", allUnits?.Count ?? 0);
                return allUnits;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving units: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<List<ShapeModel>> GetAllShapes()
        {
            _logger.LogInformation("Starting GetAllShapes");
            try
            {
                _logger.LogDebug("Attempting to get shapes from cache: {CacheKey}", CacheSettings.ShapesCacheKey);
                var shapes = await _cacheService.GetOrSetAsync(
                    CacheSettings.ShapesCacheKey,
                    async () =>
                    {
                        _logger.LogDebug("Cache miss for shapes, fetching from repository");
                        return await _commonRepository.GetAllShapes();
                    },
                    CacheSettings.ShapesCacheDuration
                );

                _logger.LogInformation("Successfully retrieved {Count} shapes", shapes?.Count ?? 0);
                return shapes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shapes: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<ComponentsType> ComponentTypeByIdService(int id)
        {
            _logger.LogInformation("Starting ComponentTypeByIdService with Id: {Id}", id);
            try
            {
                _logger.LogDebug("Fetching component type by Id: {Id}", id);
                var result = await _commonRepository.GetComponentTypeByIdAsync(id);

                if (result != null)
                {
                    _logger.LogInformation("Component type found for Id: {Id}, Name: {Name}", id, result);
                }
                else
                {
                    _logger.LogWarning("No component type found for Id: {Id}", id);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving component type by Id: {Id}. Error: {ErrorMessage}", id, ex.Message);
                throw;
            }
        }

        public async Task<List<ProductionSeriess>> ProductionSeriesService()
        {
            _logger.LogInformation("Starting ProductionSeriesService");
            try
            {
                _logger.LogDebug("Attempting to get production series from cache: {CacheKey}", CacheSettings.ProductionSeriesCacheKey);
                // Get all production series from cache or repository
                var allProductionSeries = await _cacheService.GetOrSetAsync(
                    CacheSettings.ProductionSeriesCacheKey,
                    async () => {
                        _logger.LogDebug("Cache miss for production series, fetching from repository");
                        return await _commonRepository.GetAllProductionSeries();
                    },
                    CacheSettings.ProductionSeriesCacheDuration
                );

                _logger.LogInformation("Successfully retrieved {Count} production series", allProductionSeries?.Count ?? 0);
                return allProductionSeries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving production series: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<ProductionSeriess> ProductionSeriesByNameService(string query)
        {
            _logger.LogInformation("Starting ProductionSeriesByNameService with query: {Query}", query);
            try
            {
                _logger.LogDebug("Fetching production series by name: {Query}", query);
                var result = await _commonRepository.GetProductionSeriesByName(query);

                if (result != null)
                {
                    _logger.LogInformation("Production series found for query: {Query}, Id: {Id}", query, result.Id);
                }
                else
                {
                    _logger.LogWarning("No production series found for query: {Query}", query);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving production series by name: {Query}. Error: {ErrorMessage}", query, ex.Message);
                throw;
            }
        }

        public async Task<ProductionSeriess> ProductionSeriesByIdService(int id)
        {
            _logger.LogInformation("Starting ProductionSeriesByIdService with Id: {Id}", id);
            try
            {
                _logger.LogDebug("Fetching production series by Id: {Id}", id);
                var result = await _commonRepository.GetProductionSeriesById(id);

                if (result != null)
                {
                    _logger.LogInformation("Production series found for Id: {Id}, Name: {Name}", id, result);
                }
                else
                {
                    _logger.LogWarning("No production series found for Id: {Id}", id);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving production series by Id: {Id}. Error: {ErrorMessage}", id, ex.Message);
                throw;
            }
        }

        public async Task<List<IRNumbers>> IRNumberService(GetAllIRNumberRequestDto getAllIRNumberRequestDto)
        {
            _logger.LogInformation("Starting IRNumberService with request criteria");
            try
            {
                _logger.LogDebug("Fetching IR numbers with filter criteria");
                var result = await _commonRepository.GetIRnumber(getAllIRNumberRequestDto);

                _logger.LogInformation("Successfully retrieved {Count} IR numbers", result?.Count ?? 0);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving IR numbers. Error: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<List<IRNumbers>> IRNumberByDrawingNumberService(GetIRNumberByDrawingNumberRequest getIRNumberByDrawingNumberRequest)
        {
            _logger.LogInformation("Starting IRNumberByDrawingNumberService with DrawingNumber: {DrawingNumber}", getIRNumberByDrawingNumberRequest?.DrawingNumber);
            try
            {
                _logger.LogDebug("Fetching IR numbers by drawing number: {DrawingNumber}", getIRNumberByDrawingNumberRequest?.DrawingNumber);
                var result = await _commonRepository.GetIRNumberByDrawingNumber(getIRNumberByDrawingNumberRequest);

                _logger.LogInformation("Successfully retrieved {Count} IR numbers for drawing number: {DrawingNumber}",
                    result?.Count ?? 0, getIRNumberByDrawingNumberRequest?.DrawingNumber);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving IR numbers by drawing number: {DrawingNumber}. Error: {ErrorMessage}",
                    getIRNumberByDrawingNumberRequest?.DrawingNumber, ex.Message);
                throw;
            }
        }

        public async Task<List<string>> GetAllLnItemCode(string search = null)
        {
            _logger.LogInformation($"Starting GetAllLnItemCode with search: '{search}'");
            try
            {
                // Only cache the full list (no search parameter)
                if (string.IsNullOrEmpty(search))
                {
                    _logger.LogDebug("Attempting to get all LN Item Codes from cache: {CacheKey}", CacheSettings.LnItemCodesCacheKey);
                    var lnItemCodes = await _cacheService.GetOrSetAsync(
                        CacheSettings.LnItemCodesCacheKey,
                        async () => {
                            _logger.LogDebug("Cache miss for LN Item Codes, fetching from repository");
                            return await _commonRepository.GetAllLnItemCode(search);
                        },
                        CacheSettings.LnItemCodesCacheDuration
                    );

                    _logger.LogInformation("Successfully retrieved {Count} LN Item Codes from cache", lnItemCodes?.Count ?? 0);
                    return lnItemCodes;
                }
                else
                {
                    // Don't cache search results - they're dynamic and user-specific
                    _logger.LogDebug("Searching LN Item Codes (no cache)");
                    var results = await _commonRepository.GetAllLnItemCode(search);
                    _logger.LogInformation("Successfully retrieved {Count} LN Item Codes for search '{Search}'", results?.Count ?? 0, search);
                    return results;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving LN Item Codes: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<List<MSNNumbers>> MSNNumberByDrawingNumberService(GetMSNNumberByDrawingNumberRequest getMSNNumberByDrawingNumberRequest)
        {
            _logger.LogInformation("Starting MSNNumberByDrawingNumberService with DrawingNumber: {DrawingNumber}", getMSNNumberByDrawingNumberRequest?.DrawingNumber);
            try
            {
                _logger.LogDebug("Fetching MSN numbers by drawing number: {DrawingNumber}", getMSNNumberByDrawingNumberRequest?.DrawingNumber);
                var result = await _commonRepository.GetMSNNuberByDrawingNumber(getMSNNumberByDrawingNumberRequest);

                _logger.LogInformation("Successfully retrieved {Count} MSN numbers for drawing number: {DrawingNumber}",
                    result?.Count ?? 0, getMSNNumberByDrawingNumberRequest?.DrawingNumber);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving MSN numbers by drawing number: {DrawingNumber}. Error: {ErrorMessage}",
                    getMSNNumberByDrawingNumberRequest?.DrawingNumber, ex.Message);
                throw;
            }
        }

        public async Task<List<MSNNumbers>> MSNNumberService(GetAllMSNNumberRequestDto getAllMSNNumberRequestDto)
        {
            _logger.LogInformation("Starting MSNNumberService with request criteria");
            try
            {
                _logger.LogDebug("Fetching MSN numbers with filter criteria");
                var result = await _commonRepository.GetMSNNuber(getAllMSNNumberRequestDto);

                _logger.LogInformation("Successfully retrieved {Count} MSN numbers", result?.Count ?? 0);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving MSN numbers. Error: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<List<GetAllDrawingResponseDto>> GetAllDrawingNumberService(GetAllDrawingRequestDto request = null)
        {
            _logger.LogInformation("Starting GetAllDrawingNumberService with ComponentType filter: {ComponentType}", request?.ComponentType);
            try
            {
                // Key for cached grouped results
                var groupedCacheKey = $"{CacheSettings.DrawingNumbersCacheKey}";
                _logger.LogDebug("Attempting to get grouped drawing numbers from cache: {CacheKey}", groupedCacheKey);

                // Try to get grouped results from cache first
                var groupedDrawingNumbers = await _cacheService.GetOrSetAsync(
                    groupedCacheKey,
                    async () =>
                    {
                        _logger.LogDebug("Cache miss for grouped drawing numbers, processing raw data");
                        // Get raw data from original cache or repository
                        _logger.LogDebug("Attempting to get raw drawing numbers from cache: {CacheKey}", CacheSettings.DrawingNumbersCacheKey);
                        var allDrawingNumbers = await _cacheService.GetOrSetAsync(
                            CacheSettings.DrawingNumbersCacheKey,
                            async () => {
                                _logger.LogDebug("Cache miss for raw drawing numbers, fetching from repository");
                                return await _commonRepository.GetAllDrawingNumber();
                            },
                            CacheSettings.DrawingNumbersCacheDuration
                        );

                        _logger.LogDebug("Processing and grouping {Count} drawing numbers", allDrawingNumbers?.Count ?? 0);
                        // Process and group the data
                        var grouped = allDrawingNumbers
                            .GroupBy(d => d.DrawingNumber)
                            .Select(g => new GetAllDrawingResponseDto
                            {
                                Id = g.First().Id,
                                DrawingNumber = g.Key,
                                ComponentCode = g.First().ComponentCode,
                                LnItemCode = g.First().LnItemCode,
                                CreatedBy = g.First().CreatedBy,
                                CreatedDate = g.First().CreatedDate,
                                ModifiedBy = g.First().ModifiedBy,
                                ModifiedDate = g.First().ModifiedDate,
                                Location = g.First().Location,
                                Nomenclature = g.First().Nomenclature,
                                NomenclatureId = g.First().NomenclatureId,
                                ComponentType = g.First().ComponentType,
                                DocumentType = g.First().DocumentType,
                                RackLocationId=g.First().RackLocationId,
                                LnItemCodeId = g.First().LnItemCodeId,
                                UnitId = g.First().UnitId,
                                UnitName = g.First().UnitName,
                                ComponentTypeId = g.First().ComponentTypeId,
                                DocumentTypeId = g.First().DocumentTypeId,
                                IsExpiry = g.First().IsExpiry,
                                ParentDrawingNumberIds = g.Select(x => x.ParentDrawingNumberId)
                                    .Where(x => x.HasValue)
                                    .Select(x => x.Value)
                                    .Distinct()
                                    .ToList(),
                                ParentDrawingNumbers = g.Select(x => x.ParentDrawingNumber)
                                    .Where(x => !string.IsNullOrWhiteSpace(x))
                                    .Distinct()
                                    .ToList(),
                                AvailableSeriesId = g.Select(x => x.AvailableSeriesId)
                                    .Where(x => x != null)
                                    .Distinct()
                                    .ToList(),
                                AvailableSeries = g.Select(x => x.AvailableSeries)
                                    .Where(x => x != null)
                                    .Distinct()
                                    .ToList(),
                                //AssemblyId = g.First().AssemblyId,
                                //AssemblyNumber = g.First().AssemblyNumber
                            })
                            .ToList();
                        _logger.LogDebug("Grouped raw drawing numbers into {Count} unique drawing numbers", grouped.Count);
                        return grouped;
                    },
                    CacheSettings.DrawingNumbersCacheDuration
                );

                List<GetAllDrawingResponseDto> result;
                if (request != null)
                {
                    _logger.LogDebug("Filtering drawing numbers with ComponentType: {ComponentType}, Search: {Search}", 
                        request.ComponentType, request.Search);
                    
                    var query = groupedDrawingNumbers.AsQueryable();

                    if (!string.IsNullOrEmpty(request.ComponentType))
                    {
                        query = query.Where(d => d.ComponentType == request.ComponentType);
                    }

                    if (!string.IsNullOrEmpty(request.Search))
                    {
                        var search = request.Search.ToLower();
                        query = query.Where(d => 
                            (d.DrawingNumber != null && d.DrawingNumber.ToLower().Contains(search)) ||
                            (d.Nomenclature != null && d.Nomenclature.ToLower().Contains(search)) ||
                            (d.LnItemCode != null && d.LnItemCode.ToLower().Contains(search))
                        );
                    }

                    result = query.OrderBy(d => d.Id).ToList();
                    
                    _logger.LogInformation("Found {Count} drawing numbers after filtering", result.Count);
                }
                else
                {
                    // Return all cached grouped results
                    result = groupedDrawingNumbers
                        .OrderBy(d => d.Id)
                        .ToList();
                    _logger.LogInformation("Retrieved all {Count} drawing numbers", result.Count);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving drawing numbers. Error: {ErrorMessage}", ex.Message);
                throw new ApplicationException("Error retrieving drawing numbers", ex);
            }
        }

        public async Task<List<DocumentTypes>> DocumentTypeService()
        {
            _logger.LogInformation("Starting DocumentTypeService");
            try
            {
                _logger.LogDebug("Attempting to get document types from cache: {CacheKey}", CacheSettings.DocumentTypesCacheKey);
                // Get all document types from cache or repository
                var allDocumentTypes = await _cacheService.GetOrSetAsync(
                    CacheSettings.DocumentTypesCacheKey,
                    async () => {
                        _logger.LogDebug("Cache miss for document types, fetching from repository");
                        return await _commonRepository.GetAllDocumnetType();
                    },
                    CacheSettings.DocumentTypesCacheDuration
                );

                _logger.LogInformation("Successfully retrieved {Count} document types", allDocumentTypes?.Count ?? 0);
                return allDocumentTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document types: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<DocumentTypes> DocumentTypeByNameService(string query)
        {
            _logger.LogInformation("Starting DocumentTypeByNameService with query: {Query}", query);
            try
            {
                _logger.LogDebug("Fetching document type by name: {Query}", query);
                var result = await _commonRepository.GetDocumnetTypeByName(query);

                if (result != null)
                {
                    _logger.LogInformation("Document type found for query: {Query}, Id: {Id}", query, result.Id);
                }
                else
                {
                    _logger.LogWarning("No document type found for query: {Query}", query);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document type by name: {Query}. Error: {ErrorMessage}", query, ex.Message);
                throw;
            }
        }

        public async Task<Nomenclatures> NomenclatureService(string query)
        {
            _logger.LogInformation("Starting NomenclatureService with query: {Query}", query);
            try
            {
                _logger.LogDebug("Fetching nomenclature by name: {Query}", query);
                var result = await _commonRepository.GetNomenclatureByName(query);

                if (result != null)
                {
                    _logger.LogInformation("Nomenclature found for query: {Query}, Id: {Id}", query, result.Id);
                }
                else
                {
                    _logger.LogWarning("No nomenclature found for query: {Query}", query);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving nomenclature by name: {Query}. Error: {ErrorMessage}", query, ex.Message);
                throw;
            }
        }

        public async Task<User> UserService(int id)
        {
            _logger.LogInformation("Starting UserService with Id: {Id}", id);
            try
            {
                _logger.LogDebug("Fetching user by Id: {Id}", id);
                var result = await _commonRepository.GetUserById(id);

                if (result != null)
                {
                    _logger.LogInformation("User found for Id: {Id}, Username: {Username}", id, result.UserName);
                }
                else
                {
                    _logger.LogWarning("No user found for Id: {Id}", id);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by Id: {Id}. Error: {ErrorMessage}", id, ex.Message);
                throw;
            }
        }

        public async Task<User> UserByNameService(string name)
        {
            _logger.LogInformation("Starting UserByNameService with name: {Name}", name);
            try
            {
                _logger.LogDebug("Fetching user by name: {Name}", name);
                var result = await _commonRepository.GetUserByName(name);

                if (result != null)
                {
                    _logger.LogInformation("User found for name: {Name}, Id: {Id}", name, result.UserId);
                }
                else
                {
                    _logger.LogWarning("No user found for name: {Name}", name);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by name: {Name}. Error: {ErrorMessage}", name, ex.Message);
                throw;
            }
        }

        public async Task<Department> GetDepartmentById(int departmentId)
        {
            _logger.LogInformation("Starting GetDepartmentById with ID: {DepartmentId}", departmentId);
            try
            {
                _logger.LogDebug("Fetching department by Id: {DepartmentId}", departmentId);
                var result = await _commonRepository.GetDepartmentById(departmentId);

                if (result != null)
                {
                    _logger.LogInformation("Department found for Id: {DepartmentId}, Name: {Name}", departmentId, result.Name);
                }
                else
                {
                    _logger.LogWarning("No department found for Id: {DepartmentId}", departmentId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving department by Id: {DepartmentId}. Error: {ErrorMessage}", departmentId, ex.Message);
                throw;
            }
        }

        public async Task<List<Department>> GetAllDepartment()
        {
            _logger.LogInformation("Starting GetAllDepartment");
            try
            {
                _logger.LogDebug("Fetching all departments");
                var result = await _commonRepository.GetAllDepartment();

                _logger.LogInformation("Successfully retrieved {Count} departments", result?.Count ?? 0);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all departments. Error: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<List<UserRole>> GetUserRoles()
        {
            _logger.LogInformation("Starting GetUserRoles");
            try
            {
                _logger.LogDebug("Fetching all user roles");
                var result = await _commonRepository.GetUserRoles();

                _logger.LogInformation("Successfully retrieved {Count} user roles", result?.Count ?? 0);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user roles. Error: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<List<Plant>> GetAllPlants()
        {
            _logger.LogInformation("Starting GetAllPlants");
            try
            {
                _logger.LogDebug("Fetching all plants");
                var result = await _commonRepository.GetAllPlants();

                _logger.LogInformation("Successfully retrieved {Count} plants", result?.Count ?? 0);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving plants. Error: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<List<AssemblyNumbers>> GetAllAssembly()
        {
            _logger.LogInformation("Starting GetAllAssembly");
            try
            {
                _logger.LogDebug("Fetching all assemblies");
                var result = await _commonRepository.GetAllAssembly();

                // Filter out null values
                var filteredResult = result.Where(x => x != null).ToList();

                _logger.LogInformation("Successfully retrieved {Count} assemblies", filteredResult?.Count ?? 0);
                return filteredResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assemblies. Error: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<List<AssemblyDrawingMappingDto>> GetAllAssemblyDrawingMappingsAsync(string? lnItemCode = null)
        {
            _logger.LogInformation("Starting GetAllAssemblyDrawingMappingsAsync, LnItemCode: {LnItemCode}", lnItemCode);
            try
            {
                var result = await _commonRepository.GetAllAssemblyDrawingMappingsAsync(lnItemCode);
                _logger.LogInformation("Successfully retrieved {Count} assembly drawing mappings", result?.Count ?? 0);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assembly drawing mappings. Error: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<int> RemoveChildDrawingAsync(RemoveChildDrawingRequestDto request, int modifiedBy)
        {
            _logger.LogInformation("Starting RemoveChildDrawingAsync, Assembly: {Assembly}, Child: {Child}",
                request.AssemblyDrawingNumber, request.ChildDrawingNumber);
            try
            {
                var removedId = await _commonRepository.RemoveChildDrawingAsync(request, modifiedBy);
                _logger.LogInformation("Successfully removed child drawing. RemovedRecordId: {Id}", removedId);
                return removedId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RemoveChildDrawingAsync. Error: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<int> ReassignParentDrawingAsync(ReassignParentDrawingRequestDto request, int modifiedBy)
        {
            _logger.LogInformation("Starting ReassignParentDrawingAsync, ChildLnItemCode: {ChildLnItemCode}, ParentLnItemCode: {ParentLnItemCode}",
                request.DrawingNumberLnitemcode, request.ParentDrawingNumberLnitemcode);
            try
            {
                var updatedRecordId = await _commonRepository.ReassignParentDrawingAsync(request, modifiedBy);
                _logger.LogInformation("Successfully updated assembly drawing mapping. UpdatedRecordId: {UpdatedRecordId}", updatedRecordId);
                return updatedRecordId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ReassignParentDrawingAsync. Error: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<int> DeleteDrawingNumberAsync(DeleteDrawingNumberRequestDto request, int modifiedBy)
        {
            _logger.LogInformation("Starting DeleteDrawingNumberAsync, Drawing: {Drawing}, LnItemCode: {LnItemCode}",
                request.DrawingNumber, request.LnItemCode);
            try
            {
                var deletedId = await _commonRepository.DeleteDrawingNumberAsync(request, modifiedBy);

                // The delete itself takes effect immediately in the DB, but GetAllDrawingNumberService caches
                // its result under this same key (see InsertDrawingMappingsAsync, which already clears it after
                // its own writes) - without this, a read right after a successful delete kept serving the
                // stale pre-delete list until something else happened to evict the cache, making the delete
                // look like it silently failed on the first call and only "worked" on a later retry.
                _cacheService.Remove(CacheSettings.DrawingNumbersCacheKey);

                _logger.LogInformation("Successfully deleted drawing number. DeletedRecordId: {Id}", deletedId);
                return deletedId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteDrawingNumberAsync. Error: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<int> AddAssemblyDrawingMappingAsync(AddAssemblyDrawingMappingRequestDto request, int createdBy)
        {
            _logger.LogInformation("Starting AddAssemblyDrawingMappingAsync, Drawing: {Drawing}, Parent: {Parent}",
                request.DrawingNumber, request.ParentDrawingNumber);
            try
            {
                var newId = await _commonRepository.AddAssemblyDrawingMappingAsync(request, createdBy);
                _logger.LogInformation("Successfully added assembly drawing mapping. NewId: {NewId}", newId);
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddAssemblyDrawingMappingAsync. Error: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<List<Stage>> GetIRStagesService()
        {
            _logger.LogInformation("Starting GetIRStagesService");
            try
            {
                _logger.LogDebug("Fetching IR stages from repository");
                var result = await _commonRepository.GetStagesByType("IR");

                _logger.LogInformation("Successfully retrieved {Count} IR stages", result?.Count ?? 0);
                return result ?? new List<Stage>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving IR stages. Error: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<List<Stage>> GetMSNStagesService()
        {
            _logger.LogInformation("Starting GetMSNStagesService");
            try
            {
                _logger.LogDebug("Fetching MSN stages from repository");
                var result = await _commonRepository.GetStagesByType("MSN");

                _logger.LogInformation("Successfully retrieved {Count} MSN stages", result?.Count ?? 0);
                return result ?? new List<Stage>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving MSN stages. Error: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<int> AddUserRole(UserRole role)
        {
            _logger.LogInformation("Starting AddUserRole");
            try
            {
                var result = await _commonRepository.AddUserRole(role);
                _logger.LogInformation($"Successfully added user role with ID {result}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user role: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<bool> UpdateUserRole(UserRole role)
        {
            _logger.LogInformation($"Starting UpdateUserRole for ID {role.Id}");
            try
            {
                var result = await _commonRepository.UpdateUserRole(role);
                _logger.LogInformation($"Successfully updated user role: {result}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user role: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteUserRole(int id, int modifiedBy)
        {
            _logger.LogInformation($"Starting DeleteUserRole for ID {id}");
            try
            {
                var result = await _commonRepository.DeleteUserRole(id, modifiedBy);
                _logger.LogInformation($"Successfully deleted user role: {result}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user role: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<List<User>> GetAllUsersService()
        {
            _logger.LogInformation("Starting GetAllUsersService");
            try
            {
                _logger.LogDebug("Fetching all users from repository");
                var result = await _commonRepository.GetAllUsers();

                _logger.LogInformation("Successfully retrieved {Count} users", result?.Count ?? 0);
                return result ?? new List<User>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles. Error: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<List<User>> GetPendingUsersService()
        {
            _logger.LogInformation("Starting GetPendingUsersService");
            try
            {
                var result = await _commonRepository.GetPendingUsersAsync();
                _logger.LogInformation("Successfully retrieved {Count} pending users", result?.Count ?? 0);
                return result ?? new List<User>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending users. Error: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<bool> ApproveUserService(int id, int modifiedBy)
        {
            _logger.LogInformation("Starting ApproveUserService for user ID: {UserId}", id);
            try
            {
                var result = await _commonRepository.ApproveUserAsync(id, modifiedBy);
                _logger.LogInformation("Successfully approved user: {Result}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving user ID: {UserId}. Error: {ErrorMessage}", id, ex.Message);
                throw;
            }
        }

        public async Task<bool> UpdateUserService(UserUpdateDto user)
        {
            _logger.LogInformation("Starting UpdateUserService for user ID: {UserId}", user.Id);
            try
            {
                _logger.LogDebug("Updating user ID: {UserId}", user.Id);
                var result = await _commonRepository.UpdateUser(user);

                _logger.LogInformation("Successfully updated user: {Result}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user ID: {UserId}. Error: {ErrorMessage}", user.Id, ex.Message);
                throw;
            }
        }

        public async Task<bool> UpdateUserStatusAsync(UserStatusUpdateDto request)
        {
            _logger.LogInformation("Starting UpdateUserStatusAsync for user ID: {UserId}", request.Id);
            try
            {
                var result = await _commonRepository.UpdateUserStatusAsync(request);
                _logger.LogInformation("Successfully updated user status: {Result}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user status ID: {UserId}. Error: {ErrorMessage}", request.Id, ex.Message);
                throw;
            }
        }

        public async Task<List<IRDistinctValuesDTO>> GetAllIRNumberDistinctValuesService()
        {
            _logger.LogInformation("Starting GetAllIRNumberDistinctValuesService");

            try
            {
                var result = await _commonRepository.GetAllIRNumberDistinctValues();

                _logger.LogInformation("Successfully retrieved {Count} IR numbers", result?.Count ?? 0);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving IR numbers: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<List<MSNDistinctValues>> GetAllMSNNumberService()
        {
            _logger.LogInformation("Starting GetAllMSNNumberService");

            try
            {
                var result = await _commonRepository.GetAllMSNNumber();

                _logger.LogInformation("Fetched {Count} MSN numbers", result?.Count ?? 0);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllMSNNumberService");
                throw;
            }
        }

        public async Task<int> UpdatePageRoleAccessAsync(List<PageRoleAccessUpdateDto> request)
        {
            try
            {
                _logger.LogInformation("Starting UpdatePageRoleAccessAsync for {Count} records", request.Count);

                if (request == null || !request.Any())
                    throw new ValidationException("Request list cannot be null or empty");

                // Validate each record
                foreach (var item in request)
                {
                    if (item.RoleId <= 0)
                        throw new ValidationException($"Invalid RoleId: {item.RoleId}");

                    if (item.PageId <= 0)
                        throw new ValidationException($"Invalid PageId: {item.PageId}");

                    if (item.FullAccess && item.NoAccess)
                        throw new ValidationException($"FullAccess and NoAccess cannot both be true for RoleId: {item.RoleId}, PageId: {item.PageId}");
                }

                var updatedCount = await _commonRepository.UpdatePageRoleAccessAsync(request);
                _logger.LogInformation("Successfully updated {Count} page role access records", updatedCount);
                return updatedCount;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdatePageRoleAccessAsync");
                throw;
            }
        }

        public async Task<bool> AddDepartmentAsync(AddDepartmentRequestDto request)
        {
            _logger.LogInformation($"Starting AddDepartmentAsync for DepartmentName: {request.DepartmentName}");

            try
            {
                var result = await _commonRepository.AddDepartmentAsync(request);

                if (!result)
                {
                    _logger.LogWarning($"Repository failed to add department: {request.DepartmentName}");
                    return false;
                }

                _logger.LogInformation($"Successfully added department: {request.DepartmentName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in AddDepartmentAsync for DepartmentName: {request.DepartmentName}");
                throw;
            }
        }

        public async Task<bool> AddUnitAsync(AddUnitRequestDto request)
        {
            _logger.LogInformation("Starting AddUnitAsync for UnitName: {UnitName}", request.UnitName);
            try
            {
                if (string.IsNullOrWhiteSpace(request.UnitName))
                {
                    _logger.LogWarning("AddUnitAsync called with empty UnitName");
                    throw new ArgumentException("Unit name cannot be empty.");
                }

                var result = await _commonRepository.AddUnitAsync(request);

                if (!result)
                {
                    _logger.LogWarning("Repository failed to add unit: {UnitName}", request.UnitName);
                    return false;
                }

                
                _cacheService.Remove(CacheSettings.UnitsCacheKey);
                _logger.LogInformation("Units cache cleared after successfully adding unit: {UnitName}", request.UnitName);
                _logger.LogInformation("Successfully added unit: {UnitName}", request.UnitName);
                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddUnitAsync for UnitName: {UnitName}", request.UnitName);
                throw;
            }
        }

        public async Task<bool> AddStageAsync(AddStageRequestDto request)
        {
            _logger.LogInformation("Starting AddStageAsync for StageName: {StageName}", request.StageName);

            try
            {
                if (string.IsNullOrWhiteSpace(request.StageName))
                {
                    _logger.LogWarning("AddStageAsync called with empty StageName");
                    throw new ArgumentException("Stage name cannot be empty.");
                }

                var result = await _commonRepository.AddStageAsync(request);

                if (!result)
                {
                    _logger.LogWarning("Repository failed to add stage: {StageName}", request.StageName);
                    return false;
                }

                _logger.LogInformation("Successfully added stage: {StageName}", request.StageName);
                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddStageAsync for StageName: {StageName}", request.StageName);
                throw;
            }
        }

        public async Task<bool> AddShapeAsync(AddShapeDto request)
        {
            _logger.LogInformation("Starting AddShapeAsync for ShapeName: {ShapeName}", request.ShapeName);

            try
            {
                if (string.IsNullOrWhiteSpace(request.ShapeName))
                {
                    _logger.LogWarning("AddShapeAsync called with empty ShapeName");
                    throw new ArgumentException("Shape name cannot be empty.");
                }

                var result = await _commonRepository.AddShapeAsync(request);

                if (!result)
                {
                    _logger.LogWarning("Repository failed to add shape: {ShapeName}", request.ShapeName);
                    return false;
                }

                _logger.LogInformation("Successfully added shape: {ShapeName}", request.ShapeName);
                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddShapeAsync for ShapeName: {ShapeName}", request.ShapeName);
                throw;
            }
        }

        public async Task<bool> UpdateUnitAsync(UpdateUnitRequestDto request)
        {
            _logger.LogInformation("Starting UpdateUnitAsync: Id={Id}, UnitName={UnitName}", request.Id, request.UnitName);
            try
            {
                if (request.Id <= 0)
                    throw new ArgumentException("Invalid unit ID.");

                if (string.IsNullOrWhiteSpace(request.UnitName))
                    throw new ArgumentException("Unit name cannot be empty.");

                var result = await _commonRepository.UpdateUnitAsync(request);
                if (!result)
                {
                    _logger.LogWarning("Repository failed to update unit: Id={Id}", request.Id);
                    return false;
                }

                _cacheService.Remove(CacheSettings.UnitsCacheKey);
                _logger.LogInformation("Units cache cleared after updating unit: Id={Id}", request.Id);
                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateUnitAsync: Id={Id}", request.Id);
                throw;
            }
        }

        public async Task<bool> UpdateShapeAsync(UpdateShapeRequestDto request)
        {
            _logger.LogInformation("Starting UpdateShapeAsync: Id={Id}, ShapeName={ShapeName}", request.Id, request.ShapeName);
            try
            {
                if (request.Id <= 0)
                    throw new ArgumentException("Invalid shape ID.");

                if (string.IsNullOrWhiteSpace(request.ShapeName))
                    throw new ArgumentException("Shape name cannot be empty.");

                var result = await _commonRepository.UpdateShapeAsync(request);
                if (!result)
                {
                    _logger.LogWarning("Repository failed to update shape: Id={Id}", request.Id);
                    return false;
                }

                _cacheService.Remove(CacheSettings.ShapesCacheKey);
                _logger.LogInformation("Shapes cache cleared after updating shape: Id={Id}", request.Id);
                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateShapeAsync: Id={Id}", request.Id);
                throw;
            }
        }

        public async Task<bool> UpdateStageAsync(UpdateStageRequestDto request)
        {
            _logger.LogInformation("Starting UpdateStageAsync: Id={Id}, StageName={StageName}", request.Id, request.StageName);
            try
            {
                if (request.Id <= 0)
                    throw new ArgumentException("Invalid stage ID.");

                if (string.IsNullOrWhiteSpace(request.StageName))
                    throw new ArgumentException("Stage name cannot be empty.");

                var result = await _commonRepository.UpdateStageAsync(request);
                if (!result)
                {
                    _logger.LogWarning("Repository failed to update stage: Id={Id}", request.Id);
                    return false;
                }

                _logger.LogInformation("Stage updated successfully: Id={Id}", request.Id);
                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateStageAsync: Id={Id}", request.Id);
                throw;
            }
        }

        public async Task<bool> DeleteUnitAsync(int id, int modifiedBy)
        {
            _logger.LogInformation("Starting DeleteUnitAsync: Id={Id}", id);
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid unit ID.");

                var result = await _commonRepository.DeleteUnitAsync(id, modifiedBy);
                if (!result)
                {
                    _logger.LogWarning("Unit not found or already deleted: Id={Id}", id);
                    return false;
                }

                _cacheService.Remove(CacheSettings.UnitsCacheKey);
                _logger.LogInformation("Units cache cleared after deleting unit: Id={Id}", id);
                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteUnitAsync: Id={Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteShapeAsync(int id, int modifiedBy)
        {
            _logger.LogInformation("Starting DeleteShapeAsync: Id={Id}", id);
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid shape ID.");

                var result = await _commonRepository.DeleteShapeAsync(id, modifiedBy);
                if (!result)
                {
                    _logger.LogWarning("Shape not found or already deleted: Id={Id}", id);
                    return false;
                }

                _cacheService.Remove(CacheSettings.ShapesCacheKey);
                _logger.LogInformation("Shapes cache cleared after deleting shape: Id={Id}", id);
                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteShapeAsync: Id={Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteStageAsync(int id, int modifiedBy)
        {
            _logger.LogInformation("Starting DeleteStageAsync: Id={Id}", id);
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid stage ID.");

                var result = await _commonRepository.DeleteStageAsync(id, modifiedBy);
                if (!result)
                {
                    _logger.LogWarning("Stage not found or already deleted: Id={Id}", id);
                    return false;
                }

                _logger.LogInformation("Stage deleted successfully: Id={Id}", id);
                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteStageAsync: Id={Id}", id);
                throw;
            }
        }

        public async Task<bool> UpdateDepartmentAsync(UpdateDepartmentRequestDto request)
        {
            _logger.LogInformation("Starting UpdateDepartmentAsync: Id={Id}, DepartmentName={DepartmentName}", request.Id, request.DepartmentName);
            try
            {
                if (request.Id <= 0)
                    throw new ArgumentException("Invalid department ID.");

                if (string.IsNullOrWhiteSpace(request.DepartmentName))
                    throw new ArgumentException("Department name cannot be empty.");

                var result = await _commonRepository.UpdateDepartmentAsync(request);
                if (!result)
                {
                    _logger.LogWarning("Department not found or failed to update: Id={Id}", request.Id);
                    return false;
                }

                _logger.LogInformation("Department updated successfully: Id={Id}", request.Id);
                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateDepartmentAsync: Id={Id}", request.Id);
                throw;
            }
        }

        public async Task<bool> DeleteDepartmentAsync(int id, int modifiedBy)
        {
            _logger.LogInformation("Starting DeleteDepartmentAsync: Id={Id}", id);
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid department ID.");

                var result = await _commonRepository.DeleteDepartmentAsync(id, modifiedBy);
                if (!result)
                {
                    _logger.LogWarning("Department not found or already deleted: Id={Id}", id);
                    return false;
                }

                _logger.LogInformation("Department deleted successfully: Id={Id}", id);
                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteDepartmentAsync: Id={Id}", id);
                throw;
            }
        }


        public async Task<bool> AddProdSeriesAsync(AddProdSeriesRequestDto request)
        {
            _logger.LogInformation("Starting AddProdSeriesAsync for ProductionSeries: {ProductionSeries}", request.ProductionSeries);
            try
            {
                if (string.IsNullOrWhiteSpace(request.ProductionSeries))
                {
                    _logger.LogWarning("AddProdSeriesAsync called with empty ProductionSeries");
                    throw new ArgumentException("Production series name cannot be empty.");
                }
                var result = await _commonRepository.AddProdSeriesAsync(request);
                if (!result)
                {
                    _logger.LogWarning("Repository failed to add production series: {ProductionSeries}", request.ProductionSeries);
                    return false;
                }
                _logger.LogInformation("Successfully added production series: {ProductionSeries}", request.ProductionSeries);
                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddProdSeriesAsync for ProductionSeries: {ProductionSeries}", request.ProductionSeries);
                throw;
            }
        }

        public async Task<bool> UpdateProdSeriesAsync(UpdateProdSeriesRequestDto request)
        {
            _logger.LogInformation("Starting UpdateProdSeriesAsync for Id: {Id}", request.Id);
            try
            {
                if (request.Id <= 0)
                    throw new ArgumentException("Invalid production series Id.");
                if (string.IsNullOrWhiteSpace(request.ProductionSeries))
                    throw new ArgumentException("Production series name cannot be empty.");
                var result = await _commonRepository.UpdateProdSeriesAsync(request);
                if (!result)
                {
                    _logger.LogWarning("Repository failed to update production series Id: {Id}", request.Id);
                    return false;
                }
                _logger.LogInformation("Successfully updated production series Id: {Id}", request.Id);
                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateProdSeriesAsync for Id: {Id}", request.Id);
                throw;
            }
        }

        public async Task<bool> DeleteProdSeriesAsync(int id, int deletedBy)
        {
            _logger.LogInformation("Starting DeleteProdSeriesAsync for Id: {Id}", id);
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid production series Id.");
                var result = await _commonRepository.DeleteProdSeriesAsync(id, deletedBy);
                if (!result)
                {
                    _logger.LogWarning("Repository failed to delete production series Id: {Id}", id);
                    return false;
                }
                _logger.LogInformation("Successfully deleted production series Id: {Id}", id);
                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteProdSeriesAsync for Id: {Id}", id);
                throw;
            }
        }
        public async Task<bool> UploadUserSignatureAsync(UploadSignatureRequestDto request)
        {
            _logger.LogInformation("Starting UploadUserSignatureAsync for UserId: {UserId}", request.UserId);
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request), "Request cannot be null.");

                if (request.UserId <= 0)
                    throw new ArgumentException("Invalid UserId provided.");

                if (string.IsNullOrWhiteSpace(request.Signature))
                    throw new ArgumentException("Signature cannot be null or empty.");

                var result = await _commonRepository.UploadUserSignatureAsync(request);
                if (!result)
                {
                    _logger.LogWarning("Failed to insert signature for UserId: {UserId}", request.UserId);
                    return false;
                }

                _logger.LogInformation("Signature uploaded successfully for UserId: {UserId}", request.UserId);
                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UploadUserSignatureAsync for UserId: {UserId}", request?.UserId);
                throw;
            }
        }

        public async Task<string?> GetUserSignatureAsync(int userId)
        {
            _logger.LogInformation("Starting GetUserSignatureAsync for UserId: {UserId}", userId);
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid UserId provided.");

                var signature = await _commonRepository.GetUserSignatureAsync(userId);

                _logger.LogInformation("GetUserSignatureAsync completed for UserId: {UserId} — signature {Status}",
                    userId, signature != null ? "found" : "not found");

                return signature; // null if no signature exists
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserSignatureAsync for UserId: {UserId}", userId);
                throw;
            }
        }
        public async Task<IEnumerable<Godrej.Precheck.Models.DTOs.User.UserSignatureListDto>> GetUsersWithSignaturesAsync()
        {
            _logger.LogInformation("Starting GetUsersWithSignaturesAsync");
            try
            {
                var result = await _commonRepository.GetUsersWithSignaturesAsync();
                _logger.LogInformation("GetUsersWithSignaturesAsync completed. Retrieved {Count} records.",
                    result?.Count() ?? 0);
                return result ?? Enumerable.Empty<Godrej.Precheck.Models.DTOs.User.UserSignatureListDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUsersWithSignaturesAsync");
                throw;
            }
        }
    }
}
