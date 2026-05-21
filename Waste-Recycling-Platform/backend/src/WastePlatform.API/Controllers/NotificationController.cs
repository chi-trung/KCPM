using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Enums;

namespace WastePlatform.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Roles = "Citizen")]
public class NotificationController : ControllerBase
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationController(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    private Guid GetCitizenId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Guid.Empty;
        return userId;
    }

    /// <summary>
    /// Lấy danh sách thông báo của citizen hiện tại
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        try
        {
            var citizenId = GetCitizenId();
            if (citizenId == Guid.Empty)
                return Unauthorized(new { message = "Invalid or missing user ID" });

            if (page < 1 || pageSize < 1)
                return BadRequest(new { message = "Page and PageSize must be greater than 0" });

            NotificationStatus? filterStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<NotificationStatus>(status, true, out var parsedStatus))
            {
                filterStatus = parsedStatus;
            }

            var (notifications, total) = await _notificationRepository.GetByCitizenIdAsync(
                citizenId, page, pageSize, filterStatus);

            var unreadCount = await _notificationRepository.GetUnreadCountAsync(citizenId);

            var response = notifications.Select(n => new
            {
                n.Id,
                n.Type,
                n.Channel,
                n.Status,
                n.Title,
                n.Message,
                n.ActionUrl,
                n.RelatedEntityId,
                n.RelatedEntityType,
                n.CreatedAt,
                n.ReadAt
            });

            return Ok(new
            {
                message = "Notifications retrieved successfully",
                data = response,
                unreadCount,
                pagination = new
                {
                    page,
                    pageSize,
                    total,
                    totalPages = (int)Math.Ceiling((double)total / pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy số lượng thông báo chưa đọc
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var citizenId = GetCitizenId();
            if (citizenId == Guid.Empty)
                return Unauthorized(new { message = "Invalid or missing user ID" });

            var count = await _notificationRepository.GetUnreadCountAsync(citizenId);

            return Ok(new
            {
                message = "Unread count retrieved successfully",
                unreadCount = count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Đánh dấu một thông báo là đã đọc
    /// </summary>
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        try
        {
            var citizenId = GetCitizenId();
            if (citizenId == Guid.Empty)
                return Unauthorized(new { message = "Invalid or missing user ID" });

            var marked = await _notificationRepository.MarkAsReadAsync(id, citizenId);
            if (!marked)
            {
                return NotFound(new { message = "Notification not found" });
            }

            await _notificationRepository.SaveChangesAsync();

            return Ok(new
            {
                message = "Notification marked as read"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Đánh dấu tất cả thông báo là đã đọc
    /// </summary>
    [HttpPut("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var citizenId = GetCitizenId();
            if (citizenId == Guid.Empty)
                return Unauthorized(new { message = "Invalid or missing user ID" });

            await _notificationRepository.MarkAllAsReadAsync(citizenId);
            await _notificationRepository.SaveChangesAsync();

            return Ok(new
            {
                message = "All notifications marked as read"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}
