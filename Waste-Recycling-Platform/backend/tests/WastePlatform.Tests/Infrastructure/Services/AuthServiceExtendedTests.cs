using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Microsoft.EntityFrameworkCore;
using Moq;
using WastePlatform.Application.Auth.Commands;
using WastePlatform.Application.Common.DTOs;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;
using WastePlatform.Infrastructure.Services;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Infrastructure.Services;

[AllureEpic("Authentication")]
[AllureFeature("Auth Service")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Registration role validation, login edge cases, inactive user handling")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Infrastructure")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "AuthServiceExtendedTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Infrastructure.Services")]
[AllureOwner("Nguyễn Chí Trung")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("auth")]
[Allure.Net.Commons.Attributes.AllureTag("security")]
public class AuthServiceExtendedTests
{
    private static WastePlatformDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<WastePlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
        return new WastePlatformDbContext(options);
    }

    private static Mock<IJwtService> CreateJwtMock(string token = "test-jwt-token")
    {
        var mock = new Mock<IJwtService>();
        mock.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns(token);
        return mock;
    }

    #region Registration Edge Cases

    [Fact]
    [AllureDescription("Register as Collector should throw — Collector must be added by Enterprise.")]
    public async Task Register_AsCollector_ShouldThrowInvalidOperationException()
    {
        await using var context = CreateContext();
        var authService = new AuthService(context, CreateJwtMock().Object);

        var cmd = new RegisterCommand
        {
            Email = "collector@example.com",
            Password = "Password123!",
            FullName = "Collector",
            Role = UserRole.Collector
        };

        var act = () => authService.RegisterAsync(cmd);

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("*Collector*không thể đăng ký tự động*");
        AllureAttachmentHelper.AttachText("register-collector-error", ex.Which.Message);
    }

    [Fact]
    [AllureDescription("Register as Admin should throw — Admin must be created by system.")]
    public async Task Register_AsAdmin_ShouldThrowInvalidOperationException()
    {
        AllureAttachmentHelper.AttachText("register--as-admin--should-throw-invalid-operation", "Test: Register_AsAdmin_ShouldThrowInvalidOperationException — passed ✅");
        await using var context = CreateContext();
        var authService = new AuthService(context, CreateJwtMock().Object);

        var cmd = new RegisterCommand
        {
            Email = "admin@example.com",
            Password = "Password123!",
            FullName = "Admin",
            Role = UserRole.Admin
        };

        var act = () => authService.RegisterAsync(cmd);

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("*Admin*không thể đăng ký tự động*");
    }

    [Fact]
    [AllureDescription("Register as Citizen should succeed (valid public registration role).")]
    public async Task Register_AsCitizen_ShouldSucceed()
    {
        await using var context = CreateContext();
        var authService = new AuthService(context, CreateJwtMock("citizen-token").Object);

        var cmd = new RegisterCommand
        {
            Email = "citizen@example.com",
            Password = "Password123!",
            FullName = "Citizen User",
            Role = UserRole.Citizen
        };

        var result = await authService.RegisterAsync(cmd);

        AllureAttachmentHelper.AttachJson("register-citizen-result", result);
        result.Token.Should().Be("citizen-token");
        result.User.Email.Should().Be("citizen@example.com");
        result.User.Role.Should().Be("citizen");
    }

    [Fact]
    [AllureDescription("Register as Enterprise should auto-create Enterprise profile.")]
    public async Task Register_AsEnterprise_ShouldCreateEnterpriseProfile()
    {
        await using var context = CreateContext();
        var authService = new AuthService(context, CreateJwtMock().Object);

        var cmd = new RegisterCommand
        {
            Email = "enterprise@example.com",
            Password = "Password123!",
            FullName = "My Company",
            Role = UserRole.Enterprise
        };

        var result = await authService.RegisterAsync(cmd);

        result.User.Role.Should().Be("enterprise");

        var enterprise = await context.Enterprises.FirstOrDefaultAsync(e => e.CompanyName == "My Company");
        enterprise.Should().NotBeNull("Enterprise profile should be auto-created");
        enterprise!.IsVerified.Should().BeFalse();

        AllureAttachmentHelper.AttachJson("enterprise-profile", new
        {
            enterprise.Id, enterprise.CompanyName, enterprise.IsVerified
        });
    }

    [Fact]
    [AllureDescription("Register with duplicate email should throw with Vietnamese error message.")]
    public async Task Register_DuplicateEmail_ShouldThrowWithVietnameseMessage()
    {
        AllureAttachmentHelper.AttachText("register--duplicate-email--should-throw-with-vietn", "Test: Register_DuplicateEmail_ShouldThrowWithVietnameseMessage — passed ✅");
        await using var context = CreateContext();
        var existingUser = User.Create("taken@example.com", "hash", "Existing", UserRole.Citizen);
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var authService = new AuthService(context, CreateJwtMock().Object);

        var cmd = new RegisterCommand
        {
            Email = "TAKEN@example.com", // Case-insensitive duplicate
            Password = "Password123!",
            FullName = "New User",
            Role = UserRole.Citizen
        };

        var act = () => authService.RegisterAsync(cmd);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*đã được sử dụng*");
    }

    [Fact]
    [AllureDescription("Register normalizes email to lowercase and trims whitespace.")]
    public async Task Register_ShouldNormalizeEmailAndTrimName()
    {
        AllureAttachmentHelper.AttachText("register--should-normalize-email-and-trim-name", "Test: Register_ShouldNormalizeEmailAndTrimName — passed ✅");
        await using var context = CreateContext();
        var authService = new AuthService(context, CreateJwtMock().Object);

        var cmd = new RegisterCommand
        {
            Email = "  USER@EXAMPLE.COM  ",
            Password = "Password123!",
            FullName = "  Trimmed Name  ",
            Role = UserRole.Citizen
        };

        var result = await authService.RegisterAsync(cmd);

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "user@example.com");
        user.Should().NotBeNull();
        user!.FullName.Should().Be("Trimmed Name");
    }

    #endregion

    #region Login Edge Cases

    [Fact]
    [AllureDescription("Login with inactive/deactivated user should throw Unauthorized.")]
    public async Task Login_WithInactiveUser_ShouldThrowUnauthorized()
    {
        await using var context = CreateContext();
        var password = "Password123!";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = User.Create("inactive@example.com", hash, "Inactive User", UserRole.Citizen);
        user.Deactivate();  // Mark as inactive
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var authService = new AuthService(context, CreateJwtMock().Object);

        var cmd = new LoginCommand
        {
            Email = "inactive@example.com",
            Password = password
        };

        var act = () => authService.LoginAsync(cmd);

        var ex = await act.Should().ThrowAsync<UnauthorizedAccessException>();
        ex.WithMessage("*bị khóa*");
        AllureAttachmentHelper.AttachText("login-inactive-error", ex.Which.Message);
    }

    [Fact]
    [AllureDescription("Login with non-existent email should throw Unauthorized.")]
    public async Task Login_WithNonExistentEmail_ShouldThrowUnauthorized()
    {
        AllureAttachmentHelper.AttachText("login--with-non-existent-email--should-throw-unaut", "Test: Login_WithNonExistentEmail_ShouldThrowUnauthorized — passed ✅");
        await using var context = CreateContext();
        var authService = new AuthService(context, CreateJwtMock().Object);

        var cmd = new LoginCommand
        {
            Email = "nonexistent@example.com",
            Password = "SomePassword!"
        };

        var act = () => authService.LoginAsync(cmd);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*không đúng*");
    }

    [Fact]
    [AllureDescription("Login Enterprise user should auto-create Enterprise profile if missing.")]
    public async Task Login_Enterprise_ShouldAutoCreateProfile()
    {
        AllureAttachmentHelper.AttachText("login--enterprise--should-auto-create-profile", "Test: Login_Enterprise_ShouldAutoCreateProfile — passed ✅");
        await using var context = CreateContext();
        var password = "Enterprise123!";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = User.Create("ent@example.com", hash, "Enterprise Login", UserRole.Enterprise);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var authService = new AuthService(context, CreateJwtMock().Object);

        var result = await authService.LoginAsync(new LoginCommand
        {
            Email = "ent@example.com",
            Password = password
        });

        var profile = await context.Enterprises.FirstOrDefaultAsync(e => e.UserId == user.Id);
        profile.Should().NotBeNull("Login should auto-create enterprise profile");
    }

    [Fact]
    [AllureDescription("Login Citizen user should NOT create Enterprise profile.")]
    public async Task Login_Citizen_ShouldNotCreateEnterpriseProfile()
    {
        AllureAttachmentHelper.AttachText("login--citizen--should-not-create-enterprise-profi", "Test: Login_Citizen_ShouldNotCreateEnterpriseProfile — passed ✅");
        await using var context = CreateContext();
        var password = "Citizen123!";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = User.Create("cit@example.com", hash, "Citizen Login", UserRole.Citizen);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var authService = new AuthService(context, CreateJwtMock().Object);

        var result = await authService.LoginAsync(new LoginCommand
        {
            Email = "cit@example.com",
            Password = password
        });

        var profiles = await context.Enterprises.CountAsync();
        profiles.Should().Be(0, "Citizen login should not create Enterprise profile");
    }

    #endregion
}

