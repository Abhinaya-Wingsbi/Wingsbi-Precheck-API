using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel;
using Godrej.Precheck.Repository.Repository.PrecheckRepository;
using Microsoft.Extensions.Logging;

namespace Godrej.Precheck.Service.Helper
{
    public class HelperService : IHelperService
    {
        private readonly ILogger<HelperService> _logger;
        public HelperService(ILogger<HelperService> logger)
        {
            _logger = logger;       
        }

        public AssemblyDrawingNumber GetAssemblyDrawing(string consumedDrawingNo)
        {
            if (string.IsNullOrEmpty(consumedDrawingNo))
            {
                _logger.LogError("Consumed drawing number is null or empty.");
                throw new ArgumentException("Consumed drawing number cannot be null or empty.");
            }

            var parts = consumedDrawingNo.Split('/');
            if (parts.Length != 3)
            {
                _logger.LogError("Consumed drawing number format is invalid.");
                throw new FormatException("Consumed drawing number must contain exactly two '/' characters.");
            }

            return new AssemblyDrawingNumber
            {
                Id = int.Parse(parts[0]),
                ProdSeries = parts[1],
                DrawingNo = parts[2]
            };
        }
    }
}
