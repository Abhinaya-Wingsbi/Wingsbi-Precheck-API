using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DTOs.Barcode;
using Godrej.Precheck.Models.DTOs.ConsumedIn;
using Godrej.Precheck.Models.DTOs.Precheck;
using Godrej.Precheck.Models.DTOs.QRCodeDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Service.Service.QRCodeService
{
    public interface IQRCodeService
    {
        Task<List<QRCodeDetailsResponseDto?>> InsertQRCodeDetailsAsync(QRCodeDetailsDto qrCodeDetailsDto);
        Task<List<StandardQRDetailsResponseDto?>> InsertStandardQRCodeDetailsAsync(StandardQRDataDto qrCodeDetailsDto);
        Task<QRCodeDetailsResponseDto?> GetQRCodeDetailsService(string QRCodeNumber, int? qrCodeStatusId = null);

        //Get QRCode Details with Paramter
        Task<List<QRCodeDetailsResponseDto>> GetQRCodeDetailsWithParameterService(GetQRCodeRequestDto getQRCodeRequest);

        //same as GetQRCodeDetailsWithParameterService but restricted to consumed QR codes (qrcodestatusid = 2, isactive = 0)
        Task<List<QRCodeDetailsResponseDto>> GetConsumedQRCodeDetailsWithParameterService(GetQRCodeRequestDto getQRCodeRequest);
        Task<QRCodeDetailsResponseDto> ComponentStoreInService(string QRCodeNumber);
        //byte[] ExportQRCodeToExcel(QRCodeDetailsResponseDto qrCodeItems);

        byte[] ExportQRCodeToExcel(List<QRCodeDetailsResponseDto> qrCodeItems);
        Task<List<ConsumedInResponseDto>> ConsumedInService(ConsumedInRequestDto request);

        Task<List<BatchIdResponse>> ProcessBatchService(BatchQRcodeRequestDto batchQRcodeRequest);

        Task<List<QRCodeDetailsResponseDto>> GetComponentStoreInByDateService(StoredInQrCodeRequest storeInRequest);

        Task<QrCodeResponse> InsertPrecheckQRCodeDetailsService(PrecheckQRCodeRequestDto request);

        Task<QRCodeDetailsResponseDto> UpdateQRCodeDetailsAsync(UpdateQRCodeDto request);
        Task<string> DisableQRCodeAsync(DisableQRCodeRequestDto request);

        // Standard QR Code specific methods
        Task<StandardQRDetailsResponseDto> GetStandardQRCodeDetailsService(string qrCodeNumber);
        byte[] ExportStandardQRCodeToExcel(List<StandardQRDetailsResponseDto> qrCodeItems);
        Task<List<UserDto>> GetAllUsersServiceAsync();
        Task<List<string>> GetDistinctBatchIdNumbersServiceAsync();
        Task<List<string>> GetAllFanManSerialNumbersServiceAsync();
        Task<byte[]> ExportConsumedInServiceAsync(ConsumedInRequestDto request);
        Task<int> BulkUpdateQRCodeService(BulkUpdateQRCodeRequestDto request);

        Task<List<GetAvailableComponentsResponse>> GetAvailableQrService(GetAvailableQrRequest request);
    }
}
