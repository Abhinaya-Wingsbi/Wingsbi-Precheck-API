namespace Godrej.Precheck.Models.DTOs.Reset
{
    public class ResetModel
    {
        public string UserId { get; set; }
        public string PasswordHash { get; set; }

        public int SecurityQuestionId { get; set; }
        
        public string SecurityStamp { get; set; } 

        public string SecurityAnswer { get; set; }

    }
}
