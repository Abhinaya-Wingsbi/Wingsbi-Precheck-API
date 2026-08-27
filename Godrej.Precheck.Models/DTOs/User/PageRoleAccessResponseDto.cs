using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Models.DTOs.User
{
    public class PageRoleAccessResponseDto
    {
        public int Id { get; set; }
        public string PageName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int? ParentId { get; set; }
        public bool NoAccess { get; set; }
        public bool FullAccess { get; set; }
        public List<PageRoleAccessResponseDto> Children { get; set; } = new();
    }
}
