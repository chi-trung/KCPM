using MediatR;
using WastePlatform.Application.Admin.Complaints.DTOs;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Admin.Complaints.Queries;

public class GetComplaintDetailQuery : IRequest<ComplaintDto?>
{
    public Guid ComplaintId { get; set; }
}

public class GetComplaintDetailQueryHandler : IRequestHandler<GetComplaintDetailQuery, ComplaintDto?>
{
    private readonly IComplaintRepository _complaintRepository;

    public GetComplaintDetailQueryHandler(IComplaintRepository complaintRepository)
    {
        _complaintRepository = complaintRepository;
    }

    public async Task<ComplaintDto?> Handle(GetComplaintDetailQuery request, CancellationToken cancellationToken)
    {
        var complaint = await _complaintRepository.GetByIdAsync(request.ComplaintId, cancellationToken);

        if (complaint == null)
            return null;

        return new ComplaintDto
        {
            Id = complaint.Id,
            CitizenId = complaint.CitizenId,
            CitizenName = complaint.Citizen?.FullName,
            ReportId = complaint.ReportId,
            ReportAddress = complaint.WasteReport?.Address,
            Content = complaint.Content,
            Status = complaint.Status,
            AdminResponse = complaint.AdminResponse,
            CreatedAt = complaint.CreatedAt,
            ResolvedAt = complaint.ResolvedAt
        };
    }
}
