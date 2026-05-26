using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WastePlatform.API.Controllers;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;

namespace WastePlatform.Tests.Controllers;

public class EnterpriseRewardRuleControllerTests
{
    [Fact]
    public async Task GetRewardRules_WhenEnterpriseExists_ShouldReturnItsRules()
    {
        await using var context = CreateContext();
        var scenario = await SeedEnterpriseAsync(context);

        var controller = new EnterpriseRewardRuleController(context)
        {
            ControllerContext = BuildControllerContext(scenario.EnterpriseUser.Id)
        };

        var result = await controller.GetRewardRules();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);

        json.Should().Contain(scenario.Category.Name);
        json.Should().Contain("\"PointsPerReport\":12");
        json.Should().Contain("\"BonusQuality\":4");
    }

    [Fact]
    public async Task UpdateRewardRules_WithValidPayload_ShouldUpdateAndInsertRules()
    {
        await using var context = CreateContext();
        var scenario = await SeedEnterpriseAsync(context);

        var controller = new EnterpriseRewardRuleController(context)
        {
            ControllerContext = BuildControllerContext(scenario.EnterpriseUser.Id)
        };

        var result = await controller.UpdateRewardRules(new UpdateEnterpriseRewardRulesRequest
        {
            Rules =
            [
                new UpdateEnterpriseRewardRuleItem
                {
                    WasteCategoryId = scenario.Category.Id,
                    PointsPerReport = 18,
                    BonusQuality = 6,
                    IsActive = false
                },
                new UpdateEnterpriseRewardRuleItem
                {
                    WasteCategoryId = scenario.SecondCategory.Id,
                    PointsPerReport = 8,
                    BonusQuality = 1,
                    IsActive = true
                }
            ]
        });

        result.Should().BeOfType<OkObjectResult>();

        var rule1 = await context.RewardRules.SingleAsync(rule => rule.WasteCategoryId == scenario.Category.Id);
        rule1.PointsPerReport.Should().Be(18);
        rule1.BonusQuality.Should().Be(6);
        rule1.IsActive.Should().BeFalse();

        var rule2 = await context.RewardRules.SingleAsync(rule => rule.WasteCategoryId == scenario.SecondCategory.Id);
        rule2.PointsPerReport.Should().Be(8);
        rule2.BonusQuality.Should().Be(1);
        rule2.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateRewardRules_WithDuplicateCategories_ShouldReturnBadRequest()
    {
        await using var context = CreateContext();
        var scenario = await SeedEnterpriseAsync(context);

        var controller = new EnterpriseRewardRuleController(context)
        {
            ControllerContext = BuildControllerContext(scenario.EnterpriseUser.Id)
        };

        var result = await controller.UpdateRewardRules(new UpdateEnterpriseRewardRulesRequest
        {
            Rules =
            [
                new UpdateEnterpriseRewardRuleItem
                {
                    WasteCategoryId = scenario.Category.Id,
                    PointsPerReport = 10,
                    BonusQuality = 0,
                    IsActive = true
                },
                new UpdateEnterpriseRewardRuleItem
                {
                    WasteCategoryId = scenario.Category.Id,
                    PointsPerReport = 15,
                    BonusQuality = 1,
                    IsActive = true
                }
            ]
        });

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        GetPropertyValue<string>(badRequest.Value!, "message").Should().Be("Duplicate waste category IDs are not allowed.");
    }

    [Fact]
    public async Task UpdateRewardRules_WithEmptyRules_ShouldReturnBadRequest()
    {
        await using var context = CreateContext();
        var scenario = await SeedEnterpriseAsync(context);

        var controller = new EnterpriseRewardRuleController(context)
        {
            ControllerContext = BuildControllerContext(scenario.EnterpriseUser.Id)
        };

        var result = await controller.UpdateRewardRules(new UpdateEnterpriseRewardRulesRequest
        {
            Rules = []
        });

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        GetPropertyValue<string>(badRequest.Value!, "message").Should().Be("Rules cannot be empty.");
    }

    [Fact]
    public async Task UpdateRewardRules_WithNegativeValues_ShouldReturnBadRequest()
    {
        await using var context = CreateContext();
        var scenario = await SeedEnterpriseAsync(context);

        var controller = new EnterpriseRewardRuleController(context)
        {
            ControllerContext = BuildControllerContext(scenario.EnterpriseUser.Id)
        };

        var result = await controller.UpdateRewardRules(new UpdateEnterpriseRewardRulesRequest
        {
            Rules =
            [
                new UpdateEnterpriseRewardRuleItem
                {
                    WasteCategoryId = scenario.Category.Id,
                    PointsPerReport = -1,
                    BonusQuality = 0,
                    IsActive = true
                }
            ]
        });

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        GetPropertyValue<string>(badRequest.Value!, "message").Should().Be("PointsPerReport and BonusQuality must be non-negative.");
    }

    private static async Task<EnterpriseScenario> SeedEnterpriseAsync(WastePlatformDbContext context)
    {
        var enterpriseUser = User.Create("enterprise@example.com", "hash", "Enterprise One", UserRole.Enterprise);
        var enterprise = new Enterprise
        {
            Id = Guid.NewGuid(),
            UserId = enterpriseUser.Id,
            CompanyName = "Enterprise One",
            User = enterpriseUser
        };

        var category = new WasteCategory
        {
            Id = 1,
            Name = "Plastic"
        };

        var secondCategory = new WasteCategory
        {
            Id = 2,
            Name = "Paper"
        };

        context.Users.Add(enterpriseUser);
        context.Enterprises.Add(enterprise);
        context.WasteCategories.AddRange(category, secondCategory);
        context.RewardRules.Add(new RewardRule
        {
            Id = Guid.NewGuid(),
            EnterpriseId = enterprise.Id,
            WasteCategoryId = category.Id,
            PointsPerReport = 12,
            BonusQuality = 4,
            IsActive = true
        });

        await context.SaveChangesAsync();

        return new EnterpriseScenario(enterpriseUser, enterprise, category, secondCategory);
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
        Enterprise Enterprise,
        WasteCategory Category,
        WasteCategory SecondCategory);
}