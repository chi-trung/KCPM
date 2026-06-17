using System.Security.Claims;
using System.Text.Json;
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

[AllureEpic("Enterprise Operations")]
[AllureFeature("Enterprise Task Controller - Profile, WasteTypes, Stats, Progress, Complaints")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Enterprise profile management and task monitoring endpoints")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "EnterpriseTaskControllerExtendedTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("Team")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("api")]
[Allure.Net.Commons.Attributes.AllureTag("enterprise")]
[Allure.Net.Commons.Attributes.AllureTag("extended")]
public class EnterpriseTaskControllerExtendedTests
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

    private static EnterpriseTaskController CreateController(
        WastePlatformDbContext context,
        Guid? userId = null,
        bool isAdmin = false,
        IMediator? mediator = null)
    {
        var hubContext = new Mock<IHubContext<TaskHub>>();
        var hubClients = new Mock<IHubClients>();
        var clientProxy = new Mock<IClientProxy>();
        hubClients.SetupGet(x => x.All).Returns(clientProxy.Object);
        hubContext.SetupGet(x => x.Clients).Returns(hubClients.Object);

        var controller = new EnterpriseTaskController(
            context,
            hubContext.Object,
            new Mock<INotificationService>().Object,
            mediator ?? new Mock<IMediator>().Object);

        var claims = new List<Claim>();
        if (userId.HasValue)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        if (isAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        else
            claims.Add(new Claim(ClaimTypes.Role, "Enterprise"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
            }
        };
        return controller;
    }

    private static async Task<(User EnterpriseUser, Enterprise Enterprise)> SeedEnterpriseAsync(
        WastePlatformDbContext ctx, string status = "Verified")
    {
        var user = User.Create($"ent-{Guid.NewGuid():N}@test.com", "hash", "Test Enterprise", UserRole.Enterprise);
        var enterprise = new Enterprise
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CompanyName = "Test Corp",
            Status = status,
            User = user
        };
        ctx.Users.Add(user);
        ctx.Enterprises.Add(enterprise);
        await ctx.SaveChangesAsync();
        return (user, enterprise);
    }

    private static T? GetProp<T>(object obj, string prop)
        => (T?)obj.GetType().GetProperty(prop)?.GetValue(obj);

    #endregion

    #region GetProfile

    [Fact]
    [AllureDescription("GetProfile returns Ok with enterprise data and waste types.")]
    public async Task GetProfile_WithValidEnterprise_ShouldReturnOk()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (user, enterprise) = await SeedEnterpriseAsync(ctx);

        var controller = CreateController(ctx, user.Id);

        // Act
        var result = await controller.GetProfile();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        AllureAttachmentHelper.AttachText("test-input", $"EnterpriseId: {enterprise.Id}");
        AllureAttachmentHelper.AttachText("http-response", "Status: 200 OK — enterprise profile returned");
        json.Should().Contain(enterprise.Id.ToString());
    }

    [Fact]
    [AllureDescription("GetProfile returns BadRequest when user is Admin.")]
    public async Task GetProfile_WhenAdmin_ShouldReturnBadRequest()
    {
        // Arrange
        await using var ctx = CreateContext();
        var controller = CreateController(ctx, Guid.NewGuid(), isAdmin: true);

        // Act
        var result = await controller.GetProfile();

        // Assert
        AllureAttachmentHelper.AttachText("error-details", "Role=Admin → 400 BadRequest (Admin cannot access enterprise profile)");
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("GetProfile returns Unauthorized when enterprise not found.")]
    public async Task GetProfile_WithNoEnterprise_ShouldReturnUnauthorized()
    {
        // Arrange
        await using var ctx = CreateContext();
        var controller = CreateController(ctx, Guid.NewGuid());

        // Act
        var result = await controller.GetProfile();

        // Assert
        AllureAttachmentHelper.AttachText("error-details", "Random userId with no enterprise record → 401 Unauthorized");
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion

    #region UpdateProfile

    [Fact]
    [AllureDescription("UpdateProfile returns Ok when request is valid.")]
    public async Task UpdateProfile_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (user, _) = await SeedEnterpriseAsync(ctx);

        var controller = CreateController(ctx, user.Id);
        var request = new UpdateEnterpriseProfileRequest
        {
            ServiceArea = "Quận 1, Quận 3",
            CapacityKgPerDay = 500
        };

        // Act
        var result = await controller.UpdateProfile(request);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        AllureAttachmentHelper.AttachJson("update-request", request);
        AllureAttachmentHelper.AttachText("http-response", "Status: 200 OK — 'updated successfully'");
        json.Should().Contain("updated successfully");
    }

    [Fact]
    [AllureDescription("UpdateProfile returns BadRequest when user is Admin.")]
    public async Task UpdateProfile_WhenAdmin_ShouldReturnBadRequest()
    {
        // Arrange
        await using var ctx = CreateContext();
        var controller = CreateController(ctx, Guid.NewGuid(), isAdmin: true);

        // Act
        var result = await controller.UpdateProfile(new UpdateEnterpriseProfileRequest());

        // Assert
        AllureAttachmentHelper.AttachText("error-details", "Role=Admin → 400 BadRequest (Admin cannot update enterprise profile)");
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("UpdateProfile transitions status from Rejected to Pending.")]
    public async Task UpdateProfile_WhenRejected_ShouldTransitionToPending()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (user, enterprise) = await SeedEnterpriseAsync(ctx, status: "Rejected");

        var controller = CreateController(ctx, user.Id);

        // Act
        var result = await controller.UpdateProfile(new UpdateEnterpriseProfileRequest
        {
            ServiceArea = "Quận 5",
            CapacityKgPerDay = 200
        });

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        AllureAttachmentHelper.AttachText("status-transition", "Enterprise status: Rejected → Pending after profile update");
        json.Should().Contain("Pending");
    }

    [Fact]
    [AllureDescription("UpdateProfile returns Unauthorized when enterprise not found.")]
    public async Task UpdateProfile_WithNoEnterprise_ShouldReturnUnauthorized()
    {
        // Arrange
        await using var ctx = CreateContext();
        var controller = CreateController(ctx, Guid.NewGuid());

        // Act
        var result = await controller.UpdateProfile(new UpdateEnterpriseProfileRequest());

        // Assert
        AllureAttachmentHelper.AttachText("error-details", "Random userId with no enterprise → 401 Unauthorized");
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion

    #region GetWasteTypes

    [Fact]
    [AllureDescription("GetWasteTypes returns Ok with all categories and accepted IDs.")]
    public async Task GetWasteTypes_WithValidEnterprise_ShouldReturnOk()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (user, _) = await SeedEnterpriseAsync(ctx);
        ctx.WasteCategories.Add(new WasteCategory { Id = 1, Name = "Nhựa" });
        await ctx.SaveChangesAsync();

        var controller = CreateController(ctx, user.Id);

        // Act
        var result = await controller.GetWasteTypes();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        AllureAttachmentHelper.AttachText("http-response", "Status: 200 OK — response contains 'allCategories'");
        json.Should().Contain("allCategories");
    }

    [Fact]
    [AllureDescription("GetWasteTypes returns BadRequest when user is Admin.")]
    public async Task GetWasteTypes_WhenAdmin_ShouldReturnBadRequest()
    {
        // Arrange
        await using var ctx = CreateContext();
        var controller = CreateController(ctx, Guid.NewGuid(), isAdmin: true);

        // Act
        var result = await controller.GetWasteTypes();

        // Assert
        AllureAttachmentHelper.AttachText("error-details", "Role=Admin → 400 BadRequest");
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("GetWasteTypes returns Unauthorized when enterprise not found.")]
    public async Task GetWasteTypes_WithNoEnterprise_ShouldReturnUnauthorized()
    {
        // Arrange
        await using var ctx = CreateContext();
        var controller = CreateController(ctx, Guid.NewGuid());

        // Act
        var result = await controller.GetWasteTypes();

        // Assert
        AllureAttachmentHelper.AttachText("error-details", "Random userId with no enterprise → 401 Unauthorized");
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion

    #region UpdateWasteTypes

    [Fact]
    [AllureDescription("UpdateWasteTypes returns Ok when all category IDs are valid.")]
    public async Task UpdateWasteTypes_WithValidIds_ShouldReturnOk()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (user, _) = await SeedEnterpriseAsync(ctx);
        ctx.WasteCategories.Add(new WasteCategory { Id = 1, Name = "Nhựa" });
        ctx.WasteCategories.Add(new WasteCategory { Id = 2, Name = "Kim loại" });
        await ctx.SaveChangesAsync();

        var controller = CreateController(ctx, user.Id);

        // Act
        var result = await controller.UpdateWasteTypes(new UpdateEnterpriseWasteTypesRequest
        {
            WasteCategoryIds = new List<int> { 1, 2 }
        });

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        AllureAttachmentHelper.AttachText("test-input", "WasteCategoryIds: [1, 2] (both exist in DB)");
        AllureAttachmentHelper.AttachText("http-response", "Status: 200 OK — 'updated successfully'");
        json.Should().Contain("updated successfully");
    }

    [Fact]
    [AllureDescription("UpdateWasteTypes returns BadRequest when user is Admin.")]
    public async Task UpdateWasteTypes_WhenAdmin_ShouldReturnBadRequest()
    {
        // Arrange
        await using var ctx = CreateContext();
        var controller = CreateController(ctx, Guid.NewGuid(), isAdmin: true);

        // Act
        var result = await controller.UpdateWasteTypes(new UpdateEnterpriseWasteTypesRequest());

        // Assert
        AllureAttachmentHelper.AttachText("error-details", "Role=Admin → 400 BadRequest");
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("UpdateWasteTypes returns Unauthorized when enterprise not found.")]
    public async Task UpdateWasteTypes_WithNoEnterprise_ShouldReturnUnauthorized()
    {
        // Arrange
        await using var ctx = CreateContext();
        var controller = CreateController(ctx, Guid.NewGuid());

        // Act
        var result = await controller.UpdateWasteTypes(new UpdateEnterpriseWasteTypesRequest());

        // Assert
        AllureAttachmentHelper.AttachText("error-details", "Random userId with no enterprise → 401 Unauthorized");
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    [AllureDescription("UpdateWasteTypes returns BadRequest when an invalid category ID is provided.")]
    public async Task UpdateWasteTypes_WithInvalidCategoryId_ShouldReturnBadRequest()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (user, _) = await SeedEnterpriseAsync(ctx);
        ctx.WasteCategories.Add(new WasteCategory { Id = 1, Name = "Nhựa" });
        await ctx.SaveChangesAsync();

        var controller = CreateController(ctx, user.Id);

        // Act
        var result = await controller.UpdateWasteTypes(new UpdateEnterpriseWasteTypesRequest
        {
            WasteCategoryIds = new List<int> { 1, 999 } // 999 doesn't exist
        });

        // Assert
        AllureAttachmentHelper.AttachText("invalid-input", "WasteCategoryIds: [1, 999] — categoryId 999 not found → 400 BadRequest");
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetStats

    [Fact]
    [AllureDescription("GetStats returns Ok with task counts for enterprise.")]
    public async Task GetStats_WithValidEnterprise_ShouldReturnOkWithStats()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (user, _) = await SeedEnterpriseAsync(ctx);
        var controller = CreateController(ctx, user.Id);

        // Act
        var result = await controller.GetStats();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        AllureAttachmentHelper.AttachText("http-response", "Status: 200 OK — stats contain TotalTasks");
        json.Should().Contain("TotalTasks");
    }

    [Fact]
    [AllureDescription("GetStats returns Unauthorized when enterprise not found.")]
    public async Task GetStats_WithNoEnterprise_ShouldReturnUnauthorized()
    {
        // Arrange
        await using var ctx = CreateContext();
        var controller = CreateController(ctx, Guid.NewGuid());

        // Act
        var result = await controller.GetStats();

        // Assert
        AllureAttachmentHelper.AttachText("error-details", "Random userId with no enterprise → 401 Unauthorized");
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion

    #region GetTaskProgress

    [Fact]
    [AllureDescription("GetTaskProgress returns Ok with task timeline for enterprise task.")]
    public async Task GetTaskProgress_WithValidTask_ShouldReturnOkWithTimeline()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (user, enterprise) = await SeedEnterpriseAsync(ctx);

        var citizenUser = User.Create("cit@test.com", "hash", "Citizen", UserRole.Citizen);
        var report = WasteReport.Create(citizenUser.Id, 1, 10m, 106m, "Report");
        var task = CollectionTask.Create(report.Id, enterprise.Id);

        ctx.Users.Add(citizenUser);
        ctx.WasteReports.Add(report);
        ctx.CollectionTasks.Add(task);
        await ctx.SaveChangesAsync();

        var controller = CreateController(ctx, user.Id);

        // Act
        var result = await controller.GetTaskProgress(task.Id);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        AllureAttachmentHelper.AttachText("test-input", $"TaskId: {task.Id}, EnterpriseId: {enterprise.Id}");
        AllureAttachmentHelper.AttachText("http-response", "Status: 200 OK — response contains 'Timeline'");
        json.Should().Contain("Timeline");
    }

    [Fact]
    [AllureDescription("GetTaskProgress returns NotFound when task doesn't belong to enterprise.")]
    public async Task GetTaskProgress_WithUnknownTaskId_ShouldReturnNotFound()
    {
        // Arrange
        await using var ctx = CreateContext();
        var (user, _) = await SeedEnterpriseAsync(ctx);
        var controller = CreateController(ctx, user.Id);

        // Act
        var result = await controller.GetTaskProgress(Guid.NewGuid());

        // Assert
        AllureAttachmentHelper.AttachText("error-details", "Random taskId not found in enterprise → 404 NotFound");
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    [AllureDescription("GetTaskProgress returns Unauthorized when enterprise not found.")]
    public async Task GetTaskProgress_WithNoEnterprise_ShouldReturnUnauthorized()
    {
        // Arrange
        await using var ctx = CreateContext();
        var controller = CreateController(ctx, Guid.NewGuid());

        // Act
        var result = await controller.GetTaskProgress(Guid.NewGuid());

        // Assert
        AllureAttachmentHelper.AttachText("error-details", "Random userId with no enterprise → 401 Unauthorized");
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion
}
