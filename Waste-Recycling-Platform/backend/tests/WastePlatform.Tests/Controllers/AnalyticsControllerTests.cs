using System.Security.Claims;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MediatR;
using WastePlatform.API.Controllers;

using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Controllers;

[AllureEpic("Controllers")]
[AllureFeature("Analytics")]
[AllureSuite("AnalyticsControllerTests")]
[AllureParentSuite("Controllers")]
public class AnalyticsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;

    public AnalyticsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
    }

    [Fact]
    [AllureOwner("Thanh Duy")]
    [AllureIssue("https://ut-team-36.atlassian.net/browse/WRP-BE-TESTS-007")]
    public async Task GetReportAnalytics_WhenPublicEndpoint_ShouldReturnOkAndDataNotNull()
    {
        // Arrange
        var expectedDto = new WastePlatform.Application.Admin.Analytics.DTOs.ReportAnalyticsDto();

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<WastePlatform.Application.Public.Analytics.Queries.GetPublicReportAnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var controller = new PublicAnalyticsController(_mediatorMock.Object);

        // Act
        var result = await controller.GetReportAnalytics();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();

        AllureAttachmentHelper.AttachJson(
            "public-analytics-report-request",
            new { startDate = (DateTime?)null, endDate = (DateTime?)null });

        AllureAttachmentHelper.AttachJson(
            "public-analytics-report-response",
            ok.Value!);
    }

    [Fact]
    [AllureOwner("Thanh Duy")]
    [AllureIssue("https://ut-team-36.atlassian.net/browse/WRP-BE-TESTS-007")]
    public async Task GetReportAnalytics_WhenNoAuthToken_ShouldStillReturnOk()
    {
        // Arrange
        var expectedDto = new WastePlatform.Application.Admin.Analytics.DTOs.ReportAnalyticsDto();

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<WastePlatform.Application.Public.Analytics.Queries.GetPublicReportAnalyticsQuery>(), It.IsAny<CancellationToken>()))
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

        // Act
        var result = await controller.GetReportAnalytics();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();

        AllureAttachmentHelper.AttachJson(
            "public-analytics-security-context",
            new { hasAuthHeader = false, hasUserIdentity = controller.User?.Identity?.IsAuthenticated ?? false });
    }
}

