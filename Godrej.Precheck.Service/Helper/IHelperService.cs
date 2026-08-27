using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godrej.Precheck.Models.DataModel;

namespace Godrej.Precheck.Service.Helper
{
    public interface IHelperService
    {
        AssemblyDrawingNumber GetAssemblyDrawing(string consumedDrawingNo);
    }
}
