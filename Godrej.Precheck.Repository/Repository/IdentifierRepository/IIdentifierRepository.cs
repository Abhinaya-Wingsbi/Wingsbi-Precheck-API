using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel;

namespace Godrej.Precheck.Repository.Repository.IdentifierRepository
{
    public interface IIdentifierRepository
    {
        Task<IRNumbers> InsertIRNumberAsync(IRNumbers irNumber);
        Task<MSNNumbers> InsertMSNNumberAsync(MSNNumbers msnNumber);
        Task<IRNumbers> InsertStandardIRNumberAsync(IRNumbers irNumber);
        Task<MSNNumbers> InsertStandardMSNNumberAsync(MSNNumbers msnNumber);
        Task<IRNumbers> UpdateIRNumberAsync(IRNumbers irNumber);
        Task<MSNNumbers> UpdateMSNNumberAsync(MSNNumbers msnNumber);
        Task<bool> ExistsIrIdConflictAsync(int prodSeriesId, int drawingNumberId, int idStart, int idEnd, int departmentId, string operationNumber, int stageId);
        Task<bool> ExistsMsnIdConflictAsync(int prodSeriesId, int drawingNumberId, int idStart, int idEnd, int departmentId, string operationNumber, int stageId);
        Task<string?> ExistsIrIdNumberAsync(int prodSeriesId, int drawingNumberId, string idNumber, int departmentId, string operationNumber, int stageId);
        Task<bool> ExistsMsnIdNumberAsync(int prodSeriesId, int drawingNumberId, string idNumber, int departmentId, string operationNumber, int stageId);
        
        // Update Validation Methods
        Task<bool> ExistsIrIdConflictUpdateAsync(int prodSeriesId, int drawingNumberId, int idStart, int idEnd, int excludeId, int departmentId, string operationNumber, int stageId);
        Task<bool> ExistsMsnIdConflictUpdateAsync(int prodSeriesId, int drawingNumberId, int idStart, int idEnd, int excludeId, int departmentId, string operationNumber, int stageId);
        Task<bool> ExistsIrIdNumberUpdateAsync(int prodSeriesId, int drawingNumberId, string idNumber, int excludeId, int departmentId, string operationNumber, int stageId);
        Task<bool> ExistsMsnIdNumberUpdateAsync(int prodSeriesId, int drawingNumberId, string idNumber, int excludeId, int departmentId, string operationNumber, int stageId);
        Task<string> GetDrawingNumberByID(int drawingNumberId);

    }
}
