using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.ComponentType
{
    public class ComponentTypeResponseDto
    {
        public int ID { get; set; }
        public string ComponentType { get; set; } 
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool Is_Active { get; set; }

    }
}
