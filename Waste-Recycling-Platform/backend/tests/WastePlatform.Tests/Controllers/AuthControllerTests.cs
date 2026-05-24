using System.Security.Claims;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WastePlatform.API.Controllers;
using WastePlatform.Application.Auth.Commands;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;
using WastePlatform.Infrastructure.Services;
using WastePlatform.Application.Common.DTOs;

namespace WastePlatform.Tests.Controllers;

[AllureEpic("KIEM-4 Auth Module")]
[AllureFeature("Authentication")]
[AllureOwner("chi-trung")]
[AllureSeverity(SeverityLevel.critical)]
public class AuthControllerTests
{
    [Fact]
    public async Task Register_WithValidCitizen_ShouldReturnOkAndCreateUser()
    {
        // Arrange
        await using var context = CreateContext();
        var jwtServiceMock = new Mock<IJwtService>();
        jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns("fake-jwt-token");

        var authService = new AuthService(context, jwtServiceMock.Object);
        var controller = new AuthController(authService);

        var cmd = new RegisterCommand
        {
            Email = "newcitizen@example.com",
            Password = "StrongPassword123!",
            FullName = "New Citizen",
            Role = UserRole.Citizen
        };

        // Act
        var result = await controller.Register(cmd);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthResponseDto>().Subject;
        
        response.Token.Should().Be("fake-jwt-token");
        response.User.Email.Should().Be("newcitizen@example.com");
        response.User.Role.Should().Be("citizen");

        var userInDb = await context.Users.SingleOrDefaultAsync(u => u.Email == "newcitizen@example.com");
        userInDb.Should().NotBeNull();
        userInDb!.FullName.Should().Be("New Citizen");
        userInDb.Role.Should().Be(UserRole.Citizen);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        await using var context = CreateContext();
        var existingUser = User.Create("existing@example.com", "hash", "Existing", UserRole.Citizen);
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var authService = new AuthService(context, new Mock<IJwtService>().Object);
        var controller = new AuthController(authService);

        var cmd = new RegisterCommand
        {
            Email = "existing@example.com", // Duplicate
            Password = "Password123!",
            FullName = "Another Citizen",
            Role = UserRole.Citizen
        };

        // Act
        var result = await controller.Register(cmd);

        // Assert
        var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
        GetPropertyValue<string>(conflictResult.Value!, "message").Should().Contain("đã được sử dụng");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkAndToken()
    {
        // Arrange
        await using var context = CreateContext();
        
        var password = "MySecretPassword123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = User.Create("valid@example.com", passwordHash, "Valid User", UserRole.Enterprise);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var jwtServiceMock = new Mock<IJwtService>();
        jwtServiceMock.Setup(x => x.GenerateToken(It.Is<User>(u => u.Id == user.Id))).Returns("valid-jwt-token");

        var authService = new AuthService(context, jwtServiceMock.Object);
        var controller = new AuthController(authService);

        var cmd = new LoginCommand
        {
            Email = "valid@example.com",
            Password = password
        };

        // Act
        var result = await controller.Login(cmd);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthResponseDto>().Subject;
        
        response.Token.Should().Be("valid-jwt-token");
        response.User.Email.Should().Be("valid@example.com");
        
        // Ensure Enterprise Profile was created automatically
        var enterpriseProfile = await context.Enterprises.SingleOrDefaultAsync(e => e.UserId == user.Id);
        enterpriseProfile.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        await using var context = CreateContext();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword!");
        var user = User.Create("user@example.com", passwordHash, "User", UserRole.Citizen);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var authService = new AuthService(context, new Mock<IJwtService>().Object);
        var controller = new AuthController(authService);

        var cmd = new LoginCommand
        {
            Email = "user@example.com",
            Password = "WrongPassword!" // Incorrect
        };

        // Act
        var result = await controller.Login(cmd);

        // Assert
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        GetPropertyValue<string>(unauthorizedResult.Value!, "message").Should().Contain("không đúng");
    }

    [Fact]
    public void Me_WhenAuthenticated_ShouldReturnUserClaims()
    {
        // Arrange
        var controller = new AuthController(null!);
        var userId = Guid.NewGuid().ToString();
        
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Email, "me@example.com"),
                    new Claim(ClaimTypes.Role, "Admin"),
                    new Claim("fullName", "Admin User")
                ], "TestAuth"))
            }
        };

        // Act
        var result = controller.Me();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        GetPropertyValue<string>(okResult.Value!, "userId").Should().Be(userId);
        GetPropertyValue<string>(okResult.Value!, "email").Should().Be("me@example.com");
        GetPropertyValue<string>(okResult.Value!, "role").Should().Be("Admin");
        GetPropertyValue<string>(okResult.Value!, "fullName").Should().Be("Admin User");
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
}
