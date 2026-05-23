using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WastePlatform.Infrastructure.Persistence;

namespace WastePlatform.API.Middleware;

/// <summary>
/// Middleware kiểm tra xem user còn active hay không trước khi xử lý request.
/// Fix bug: User bị lock vẫn có thể dùng JWT token cũ để call API
/// </summary>
public class ValidateUserStatusMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidateUserStatusMiddleware> _logger;

    public ValidateUserStatusMiddleware(RequestDelegate next, ILogger<ValidateUserStatusMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, WastePlatformDbContext dbContext)
    {
        try
        {
            // Kiểm tra nếu request có user được authenticate
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier) 
                               ?? context.User.FindFirstValue("sub");

                _logger.LogInformation($"🔍 [ValidateUserStatus] JWT UserId: {userIdClaim}");

                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
                {
                    // Lấy user từ database
                    var user = await dbContext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == userId);

                    if (user != null)
                    {
                        _logger.LogInformation($"✓ [ValidateUserStatus] User found: {user.Email}, IsActive={user.IsActive}");

                        // Nếu user bị lock (IsActive = false), từ chối request
                        if (!user.IsActive)
                        {
                            _logger.LogError($"❌ [ValidateUserStatus] BLOCKED - User is inactive: {user.Email} ({userId})");
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            
                            await context.Response.WriteAsJsonAsync(new 
                            { 
                                success = false,
                                message = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.",
                                code = "ACCOUNT_LOCKED"
                            });
                            return;
                        }
                        else
                        {
                            _logger.LogInformation($"✅ [ValidateUserStatus] User is active: {user.Email}, proceeding...");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"⚠️ [ValidateUserStatus] User not found in DB: {userId}");
                    }
                }
                else
                {
                    _logger.LogWarning($"⚠️ [ValidateUserStatus] Invalid/missing userId in JWT: {userIdClaim}");
                }
            }
            else
            {
                _logger.LogDebug($"ℹ️ [ValidateUserStatus] Unauthenticated request, skipping status check");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ [ValidateUserStatus] Exception: {ex.Message}");
        }

        await _next(context);
    }
}

