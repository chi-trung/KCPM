using MediatR;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Services;

namespace WastePlatform.Application.Complaints.Commands;

public class CitizenEscalateComplaintCommand : IRequest<CitizenEscalateResult>
{
    public Guid ComplaintId { get; set; }
    public Guid CitizenId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class CitizenEscalateResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? ComplaintId { get; set; }
}

public class CitizenEscalateComplaintCommandHandler : IRequestHandler<CitizenEscalateComplaintCommand, CitizenEscalateResult>
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly INotificationService _notificationService;

    public CitizenEscalateComplaintCommandHandler(
        IComplaintRepository complaintRepository,
        INotificationService notificationService)
    {
        _complaintRepository = complaintRepository;
        _notificationService = notificationService;
    }

    public async Task<CitizenEscalateResult> Handle(CitizenEscalateComplaintCommand request, CancellationToken cancellationToken)
    {
        var complaint = await _complaintRepository.GetByIdAsync(request.ComplaintId, cancellationToken);

        if (complaint == null)
        {
            return new CitizenEscalateResult
            {
                Success = false,
                Message = "Không tìm thấy khiếu nại"
            };
        }

        if (complaint.CitizenId != request.CitizenId)
        {
            return new CitizenEscalateResult
            {
                Success = false,
                Message = "Bạn không có quyền thực hiện hành động này"
            };
        }

        // Only allow escalation if complaint is in progress or resolved
        if (complaint.Status != ComplaintStatus.InProgress && complaint.Status != ComplaintStatus.Resolved)
        {
            return new CitizenEscalateResult
            {
                Success = false,
                Message = "Không thể chuyển lên Admin ở trạng thái hiện tại"
            };
        }

        complaint.EscalateToAdmin(request.Reason);
        await _complaintRepository.SaveChangesAsync(cancellationToken);

        // Notify admin (all admins with ManageComplaints permission)
        await _notificationService.NotifyComplaintEscalatedAsync(complaint.Id, cancellationToken);

        return new CitizenEscalateResult
        {
            Success = true,
            Message = "Đã chuyển khiếu nại lên Admin",
            ComplaintId = complaint.Id
        };
    }
}
