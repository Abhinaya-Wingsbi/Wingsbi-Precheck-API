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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Repository.Repository.CommonRepository
{
    public interface ICommonRepository
    {

        Task<int> UpdatePageRoleAccessAsync(List<PageRoleAccessUpdateDto> request);

        Task<List<MSNDistinctValues>> GetAllMSNNumber();
        Task<List<IRDistinctValuesDTO>> GetAllIRNumberDistinctValues();
        Task<List<SecurityQuestionsInfo?>> GetAllSecurityQuestionModule();
        Task<List<PrecheckModule?>> GetAllPrecheckModule();
        Task<List<ComponentsType?>> GetAllComponentType();
        Task<ComponentsType?> GetComponentTypeByNameAsync(string query);
        Task<ComponentsType?> GetComponentTypeByIdAsync(int Id);
        Task<List<ProductionSeriess?>> GetAllProductionSeries();
        Task<List<IRNumbers?>> GetIRnumber(GetAllIRNumberRequestDto getAllIRNumberRequestDto);
        Task<IRNumbers> GetSingleIRnumber(string irNumber);
        Task<List<MSNNumbers?>> GetMSNNuber(GetAllMSNNumberRequestDto getAllMSNNumberRequestDto);
        Task<MSNNumbers> GetSingleMSNNuber(string msnNumber);
        Task<List<DrawingNumbers?>> GetAllDrawingNumber();
        Task<DrawingNumbers?> GetDrawingNumberById(int drawingId);
        Task<List<DocumentTypes?>> GetAllDocumnetType();
        Task<DocumentTypes?> GetDocumnetTypeByName(string query);
        Task<ProductionOrderModel> GetProductionOrderByName(string ProductionOrder);
        Task<List<UnitModel>> GetUnitByName();
        Task<List<ShapeModel>> GetAllShapes();
        Task<AssemblyNumbers?> GetAssemblyById(int assemblyId);
        Task<List<AssemblyNumbers?>> GetAllAssembly();
        Task<List<AssemblyDrawingMappingDto>> GetAllAssemblyDrawingMappingsAsync(string? lnItemCode = null);
        Task<int> ReassignParentDrawingAsync(ReassignParentDrawingRequestDto request, int modifiedBy);
        Task<int> RemoveChildDrawingAsync(RemoveChildDrawingRequestDto request, int modifiedBy);
        Task<int> AddAssemblyDrawingMappingAsync(AddAssemblyDrawingMappingRequestDto request, int createdBy);
        Task<int> DeleteDrawingNumberAsync(DeleteDrawingNumberRequestDto request, int modifiedBy);
        Task<ProductionSeriess?> GetProductionSeriesByName(string query);
        Task<ProductionSeriess?> GetProductionSeriesById(int Id);
        Task<Nomenclatures?> GetNomenclatureByName(string query);
        Task<User?> GetUserById(int UserId);
        
        //Task<List<IRNumbers>> GetIRNumberByDrawingNumber(string query);
        Task<List<IRNumbers>> GetIRNumberByDrawingNumber(GetIRNumberByDrawingNumberRequest getIRNumberByDrawingNumberRequest);
        Task<List<MSNNumbers>> GetMSNNuberByDrawingNumber(GetMSNNumberByDrawingNumberRequest getMSNNumberByDrawingNumberRequest);
        //Task<List<MSNNumbers>> GetMSNNuberByDrawingNumber(string query);
        Task<User?> GetUserByName(string name);
        Task<Department> GetDepartmentById(int departmentId);

        Task<int> GetLastSequenceNoIrNumberTable();

        Task<int> GetLastSequenceNoMSNNumberTable();

        Task<List<Department>> GetAllDepartment();

        Task<List<UserRole>> GetUserRoles();

        Task<List<Plant>> GetAllPlants();

        Task<List<Stage>> GetStagesByType(string stageType);

        Task<Stage> GetStageById(int stageId);
        Task<List<string>> GetAllLnItemCode(string search = null);
        Task<int> AddUserRole(UserRole role);
        Task<bool> UpdateUserRole(UserRole role);
        Task<bool> DeleteUserRole(int id, int modifiedBy);
        Task<List<User>> GetAllUsers();
        Task<List<User>> GetPendingUsersAsync();
        Task<bool> ApproveUserAsync(int id, int modifiedBy);
        Task<bool> UpdateUser(UserUpdateDto user);
        Task<bool> UpdateUserStatusAsync(UserStatusUpdateDto request);
        Task<List<PageRoleAccessResponseDto>> GetAllPageRoleAccessAsync(int roleId);
        Task<bool> AddDepartmentAsync(AddDepartmentRequestDto request);
        Task<bool> AddUnitAsync(AddUnitRequestDto request);
        Task<bool> AddShapeAsync(AddShapeDto request);
        Task<bool> AddStageAsync(AddStageRequestDto request);
        Task<bool> UpdateUnitAsync(UpdateUnitRequestDto request);
        Task<bool> UpdateShapeAsync(UpdateShapeRequestDto request);
        Task<bool> UpdateStageAsync(UpdateStageRequestDto request);

        Task<bool> DeleteUnitAsync(int id, int modifiedBy);
        Task<bool> DeleteShapeAsync(int id, int modifiedBy);
        Task<bool> DeleteStageAsync(int id, int modifiedBy);
        Task<bool> UpdateDepartmentAsync(UpdateDepartmentRequestDto request);
        Task<bool> DeleteDepartmentAsync(int id, int modifiedBy);
        Task<AddUserResponseDto?> AddUserAsync(AddUserRequestDto request, int createdBy);
        Task<bool> AddProdSeriesAsync(AddProdSeriesRequestDto request);
        Task<bool> UpdateProdSeriesAsync(UpdateProdSeriesRequestDto request);
        Task<bool> DeleteProdSeriesAsync(int id, int deletedBy);
        Task<bool> UploadUserSignatureAsync(UploadSignatureRequestDto request);
        Task<string?> GetUserSignatureAsync(int userId);
        Task<IEnumerable<Godrej.Precheck.Models.DTOs.User.UserSignatureListDto>> GetUsersWithSignaturesAsync();
    }
}
