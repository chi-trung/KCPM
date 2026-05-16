using MediatR;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.Rewards.Commands;

public class CreateRewardPointsCommandHandler : IRequestHandler<CreateRewardPointsCommand, CreateRewardPointsCommandResult>
{
    private readonly IRewardPointsRepository _rewardPointsRepository;

    public CreateRewardPointsCommandHandler(IRewardPointsRepository rewardPointsRepository)
    {
        _rewardPointsRepository = rewardPointsRepository;
    }

    public async Task<CreateRewardPointsCommandResult> Handle(
        CreateRewardPointsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var rewardPoints = await _rewardPointsRepository.CreateRewardPointsAsync(
                request.CitizenId,
                request.ReportId,
                request.TaskId,
                request.EnterpriseId,
                request.WasteCategoryId,
                request.Reason ?? "Báo cáo và thu gom rác",
                cancellationToken);

            return new CreateRewardPointsCommandResult
            {
                RewardPointsId = rewardPoints.Id,
                CitizenId = rewardPoints.CitizenId,
                Points = rewardPoints.Points,
                Reason = rewardPoints.Reason,
                CreatedAt = rewardPoints.CreatedAt
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi tạo điểm thưởng: {ex.Message}", ex);
        }
    }
}

