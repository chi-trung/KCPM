using Moq;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.WasteCategories.Queries;
using WastePlatform.Domain.Entities;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Application.WasteCategories;

[AllureEpic("WasteCategories")]
[AllureFeature("Waste Category Query Handlers")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Querying waste category list and individual categories")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "WasteCategoryQueryHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.WasteCategories")]
[AllureOwner("Team")]
[AllureSeverity(SeverityLevel.minor)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("waste-categories")]
public class WasteCategoryQueryHandlerTests
{
    private readonly Mock<IWasteCategoryRepository> _mockRepo;

    public WasteCategoryQueryHandlerTests()
    {
        _mockRepo = new Mock<IWasteCategoryRepository>();
    }

    private static WasteCategory CreateCategory(int id, string name, string? description = null)
    {
        return new WasteCategory
        {
            Id = id,
            Name = name,
            Description = description
        };
    }

    #region GetAllCategoriesQueryHandler

    [Fact]
    [AllureDescription("GetAllCategories returns all categories mapped to DTOs.")]
    public async Task GetAllCategories_ShouldReturnAllCategoriesMappedToDtos()
    {
        // Arrange
        var categories = new List<WasteCategory>
        {
            CreateCategory(1, "Organic", "Biodegradable waste"),
            CreateCategory(2, "Recyclable", "Plastic, paper, metal"),
            CreateCategory(3, "Hazardous", "Chemical waste")
        };

        _mockRepo
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var handler = new GetAllCategoriesQueryHandler(_mockRepo.Object);
        var query = new GetAllCategoriesQuery();

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().HaveCount(3);
        result[0].Id.Should().Be(1);
        result[0].Name.Should().Be("Organic");
        result[0].Description.Should().Be("Biodegradable waste");
        result[1].Id.Should().Be(2);
        result[2].Name.Should().Be("Hazardous");
    }

    [Fact]
    [AllureDescription("GetAllCategories maps null description to empty string.")]
    public async Task GetAllCategories_WithNullDescription_ShouldMapToEmptyString()
    {
        // Arrange
        var categories = new List<WasteCategory>
        {
            CreateCategory(1, "General Waste", null) // null description
        };

        _mockRepo
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var handler = new GetAllCategoriesQueryHandler(_mockRepo.Object);

        // Act
        var result = (await handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None)).ToList();

        // Assert
        result[0].Description.Should().Be(string.Empty);
    }

    [Fact]
    [AllureDescription("GetAllCategories returns empty enumerable when no categories exist.")]
    public async Task GetAllCategories_WithNoCategories_ShouldReturnEmpty()
    {
        // Arrange
        _mockRepo
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WasteCategory>());

        var handler = new GetAllCategoriesQueryHandler(_mockRepo.Object);

        // Act
        var result = await handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().BeEmpty();
    }

    #endregion

    #region GetCategoryByIdQueryHandler

    [Fact]
    [AllureDescription("GetCategoryById returns DTO when category exists.")]
    public async Task GetCategoryById_WhenFound_ShouldReturnDto()
    {
        // Arrange
        var category = CreateCategory(5, "Electronic Waste", "Old electronics");

        _mockRepo
            .Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var handler = new GetCategoryByIdQueryHandler(_mockRepo.Object);
        var query = new GetCategoryByIdQuery { Id = 5 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().NotBeNull();
        result!.Id.Should().Be(5);
        result.Name.Should().Be("Electronic Waste");
        result.Description.Should().Be("Old electronics");
    }

    [Fact]
    [AllureDescription("GetCategoryById returns null when category does not exist.")]
    public async Task GetCategoryById_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        _mockRepo
            .Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WasteCategory?)null);

        var handler = new GetCategoryByIdQueryHandler(_mockRepo.Object);
        var query = new GetCategoryByIdQuery { Id = 99 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().BeNull();
    }

    [Fact]
    [AllureDescription("GetCategoryById maps null description to empty string.")]
    public async Task GetCategoryById_WithNullDescription_ShouldMapToEmptyString()
    {
        // Arrange
        var category = CreateCategory(3, "Mixed Waste", null);

        _mockRepo
            .Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var handler = new GetCategoryByIdQueryHandler(_mockRepo.Object);
        var query = new GetCategoryByIdQuery { Id = 3 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result!.Description.Should().Be(string.Empty);
    }

    #endregion
}

