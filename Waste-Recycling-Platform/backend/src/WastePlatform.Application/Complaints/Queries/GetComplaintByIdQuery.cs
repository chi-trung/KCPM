using MediatR;
using WastePlatform.Application.Common.DTOs;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Complaints.Queries;

public class GetComplaintByIdQuery : IRequest<ComplaintDto?>
{
    public Guid Id { get; set; }
}

public class GetComplaintByIdQueryHandler : IRequestHandler<GetComplaintByIdQuery, ComplaintDto?>
{
    private readonly IComplaintRepository _complaintRepository;

    public GetComplaintByIdQueryHandler(IComplaintRepository complaintRepository)
    {
        _complaintRepository = complaintRepository;
    }

    public async Task<ComplaintDto?> Handle(GetComplaintByIdQuery request, CancellationToken cancellationToken)
    {
        var complaint = await _complaintRepository.GetByIdAsync(request.Id, cancellationToken);

        if (complaint == null)
            return null;

        return new ComplaintDto
        {
            Id = complaint.Id,
            CitizenId = complaint.CitizenId,
            CitizenName = complaint.Citizen?.FullName,
            ReportId = complaint.ReportId,
            Content = complaint.Content,
            Status = complaint.Status,
            AdminResponse = complaint.AdminResponse,
            CreatedAt = complaint.CreatedAt,
            ResolvedAt = complaint.ResolvedAt
        };
    }
}
