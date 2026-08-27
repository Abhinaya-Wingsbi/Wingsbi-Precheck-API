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

namespace Godrej.Precheck.Service.Service.CommonSevice
{
    public interface ICommonService
    {
        Task<List<MSNDistinctValues>> GetAllMSNNumberService();
        Task<List<IRDistinctValuesDTO>> GetAllIRNumberDistinctValuesService();
        Task<List<SecurityQuestionsInfo>> GetAllSecurityQuestions();
        Task<List<PrecheckModule>> GetAllModules();

        Task<List<ComponentsType>> ComponentTypeService();
        Task<int> UpdatePageRoleAccessAsync(List<PageRoleAccessUpdateDto> request);

        Task<ComponentsType> ComponentTypeByNameService(string query);

        Task<ComponentsType> ComponentTypeByIdService(int Id);

        Task<List<ProductionSeriess>> ProductionSeriesService();

        Task<ProductionSeriess> ProductionSeriesByNameService(string query);

        Task<ProductionSeriess> ProductionSeriesByIdService(int Id);

        Task<List<MSNNumbers>> MSNNumberService(GetAllMSNNumberRequestDto getAllMSNNumberRequestDto);

        Task<List<IRNumbers>> IRNumberService(GetAllIRNumberRequestDto getAllIRNumberRequestDto);

        Task<List<GetAllDrawingResponseDto>> GetAllDrawingNumberService(GetAllDrawingRequestDto request = null);

        Task<List<DocumentTypes>> DocumentTypeService();

        Task<DocumentTypes> DocumentTypeByNameService(string query);

        Task<Nomenclatures> NomenclatureService(string query);

        Task<User> UserService(int Id);
        //Task<List<IRNumbers>> IRNumberByDrawingNumberService(string query);
        Task<List<IRNumbers>> IRNumberByDrawingNumberService(GetIRNumberByDrawingNumberRequest getIRNumberByDrawingNumberRequest);

        //Task<List<MSNNumbers>> MSNNumberByDrawingNumberService(string query);
        Task<List<MSNNumbers>> MSNNumberByDrawingNumberService(GetMSNNumberByDrawingNumberRequest getMSNNumberByDrawingNumberRequest);

        Task<User> UserByNameService(string name);

        Task<ProductionOrderModel> ProductionOrderByNameService(string query);

        Task<List<UnitModel>> UnitByName();
        Task<List<ShapeModel>> GetAllShapes();
        Task<List<string>> GetAllLnItemCode(string search = null);

        Task<Department> GetDepartmentById(int departmentId);

        Task<List<Department>> GetAllDepartment();

        Task<List<UserRole>> GetUserRoles();

        Task<List<Plant>> GetAllPlants();

        Task<List<AssemblyNumbers>> GetAllAssembly();
        Task<List<AssemblyDrawingMappingDto>> GetAllAssemblyDrawingMappingsAsync(string? lnItemCode = null);
        Task<int> ReassignParentDrawingAsync(ReassignParentDrawingRequestDto request, int modifiedBy);
        Task<int> RemoveChildDrawingAsync(RemoveChildDrawingRequestDto request, int modifiedBy);
        Task<int> AddAssemblyDrawingMappingAsync(AddAssemblyDrawingMappingRequestDto request, int createdBy);
        Task<int> DeleteDrawingNumberAsync(DeleteDrawingNumberRequestDto request, int modifiedBy);

        Task<List<Stage>> GetIRStagesService();

        Task<List<Stage>> GetMSNStagesService();

        Task<int> AddUserRole(UserRole role);
        Task<bool> UpdateUserRole(UserRole role);
        Task<bool> DeleteUserRole(int id, int modifiedBy);
        Task<List<User>> GetAllUsersService();
        Task<List<User>> GetPendingUsersService();
        Task<bool> ApproveUserService(int id, int modifiedBy);
        Task<bool> UpdateUserService(UserUpdateDto user);
        Task<bool> UpdateUserStatusAsync(UserStatusUpdateDto request);
        Task<List<PageRoleAccessResponseDto>> GetAllPageRoleAccessAsync(int roleId);
        Task<bool> AddDepartmentAsync(AddDepartmentRequestDto request);
        Task<bool> AddUnitAsync(AddUnitRequestDto request);
        Task<bool> AddStageAsync(AddStageRequestDto stageDto);
        Task<bool> AddShapeAsync(AddShapeDto stageDto);
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
