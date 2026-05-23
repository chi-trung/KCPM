namespace WastePlatform.Domain.Enums;

public enum NotificationType
{
    ReportCreated,      // 1. Báo cáo mới được tạo
    ReportAccepted,     // 2. Pending → Accepted
    ReportAssigned,     // 3. Accepted → Assigned
    CollectorOnTheWay,  // 4. Collector bắt đầu đi
    ReportCollected,    // 5. Đã thu gom + nhận điểm
    ReportRejected,     // 6. Bị từ chối
    ComplaintReplied,   // 7. Phản hồi được trả lời
    ComplaintEscalated  // 8. Khiếu nại được chuyển lên Admin
}

public enum NotificationChannel
{
    InApp,      // Chỉ hiển thị trong app
    Push,       // Push notification
    Both        // Cả 2
}

public enum NotificationStatus
{
    Unread,
    Read
}
