using MediatR;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Application.Complaints.Commands;

public class EnterpriseRespondToComplaintCommand : IRequest<EnterpriseRespondResult>
{
    public Guid EnterpriseId { get; set; }
    public string EnterpriseName { get; set; } = null!;
    public Guid ComplaintId { get; set; }
    public string? Response { get; set; }
    public bool ResolveImmediately { get; set; } = false;
    public bool EscalateToAdmin { get; set; } = false;
}

public class EnterpriseRespondResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid ComplaintId { get; set; }
    public string? NewStatus { get; set; }
}

public class EnterpriseRespondToComplaintCommandHandler : IRequestHandler<EnterpriseRespondToComplaintCommand, EnterpriseRespondResult>
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly INotificationService _notificationService;

    public EnterpriseRespondToComplaintCommandHandler(IComplaintRepository complaintRepository, INotificationService notificationService)
    {
        _complaintRepository = complaintRepository;
        _notificationService = notificationService;
    }

    public async Task<EnterpriseRespondResult> Handle(EnterpriseRespondToComplaintCommand request, CancellationToken cancellationToken)
    {
        var complaint = await _complaintRepository.GetByIdAsync(request.ComplaintId, cancellationToken);
        
        if (complaint == null)
        {
            return new EnterpriseRespondResult 
            { 
                Success = false, 
                Message = "Complaint not found",
                ComplaintId = request.ComplaintId
            };
        }

        // Verify enterprise owns this complaint
        if (complaint.EnterpriseId != request.EnterpriseId)
        {
            return new EnterpriseRespondResult 
            { 
                Success = false, 
                Message = "You are not authorized to respond to this complaint",
                ComplaintId = request.ComplaintId
            };
        }

        // Check if complaint can be responded to
        if (complaint.Status != ComplaintStatus.Open && complaint.Status != ComplaintStatus.InProgress)
        {
            return new EnterpriseRespondResult 
            { 
                Success = false, 
                Message = $"Cannot respond to complaint with status '{complaint.Status}'",
                ComplaintId = request.ComplaintId
            };
        }

        if (!request.EscalateToAdmin && string.IsNullOrWhiteSpace(request.Response))
        {
            throw new ArgumentException("Phản hồi không được để trống.");
        }

        var response = request.Response;

        // Handle escalation to admin
        if (request.EscalateToAdmin)
        {
            complaint.EscalateToAdmin(response);
            await _complaintRepository.SaveChangesAsync(cancellationToken);
            
            // Notify citizen
            await _notificationService.NotifyComplaintRepliedAsync(complaint.CitizenId, complaint.Id, request.EnterpriseName, cancellationToken);
            
            return new EnterpriseRespondResult 
            { 
                Success = true, 
                Message = "Complaint escalated to admin for review",
                ComplaintId = complaint.Id,
                NewStatus = complaint.Status.ToString()
            };
        }

        // Handle immediate resolution by enterprise
        if (request.ResolveImmediately)
        {
            complaint.ResolveByEnterprise(response);
            await _complaintRepository.SaveChangesAsync(cancellationToken);
            
            // Notify citizen
            await _notificationService.NotifyComplaintRepliedAsync(complaint.CitizenId, complaint.Id, request.EnterpriseName, cancellationToken);
            
            return new EnterpriseRespondResult 
            { 
                Success = true, 
                Message = "Complaint resolved successfully",
                ComplaintId = complaint.Id,
                NewStatus = complaint.Status.ToString()
            };
        }

        // Just add response without resolving
        if (string.IsNullOrWhiteSpace(response))
        {
            throw new ArgumentException("Phản hồi không được để trống.");
        }

        complaint.AddEnterpriseResponse(response);
        await _complaintRepository.SaveChangesAsync(cancellationToken);
        
        // Notify citizen
        await _notificationService.NotifyComplaintRepliedAsync(complaint.CitizenId, complaint.Id, request.EnterpriseName, cancellationToken);
        
        return new EnterpriseRespondResult 
        { 
            Success = true, 
            Message = "Response added successfully",
            ComplaintId = complaint.Id,
            NewStatus = complaint.Status.ToString()
        };
    }
}
