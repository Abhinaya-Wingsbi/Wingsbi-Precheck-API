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
using Godrej.Precheck.Repository.Database;
using Godrej.Precheck.Repository.Queries;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Godrej.Precheck.Repository.Repository.CommonRepository
{
    public class CommonRepository : ICommonRepository
    {
        private readonly ILogger<CommonRepository> _logger;

        private readonly ApplicationDbContext _db;


        public CommonRepository(ILogger<CommonRepository> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<List<PrecheckModule?>> GetAllPrecheckModule()
        {
            _logger.LogInformation($"Request for CommonRepository:GetAllPrecheckModule");
            var results = await _db.GetAll<PrecheckModule?>(
                Common.GET_PRECHECK_MODULE_QUERY,
            new { });
            _logger.LogInformation($"Result for CommonRepository:GetAllPrecheckModule : ");
            return results.ToList();
        }

        public async Task<AddUserResponseDto?> AddUserAsync(AddUserRequestDto request, int createdBy)
        {
            _logger.LogInformation("Request for CommonRepository:AddUserAsync for user: {UserName}", request.UserName);
            try
            {
                var indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

                // Step 1 — Check if user already exists
                var existingUser = await _db.GetSingle<int>(
                    Common.CHECK_USER_EXISTS_QUERY,
                    new { UserId = request.UserId, UserName = request.UserName });

                if (existingUser > 0)
                {
                    _logger.LogWarning("User already exists: {UserName}", request.UserName);
                    throw new ApplicationException($"User '{request.UserName}' already exists.");
                }

                // Step 2 — Insert user and get new ID
                var newUserId = await _db.ExecuteScalar<int>(   
                    Common.ADD_USER_QUERY,
                    new
                    {
                        UserId = request.UserId,
                        UserName = request.UserName,
                        RoleId = request.RoleId,
                        SecurityStamp = request.SecurityStamp,
                        DepartmentId=request.DepartmentId,
                        Password = request.Password,    
                        CreatedBy = createdBy,
                        CreatedDate = indianTime
                    });

                if (newUserId <= 0)
                {
                    _logger.LogWarning("Failed to insert user: {UserName}", request.UserName);
                    return null;
                }

                // Step 3 — Fetch and return inserted user
                var result = await _db.GetSingle<AddUserResponseDto>(
                    Common.GET_USER_BY_ID_QUERY,
                    new { Id = newUserId });

                _logger.LogInformation("Successfully added user: {UserName}, ID: {Id}", request.UserName, newUserId);
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
        public async Task<List<SecurityQuestionsInfo?>> GetAllSecurityQuestionModule()
        {
            _logger.LogInformation($"Request for CommonRepository:GetAllPrecheckModule");
            var results = await _db.GetAll<SecurityQuestionsInfo?>(
                Common.GET_SECURITY_QUESTION_MODULE_QUERY,
            new { });
            _logger.LogInformation($"Result for CommonRepository:GetAllPrecheckModule : ");
            return results.ToList();
        }

        public async Task<List<ComponentsType?>> GetAllComponentType()
        {
            _logger.LogInformation($"Request for CommonRepository:GetAllComponentType");
            var results = await _db.GetAll<ComponentsType?>(
            Common.GET_COMPONENT_TYPE_QUERY,
            new { });
            _logger.LogInformation($"Result for CommonRepository:GetAllComponentType");
            return results.ToList();
        }

        public async Task<ComponentsType?> GetComponentTypeByNameAsync(string query)
        {
            _logger.LogInformation($"Request for CommonRepository:GetComponentTypeByNameAsync {query}");
            var result = await _db.GetSingle<ComponentsType?>(
            Common.GET_COMPONENT_TYPE_BY_NAME_QUERY,
            new { query= query });
            _logger.LogInformation($"Result for CommonRepository:GetComponentTypeByNameAsync {result}");
            return result;
        }

        public async Task<ComponentsType?> GetComponentTypeByIdAsync(int Id)
        {
            _logger.LogInformation($"Request for CommonRepository:GetComponentTypeByIdAsync");
            var result = await _db.GetSingle<ComponentsType?>(
            Common.GET_COMPONENT_TYPE_BY_Id_QUERY,
            new { Id = Id });
            _logger.LogInformation($"Result for CommonRepository:GetComponentTypeByIdAsync");
            return result;
        }
        public async Task<List<ProductionSeriess?>> GetAllProductionSeries()
        {
            _logger.LogInformation($"Request for CommonRepository:GetAllProductionSeries");
            var results = await _db.GetAll<ProductionSeriess?>(
            Common.GET_Production_Series_Query,
            new { });
            _logger.LogInformation($"Result for CommonRepository:GetAllProductionSeries");
            return results.ToList();
        }

        //GetProductionSeriesByName 
        public async Task<ProductionSeriess?> GetProductionSeriesByName(string query)
        {
            _logger.LogInformation($"Request for CommonRepository:GetProductionSeriesByName");
            var results = await _db.GetSingle<ProductionSeriess?>(
            Common.GET_ProductionSeriesByName_Query,
            new {query= query});
            _logger.LogInformation($"Result for CommonRepository:GetProductionSeriesByName");
            return results;
        }

        public async Task<ProductionSeriess?> GetProductionSeriesById(int Id)
        {
            _logger.LogInformation($"Request for CommonRepository:GetProductionSeriesById");
            var results = await _db.GetSingle<ProductionSeriess?>(
            Common.GET_ProductionSeriesById_Query,
            new { Id = Id });
            _logger.LogInformation($"Result for CommonRepository:GetProductionSeriesById");
            return results;
        }

        public async Task<List<IRNumbers>> GetIRnumber(GetAllIRNumberRequestDto getAllIRNumberRequestDto)
        {
            _logger.LogInformation($"Request for CommonRepository:GetAllIRnumber");
            var results = await _db.GetAll<IRNumbers>(
            Common.GET_IRNUMBER_Query,
            new { query= getAllIRNumberRequestDto.query , departmentid = getAllIRNumberRequestDto.departmentId  });
            _logger.LogInformation($"Result for CommonRepository:GetAllIRnumber");
            return results.ToList() ;
        }

        public async Task<IRNumbers> GetSingleIRnumber(string irNumber)
        {
            _logger.LogInformation($"Request for CommonRepository:GetAllIRnumber");
            var results = await _db.GetSingle<IRNumbers>(
            Common.GET_SINGLE_IRNUMBER_Query,
            new { query = irNumber });
            _logger.LogInformation($"Result for CommonRepository:GetAllIRnumber");
            return results;
        }
        public async Task<List<IRNumbers>> GetIRNumberByDrawingNumber(GetIRNumberByDrawingNumberRequest getIRNumberByDrawingNumberRequest)
        {
            _logger.LogInformation($"Request for CommonRepository:GetIRnumberByDrawingNumber - DrawingNumber: {getIRNumberByDrawingNumberRequest.DrawingNumber}, ProductionSeries: {getIRNumberByDrawingNumberRequest.Productionseries}, DepartmentID: {getIRNumberByDrawingNumberRequest.DepartmentTypeId}, Stage: {getIRNumberByDrawingNumberRequest.Stage}");
            
            var results = await _db.GetAll<IRNumbers>(
            Common.GET_IRNUMBERByDrawing_Query,
            new { query = getIRNumberByDrawingNumberRequest.DrawingNumber,
                 productionseries = getIRNumberByDrawingNumberRequest.Productionseries,
                DepartmentID = getIRNumberByDrawingNumberRequest.DepartmentTypeId,
                Stage = getIRNumberByDrawingNumberRequest.Stage,
                LnItemCode = getIRNumberByDrawingNumberRequest.LnItemCode,
                FromDate = getIRNumberByDrawingNumberRequest.FromDate,
                ToDate = getIRNumberByDrawingNumberRequest.ToDate,
                IRNumberId= getIRNumberByDrawingNumberRequest.IRNumeberId
            });
            
            _logger.LogInformation($"Result for CommonRepository:GetIRnumberByDrawingNumber - Count: {results.Count()}");
            return results.ToList();
        }

        public async Task<List<MSNNumbers>> GetMSNNuberByDrawingNumber(GetMSNNumberByDrawingNumberRequest getMSNNumberByDrawingNumberRequest)
        {
            _logger.LogInformation($"Request for CommonRepository:GetMSNNuberByDrawingNumber");
            var results = await _db.GetAll<MSNNumbers>(
            Common.GET_MSNNUMBERByDrawing_Query,
            new { query = getMSNNumberByDrawingNumberRequest.DrawingNumber,
                productionseries = getMSNNumberByDrawingNumberRequest.Productionseries,
                DepartmentID = getMSNNumberByDrawingNumberRequest.DepartmentTypeId,
                Stage = getMSNNumberByDrawingNumberRequest.Stage,
                LnItemCode = getMSNNumberByDrawingNumberRequest.LnItemCode,
                FromDate = getMSNNumberByDrawingNumberRequest.FromDate,
                ToDate = getMSNNumberByDrawingNumberRequest.ToDate,
                MSNNumberId=getMSNNumberByDrawingNumberRequest.MSNNumberId,
            });
            _logger.LogInformation($"Result for CommonRepository:GetMSNNuberByDrawingNumber");
            return results.ToList();
        }

        public async Task<List<MSNNumbers>> GetMSNNuber(GetAllMSNNumberRequestDto getAllMSNNumberRequestDto)
        {
            _logger.LogInformation($"Request for CommonRepository:GetAllIRnumber");
            var results = await _db.GetAll<MSNNumbers>(
            Common.GET_MSNNUMBER_Query,
            new { query = getAllMSNNumberRequestDto.query , departmentid = getAllMSNNumberRequestDto.departmentId });
            _logger.LogInformation($"Result for CommonRepository:GetAllIRnumber");
            return results.ToList();
        }

        public async Task<MSNNumbers> GetSingleMSNNuber(string msnNumber)
        {
            _logger.LogInformation($"Request for CommonRepository:GetSingleMSNNuber");
            var results = await _db.GetSingle<MSNNumbers>(
            Common.GET_SINGLE_MSNNUMBER_Query,
            new { query = msnNumber });
            _logger.LogInformation($"Result for CommonRepository:GetAllIRnumber");
            return results;
        }


        public async Task<List<DrawingNumbers?>> GetAllDrawingNumber()
        {
            _logger.LogInformation($"Request for CommonRepository:GetDrawingNumber");
            var results = await _db.GetAll<DrawingNumbers?>(
            Common.GET_DrawingNumber_Query,
            new {  });
            _logger.LogInformation($"Result for CommonRepository:GetDrawingNumber");
            return results.ToList();
        }

        //Get Drawing number by ID
        public async Task<DrawingNumbers> GetDrawingNumberById(int drawingId)
        {
            _logger.LogInformation($"Request for CommonRepository:GetDrawingNumber");
            var result = await _db.GetSingle<DrawingNumbers>(
            Common.GET_DrawingNumberById_Query,
            new { Id = drawingId });
            _logger.LogInformation($"Result for CommonRepository:GetDrawingNumber");
            return result;
        }


        public async Task<List<DocumentTypes?>> GetAllDocumnetType()
        {
            _logger.LogInformation($"Request for CommonRepository:GetAllDocumnetType");
            var results = await _db.GetAll<DocumentTypes?>(
                Common.GET_DocumentType_QUERY,
            new { });
            _logger.LogInformation($"Result for CommonRepository:GetAllDocumnetType : ");
            return results.ToList();
        }

        public async Task<List<DocumentTypes?>> GetDocumnetTypeB()
        {
            _logger.LogInformation($"Request for CommonRepository:GetAllDocumnetType");
            var results = await _db.GetAll<DocumentTypes?>(
                Common.GET_DocumentType_QUERY,
            new { });
            _logger.LogInformation($"Result for CommonRepository:GetAllDocumnetType : ");
            return results.ToList();
        }

        public async Task<DocumentTypes?> GetDocumnetTypeByName(string query)
        {
            _logger.LogInformation($"Request for CommonRepository:GetDocumnetTypeByName");
            var results = await _db.GetSingle<DocumentTypes?>(
                Common.GET_DocumentTypeByName_QUERY,
            new { query = query });
            _logger.LogInformation($"Result for CommonRepository:GetDocumnetTypeByName : ");
            return results;
        }

        public async Task<ProductionOrderModel> GetProductionOrderByName(string ProductionOrder)
        {
            _logger.LogInformation($"Request for CommonRepository:GetProductionOrderByName");
            var results = await _db.GetSingle<ProductionOrderModel>(
                Common.GET_ProductionOrderByName_QUERY,
            new { ProductionOrder = ProductionOrder });
            _logger.LogInformation($"Result for CommonRepository:GetProductionOrderByName ");
            return results;
        }

        public async Task<List<UnitModel>> GetUnitByName()
        {
            _logger.LogInformation($"Request for CommonRepository:GetUnitByName");
            var results = await _db.GetAll<UnitModel>(
                Common.GET_UnitByName_QUERY,
            new {});
            _logger.LogInformation($"Result for CommonRepository:GetUnitByName  {results}");
            return results.ToList();
        }

        public async Task<List<ShapeModel>> GetAllShapes()
        {
            _logger.LogInformation($"Request for CommonRepository:GetAllShapes");
            var results = await _db.GetAll<ShapeModel>(
                Common.GET_All_Shapes_QUERY,
            new { });
            _logger.LogInformation($"Result for CommonRepository:GetAllShapes  {results}");
            return results.ToList();
        }

        public async Task<AssemblyNumbers?> GetAssemblyById(int assemblyId)
        {
            _logger.LogInformation($"Request for CommonRepository:GetDocumnetTypeByName");
            var results = await _db.GetSingle<AssemblyNumbers?>(
                Common.GET_AssemblyBy_Id_Query,
            new { assemblyId = assemblyId });
            _logger.LogInformation($"Result for CommonRepository:GetDocumnetTypeByName :{results}");
            return results;
        }

        public async Task<List<AssemblyNumbers?>> GetAllAssembly()
        {
            _logger.LogInformation($"Request for CommonRepository:GetAllAssembly");
            var results = await _db.GetAll<AssemblyNumbers?>(
                Common.GET_ALL_Assembly_Query,
            new { });
            _logger.LogInformation($"Result for CommonRepository:GetAllAssembly :{results}");
            return results.ToList();
        }

        public async Task<List<AssemblyDrawingMappingDto>> GetAllAssemblyDrawingMappingsAsync(string? lnItemCode = null)
        {
            _logger.LogInformation("Request for CommonRepository:GetAllAssemblyDrawingMappingsAsync, LnItemCode: {LnItemCode}", lnItemCode);
            var results = await _db.GetAll<AssemblyDrawingMappingDto>(
                Common.GET_ASSEMBLY_DRAWING_MAPPING_QUERY,
                new { SearchQuery = lnItemCode });
            _logger.LogInformation("Result for CommonRepository:GetAllAssemblyDrawingMappingsAsync");
            return results.ToList();
        }

        public async Task<int> RemoveChildDrawingAsync(RemoveChildDrawingRequestDto request, int modifiedBy)
        {
            _logger.LogInformation("Request for CommonRepository:RemoveChildDrawingAsync, Assembly: {Assembly}, Child: {Child}",
                request.AssemblyDrawingNumber, request.ChildDrawingNumber);

            var removedId = await _db.ExecuteScalar<int>(
                Common.REMOVE_CHILD_DRAWING_QUERY,
                new
                {
                    AssemblyDrawingNumber = request.AssemblyDrawingNumber,
                    AssemblyLnItemCode    = request.AssemblyLnItemCode,
                    ChildDrawingNumber    = request.ChildDrawingNumber,
                    ChildLnItemCode       = request.ChildLnItemCode,
                    ModifiedBy            = modifiedBy
                });

            _logger.LogInformation("Result for CommonRepository:RemoveChildDrawingAsync, RemovedRecordId: {Id}", removedId);
            return removedId;
        }

        public async Task<int> ReassignParentDrawingAsync(ReassignParentDrawingRequestDto request, int modifiedBy)
        {
            _logger.LogInformation("Request for CommonRepository:ReassignParentDrawingAsync, ChildLnItemCode: {ChildLnItemCode}, ParentLnItemCode: {ParentLnItemCode}",
                request.DrawingNumberLnitemcode, request.ParentDrawingNumberLnitemcode);

            var updatedRecordId = await _db.ExecuteScalar<int>(
                Common.REASSIGN_PARENT_DRAWING_QUERY,
                new
                {
                    ChildLnItemCode  = request.DrawingNumberLnitemcode,
                    ParentLnItemCode = request.ParentDrawingNumberLnitemcode,
                    FindNo           = request.FindNo,
                    Quantity         = request.Quantity,
                    ModifiedBy       = modifiedBy
                });

            _logger.LogInformation("Result for CommonRepository:ReassignParentDrawingAsync, UpdatedRecordId: {UpdatedRecordId}", updatedRecordId);
            return updatedRecordId;
        }

        public async Task<int> AddAssemblyDrawingMappingAsync(AddAssemblyDrawingMappingRequestDto request, int createdBy)
        {
            _logger.LogInformation("Request for CommonRepository:AddAssemblyDrawingMappingAsync, Drawing: {Drawing}, Parent: {Parent}",
                request.DrawingNumber, request.ParentDrawingNumber);

            var newId = await _db.ExecuteScalar<int>(
                Common.ADD_ASSEMBLY_DRAWING_MAPPING_QUERY,
                new
                {
                    DrawingNumber       = request.DrawingNumber,
                    ParentDrawingNumber = request.ParentDrawingNumber,
                    AssemblyLnItemCode  = request.AssemblyLnItemCode,
                    ChildLnItemCode     = request.ChildLnItemCode,
                    FindNo              = request.FindNo,
                    ConsumedProdSeriesId = request.ConsumedProdSeriesId,
                    Quantity            = request.Quantity,
                    CreatedBy           = createdBy,
                    Unit=request.Unit,
                    Nomenclature=request.Nomenclature
                });

            _logger.LogInformation("Result for CommonRepository:AddAssemblyDrawingMappingAsync, NewId: {NewId}", newId);
            return newId;
        }

        public async Task<int> DeleteDrawingNumberAsync(DeleteDrawingNumberRequestDto request, int modifiedBy)
        {
            _logger.LogInformation("Request for CommonRepository:DeleteDrawingNumberAsync, Drawing: {Drawing}, LnItemCode: {LnItemCode}",
                request.DrawingNumber, request.LnItemCode);

            var deletedId = await _db.ExecuteScalar<int>(
                Common.DELETE_DRAWING_NUMBER_QUERY,
                new
                {
                    DrawingNumber = request.DrawingNumber,
                    LnItemCode    = request.LnItemCode,
                    ModifiedBy    = modifiedBy
                });

            _logger.LogInformation("Result for CommonRepository:DeleteDrawingNumberAsync, DeletedRecordId: {Id}", deletedId);
            return deletedId;
        }

        public async Task<Nomenclatures?> GetNomenclatureByName(string query)
        {
            _logger.LogInformation($"Request for CommonRepository:GetNomenclatureByName");
            var results = await _db.GetSingle<Nomenclatures?>(
                Common.GET_Nomenclature_QUERY,
            new { query = query });
            _logger.LogInformation($"Result for CommonRepository:GetNomenclatureByName :{results}");
            return results;
        }

        public async Task<User?> GetUserById(int UserId)
        {
            _logger.LogInformation($"Request for CommonRepository:GetUserById");
            var results = await _db.GetSingle<User?>(
                Common.GET_User_QUERY,
            new { UserId = UserId });
            _logger.LogInformation($"Result for CommonRepository:GetUserById :  {results}");
            return results;
        }

        public async Task<User?> GetUserByName(string name)
        {
            _logger.LogInformation($"Request for CommonRepository:GetUserById");
            var results = await _db.GetSingle<User?>(
                Common.GET_UserByName,
            new { Name = name });
            _logger.LogInformation($"Result for CommonRepository:GetUserById :  {results}");
            return results;
        }

        public async Task<Department> GetDepartmentById(int departmentId)
        {
            _logger.LogInformation($"Request for CommonRepository:GetDepartMentById");
            var results = await _db.GetSingle<Department?>(
                Common.GET_DEPARTMENT_BY_ID,
            new { DepartmentId = departmentId });
            _logger.LogInformation($"Result for CommonRepository: :GetDepartMentById {results}");
            return results;
        }

        public async Task<int> GetLastSequenceNoIrNumberTable()
        {
            _logger.LogInformation($"Request for CommonRepository:GetLastSequenceNoIrNumberTable");
            var results = await _db.GetSingle<int?>(
                Common.GET_LAST_SEQUENCE_IRNUMBER, 0);
            _logger.LogInformation($"Result for CommonRepository: :GetLastSequenceNoIrNumberTable{results}");

            return results.Value;
        }

        public async Task<int> GetLastSequenceNoMSNNumberTable()
        {
            _logger.LogInformation($"Request for CommonRepository:GetLastSequenceNoMSNNumberTable");
            var results = await _db.GetSingle<int?>(
                Common.GET_LAST_SEQUENCE_MSNNUMBER, 0);
            _logger.LogInformation($"Result for CommonRepository: :GetLastSequenceNoMSNNumberTable {results}");

            return results.Value;
        }

        public async Task<List<Department>> GetAllDepartment()
        {
            _logger.LogInformation($"Request for CommonRepository:GetLastSequenceNoMSNNumberTable");
            var results = await _db.GetAll<Department?>(
                Common.GET_ALL_DEPARTMENT, 0);
            _logger.LogInformation($"Result for CommonRepository: :GetLastSequenceNoMSNNumberTable{results}");

            return results.ToList();
        }

        public async Task<List<UserRole>> GetUserRoles()
        {
            _logger.LogInformation($"Request for CommonRepository:GetLastSequenceNoMSNNumberTable");
            var results = await _db.GetAll<UserRole?>(
                Common.GET_ALL_USERROLES, 0);
            _logger.LogInformation($"Result for CommonRepository: :GetLastSequenceNoMSNNumberTable: {results}");

            return results.ToList();
        }

        public async Task<List<Plant>> GetAllPlants()
        {
            _logger.LogInformation($"Request for CommonRepository:GetLastSequenceNoMSNNumberTable");
            var results = await _db.GetAll<Plant?>(
                Common.GET_ALL_PLANTS, 0);
            _logger.LogInformation($"Result for CommonRepository: :GetLastSequenceNoMSNNumberTable{results}");

            return results.ToList();
        }

        public async Task<List<Stage>> GetStagesByType(string stageType)
        {
            _logger.LogInformation($"Request for CommonRepository:GetStagesByType with stageType: {stageType}");
            var results = await _db.GetAll<Stage>(
                Common.GET_Stage_ByType_QUERY,
                new { stageType = stageType });
            _logger.LogInformation($"Result for CommonRepository:GetStagesByType: Retrieved {results?.Count() ?? 0} stages");
            return results.ToList();
        }

        public async Task<Stage> GetStageById(int stageId)
        {
            _logger.LogInformation($"Request for CommonRepository:GetStageById with stageId: {stageId}");
            var result = await _db.GetSingle<Stage>(
                Common.GET_Stage_ById_QUERY,
                new { stageId = stageId });
            _logger.LogInformation($"Result for CommonRepository:GetStageById: {(result != null ? "Found" : "Not found")}");
            return result;
        }

        public async Task<List<string>> GetAllLnItemCode(string search = null)
        {
            _logger.LogInformation($"Request for CommonRepository:GetAllLnItemCode with search: '{search}'");
            
            var results = await _db.GetAll<string>(Common.GET_ALL_LNITEMCODE, new { search = search ?? "" });
            
            _logger.LogInformation($"Result for CommonRepository:GetAllLnItemCode: {results.Count()} items");
            return results.ToList();
        }
   
        public async Task<int> AddUserRole(UserRole role)
        {
            _logger.LogInformation("Request for CommonRepository:AddUserRole");
            var roleId = await _db.ExecuteScalar<int>(
                Common.INSERT_USER_ROLE_QUERY,
                new
                {
                    Role = role.Role,
                    Description = role.Description,
                    CreatedBy = role.CreatedBy ?? 1 // Fallback if CreatedBy is null, though it shouldn't be
                });
            var newUserPermissions = await _db.ExecuteScalar<int>(
                Common.ADD_NEW_ROLE_PERMISSIONS,
                new
                {
                    RoleId = roleId,
                    CreatedBy = role.CreatedBy,
                });
            _logger.LogInformation($"Result for CommonRepository:AddUserRole: ID {roleId}");
            return roleId;
        }

        public async Task<bool> UpdateUserRole(UserRole role)
        {
            _logger.LogInformation($"Request for CommonRepository:UpdateUserRole for ID {role.Id}");
            var rowsAffected = await _db.Execute(
                Common.UPDATE_USER_ROLE_QUERY,
                new
                {
                    Id = role.Id,
                    Role = role.Role,
                    Description = role.Description,
                    ModifiedBy = role.ModifiedBy ?? 1,
                    IsActive = role.IsActive
                });
            _logger.LogInformation($"Result for CommonRepository:UpdateUserRole: {rowsAffected > 0}");
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteUserRole(int id, int modifiedBy)
        {
            _logger.LogInformation($"Request for CommonRepository:DeleteUserRole for ID {id}");
            var rowsAffected = await _db.Execute(
                Common.DELETE_USER_ROLE_QUERY,
                new
                {
                    Id = id,
                    ModifiedBy = modifiedBy
                });
            _logger.LogInformation($"Result for CommonRepository:DeleteUserRole: {rowsAffected > 0}");
            return rowsAffected > 0;
        }

        public async Task<List<User>> GetAllUsers()
        {
            _logger.LogInformation("Request for CommonRepository:GetAllUsers");
            var results = await _db.GetAll<User>(
                Common.GET_ALL_USERS_QUERY,
                new { });
            _logger.LogInformation($"Result for CommonRepository:GetAllUsers: Retrieved {results.Count()} users");
            return results.ToList();
        }

        public async Task<List<User>> GetPendingUsersAsync()
        {
            _logger.LogInformation("Request for CommonRepository:GetPendingUsersAsync");
            var result = await _db.GetAll<User>(
                Users.GET_PENDING_USERS_QUERY, new { }
            );
            
            var userList = result?.ToList() ?? new List<User>();
            _logger.LogInformation($"Result for CommonRepository:GetPendingUsersAsync: {userList.Count} users found");
            return userList;
        }

        public async Task<bool> ApproveUserAsync(int id, int modifiedBy)
        {
            _logger.LogInformation($"Request for CommonRepository:ApproveUserAsync for ID {id}");
            
            var rowsAffected = await _db.Execute(
                Users.APPROVE_USER_QUERY,
                new
                {
                    Id = id,
                    ModifiedBy = modifiedBy
                });
            _logger.LogInformation($"Result for CommonRepository:ApproveUserAsync: {rowsAffected > 0}");
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateUser(UserUpdateDto user)
        {
            _logger.LogInformation($"Request for CommonRepository:UpdateUser for ID {user.Id}");
            var indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);
            
            var rowsAffected = await _db.Execute(
                Common.UPDATE_USER_QUERY,
                new
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    DepartmentId = user.DepartmentId,
                    UserRoleId = user.UserRoleId,
                    PlantId=user.PlantId,
                    SecurityQuestionId = user.SecurityQuestionId,
                    SecurityAnswer = user.SecurityAnswer,
                    ModifiedBy = user.ModifiedBy,
                    ModifiedDate = indianTime
                });
            _logger.LogInformation($"Result for CommonRepository:UpdateUser: {rowsAffected > 0}");
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateUserStatusAsync(UserStatusUpdateDto request)
        {
            _logger.LogInformation($"Request for CommonRepository:UpdateUserStatusAsync for ID {request.Id}");
            
            var rowsAffected = await _db.Execute(
                Common.UPDATE_USER_STATUS_QUERY,
                new
                {
                    Id = request.Id,
                    IsActive = request.IsActive ? 1 : 0,
                    ModifiedBy = request.ModifiedBy,
                });
            _logger.LogInformation($"Result for CommonRepository:UpdateUserStatusAsync: {rowsAffected > 0}");
            return rowsAffected > 0;
        }

        public async Task<List<IRDistinctValuesDTO>> GetAllIRNumberDistinctValues()
        {
            _logger.LogInformation("Request for CommonRepository:GetAllIRNumberDistinctValues");
            
            var result = await _db.GetAll<IRDistinctValuesDTO>(
                Common.DISTINCT_VALUES_IRNUMBER_QUERY, new { }
                );

            _logger.LogInformation("Response for CommonRepository:GetAllIRNumberDistinctValues");

            return result.ToList();
        }

        public async Task<List<MSNDistinctValues>> GetAllMSNNumber()
        {
            _logger.LogInformation("Request for CommonRepository:GetAllMSNNumber");

            var result = await _db.GetAll<MSNDistinctValues>(
                Common.GET_ALL_MSN_NUMBER_QUERY,
                new { }
            );

            _logger.LogInformation("Response for CommonRepository:GetAllMSNNumber");

            return result?.ToList() ?? new List<MSNDistinctValues>();
        }

        public async Task<List<PageRoleAccessResponseDto>> GetAllPageRoleAccessAsync(int roleId)
        {
            _logger.LogInformation("Fetching all page role access");
            try
            {
                var result = await _db.GetAll<PageRoleAccessResponseDto>(
                    Common.GET_ALL_PAGE_ROLE_ACCESS_QUERY, new { RoleId = roleId });

                var allPages = result.ToList();

                // Build parent-child hierarchy
                var parentPages = allPages
                    .Where(p => p.ParentId == null)
                    .OrderBy(p => p.DisplayOrder)
                    .ToList();

                foreach (var parent in parentPages)
                {
                    parent.Children = allPages
                        .Where(p => p.ParentId == parent.Id)
                        .OrderBy(p => p.DisplayOrder)
                        .ToList();
                }

                _logger.LogInformation("Fetched {Count} parent pages with children", parentPages.Count);
                return parentPages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all page role access");
                throw;
            }
        }

        public async Task<int> UpdatePageRoleAccessAsync(List<PageRoleAccessUpdateDto> request)
        {
            _logger.LogInformation("Updating {Count} page role access records", request.Count);
            try
            {
                int totalRowsAffected = 0;
                foreach (var item in request)
                {
                    var rowsAffected = await _db.Execute(Common.UPDATE_PAGE_ROLE_ACCESS_QUERY, new
                    {
                        item.RoleId,
                        item.FullAccess,
                        item.NoAccess,
                        item.ModifiedBy,
                        item.PageId
                    });
                    totalRowsAffected += rowsAffected;
                }
                _logger.LogInformation("Total {Count} rows updated", totalRowsAffected);
                return totalRowsAffected;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating page role access records");
                throw;
            }
        }

        public async Task<bool> AddDepartmentAsync(AddDepartmentRequestDto request)
        {
            _logger.LogInformation($"Inserting department into database: {request.DepartmentName}");

            try
            {
                var rowsAffected = await _db.Execute(
                    Common.ADD_DEPARTMENT_QUERY,
                    new { 
                            DepartmentName = request.DepartmentName,
                            CreatedBy=request.CreatedBy
                    }
                );

                _logger.LogInformation($"Department inserted successfully: {request.DepartmentName}");
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inserting department: {request.DepartmentName}");
                throw;
            }
        }

        public async Task<bool> AddUnitAsync(AddUnitRequestDto request)
        {
            _logger.LogInformation("Inserting unit into database: {UnitName}", request.UnitName);

            try
            {
              
                var rowsAffected = await _db.Execute(
                    Common.ADD_UNIT_QUERY,
                    new { UnitName = request.UnitName.Trim(), CreatedBy = request.CreatedBy }
                );

                _logger.LogInformation("Unit inserted successfully: {UnitName}", request.UnitName);
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error while inserting unit: {UnitName}", request.UnitName);
                throw new Exception("Database error occurred while adding unit.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting unit: {UnitName}", request.UnitName);
                throw;
            }
        }

        public async Task<bool> AddShapeAsync(AddShapeDto request)
        {
            _logger.LogInformation("Inserting shape into database: {ShapeName}", request.ShapeName);

            try
            {

                var rowsAffected = await _db.Execute(
                    Common.ADD_SHAPE_QUERY,
                    new { ShapeName = request.ShapeName.Trim(), CreatedBy = request.CreatedBy }
                );

                _logger.LogInformation("Shape inserted successfully: {ShapeName}", request.ShapeName);
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error while inserting shape: {ShapeName}", request.ShapeName);
                throw new Exception("Database error occurred while adding shape.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting shape: {ShapeName}", request.ShapeName);
                throw;
            }
        }

        public async Task<bool> AddStageAsync(AddStageRequestDto request)
        {
            _logger.LogInformation("Inserting stage into database: {StageName}", request.StageName);

            try
            {

                var rowsAffected = await _db.Execute(
                    Common.ADD_STAGE_QUERY,
                    new { StageName = request.StageName.Trim(), StageType= request.StageType, CreatedBy = request.CreatedBy }
                );

                _logger.LogInformation("Shape inserted successfully: {ShapeName}", request.StageName);
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error while inserting shape: {ShapeName}", request.StageName);
                throw new Exception("Database error occurred while adding shape.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting stage: {StageName}", request.StageName);
                throw;
            }
        }

        public async Task<bool> UpdateUnitAsync(UpdateUnitRequestDto request)
        {
            _logger.LogInformation("Updating unit in database: Id={Id}, UnitName={UnitName}", request.Id, request.UnitName);
            try
            {
                var rowsAffected = await _db.Execute(
                    Common.UPDATE_UNIT_QUERY,
                    new
                    {
                        Id = request.Id,
                        UnitName = request.UnitName.Trim(),
                        ModifiedBy = request.ModifiedBy
                    });
                _logger.LogInformation("Unit updated successfully: Id={Id}", request.Id);
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error while updating unit: Id={Id}", request.Id);
                throw new Exception("Database error occurred while updating unit.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating unit: Id={Id}", request.Id);
                throw;
            }
        }

        public async Task<bool> UpdateShapeAsync(UpdateShapeRequestDto request)
        {
            _logger.LogInformation("Updating shape in database: Id={Id}, ShapeName={ShapeName}", request.Id, request.ShapeName);
            try
            {
                var rowsAffected = await _db.Execute(
                    Common.UPDATE_SHAPE_QUERY,
                    new
                    {
                        Id = request.Id,
                        ShapeName = request.ShapeName.Trim(),
                        ModifiedBy = request.ModifiedBy
                    });
                _logger.LogInformation("Shape updated successfully: Id={Id}", request.Id);
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error while updating shape: Id={Id}", request.Id);
                throw new Exception("Database error occurred while updating shape.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shape: Id={Id}", request.Id);
                throw;
            }
        }

        public async Task<bool> UpdateStageAsync(UpdateStageRequestDto request)
        {
            _logger.LogInformation("Updating stage in database: Id={Id}, StageName={StageName}", request.Id, request.StageName);
            try
            {
                var rowsAffected = await _db.Execute(
                    Common.UPDATE_STAGE_QUERY,
                    new
                    {
                        Id = request.Id,
                        StageName = request.StageName.Trim(),
                        StageType = request.StageType,
                        ModifiedBy = request.ModifiedBy
                    });
                _logger.LogInformation("Stage updated successfully: Id={Id}", request.Id);
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error while updating stage: Id={Id}", request.Id);
                throw new Exception("Database error occurred while updating stage.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stage: Id={Id}", request.Id);
                throw;
            }
        }

        public async Task<bool> DeleteUnitAsync(int id, int modifiedBy)
        {
            _logger.LogInformation("Soft deleting unit: Id={Id}", id);
            try
            {
                var rowsAffected = await _db.Execute(
                    Common.DELETE_UNIT_QUERY,
                    new { Id = id, ModifiedBy = modifiedBy });

                _logger.LogInformation("Unit soft deleted successfully: Id={Id}", id);
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error while deleting unit: Id={Id}", id);
                throw new Exception("Database error occurred while deleting unit.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting unit: Id={Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteShapeAsync(int id, int modifiedBy)
        {
            _logger.LogInformation("Soft deleting shape: Id={Id}", id);
            try
            {
                var rowsAffected = await _db.Execute(
                    Common.DELETE_SHAPE_QUERY,
                    new { Id = id, ModifiedBy = modifiedBy });

                _logger.LogInformation("Shape soft deleted successfully: Id={Id}", id);
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error while deleting shape: Id={Id}", id);
                throw new Exception("Database error occurred while deleting shape.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting shape: Id={Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteStageAsync(int id, int modifiedBy)
        {
            _logger.LogInformation("Soft deleting stage: Id={Id}", id);
            try
            {
                var rowsAffected = await _db.Execute(
                    Common.DELETE_STAGE_QUERY,
                    new { Id = id, ModifiedBy = modifiedBy });

                _logger.LogInformation("Stage soft deleted successfully: Id={Id}", id);
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error while deleting stage: Id={Id}", id);
                throw new Exception("Database error occurred while deleting stage.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting stage: Id={Id}", id);
                throw;
            }
        }

        public async Task<bool> UpdateDepartmentAsync(UpdateDepartmentRequestDto request)
        {
            _logger.LogInformation("Updating department in database: Id={Id}, DepartmentName={DepartmentName}", request.Id, request.DepartmentName);
            try
            {
                var rowsAffected = await _db.Execute(
                    Common.UPDATE_DEPARTMENT_QUERY,
                    new
                    {
                        Id = request.Id,
                        DepartmentName = request.DepartmentName.Trim(),
                        ModifiedBy = request.ModifiedBy
                    });
                _logger.LogInformation("Department updated successfully: Id={Id}", request.Id);
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error while updating department: Id={Id}", request.Id);
                throw new Exception("Database error occurred while updating department.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating department: Id={Id}", request.Id);
                throw;
            }
        }

        public async Task<bool> DeleteDepartmentAsync(int id, int modifiedBy)
        {
            _logger.LogInformation("Soft deleting department: Id={Id}", id);
            try
            {
                var rowsAffected = await _db.Execute(
                    Common.DELETE_DEPARTMENT_QUERY,
                    new { Id = id, ModifiedBy = modifiedBy });
                _logger.LogInformation("Department soft deleted successfully: Id={Id}", id);
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error while deleting department: Id={Id}", id);
                throw new Exception("Database error occurred while deleting department.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting department: Id={Id}", id);
                throw;
            }
        }

        public async Task<bool> AddProdSeriesAsync(AddProdSeriesRequestDto request)
        {
            _logger.LogInformation("Inserting production series into database: {ProductionSeries}", request.ProductionSeries);
            try
            {
                var rowsAffected = await _db.Execute(
                    Common.ADD_PROD_SERIES_QUERY,
                    new
                    {
                        ProductionSeries = request.ProductionSeries.Trim(),
                        CreatedBy = request.CreatedBy
                    }
                );
                _logger.LogInformation("Production series inserted successfully: {ProductionSeries}", request.ProductionSeries);
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error while inserting production series: {ProductionSeries}", request.ProductionSeries);
                throw new Exception("Database error occurred while adding production series.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting production series: {ProductionSeries}", request.ProductionSeries);
                throw;
            }
        }

        public async Task<bool> UpdateProdSeriesAsync(UpdateProdSeriesRequestDto request)
        {
            _logger.LogInformation("Updating production series in database for Id: {Id}", request.Id);
            try
            {
                var rowsAffected = await _db.Execute(
                    Common.UPDATE_PROD_SERIES_QUERY,
                    new
                    {
                        Id = request.Id,
                        ProductionSeries = request.ProductionSeries.Trim(),
                        ModifiedBy = request.ModifiedBy
                    }
                );
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error while updating production series Id: {Id}", request.Id);
                throw new Exception("Database error occurred while updating production series.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating production series Id: {Id}", request.Id);
                throw;
            }
        }

        public async Task<bool> DeleteProdSeriesAsync(int id, int deletedBy)
        {
            _logger.LogInformation("Soft deleting production series from database for Id: {Id}", id);
            try
            {
                var rowsAffected = await _db.Execute(
                    Common.DELETE_PROD_SERIES_QUERY,
                    new
                    {
                        Id = id,
                        DeletedBy = deletedBy
                    }
                );
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error while deleting production series Id: {Id}", id);
                throw new Exception("Database error occurred while deleting production series.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting production series Id: {Id}", id);
                throw;
            }
        }
        public async Task<bool> UploadUserSignatureAsync(UploadSignatureRequestDto request)
        {
            _logger.LogInformation("Inserting user signature into database for UserId: {UserId}", request.UserId);
            try
            {
                var rowsAffected = await _db.Execute(
                    Users.INSERT_USER_SIGNATURE_QUERY,
                    new
                    {
                        UserId = request.UserId,
                        Signature = request.Signature,
                        Modifiedby=request.ModifiedBy
                    }
                );
                _logger.LogInformation("User signature inserted successfully for UserId: {UserId}", request.UserId);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting user signature for UserId: {UserId}", request.UserId);
                throw;
            }
        }

        public async Task<string?> GetUserSignatureAsync(int userId)
        {
            _logger.LogInformation("Fetching signature for UserId: {UserId}", userId);
            try
            {
                var result = await _db.GetSingle<string?>(
                    Users.GET_USER_SIGNATURE_BY_USERID,
                    new { UserId = userId });

                _logger.LogInformation("Signature {Status} for UserId: {UserId}",
                    result != null ? "found" : "not found", userId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching signature for UserId: {UserId}", userId);
                throw;
            }
        }
        public async Task<IEnumerable<Godrej.Precheck.Models.DTOs.User.UserSignatureListDto>> GetUsersWithSignaturesAsync()
        {
            _logger.LogInformation("Fetching all users with signatures from database.");
            try
            {
                var result = await _db.GetAll<Godrej.Precheck.Models.DTOs.User.UserSignatureListDto>(
                    Users.GET_USERS_WITH_SIGNATURES, new { });

                _logger.LogInformation("Fetched {Count} user signature records.", result?.Count() ?? 0);
                return result ?? Enumerable.Empty<Godrej.Precheck.Models.DTOs.User.UserSignatureListDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users with signatures.");
                throw;
            }
        }
    }
}
