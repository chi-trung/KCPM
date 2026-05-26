using System.Security.Claims;
using Allure.Net.Commons;
using Allure.Xunit.Attributes;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WastePlatform.API.Controllers;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;

namespace WastePlatform.Tests.Controllers;

[AllureEpic("Collector Operations")]
[AllureFeature("Collector Profile")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Get profile and toggle availability for collector users")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "CollectorControllerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("backend")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("api")]
[Allure.Net.Commons.Attributes.AllureTag("collector")]
[Allure.Net.Commons.Attributes.AllureTag("profile")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-14")]
public class CollectorControllerTests
{
    [Fact]
    public async Task GetProfile_WithValidCollector_ShouldReturnCollectorProfile()
    {
        await using var context = CreateContext();
        var scenario = await SeedCollectorAsync(context);

        var controller = new CollectorController(context)
        {
            ControllerContext = BuildControllerContext(scenario.CollectorUser.Id)
        };

        var result = await controller.GetProfile();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

        GetPropertyValue<Guid>(okResult.Value!, "Id").Should().Be(scenario.Collector.Id);
        GetPropertyValue<Guid>(okResult.Value!, "UserId").Should().Be(scenario.CollectorUser.Id);
        GetPropertyValue<Guid>(okResult.Value!, "EnterpriseId").Should().Be(scenario.Enterprise.Id);
        GetPropertyValue<string>(okResult.Value!, "EnterpriseName").Should().Be("Enterprise One");
        GetPropertyValue<string>(okResult.Value!, "FullName").Should().Be("Collector One");
        GetPropertyValue<string>(okResult.Value!, "Email").Should().Be("collector@example.com");
        GetPropertyValue<string>(okResult.Value!, "Phone").Should().Be("0900000001");
        GetPropertyValue<bool>(okResult.Value!, "IsAvailable").Should().BeTrue();
    }

    [Fact]
    public async Task GetProfile_WithoutCollectorRecord_ShouldReturnUnauthorized()
    {
        await using var context = CreateContext();

        var user = User.Create("not-collector@example.com", "hash", "Not Collector", UserRole.Citizen);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = new CollectorController(context)
        {
            ControllerContext = BuildControllerContext(user.Id)
        };

        var result = await controller.GetProfile();

        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        GetPropertyValue<string>(unauthorized.Value!, "message").Should().Be("Không tìm thấy hồ sơ Collector.");
    }

    [Fact]
    public async Task ToggleAvailability_WithValidCollector_ShouldUpdateAvailability()
    {
        await using var context = CreateContext();
        var scenario = await SeedCollectorAsync(context, initialAvailability: false);

        var controller = new CollectorController(context)
        {
            ControllerContext = BuildControllerContext(scenario.CollectorUser.Id)
        };

        var result = await controller.ToggleAvailability(new ToggleAvailabilityRequest { IsAvailable = true });

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

        GetPropertyValue<string>(okResult.Value!, "message").Should().Be("Cập nhật trạng thái thành công.");
        GetPropertyValue<bool>(okResult.Value!, "isAvailable").Should().BeTrue();

        await context.Entry(scenario.Collector).ReloadAsync();
        scenario.Collector.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task ToggleAvailability_WithoutCollectorRecord_ShouldReturnUnauthorized()
    {
        await using var context = CreateContext();

        var user = User.Create("no-collector@example.com", "hash", "No Collector", UserRole.Citizen);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = new CollectorController(context)
        {
            ControllerContext = BuildControllerContext(user.Id)
        };

        var result = await controller.ToggleAvailability(new ToggleAvailabilityRequest { IsAvailable = true });

        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        GetPropertyValue<string>(unauthorized.Value!, "message").Should().Be("Không tìm thấy hồ sơ Collector.");
    }

    private static async Task<CollectorScenario> SeedCollectorAsync(WastePlatformDbContext context, bool initialAvailability = true)
    {
        var enterpriseUser = User.Create("enterprise@example.com", "hash", "Enterprise One", UserRole.Enterprise, phone: "0900000002");
        var collectorUser = User.Create("collector@example.com", "hash", "Collector One", UserRole.Collector, phone: "0900000001");

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
            IsAvailable = initialAvailability,
            User = collectorUser,
            Enterprise = enterprise
        };

        context.Users.AddRange(enterpriseUser, collectorUser);
        context.Enterprises.Add(enterprise);
        context.Collectors.Add(collector);
        await context.SaveChangesAsync();

        return new CollectorScenario(enterpriseUser, collectorUser, enterprise, collector);
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
        Enterprise Enterprise,
        Collector Collector);
}