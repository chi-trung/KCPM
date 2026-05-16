using MediatR;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Rewards.Queries;

public class GetLeaderboardQuery : IRequest<LeaderboardResponseDto>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class LeaderboardResponseDto
{
    public IEnumerable<LeaderboardItemDto> Leaderboard { get; set; } = new List<LeaderboardItemDto>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
}

public class LeaderboardItemDto
{
    public Guid CitizenId { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public int ReportCount { get; set; }
}

public class GetLeaderboardQueryHandler : IRequestHandler<GetLeaderboardQuery, LeaderboardResponseDto>
{
    private readonly IRewardPointsRepository _repository;

    public GetLeaderboardQueryHandler(IRewardPointsRepository repository)
    {
        _repository = repository;
    }

    public async Task<LeaderboardResponseDto> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        // Gọi hàm siêu xịn mà ông đã viết sẵn trong Repository
        var (points, total) = await _repository.GetLeaderboardAsync(request.Page, request.PageSize, cancellationToken);

        var items = points.Select(p => new LeaderboardItemDto
        {
            CitizenId = p.CitizenId,
            CitizenName = p.CitizenName,
            TotalPoints = p.TotalPoints,
            ReportCount = p.ReportCount
        });

        return new LeaderboardResponseDto
        {
            Leaderboard = items,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}