using Moq;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Rewards.Queries;
using WastePlatform.Domain.Entities;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Application.Rewards;

[AllureEpic("Rewards Module")]
[AllureFeature("Reward Query Handlers")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Citizen reward point queries: total, history, leaderboard")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "RewardsQueryHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Rewards")]
[AllureOwner("Team")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("rewards")]
public class RewardsQueryHandlerTests
{
    private readonly Mock<IRewardPointsRepository> _mockRepo;

    public RewardsQueryHandlerTests()
    {
        _mockRepo = new Mock<IRewardPointsRepository>();
    }

    #region GetTotalPointsQueryHandler

    [Fact]
    [AllureDescription("GetTotalPoints returns correct total for a citizen with points.")]
    public async Task GetTotalPoints_ShouldReturnCorrectTotal()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        const int expectedPoints = 250;

        _mockRepo
            .Setup(x => x.GetTotalPointsByCitizenIdAsync(citizenId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPoints);

        var handler = new GetTotalPointsQueryHandler(_mockRepo.Object);
        var query = new GetTotalPointsQuery { CitizenId = citizenId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        result.TotalPoints.Should().Be(expectedPoints);
        result.LastUpdated.Should().NotBeNull();
        _mockRepo.Verify(
            x => x.GetTotalPointsByCitizenIdAsync(citizenId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("GetTotalPoints returns zero for a citizen with no reward points.")]
    public async Task GetTotalPoints_WithNoPoints_ShouldReturnZero()
    {
        // Arrange
        var citizenId = Guid.NewGuid();

        _mockRepo
            .Setup(x => x.GetTotalPointsByCitizenIdAsync(citizenId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var handler = new GetTotalPointsQueryHandler(_mockRepo.Object);
        var query = new GetTotalPointsQuery { CitizenId = citizenId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        result.TotalPoints.Should().Be(0);
    }

    #endregion

    #region GetRewardHistoryQueryHandler

    [Fact]
    [AllureDescription("GetRewardHistory returns paginated reward history for a citizen.")]
    public async Task GetRewardHistory_ShouldReturnPaginatedHistory()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var rewardPoints = new List<RewardPoints>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CitizenId = citizenId,
                ReportId = reportId,
                Points = 50,
                Reason = "Waste collection report",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                CitizenId = citizenId,
                ReportId = null,
                Points = 10,
                Reason = "Bonus points",
                CreatedAt = DateTime.UtcNow
            }
        };
        const int total = 2;

        _mockRepo
            .Setup(x => x.GetByCitizenIdAsync(citizenId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((rewardPoints, total));

        var handler = new GetRewardHistoryQueryHandler(_mockRepo.Object);
        var query = new GetRewardHistoryQuery
        {
            CitizenId = citizenId,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(total);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.First().Points.Should().Be(50);
        result.Items.First().Reason.Should().Be("Waste collection report");
    }

    [Fact]
    [AllureDescription("GetRewardHistory returns empty list when citizen has no history.")]
    public async Task GetRewardHistory_WithNoHistory_ShouldReturnEmptyList()
    {
        // Arrange
        var citizenId = Guid.NewGuid();

        _mockRepo
            .Setup(x => x.GetByCitizenIdAsync(citizenId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<RewardPoints>(), 0));

        var handler = new GetRewardHistoryQueryHandler(_mockRepo.Object);
        var query = new GetRewardHistoryQuery { CitizenId = citizenId, Page = 1, PageSize = 10 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
    }

    #endregion

    #region GetAreaLeaderboardQueryHandler

    [Fact]
    [AllureDescription("GetAreaLeaderboard returns paginated area leaderboard data.")]
    public async Task GetAreaLeaderboard_ShouldReturnPaginatedAreaData()
    {
        // Arrange
        var areas = new List<AreaLeaderboardDto>
        {
            new() { Area = "Quận 1", TotalPoints = 5000, TotalReports = 100, Participants = 20 },
            new() { Area = "Quận 3", TotalPoints = 3500, TotalReports = 70, Participants = 15 },
        };
        const int total = 2;

        _mockRepo
            .Setup(x => x.GetAreaLeaderboardAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((areas, total));

        var handler = new GetAreaLeaderboardQueryHandler(_mockRepo.Object);
        var query = new GetAreaLeaderboardQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        result.Leaderboard.Should().HaveCount(2);
        result.Total.Should().Be(total);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);
        _mockRepo.Verify(
            x => x.GetAreaLeaderboardAsync(1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("GetAreaLeaderboard returns empty when no area data exists.")]
    public async Task GetAreaLeaderboard_WithNoData_ShouldReturnEmpty()
    {
        // Arrange
        _mockRepo
            .Setup(x => x.GetAreaLeaderboardAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<AreaLeaderboardDto>(), 0));

        var handler = new GetAreaLeaderboardQueryHandler(_mockRepo.Object);
        var query = new GetAreaLeaderboardQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        result.Leaderboard.Should().BeEmpty();
        result.Total.Should().Be(0);
    }

    #endregion
}


