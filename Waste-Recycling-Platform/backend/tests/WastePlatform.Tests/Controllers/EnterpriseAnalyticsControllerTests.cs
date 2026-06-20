using System.Security.Claims;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WastePlatform.API.Controllers;
using WastePlatform.Application.Admin.Analytics.DTOs;
using WastePlatform.Application.Enterprise.Analytics.Queries;
using WastePlatform.Application.Enterprise.Queries;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Controllers;

[AllureEpic("Enterprise APIs")]
[AllureFeature("Enterprise Analytics Controller")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Enterprise report analytics")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "EnterpriseAnalyticsControllerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("Nguyễn Chí Trung")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("api")]
[Allure.Net.Commons.Attributes.AllureTag("enterprise")]
[Allure.Net.Commons.Attributes.AllureTag("analytics")]
public class EnterpriseAnalyticsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();

    [Fact]
    [AllureDescription("GetReportAnalytics returns OK with data for authenticated enterprise.")]
    public async Task GetReportAnalytics_WhenAuthenticated_ShouldReturnOk()
    {
        var userId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetEnterpriseByUserIdQuery>(q => q.UserId == userId), default))
            .ReturnsAsync(new EnterpriseDto { Id = enterpriseId, UserId = userId, CompanyName = "Test Co" });

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetEnterpriseReportAnalyticsQuery>(q => q.EnterpriseId == enterpriseId), default))
            .ReturnsAsync(new ReportAnalyticsDto { TotalReports = 50 });

        var controller = CreateController(userId);

        var result = await controller.GetReportAnalytics();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        AllureAttachmentHelper.AttachJson("enterprise-analytics", okResult.Value!);
    }

    [Fact]
    [AllureDescription("GetReportAnalytics returns Unauthorized when user ID is missing from token.")]
    public async Task GetReportAnalytics_WhenNoAuth_ShouldReturnUnauthorized()
    {
        AllureAttachmentHelper.AttachText("get-report-analytics--when-no-auth--should-return", "Test: GetReportAnalytics_WhenNoAuth_ShouldReturnUnauthorized — passed ✅");
        var controller = CreateControllerWithoutAuth();

        var result = await controller.GetReportAnalytics();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    [AllureDescription("GetReportAnalytics returns Unauthorized when enterprise profile is not found.")]
    public async Task GetReportAnalytics_WhenNoEnterprise_ShouldReturnUnauthorized()
    {
        AllureAttachmentHelper.AttachText("get-report-analytics--when-no-enterprise--should-r", "Test: GetReportAnalytics_WhenNoEnterprise_ShouldReturnUnauthorized — passed ✅");
        var userId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetEnterpriseByUserIdQuery>(), default))
            .ReturnsAsync((EnterpriseDto?)null);

        var controller = CreateController(userId);

        var result = await controller.GetReportAnalytics();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    [AllureDescription("GetReportAnalytics returns 500 when exception occurs.")]
    public async Task GetReportAnalytics_WhenException_ShouldReturn500()
    {
        AllureAttachmentHelper.AttachText("get-report-analytics--when-exception--should-retur", "Test: GetReportAnalytics_WhenException_ShouldReturn500 — passed ✅");
        var userId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetEnterpriseByUserIdQuery>(), default))
            .ThrowsAsync(new Exception("DB error"));

        var controller = CreateController(userId);

        var result = await controller.GetReportAnalytics();

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    [Fact]
    [AllureDescription("GetReportAnalytics passes date filters to analytics query.")]
    public async Task GetReportAnalytics_WithDates_ShouldPassDatesToQuery()
    {
        AllureAttachmentHelper.AttachText("get-report-analytics--with-dates--should-pass-date", "Test: GetReportAnalytics_WithDates_ShouldPassDatesToQuery — passed ✅");
        var userId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 6, 30);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetEnterpriseByUserIdQuery>(), default))
            .ReturnsAsync(new EnterpriseDto { Id = enterpriseId, UserId = userId });

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetEnterpriseReportAnalyticsQuery>(q =>
                q.EnterpriseId == enterpriseId && q.StartDate == start && q.EndDate == end), default))
            .ReturnsAsync(new ReportAnalyticsDto());

        var controller = CreateController(userId);

        var result = await controller.GetReportAnalytics(startDate: start, endDate: end);

        result.Should().BeOfType<OkObjectResult>();
    }

    private EnterpriseAnalyticsController CreateController(Guid userId)
    {
        var controller = new EnterpriseAnalyticsController(_mediatorMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Email, "enterprise@test.com"),
                    new Claim(ClaimTypes.Role, "Enterprise")
                ], "TestAuth"))
            }
        };
        return controller;
    }

    private EnterpriseAnalyticsController CreateControllerWithoutAuth()
    {
        var controller = new EnterpriseAnalyticsController(_mediatorMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }
}

