using Microsoft.EntityFrameworkCore;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Rewards.Queries;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums; // Thêm dòng này để gọi Enum Role
using WastePlatform.Infrastructure.Persistence;

namespace WastePlatform.Infrastructure.Persistence.Repositories;

public class RewardPointsRepository : IRewardPointsRepository
{
    private readonly WastePlatformDbContext _context;

    public RewardPointsRepository(WastePlatformDbContext context)
    {
        _context = context;
    }

    public async Task<RewardPoints> AddAsync(RewardPoints rewardPoints, CancellationToken cancellationToken = default)
    {
        await _context.RewardPoints.AddAsync(rewardPoints, cancellationToken);
        return rewardPoints;
    }

    public async Task<(IEnumerable<RewardPoints> Points, int Total)> GetByCitizenIdAsync(Guid citizenId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.RewardPoints
            .Where(rp => rp.CitizenId == citizenId)
            .Include(rp => rp.WasteReport);

        var total = await query.CountAsync(cancellationToken);

        var points = await query
            .OrderByDescending(rp => rp.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (points, total);
    }

    public async Task<int> GetTotalPointsByCitizenIdAsync(Guid citizenId, CancellationToken cancellationToken = default)
    {
        return await _context.RewardPoints
            .Where(rp => rp.CitizenId == citizenId)
            .SumAsync(rp => rp.Points, cancellationToken);
    }

    public async Task<(IEnumerable<(Guid CitizenId, string CitizenName, int TotalPoints, int ReportCount)>, int Total)> GetLeaderboardAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // HƯỚNG 2: Bắt đầu từ bảng Users, lấy tất cả những người là Citizen
        var leaderboardQuery = _context.Users
            // Lọc ra các user có quyền là người dân
            .Where(u => u.Role.ToString() == "Citizen")
            .Select(u => new
            {
                CitizenId = u.Id,
                CitizenName = u.FullName,
                
                // Tính tổng điểm từ bảng RewardPoints (nếu null thì gán = 0)

                TotalPoints = _context.RewardPoints
                                .Where(rp => rp.CitizenId == u.Id)
                                .Sum(rp => (int?)rp.Points) ?? 0,
                
                // Đếm số lần có ghi nhận báo cáo (ReportId khác null)
                ReportCount = _context.RewardPoints
                                .Count(rp => rp.CitizenId == u.Id && rp.ReportId != null)
            })
            // Sắp xếp: Ưu tiên Tổng Điểm cao nhất -> Nếu bằng điểm thì ai Báo cáo nhiều hơn sẽ xếp trên
            .OrderByDescending(x => x.TotalPoints)
            .ThenByDescending(x => x.ReportCount);

        var total = await leaderboardQuery.CountAsync(cancellationToken);

        var leaderboard = await leaderboardQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Convert anonymous objects to tuples
        var result = leaderboard
            .Select(x => (x.CitizenId, x.CitizenName, x.TotalPoints, x.ReportCount))
            .ToList();

        return (result, total);
    }

    public async Task<(IEnumerable<AreaLeaderboardDto> Areas, int Total)> GetAreaLeaderboardAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // Gom nhóm người dân theo Quận/Huyện (District)
        var areaQuery = _context.Users
            .Where(u => u.Role.ToString() == "Citizen" && !string.IsNullOrEmpty(u.District))
            .Select(u => new
            {
                District = u.District,
                UserId = u.Id,
                Points = _context.RewardPoints.Where(rp => rp.CitizenId == u.Id).Sum(rp => (int?)rp.Points) ?? 0,
                Reports = _context.RewardPoints.Count(rp => rp.CitizenId == u.Id && rp.ReportId != null)
            })
            .GroupBy(x => x.District)
            .Select(g => new AreaLeaderboardDto
            {
                Area = g.Key!,
                TotalPoints = g.Sum(x => x.Points),
                TotalReports = g.Sum(x => x.Reports),
                Participants = g.Count() // Tổng số người dân trong khu vực
            })
            .OrderByDescending(x => x.TotalPoints);

        var total = await areaQuery.CountAsync(cancellationToken);

        var areas = await areaQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (areas, total);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<RewardPoints> CreateRewardPointsAsync(
        Guid citizenId,
        Guid reportId,
        Guid taskId,
        Guid enterpriseId,
        int wasteCategoryId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Tìm quy tắc thưởng cho enterprise và loại rác
            var rewardRule = await _context.RewardRules
                .FirstOrDefaultAsync(
                    r => r.EnterpriseId == enterpriseId &&
                         r.WasteCategoryId == wasteCategoryId &&
                         r.IsActive,
                    cancellationToken);

            // Nếu không có quy tắc riêng, sử dụng điểm mặc định
            var points = rewardRule?.PointsPerReport ?? 10;
            
            // Thêm điểm bonus nếu có
            if (rewardRule?.BonusQuality > 0)
            {
                points += rewardRule.BonusQuality;
            }

            // Tạo idempotency key để tránh trùng lặp
            var idempotencyKey = $"task_{taskId}_{reportId}";

            // Kiểm tra xem đã có reward cho task này rồi không
            var existingReward = await _context.RewardPoints
                .FirstOrDefaultAsync(
                    rp => rp.IdempotencyKey == idempotencyKey,
                    cancellationToken);

            if (existingReward != null)
            {
                // Đã tạo rồi, trả về kết quả
                return existingReward;
            }

            // Tạo bản ghi reward points mới
            var rewardPoints = new RewardPoints
            {
                Id = Guid.NewGuid(),
                CitizenId = citizenId,
                ReportId = reportId,
                IdempotencyKey = idempotencyKey,
                Points = points,
                Reason = reason ?? "Báo cáo và thu gom rác",
                CreatedAt = DateTime.UtcNow
            };

            // Lưu vào database
            await _context.RewardPoints.AddAsync(rewardPoints, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return rewardPoints;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi tạo điểm thưởng: {ex.Message}", ex);
        }
    }
}