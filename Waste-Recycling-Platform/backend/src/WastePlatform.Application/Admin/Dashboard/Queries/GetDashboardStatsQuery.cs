using MediatR;
using WastePlatform.Application.Admin.Dashboard.DTOs;
using WastePlatform.Application.Common.Interfaces; // Only use Interface


namespace WastePlatform.Application.Admin.Dashboard.Queries
{
    public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

    public class GetDashboardStatsHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
    {
        private readonly IDashboardRepository _dashboardRepository;

        // Inject Interface instead of DbContext
        public GetDashboardStatsHandler(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken ct)
        {
            // Delegate SQL query to Infrastructure
            return await _dashboardRepository.GetStatsAsync(ct);
        }
    }
}