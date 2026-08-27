using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Models.DTOs.Barcode;
using Godrej.Precheck.Models.DTOs.Login;
using Godrej.Precheck.Models.DTOs.Stage;
using Mapster;

namespace Godrej.Precheck.Service.MapperSetup
{

    public static class MappingSetup
    {
        private static object Padlock = new object();
        private static bool HasInitialised = false;
        public static void Init()
        {

            if (HasInitialised)
            {
                return;
            }

            lock (Padlock)
            {
                if (HasInitialised)
                {
                    return;
                }

                HasInitialised = true;
            }

            TypeAdapterConfig<LoginRequest, User>
                .NewConfig()
                .Map(dest => dest.UserId, src => src.UserId)
                .Map(dest => dest.PasswordHash, src => src.Password);

            // Mapping for QRCodeDetailsDto to QRCodeDetails
            TypeAdapterConfig<QRCodeDetailsDto, QRCodeDetails>
                .NewConfig()
                //.Map(dest => dest.QRCodeNumber, src => src.QRCodeNumber)
                .Map(dest => dest.ProductionSeriesId, src => src.ProductionSeriesId)
                //.Map(dest => dest.AssemblyNumberId, src => src.AssemblyNumberId)
                .Map(dest => dest.NomenclatureId, src => src.NomenclatureId)
                .Map(dest => dest.ComponentTypeId, src => src.ComponentTypeId)
                .Map(dest => dest.IdNumber, src => src.IdNumber)
                .Map(dest => dest.IrNumberId, src => src.IrNumberId)
                .Map(dest => dest.MsnNumberId, src => src.MsnNumberId)
                .Map(dest => dest.RefDocRemarks, src => src.RefDocRemarks)
                .Map(dest => dest.Quantity, src => src.Quantity)
                .Map(dest => dest.Desposition, src => src.Desposition)
                .Map(dest => dest.MyDate, src => src.MyDate)
                .Map(dest => dest.Users, src => src.Users)
                .Map(dest => dest.ProductionOrderNumber, src => src.ProductionOrderNumber)
                .Map(dest => dest.RackLocation, src => src.RackLocation)
                .Map(dest => dest.OperationNo, src => src.OperationNo)
                .Map(dest => dest.ExpiryDate, src => src.ExpiryDate)
                .Map(dest => dest.CreatedBy, src => src.CreatedBy)
                .Map(dest => dest.CreatedDate, src => src.CreatedDate)
                .Map(dest => dest.MRIRNumber, src => src.MRIRNumber)
                //.Map(dest => dest.ModifiedDate, src => src.ModifiedDate)
                .Map(dest => dest.IsActive, src => src.IsActive);

            // Mapping for Stage to StageResponseDto (StageName -> Stage)
            TypeAdapterConfig<Stage, StageResponseDto>
                .NewConfig()
                .Map(dest => dest.Stage, src => src.StageName);

        }

    }

}


