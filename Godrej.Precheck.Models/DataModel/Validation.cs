using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel
{
    public class Validation
    {
        public class UnsubmittedComponent
        {
            public string DrawingNumber { get; set; }
        }

        public class PrecheckValidationError
        {
            public string Error { get; set; }
            public List<UnsubmittedComponent> UnsubmitedComponents { get; set; }
        }
    }
}
