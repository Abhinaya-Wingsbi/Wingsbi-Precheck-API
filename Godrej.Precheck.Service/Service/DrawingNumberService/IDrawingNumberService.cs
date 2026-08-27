using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DTOs.DrawingNumber;

namespace Godrej.Precheck.Service.Service.DrawingNumberService
{
    public interface IDrawingNumberService
    {
        Task<DrawingMappingResponseDto> InsertDrawingMappingsAsync(InsertDrawingMappingDto request);
        Task<GetDrawingMappingDto> GetDrawingMappingsAsync(int drawingNumberId);
    }
}

