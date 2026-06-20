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

[AllureEpic("KIEM-15: CollectorTask Module Testing")]
[AllureFeature("Task Lifecycle")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Set on the way and complete collection tasks")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "CollectorTaskControllerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("Minh Phụng")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("api")]
[Allure.Net.Commons.Attributes.AllureTag("collector")]
[Allure.Net.Commons.Attributes.AllureTag("task")]
[Allure.Net.Commons.Attributes.AllureIssue("KIEM-15")]
public class CollectorTaskControllerTests
{
    [Fact]
    [AllureDescription("TC-TASK-003: Collector sets task OnTheWay — status updates, SignalR broadcasts, and citizen is notified.")]
    public async Task SetOnTheWay_WhenTaskBelongsToCollector_ShouldUpdateStatusBroadcastAndNotify()
    {
        await using var context = CreateContext();
        var scenario = await SeedCollectorScenarioAsync(context);

        var hubContext = CreateHubContextMock(out var allClient, out _);
        allClient
            .Setup(x => x.SendCoreAsync("TaskStatusUpdated", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var notificationService = new Mock<INotificationService>();
        notificationService
            .Setup(x => x.NotifyCollectorOnTheWayAsync(scenario.CitizenUser.Id, scenario.Report.Id, scenario.CollectorUser.FullName, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller = new CollectorTaskController(
            context,
            hubContext.Object,
            new Mock<IMediator>().Object,
            notificationService.Object)
        {
            ControllerContext = BuildControllerContext(scenario.CollectorUser.Id)
        };

        var result = await controller.SetOnTheWay(scenario.Task.Id);

        AllureAttachmentHelper.AttachJson("set-on-the-way-request", new { TaskId = scenario.Task.Id, CollectorId = scenario.Collector.Id });

        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        AllureAttachmentHelper.AttachJson("set-on-the-way-response", okResult.Value!);
        scenario.Task.Status.Should().Be(CollectionTaskStatus.OnTheWay);
        scenario.Task.StatusLogs.Should().ContainSingle(log => log.Status == CollectionTaskStatus.OnTheWay);

        await context.Entry(scenario.Task).ReloadAsync();
        context.CollectionTasks.Single(t => t.Id == scenario.Task.Id).Status.Should().Be(CollectionTaskStatus.OnTheWay);

        allClient.Verify(
            x => x.SendCoreAsync("TaskStatusUpdated", It.Is<object?[]>(args => (Guid)args[0]! == scenario.Task.Id && (string)args[1]! == CollectionTaskStatus.OnTheWay.ToString()), It.IsAny<CancellationToken>()),
            Times.Once);
        notificationService.VerifyAll();
    }

    [Fact]
    [AllureDescription("TC-TASK-004: Collector completes task with weight and notes — reward points created and SignalR broadcasts to all and user.")]
    public async Task CompleteTask_WithRewardRule_ShouldCollectTaskCreateRewardAndBroadcast()
    {
        await using var context = CreateContext();
        var scenario = await SeedCollectorScenarioAsync(context, includeRewardRule: true);

        var hubContext = CreateHubContextMock(out var allClient, out var userClient);
        allClient
            .Setup(x => x.SendCoreAsync("TaskStatusUpdated", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        userClient
            .Setup(x => x.SendCoreAsync("RewardReceived", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller = new CollectorTaskController(
            context,
            hubContext.Object,
            new Mock<IMediator>().Object,
            new Mock<INotificationService>().Object)
        {
            ControllerContext = BuildControllerContext(scenario.CollectorUser.Id)
        };

        var form = new FormCollection(
            new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["WeightKg"] = "12.5",
                ["Notes"] = "Collected at front gate"
            },
            new FormFileCollection());

        AllureAttachmentHelper.AttachJson("complete-task-form", new { TaskId = scenario.Task.Id, WeightKg = "12.5", Notes = "Collected at front gate" });

        var result = await controller.CompleteTask(scenario.Task.Id, form);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        AllureAttachmentHelper.AttachJson("complete-task-response", okResult.Value!);

        var updatedTask = await context.CollectionTasks
            .Include(t => t.WasteReport)
            .SingleAsync(t => t.Id == scenario.Task.Id);

        updatedTask.Status.Should().Be(CollectionTaskStatus.Collected);
        updatedTask.CollectedWeightKg.Should().Be(12.5m);
        updatedTask.Notes.Should().Be("Collected at front gate");
        updatedTask.WasteReport.Status.Should().Be(ReportStatus.Collected);

        var rewardPoint = await context.RewardPoints.SingleAsync();
        rewardPoint.Points.Should().Be(15);
        rewardPoint.Reason.Should().Be($"Reward for collected waste report {scenario.Report.Id}");
        // Attach reward information
        AllureAttachmentHelper.AttachJson("reward-point", new { rewardPoint.Id, rewardPoint.Points, rewardPoint.Reason });

        allClient.Verify(
            x => x.SendCoreAsync("TaskStatusUpdated", It.Is<object?[]>(args => (Guid)args[0]! == scenario.Task.Id && (string)args[1]! == CollectionTaskStatus.Collected.ToString()), It.IsAny<CancellationToken>()),
            Times.Once);
        userClient.Verify(
            x => x.SendCoreAsync("RewardReceived", It.Is<object?[]>(args => (int)args[0]! == 15), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("TC-TASK-004: CompleteTask rejects non-numeric WeightKg with 400 Bad Request.")]
    public async Task CompleteTask_WithInvalidWeight_ShouldReturnBadRequest()
    {
        await using var context = CreateContext();
        var scenario = await SeedCollectorScenarioAsync(context, includeRewardRule: false);

        var controller = new CollectorTaskController(
            context,
            CreateHubContextMock(out _, out _).Object,
            new Mock<IMediator>().Object,
            new Mock<INotificationService>().Object)
        {
            ControllerContext = BuildControllerContext(scenario.CollectorUser.Id)
        };

        var form = new FormCollection(
            new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["WeightKg"] = "not-a-number"
            },
            new FormFileCollection());

        // Attach invalid form data for debugging
        AllureAttachmentHelper.AttachJson("complete-task-invalid-form", new { TaskId = scenario.Task.Id, WeightKg = "not-a-number" });

        var result = await controller.CompleteTask(scenario.Task.Id, form);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var message = GetPropertyValue<string>(badRequest.Value!, "message");
        AllureAttachmentHelper.AttachText("complete-task-invalid-message", message ?? string.Empty);
        message.Should().Be("Invalid or missing WeightKg.");
    }

    private static async Task<CollectorScenario> SeedCollectorScenarioAsync(WastePlatformDbContext context, bool includeRewardRule = false)
    {
        var enterpriseUser = User.Create("enterprise@example.com", "hash", "Enterprise One", UserRole.Enterprise);
        var collectorUser = User.Create("collector@example.com", "hash", "Collector One", UserRole.Collector);
        var citizenUser = User.Create("citizen@example.com", "hash", "Citizen One", UserRole.Citizen);

        var enterprise = new Enterprise
        {
            Id = Guid.NewGuid(),
            UserId = enterpriseUser.Id,
            CompanyName = "Enterprise One",
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

        var report = WasteReport.Create(citizenUser.Id, wasteCategoryId: 1, latitude: 10m, longitude: 106m, description: "Test report", address: "Test address");
        var task = CollectionTask.Create(report.Id, enterprise.Id);
        task.AssignCollector(collector.Id);

        if (includeRewardRule)
        {
            task.SetOnTheWay();
        }

        context.Users.AddRange(enterpriseUser, collectorUser, citizenUser);
        context.Enterprises.Add(enterprise);
        context.Collectors.Add(collector);
        context.WasteReports.Add(report);
        context.CollectionTasks.Add(task);

        if (includeRewardRule)
        {
            context.RewardRules.Add(new RewardRule
            {
                Id = Guid.NewGuid(),
                EnterpriseId = enterprise.Id,
                WasteCategoryId = 1,
                PointsPerReport = 10,
                BonusQuality = 5,
                IsActive = true
            });
        }

        await context.SaveChangesAsync();

        return new CollectorScenario(enterpriseUser, collectorUser, citizenUser, enterprise, collector, report, task);
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

    private static Mock<IHubContext<TaskHub>> CreateHubContextMock(out Mock<IClientProxy> allClient, out Mock<IClientProxy> userClient)
    {
        allClient = new Mock<IClientProxy>();
        userClient = new Mock<IClientProxy>();

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
        if (property is null)
        {
            return default;
        }

        return (T?)property.GetValue(obj);
    }

    private sealed record CollectorScenario(
        User EnterpriseUser,
        User CollectorUser,
        User CitizenUser,
        Enterprise Enterprise,
        Collector Collector,
        WasteReport Report,
        CollectionTask Task);
}