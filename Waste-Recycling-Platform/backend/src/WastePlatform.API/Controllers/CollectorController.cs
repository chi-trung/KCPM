using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WastePlatform.Domain.Entities;
using WastePlatform.Infrastructure.Persistence;

namespace WastePlatform.API.Controllers;

[ApiController]
[Route("api/collector/profile")]
[Authorize(Roles = "Collector")]
public class CollectorController : ControllerBase
{
    private readonly WastePlatformDbContext _context;

    public CollectorController(WastePlatformDbContext context)
    {
        _context = context;
    }

    private async Task<Collector?> GetCurrentCollectorAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return null;

        return await _context.Collectors
            .Include(c => c.User)
            .Include(c => c.Enterprise)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    /// <summary>
    /// Lấy thông tin cá nhân của Collector
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var collector = await GetCurrentCollectorAsync();
        if (collector == null)
            return Unauthorized(new { message = "Không tìm thấy hồ sơ Collector." });

        return Ok(new
        {
            collector.Id,
            collector.UserId,
            collector.EnterpriseId,
            EnterpriseName = collector.Enterprise.CompanyName,
            collector.User.FullName,
            collector.User.Email,
            collector.User.Phone,
            collector.IsAvailable,
            collector.CreatedAt
        });
    }

    /// <summary>
    /// Bật/tắt trạng thái sẵn sàng nhận việc (Available/Not Available)
    /// </summary>
    [HttpPatch("availability")]
    public async Task<IActionResult> ToggleAvailability([FromBody] ToggleAvailabilityRequest request)
    {
        var collector = await GetCurrentCollectorAsync();
        if (collector == null)
            return Unauthorized(new { message = "Không tìm thấy hồ sơ Collector." });

        collector.ToggleAvailability(request.IsAvailable);
        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            message = "Cập nhật trạng thái thành công.",
            isAvailable = collector.IsAvailable 
        });
    }
}

public class ToggleAvailabilityRequest
{
    [Required]
    public bool IsAvailable { get; set; }
}