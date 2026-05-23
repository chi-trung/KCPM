using MediatR;
using WastePlatform.Application.Admin.Complaints.DTOs;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Application.Complaints.Queries;

public class GetEnterpriseComplaintsQuery : IRequest<(IEnumerable<ComplaintListDto> Complaints, int Total, int TotalPages)>
{
    public Guid EnterpriseId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public ComplaintStatus? Status { get; set; }
}

public class GetEnterpriseComplaintsQueryHandler : IRequestHandler<GetEnterpriseComplaintsQuery, (IEnumerable<ComplaintListDto> Complaints, int Total, int TotalPages)>
{
    private readonly IComplaintRepository _complaintRepository;

    public GetEnterpriseComplaintsQueryHandler(IComplaintRepository complaintRepository)
    {
        _complaintRepository = complaintRepository;
    }

    public async Task<(IEnumerable<ComplaintListDto> Complaints, int Total, int TotalPages)> Handle(GetEnterpriseComplaintsQuery request, CancellationToken cancellationToken)
    {
        var (complaints, total) = await _complaintRepository.GetByEnterpriseIdAsync(
            request.EnterpriseId,
            request.Page,
            request.PageSize,
            request.Status,
            cancellationToken);

        var dtoList = complaints.Select(c => new ComplaintListDto
        {
            Id = c.Id,
            CitizenName = c.Citizen?.FullName ?? "Unknown",
            Content = c.Content,
            Status = c.Status,
            CreatedAt = c.CreatedAt,
            HasReport = c.ReportId.HasValue,
            EnterpriseResponse = c.EnterpriseResponse,
            EnterpriseRespondedAt = c.EnterpriseRespondedAt
        }).ToList();

        int totalPages = (total + request.PageSize - 1) / request.PageSize;

        return (dtoList, total, totalPages);
    }
}
