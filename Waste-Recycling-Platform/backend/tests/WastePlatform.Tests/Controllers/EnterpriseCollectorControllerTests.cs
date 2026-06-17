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

[AllureEpic("Enterprise Operations")]
[AllureFeature("Enterprise Collector Management")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Enterprise manages collector accounts (create, update, delete, list)")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "EnterpriseCollectorControllerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("Team")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("api")]
[Allure.Net.Commons.Attributes.AllureTag("enterprise")]
[Allure.Net.Commons.Attributes.AllureTag("collectors")]
public class EnterpriseCollectorControllerTests
{
    #region Helpers

    private static WastePlatformDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<WastePlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
        return new WastePlatformDbContext(options);
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
                ], "TestAuth"))
            }
        };
    }

    private static ControllerContext BuildAnonymousControllerContext()
    {
        return new ControllerContext { HttpContext = new DefaultHttpContext() };
    }

    private static T? GetProp<T>(object obj, string prop)
        => (T?)obj.GetType().GetProperty(prop)?.GetValue(obj);

    private static async Task<(User EnterpriseUser, Enterprise Enterprise)> SeedEnterpriseAsync(WastePlatformDbContext ctx)
    {
        var user = User.Create($"enterprise-{Guid.NewGuid():N}@test.com", "hash", "Test Enterprise", UserRole.Enterprise);
        var enterprise = new Enterprise
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CompanyName = "Test Corp",
            User = user
        };
        ctx.Users.Add(user);
        ctx.Enterprises.Add(enterprise);
        await ctx.SaveChangesAsync();
        return (user, enterprise);
    }

    private static async Task<(User CollectorUser, Collector Collector)> SeedCollectorAsync(
        WastePlatformDbContext ctx, Guid enterpriseId)
    {
        var suffix = Guid.NewGuid().ToString("N")[..6];
        var user = User.Create($"collector-{suffix}@test.com", "hash", $"Test Collector {suffix}", UserRole.Collector,
            $"09000{suffix[..5]}");
        var collector = new Collector
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            EnterpriseId = enterpriseId,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow,
            User = user
        };
        ctx.Users.Add(user);
        ctx.Collectors.Add(collector);
        await ctx.SaveChangesAsync();
        return (user, collector);
    }

    #endregion

    #region GetCollectors

    [Fact]
    [AllureDescription("GetCollectors returns Ok with list of collectors for a valid enterprise.")]
    public async Task GetCollectors_WithValidEnterprise_ShouldReturnOkWithCollectors()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (enterpriseUser, enterprise) = await SeedEnterpriseAsync(ctx);
        var (_, collector) = await SeedCollectorAsync(ctx, enterprise.Id);

        var controller = new EnterpriseCollectorController(ctx)
        {
            ControllerContext = BuildControllerContext(enterpriseUser.Id)
        };

        // Act
        var result = await controller.GetCollectors();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        json.Should().Contain(collector.Id.ToString());
    }

    [Fact]
    [AllureDescription("GetCollectors returns Unauthorized when enterprise not found for user.")]
    public async Task GetCollectors_WithNoEnterprise_ShouldReturnUnauthorized()
    {
        // Arrange
        await using var ctx = CreateContext();
        var controller = new EnterpriseCollectorController(ctx)
        {
            ControllerContext = BuildControllerContext(Guid.NewGuid())
        };

        // Act
        var result = await controller.GetCollectors();

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion

    #region CreateCollector

    [Fact]
    [AllureDescription("CreateCollector returns Ok when all required fields are provided.")]
    public async Task CreateCollector_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (enterpriseUser, _) = await SeedEnterpriseAsync(ctx);

        var controller = new EnterpriseCollectorController(ctx)
        {
            ControllerContext = BuildControllerContext(enterpriseUser.Id)
        };

        var request = new CreateEnterpriseCollectorRequest
        {
            FullName = "New Collector",
            Email = $"newcol-{Guid.NewGuid():N}@test.com",
            Phone = "0912345678",
            TemporaryPassword = "Password123",
            IsAvailable = true
        };

        // Act
        var result = await controller.CreateCollector(request);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        json.Should().Contain("Collector account created successfully");
    }

    [Fact]
    [AllureDescription("CreateCollector returns Unauthorized when enterprise is not found.")]
    public async Task CreateCollector_WithNoEnterprise_ShouldReturnUnauthorized()
    {
        // Arrange
        await using var ctx = CreateContext();
        var controller = new EnterpriseCollectorController(ctx)
        {
            ControllerContext = BuildControllerContext(Guid.NewGuid())
        };

        var result = await controller.CreateCollector(new CreateEnterpriseCollectorRequest
        {
            FullName = "Test", Email = "x@x.com", TemporaryPassword = "pass123", IsAvailable = true
        });

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    [AllureDescription("CreateCollector returns BadRequest when required fields are missing.")]
    public async Task CreateCollector_WithMissingRequiredFields_ShouldReturnBadRequest()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (enterpriseUser, _) = await SeedEnterpriseAsync(ctx);
        var controller = new EnterpriseCollectorController(ctx)
        {
            ControllerContext = BuildControllerContext(enterpriseUser.Id)
        };

        var result = await controller.CreateCollector(new CreateEnterpriseCollectorRequest
        {
            FullName = "",
            Email = "valid@test.com",
            TemporaryPassword = "password123",
            IsAvailable = false
        });

        // Assert
        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        GetProp<string>(bad.Value!, "message").Should().Contain("required");
    }

    [Fact]
    [AllureDescription("CreateCollector returns BadRequest when password is too short.")]
    public async Task CreateCollector_WithShortPassword_ShouldReturnBadRequest()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (enterpriseUser, _) = await SeedEnterpriseAsync(ctx);
        var controller = new EnterpriseCollectorController(ctx)
        {
            ControllerContext = BuildControllerContext(enterpriseUser.Id)
        };

        var result = await controller.CreateCollector(new CreateEnterpriseCollectorRequest
        {
            FullName = "Test", Email = "valid2@test.com", TemporaryPassword = "abc", IsAvailable = true
        });

        // Assert
        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        GetProp<string>(bad.Value!, "message").Should().Contain("6 characters");
    }

    [Fact]
    [AllureDescription("CreateCollector returns Conflict when email is already in use.")]
    public async Task CreateCollector_WithDuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (enterpriseUser, enterprise) = await SeedEnterpriseAsync(ctx);
        var (collectorUser, _) = await SeedCollectorAsync(ctx, enterprise.Id);

        var controller = new EnterpriseCollectorController(ctx)
        {
            ControllerContext = BuildControllerContext(enterpriseUser.Id)
        };

        var result = await controller.CreateCollector(new CreateEnterpriseCollectorRequest
        {
            FullName = "Another",
            Email = collectorUser.Email!,  // Duplicate email
            TemporaryPassword = "Password123",
            IsAvailable = true
        });

        // Assert
        var conflict = result.Should().BeOfType<ConflictObjectResult>().Subject;
        GetProp<string>(conflict.Value!, "message").Should().Contain("Email");
    }

    #endregion

    #region UpdateCollector

    [Fact]
    [AllureDescription("UpdateCollector returns Ok when update is valid.")]
    public async Task UpdateCollector_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (enterpriseUser, enterprise) = await SeedEnterpriseAsync(ctx);
        var (_, collector) = await SeedCollectorAsync(ctx, enterprise.Id);

        var controller = new EnterpriseCollectorController(ctx)
        {
            ControllerContext = BuildControllerContext(enterpriseUser.Id)
        };

        var result = await controller.UpdateCollector(collector.Id, new UpdateEnterpriseCollectorRequest
        {
            FullName = "Updated Name",
            Email = $"updated-{Guid.NewGuid():N}@test.com",
            IsAvailable = false
        });

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        json.Should().Contain("Updated Name");
    }

    [Fact]
    [AllureDescription("UpdateCollector returns Unauthorized when enterprise is not found.")]
    public async Task UpdateCollector_WithNoEnterprise_ShouldReturnUnauthorized()
    {
        // Arrange
        await using var ctx = CreateContext();
        var controller = new EnterpriseCollectorController(ctx)
        {
            ControllerContext = BuildControllerContext(Guid.NewGuid())
        };

        var result = await controller.UpdateCollector(Guid.NewGuid(), new UpdateEnterpriseCollectorRequest
        {
            FullName = "Test", Email = "x@x.com", IsAvailable = true
        });

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    [AllureDescription("UpdateCollector returns NotFound when collector doesn't belong to enterprise.")]
    public async Task UpdateCollector_WithWrongCollectorId_ShouldReturnNotFound()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (enterpriseUser, _) = await SeedEnterpriseAsync(ctx);

        var controller = new EnterpriseCollectorController(ctx)
        {
            ControllerContext = BuildControllerContext(enterpriseUser.Id)
        };

        var result = await controller.UpdateCollector(Guid.NewGuid(), new UpdateEnterpriseCollectorRequest
        {
            FullName = "Test", Email = "x@x.com", IsAvailable = true
        });

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    [AllureDescription("UpdateCollector with new password shorter than 6 chars returns BadRequest.")]
    public async Task UpdateCollector_WithShortPassword_ShouldReturnBadRequest()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (enterpriseUser, enterprise) = await SeedEnterpriseAsync(ctx);
        var (_, collector) = await SeedCollectorAsync(ctx, enterprise.Id);

        var controller = new EnterpriseCollectorController(ctx)
        {
            ControllerContext = BuildControllerContext(enterpriseUser.Id)
        };

        var result = await controller.UpdateCollector(collector.Id, new UpdateEnterpriseCollectorRequest
        {
            FullName = "Valid Name",
            Email = $"valid-{Guid.NewGuid():N}@test.com",
            TemporaryPassword = "abc",  // too short
            IsAvailable = true
        });

        // Assert
        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        GetProp<string>(bad.Value!, "message").Should().Contain("6 characters");
    }

    #endregion

    #region DeleteCollector

    [Fact]
    [AllureDescription("DeleteCollector returns Ok when collector has no active tasks.")]
    public async Task DeleteCollector_WithNoActiveTasks_ShouldReturnOk()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (enterpriseUser, enterprise) = await SeedEnterpriseAsync(ctx);
        var (_, collector) = await SeedCollectorAsync(ctx, enterprise.Id);

        var controller = new EnterpriseCollectorController(ctx)
        {
            ControllerContext = BuildControllerContext(enterpriseUser.Id)
        };

        // Act
        var result = await controller.DeleteCollector(collector.Id);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        GetProp<string>(ok.Value!, "message").Should().Contain("deleted successfully");
    }

    [Fact]
    [AllureDescription("DeleteCollector returns Unauthorized when enterprise is not found.")]
    public async Task DeleteCollector_WithNoEnterprise_ShouldReturnUnauthorized()
    {
        // Arrange
        await using var ctx = CreateContext();
        var controller = new EnterpriseCollectorController(ctx)
        {
            ControllerContext = BuildControllerContext(Guid.NewGuid())
        };

        var result = await controller.DeleteCollector(Guid.NewGuid());

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    [AllureDescription("DeleteCollector returns NotFound when collector doesn't exist.")]
    public async Task DeleteCollector_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (enterpriseUser, _) = await SeedEnterpriseAsync(ctx);

        var controller = new EnterpriseCollectorController(ctx)
        {
            ControllerContext = BuildControllerContext(enterpriseUser.Id)
        };

        var result = await controller.DeleteCollector(Guid.NewGuid());

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    [AllureDescription("DeleteCollector returns BadRequest when collector has active tasks.")]
    public async Task DeleteCollector_WithActiveTasks_ShouldReturnBadRequest()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (enterpriseUser, enterprise) = await SeedEnterpriseAsync(ctx);
        var (_, collector) = await SeedCollectorAsync(ctx, enterprise.Id);

        // Seed a report + active task assigned to this collector
        var citizenUser = User.Create("citizen@test.com", "hash", "Citizen", UserRole.Citizen);
        var report = WasteReport.Create(citizenUser.Id, 1, 10m, 106m, "Waste report");
        var task = CollectionTask.Create(report.Id, enterprise.Id);
        task.AssignCollector(collector.Id);  // Assign so task is active (Assigned status)

        ctx.Users.Add(citizenUser);
        ctx.WasteReports.Add(report);
        ctx.CollectionTasks.Add(task);
        await ctx.SaveChangesAsync();

        var controller = new EnterpriseCollectorController(ctx)
        {
            ControllerContext = BuildControllerContext(enterpriseUser.Id)
        };

        // Act
        var result = await controller.DeleteCollector(collector.Id);

        // Assert
        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        GetProp<string>(bad.Value!, "message").Should().Contain("active tasks");
    }

    #endregion
}
