using MediatR;
using WastePlatform.Application.Admin.Analytics.DTOs;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Admin.Analytics.Queries;

public class GetUserAnalyticsQuery : IRequest<UserAnalyticsDto>
{
}

public class GetUserAnalyticsQueryHandler : IRequestHandler<GetUserAnalyticsQuery, UserAnalyticsDto>
{
    private readonly IAnalyticsRepository _analyticsRepository;

    public GetUserAnalyticsQueryHandler(IAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    public async Task<UserAnalyticsDto> Handle(GetUserAnalyticsQuery request, CancellationToken cancellationToken)
    {
        return await _analyticsRepository.GetUserAnalyticsAsync(cancellationToken);
    }
}
