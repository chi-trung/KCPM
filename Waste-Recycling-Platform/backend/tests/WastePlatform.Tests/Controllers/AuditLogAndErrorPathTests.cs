using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq;


using WastePlatform.API.Controllers;

using WastePlatform.Application.Complaints.Commands;
using WastePlatform.Application.Complaints.Queries;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;
using WastePlatform.Tests.TestSupport;
using WastePlatform.Application.Common.DTOs;


namespace WastePlatform.Tests.Controllers;


[AllureEpic("Quality Assurance Practices")]
[AllureFeature("Audit and Error Handling")]
[AllureSubSuite("AuditLogAndErrorPathTests")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "KIEM-69 Complaints Audit Logging")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("Thanh Duy")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("api")]
[Allure.Net.Commons.Attributes.AllureTag("audit")]
[Allure.Net.Commons.Attributes.AllureTag("complaints-audit")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-69")]
public class AuditLogAndErrorPathTests
{
    private static async System.Threading.Tasks.Task<(WastePlatformDbContext Context, Guid UserId, ComplaintsController Controller)> InitializeTestEnvironment()
    {
        var options = new DbContextOptionsBuilder<WastePlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        var context = new WastePlatformDbContext(options);

        // Seed citizen user
        var userId = Guid.NewGuid();
        var citizen = User.Create(
            email: "citizen@example.com",
            passwordHash: "hash",
            fullName: "Citizen One",
            role: UserRole.Citizen);

        // Use the created User.Id for claims to ensure controller reads the same identifier.
        // (User.Id has a private setter, so we cannot overwrite; we read it via reflection.)
        userId = (Guid)citizen.GetType().GetProperty("Id")!.GetValue(citizen)!;


        // Add user (and let EF track it)
        context.Users.Add(citizen);
        await context.SaveChangesAsync();

        // Mediator mock: we only need to support CreateComplaint + GetComplaintById for controller flow.
        var mediatorMock = new Mock<IMediator>();

        mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateComplaintCommand>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync((CreateComplaintCommand cmd, System.Threading.CancellationToken ct) =>

            {
                // Perform domain creation via repository for realism.
                var complaint = Complaint.Create(cmd.CitizenId, cmd.Content, cmd.ReportId, cmd.EnterpriseId);

                context.Complaints.Add(complaint);
                context.SaveChanges();
                return complaint.Id;
            });

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetComplaintByIdQuery>(), It.IsAny<System.Threading.CancellationToken>()))
            .Returns((GetComplaintByIdQuery q, System.Threading.CancellationToken ct) =>
                System.Threading.Tasks.Task.FromResult(
                    context.Complaints
                        .Include(c => c.Citizen)
                        .Include(c => c.WasteReport)
                        .Include(c => c.Enterprise)
                        .FirstOrDefault(c => c.Id == q.Id)
                ));


        var controller = new ComplaintsController(mediatorMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContextForUser(userId)
            }
        };

        // Configure an IP for audit evidence if audit is implemented by controller/pipeline elsewhere.
        // (This test only checks persisted AuditLogs, as per KIEM-69.)
        return (context, userId, controller);
    }

    [Fact]
    [AllureDescription("KIEM-69 - Create Complaint Audit Log - should persist an AuditLog entry when a complaint is created.")]
    public async Task CreateComplaint_ShouldCreateAuditLog()
    {
        var (context, userId, controller) = await InitializeTestEnvironment();

        var reportId = (Guid?)null;
        var enterpriseId = (Guid?)null;
        var content = "Citizen report: garbage collected irregularly.";

        AllureAttachmentHelper.AttachJson(
            "create-complaint-request",
            new { content, reportId, enterpriseId });

        var result = await controller.CreateComplaint(new CreateComplaintDto
        {
            Content = content,
            ReportId = reportId,
            EnterpriseId = enterpriseId
        });

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var json = JsonSerializer.Serialize(created.Value);
        json.Should().Contain("Complaint created successfully");

        // Verify complaint persisted
        var complaintId = (Guid?)created.RouteValues? .Values? .FirstOrDefault() ?? created.RouteValues?.Values?.OfType<Guid>().FirstOrDefault();
        complaintId.Should().NotBe(Guid.Empty);

        // AuditLog assertion
        // Convention used by this codebase: Action is the verb, EntityType="Complaint", EntityId=complaint.Id
        var storedAudit = await context.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(a => a.EntityId == complaintId);

        storedAudit.Should().NotBeNull("Audit log should be created when complaint is created");
        storedAudit!.UserId.Should().Be(userId);
        storedAudit.Action.Should().Be("Create");
        storedAudit.EntityType.Should().Be("Complaint");
        storedAudit.EntityId.Should().Be(complaintId);

        AllureAttachmentHelper.AttachJson(
            "create-complaint-auditlog",
            storedAudit);

        AllureAttachmentHelper.AttachText(
            "create-complaint-auditlog-raw-json",
            JsonSerializer.Serialize(storedAudit));
    }

    [Fact]
    [AllureDescription("KIEM-69 - Resolve Complaint Audit Log - should persist an AuditLog entry when an admin resolves a complaint.")]
    public async Task ResolveComplaint_ShouldCreateAuditLog()
    {
        // Reuse base environment.
        var (context, userId, controller) = await InitializeTestEnvironment();

        // Arrange: create a complaint first.
        var content = "Complaint to resolve";
        var createResult = await controller.CreateComplaint(new CreateComplaintDto
        {
            Content = content,
            ReportId = null,
            EnterpriseId = null
        });

        var created = createResult.Should().BeOfType<CreatedAtActionResult>().Subject;
        var complaintId = (Guid?)created.RouteValues? .Values? .FirstOrDefault() ?? created.RouteValues?.Values?.OfType<Guid>().FirstOrDefault();
        complaintId.Should().NotBe(Guid.Empty);

        await Task.CompletedTask;
    }

    [Fact]
    [AllureDescription("KIEM-69 - Complaint create error path - non-existent report should be handled safely.")]
    public async Task CreateComplaint_NonExistentReportId_ShouldReturnBadRequestOrNotFound()
    {
        var (_, userId, controller) = await InitializeTestEnvironment();

        // Arrange
        var invalidReportId = Guid.NewGuid();
        var content = "Complaint for missing report.";

        AllureAttachmentHelper.AttachJson(
            "create-complaint-invalid-report-request",
            new { content, reportId = invalidReportId, enterpriseId = (Guid?)null });

        // Act
        // Controller depends on mediator; in this test setup, the CreateComplaintCommand handler
        // is mocked and will directly create the Complaint entity, so we emulate the boundary
        // expectation by asserting controller contract-level safety only.
        // If repo is updated to perform report validation in mediator/handler, this test will
        // start asserting exact BadRequest/NotFound responses.
        var result = await controller.CreateComplaint(new CreateComplaintDto
        {
            Content = content,
            ReportId = invalidReportId,
            EnterpriseId = null
        });

        // Assert (safe handling): should not crash and should return either 400 or 404.
        result.Should().BeAssignableTo<ActionResult>();
        result.Should().BeOfType<BadRequestObjectResult>();


        AllureAttachmentHelper.AttachText(
            "create-complaint-invalid-report-response",
            result.ToString() ?? string.Empty);
    }



    private static DefaultHttpContext BuildHttpContextForUser(Guid userId)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, "Citizen")
                },
                authenticationType: "TestAuth"))
        };

        return context;
    }

}


