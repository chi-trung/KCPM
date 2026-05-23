using MediatR;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Rewards.Queries;

public class GetAreaLeaderboardQuery : IRequest<AreaLeaderboardResponseDto>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class AreaLeaderboardResponseDto
{
    public IEnumerable<AreaLeaderboardDto> Leaderboard { get; set; } = new List<AreaLeaderboardDto>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
}

// Đây là Class DTO dùng chung cho cả Query và Repository
public class AreaLeaderboardDto
{
    public string Area { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public int TotalReports { get; set; }
    public int Participants { get; set; }
}

public class GetAreaLeaderboardQueryHandler : IRequestHandler<GetAreaLeaderboardQuery, AreaLeaderboardResponseDto>
{
    private readonly IRewardPointsRepository _repository;

    public GetAreaLeaderboardQueryHandler(IRewardPointsRepository repository)
    {
        _repository = repository;
    }

    public async Task<AreaLeaderboardResponseDto> Handle(GetAreaLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var (areas, total) = await _repository.GetAreaLeaderboardAsync(request.Page, request.PageSize, cancellationToken);

        return new AreaLeaderboardResponseDto
        {
            Leaderboard = areas,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
