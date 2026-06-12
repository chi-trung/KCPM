using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WastePlatform.API.Controllers;
using WastePlatform.Application.Admin.Analytics.DTOs;
using WastePlatform.Application.Public.Analytics.Queries;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Controllers;

[AllureEpic("Public APIs")]
[AllureFeature("Public Analytics Controller")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Public waste report analytics")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "PublicAnalyticsControllerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("Chi Trung")]
[AllureSeverity(SeverityLevel.minor)]
[Allure.Net.Commons.Attributes.AllureTag("api")]
[Allure.Net.Commons.Attributes.AllureTag("public")]
[Allure.Net.Commons.Attributes.AllureTag("analytics")]
public class PublicAnalyticsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();

    [Fact]
    [AllureDescription("GetReportAnalytics returns OK with analytics data.")]
    public async Task GetReportAnalytics_ShouldReturnOkWithData()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetPublicReportAnalyticsQuery>(), default))
            .ReturnsAsync(new ReportAnalyticsDto { TotalReports = 100 });

        var controller = new PublicAnalyticsController(_mediatorMock.Object);

        var result = await controller.GetReportAnalytics();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        AllureAttachmentHelper.AttachJson("analytics-response", okResult.Value!);
    }

    [Fact]
    [AllureDescription("GetReportAnalytics passes date filters to query.")]
    public async Task GetReportAnalytics_WithDateFilters_ShouldPassToQuery()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 6, 30);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetPublicReportAnalyticsQuery>(q =>
                q.StartDate == start && q.EndDate == end), default))
            .ReturnsAsync(new ReportAnalyticsDto());

        var controller = new PublicAnalyticsController(_mediatorMock.Object);

        var result = await controller.GetReportAnalytics(startDate: start, endDate: end);

        result.Should().BeOfType<OkObjectResult>();
        _mediatorMock.Verify(m => m.Send(It.Is<GetPublicReportAnalyticsQuery>(q =>
            q.StartDate == start && q.EndDate == end), default), Times.Once);
    }

    [Fact]
    [AllureDescription("GetReportAnalytics returns 500 when mediator throws.")]
    public async Task GetReportAnalytics_WhenException_ShouldReturn500()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetPublicReportAnalyticsQuery>(), default))
            .ThrowsAsync(new Exception("Database error"));

        var controller = new PublicAnalyticsController(_mediatorMock.Object);

        var result = await controller.GetReportAnalytics();

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    [Fact]
    [AllureDescription("GetReportAnalytics works without date filters (defaults).")]
    public async Task GetReportAnalytics_WithoutFilters_ShouldSendNullDates()
    {
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetPublicReportAnalyticsQuery>(q =>
                q.StartDate == null && q.EndDate == null), default))
            .ReturnsAsync(new ReportAnalyticsDto());

        var controller = new PublicAnalyticsController(_mediatorMock.Object);

        await controller.GetReportAnalytics();

        _mediatorMock.Verify(m => m.Send(It.Is<GetPublicReportAnalyticsQuery>(q =>
            q.StartDate == null && q.EndDate == null), default), Times.Once);
    }
}
