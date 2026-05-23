using MediatR;
using WastePlatform.Application.Admin.Analytics.DTOs;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Public.Analytics.Queries;

public class GetPublicReportAnalyticsQuery : IRequest<ReportAnalyticsDto>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetPublicReportAnalyticsQueryHandler : IRequestHandler<GetPublicReportAnalyticsQuery, ReportAnalyticsDto>
{
    private readonly IAnalyticsRepository _analyticsRepository;

    public GetPublicReportAnalyticsQueryHandler(IAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    public async Task<ReportAnalyticsDto> Handle(GetPublicReportAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-3); // Public sees last 3 months
        var endDate = request.EndDate ?? DateTime.UtcNow;

        return await _analyticsRepository.GetReportAnalyticsAsync(startDate, endDate, cancellationToken);
    }
}
