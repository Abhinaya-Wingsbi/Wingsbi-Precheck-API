using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.User
{
    public class PageRoleAccessUpdateDto
    {
        public int RoleId { get; set; }
        public bool FullAccess { get; set; }
        public bool NoAccess { get; set; }
        public int? ModifiedBy { get; set; }
        public int PageId { get; set; }
    }
}
