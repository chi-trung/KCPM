namespace WastePlatform.Application.Common.DTOs;

/// <summary>Response khi đăng nhập/đăng ký thành công</summary>
public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public UserDto User { get; set; } = null!;
}

/// <summary>Thông tin người dùng trong response</summary>
public class UserDto
{
    public string Id { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
}
