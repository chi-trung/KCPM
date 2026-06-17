using System.Reflection;
using System.Security.Claims;
using Allure.Net.Commons;
using Allure.Xunit.Attributes;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WastePlatform.API.Controllers;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Common.DTOs;
using WastePlatform.Application.Reports.Commands;
using WastePlatform.Application.Reports.Queries;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;
using WastePlatform.Tests.TestSupport;
using Xunit;

namespace WastePlatform.Tests.Controllers;

[AllureEpic("KIEM-5: Reports Module Testing")]
[AllureFeature("Report Controller")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Create, retrieve and manage waste reports")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "ReportControllerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("Nguyen Minh Phung")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("api")]
[Allure.Net.Commons.Attributes.AllureTag("reports")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-5")]
public class ReportControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<INotificationService> _notificationServiceMock;

    public ReportControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _notificationServiceMock = new Mock<INotificationService>();
    }

    [Fact]
    [AllureDescription("TC-REP-API-001: Create Report with valid Form data yields 201 Created and sends notification.")]
    public async Task CreateReport_WithValidForm_ShouldReturnCreatedAndNotify()
    {
        // Arrange
        await using var context = CreateContext();
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();

        var form = new FormCollection(
            new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["WasteCategoryId"] = "1",
                ["Latitude"] = "10.7769",
                ["Longitude"] = "106.7009",
                ["Description"] = "Rác hữu cơ",
                ["Address"] = "123 Nguyen Trai",
                ["AiSuggestion"] = "Organic"
            },
            new FormFileCollection());

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateReportCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reportId);

        var reportDto = new ReportDto
        {
            Id = reportId,
            CitizenId = citizenId,
            CitizenName = "Citizen One",
            WasteCategoryId = 1,
            CategoryName = "Organic",
            Description = "Rác hữu cơ",
            Latitude = 10.7769m,
            Longitude = 106.7009m,
            Address = "123 Nguyen Trai",
            Status = ReportStatus.Pending,
            AiSuggestion = "Organic",
            CreatedAt = DateTime.UtcNow,
            ImageUrls = new List<string> { "image1.jpg" }
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetReportByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reportDto);

        _notificationServiceMock
            .Setup(x => x.NotifyReportCreatedAsync(citizenId, reportId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller = new ReportController(_mediatorMock.Object, context, _notificationServiceMock.Object)
        {
            ControllerContext = BuildControllerContext(citizenId, "Citizen")
        };

        // Act
        var result = await controller.CreateReport(form);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ReportController.GetReportById));
        createdResult.RouteValues.Should().ContainKey("id").And.ContainValue(reportId);

        var value = createdResult.Value!;
        GetPropertyValue<string>(value, "message").Should().Be("Report created successfully");

        var reportObj = GetPropertyValue<ReportDto>(value, "report");
        reportObj.Should().NotBeNull();
        reportObj!.Id.Should().Be(reportId);
        reportObj.CategoryName.Should().Be("Organic");

        _notificationServiceMock.Verify(
            x => x.NotifyReportCreatedAsync(citizenId, reportId, It.IsAny<CancellationToken>()),
            Times.Once);

        AllureAttachmentHelper.AttachJson("create-report-api-response", value);
    }

    [Fact]
    [AllureDescription("TC-REP-API-002: Create Report fails with BadRequest when WasteCategoryId is invalid.")]
    public async Task CreateReport_WithInvalidWasteCategoryId_ShouldReturnBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var citizenId = Guid.NewGuid();

        var form = new FormCollection(
            new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["WasteCategoryId"] = "not-an-integer",
                ["Latitude"] = "10.7769",
                ["Longitude"] = "106.7009"
            },
            new FormFileCollection());

        var controller = new ReportController(_mediatorMock.Object, context, _notificationServiceMock.Object)
        {
            ControllerContext = BuildControllerContext(citizenId, "Citizen")
        };

        // Act
        var result = await controller.CreateReport(form);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        GetPropertyValue<string>(badRequest.Value!, "message").Should().Be("Invalid WasteCategoryId");
    }

    [Fact]
    [AllureDescription("TC-REP-API-003: Create Report fails with BadRequest when Latitude is invalid.")]
    public async Task CreateReport_WithInvalidLatitude_ShouldReturnBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var citizenId = Guid.NewGuid();

        var form = new FormCollection(
            new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["WasteCategoryId"] = "1",
                ["Latitude"] = "invalid-latitude",
                ["Longitude"] = "106.7009"
            },
            new FormFileCollection());

        var controller = new ReportController(_mediatorMock.Object, context, _notificationServiceMock.Object)
        {
            ControllerContext = BuildControllerContext(citizenId, "Citizen")
        };

        // Act
        var result = await controller.CreateReport(form);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        GetPropertyValue<string>(badRequest.Value!, "message").Should().Be("Invalid Latitude");
    }

    [Fact]
    [AllureDescription("TC-REP-API-004: Create Report fails with BadRequest when Longitude is invalid.")]
    public async Task CreateReport_WithInvalidLongitude_ShouldReturnBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var citizenId = Guid.NewGuid();

        var form = new FormCollection(
            new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["WasteCategoryId"] = "1",
                ["Latitude"] = "10.7769",
                ["Longitude"] = "invalid-longitude"
            },
            new FormFileCollection());

        var controller = new ReportController(_mediatorMock.Object, context, _notificationServiceMock.Object)
        {
            ControllerContext = BuildControllerContext(citizenId, "Citizen")
        };

        // Act
        var result = await controller.CreateReport(form);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        GetPropertyValue<string>(badRequest.Value!, "message").Should().Be("Invalid Longitude");
    }

    [Fact]
    [AllureDescription("TC-REP-API-005: Create Report yields 401 Unauthorized if citizen user claim is missing.")]
    public async Task CreateReport_WithMissingUserClaim_ShouldReturnUnauthorized()
    {
        // Arrange
        await using var context = CreateContext();

        var form = new FormCollection(
            new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["WasteCategoryId"] = "1",
                ["Latitude"] = "10.7769",
                ["Longitude"] = "106.7009"
            },
            new FormFileCollection());

        var controller = new ReportController(_mediatorMock.Object, context, _notificationServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() // No claims
            }
        };

        // Act
        var result = await controller.CreateReport(form);

        // Assert
        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        GetPropertyValue<string>(unauthorized.Value!, "message").Should().Be("Invalid or missing user ID in token");
    }

    [Fact]
    [AllureDescription("TC-REP-API-006: GetReportById returns Ok with correct details when report exists.")]
    public async Task GetReportById_WhenFound_ShouldReturnOk()
    {
        // Arrange
        await using var context = CreateContext();
        var reportId = Guid.NewGuid();
        var reportDto = new ReportDto { Id = reportId, Description = "Test report", Status = ReportStatus.Pending };

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetReportByIdQuery>(q => q.Id == reportId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reportDto);

        var controller = new ReportController(_mediatorMock.Object, context, _notificationServiceMock.Object);

        // Act
        var result = await controller.GetReportById(reportId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        GetPropertyValue<string>(okResult.Value!, "message").Should().Be("Report retrieved successfully");
        var report = GetPropertyValue<ReportDto>(okResult.Value!, "report");
        report.Should().NotBeNull();
        report!.Id.Should().Be(reportId);
    }

    [Fact]
    [AllureDescription("TC-REP-API-007: GetReportById returns NotFound when report does not exist.")]
    public async Task GetReportById_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        await using var context = CreateContext();
        var reportId = Guid.NewGuid();

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetReportByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReportDto?)null);

        var controller = new ReportController(_mediatorMock.Object, context, _notificationServiceMock.Object);

        // Act
        var result = await controller.GetReportById(reportId);

        // Assert
        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        GetPropertyValue<string>(notFound.Value!, "message").Should().Be("Report not found");
    }

    [Fact]
    [AllureDescription("TC-REP-API-008: GetMyReports returns Citizen's paged reports successfully.")]
    public async Task GetMyReports_WithValidUser_ShouldReturnOkWithPagedReports()
    {
        // Arrange
        await using var context = CreateContext();
        var citizenId = Guid.NewGuid();
        var reportsList = new List<ReportListDto>
        {
            new() { Id = Guid.NewGuid(), CitizenName = "Citizen One", Status = ReportStatus.Pending }
        };

        var queryResult = (Reports: (IEnumerable<ReportListDto>)reportsList, Total: 1, TotalPages: 1);

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetMyReportsQuery>(q => q.UserId == citizenId && q.Page == 1 && q.PageSize == 10), It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult);

        var controller = new ReportController(_mediatorMock.Object, context, _notificationServiceMock.Object)
        {
            ControllerContext = BuildControllerContext(citizenId, "Citizen")
        };

        // Act
        var result = await controller.GetMyReports(page: 1, pageSize: 10);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value!;
        GetPropertyValue<string>(value, "message").Should().Be("Reports retrieved successfully");

        var pagination = GetPropertyValue<object>(value, "pagination");
        GetPropertyValue<int>(pagination!, "total").Should().Be(1);

        var reports = GetPropertyValue<IEnumerable<ReportListDto>>(value, "reports");
        reports.Should().HaveCount(1);
    }

    [Fact]
    [AllureDescription("TC-REP-API-009: GetMyReports returns Unauthorized if citizen user claim is missing.")]
    public async Task GetMyReports_WithMissingUserClaim_ShouldReturnUnauthorized()
    {
        // Arrange
        await using var context = CreateContext();
        var controller = new ReportController(_mediatorMock.Object, context, _notificationServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        // Act
        var result = await controller.GetMyReports();

        // Assert
        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        GetPropertyValue<string>(unauthorized.Value!, "message").Should().Be("Invalid or missing user ID");
    }

    [Fact]
    [AllureDescription("TC-REP-API-010: GetAllReports returns paged reports for Admin/Enterprise role.")]
    public async Task GetAllReports_WithValidUser_ShouldReturnOkWithPagedReports()
    {
        // Arrange
        await using var context = CreateContext();
        var reportsList = new List<ReportListDto> { new() { Id = Guid.NewGuid(), Status = ReportStatus.Pending } };
        var queryResult = (Reports: (IEnumerable<ReportListDto>)reportsList, Total: 1, TotalPages: 1);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetAllReportsQuery>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(queryResult));

        var adminId = Guid.NewGuid();
        var controller = new ReportController(_mediatorMock.Object, context, _notificationServiceMock.Object)
        {
            ControllerContext = BuildControllerContext(adminId, "Admin")
        };

        // Act
        var result = await controller.GetAllReports(page: 1, pageSize: 10, status: "Pending");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value!;
        GetPropertyValue<string>(value, "message").Should().Be("All reports retrieved successfully");

        var pagination = GetPropertyValue<object>(value, "pagination");
        GetPropertyValue<int>(pagination!, "total").Should().Be(1);
    }

    [Fact]
    [AllureDescription("TC-REP-API-011: Admin accepts report — status updates to Accepted, CollectionTask is created, and Notification is sent.")]
    public async Task AcceptReport_WhenPendingAsAdmin_ShouldAcceptSuccessfully()
    {
        // Arrange
        await using var context = CreateContext();
        var report = WasteReport.Create(Guid.NewGuid(), 1, 10m, 106m, "Desc", "Address");
        context.WasteReports.Add(report);

        // Seed an enterprise so that Admin auto-assign task works
        var enterpriseUser = User.Create("enterprise@example.com", "hash", "Enterprise One", UserRole.Enterprise);
        var enterprise = new Enterprise { Id = Guid.NewGuid(), UserId = enterpriseUser.Id, CompanyName = "Enterprise One", User = enterpriseUser };
        context.Users.Add(enterpriseUser);
        context.Enterprises.Add(enterprise);
        await context.SaveChangesAsync();

        _notificationServiceMock
            .Setup(x => x.NotifyReportAcceptedAsync(report.CitizenId, report.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var adminId = Guid.NewGuid();
        var controller = new ReportController(_mediatorMock.Object, context, _notificationServiceMock.Object)
        {
            ControllerContext = BuildControllerContext(adminId, "Admin")
        };

        // Act
        var result = await controller.AcceptReportAndCreateTask(report.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        GetPropertyValue<string>(okResult.Value!, "message").Should().Be("Report accepted successfully");
        GetPropertyValue<Guid>(okResult.Value!, "reportId").Should().Be(report.Id);
        GetPropertyValue<string>(okResult.Value!, "reportStatus").Should().Be("Accepted");
        GetPropertyValue<Guid?>(okResult.Value!, "taskId").Should().NotBeNull().And.NotBe(Guid.Empty);

        // Verify state is saved
        var updatedReport = await context.WasteReports.FindAsync(report.Id);
        updatedReport!.Status.Should().Be(ReportStatus.Accepted);

        // Verify task created
        var task = await context.CollectionTasks.SingleOrDefaultAsync(t => t.ReportId == report.Id);
        task.Should().NotBeNull();
        task!.EnterpriseId.Should().Be(enterprise.Id);

        _notificationServiceMock.Verify(
            x => x.NotifyReportAcceptedAsync(report.CitizenId, report.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("TC-REP-API-012: Enterprise accepts report matching their category and service area successfully.")]
    public async Task AcceptReport_WhenPendingAsEnterprise_ShouldAcceptSuccessfully()
    {
        // Arrange
        await using var context = CreateContext();
        // Report inside District 1
        var report = WasteReport.Create(Guid.NewGuid(), 2, 10m, 106m, "Desc", "District 1, HCMC");
        context.WasteReports.Add(report);

        var enterpriseUser = User.Create("ent@example.com", "hash", "Enterprise Two", UserRole.Enterprise);
        var enterprise = new Enterprise 
        { 
            Id = Guid.NewGuid(), 
            UserId = enterpriseUser.Id, 
            CompanyName = "Enterprise Two", 
            User = enterpriseUser,
            ServiceArea = "[\"District 1\", \"District 3\"]" // JSON service area list
        };

        var wasteType = new EnterpriseWasteType { EnterpriseId = enterprise.Id, WasteCategoryId = 2 };

        context.Users.Add(enterpriseUser);
        context.Enterprises.Add(enterprise);
        context.EnterpriseWasteTypes.Add(wasteType);
        await context.SaveChangesAsync();

        _notificationServiceMock
            .Setup(x => x.NotifyReportAcceptedAsync(report.CitizenId, report.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller = new ReportController(_mediatorMock.Object, context, _notificationServiceMock.Object)
        {
            ControllerContext = BuildControllerContext(enterpriseUser.Id, "Enterprise")
        };

        // Act
        var result = await controller.AcceptReportAndCreateTask(report.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        GetPropertyValue<string>(okResult.Value!, "message").Should().Be("Report accepted successfully");
        GetPropertyValue<Guid?>(okResult.Value!, "taskId").Should().NotBeNull().And.NotBe(Guid.Empty);

        var updatedReport = await context.WasteReports.FindAsync(report.Id);
        updatedReport!.Status.Should().Be(ReportStatus.Accepted);

        var task = await context.CollectionTasks.SingleOrDefaultAsync(t => t.ReportId == report.Id);
        task.Should().NotBeNull();
        task!.EnterpriseId.Should().Be(enterprise.Id);

        _notificationServiceMock.Verify(
            x => x.NotifyReportAcceptedAsync(report.CitizenId, report.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("TC-REP-API-013: Enterprise accepts report but fails with 400 when category is not handled.")]
    public async Task AcceptReport_WithUnHandledCategory_ShouldReturnBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var report = WasteReport.Create(Guid.NewGuid(), 99, 10m, 106m, "Desc", "District 1, HCMC"); // Category 99
        context.WasteReports.Add(report);

        var enterpriseUser = User.Create("ent@example.com", "hash", "Enterprise", UserRole.Enterprise);
        var enterprise = new Enterprise 
        { 
            Id = Guid.NewGuid(), 
            UserId = enterpriseUser.Id, 
            CompanyName = "Enterprise", 
            User = enterpriseUser,
            ServiceArea = "[\"District 1\"]"
        };

        context.Users.Add(enterpriseUser);
        context.Enterprises.Add(enterprise);
        await context.SaveChangesAsync();

        var controller = new ReportController(_mediatorMock.Object, context, _notificationServiceMock.Object)
        {
            ControllerContext = BuildControllerContext(enterpriseUser.Id, "Enterprise")
        };

        // Act
        var result = await controller.AcceptReportAndCreateTask(report.Id);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        GetPropertyValue<string>(badRequest.Value!, "message").Should().Be("This report's waste category is not handled by your enterprise.");
    }

    [Fact]
    [AllureDescription("TC-REP-API-014: Enterprise accepts report but fails with 400 when outside service area.")]
    public async Task AcceptReport_WithOutsideServiceArea_ShouldReturnBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var report = WasteReport.Create(Guid.NewGuid(), 2, 10m, 106m, "Desc", "District 9, HCMC"); // Outside service area
        context.WasteReports.Add(report);

        var enterpriseUser = User.Create("ent@example.com", "hash", "Enterprise", UserRole.Enterprise);
        var enterprise = new Enterprise 
        { 
            Id = Guid.NewGuid(), 
            UserId = enterpriseUser.Id, 
            CompanyName = "Enterprise", 
            User = enterpriseUser,
            ServiceArea = "[\"District 1\", \"District 3\"]"
        };
        var wasteType = new EnterpriseWasteType { EnterpriseId = enterprise.Id, WasteCategoryId = 2 };

        context.Users.Add(enterpriseUser);
        context.Enterprises.Add(enterprise);
        context.EnterpriseWasteTypes.Add(wasteType);
        await context.SaveChangesAsync();

        var controller = new ReportController(_mediatorMock.Object, context, _notificationServiceMock.Object)
        {
            ControllerContext = BuildControllerContext(enterpriseUser.Id, "Enterprise")
        };

        // Act
        var result = await controller.AcceptReportAndCreateTask(report.Id);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        GetPropertyValue<string>(badRequest.Value!, "message").Should().Be("This report is outside your enterprise service area.");
    }

    [Fact]
    [AllureDescription("TC-REP-API-015: AcceptReport fails with BadRequest when report status is not Pending.")]
    public async Task AcceptReport_WhenAlreadyAccepted_ShouldReturnBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var report = WasteReport.Create(Guid.NewGuid(), 1, 10m, 106m, "Desc", "Address");
        report.Accept(); // Transition to Accepted
        context.WasteReports.Add(report);
        await context.SaveChangesAsync();

        var controller = new ReportController(_mediatorMock.Object, context, _notificationServiceMock.Object)
        {
            ControllerContext = BuildControllerContext(Guid.NewGuid(), "Admin")
        };

        // Act
        var result = await controller.AcceptReportAndCreateTask(report.Id);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        GetPropertyValue<string>(badRequest.Value!, "message").Should().Contain("Report can only be accepted if it is in Pending status");
    }

    [Fact]
    [AllureDescription("TC-REP-API-016: RejectReport transitions status to Rejected, saves reason, and notifies citizen.")]
    public async Task RejectReport_WhenPending_ShouldRejectSuccessfully()
    {
        // Arrange
        await using var context = CreateContext();
        var report = WasteReport.Create(Guid.NewGuid(), 1, 10m, 106m, "Desc", "Address");
        context.WasteReports.Add(report);
        await context.SaveChangesAsync();

        _notificationServiceMock
            .Setup(x => x.NotifyReportRejectedAsync(report.CitizenId, report.Id, "Duplicate report", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller = new ReportController(_mediatorMock.Object, context, _notificationServiceMock.Object)
        {
            ControllerContext = BuildControllerContext(Guid.NewGuid(), "Admin")
        };

        var request = new RejectReportRequest { Reason = "Duplicate report" };

        // Act
        var result = await controller.RejectReport(report.Id, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        GetPropertyValue<string>(okResult.Value!, "message").Should().Be("Report rejected successfully");
        GetPropertyValue<Guid>(okResult.Value!, "reportId").Should().Be(report.Id);
        GetPropertyValue<string>(okResult.Value!, "reportStatus").Should().Be("Rejected");
        GetPropertyValue<string>(okResult.Value!, "rejectionReason").Should().Be("Duplicate report");

        var updatedReport = await context.WasteReports.FindAsync(report.Id);
        updatedReport!.Status.Should().Be(ReportStatus.Rejected);

        _notificationServiceMock.Verify(
            x => x.NotifyReportRejectedAsync(report.CitizenId, report.Id, "Duplicate report", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("TC-REP-API-017: RejectReport fails with BadRequest when report status is not Pending.")]
    public async Task RejectReport_WhenAlreadyRejected_ShouldReturnBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var report = WasteReport.Create(Guid.NewGuid(), 1, 10m, 106m, "Desc", "Address");
        report.Reject(); // Transition to Rejected
        context.WasteReports.Add(report);
        await context.SaveChangesAsync();

        var controller = new ReportController(_mediatorMock.Object, context, _notificationServiceMock.Object)
        {
            ControllerContext = BuildControllerContext(Guid.NewGuid(), "Admin")
        };

        var request = new RejectReportRequest { Reason = "Already rejected" };

        // Act
        var result = await controller.RejectReport(report.Id, request);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        GetPropertyValue<string>(badRequest.Value!, "message").Should().Contain("Report can only be rejected if it is in Pending status");
    }

    [Fact]
    [AllureDescription("TC-REP-API-018: GetEnterpriseAvailableReports yields paged reports matching their profile.")]
    public async Task GetEnterpriseAvailableReports_WithValidEnterprise_ShouldReturnOk()
    {
        // Arrange
        await using var context = CreateContext();
        var enterpriseUser = User.Create("ent@example.com", "hash", "Enterprise One", UserRole.Enterprise);
        var enterprise = new Enterprise { Id = Guid.NewGuid(), UserId = enterpriseUser.Id, CompanyName = "Enterprise One", User = enterpriseUser };

        context.Users.Add(enterpriseUser);
        context.Enterprises.Add(enterprise);
        await context.SaveChangesAsync();

        var reportsList = new List<ReportListDto> { new() { Id = Guid.NewGuid(), Status = ReportStatus.Pending } };
        var queryResult = (Reports: (IEnumerable<ReportListDto>)reportsList, Total: 1, TotalPages: 1);

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetEnterpriseReportsQuery>(q => q.EnterpriseId == enterprise.Id && q.Page == 1 && q.PageSize == 10), It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult);

        var controller = new ReportController(_mediatorMock.Object, context, _notificationServiceMock.Object)
        {
            ControllerContext = BuildControllerContext(enterpriseUser.Id, "Enterprise")
        };

        // Act
        var result = await controller.GetEnterpriseAvailableReports(page: 1, pageSize: 10);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value!;
        GetPropertyValue<string>(value, "message").Should().Be("Available reports retrieved successfully");

        var reports = GetPropertyValue<IEnumerable<ReportListDto>>(value, "reports");
        reports.Should().HaveCount(1);
    }

    private static ControllerContext BuildControllerContext(Guid userId, string role)
    {
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                        new Claim(ClaimTypes.Role, role)
                    },
                    "TestAuth"))
            }
        };
    }

    private static WastePlatformDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<WastePlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new WastePlatformDbContext(options);
    }

    private static T? GetPropertyValue<T>(object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (property is null)
        {
            return default;
        }

        return (T?)property.GetValue(obj);
    }
}
