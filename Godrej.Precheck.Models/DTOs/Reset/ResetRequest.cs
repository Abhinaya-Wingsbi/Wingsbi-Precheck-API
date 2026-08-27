namespace Godrej.Precheck.Models.DTOs.Reset
{
    public class ResetRequest
    {
        public string UserId { get; set; }
        public string Password { get; set; }

        public int SecurityQuestionId { get; set; }

        public string SecurityAnswer { get; set; }

    }
}
