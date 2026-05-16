using MediatR;
using WastePlatform.Application.Common.DTOs;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Application.Complaints.Queries;

public class GetCitizenComplaintsQuery : IRequest<ComplaintsResponseDto>
{
    public Guid CitizenId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public ComplaintStatus? Status { get; set; }
}

public class GetCitizenComplaintsQueryHandler : IRequestHandler<GetCitizenComplaintsQuery, ComplaintsResponseDto>
{
    private readonly IComplaintRepository _complaintRepository;

    public GetCitizenComplaintsQueryHandler(IComplaintRepository complaintRepository)
    {
        _complaintRepository = complaintRepository;
    }

    public async Task<ComplaintsResponseDto> Handle(GetCitizenComplaintsQuery request, CancellationToken cancellationToken)
    {
        var (complaints, total) = await _complaintRepository.GetByCitizenIdAsync(
            request.CitizenId, 
            request.Page, 
            request.PageSize,
            request.Status,
            cancellationToken);

        var items = complaints.Select(c => new ComplaintListDto
        {
            Id = c.Id,
            ReportId = c.ReportId,
            Content = c.Content,
            Status = c.Status,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            ResolvedAt = c.ResolvedAt,
            AdminResponse = c.AdminResponse,
            EnterpriseResponse = c.EnterpriseResponse,
            EnterpriseRespondedAt = c.EnterpriseRespondedAt,
            EnterpriseName = c.Enterprise?.CompanyName
        }).ToList();

        return new ComplaintsResponseDto
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            Total = total
        };
    }
}
