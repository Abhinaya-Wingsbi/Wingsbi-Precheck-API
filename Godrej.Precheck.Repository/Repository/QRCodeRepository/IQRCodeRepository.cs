using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DTOs.ConsumedIn;
using Godrej.Precheck.Models.DTOs.Precheck;
using Godrej.Precheck.Models.DTOs.QRCodeDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Repository.Repository.QRCodeRepository
{
    public interface IQRCodeRepository
    {
        Task<QrCodeResponse> InsertQRCodeDetailsAsync(QRCodeDetails qrCodeDetails);
        Task<QrCodeResponse> InsertStandardQRCodeDetailsAsync(StandardQRCodeDetails qrCodeDetails);

        Task<QRCodeDetailsResponseDto?> GetActiveQRcodeDetailsAsync(string QRCodeNumber);

        Task<QRCodeDetailsResponseDto> GetQRcodeDetailsAsync(string QRCodeNumber, int? qrCodeStatusId = null);
        Task<StandardQRDetailsResponseDto> GetStandardQRCodeDetailsAsync(string QRCodeNumber);

        //get qrcode details with QRCodeNumber Or (prodseries and DrawingNumberId)
        Task<List<QRCodeDetailsResponseDto>> GetQRcodeWithParameterAsync(GetQRCodeRequestDto getQRCodeRequestDto);

        //same as GetQRcodeWithParameterAsync but restricted to consumed QR codes (qrcodestatusid = 2, isactive = 0)
        Task<List<QRCodeDetailsResponseDto>> GetConsumedQRcodeWithParameterAsync(GetQRCodeRequestDto getQRCodeRequestDto);
        Task<bool> InsertQRCodeInConsumptionAsync(QRCodeDetails qrCodeDetails);
        Task<bool> InsertStandardQRCodeInConsumptionAsync(StandardQRCodeDetails qrCodeDetails);

        Task<bool> UpdateQrCodeDetails(string qrCode, string consumedInDrawing, decimal? quantity);

        Task<bool> ComponentStoreIn(string QRCodeNumber);

        Task<QRCodeDetailsResponseDto> ValiadateQrCode(int productionseriesid, int idnumbers, int drawingnumberid, string? productionOrderNumber);

        Task<List<ConsumedInResponseDto>> ConsumedInRepoAsync(ConsumedInRequestDto request);

        Task<string> VerifyIdNumber(string idNumber);

        Task<string> GetLatestBatchIdNumber();

        Task<List<BatchQRcodeResponse>> GetChildComponenetforAssembly(int DrawingId);

        Task<List<QRCodeDetailsResponseDto>> GetComponentByStorInByDate(StoredInQrCodeRequest storeInRequest);

        Task<QrCodeResponse> InsertPrecheckQRCodeDetailsAsync(PrecheckQRCodeRequestDto request);

        Task<List<StandardQRDetailsResponseDto>> GetQRCodesByIdNumbersAsync(List<string> idNumbers);

        Task<List<StandardQRDetailsResponseDto>> GetQRCodesByIdMrirHtCombinationAsync(List<(string IdNo, string Mirir, string HtLotNo,int? LnItemCodeId,int DrawingNumberId)> combinations);

        Task<bool> UpdateQRCodeDetailsAsync(UpdateQRCodeDto request);
        Task<bool> DisableQRCodeAsync(DisableQRCodeRequestDto request);
        Task<QRCodeDetailsResponseDto?> GetQRcodeDetailsAnyStatusAsync(string QRCodeNumber);

        Task<bool> IsStandardQRCode(string qrCodeNumber);
        Task<bool> CheckPreviousBatchExists(int drawingNumberId,int idNumbers);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<List<string>> GetDistinctBatchIdNumbersAsync();
        Task<List<string>> GetAllFanManSerialNumbersAsync();
        Task<List<ConsumedInResponseDto>> ExportConsumedInRepoAsync(ConsumedInRequestDto request);
        Task<int> BulkUpdateQRCodeAsync(BulkUpdateQRCodeRequestDto request);

        Task<List<GetAvailableComponentsResponse>> GetAvailableQr(GetAvailableQrRequest request);

    }
}
