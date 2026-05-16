using WastePlatform.Application.Rewards.Queries;
using WastePlatform.Domain.Entities;

namespace WastePlatform.Application.Common.Interfaces;

public interface IRewardPointsRepository
{
    Task<RewardPoints> AddAsync(RewardPoints rewardPoints, CancellationToken cancellationToken = default);
    Task<(IEnumerable<RewardPoints> Points, int Total)> GetByCitizenIdAsync(Guid citizenId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalPointsByCitizenIdAsync(Guid citizenId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<(Guid CitizenId, string CitizenName, int TotalPoints, int ReportCount)>, int Total)> GetLeaderboardAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    // Thêm hàm này cho WRP-122
    Task<(IEnumerable<AreaLeaderboardDto> Areas, int Total)> GetAreaLeaderboardAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    
    // Hàm để tạo reward points khi collector hoàn thành task
    Task<RewardPoints> CreateRewardPointsAsync(
        Guid citizenId,
        Guid reportId,
        Guid taskId,
        Guid enterpriseId,
        int wasteCategoryId,
        string? reason = null,
        CancellationToken cancellationToken = default);
}
