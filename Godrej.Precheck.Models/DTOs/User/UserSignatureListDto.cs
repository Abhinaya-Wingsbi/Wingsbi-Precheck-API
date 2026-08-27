namespace Godrej.Precheck.Models.DTOs.User
{
    public class UserSignatureListDto
    {
        public int SignatureId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? EmployeeId { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? RoleName { get; set; }
        public DateTime SignatureCreatedDate { get; set; }
    }
}
