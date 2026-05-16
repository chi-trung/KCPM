using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;

namespace WastePlatform.API.Controllers;

[ApiController]
[Route("api/enterprise/collectors")]
[Authorize(Roles = "Enterprise")]
public class EnterpriseCollectorController : ControllerBase
{
    private readonly WastePlatformDbContext _context;

    public EnterpriseCollectorController(WastePlatformDbContext context)
    {
        _context = context;
    }

    private async Task<Enterprise?> GetCurrentEnterpriseAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return null;

        return await _context.Enterprises.FirstOrDefaultAsync(e => e.UserId == userId);
    }

    [HttpGet]
    public async Task<IActionResult> GetCollectors()
    {
        var enterprise = await GetCurrentEnterpriseAsync();
        if (enterprise == null)
            return Unauthorized(new { message = "Enterprise profile not found." });

        var collectors = await _context.Collectors
            .Include(c => c.User)
            .Where(c => c.EnterpriseId == enterprise.Id)
            .OrderBy(c => c.User.FullName)
            .Select(c => new
            {
                c.Id,
                Name = c.User.FullName,
                Email = c.User.Email,
                Phone = c.User.Phone,
                c.IsAvailable,
                c.CreatedAt,
                TaskCount = c.CollectionTasks.Count(t => t.Status != CollectionTaskStatus.Collected)
            })
            .ToListAsync();

        return Ok(collectors);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCollector([FromBody] CreateEnterpriseCollectorRequest request)
    {
        var enterprise = await GetCurrentEnterpriseAsync();
        if (enterprise == null)
            return Unauthorized(new { message = "Enterprise profile not found." });

        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.TemporaryPassword))
            return BadRequest(new { message = "FullName, Email and TemporaryPassword are required." });

        if (request.TemporaryPassword.Length < 6)
            return BadRequest(new { message = "TemporaryPassword must be at least 6 characters." });

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedPhone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();

        if (await _context.Users.AnyAsync(u => u.Email == normalizedEmail))
            return Conflict(new { message = "Email is already in use." });

        if (normalizedPhone != null && await _context.Users.AnyAsync(u => u.Phone == normalizedPhone))
            return Conflict(new { message = "Phone is already in use." });

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.TemporaryPassword);
        var user = WastePlatform.Domain.Entities.User.Create(
            normalizedEmail,
            passwordHash,
            request.FullName.Trim(),
            UserRole.Collector,
            normalizedPhone
        );

        var collector = new Collector
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            EnterpriseId = enterprise.Id,
            IsAvailable = request.IsAvailable,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.Collectors.Add(collector);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Collector account created successfully.",
            collector = new
            {
                collector.Id,
                Name = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                collector.IsAvailable,
                collector.CreatedAt,
                TaskCount = 0
            }
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCollector(Guid id, [FromBody] UpdateEnterpriseCollectorRequest request)
    {
        var enterprise = await GetCurrentEnterpriseAsync();
        if (enterprise == null)
            return Unauthorized(new { message = "Enterprise profile not found." });

        var collector = await _context.Collectors
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id && c.EnterpriseId == enterprise.Id);

        if (collector == null)
            return NotFound(new { message = "Collector not found." });

        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "FullName and Email are required." });

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedPhone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();

        var emailTaken = await _context.Users
            .AnyAsync(u => u.Email == normalizedEmail && u.Id != collector.UserId);
        if (emailTaken)
            return Conflict(new { message = "Email is already in use." });

        var phoneTaken = normalizedPhone != null
            && await _context.Users.AnyAsync(u => u.Phone == normalizedPhone && u.Id != collector.UserId);
        if (phoneTaken)
            return Conflict(new { message = "Phone is already in use." });

        _context.Entry(collector.User).Property(u => u.FullName).CurrentValue = request.FullName.Trim();
        _context.Entry(collector.User).Property(u => u.Email).CurrentValue = normalizedEmail;
        _context.Entry(collector.User).Property(u => u.Phone).CurrentValue = normalizedPhone;
        _context.Entry(collector).Property(c => c.IsAvailable).CurrentValue = request.IsAvailable;

        if (!string.IsNullOrWhiteSpace(request.TemporaryPassword))
        {
            if (request.TemporaryPassword.Length < 6)
                return BadRequest(new { message = "TemporaryPassword must be at least 6 characters." });

            _context.Entry(collector.User).Property(u => u.PasswordHash).CurrentValue = BCrypt.Net.BCrypt.HashPassword(request.TemporaryPassword);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Collector updated successfully.",
            collector = new
            {
                collector.Id,
                Name = request.FullName.Trim(),
                Email = normalizedEmail,
                Phone = normalizedPhone,
                collector.IsAvailable
            }
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCollector(Guid id)
    {
        var enterprise = await GetCurrentEnterpriseAsync();
        if (enterprise == null)
            return Unauthorized(new { message = "Enterprise profile not found." });

        var collector = await _context.Collectors
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id && c.EnterpriseId == enterprise.Id);

        if (collector == null)
            return NotFound(new { message = "Collector not found." });

        var hasActiveTasks = await _context.CollectionTasks
            .AnyAsync(t => t.CollectorId == collector.Id && t.Status != CollectionTaskStatus.Collected);

        if (hasActiveTasks)
            return BadRequest(new { message = "Cannot delete collector with active tasks." });

        var completedTasks = await _context.CollectionTasks
            .Where(t => t.CollectorId == collector.Id)
            .ToListAsync();

        foreach (var task in completedTasks)
        {
            _context.Entry(task).Property(t => t.CollectorId).CurrentValue = null;
        }

        _context.Users.Remove(collector.User);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Collector deleted successfully." });
    }
}

public class CreateEnterpriseCollectorRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string TemporaryPassword { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
}

public class UpdateEnterpriseCollectorRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? TemporaryPassword { get; set; }
    public bool IsAvailable { get; set; } = true;
}
