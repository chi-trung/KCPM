using Microsoft.EntityFrameworkCore;
using WastePlatform.Domain.Entities;
using WastePlatform.Infrastructure.Persistence;
using WastePlatform.Infrastructure.Persistence.Repositories;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Infrastructure;

[AllureEpic("Infrastructure")]
[AllureFeature("Reward Points Repository")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Reward calculation and idempotency")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Infrastructure")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "RewardPointsRepositoryTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Infrastructure")]
[AllureOwner("backend")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("repository")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-17")]
public class RewardPointsRepositoryTests
{
    [Fact]
    [AllureDescription("Creates reward points from an active matching rule and applies the configured points plus bonus.")]
    public async Task CreateRewardPointsAsync_WithMatchingActiveRule_ShouldUseRulePointsAndBonus()
    {
        await using var context = CreateContext();
        var repository = new RewardPointsRepository(context);

        var enterpriseId = Guid.NewGuid();
        var wasteCategoryId = 1;
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        context.RewardRules.Add(new RewardRule
        {
            Id = Guid.NewGuid(),
            EnterpriseId = enterpriseId,
            WasteCategoryId = wasteCategoryId,
            PointsPerReport = 10,
            BonusQuality = 3,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var reward = await repository.CreateRewardPointsAsync(
            citizenId,
            reportId,
            taskId,
            enterpriseId,
            wasteCategoryId,
            "Collected successfully");

        reward.Points.Should().Be(13);
        reward.Reason.Should().Be("Collected successfully");
        reward.IdempotencyKey.Should().Be($"task_{taskId}_{reportId}");
        (await context.RewardPoints.CountAsync()).Should().Be(1);
        AllureAttachmentHelper.AttachJson("created-reward", new { reward.Id, reward.Points, reward.Reason, reward.IdempotencyKey });
    }

    [Fact]
    [AllureDescription("Returns the already persisted reward record when the same idempotency key is submitted again.")]
    public async Task CreateRewardPointsAsync_WhenRewardAlreadyExists_ShouldReturnExistingRecord()
    {
        await using var context = CreateContext();
        var repository = new RewardPointsRepository(context);

        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();
        var wasteCategoryId = 1;
        var idempotencyKey = $"task_{taskId}_{reportId}";

        var existingReward = new RewardPoints
        {
            Id = Guid.NewGuid(),
            CitizenId = citizenId,
            ReportId = reportId,
            IdempotencyKey = idempotencyKey,
            Points = 22,
            Reason = "Already created",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        context.RewardPoints.Add(existingReward);
        await context.SaveChangesAsync();

        AllureAttachmentHelper.AttachJson("existing-reward-seed", new
        {
            citizenId,
            reportId,
            taskId,
            enterpriseId,
            wasteCategoryId,
            idempotencyKey,
            existingReward.Id,
            existingReward.Points
        });

        var reward = await repository.CreateRewardPointsAsync(
            citizenId,
            reportId,
            taskId,
            enterpriseId,
            wasteCategoryId,
            "Should not duplicate");

        reward.Id.Should().Be(existingReward.Id);
        reward.Points.Should().Be(22);
        (await context.RewardPoints.CountAsync()).Should().Be(1);

        AllureAttachmentHelper.AttachJson("existing-reward-result", new { reward.Id, reward.Points, reward.IdempotencyKey });
    }

    [Fact]
    [AllureDescription("Falls back to the default reward points and reason when no active rule exists.")]
    public async Task CreateRewardPointsAsync_WithoutRule_ShouldUseDefaultPoints()
    {
        await using var context = CreateContext();
        var repository = new RewardPointsRepository(context);

        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();
        var wasteCategoryId = 99;

        AllureAttachmentHelper.AttachText("default-reward-request", $"citizenId={citizenId}\nreportId={reportId}\ntaskId={taskId}\nenterpriseId={enterpriseId}\nwasteCategoryId={wasteCategoryId}");

        var reward = await repository.CreateRewardPointsAsync(
            citizenId,
            reportId,
            taskId,
            enterpriseId,
            wasteCategoryId,
            reason: null);

        reward.Points.Should().Be(10);
        reward.Reason.Should().Be("Báo cáo và thu gom rác");
        (await context.RewardPoints.CountAsync()).Should().Be(1);

        AllureAttachmentHelper.AttachJson("default-reward-result", new { reward.Id, reward.Points, reward.Reason, reward.IdempotencyKey });
    }

    private static WastePlatformDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<WastePlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new WastePlatformDbContext(options);
    }
}