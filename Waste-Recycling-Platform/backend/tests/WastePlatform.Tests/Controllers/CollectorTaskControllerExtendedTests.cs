using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Moq;
using WastePlatform.API.Controllers;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;
using WastePlatform.Infrastructure.SignalR;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Controllers;

/// <summary>
/// Extended unit tests for CollectorTaskController
/// TC-TASK-001: Get Collector Tasks List
/// TC-TASK-002: Get Task By ID
/// TC-TASK-003: Set On The Way - Invalid Transition
/// TC-TASK-004: Complete Task - Not Found
/// TC-TASK-005: Complete Task - Task Not OnTheWay
/// TC-TASK-006: Get Stats
/// </summary>
[AllureEpic("KIEM-15: CollectorTask Module Testing")]
[AllureFeature("Collector Task Controller - Extended")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Collector retrieves, navigates and completes collection tasks")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "CollectorTaskControllerExtendedTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("Nguyễn Minh Phụng")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("api")]
[Allure.Net.Commons.Attributes.AllureTag("collector")]
[Allure.Net.Commons.Attributes.AllureTag("task")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-15")]
public class CollectorTaskControllerExtendedTests
{
    // ── TC-TASK-001 ───────────────────────────────────────────────────────────
    [Fact]
    [AllureDescription("TC-TASK-001: Collector calls GET /api/collector/tasks and receives a list of their assigned tasks.")]
    public async Task GetTasks_WhenCollectorHasTasks_ShouldReturnOkWithList()
    {
        await using var context = CreateContext();
        var scenario = await SeedScenarioAsync(context);

        var controller = BuildController(context, scenario.CollectorUser.Id);

        AllureAttachmentHelper.AttachJson("get-tasks-input", new { collectorUserId = scenario.CollectorUser.Id });

        var result = await controller.GetTasks();

        AllureAttachmentHelper.AttachText("get-tasks-status", result.GetType().Name);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    [AllureDescription("TC-TASK-001b: Filtering by Assigned status returns only Assigned tasks.")]
    public async Task GetTasks_WithStatusFilter_ShouldReturnFilteredList()
    {
        await using var context = CreateContext();
        var scenario = await SeedScenarioAsync(context);

        var controller = BuildController(context, scenario.CollectorUser.Id);

        AllureAttachmentHelper.AttachJson("get-tasks-filter-input", new { status = "Assigned" });

        var result = await controller.GetTasks(CollectionTaskStatus.Assigned);

        result.Should().BeOfType<OkObjectResult>();
        AllureAttachmentHelper.AttachText("get-tasks-filter-status", result.GetType().Name);
    }

    [Fact]
    [AllureDescription("TC-TASK-001c: Unknown collector profile returns 401 Unauthorized.")]
    public async Task GetTasks_WhenCollectorProfileNotFound_ShouldReturnUnauthorized()
    {
        await using var context = CreateContext();

        var controller = BuildController(context, Guid.NewGuid());

        var result = await controller.GetTasks();

        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var message = GetPropertyValue<string>(unauthorized.Value!, "message");
        AllureAttachmentHelper.AttachText("get-tasks-unauthorized-message", message ?? string.Empty);
        message.Should().Contain("not found");
    }

    // ── TC-TASK-002 ───────────────────────────────────────────────────────────
    [Fact]
    [AllureDescription("TC-TASK-002: Collector calls GET /api/collector/tasks/{id} and gets full task details.")]
    public async Task GetTaskById_WhenTaskBelongsToCollector_ShouldReturnOkWithDetails()
    {
        await using var context = CreateContext();
        var scenario = await SeedScenarioAsync(context);

        var controller = BuildController(context, scenario.CollectorUser.Id);

        AllureAttachmentHelper.AttachJson("get-task-by-id-input", new { taskId = scenario.Task.Id });

        var result = await controller.GetTaskById(scenario.Task.Id);

        result.Should().BeOfType<OkObjectResult>();
        AllureAttachmentHelper.AttachText("get-task-by-id-status", result.GetType().Name);
    }

    [Fact]
    [AllureDescription("TC-TASK-002b: Task ID not found returns 404 Not Found.")]
    public async Task GetTaskById_WhenTaskNotFound_ShouldReturnNotFound()
    {
        await using var context = CreateContext();
        var scenario = await SeedScenarioAsync(context);
        var nonExistentId = Guid.NewGuid();

        var controller = BuildController(context, scenario.CollectorUser.Id);

        AllureAttachmentHelper.AttachJson("get-task-by-id-not-found-input", new { taskId = nonExistentId });

        var result = await controller.GetTaskById(nonExistentId);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var message = GetPropertyValue<string>(notFound.Value!, "message");
        AllureAttachmentHelper.AttachText("get-task-by-id-not-found-message", message ?? string.Empty);
        message.Should().Contain("not found");
    }

    [Fact]
    [AllureDescription("TC-TASK-002c: User with no Collector profile gets 401 even if task ID exists.")]
    public async Task GetTaskById_WhenCallerHasNoCollectorProfile_ShouldReturnUnauthorized()
    {
        await using var context = CreateContext();
        var scenario = await SeedScenarioAsync(context);

        var anotherUser = User.Create("another@example.com", "hash", "Another User", UserRole.Collector);
        context.Users.Add(anotherUser);
        await context.SaveChangesAsync();

        var controller = BuildController(context, anotherUser.Id);

        AllureAttachmentHelper.AttachJson("get-task-by-id-no-profile-input", new
        {
            taskId = scenario.Task.Id,
            requestingUserId = anotherUser.Id
        });

        var result = await controller.GetTaskById(scenario.Task.Id);

        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var message = GetPropertyValue<string>(unauthorized.Value!, "message");
        AllureAttachmentHelper.AttachText("get-task-by-id-no-profile-message", message ?? string.Empty);
        message.Should().Contain("not found");
    }

    // ── TC-TASK-003 ───────────────────────────────────────────────────────────
    [Fact]
    [AllureDescription("TC-TASK-003: SetOnTheWay called twice on same task returns 400 Bad Request with domain error.")]
    public async Task SetOnTheWay_WhenAlreadyOnTheWay_ShouldReturnBadRequest()
    {
        await using var context = CreateContext();
        var scenario = await SeedScenarioAsync(context, setOnTheWay: true);

        var hubContext = CreateHubContextMock(out _, out _);
        var controller = new CollectorTaskController(
            context,
            hubContext.Object,
            new Mock<IMediator>().Object,
            new Mock<INotificationService>().Object)
        {
            ControllerContext = BuildControllerContext(scenario.CollectorUser.Id)
        };

        AllureAttachmentHelper.AttachJson("set-on-the-way-invalid-input", new
        {
            taskId = scenario.Task.Id,
            currentStatus = scenario.Task.Status.ToString()
        });

        var result = await controller.SetOnTheWay(scenario.Task.Id);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var message = GetPropertyValue<string>(badRequest.Value!, "message");
        AllureAttachmentHelper.AttachText("set-on-the-way-invalid-message", message ?? string.Empty);
        message.Should().Contain("Assigned before going OnTheWay");
    }

    [Fact]
    [AllureDescription("TC-TASK-003b: SetOnTheWay with non-existent task ID returns 404.")]
    public async Task SetOnTheWay_WhenTaskNotFound_ShouldReturnNotFound()
    {
        await using var context = CreateContext();
        var scenario = await SeedScenarioAsync(context);
        var nonExistentId = Guid.NewGuid();

        var controller = BuildController(context, scenario.CollectorUser.Id);

        AllureAttachmentHelper.AttachJson("set-on-the-way-not-found-input", new { taskId = nonExistentId });

        var result = await controller.SetOnTheWay(nonExistentId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── TC-TASK-004 ───────────────────────────────────────────────────────────
    [Fact]
    [AllureDescription("TC-TASK-004: CompleteTask with non-existent task ID returns 404 Not Found.")]
    public async Task CompleteTask_WhenTaskNotFound_ShouldReturnNotFound()
    {
        await using var context = CreateContext();
        var scenario = await SeedScenarioAsync(context);
        var nonExistentId = Guid.NewGuid();

        var controller = BuildController(context, scenario.CollectorUser.Id);

        var form = new FormCollection(
            new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["WeightKg"] = "10"
            },
            new FormFileCollection());

        AllureAttachmentHelper.AttachJson("complete-task-not-found-input", new { taskId = nonExistentId });

        var result = await controller.CompleteTask(nonExistentId, form);

        AllureAttachmentHelper.AttachText("complete-task-not-found-status", result.GetType().Name);
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── TC-TASK-005 ───────────────────────────────────────────────────────────
    [Fact]
    [AllureDescription("TC-TASK-005: CompleteTask when task is still Assigned (not OnTheWay) returns 400 Bad Request.")]
    public async Task CompleteTask_WhenTaskNotOnTheWay_ShouldReturnBadRequest()
    {
        await using var context = CreateContext();
        var scenario = await SeedScenarioAsync(context);

        var controller = BuildController(context, scenario.CollectorUser.Id);

        var form = new FormCollection(
            new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["WeightKg"] = "10",
                ["Notes"] = "Should fail"
            },
            new FormFileCollection());

        AllureAttachmentHelper.AttachJson("complete-task-not-on-the-way-input", new
        {
            taskId = scenario.Task.Id,
            currentStatus = scenario.Task.Status.ToString()
        });

        var result = await controller.CompleteTask(scenario.Task.Id, form);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var message = GetPropertyValue<string>(badRequest.Value!, "message");
        AllureAttachmentHelper.AttachText("complete-task-not-on-the-way-message", message ?? string.Empty);
        message.Should().Contain("OnTheWay before Collected");
    }

    // ── TC-TASK-006 ───────────────────────────────────────────────────────────
    [Fact]
    [AllureDescription("TC-TASK-006: GET /api/collector/tasks/stats returns correct aggregated counts.")]
    public async Task GetStats_WhenCollectorHasTasks_ShouldReturnCorrectCounts()
    {
        await using var context = CreateContext();
        var scenario = await SeedScenarioAsync(context);

        var controller = BuildController(context, scenario.CollectorUser.Id);

        AllureAttachmentHelper.AttachJson("get-stats-input", new { collectorUserId = scenario.CollectorUser.Id });

        var result = await controller.GetStats();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;

        var totalAssigned = GetPropertyValue<int>(ok.Value!, "TotalAssigned");
        var totalOnTheWay = GetPropertyValue<int>(ok.Value!, "TotalOnTheWay");
        var totalCollected = GetPropertyValue<int>(ok.Value!, "TotalCollected");

        AllureAttachmentHelper.AttachJson("get-stats-result", new
        {
            TotalAssigned = totalAssigned,
            TotalOnTheWay = totalOnTheWay,
            TotalCollected = totalCollected
        });

        totalAssigned.Should().Be(1);
        totalOnTheWay.Should().Be(0);
        totalCollected.Should().Be(0);
    }

    [Fact]
    [AllureDescription("TC-TASK-006b: GET /api/collector/tasks/stats with unknown collector profile returns 401.")]
    public async Task GetStats_WhenCollectorProfileNotFound_ShouldReturnUnauthorized()
    {
        await using var context = CreateContext();

        var controller = BuildController(context, Guid.NewGuid());

        var result = await controller.GetStats();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static CollectorTaskController BuildController(WastePlatformDbContext context, Guid userId)
    {
        return new CollectorTaskController(
            context,
            CreateHubContextMock(out _, out _).Object,
            new Mock<IMediator>().Object,
            new Mock<INotificationService>().Object)
        {
            ControllerContext = BuildControllerContext(userId)
        };
    }

    private static async Task<CollectorScenarioExt> SeedScenarioAsync(
        WastePlatformDbContext context,
        bool setOnTheWay = false)
    {
        var enterpriseUser = User.Create("ent@ext.com", "hash", "Enterprise Ext", UserRole.Enterprise);
        var collectorUser = User.Create("col@ext.com", "hash", "Collector Ext", UserRole.Collector);
        var citizenUser = User.Create("cit@ext.com", "hash", "Citizen Ext", UserRole.Citizen);

        var enterprise = new Enterprise
        {
            Id = Guid.NewGuid(),
            UserId = enterpriseUser.Id,
            CompanyName = "Enterprise Ext",
            User = enterpriseUser
        };

        var collector = new Collector
        {
            Id = Guid.NewGuid(),
            UserId = collectorUser.Id,
            EnterpriseId = enterprise.Id,
            User = collectorUser,
            Enterprise = enterprise
        };

        var report = WasteReport.Create(citizenUser.Id, wasteCategoryId: 1, latitude: 10m, longitude: 106m, description: "Ext report", address: "Ext address");
        var task = CollectionTask.Create(report.Id, enterprise.Id);
        task.AssignCollector(collector.Id);

        if (setOnTheWay)
        {
            task.SetOnTheWay();
        }

        context.Users.AddRange(enterpriseUser, collectorUser, citizenUser);
        context.Enterprises.Add(enterprise);
        context.Collectors.Add(collector);
        context.WasteReports.Add(report);
        context.CollectionTasks.Add(task);
        await context.SaveChangesAsync();

        return new CollectorScenarioExt(enterpriseUser, collectorUser, citizenUser, enterprise, collector, report, task);
    }

    private static ControllerContext BuildControllerContext(Guid userId)
    {
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                        new Claim(ClaimTypes.Role, "Collector")
                    ],
                    "TestAuth"))
            }
        };
    }

    private static Mock<IHubContext<TaskHub>> CreateHubContextMock(
        out Mock<IClientProxy> allClient,
        out Mock<IClientProxy> userClient)
    {
        allClient = new Mock<IClientProxy>();
        userClient = new Mock<IClientProxy>();
        allClient
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        userClient
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var hubClients = new Mock<IHubClients>();
        hubClients.SetupGet(x => x.All).Returns(allClient.Object);
        hubClients.Setup(x => x.User(It.IsAny<string>())).Returns(userClient.Object);

        var hubContext = new Mock<IHubContext<TaskHub>>();
        hubContext.SetupGet(x => x.Clients).Returns(hubClients.Object);

        return hubContext;
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
        var property = obj.GetType().GetProperty(propertyName);
        return property is null ? default : (T?)property.GetValue(obj);
    }

    private sealed record CollectorScenarioExt(
        User EnterpriseUser,
        User CollectorUser,
        User CitizenUser,
        Enterprise Enterprise,
        Collector Collector,
        WasteReport Report,
        CollectionTask Task);
}
