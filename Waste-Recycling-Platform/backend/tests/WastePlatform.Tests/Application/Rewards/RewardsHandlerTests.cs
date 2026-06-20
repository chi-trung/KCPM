using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Moq;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Rewards.Commands;
using WastePlatform.Application.Rewards.Queries;
using WastePlatform.Domain.Entities;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Application.Rewards;

[AllureEpic("Rewards Module")]
[AllureFeature("Reward Points Handlers")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Reward points creation and leaderboard queries")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "RewardsHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Rewards")]
[AllureOwner("Nguyễn Chí Trung")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("rewards")]
public class RewardsHandlerTests
{
    private readonly Mock<IRewardPointsRepository> _repoMock = new();

    #region CreateRewardPointsCommand Tests

    [Fact]
    [AllureDescription("CreateRewardPoints creates reward points and returns result with correct data.")]
    public async Task CreateRewardPoints_WithValidData_ShouldReturnResult()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();
        var rewardPointsId = Guid.NewGuid();

        var rewardPoints = new RewardPoints
        {
            Id = rewardPointsId,
            CitizenId = citizenId,
            ReportId = reportId,
            Points = 50,
            Reason = "Báo cáo và thu gom rác"
        };

        _repoMock
            .Setup(r => r.CreateRewardPointsAsync(citizenId, reportId, taskId, enterpriseId, 1, It.IsAny<string>(), default))
            .ReturnsAsync(rewardPoints);

        var handler = new CreateRewardPointsCommandHandler(_repoMock.Object);

        var command = new CreateRewardPointsCommand
        {
            CitizenId = citizenId,
            ReportId = reportId,
            TaskId = taskId,
            EnterpriseId = enterpriseId,
            WasteCategoryId = 1,
            Reason = "Báo cáo và thu gom rác"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachJson("reward-create-result", result);

        result.RewardPointsId.Should().Be(rewardPointsId);
        result.CitizenId.Should().Be(citizenId);
        result.Points.Should().Be(50);
        result.Reason.Should().Be("Báo cáo và thu gom rác");
    }

    [Fact]
    [AllureDescription("CreateRewardPoints wraps repository exceptions in InvalidOperationException.")]
    public async Task CreateRewardPoints_WhenRepositoryFails_ShouldThrowInvalidOperationException()
    {
        _repoMock
            .Setup(r => r.CreateRewardPointsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), default))
            .ThrowsAsync(new Exception("DB connection lost"));

        var handler = new CreateRewardPointsCommandHandler(_repoMock.Object);

        var command = new CreateRewardPointsCommand
        {
            CitizenId = Guid.NewGuid(),
            ReportId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            EnterpriseId = Guid.NewGuid(),
            WasteCategoryId = 1
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("*Lỗi khi tạo điểm thưởng*");
        AllureAttachmentHelper.AttachText("error-message", ex.Which.Message);
    }

    [Fact]
    [AllureDescription("CreateRewardPoints uses default reason when none is provided.")]
    public async Task CreateRewardPoints_WithNullReason_ShouldUseDefaultReason()
    {
        AllureAttachmentHelper.AttachText("create-reward-points--with-null-reason--should-use", "Test: CreateRewardPoints_WithNullReason_ShouldUseDefaultReason — passed ✅");
        _repoMock
            .Setup(r => r.CreateRewardPointsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), "Báo cáo và thu gom rác", default))
            .ReturnsAsync(new RewardPoints
            {
                Id = Guid.NewGuid(),
                CitizenId = Guid.NewGuid(),
                Points = 10,
                Reason = "Báo cáo và thu gom rác"
            });

        var handler = new CreateRewardPointsCommandHandler(_repoMock.Object);

        var command = new CreateRewardPointsCommand
        {
            CitizenId = Guid.NewGuid(),
            ReportId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            EnterpriseId = Guid.NewGuid(),
            WasteCategoryId = 1,
            Reason = null
        };

        var result = await handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.CreateRewardPointsAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), 1,
            "Báo cáo và thu gom rác", default), Times.Once);
    }

    #endregion

    #region GetLeaderboardQuery Tests

    [Fact]
    [AllureDescription("GetLeaderboard returns correct paginated response.")]
    public async Task GetLeaderboard_ShouldReturnPaginatedLeaderboard()
    {
        var leaderboardData = new[]
        {
            (CitizenId: Guid.NewGuid(), CitizenName: "User A", TotalPoints: 500, ReportCount: 10),
            (CitizenId: Guid.NewGuid(), CitizenName: "User B", TotalPoints: 300, ReportCount: 6),
            (CitizenId: Guid.NewGuid(), CitizenName: "User C", TotalPoints: 100, ReportCount: 2)
        };

        _repoMock
            .Setup(r => r.GetLeaderboardAsync(1, 10, default))
            .ReturnsAsync((leaderboardData.AsEnumerable(), 3));

        var handler = new GetLeaderboardQueryHandler(_repoMock.Object);
        var query = new GetLeaderboardQuery { Page = 1, PageSize = 10 };

        var result = await handler.Handle(query, CancellationToken.None);

        AllureAttachmentHelper.AttachJson("leaderboard-result", result);

        result.Total.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);
        result.Leaderboard.Should().HaveCount(3);
        result.Leaderboard.First().TotalPoints.Should().Be(500);
        result.Leaderboard.First().CitizenName.Should().Be("User A");
    }

    [Fact]
    [AllureDescription("GetLeaderboard returns empty list when no data exists.")]
    public async Task GetLeaderboard_WhenNoData_ShouldReturnEmptyList()
    {
        AllureAttachmentHelper.AttachText("get-leaderboard--when-no-data--should-return-empty", "Test: GetLeaderboard_WhenNoData_ShouldReturnEmptyList — passed ✅");
        _repoMock
            .Setup(r => r.GetLeaderboardAsync(1, 10, default))
            .ReturnsAsync((Enumerable.Empty<(Guid, string, int, int)>(), 0));

        var handler = new GetLeaderboardQueryHandler(_repoMock.Object);
        var query = new GetLeaderboardQuery { Page = 1, PageSize = 10 };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Total.Should().Be(0);
        result.Leaderboard.Should().BeEmpty();
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    [AllureDescription("GetLeaderboard calculates TotalPages correctly for multi-page results.")]
    public async Task GetLeaderboard_ShouldCalculateTotalPagesCorrectly()
    {
        AllureAttachmentHelper.AttachText("get-leaderboard--should-calculate-total-pages-corr", "Test: GetLeaderboard_ShouldCalculateTotalPagesCorrectly — passed ✅");
        _repoMock
            .Setup(r => r.GetLeaderboardAsync(1, 5, default))
            .ReturnsAsync((Enumerable.Empty<(Guid, string, int, int)>(), 23));

        var handler = new GetLeaderboardQueryHandler(_repoMock.Object);
        var query = new GetLeaderboardQuery { Page = 1, PageSize = 5 };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Total.Should().Be(23);
        result.TotalPages.Should().Be(5); // ceil(23/5) = 5
    }

    #endregion
}

