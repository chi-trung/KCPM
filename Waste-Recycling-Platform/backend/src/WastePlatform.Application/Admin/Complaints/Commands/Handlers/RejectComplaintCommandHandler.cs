using MediatR;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Admin.Complaints.Commands;

namespace WastePlatform.Application.Admin.Complaints.Commands.Handlers;

public class RejectComplaintCommandHandler : IRequestHandler<RejectComplaintCommand, RejectComplaintResult>
{
    private readonly IComplaintRepository _complaintRepository;

    public RejectComplaintCommandHandler(IComplaintRepository complaintRepository)
    {
        _complaintRepository = complaintRepository;
    }

    public async Task<RejectComplaintResult> Handle(RejectComplaintCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AdminResponse))
        {
            throw new ArgumentException("Phản hồi không được để trống khi đóng khiếu nại.");
        }

        var complaint = await _complaintRepository.GetByIdAsync(request.ComplaintId, cancellationToken);

        if (complaint == null)
        {
            return new RejectComplaintResult
            {
                Success = false,
                Message = "Complaint not found",
                ComplaintId = request.ComplaintId
            };
        }

        complaint.Reject(request.AdminResponse);

        await _complaintRepository.SaveChangesAsync(cancellationToken);

        return new RejectComplaintResult
        {
            Success = true,
            Message = "Complaint rejected successfully",
            ComplaintId = request.ComplaintId
        };
    }
}
