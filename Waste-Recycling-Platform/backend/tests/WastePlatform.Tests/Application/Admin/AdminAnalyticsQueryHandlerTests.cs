using Moq;
using WastePlatform.Application.Admin.Analytics.DTOs;
using WastePlatform.Application.Admin.Analytics.Queries;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Application.Admin;

[AllureEpic("Admin")]
[AllureFeature("Admin Analytics Query Handlers")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Analytics for reports, users, waste, and overview")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "AdminAnalyticsQueryHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Admin")]
[AllureOwner("Team")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("analytics")]
public class AdminAnalyticsQueryHandlerTests
{
    private readonly Mock<IAnalyticsRepository> _mockRepo;

    public AdminAnalyticsQueryHandlerTests()
    {
        _mockRepo = new Mock<IAnalyticsRepository>();
    }

    #region GetAnalyticsOverviewQueryHandler

    [Fact]
    [AllureDescription("GetAnalyticsOverview returns overview data from repository.")]
    public async Task GetAnalyticsOverview_ShouldDelegateToRepositoryAndReturn()
    {
        // Arrange
        var overview = new AnalyticsOverviewDto
        {
            TotalReports = 250,
            TotalComplaints = 30,
            TotalUsers = 500,
            ActiveEnterprises = 15,
            TotalWasteCollected = 1500.5m
        };

        _mockRepo
            .Setup(x => x.GetOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(overview);

        var handler = new GetAnalyticsOverviewQueryHandler(_mockRepo.Object);
        var query = new GetAnalyticsOverviewQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().NotBeNull();
        result.TotalReports.Should().Be(250);
        result.TotalComplaints.Should().Be(30);
        result.TotalUsers.Should().Be(500);
        result.ActiveEnterprises.Should().Be(15);
        _mockRepo.Verify(x => x.GetOverviewAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetReportAnalyticsQueryHandler

    [Fact]
    [AllureDescription("GetReportAnalytics uses default date range (last month to now) when not specified.")]
    public async Task GetReportAnalytics_WithNoDates_ShouldUseDefaultRange()
    {
        // Arrange
        var reportAnalytics = new ReportAnalyticsDto { TotalReports = 100, AcceptedReports = 80 };

        _mockRepo
            .Setup(x => x.GetReportAnalyticsAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reportAnalytics);

        var handler = new GetReportAnalyticsQueryHandler(_mockRepo.Object);
        var query = new GetReportAnalyticsQuery { StartDate = null, EndDate = null };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalReports.Should().Be(100);
        result.AcceptedReports.Should().Be(80);
        _mockRepo.Verify(
            x => x.GetReportAnalyticsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("GetReportAnalytics passes explicit date range to repository.")]
    public async Task GetReportAnalytics_WithExplicitDates_ShouldPassDatesToRepository()
    {
        // Arrange
        var start = new DateTime(2025, 1, 1);
        var end = new DateTime(2025, 6, 30);
        var reportAnalytics = new ReportAnalyticsDto { TotalReports = 50 };

        _mockRepo
            .Setup(x => x.GetReportAnalyticsAsync(start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reportAnalytics);

        var handler = new GetReportAnalyticsQueryHandler(_mockRepo.Object);
        var query = new GetReportAnalyticsQuery { StartDate = start, EndDate = end };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalReports.Should().Be(50);
        _mockRepo.Verify(x => x.GetReportAnalyticsAsync(start, end, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetUserAnalyticsQueryHandler

    [Fact]
    [AllureDescription("GetUserAnalytics returns user distribution data from repository.")]
    public async Task GetUserAnalytics_ShouldReturnUserData()
    {
        // Arrange
        var userAnalytics = new UserAnalyticsDto
        {
            TotalCitizens = 300,
            ActiveCitizens = 250,
            TotalEnterprises = 20,
            VerifiedEnterprises = 15
        };

        _mockRepo
            .Setup(x => x.GetUserAnalyticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(userAnalytics);

        var handler = new GetUserAnalyticsQueryHandler(_mockRepo.Object);
        var query = new GetUserAnalyticsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCitizens.Should().Be(300);
        result.ActiveCitizens.Should().Be(250);
        result.TotalEnterprises.Should().Be(20);
        result.VerifiedEnterprises.Should().Be(15);
        _mockRepo.Verify(x => x.GetUserAnalyticsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetWasteAnalyticsQueryHandler

    [Fact]
    [AllureDescription("GetWasteAnalytics uses default date range when not specified.")]
    public async Task GetWasteAnalytics_WithNoDates_ShouldUseDefaultRange()
    {
        // Arrange
        var wasteAnalytics = new WasteAnalyticsDto
        {
            TotalWasteKg = 2500m,
            TotalWasteCategories = 5
        };

        _mockRepo
            .Setup(x => x.GetWasteAnalyticsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(wasteAnalytics);

        var handler = new GetWasteAnalyticsQueryHandler(_mockRepo.Object);
        var query = new GetWasteAnalyticsQuery { StartDate = null, EndDate = null };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalWasteKg.Should().Be(2500m);
        result.TotalWasteCategories.Should().Be(5);
        _mockRepo.Verify(
            x => x.GetWasteAnalyticsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("GetWasteAnalytics passes explicit date range to repository.")]
    public async Task GetWasteAnalytics_WithExplicitDates_ShouldPassDatesToRepository()
    {
        // Arrange
        var start = new DateTime(2025, 3, 1);
        var end = new DateTime(2025, 3, 31);
        var wasteAnalytics = new WasteAnalyticsDto { TotalWasteKg = 500m };

        _mockRepo
            .Setup(x => x.GetWasteAnalyticsAsync(start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wasteAnalytics);

        var handler = new GetWasteAnalyticsQueryHandler(_mockRepo.Object);
        var query = new GetWasteAnalyticsQuery { StartDate = start, EndDate = end };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalWasteKg.Should().Be(500m);
        _mockRepo.Verify(x => x.GetWasteAnalyticsAsync(start, end, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetAnalyticsSummaryQueryHandler

    [Fact]
    [AllureDescription("GetAnalyticsSummary uses default date range when not specified.")]
    public async Task GetAnalyticsSummary_WithNoDates_ShouldUseDefaultRange()
    {
        // Arrange
        var summary = new AnalyticsSummaryDto
        {
            Overview = new AnalyticsOverviewDto { TotalReports = 500 },
            ReportAnalytics = new ReportAnalyticsDto { TotalReports = 500 },
            UserAnalytics = new UserAnalyticsDto { TotalCitizens = 200 },
            WasteAnalytics = new WasteAnalyticsDto { TotalWasteKg = 3000m }
        };

        _mockRepo
            .Setup(x => x.GetSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var handler = new GetAnalyticsSummaryQueryHandler(_mockRepo.Object);
        var query = new GetAnalyticsSummaryQuery { StartDate = null, EndDate = null };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().NotBeNull();
        result.Overview.TotalReports.Should().Be(500);
        result.UserAnalytics.TotalCitizens.Should().Be(200);
        result.WasteAnalytics.TotalWasteKg.Should().Be(3000m);
        _mockRepo.Verify(
            x => x.GetSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("GetAnalyticsSummary passes explicit date range to repository.")]
    public async Task GetAnalyticsSummary_WithExplicitDates_ShouldPassDatesToRepository()
    {
        // Arrange
        var start = new DateTime(2025, 1, 1);
        var end = new DateTime(2025, 12, 31);
        var summary = new AnalyticsSummaryDto();

        _mockRepo
            .Setup(x => x.GetSummaryAsync(start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var handler = new GetAnalyticsSummaryQueryHandler(_mockRepo.Object);
        var query = new GetAnalyticsSummaryQuery { StartDate = start, EndDate = end };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().NotBeNull();
        _mockRepo.Verify(x => x.GetSummaryAsync(start, end, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}

