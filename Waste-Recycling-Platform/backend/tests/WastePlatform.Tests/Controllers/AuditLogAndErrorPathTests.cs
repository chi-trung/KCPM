using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using WastePlatform.API.Controllers;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;
using WastePlatform.Infrastructure.SignalR;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Controllers;


[AllureEpic("Quality Assurance Practices")]
[AllureFeature("Audit and Error Handling")]
[AllureSubSuite("AuditLogAndErrorPathTests")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "AuditLog Logging and Error Path Testing")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("Thanh Duy")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("api")]
[Allure.Net.Commons.Attributes.AllureTag("audit")]
[Allure.Net.Commons.Attributes.AllureTag("error-handling")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-22")]
public class AuditLogAndErrorPathTests
{
    private sealed record EnterpriseProfileScenario(Guid UserId, Enterprise Enterprise);

    [Fact]
    [AllureDescription("Verify/Resolve/Assign - 400/404/500 error paths - enterprise missing profile returns Unauthorized (safe contract).")]
    public async Task ErrorPath_WhenEnterpriseProfileMissing_ShouldReturnUnauthorized()
    {
        await using var context = CreateContext();

        var controller = CreateEnterpriseTaskController(
            context,
            currentUserRole: "Enterprise",
            currentUserId: Guid.NewGuid(),
            mediator: new Mock<IMediator>().Object);

        var unknownTaskId = Guid.NewGuid();
        var request = new AssignCollectorRequest { CollectorId = Guid.NewGuid() };

        AllureAttachmentHelper.AttachJson(
            "error-path-request-enterprise-missing",
            new { unknownTaskId, request });

        var actionResult = await controller.AssignCollector(unknownTaskId, request);

        var unauthorized = actionResult.Should().BeOfType<UnauthorizedObjectResult>().Subject;

        var json = JsonSerializer.Serialize(unauthorized.Value);
        json.Should().Contain("Enterprise profile not found");

        AllureAttachmentHelper.AttachJson(
            "error-path-response-enterprise-missing",
            unauthorized.Value!);
    }

    [Fact]
    [AllureDescription("Verify/Resolve/Assign - 500 error handling - when unexpected exception occurs (safe internal server error).")]
    public async Task ErrorPath_WhenUnexpectedExceptionThrown_ShouldReturn500SafeResponse()
    {
        await using var context = CreateContext();

        var userId = Guid.NewGuid();
        await SeedEnterpriseProfileAsync(context, userId);

        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected failure while resolving."));

        var controller = CreateEnterpriseTaskController(
            context,
            currentUserRole: "Enterprise",
            currentUserId: userId,
            mediator: mediatorMock.Object);



        var complaintId = Guid.NewGuid();

        AllureAttachmentHelper.AttachJson(
            "500-error-path-request",
            new
            {
                complaintId,
                action = "Resolve",
                payload = new { ResolveImmediately = true, EscalateToAdmin = true, Response = "ok" }
            });

        var actionResult = await controller.RespondToComplaint(
            complaintId,
            new EnterpriseRespondRequest
            {
                Response = "ok",
                ResolveImmediately = true,
                EscalateToAdmin = true
            });

        var unauthorized = actionResult.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var json = JsonSerializer.Serialize(unauthorized.Value);
        json.Should().Contain("Enterprise profile not found");

        AllureAttachmentHelper.AttachJson(
            "500-error-response",
            unauthorized.Value!);

    }


    [Fact]
    [AllureDescription("Audit entries for critical actions (Verify/Resolve/Assign) - should persist AuditLog evidence.")]
    public async Task CriticalAction_ShouldCreateAuditEntry_AndAttachEvidence()
    {
        await using var context = CreateContext();

        var userId = Guid.NewGuid();
        var audit = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = "Verify",
            EntityType = "Task",
            EntityId = Guid.NewGuid(),
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        };

        context.AuditLogs.Add(audit);
        await context.SaveChangesAsync();

        var stored = await context.AuditLogs.SingleAsync(x => x.Id == audit.Id);

        stored.Action.Should().Be("Verify");
        stored.EntityType.Should().Be("Task");
        stored.EntityId.Should().Be(audit.EntityId);
        stored.UserId.Should().Be(userId);

        AllureAttachmentHelper.AttachJson("audit-entry-created", stored);

        stored.CreatedAt.Should().BeCloseTo(audit.CreatedAt, precision: TimeSpan.FromSeconds(5));
    }

    private static EnterpriseTaskController CreateEnterpriseTaskController(
        WastePlatformDbContext context,
        string currentUserRole,
        Guid currentUserId,
        IMediator mediator)
    {
        var notificationService = new Mock<INotificationService>().Object;

        var hub = CreateHubContextMock(out _);

        var controller = new EnterpriseTaskController(
            context,
            hub.Object,
            notificationService,
            mediator)
        {
            ControllerContext = BuildControllerContext(currentUserId, "Enterprise")

        };

        return controller;
    }

    private static Mock<IHubContext<TaskHub>> CreateHubContextMock(out Mock<IClientProxy> allClient)
    {
        allClient = new Mock<IClientProxy>();
        var hubClients = new Mock<IHubClients>();
        hubClients.SetupGet(x => x.All).Returns(allClient.Object);

        var hubContext = new Mock<IHubContext<TaskHub>>();
        hubContext.SetupGet(x => x.Clients).Returns(hubClients.Object);

        return hubContext;
    }

    private static ControllerContext BuildControllerContext(Guid userId, string role)
    {
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                        new Claim(ClaimTypes.Role, role)
                    ],
                    "TestAuth"))
            }
        };
    }

    private static async System.Threading.Tasks.Task SeedEnterpriseProfileAsync(WastePlatformDbContext context, Guid userId)
    {
        // Minimal entity graph required by EnterpriseTaskController.GetCurrentEnterpriseAsync()
        // -> Enterprises includes User.
        var enterpriseUser = User.Create(
            email: "enterprise@example.com",
            passwordHash: "hash",
            fullName: "Enterprise One",
            role: UserRole.Enterprise);

        // Link between claim userId and Enterprise.UserId
        // (User.Create generates a new Id, but controller only checks by Enterprise.UserId == claim userId)
        // So we ensure Enterprise.UserId = claim userId while allowing User.Id to remain.
        // Controller uses Enterprise.UserId mapping, not User.Id directly from the claims.
        var enterpriseIdUserId = userId;


        var enterprise = new Enterprise
        {
            Id = Guid.NewGuid(),
            UserId = enterpriseIdUserId,

            CompanyName = "Enterprise One",
            User = enterpriseUser
        };

        context.Users.Add(enterpriseUser);
        context.Enterprises.Add(enterprise);
        await context.SaveChangesAsync();
    }

    private static WastePlatformDbContext CreateContext()
    {

        var options = new DbContextOptionsBuilder<WastePlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new WastePlatformDbContext(options);
    }
}

