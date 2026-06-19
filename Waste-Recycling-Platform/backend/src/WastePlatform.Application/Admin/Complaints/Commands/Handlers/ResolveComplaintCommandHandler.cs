using MediatR;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Admin.Complaints.Commands;
// SignalR notifications are sent from API layer/controller

namespace WastePlatform.Application.Admin.Complaints.Commands.Handlers;

public class ResolveComplaintCommandHandler : IRequestHandler<ResolveComplaintCommand, ResolveComplaintResult>
{
    private readonly IComplaintRepository _complaintRepository;

    public ResolveComplaintCommandHandler(IComplaintRepository complaintRepository)
    {
        _complaintRepository = complaintRepository;
    }

    public async Task<ResolveComplaintResult> Handle(ResolveComplaintCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AdminResponse))
        {
            throw new ArgumentException("Phản hồi không được để trống khi đóng khiếu nại.");
        }

        var complaint = await _complaintRepository.GetByIdAsync(request.ComplaintId, cancellationToken);

        if (complaint == null)
        {
            return new ResolveComplaintResult
            {
                Success = false,
                Message = "Complaint not found",
                ComplaintId = request.ComplaintId
            };
        }

        complaint.Resolve(request.AdminResponse);

        await _complaintRepository.SaveChangesAsync(cancellationToken);



        return new ResolveComplaintResult
        {
            Success = true,
            Message = "Complaint resolved successfully",
            ComplaintId = request.ComplaintId
        };
    }
}
