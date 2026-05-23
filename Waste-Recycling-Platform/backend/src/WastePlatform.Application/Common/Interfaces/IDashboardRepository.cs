using WastePlatform.Application.Admin.Dashboard.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace WastePlatform.Application.Common.Interfaces
{
    public interface IDashboardRepository
    {
        Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct);
    }
}
