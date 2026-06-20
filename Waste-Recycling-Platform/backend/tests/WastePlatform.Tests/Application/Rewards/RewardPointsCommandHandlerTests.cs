using Moq;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Rewards.Commands;
using WastePlatform.Domain.Entities;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Application.Rewards;

[AllureEpic("Rewards")]
[AllureFeature("Reward Points Command Handlers")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Creating reward points for citizens on task completion")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "RewardPointsCommandHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Rewards")]
[AllureOwner("Team")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("rewards")]
public class RewardPointsCommandHandlerTests
{
    private readonly Mock<IRewardPointsRepository> _mockRepo;

    public RewardPointsCommandHandlerTests()
    {
        _mockRepo = new Mock<IRewardPointsRepository>();
    }

    private static RewardPoints CreateRewardPoints(Guid citizenId, int points = 50, string reason = "Báo cáo rác")
    {
        return new RewardPoints
        {
            Id = Guid.NewGuid(),
            CitizenId = citizenId,
            Points = points,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };
    }

    #region CreateRewardPointsCommandHandler

    [Fact]
    [AllureDescription("CreateRewardPoints returns mapped result from repository on success.")]
    public async Task CreateRewardPoints_WithValidRequest_ShouldReturnMappedResult()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();
        const int wasteCategoryId = 1;
        const string reason = "Báo cáo và thu gom rác thành công";

        var createdPoints = CreateRewardPoints(citizenId, 100, reason);

        _mockRepo
            .Setup(x => x.CreateRewardPointsAsync(
                citizenId, reportId, taskId, enterpriseId, wasteCategoryId, reason,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPoints);

        var handler = new CreateRewardPointsCommandHandler(_mockRepo.Object);
        var command = new CreateRewardPointsCommand
        {
            CitizenId = citizenId,
            ReportId = reportId,
            TaskId = taskId,
            EnterpriseId = enterpriseId,
            WasteCategoryId = wasteCategoryId,
            Reason = reason
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().NotBeNull();
        result.RewardPointsId.Should().Be(createdPoints.Id);
        result.CitizenId.Should().Be(citizenId);
        result.Points.Should().Be(100);
        result.Reason.Should().Be(reason);
    }

    [Fact]
    [AllureDescription("CreateRewardPoints uses default reason when Reason is null.")]
    public async Task CreateRewardPoints_WithNullReason_ShouldUseDefaultReason()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        const string defaultReason = "Báo cáo và thu gom rác";

        var createdPoints = CreateRewardPoints(citizenId, 50, defaultReason);

        _mockRepo
            .Setup(x => x.CreateRewardPointsAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(),
                defaultReason, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPoints);

        var handler = new CreateRewardPointsCommandHandler(_mockRepo.Object);
        var command = new CreateRewardPointsCommand
        {
            CitizenId = citizenId,
            Reason = null  // null reason → should fallback to default
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().NotBeNull();
        result.Reason.Should().Be(defaultReason);
    }

    [Fact]
    [AllureDescription("CreateRewardPoints wraps repository exceptions in InvalidOperationException.")]
    public async Task CreateRewardPoints_WhenRepositoryThrows_ShouldWrapInInvalidOperationException()
    {
        // Arrange
        _mockRepo
            .Setup(x => x.CreateRewardPointsAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        var handler = new CreateRewardPointsCommandHandler(_mockRepo.Object);
        var command = new CreateRewardPointsCommand
        {
            CitizenId = Guid.NewGuid(),
            Reason = "Test"
        };

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Lỗi khi tạo điểm thưởng*");
    }

    #endregion
}

