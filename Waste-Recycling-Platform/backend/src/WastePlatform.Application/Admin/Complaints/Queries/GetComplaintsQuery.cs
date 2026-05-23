using MediatR;
using WastePlatform.Application.Admin.Complaints.DTOs;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Application.Admin.Complaints.Queries;

public class GetComplaintsQuery : IRequest<(IEnumerable<ComplaintListDto> Complaints, int Total, int TotalPages)>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Status { get; set; }
    public string? SearchTerm { get; set; }
}

public class GetComplaintsQueryHandler : IRequestHandler<GetComplaintsQuery, (IEnumerable<ComplaintListDto> Complaints, int Total, int TotalPages)>
{
    private readonly IComplaintRepository _complaintRepository;

    public GetComplaintsQueryHandler(IComplaintRepository complaintRepository)
    {
        _complaintRepository = complaintRepository;
    }

    public async Task<(IEnumerable<ComplaintListDto> Complaints, int Total, int TotalPages)> Handle(GetComplaintsQuery request, CancellationToken cancellationToken)
    {
        ComplaintStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<ComplaintStatus>(request.Status, true, out var parsed))
        {
            statusEnum = parsed;
        }

        var (complaints, total) = await _complaintRepository.GetAllAsync(
            request.Page,
            request.PageSize,
            statusEnum,
            request.SearchTerm,
            cancellationToken);

        var dtoList = complaints.Select(c => new ComplaintListDto
        {
            Id = c.Id,
            CitizenName = c.Citizen?.FullName,
            Content = c.Content,
            Status = c.Status,
            CreatedAt = c.CreatedAt,
            HasReport = c.ReportId.HasValue,
            EnterpriseResponse = c.EnterpriseResponse,
            EnterpriseRespondedAt = c.EnterpriseRespondedAt,
            EscalationReason = c.EscalationReason,
            AdminResponse = c.AdminResponse
        }).ToList();

        int totalPages = (total + request.PageSize - 1) / request.PageSize;

        return (dtoList, total, totalPages);
    }
}
