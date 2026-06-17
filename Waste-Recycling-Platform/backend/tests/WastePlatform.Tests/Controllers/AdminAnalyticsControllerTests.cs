using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WastePlatform.API.Controllers;
using WastePlatform.Application.Admin.Analytics.DTOs;
using WastePlatform.Application.Admin.Analytics.Queries;

namespace WastePlatform.Tests.Controllers;

[AllureEpic("Admin Operations")]
[AllureFeature("Admin Analytics Controller")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Admin analytics endpoints for overview, reports, users, waste, and summary")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "AdminAnalyticsControllerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("Team")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("api")]
[Allure.Net.Commons.Attributes.AllureTag("admin")]
[Allure.Net.Commons.Attributes.AllureTag("analytics")]
public class AdminAnalyticsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AdminAnalyticsController _controller;

    public AdminAnalyticsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AdminAnalyticsController(_mediatorMock.Object);
    }

    private static T? GetProp<T>(object obj, string prop)
        => (T?)obj.GetType().GetProperty(prop)?.GetValue(obj);

    #region GetOverview

    [Fact]
    [AllureDescription("GetOverview returns Ok with overview data when mediator succeeds.")]
    public async Task GetOverview_WhenSuccessful_ShouldReturnOkWithData()
    {
        // Arrange
        var overview = new AnalyticsOverviewDto { TotalReports = 100, TotalUsers = 50 };
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetAnalyticsOverviewQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(overview);

        // Act
        var result = await _controller.GetOverview();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        GetProp<string>(ok.Value!, "message").Should().Contain("successfully");
        _mediatorMock.Verify(x => x.Send(It.IsAny<GetAnalyticsOverviewQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [AllureDescription("GetOverview returns 500 when mediator throws exception.")]
    public async Task GetOverview_WhenMediatorThrows_ShouldReturn500()
    {
        // Arrange
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetAnalyticsOverviewQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _controller.GetOverview();

        // Assert
        var status = result.Should().BeOfType<ObjectResult>().Subject;
        status.StatusCode.Should().Be(500);
        GetProp<string>(status.Value!, "message").Should().Contain("Internal server error");
    }

    #endregion

    #region GetReportAnalytics

    [Fact]
    [AllureDescription("GetReportAnalytics returns Ok with report analytics data.")]
    public async Task GetReportAnalytics_WhenSuccessful_ShouldReturnOkWithData()
    {
        // Arrange
        var reportAnalytics = new ReportAnalyticsDto { TotalReports = 200, AcceptedReports = 150 };
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetReportAnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reportAnalytics);

        // Act
        var result = await _controller.GetReportAnalytics();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        GetProp<string>(ok.Value!, "message").Should().Contain("successfully");
    }

    [Fact]
    [AllureDescription("GetReportAnalytics returns BadRequest when startDate is after endDate.")]
    public async Task GetReportAnalytics_WithInvalidDateRange_ShouldReturnBadRequest()
    {
        // Arrange
        var start = new DateTime(2025, 6, 30);
        var end = new DateTime(2025, 1, 1);

        // Act
        var result = await _controller.GetReportAnalytics(start, end);

        // Assert
        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        GetProp<string>(bad.Value!, "message").Should().Contain("Start date");
    }

    [Fact]
    [AllureDescription("GetReportAnalytics returns 500 when mediator throws.")]
    public async Task GetReportAnalytics_WhenMediatorThrows_ShouldReturn500()
    {
        // Arrange
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetReportAnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _controller.GetReportAnalytics();

        // Assert
        var status = result.Should().BeOfType<ObjectResult>().Subject;
        status.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetUserAnalytics

    [Fact]
    [AllureDescription("GetUserAnalytics returns Ok with user data.")]
    public async Task GetUserAnalytics_WhenSuccessful_ShouldReturnOkWithData()
    {
        // Arrange
        var userAnalytics = new UserAnalyticsDto { TotalCitizens = 300 };
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetUserAnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userAnalytics);

        // Act
        var result = await _controller.GetUserAnalytics();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        GetProp<string>(ok.Value!, "message").Should().Contain("successfully");
    }

    [Fact]
    [AllureDescription("GetUserAnalytics returns 500 when mediator throws.")]
    public async Task GetUserAnalytics_WhenMediatorThrows_ShouldReturn500()
    {
        // Arrange
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetUserAnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetUserAnalytics();

        // Assert
        var status = result.Should().BeOfType<ObjectResult>().Subject;
        status.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetWasteAnalytics

    [Fact]
    [AllureDescription("GetWasteAnalytics returns Ok with waste data.")]
    public async Task GetWasteAnalytics_WhenSuccessful_ShouldReturnOkWithData()
    {
        // Arrange
        var wasteAnalytics = new WasteAnalyticsDto { TotalWasteKg = 5000m };
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetWasteAnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(wasteAnalytics);

        // Act
        var result = await _controller.GetWasteAnalytics();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        GetProp<string>(ok.Value!, "message").Should().Contain("successfully");
    }

    [Fact]
    [AllureDescription("GetWasteAnalytics returns BadRequest when startDate is after endDate.")]
    public async Task GetWasteAnalytics_WithInvalidDateRange_ShouldReturnBadRequest()
    {
        // Arrange
        var start = new DateTime(2025, 12, 31);
        var end = new DateTime(2025, 1, 1);

        // Act
        var result = await _controller.GetWasteAnalytics(start, end);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("GetWasteAnalytics returns 500 when mediator throws.")]
    public async Task GetWasteAnalytics_WhenMediatorThrows_ShouldReturn500()
    {
        // Arrange
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetWasteAnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _controller.GetWasteAnalytics();

        // Assert
        var status = result.Should().BeOfType<ObjectResult>().Subject;
        status.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetAnalyticsSummary

    [Fact]
    [AllureDescription("GetAnalyticsSummary returns Ok with comprehensive summary data.")]
    public async Task GetAnalyticsSummary_WhenSuccessful_ShouldReturnOkWithData()
    {
        // Arrange
        var summary = new AnalyticsSummaryDto
        {
            Overview = new AnalyticsOverviewDto { TotalReports = 500 }
        };
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetAnalyticsSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        // Act
        var result = await _controller.GetAnalyticsSummary();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        GetProp<string>(ok.Value!, "message").Should().Contain("successfully");
    }

    [Fact]
    [AllureDescription("GetAnalyticsSummary returns BadRequest when startDate is after endDate.")]
    public async Task GetAnalyticsSummary_WithInvalidDateRange_ShouldReturnBadRequest()
    {
        // Arrange
        var start = new DateTime(2025, 12, 1);
        var end = new DateTime(2025, 1, 1);

        // Act
        var result = await _controller.GetAnalyticsSummary(start, end);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("GetAnalyticsSummary returns 500 when mediator throws.")]
    public async Task GetAnalyticsSummary_WhenMediatorThrows_ShouldReturn500()
    {
        // Arrange
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetAnalyticsSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Timeout"));

        // Act
        var result = await _controller.GetAnalyticsSummary();

        // Assert
        var status = result.Should().BeOfType<ObjectResult>().Subject;
        status.StatusCode.Should().Be(500);
    }

    #endregion
}
