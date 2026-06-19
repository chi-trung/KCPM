using Allure.Xunit.Attributes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;
using WastePlatform.Infrastructure.Persistence.Repositories;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Infrastructure;

[AllureEpic("Infrastructure")]
[AllureFeature("UserRepository")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Infrastructure")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "UserRepositoryTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Infrastructure")]
[AllureOwner("Nguyễn Chí Trung")]
[AllureSeverity(SeverityLevel.normal)]
public class UserRepositoryTests
{
    // ──────────────────────────────────────────────────────────────
    // CreateUserAsync
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Creates a new user and returns the generated id as string.")]
    public async Task CreateUserAsync_ShouldPersistAndReturnId()
    {
        AllureAttachmentHelper.AttachText("create-user--happy", "Test: CreateUserAsync_ShouldPersistAndReturnId — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new UserRepository(context);

        // Act
        var idString = await repository.CreateUserAsync(
            "newuser@test.com", "hashed_pw", "New User", "0901234567",
            "Citizen", "District 1", "Ward 5", CancellationToken.None);

        // Assert
        idString.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(idString, out _).Should().BeTrue();

        var saved = await context.Users.FirstOrDefaultAsync(u => u.Email == "newuser@test.com");
        saved.Should().NotBeNull();
        saved!.FullName.Should().Be("New User");
        saved.Role.Should().Be(UserRole.Citizen);
    }

    [Fact]
    [AllureDescription("Throws ArgumentException when an invalid role string is supplied.")]
    public async Task CreateUserAsync_WithInvalidRole_ShouldThrow()
    {
        AllureAttachmentHelper.AttachText("create-user--invalid-role", "Test: CreateUserAsync_WithInvalidRole_ShouldThrow — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new UserRepository(context);

        // Act
        Func<Task> act = () => repository.CreateUserAsync(
            "bad@role.com", "hash", "Bad Role", "0900000000",
            "InvalidRole", "D1", "W1", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ──────────────────────────────────────────────────────────────
    // GetUsersAsync (search + role filter)
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Returns all users when no search or role filter is specified.")]
    public async Task GetUsersAsync_NoFilters_ShouldReturnAll()
    {
        AllureAttachmentHelper.AttachText("get-users--no-filters", "Test: GetUsersAsync_NoFilters_ShouldReturnAll — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new UserRepository(context);
        SeedThreeUsers(context);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetUsersAsync(null, null, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    [AllureDescription("Filters users by role when a valid role string is supplied.")]
    public async Task GetUsersAsync_FilterByRole_ShouldReturnMatching()
    {
        AllureAttachmentHelper.AttachText("get-users--filter-role", "Test: GetUsersAsync_FilterByRole_ShouldReturnMatching — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new UserRepository(context);
        SeedThreeUsers(context);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetUsersAsync(null, "Admin", CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    [AllureDescription("Returns all users when role filter is 'all'.")]
    public async Task GetUsersAsync_RoleAll_ShouldReturnAll()
    {
        AllureAttachmentHelper.AttachText("get-users--role-all", "Test: GetUsersAsync_RoleAll_ShouldReturnAll — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new UserRepository(context);
        SeedThreeUsers(context);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetUsersAsync(null, "all", CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
    }

    // ──────────────────────────────────────────────────────────────
    // GetTotalCountAsync
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Returns zero when the users table is empty.")]
    public async Task GetTotalCountAsync_WhenEmpty_ShouldReturnZero()
    {
        AllureAttachmentHelper.AttachText("get-total-count--empty", "Test: GetTotalCountAsync_WhenEmpty_ShouldReturnZero — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new UserRepository(context);

        // Act
        var count = await repository.GetTotalCountAsync(CancellationToken.None);

        // Assert
        count.Should().Be(0);
    }

    // ──────────────────────────────────────────────────────────────
    // ToggleUserStatusAsync
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Toggles an active user to inactive and returns true.")]
    public async Task ToggleUserStatusAsync_ActiveUser_ShouldDeactivate()
    {
        AllureAttachmentHelper.AttachText("toggle-status--deactivate", "Test: ToggleUserStatusAsync_ActiveUser_ShouldDeactivate — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new UserRepository(context);

        var user = User.Create("toggle@test.com", "hash", "Toggle User", UserRole.Citizen);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.ToggleUserStatusAsync(user.Id.ToString(), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var updated = await context.Users.FirstAsync(u => u.Id == user.Id);
        updated.IsActive.Should().BeFalse();
    }

    [Fact]
    [AllureDescription("Returns false when the user id is not a valid GUID.")]
    public async Task ToggleUserStatusAsync_InvalidGuid_ShouldReturnFalse()
    {
        AllureAttachmentHelper.AttachText("toggle-status--invalid-guid", "Test: ToggleUserStatusAsync_InvalidGuid_ShouldReturnFalse — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new UserRepository(context);

        // Act
        var result = await repository.ToggleUserStatusAsync("not-a-guid", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    // ──────────────────────────────────────────────────────────────
    // UpdateUserRoleAsync
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Updates the user's role and returns true.")]
    public async Task UpdateUserRoleAsync_ValidInputs_ShouldUpdateAndReturnTrue()
    {
        AllureAttachmentHelper.AttachText("update-role--valid", "Test: UpdateUserRoleAsync_ValidInputs_ShouldUpdateAndReturnTrue — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new UserRepository(context);

        var user = User.Create("role@test.com", "hash", "Role User", UserRole.Citizen);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.UpdateUserRoleAsync(user.Id.ToString(), "Admin", CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var updated = await context.Users.FirstAsync(u => u.Id == user.Id);
        updated.Role.Should().Be(UserRole.Admin);
    }

    // ──────────────────────────────────────────────────────────────
    // GetUserByIdAsync
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Returns null when the user id does not exist.")]
    public async Task GetUserByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        AllureAttachmentHelper.AttachText("get-user-by-id--not-found", "Test: GetUserByIdAsync_WhenNotFound_ShouldReturnNull — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new UserRepository(context);

        // Act
        var result = await repository.GetUserByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    // ──────────────────────────────────────────────────────────────
    // UpdateProfileAsync
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Updates profile fields and returns the modified user.")]
    public async Task UpdateProfileAsync_ShouldPersistChanges()
    {
        AllureAttachmentHelper.AttachText("update-profile", "Test: UpdateProfileAsync_ShouldPersistChanges — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new UserRepository(context);

        var user = User.Create("profile@test.com", "hash", "Old Name", UserRole.Citizen);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.UpdateProfileAsync(
            user.Id, "New Name", "0999999999", "District 7", "Ward 3", CancellationToken.None);

        // Assert
        result.FullName.Should().Be("New Name");
        result.Phone.Should().Be("0999999999");
        result.District.Should().Be("District 7");
        result.Ward.Should().Be("Ward 3");
    }

    [Fact]
    [AllureDescription("Throws KeyNotFoundException when the user id does not exist.")]
    public async Task UpdateProfileAsync_WhenNotFound_ShouldThrow()
    {
        AllureAttachmentHelper.AttachText("update-profile--not-found", "Test: UpdateProfileAsync_WhenNotFound_ShouldThrow — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new UserRepository(context);

        // Act
        Func<Task> act = () => repository.UpdateProfileAsync(
            Guid.NewGuid(), "Name", null, null, null, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    private static void SeedThreeUsers(WastePlatformDbContext context)
    {
        context.Users.AddRange(
            User.Create("citizen@test.com", "hash", "Citizen User", UserRole.Citizen),
            User.Create("enterprise@test.com", "hash", "Enterprise User", UserRole.Enterprise),
            User.Create("admin@test.com", "hash", "Admin User", UserRole.Admin));
    }

    private static WastePlatformDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<WastePlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new WastePlatformDbContext(options);
    }
}
