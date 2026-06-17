using Moq;
using WastePlatform.Application.Admin.Dashboard.DTOs;
using WastePlatform.Application.Admin.Dashboard.Queries;
using WastePlatform.Application.Admin.Enterprises.Commands;
using WastePlatform.Application.Admin.Enterprises.Commands.Handlers;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;

namespace WastePlatform.Tests.Application.Admin;

[AllureEpic("Admin")]
[AllureFeature("Admin Enterprise Commands and Dashboard")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Verifying/Rejecting enterprises and Dashboard stats")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "AdminEnterpriseCommandHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Admin")]
[AllureOwner("Team")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("admin")]
public class AdminEnterpriseCommandHandlerTests
{
    private readonly Mock<IEnterpriseRepository> _mockRepo;
    private readonly Mock<IDashboardRepository> _mockDashboardRepo;

    public AdminEnterpriseCommandHandlerTests()
    {
        _mockRepo = new Mock<IEnterpriseRepository>();
        _mockDashboardRepo = new Mock<IDashboardRepository>();
    }

    private static Enterprise CreateEnterprise(bool isVerified = false)
    {
        return new Enterprise
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CompanyName = "Test Enterprise Co.",
            IsVerified = isVerified,
            Status = isVerified ? "Verified" : "Pending",
            CreatedAt = DateTime.UtcNow
        };
    }

    #region VerifyEnterpriseCommandHandler

    [Fact]
    [AllureDescription("VerifyEnterprise returns success when enterprise exists and is not yet verified.")]
    public async Task VerifyEnterprise_WhenFoundAndUnverified_ShouldReturnSuccess()
    {
        // Arrange
        var enterprise = CreateEnterprise(isVerified: false);

        _mockRepo
            .Setup(x => x.GetEnterpriseByIdAsync(enterprise.Id.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(enterprise);
        _mockRepo
            .Setup(x => x.UpdateAsync(enterprise, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new VerifyEnterpriseCommandHandler(_mockRepo.Object);
        var command = new VerifyEnterpriseCommand { EnterpriseId = enterprise.Id };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("verified");
        result.EnterpriseId.Should().Be(enterprise.Id);
        enterprise.IsVerified.Should().BeTrue();
        enterprise.Status.Should().Be("Verified");
        enterprise.RejectionReason.Should().BeNull();
        _mockRepo.Verify(x => x.UpdateAsync(enterprise, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [AllureDescription("VerifyEnterprise returns failure when enterprise is not found.")]
    public async Task VerifyEnterprise_WhenNotFound_ShouldReturnFailure()
    {
        // Arrange
        var id = Guid.NewGuid();

        _mockRepo
            .Setup(x => x.GetEnterpriseByIdAsync(id.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enterprise?)null);

        var handler = new VerifyEnterpriseCommandHandler(_mockRepo.Object);
        var command = new VerifyEnterpriseCommand { EnterpriseId = id };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<Enterprise>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    [AllureDescription("VerifyEnterprise returns failure when enterprise is already verified.")]
    public async Task VerifyEnterprise_WhenAlreadyVerified_ShouldReturnFailure()
    {
        // Arrange
        var enterprise = CreateEnterprise(isVerified: true);

        _mockRepo
            .Setup(x => x.GetEnterpriseByIdAsync(enterprise.Id.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(enterprise);

        var handler = new VerifyEnterpriseCommandHandler(_mockRepo.Object);
        var command = new VerifyEnterpriseCommand { EnterpriseId = enterprise.Id };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already verified");
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<Enterprise>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region RejectEnterpriseCommandHandler

    [Fact]
    [AllureDescription("RejectEnterprise marks enterprise as rejected with rejection reason.")]
    public async Task RejectEnterprise_WhenFound_ShouldSetRejectedStatus()
    {
        // Arrange
        var enterprise = CreateEnterprise(isVerified: false);
        const string reason = "Insufficient documentation";

        _mockRepo
            .Setup(x => x.GetEnterpriseByIdAsync(enterprise.Id.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(enterprise);
        _mockRepo
            .Setup(x => x.UpdateAsync(enterprise, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new RejectEnterpriseCommandHandler(_mockRepo.Object);
        var command = new RejectEnterpriseCommand
        {
            EnterpriseId = enterprise.Id,
            ReasonForRejection = reason
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("rejected");
        enterprise.Status.Should().Be("Rejected");
        enterprise.IsVerified.Should().BeFalse();
        enterprise.RejectionReason.Should().Be(reason);
        _mockRepo.Verify(x => x.UpdateAsync(enterprise, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [AllureDescription("RejectEnterprise returns failure when enterprise is not found.")]
    public async Task RejectEnterprise_WhenNotFound_ShouldReturnFailure()
    {
        // Arrange
        var id = Guid.NewGuid();

        _mockRepo
            .Setup(x => x.GetEnterpriseByIdAsync(id.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enterprise?)null);

        var handler = new RejectEnterpriseCommandHandler(_mockRepo.Object);
        var command = new RejectEnterpriseCommand
        {
            EnterpriseId = id,
            ReasonForRejection = "Some reason"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<Enterprise>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region GetDashboardStatsHandler

    [Fact]
    [AllureDescription("GetDashboardStats delegates to IDashboardRepository and returns stats.")]
    public async Task GetDashboardStats_ShouldReturnRepositoryStats()
    {
        // Arrange
        var stats = new DashboardStatsDto
        {
            TotalUsers = 100,
            TotalReports = 500,
            PendingComplaints = 15,
            CompletedReports = 420
        };

        _mockDashboardRepo
            .Setup(x => x.GetStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        var handler = new GetDashboardStatsHandler(_mockDashboardRepo.Object);
        var query = new GetDashboardStatsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalUsers.Should().Be(100);
        result.TotalReports.Should().Be(500);
        result.PendingComplaints.Should().Be(15);
        result.CompletedReports.Should().Be(420);
        _mockDashboardRepo.Verify(x => x.GetStatsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
