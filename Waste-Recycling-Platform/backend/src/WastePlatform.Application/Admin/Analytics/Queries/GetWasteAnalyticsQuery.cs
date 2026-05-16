using MediatR;
using WastePlatform.Application.Admin.Analytics.DTOs;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Admin.Analytics.Queries;

public class GetWasteAnalyticsQuery : IRequest<WasteAnalyticsDto>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetWasteAnalyticsQueryHandler : IRequestHandler<GetWasteAnalyticsQuery, WasteAnalyticsDto>
{
    private readonly IAnalyticsRepository _analyticsRepository;

    public GetWasteAnalyticsQueryHandler(IAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    public async Task<WasteAnalyticsDto> Handle(GetWasteAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        return await _analyticsRepository.GetWasteAnalyticsAsync(startDate, endDate, cancellationToken);
    }
}
