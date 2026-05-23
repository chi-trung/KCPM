using MediatR;
using WastePlatform.Application.Admin.Analytics.DTOs;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Admin.Analytics.Queries;

public class GetAnalyticsSummaryQuery : IRequest<AnalyticsSummaryDto>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetAnalyticsSummaryQueryHandler : IRequestHandler<GetAnalyticsSummaryQuery, AnalyticsSummaryDto>
{
    private readonly IAnalyticsRepository _analyticsRepository;

    public GetAnalyticsSummaryQueryHandler(IAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    public async Task<AnalyticsSummaryDto> Handle(GetAnalyticsSummaryQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        return await _analyticsRepository.GetSummaryAsync(startDate, endDate, cancellationToken);
    }
}
