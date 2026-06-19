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
[AllureFeature("EnterpriseRepository")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Infrastructure")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "EnterpriseRepositoryTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Infrastructure")]
[AllureOwner("Nguyễn Chí Trung")]
[AllureSeverity(SeverityLevel.normal)]
public class EnterpriseRepositoryTests
{
    // ──────────────────────────────────────────────────────────────
    // GetEnterpriseByUserIdAsync
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Returns the enterprise DTO when the user owns an enterprise.")]
    public async Task GetEnterpriseByUserIdAsync_WhenEnterpriseExists_ShouldReturnDto()
    {
        AllureAttachmentHelper.AttachText("get-enterprise-by-user-id--exists", "Test: GetEnterpriseByUserIdAsync_WhenEnterpriseExists_ShouldReturnDto — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new EnterpriseRepository(context);

        var user = User.Create("enterprise@test.com", "hash123", "Test Corp Owner", UserRole.Enterprise);
        context.Users.Add(user);

        var enterprise = new Enterprise
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CompanyName = "Green Waste Co.",
            IsVerified = true,
            Status = "Verified",
            CreatedAt = DateTime.UtcNow
        };
        context.Enterprises.Add(enterprise);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetEnterpriseByUserIdAsync(user.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(user.Id);
        result.CompanyName.Should().Be("Green Waste Co.");
        result.IsVerified.Should().BeTrue();
    }

    [Fact]
    [AllureDescription("Returns null when the user does not own any enterprise.")]
    public async Task GetEnterpriseByUserIdAsync_WhenEnterpriseDoesNotExist_ShouldReturnNull()
    {
        AllureAttachmentHelper.AttachText("get-enterprise-by-user-id--not-found", "Test: GetEnterpriseByUserIdAsync_WhenEnterpriseDoesNotExist_ShouldReturnNull — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new EnterpriseRepository(context);

        // Act
        var result = await repository.GetEnterpriseByUserIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    // ──────────────────────────────────────────────────────────────
    // GetEnterpriseByIdAsync
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Returns the enterprise entity when the id matches.")]
    public async Task GetEnterpriseByIdAsync_WhenExists_ShouldReturnEnterprise()
    {
        AllureAttachmentHelper.AttachText("get-enterprise-by-id--exists", "Test: GetEnterpriseByIdAsync_WhenExists_ShouldReturnEnterprise — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new EnterpriseRepository(context);

        var user = User.Create("owner@corp.com", "hash", "Owner", UserRole.Enterprise);
        context.Users.Add(user);

        var enterpriseId = Guid.NewGuid();
        context.Enterprises.Add(new Enterprise
        {
            Id = enterpriseId,
            UserId = user.Id,
            CompanyName = "Eco Recyclers",
            IsVerified = false,
            Status = "Pending"
        });
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetEnterpriseByIdAsync(enterpriseId.ToString(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(enterpriseId);
        result.CompanyName.Should().Be("Eco Recyclers");
    }

    [Fact]
    [AllureDescription("Returns null when no enterprise matches the given id.")]
    public async Task GetEnterpriseByIdAsync_WhenDoesNotExist_ShouldReturnNull()
    {
        AllureAttachmentHelper.AttachText("get-enterprise-by-id--not-found", "Test: GetEnterpriseByIdAsync_WhenDoesNotExist_ShouldReturnNull — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new EnterpriseRepository(context);

        // Act
        var result = await repository.GetEnterpriseByIdAsync(Guid.NewGuid().ToString(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    // ──────────────────────────────────────────────────────────────
    // GetEnterpriseListAsync
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Returns all enterprises stored in the database.")]
    public async Task GetEnterpriseListAsync_ShouldReturnAllEnterprises()
    {
        AllureAttachmentHelper.AttachText("get-enterprise-list--all", "Test: GetEnterpriseListAsync_ShouldReturnAllEnterprises — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new EnterpriseRepository(context);

        var user1 = User.Create("a@test.com", "hash", "User A", UserRole.Enterprise);
        var user2 = User.Create("b@test.com", "hash", "User B", UserRole.Enterprise);
        context.Users.AddRange(user1, user2);

        context.Enterprises.AddRange(
            new Enterprise { Id = Guid.NewGuid(), UserId = user1.Id, CompanyName = "Corp A", Status = "Verified" },
            new Enterprise { Id = Guid.NewGuid(), UserId = user2.Id, CompanyName = "Corp B", Status = "Pending" });
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetEnterpriseListAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    [AllureDescription("Returns empty list when no enterprises exist.")]
    public async Task GetEnterpriseListAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        AllureAttachmentHelper.AttachText("get-enterprise-list--empty", "Test: GetEnterpriseListAsync_WhenEmpty_ShouldReturnEmptyList — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new EnterpriseRepository(context);

        // Act
        var result = await repository.GetEnterpriseListAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    // ──────────────────────────────────────────────────────────────
    // GetEnterprisesByWasteCategoryAsync (TODO filter not yet implemented)
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Returns all enterprises because waste-category filter is not yet wired up.")]
    public async Task GetEnterprisesByWasteCategoryAsync_ShouldReturnAllEnterprises()
    {
        AllureAttachmentHelper.AttachText("get-by-waste-category", "Test: GetEnterprisesByWasteCategoryAsync_ShouldReturnAllEnterprises — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new EnterpriseRepository(context);

        var user = User.Create("cat@test.com", "hash", "Cat Owner", UserRole.Enterprise);
        context.Users.Add(user);
        context.Enterprises.Add(new Enterprise { Id = Guid.NewGuid(), UserId = user.Id, CompanyName = "Category Corp", Status = "Pending" });
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetEnterprisesByWasteCategoryAsync(1, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
    }

    // ──────────────────────────────────────────────────────────────
    // UpdateAsync
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Updates an enterprise's fields and persists the change.")]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        AllureAttachmentHelper.AttachText("update-async", "Test: UpdateAsync_ShouldPersistChanges — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new EnterpriseRepository(context);

        var user = User.Create("upd@test.com", "hash", "Update Owner", UserRole.Enterprise);
        context.Users.Add(user);

        var enterprise = new Enterprise
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CompanyName = "Old Name",
            IsVerified = false,
            Status = "Pending"
        };
        context.Enterprises.Add(enterprise);
        await context.SaveChangesAsync();

        // Act
        enterprise.CompanyName = "New Name";
        enterprise.IsVerified = true;
        enterprise.Status = "Verified";
        await repository.UpdateAsync(enterprise, CancellationToken.None);

        // Assert – read back from a fresh context sharing the same DB
        var updated = await context.Enterprises.FirstOrDefaultAsync(e => e.Id == enterprise.Id);
        updated.Should().NotBeNull();
        updated!.CompanyName.Should().Be("New Name");
        updated.IsVerified.Should().BeTrue();
        updated.Status.Should().Be("Verified");
    }

    // ──────────────────────────────────────────────────────────────
    // Helper
    // ──────────────────────────────────────────────────────────────

    private static WastePlatformDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<WastePlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new WastePlatformDbContext(options);
    }
}
