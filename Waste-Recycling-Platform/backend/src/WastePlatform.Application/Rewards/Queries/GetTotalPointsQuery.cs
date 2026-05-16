using MediatR;
using WastePlatform.Application.Common.DTOs;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Rewards.Queries;

public class GetTotalPointsQuery : IRequest<TotalRewardsDto>
{
    public Guid CitizenId { get; set; }
}

public class GetTotalPointsQueryHandler : IRequestHandler<GetTotalPointsQuery, TotalRewardsDto>
{
    private readonly IRewardPointsRepository _rewardRepository;

    public GetTotalPointsQueryHandler(IRewardPointsRepository rewardRepository)
    {
        _rewardRepository = rewardRepository;
    }

    public async Task<TotalRewardsDto> Handle(GetTotalPointsQuery request, CancellationToken cancellationToken)
    {
        var totalPoints = await _rewardRepository.GetTotalPointsByCitizenIdAsync(
            request.CitizenId,
            cancellationToken);

        return new TotalRewardsDto
        {
            TotalPoints = totalPoints,
            LastUpdated = DateTime.UtcNow // This could be enhanced to track the actual last update time
        };
    }
}
