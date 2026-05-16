using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WastePlatform.Application.Auth.Commands;
using WastePlatform.Application.Common.DTOs;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Services;

namespace WastePlatform.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Lấy danh sách các role hợp lệ</summary>
    [HttpGet("roles")]
    [AllowAnonymous]
    public IActionResult GetAvailableRoles()
    {
        var roles = Enum.GetValues(typeof(UserRole))
            .Cast<UserRole>()
            .Select(r => new { name = r.ToString(), value = (int)r })
            .ToList();
        
        return Ok(new
        {
            message = "Available user roles",
            roles = roles,
            details = new
            {
                citizen = "Người dùng bình thường - báo cáo rác thải",
                enterprise = "Công ty tái chế - nhận và xử lý rác",
                collector = "Nhân viên thu gom - nhân viên của công ty tái chế",
                admin = "Quản trị viên hệ thống"
            }
        });
    }

    /// <summary>Đăng ký tài khoản mới</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterCommand cmd)
    {
        if (!ModelState.IsValid)
            return BadRequest(new
            {
                message = "Invalid input",
                errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });

        try
        {
            var result = await _authService.RegisterAsync(cmd);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Đăng nhập, nhận JWT token</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand cmd)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _authService.LoginAsync(cmd);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>Lấy thông tin người dùng đang đăng nhập từ JWT claims</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub");
        var email    = User.FindFirstValue(ClaimTypes.Email)
                    ?? User.FindFirstValue("email");
        var role     = User.FindFirstValue(ClaimTypes.Role);
        var fullName = User.FindFirstValue("fullName");

        return Ok(new
        {
            userId,
            email,
            role,
            fullName
        });
    }
}
