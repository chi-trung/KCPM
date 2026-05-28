using System.Security.Claims;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MediatR;
using WastePlatform.API.Controllers;
using WastePlatform.Application.Admin.Analytics.DTOs;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Public.Analytics.Queries;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Controllers;

[AllureEpic("Verification Practice")]
[AllureFeature("Analytics Module")]
[AllureLabel("story", "Public Analytics Verification")]
[AllureLabel("parentSuite", "xUnit Backend Tests")]
[AllureLabel("suite", "Controllers")]
[AllureLabel("package", "WastePlatform.Tests.Controllers")]
public class AnalyticsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;

    public AnalyticsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
    }

    [Fact]
    [AllureOwner("Thanh Duy")]
    [AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-10")]
    [AllureDescription("Verify public analytics endpoint for data availability, response structure, and non-auth access")]
    public async Task GetReportAnalytics_PublicEndpointWithoutToken_ShouldReturn200Ok()
    {
        // Arrange
        var expectedDto = new ReportAnalyticsDto
        {
            TotalReports = 1,
            AcceptedReports = 1,
            PendingReports = 0,
            RejectedReports = 0,
            CollectedReports = 1,
            ReportsByCategory = new Dictionary<string, int> { ["General"] = 1 },
            WasteByArea = new List<WasteByAreaDto> { new() { Area = "A", Count = 1, WeightKg = 10 } },
            WasteByType = new List<WasteByTypeDto> { new() { Type = "Organic", Count = 1, WeightKg = 10, Percentage = 100 } },
            MonthlyTrends = new List<MonthlyTrendDto> { new() { Month = "2024-01", ReportCount = 1, WeightKg = 10 } }
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetPublicReportAnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var controller = new PublicAnalyticsController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    // no Authorization header / no user claims
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };

        AllureAttachmentHelper.AttachJson(
            "public-analytics-report-request",
            new { startDate = (DateTime?)null, endDate = (DateTime?)null, noAuth = true });

        // Act
        var result = await controller.GetReportAnalytics();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);

        AllureAttachmentHelper.AttachJson(
            "public-analytics-report-response-200",
            ok.Value!);
    }

    [Fact]
    [AllureOwner("Thanh Duy")]
    [AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-10")]
    [AllureDescription("Verify public analytics endpoint for data availability, response structure, and non-auth access")]
    public async Task GetReportAnalytics_PublicEndpoint_ShouldReturnValidResponseBodyStructure()
    {
        // Arrange
        var expectedDto = new ReportAnalyticsDto();

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetPublicReportAnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var controller = new PublicAnalyticsController(_mediatorMock.Object);

        // Act
        var result = await controller.GetReportAnalytics();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();

        // Controller wraps response as: { message, data }
        ok.Value!.Should().BeAssignableTo<object>();

        var valueType = ok.Value.GetType();
        var dataProp = valueType.GetProperty("data");
        dataProp.Should().NotBeNull("Response should contain 'data' property");

        var dataObj = dataProp!.GetValue(ok.Value);
        dataObj.Should().NotBeNull("Response 'data' must not be null");
        dataObj.Should().BeOfType<ReportAnalyticsDto>();

        var data = (ReportAnalyticsDto)dataObj!;

        data.ReportsByCategory.Should().NotBeNull();
        data.WasteByArea.Should().NotBeNull();
        data.WasteByType.Should().NotBeNull();
        data.MonthlyTrends.Should().NotBeNull();

        // key required stats
        data.TotalReports.Should().Be(expectedDto.TotalReports);

        AllureAttachmentHelper.AttachJson(
            "public-analytics-report-response-structure",
            ok.Value!);
    }
}

