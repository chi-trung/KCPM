using MediatR;
using WastePlatform.Application.Admin.Analytics.DTOs;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Admin.Analytics.Queries;

public class GetReportAnalyticsQuery : IRequest<ReportAnalyticsDto>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetReportAnalyticsQueryHandler : IRequestHandler<GetReportAnalyticsQuery, ReportAnalyticsDto>
{
    private readonly IAnalyticsRepository _analyticsRepository;

    public GetReportAnalyticsQueryHandler(IAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    public async Task<ReportAnalyticsDto> Handle(GetReportAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        return await _analyticsRepository.GetReportAnalyticsAsync(startDate, endDate, cancellationToken);
    }
}
