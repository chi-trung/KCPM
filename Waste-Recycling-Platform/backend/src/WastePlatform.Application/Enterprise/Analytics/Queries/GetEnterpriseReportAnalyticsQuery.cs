using MediatR;
using WastePlatform.Application.Admin.Analytics.DTOs;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Enterprise.Analytics.Queries;

public class GetEnterpriseReportAnalyticsQuery : IRequest<ReportAnalyticsDto>
{
    public Guid EnterpriseId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetEnterpriseReportAnalyticsQueryHandler : IRequestHandler<GetEnterpriseReportAnalyticsQuery, ReportAnalyticsDto>
{
    private readonly IAnalyticsRepository _analyticsRepository;

    public GetEnterpriseReportAnalyticsQueryHandler(IAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    public async Task<ReportAnalyticsDto> Handle(GetEnterpriseReportAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        return await _analyticsRepository.GetEnterpriseReportAnalyticsAsync(
            request.EnterpriseId,
            startDate,
            endDate,
            cancellationToken);
    }
}
