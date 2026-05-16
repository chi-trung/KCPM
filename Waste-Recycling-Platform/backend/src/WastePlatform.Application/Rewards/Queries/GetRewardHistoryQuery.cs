using MediatR;
using WastePlatform.Application.Common.DTOs;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Rewards.Queries;

public class GetRewardHistoryQuery : IRequest<RewardHistoryResponseDto>
{
    public Guid CitizenId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetRewardHistoryQueryHandler : IRequestHandler<GetRewardHistoryQuery, RewardHistoryResponseDto>
{
    private readonly IRewardPointsRepository _rewardRepository;

    public GetRewardHistoryQueryHandler(IRewardPointsRepository rewardRepository)
    {
        _rewardRepository = rewardRepository;
    }

    public async Task<RewardHistoryResponseDto> Handle(GetRewardHistoryQuery request, CancellationToken cancellationToken)
    {
        var (points, total) = await _rewardRepository.GetByCitizenIdAsync(
            request.CitizenId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = points.Select(p => new RewardHistoryDto
        {
            Id = p.Id,
            Points = p.Points,
            Reason = p.Reason,
            CreatedAt = p.CreatedAt,
            ReportId = p.ReportId
        }).ToList();

        return new RewardHistoryResponseDto
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            Total = total
        };
    }
}
