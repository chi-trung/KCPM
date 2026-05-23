using WastePlatform.Domain.Enums;

namespace WastePlatform.Application.Common.Interfaces;

public interface INotificationService
{
    // 1. Báo cáo mới được tạo
    Task NotifyReportCreatedAsync(Guid citizenId, Guid reportId, CancellationToken cancellationToken = default);
    
    // 2. Report được chấp nhận (Pending → Accepted)
    Task NotifyReportAcceptedAsync(Guid citizenId, Guid reportId, CancellationToken cancellationToken = default);
    
    // 3. Report được phân công (Accepted → Assigned)
    Task NotifyReportAssignedAsync(Guid citizenId, Guid reportId, string collectorName, CancellationToken cancellationToken = default);
    
    // 4. Collector đang đến
    Task NotifyCollectorOnTheWayAsync(Guid citizenId, Guid reportId, string collectorName, CancellationToken cancellationToken = default);
    
    // 5. Đã thu gom xong + nhận điểm (Assigned → Collected)
    Task NotifyReportCollectedAsync(Guid citizenId, Guid reportId, int points, CancellationToken cancellationToken = default);
    
    // 6. Report bị từ chối
    Task NotifyReportRejectedAsync(Guid citizenId, Guid reportId, string? reason, CancellationToken cancellationToken = default);
    
    // 7. Phản hồi/khiếu nại được trả lời
    Task NotifyComplaintRepliedAsync(Guid citizenId, Guid complaintId, string repliedBy, CancellationToken cancellationToken = default);

    // 8. Khiếu nại được chuyển lên Admin
    Task NotifyComplaintEscalatedAsync(Guid complaintId, CancellationToken cancellationToken = default);
}
