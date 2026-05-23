using Microsoft.EntityFrameworkCore;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Infrastructure.Persistence.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly WastePlatformDbContext _context;

    public NotificationRepository(WastePlatformDbContext context)
    {
        _context = context;
    }

    public async Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _context.Notifications.AddAsync(notification, cancellationToken);
        return notification;
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<(IEnumerable<Notification> Notifications, int Total)> GetByCitizenIdAsync(
        Guid citizenId, int page, int pageSize, NotificationStatus? status, CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .Where(n => n.CitizenId == citizenId)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(n => n.Status == status.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (notifications, total);
    }

    public async Task<int> GetUnreadCountAsync(Guid citizenId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .CountAsync(n => n.CitizenId == citizenId && n.Status == NotificationStatus.Unread, cancellationToken);
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid citizenId, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.CitizenId == citizenId, cancellationToken);

        if (notification == null)
        {
            return false;
        }

        if (notification.Status == NotificationStatus.Unread)
        {
            notification.Status = NotificationStatus.Read;
            notification.ReadAt = DateTime.UtcNow;
            _context.Notifications.Update(notification);
        }

        return true;
    }

    public async Task MarkAllAsReadAsync(Guid citizenId, CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.CitizenId == citizenId && n.Status == NotificationStatus.Unread)
            .ToListAsync(cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.Status = NotificationStatus.Read;
            notification.ReadAt = DateTime.UtcNow;
        }

        _context.Notifications.UpdateRange(unreadNotifications);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
