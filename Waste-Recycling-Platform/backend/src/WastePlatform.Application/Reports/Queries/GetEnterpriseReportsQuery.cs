using MediatR;
using WastePlatform.Application.Common.DTOs;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Application.Reports.Queries;

/// <summary>
/// Lấy danh sách báo cáo mà doanh nghiệp có thể xử lý
/// (dựa trên các loại rác mà doanh nghiệp quản lý)
/// </summary>
public class GetEnterpriseReportsQuery : IRequest<(IEnumerable<ReportListDto> Reports, int Total, int TotalPages)>
{
    public Guid EnterpriseId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Status { get; set; }
}

public class GetEnterpriseReportsQueryHandler : IRequestHandler<GetEnterpriseReportsQuery, (IEnumerable<ReportListDto> Reports, int Total, int TotalPages)>
{
    private readonly IReportRepository _repo;

    public GetEnterpriseReportsQueryHandler(IReportRepository repo)
    {
        _repo = repo;
    }

    public async Task<(IEnumerable<ReportListDto> Reports, int Total, int TotalPages)> Handle(GetEnterpriseReportsQuery request, CancellationToken cancellationToken)
    {
        ReportStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<ReportStatus>(request.Status, true, out var parsed))
        {
            statusEnum = parsed;
        }

        var (reports, total) = await _repo.GetEnterpriseReportsAsync(request.EnterpriseId, request.Page, request.PageSize, statusEnum, cancellationToken);

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
