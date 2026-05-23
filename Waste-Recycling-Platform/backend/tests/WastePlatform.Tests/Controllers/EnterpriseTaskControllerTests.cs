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
using Allure.Xunit.Attributes;

namespace WastePlatform.Tests.Controllers;

[AllureEpic("KIEM-16 Enterprise Task Module")]
[AllureFeature("WRP-BE-TESTS-013 Enterprise Task Controller")]
public class EnterpriseTaskControllerTests
{
    [Fact]
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

        var result = await controller.AssignCollector(scenario.Task.Id, new AssignCollectorRequest
        {
            CollectorId = scenario.Collector.Id
        });

        result.Should().BeOfType<OkObjectResult>();

        var updatedTask = await context.CollectionTasks.SingleAsync(t => t.Id == scenario.Task.Id);
        updatedTask.CollectorId.Should().Be(scenario.Collector.Id);

        allClient.Verify(
            x => x.SendCoreAsync("TaskStatusUpdated", It.Is<object?[]>(args => (Guid)args[0]! == scenario.Task.Id && (string)args[1]! == CollectionTaskStatus.Assigned.ToString()), It.IsAny<CancellationToken>()),
            Times.Once);
        notificationService.Verify(
            x => x.NotifyReportAssignedAsync(scenario.CitizenUser.Id, scenario.Report.Id, scenario.CollectorUser.FullName, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
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