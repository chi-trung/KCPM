using Moq;
using WastePlatform.Application.Admin.Dashboard.Queries;
using WastePlatform.Application.Admin.Users.Commands;
using WastePlatform.Application.Admin.Users.Queries;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Enterprise.Analytics.Queries;
using WastePlatform.Application.Admin.Analytics.DTOs;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Application.Admin;

[AllureEpic("Admin")]
[AllureFeature("Admin User and Dashboard Handlers")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Admin managing users, roles, and dashboard stats")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "AdminUserHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Admin")]
[AllureOwner("Team")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("admin")]
[Allure.Net.Commons.Attributes.AllureTag("users")]
public class AdminUserHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IDashboardRepository> _mockDashboardRepo;
    private readonly Mock<IAnalyticsRepository> _mockAnalyticsRepo;
    private readonly Mock<IEnterpriseRepository> _mockEnterpriseRepo;

    public AdminUserHandlerTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockDashboardRepo = new Mock<IDashboardRepository>();
        _mockAnalyticsRepo = new Mock<IAnalyticsRepository>();
        _mockEnterpriseRepo = new Mock<IEnterpriseRepository>();
    }

    #region GetUsersQueryHandler

    [Fact]
    [AllureDescription("GetUsers returns mapped list of UserDto from repository.")]
    public async Task GetUsers_ShouldReturnMappedUserDtos()
    {
        // Arrange
        var users = new List<User>
        {
            User.Create("citizen@test.com", "hash", "Citizen One", UserRole.Citizen, "0901234567"),
            User.Create("enterprise@test.com", "hash", "Enterprise User", UserRole.Enterprise),
        };

        _mockUserRepo
            .Setup(x => x.GetUsersAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var handler = new GetUsersQueryHandler(_mockUserRepo.Object);
        var query = new GetUsersQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().HaveCount(2);
        result[0].Email.Should().Be("citizen@test.com");
        result[0].Role.Should().Be("citizen");
        result[0].IsActive.Should().BeTrue();
        result[1].Email.Should().Be("enterprise@test.com");
        result[1].Role.Should().Be("enterprise");
    }

    [Fact]
    [AllureDescription("GetUsers passes search and role filters to repository.")]
    public async Task GetUsers_WithSearchAndRoleFilter_ShouldPassFiltersToRepository()
    {
        // Arrange
        _mockUserRepo
            .Setup(x => x.GetUsersAsync("test", "citizen", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        var handler = new GetUsersQueryHandler(_mockUserRepo.Object);
        var query = new GetUsersQuery { Search = "test", Role = "citizen" };

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _mockUserRepo.Verify(
            x => x.GetUsersAsync("test", "citizen", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("GetUsers returns empty list when no users match.")]
    public async Task GetUsers_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        _mockUserRepo
            .Setup(x => x.GetUsersAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        var handler = new GetUsersQueryHandler(_mockUserRepo.Object);
        var query = new GetUsersQuery { Search = "nonexistent" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateUserHandler

    [Fact]
    [AllureDescription("CreateUser calls repository with correct parameters and returns user ID.")]
    public async Task CreateUser_ShouldCallRepositoryAndReturnUserId()
    {
        // Arrange
        var expectedId = Guid.NewGuid().ToString();

        _mockUserRepo
            .Setup(x => x.CreateUserAsync(
                "new@test.com", It.IsAny<string>(), "New User", "0901111111",
                "citizen", "District 1", "Ward 1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        var handler = new CreateUserHandler(_mockUserRepo.Object);
        var command = new CreateUserCommand
        {
            Email = "new@test.com",
            FullName = "New User",
            Phone = "0901111111",
            Role = "citizen",
            District = "District 1",
            Ward = "Ward 1"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().Be(expectedId);
    }

    #endregion

    #region ToggleUserStatusHandler

    [Fact]
    [AllureDescription("ToggleUserStatus returns true when toggle succeeds.")]
    public async Task ToggleUserStatus_ShouldReturnTrueWhenSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        _mockUserRepo
            .Setup(x => x.ToggleUserStatusAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new ToggleUserStatusHandler(_mockUserRepo.Object);
        var command = new ToggleUserStatusCommand { UserId = userId };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().BeTrue();
        _mockUserRepo.Verify(x => x.ToggleUserStatusAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region UpdateUserRoleHandler

    [Fact]
    [AllureDescription("UpdateUserRole calls repository and returns success status.")]
    public async Task UpdateUserRole_ShouldReturnTrueWhenSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        _mockUserRepo
            .Setup(x => x.UpdateUserRoleAsync(userId, "enterprise", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new UpdateUserRoleHandler(_mockUserRepo.Object);
        var command = new UpdateUserRoleCommand { UserId = userId, NewRole = "enterprise" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().BeTrue();
        _mockUserRepo.Verify(x => x.UpdateUserRoleAsync(userId, "enterprise", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetEnterpriseReportAnalyticsQueryHandler

    [Fact]
    [AllureDescription("GetEnterpriseReportAnalytics uses default date range when no dates provided.")]
    public async Task GetEnterpriseAnalytics_WithNoDates_ShouldUseDefaultDateRange()
    {
        // Arrange
        var enterpriseId = Guid.NewGuid();
        var analyticsResult = new ReportAnalyticsDto { TotalReports = 42, CollectedReports = 35 };

        _mockAnalyticsRepo
            .Setup(x => x.GetEnterpriseReportAnalyticsAsync(
                enterpriseId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(analyticsResult);

        var handler = new GetEnterpriseReportAnalyticsQueryHandler(_mockAnalyticsRepo.Object);
        var query = new GetEnterpriseReportAnalyticsQuery
        {
            EnterpriseId = enterpriseId,
            StartDate = null,
            EndDate = null
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().NotBeNull();
        result.TotalReports.Should().Be(42);
        result.CollectedReports.Should().Be(35);
        _mockAnalyticsRepo.Verify(
            x => x.GetEnterpriseReportAnalyticsAsync(
                enterpriseId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("GetEnterpriseReportAnalytics uses provided date range when specified.")]
    public async Task GetEnterpriseAnalytics_WithExplicitDates_ShouldPassThemThrough()
    {
        // Arrange
        var enterpriseId = Guid.NewGuid();
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 6, 30);
        var analyticsResult = new ReportAnalyticsDto { TotalReports = 15 };

        _mockAnalyticsRepo
            .Setup(x => x.GetEnterpriseReportAnalyticsAsync(
                enterpriseId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(analyticsResult);

        var handler = new GetEnterpriseReportAnalyticsQueryHandler(_mockAnalyticsRepo.Object);
        var query = new GetEnterpriseReportAnalyticsQuery
        {
            EnterpriseId = enterpriseId,
            StartDate = startDate,
            EndDate = endDate
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalReports.Should().Be(15);
        _mockAnalyticsRepo.Verify(
            x => x.GetEnterpriseReportAnalyticsAsync(enterpriseId, startDate, endDate, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}

