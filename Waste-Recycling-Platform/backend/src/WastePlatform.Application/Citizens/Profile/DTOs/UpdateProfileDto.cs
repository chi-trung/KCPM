namespace WastePlatform.Application.Citizens.Profile.DTOs
{
    public class UpdateProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
    }
}
