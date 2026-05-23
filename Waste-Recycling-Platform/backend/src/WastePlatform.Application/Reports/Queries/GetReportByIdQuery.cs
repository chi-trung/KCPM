using MediatR;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Common.DTOs;

namespace WastePlatform.Application.Reports.Queries;

public class GetReportByIdQuery : IRequest<ReportDto?>
{
    public Guid Id { get; set; }
}

public class GetReportByIdQueryHandler : IRequestHandler<GetReportByIdQuery, ReportDto?>
{
    private readonly IReportRepository _repo;

    public GetReportByIdQueryHandler(IReportRepository repo)
    {
        _repo = repo;
    }

    public async Task<ReportDto?> Handle(GetReportByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (r == null) return null;

        return new ReportDto
        {
            Id = r.Id,
            CitizenId = r.CitizenId,
            CitizenName = r.Citizen?.FullName,
            WasteCategoryId = r.WasteCategoryId,
            CategoryName = r.WasteCategory?.Name,
            Description = r.Description,
            Latitude = r.Latitude,
            Longitude = r.Longitude,
            Address = r.Address,
            Status = r.Status,
            AiSuggestion = r.AiSuggestion,
            CreatedAt = r.CreatedAt,
            ImageUrls = r.Images.Select(i => i.ImageUrl).ToList(),
            RewardPoints = r.RewardPoints.Select(rp => new RewardPointsDto
            {
                Id = rp.Id,
                Points = rp.Points,
                Reason = rp.Reason,
                CreatedAt = rp.CreatedAt
            }).ToList()
        };
    }
}
