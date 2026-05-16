using WastePlatform.Application.Admin.Analytics.DTOs;

namespace WastePlatform.Application.Common.Interfaces;

public interface IAnalyticsRepository
{
    Task<AnalyticsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<ReportAnalyticsDto> GetReportAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<ReportAnalyticsDto> GetEnterpriseReportAnalyticsAsync(Guid enterpriseId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<UserAnalyticsDto> GetUserAnalyticsAsync(CancellationToken cancellationToken = default);
    Task<WasteAnalyticsDto> GetWasteAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<AnalyticsSummaryDto> GetSummaryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
