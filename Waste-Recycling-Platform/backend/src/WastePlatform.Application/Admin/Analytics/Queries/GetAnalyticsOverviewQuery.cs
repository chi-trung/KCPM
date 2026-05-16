using MediatR;
using WastePlatform.Application.Admin.Analytics.DTOs;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Admin.Analytics.Queries;

public class GetAnalyticsOverviewQuery : IRequest<AnalyticsOverviewDto>
{
}

public class GetAnalyticsOverviewQueryHandler : IRequestHandler<GetAnalyticsOverviewQuery, AnalyticsOverviewDto>
{
    private readonly IAnalyticsRepository _analyticsRepository;

    public GetAnalyticsOverviewQueryHandler(IAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    public async Task<AnalyticsOverviewDto> Handle(GetAnalyticsOverviewQuery request, CancellationToken cancellationToken)
    {
        return await _analyticsRepository.GetOverviewAsync(cancellationToken);
    }
}
