using System.Security.Claims;
using System.Text.Json;
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

[AllureEpic("Enterprise Operations")]
[AllureFeature("Task Assignment")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Assign collector to a waste collection task")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "EnterpriseTaskControllerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("backend")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("api")]
[Allure.Net.Commons.Attributes.AllureTag("enterprise")]
[Allure.Net.Commons.Attributes.AllureTag("task")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-16")]
public class EnterpriseTaskControllerTests
{
    [Fact]
    [AllureDescription("Returns the assigned tasks for a valid enterprise user.")]
    public async Task GetTasks_WithValidEnterprise_ShouldReturnEnterpriseTasks()
    {
        await using var context = CreateContext();
        var scenario = await SeedEnterpriseScenarioAsync(context);

        var controller = new EnterpriseTaskController(
            context,
            CreateHubContextMock(out _).Object,
            new Mock<INotificationService>().Object,
            new Mock<IMediator>().Object)
        {
            ControllerContext = BuildControllerContext(scenario.EnterpriseUser.Id)
        };

        var result = await controller.GetTasks();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(okResult.Value);

        json.Should().Contain(scenario.Task.Id.ToString());
        json.Should().Contain(scenario.Report.Id.ToString());
        json.Should().Contain("Assigned");
        json.Should().Contain("Test report");

        AllureAttachmentHelper.AttachJson("enterprise-tasks-result", new { taskId = scenario.Task.Id, reportId = scenario.Report.Id, enterpriseId = scenario.Enterprise.Id });
    }

    [Fact]
    [AllureDescription("Returns the collectors that belong to the enterprise and are available for assignment.")]
    public async Task GetAvailableCollectors_WithValidEnterprise_ShouldReturnCollectors()
    {
        await using var context = CreateContext();
        var scenario = await SeedEnterpriseScenarioAsync(context);

        var controller = new EnterpriseTaskController(
            context,
            CreateHubContextMock(out _).Object,
            new Mock<INotificationService>().Object,
            new Mock<IMediator>().Object)
        {
            ControllerContext = BuildControllerContext(scenario.EnterpriseUser.Id)
        };

        var result = await controller.GetAvailableCollectors();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(okResult.Value);

        json.Should().Contain(scenario.Collector.Id.ToString());
        json.Should().Contain("Collector One");
        json.Should().Contain("collector@example.com");

        AllureAttachmentHelper.AttachJson("enterprise-available-collectors-result", new { collectorId = scenario.Collector.Id, enterpriseId = scenario.Enterprise.Id });
    }

    [Fact]
    [AllureDescription("Assigns a collector to a task and notifies downstream systems when the request is valid.")]
    public async Task AssignCollector_WhenRequestIsValid_ShouldBroadcastAndNotifyCitizen()
    {
        await using var context = CreateContext();
        var scenario = await SeedEnterpriseScenarioAsync(context);

        var hubContext = CreateHubContextMock(out var allClient);
        allClient
            .Setup(x => x.SendCoreAsync("TaskStatusUpdated", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var notificationService = new Mock<INotificationService>();
        notificationService
            .Setup(x => x.NotifyReportAssignedAsync(scenario.CitizenUser.Id, scenario.Report.Id, scenario.CollectorUser.FullName, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller = new EnterpriseTaskController(
            context,
            hubContext.Object,
            notificationService.Object,
            new Mock<IMediator>().Object)
        {
            ControllerContext = BuildControllerContext(scenario.EnterpriseUser.Id)
        };

        // Attach assign-collector request for Allure
        AllureAttachmentHelper.AttachJson("assign-collector-request", new { TaskId = scenario.Task.Id, CollectorId = scenario.Collector.Id });

        var result = await controller.AssignCollector(scenario.Task.Id, new AssignCollectorRequest
        {
            CollectorId = scenario.Collector.Id
        });
        result.Should().BeOfType<OkObjectResult>();

        var updatedTask = await context.CollectionTasks.SingleAsync(t => t.Id == scenario.Task.Id);
        updatedTask.CollectorId.Should().Be(scenario.Collector.Id);
        AllureAttachmentHelper.AttachJson("assign-collector-result", new { updatedTask.Id, updatedTask.CollectorId });

        allClient.Verify(
            x => x.SendCoreAsync("TaskStatusUpdated", It.Is<object?[]>(args => (Guid)args[0]! == scenario.Task.Id && (string)args[1]! == CollectionTaskStatus.Assigned.ToString()), It.IsAny<CancellationToken>()),
            Times.Once);
        notificationService.Verify(
            x => x.NotifyReportAssignedAsync(scenario.CitizenUser.Id, scenario.Report.Id, scenario.CollectorUser.FullName, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("Returns a bad request when the selected collector does not belong to the enterprise.")]
    public async Task AssignCollector_WithUnknownCollector_ShouldReturnBadRequest()
    {
        await using var context = CreateContext();
        var scenario = await SeedEnterpriseScenarioAsync(context);

        var controller = new EnterpriseTaskController(
            context,
            CreateHubContextMock(out _).Object,
            new Mock<INotificationService>().Object,
            new Mock<IMediator>().Object)
        {
            ControllerContext = BuildControllerContext(scenario.EnterpriseUser.Id)
        };

        var result = await controller.AssignCollector(scenario.Task.Id, new AssignCollectorRequest
        {
            CollectorId = Guid.NewGuid()
        });

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        GetPropertyValue<string>(badRequest.Value!, "message").Should().Be("Collector not found or does not belong to your enterprise.");

        AllureAttachmentHelper.AttachText("assign-collector-invalid-result", "Collector not found or does not belong to your enterprise.");
    }

    private static async Task<EnterpriseScenario> SeedEnterpriseScenarioAsync(WastePlatformDbContext context)
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

        context.Users.AddRange(enterpriseUser, collectorUser, citizenUser);
        context.Enterprises.Add(enterprise);
        context.Collectors.Add(collector);
        context.WasteReports.Add(report);
        context.CollectionTasks.Add(task);
        await context.SaveChangesAsync();

        return new EnterpriseScenario(enterpriseUser, collectorUser, citizenUser, enterprise, collector, report, task);
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
                        new Claim(ClaimTypes.Role, "Enterprise")
                    ],
                    "TestAuth"))
            }
        };
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

    private sealed record EnterpriseScenario(
        User EnterpriseUser,
        User CollectorUser,
        User CitizenUser,
        Enterprise Enterprise,
        Collector Collector,
        WasteReport Report,
        CollectionTask Task);
}