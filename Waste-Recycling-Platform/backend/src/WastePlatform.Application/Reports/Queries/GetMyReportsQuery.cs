using MediatR;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Common.DTOs;

namespace WastePlatform.Application.Reports.Queries;

public class GetMyReportsQuery : IRequest<(IEnumerable<ReportListDto> Reports, int Total, int TotalPages)>
{
    public Guid UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetMyReportsQueryHandler : IRequestHandler<GetMyReportsQuery, (IEnumerable<ReportListDto> Reports, int Total, int TotalPages)>
{
    private readonly IReportRepository _reportRepository;

    public GetMyReportsQueryHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<(IEnumerable<ReportListDto> Reports, int Total, int TotalPages)> Handle(GetMyReportsQuery request, CancellationToken cancellationToken)
    {
        var (reports, total) = await _reportRepository.GetByCitizenIdAsync(request.UserId, request.Page, request.PageSize, cancellationToken);

        var dtoList = reports.Select(r => new ReportListDto
        {
            Id = r.Id,
            CitizenName = r.Citizen?.FullName,
            CategoryName = r.WasteCategory?.Name,
            Status = r.Status,
            Address = r.Address,
            CreatedAt = r.CreatedAt,
            ImageCount = r.Images.Count
        }).ToList();

        int totalPages = (total + request.PageSize - 1) / request.PageSize;

        return (dtoList, total, totalPages);
    }
}
