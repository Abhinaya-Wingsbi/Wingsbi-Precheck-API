using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel.Precheck
{
    public class MakePrecheckResponse
    {
        public List<int> UpadatedConsunptionId { get; set; }
        public List<int> UpadatedPrecheckId { get; set; }
    }
}
