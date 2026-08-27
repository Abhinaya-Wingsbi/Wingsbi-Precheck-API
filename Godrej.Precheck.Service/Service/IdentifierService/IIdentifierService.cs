using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DTOs.IRNumber;
using Godrej.Precheck.Models.DTOs.MSNNumber;

namespace Godrej.Precheck.Service.Service.IdentifierService
{
    public interface IIdentifierService
    {
        Task<IRNumbers> InsertIRNumberAsync(IRNumberDto irNumberDto);

        Task<MSNNumbers> InsertMSNNumberAsync(MSNNumberDto msnNumberDto);

        Task<IRNumbers> InsertStandardIRNumberAsync(StandardIRNumberDto standardIRNumberDto);

        Task<MSNNumbers> InsertStandardMSNNumberAsync(StandardMSNNumberDto standardMSNNumberDto);

        Task<IRNumbers> UpadateIRNumberAsync(UpdateIRDto updateIR);

        Task<MSNNumbers> UpadateMSNNumber(UpdateMSNDto updateMSN);

        byte[] GenerateDownloadMSNMemoPdf(Godrej.Precheck.Models.DTOs.Identifier.DownloadMSNMemoDto request);

        Task<string> GenerateDownloadMSNMemoHtml(Godrej.Precheck.Models.DTOs.Identifier.DownloadMSNMemoDto request);
    }
}
