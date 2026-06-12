using System.Security.Claims;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WastePlatform.API.Controllers;
using WastePlatform.Application.Admin.Users.Queries;
using WastePlatform.Application.Admin.Users.Commands;
using WastePlatform.Application.Admin.Dashboard.Queries;
using WastePlatform.Application.Common.DTOs;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Controllers;

[AllureEpic("Admin APIs")]
[AllureFeature("Admin Users Controller")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "User management: list, create, toggle status, update role")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "AdminUsersControllerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("Chi Trung")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("api")]
[Allure.Net.Commons.Attributes.AllureTag("admin")]
public class AdminUsersControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();

    [Fact]
    [AllureDescription("GetUsers returns OK with user list.")]
    public async Task GetUsers_ShouldReturnOkWithUserList()
    {
        var users = new List<UserDto>
        {
            new() { Id = Guid.NewGuid().ToString(), FullName = "User 1", Email = "u1@test.com", Role = "citizen" },
            new() { Id = Guid.NewGuid().ToString(), FullName = "User 2", Email = "u2@test.com", Role = "admin" }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUsersQuery>(), default))
            .ReturnsAsync(users);

        var controller = new AdminUsersController(_mediatorMock.Object);

        var result = await controller.GetUsers(search: null, role: null);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        AllureAttachmentHelper.AttachJson("users-response", okResult.Value!);
    }

    [Fact]
    [AllureDescription("GetUsers passes search and role filters to query.")]
    public async Task GetUsers_WithFilters_ShouldPassFiltersToQuery()
    {
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetUsersQuery>(q => q.Search == "test" && q.Role == "citizen"), default))
            .ReturnsAsync(new List<UserDto>());

        var controller = new AdminUsersController(_mediatorMock.Object);

        await controller.GetUsers(search: "test", role: "citizen");

        _mediatorMock.Verify(m => m.Send(It.Is<GetUsersQuery>(q =>
            q.Search == "test" && q.Role == "citizen"), default), Times.Once);
    }

    [Fact]
    [AllureDescription("GetStats returns OK with dashboard statistics.")]
    public async Task GetStats_ShouldReturnOkWithStats()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetDashboardStatsQuery>(), default))
            .ReturnsAsync(new { TotalUsers = 100, TotalReports = 50 });

        var controller = new AdminUsersController(_mediatorMock.Object);

        var result = await controller.GetStats();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    [AllureDescription("CreateUser returns OK with new user ID.")]
    public async Task CreateUser_ShouldReturnOkWithNewId()
    {
        var newId = Guid.NewGuid().ToString();
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateUserCommand>(), default))
            .ReturnsAsync(newId);

        var controller = new AdminUsersController(_mediatorMock.Object);
        var command = new CreateUserCommand();

        var result = await controller.CreateUser(command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        AllureAttachmentHelper.AttachJson("create-user-response", okResult.Value!);
    }

    [Fact]
    [AllureDescription("ToggleStatus returns OK when user exists.")]
    public async Task ToggleStatus_WhenUserExists_ShouldReturnOk()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ToggleUserStatusCommand>(), default))
            .ReturnsAsync(true);

        var controller = new AdminUsersController(_mediatorMock.Object);

        var result = await controller.ToggleStatus(Guid.NewGuid().ToString());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    [AllureDescription("ToggleStatus returns NotFound when user doesn't exist.")]
    public async Task ToggleStatus_WhenUserNotFound_ShouldReturnNotFound()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ToggleUserStatusCommand>(), default))
            .ReturnsAsync(false);

        var controller = new AdminUsersController(_mediatorMock.Object);

        var result = await controller.ToggleStatus("nonexistent-id");

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    [AllureDescription("UpdateRole returns OK when user exists.")]
    public async Task UpdateRole_WhenUserExists_ShouldReturnOk()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateUserRoleCommand>(), default))
            .ReturnsAsync(true);

        var controller = new AdminUsersController(_mediatorMock.Object);
        var command = new UpdateUserRoleCommand();

        var result = await controller.UpdateRole(Guid.NewGuid().ToString(), command);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    [AllureDescription("UpdateRole returns NotFound when user doesn't exist.")]
    public async Task UpdateRole_WhenUserNotFound_ShouldReturnNotFound()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateUserRoleCommand>(), default))
            .ReturnsAsync(false);

        var controller = new AdminUsersController(_mediatorMock.Object);
        var command = new UpdateUserRoleCommand();

        var result = await controller.UpdateRole("nonexistent-id", command);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    [AllureDescription("UpdateRole sets UserId from URL parameter into the command.")]
    public async Task UpdateRole_ShouldSetUserIdFromUrl()
    {
        var userId = "test-user-id";
        _mediatorMock
            .Setup(m => m.Send(It.Is<UpdateUserRoleCommand>(c => c.UserId == userId), default))
            .ReturnsAsync(true);

        var controller = new AdminUsersController(_mediatorMock.Object);
        var command = new UpdateUserRoleCommand();

        await controller.UpdateRole(userId, command);

        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateUserRoleCommand>(c => c.UserId == userId), default), Times.Once);
    }
}
