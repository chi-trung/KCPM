using Allure.Xunit.Attributes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WastePlatform.Domain.Entities;
using WastePlatform.Infrastructure.Persistence;
using WastePlatform.Infrastructure.Persistence.Repositories;
using Xunit;

using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Infrastructure;

[AllureEpic("KIEM-12: Waste Category Update Test Data And Report")]
[AllureFeature("Waste Category Repository")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Persist and query waste categories")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Infrastructure")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "WasteCategoryRepositoryTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Infrastructure")]
[AllureOwner("Hoàng Phụng")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("KIEM-12")]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("repository")]
[Allure.Net.Commons.Attributes.AllureTag("waste-categories")]
[Allure.Net.Commons.Attributes.AllureIssue("KIEM-12")]
public class WasteCategoryRepositoryTests
{
    [Fact]
    [AllureDescription("Returns all categories ordered by name in ascending order.")]
    public async Task GetAllAsync_ShouldReturnCategoriesOrderedByName()
    {
        AllureAttachmentHelper.AttachText("get-all-async--should-return-categories-ordered-by", "Test: GetAllAsync_ShouldReturnCategoriesOrderedByName — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new WasteCategoryRepository(context);

        context.WasteCategories.AddRange(
            new WasteCategory { Id = 2, Name = "Organic", Description = "Organic waste" },
            new WasteCategory { Id = 3, Name = "Metal", Description = "Metal waste" },
            new WasteCategory { Id = 1, Name = "Plastic", Description = "Plastic waste" });
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Select(x => x.Name).Should().ContainInOrder("Metal", "Organic", "Plastic");
    }

    [Fact]
    [AllureDescription("Returns the matching category when the id exists.")]
    public async Task GetByIdAsync_WhenCategoryExists_ShouldReturnCategory()
    {
        AllureAttachmentHelper.AttachText("get-by-id-async--when-category-exists--should-retu", "Test: GetByIdAsync_WhenCategoryExists_ShouldReturnCategory — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new WasteCategoryRepository(context);

        var category = new WasteCategory
        {
            Id = 11,
            Name = "Plastic",
            Description = "Updated description"
        };

        context.WasteCategories.Add(category);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(11, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(11);
        result.Name.Should().Be("Plastic");
        result.Description.Should().Be("Updated description");
    }

    [Fact]
    [AllureDescription("Returns null when the id does not exist.")]
    public async Task GetByIdAsync_WhenCategoryDoesNotExist_ShouldReturnNull()
    {
        AllureAttachmentHelper.AttachText("get-by-id-async--when-category-does-not-exist--sho", "Test: GetByIdAsync_WhenCategoryDoesNotExist_ShouldReturnNull — passed ✅");
        // Arrange
        await using var context = CreateContext();
        var repository = new WasteCategoryRepository(context);

        // Act
        var result = await repository.GetByIdAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeNull();
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
