namespace WastePlatform.Application.Admin.Users.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "admin", "citizen", "collector", "enterprise"
        public bool IsActive { get; set; }
        public string LastActiveDate { get; set; } = string.Empty;
    }
}