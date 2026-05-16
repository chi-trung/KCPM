using Microsoft.EntityFrameworkCore;
using WastePlatform.Application.Auth.Commands;
using WastePlatform.Application.Common.DTOs;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;

namespace WastePlatform.Infrastructure.Services;

public class AuthService
{
    private readonly WastePlatformDbContext _db;
    private readonly IJwtService _jwtService;

    public AuthService(WastePlatformDbContext db, IJwtService jwtService)
    {
        _db = db;
        _jwtService = jwtService;
    }

    // ── Register ────────────────────────────────────────────────────────
    public async Task<AuthResponseDto> RegisterAsync(RegisterCommand cmd)
    {
        if (await _db.Users.AnyAsync(u => u.Email == cmd.Email.ToLower()))
            throw new InvalidOperationException("Email đã được sử dụng.");

        // Validate role - only allow Citizen and Enterprise during public registration
        if (cmd.Role != UserRole.Citizen && cmd.Role != UserRole.Enterprise)
            throw new InvalidOperationException(
                $"Role '{cmd.Role}' không thể đăng ký tự động. " +
                "Collector phải được thêm bởi Enterprise, Admin phải được tạo bởi hệ thống.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(cmd.Password);
        var user = User.Create(
            email: cmd.Email.ToLower().Trim(),
            passwordHash: passwordHash,
            fullName: cmd.FullName.Trim(),
            role: cmd.Role
        );

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await EnsureEnterpriseProfileAsync(user);

        return BuildResponse(user);
    }

    // ── Login ────────────────────────────────────────────────────────────
    public async Task<AuthResponseDto> LoginAsync(LoginCommand cmd)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == cmd.Email.ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(cmd.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Tài khoản đã bị khóa.");

        await EnsureEnterpriseProfileAsync(user);

        return BuildResponse(user);
    }

    private async Task EnsureEnterpriseProfileAsync(User user)
    {
        if (user.Role != UserRole.Enterprise)
            return;

        var hasEnterpriseProfile = await _db.Enterprises.AnyAsync(e => e.UserId == user.Id);
        if (hasEnterpriseProfile)
            return;

        var enterprise = new Enterprise
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CompanyName = user.FullName,
            ServiceArea = null,
            CapacityKgPerDay = null,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Enterprises.Add(enterprise);
        await _db.SaveChangesAsync();
    }

    // ── Helper ───────────────────────────────────────────────────────────
    private AuthResponseDto BuildResponse(User user)
    {
        var token = _jwtService.GenerateToken(user);
        return new AuthResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id.ToString(),
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString().ToLower()
            }
        };
    }
}
