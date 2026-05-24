using Microsoft.EntityFrameworkCore;
using WastePlatform.Domain.Entities;
using WastePlatform.Infrastructure.Persistence;
using WastePlatform.Infrastructure.Persistence.Repositories;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Infrastructure;

[Allure.Net.Commons.Attributes.AllureLabel("story", "Reward Points Calculation")]
[Allure.Net.Commons.Attributes.AllureTag("auto-jira")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-17")]
[Allure.Net.Commons.Attributes.AllureOwner("chi-trung")]
public class RewardPointsRepositoryTests
{
    [Fact]
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
        // Attach created reward for Allure
        AllureAttachmentHelper.AttachJson("created-reward", new { reward.Id, reward.Points, reward.Reason, reward.IdempotencyKey });
    }

    [Fact]
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
    }

    [Fact]
    public async Task CreateRewardPointsAsync_WithoutRule_ShouldUseDefaultPoints()
    {
        await using var context = CreateContext();
        var repository = new RewardPointsRepository(context);

        var reward = await repository.CreateRewardPointsAsync(
            citizenId: Guid.NewGuid(),
            reportId: Guid.NewGuid(),
            taskId: Guid.NewGuid(),
            enterpriseId: Guid.NewGuid(),
            wasteCategoryId: 99,
            reason: null);

        reward.Points.Should().Be(10);
        reward.Reason.Should().Be("Báo cáo và thu gom rác");
        (await context.RewardPoints.CountAsync()).Should().Be(1);
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