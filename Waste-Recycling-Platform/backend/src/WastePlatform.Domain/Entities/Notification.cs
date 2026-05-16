using WastePlatform.Domain.Enums;

namespace WastePlatform.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? CitizenId { get; set; }                   // Người nhận thông báo (null nếu là thông báo chung cho admin)
    public NotificationType Type { get; set; }                // Loại thông báo
    public NotificationChannel Channel { get; set; }        // Kênh gửi
    public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
    public string Title { get; set; } = string.Empty;       // Tiêu đề
    public string Message { get; set; } = string.Empty;     // Nội dung
    public string? ActionUrl { get; set; }                 // Link khi click (optional)
    public Guid? RelatedEntityId { get; set; }               // ID của entity liên quan (Report/Complaint ID)
    public string? RelatedEntityType { get; set; }         // Loại entity: "Report", "Complaint"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    // Navigation
    public virtual User Citizen { get; set; } = null!;
}
