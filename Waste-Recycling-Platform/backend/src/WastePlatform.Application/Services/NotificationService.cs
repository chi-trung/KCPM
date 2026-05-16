using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IRealTimeNotifier _realTimeNotifier;

    public NotificationService(INotificationRepository notificationRepository, IRealTimeNotifier realTimeNotifier)
    {
        _notificationRepository = notificationRepository;
        _realTimeNotifier = realTimeNotifier;
    }

    private async Task PushNotificationAsync(Guid citizenId, Notification notification)
    {
        await _realTimeNotifier.NotifyUserAsync(citizenId, "NewNotification", new
        {
            notification.Id,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.ActionUrl,
            notification.RelatedEntityId,
            notification.RelatedEntityType,
            notification.CreatedAt
        });
    }

    public async Task NotifyReportCreatedAsync(Guid citizenId, Guid reportId, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            CitizenId = citizenId,
            Type = NotificationType.ReportCreated,
            Channel = NotificationChannel.InApp,
            Title = "Báo cáo đã gửi thành công",
            Message = $"Báo cáo #{reportId.ToString()[..8]} của bạn đã được gửi và đang chờ xác nhận.",
            RelatedEntityId = reportId,
            RelatedEntityType = "Report",
            ActionUrl = $"/citizen/reports/{reportId}"
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);

        // Push real-time notification
        await PushNotificationAsync(citizenId, notification);
    }

    public async Task NotifyReportAcceptedAsync(Guid citizenId, Guid reportId, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            CitizenId = citizenId,
            Type = NotificationType.ReportAccepted,
            Channel = NotificationChannel.Both,  // In-app + Push
            Title = "Báo cáo đã được xác nhận",
            Message = $"Báo cáo #{reportId.ToString()[..8]} đã được xác nhận và đang chờ phân công thu gom.",
            RelatedEntityId = reportId,
            RelatedEntityType = "Report",
            ActionUrl = $"/citizen/reports/{reportId}"
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);

        // Push real-time notification
        await PushNotificationAsync(citizenId, notification);
    }

    public async Task NotifyReportAssignedAsync(Guid citizenId, Guid reportId, string collectorName, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            CitizenId = citizenId,
            Type = NotificationType.ReportAssigned,
            Channel = NotificationChannel.Both,  // In-app + Push
            Title = "Đã phân công người thu gom",
            Message = $"Collector {collectorName} sẽ đến thu gom báo cáo #{reportId.ToString()[..8]}.",
            RelatedEntityId = reportId,
            RelatedEntityType = "Report",
            ActionUrl = $"/citizen/reports/{reportId}"
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);

        // Push real-time notification (Both channel)
        await PushNotificationAsync(citizenId, notification);
    }

    public async Task NotifyCollectorOnTheWayAsync(Guid citizenId, Guid reportId, string collectorName, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            CitizenId = citizenId,
            Type = NotificationType.CollectorOnTheWay,
            Channel = NotificationChannel.Push,  // Chỉ Push
            Title = "Collector đang trên đường",
            Message = $"{collectorName} đang trên đường đến địa điểm thu gom của bạn.",
            RelatedEntityId = reportId,
            RelatedEntityType = "Report",
            ActionUrl = $"/citizen/reports/{reportId}"
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);

        // Push real-time notification (Both channel)
        await PushNotificationAsync(citizenId, notification);
    }

    public async Task NotifyReportCollectedAsync(Guid citizenId, Guid reportId, int points, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            CitizenId = citizenId,
            Type = NotificationType.ReportCollected,
            Channel = NotificationChannel.Both,  // In-app + Push
            Title = "Đã thu gom thành công!",
            Message = $"Báo cáo #{reportId.ToString()[..8]} đã được thu gom. Bạn nhận được +{points} điểm thưởng!",
            RelatedEntityId = reportId,
            RelatedEntityType = "Report",
            ActionUrl = $"/citizen/reports/{reportId}"
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);

        // Push real-time notification
        await PushNotificationAsync(citizenId, notification);
    }

    public async Task NotifyReportRejectedAsync(Guid citizenId, Guid reportId, string? reason, CancellationToken cancellationToken = default)
    {
        var message = string.IsNullOrEmpty(reason)
            ? $"Báo cáo #{reportId.ToString()[..8]} không được chấp nhận."
            : $"Báo cáo #{reportId.ToString()[..8]} không được chấp nhận. Lý do: {reason}";

        var notification = new Notification
        {
            CitizenId = citizenId,
            Type = NotificationType.ReportRejected,
            Channel = NotificationChannel.InApp,
            Title = "Báo cáo bị từ chối",
            Message = message,
            RelatedEntityId = reportId,
            RelatedEntityType = "Report",
            ActionUrl = $"/citizen/reports/{reportId}"
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);

        // Push real-time notification
        await PushNotificationAsync(citizenId, notification);
    }

    public async Task NotifyComplaintRepliedAsync(Guid citizenId, Guid complaintId, string repliedBy, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            CitizenId = citizenId,
            Type = NotificationType.ComplaintReplied,
            Channel = NotificationChannel.InApp,
            Title = "Phản hồi đã được trả lời",
            Message = $"{repliedBy} đã phản hồi ý kiến của bạn.",
            RelatedEntityId = complaintId,
            RelatedEntityType = "Complaint",
            ActionUrl = $"/citizen/complaints/{complaintId}"
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);

        await PushNotificationAsync(citizenId, notification);
    }

    public async Task NotifyComplaintEscalatedAsync(Guid complaintId, CancellationToken cancellationToken = default)
    {
        // Notify all admins about the escalation
        var notification = new Notification
        {
            Type = NotificationType.ComplaintEscalated,
            Channel = NotificationChannel.InApp,
            Title = "Khiếu nại được chuyển lên Admin",
            Message = "Một khiếu nại đã được Citizen chuyển lên Admin xử lý.",
            RelatedEntityId = complaintId,
            RelatedEntityType = "Complaint",
            ActionUrl = $"/admin/complaints/{complaintId}"
        };

        // For now, save to system - admin notification logic would need admin user IDs
        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);
    }
}
