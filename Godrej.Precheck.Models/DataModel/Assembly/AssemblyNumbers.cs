using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DataModel.Assembly
{
    public class AssemblyNumbers
    {
        public int Id { get; set; } 
        public string AssemblyNumber { get; set; } 
        public int? CreatedBy { get; set; } 
        public DateTime? CreatedDate { get; set; } 
        public int? ModifiedBy { get; set; } 
        public DateTime? ModifiedDate { get; set; } 
        public bool IsActive { get; set; } 
    }
}
