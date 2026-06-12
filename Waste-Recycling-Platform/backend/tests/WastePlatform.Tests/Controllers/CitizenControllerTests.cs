using System.Security.Claims;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WastePlatform.API.Controllers;
using WastePlatform.Application.Rewards.Queries;
using WastePlatform.Application.Citizens.Profile.Commands;
using WastePlatform.Application.Citizens.Profile.Queries;
using WastePlatform.Application.Citizens.Profile.DTOs;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Controllers;

[AllureEpic("Citizen APIs")]
[AllureFeature("Citizen Controller")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Citizen rewards, leaderboard, and profile management")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "CitizenControllerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("Chi Trung")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("api")]
[Allure.Net.Commons.Attributes.AllureTag("citizen")]
public class CitizenControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();

    [Fact]
    [AllureDescription("GetTotalRewards returns OK with total points for authenticated citizen.")]
    public async Task GetTotalRewards_WhenAuthenticated_ShouldReturnOkWithPoints()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetTotalPointsQuery>(q => q.CitizenId == userId), default))
            .ReturnsAsync(150);

        var controller = CreateController(userId);

        // Act
        var result = await controller.GetTotalRewards();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        AllureAttachmentHelper.AttachJson("rewards-response", okResult.Value!);

        GetProperty<string>(okResult.Value!, "message").Should().Contain("retrieved successfully");
    }

    [Fact]
    [AllureDescription("GetTotalRewards returns Unauthorized when user ID is missing from token.")]
    public async Task GetTotalRewards_WhenNoUserId_ShouldReturnUnauthorized()
    {
        var controller = CreateControllerWithoutAuth();

        var result = await controller.GetTotalRewards();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    [AllureDescription("GetRewardHistory returns OK with paginated reward history.")]
    public async Task GetRewardHistory_WithValidParams_ShouldReturnOk()
    {
        var userId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetRewardHistoryQuery>(), default))
            .ReturnsAsync(new object());

        var controller = CreateController(userId);

        var result = await controller.GetRewardHistory(page: 1, pageSize: 10);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        AllureAttachmentHelper.AttachJson("history-response", okResult.Value!);
    }

    [Fact]
    [AllureDescription("GetRewardHistory returns BadRequest when page < 1.")]
    public async Task GetRewardHistory_WithInvalidPage_ShouldReturnBadRequest()
    {
        var controller = CreateController(Guid.NewGuid());

        var result = await controller.GetRewardHistory(page: 0, pageSize: 10);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("GetRewardHistory returns BadRequest when pageSize < 1.")]
    public async Task GetRewardHistory_WithInvalidPageSize_ShouldReturnBadRequest()
    {
        var controller = CreateController(Guid.NewGuid());

        var result = await controller.GetRewardHistory(page: 1, pageSize: 0);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("GetLeaderboard returns OK with leaderboard data (public endpoint).")]
    public async Task GetLeaderboard_ShouldReturnOkWithLeaderboard()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetLeaderboardQuery>(), default))
            .ReturnsAsync(new LeaderboardResponseDto
            {
                Leaderboard = new List<LeaderboardItemDto>(),
                Page = 1,
                PageSize = 10,
                Total = 0
            });

        var controller = CreateController(Guid.NewGuid());

        var result = await controller.GetLeaderboard();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        AllureAttachmentHelper.AttachJson("leaderboard-response", okResult.Value!);
    }

    [Fact]
    [AllureDescription("GetLeaderboard returns BadRequest with invalid pagination.")]
    public async Task GetLeaderboard_WithInvalidPagination_ShouldReturnBadRequest()
    {
        var controller = CreateController(Guid.NewGuid());

        var result = await controller.GetLeaderboard(page: -1, pageSize: 10);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("GetAreaLeaderboard returns BadRequest with invalid pagination.")]
    public async Task GetAreaLeaderboard_WithInvalidPagination_ShouldReturnBadRequest()
    {
        var controller = CreateController(Guid.NewGuid());

        var result = await controller.GetAreaLeaderboard(page: 0, pageSize: 0);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("GetProfile returns OK with user profile.")]
    public async Task GetProfile_WhenAuthenticated_ShouldReturnOk()
    {
        var userId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetProfileQuery>(q => q.UserId == userId), default))
            .ReturnsAsync(new object());

        var controller = CreateController(userId);

        var result = await controller.GetProfile();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    [AllureDescription("GetProfile returns Unauthorized when user ID is missing.")]
    public async Task GetProfile_WhenNoAuth_ShouldReturnUnauthorized()
    {
        var controller = CreateControllerWithoutAuth();

        var result = await controller.GetProfile();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    [AllureDescription("GetProfile returns NotFound when user doesn't exist.")]
    public async Task GetProfile_WhenUserNotFound_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProfileQuery>(), default))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        var controller = CreateController(userId);

        var result = await controller.GetProfile();

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    [AllureDescription("UpdateProfile returns OK when updating with valid data.")]
    public async Task UpdateProfile_WithValidData_ShouldReturnOk()
    {
        var userId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateProfileCommand>(), default))
            .ReturnsAsync(new object());

        var controller = CreateController(userId);
        var profile = new UpdateProfileDto { FullName = "Updated Name" };

        var result = await controller.UpdateProfile(profile);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    [AllureDescription("UpdateProfile returns BadRequest when FullName is empty.")]
    public async Task UpdateProfile_WithEmptyFullName_ShouldReturnBadRequest()
    {
        var controller = CreateController(Guid.NewGuid());
        var profile = new UpdateProfileDto { FullName = "  " };

        var result = await controller.UpdateProfile(profile);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("UpdateProfile returns Unauthorized when user ID is missing.")]
    public async Task UpdateProfile_WhenNoAuth_ShouldReturnUnauthorized()
    {
        var controller = CreateControllerWithoutAuth();
        var profile = new UpdateProfileDto { FullName = "Test" };

        var result = await controller.UpdateProfile(profile);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    private CitizenController CreateController(Guid userId)
    {
        var controller = new CitizenController(_mediatorMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Email, "citizen@test.com"),
                    new Claim(ClaimTypes.Role, "Citizen")
                ], "TestAuth"))
            }
        };
        return controller;
    }

    private CitizenController CreateControllerWithoutAuth()
    {
        var controller = new CitizenController(_mediatorMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    private static T? GetProperty<T>(object obj, string name)
    {
        var prop = obj.GetType().GetProperty(name);
        return prop is null ? default : (T?)prop.GetValue(obj);
    }
}
