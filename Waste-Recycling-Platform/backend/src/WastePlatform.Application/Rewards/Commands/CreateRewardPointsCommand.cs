using MediatR;

namespace WastePlatform.Application.Rewards.Commands;

public class CreateRewardPointsCommand : IRequest<CreateRewardPointsCommandResult>
{
    public Guid CitizenId { get; set; }
    public Guid TaskId { get; set; }
    public Guid ReportId { get; set; }
    public Guid EnterpriseId { get; set; }
    public int WasteCategoryId { get; set; }
    public string? Reason { get; set; } = "Báo cáo và thu gom rác";
}

public class CreateRewardPointsCommandResult
{
    public Guid RewardPointsId { get; set; }
    public Guid CitizenId { get; set; }
    public int Points { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}
