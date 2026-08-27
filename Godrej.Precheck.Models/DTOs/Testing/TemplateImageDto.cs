namespace Godrej.Precheck.Models.DTOs.Testing
{
    public class TemplateImageDto
    {
        public string PlaceholderKey { get; set; } = string.Empty;
        public string ImageData { get; set; } = string.Empty;
        public string MimeType { get; set; } = "image/png";

        public string ToDataUri() =>
            string.IsNullOrWhiteSpace(ImageData)
                ? string.Empty
                : ImageData.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
                    ? ImageData
                    : $"data:{MimeType};base64,{ImageData}";
    }
}
