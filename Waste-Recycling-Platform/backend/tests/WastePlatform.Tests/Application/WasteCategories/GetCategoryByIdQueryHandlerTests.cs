using Allure.Xunit.Attributes;
using FluentAssertions;
using Moq;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.WasteCategories.Queries;
using WastePlatform.Domain.Entities;
using Xunit;

namespace WastePlatform.Tests.Application.WasteCategories;

[AllureEpic("Waste Categories")]
[AllureFeature("Get Category By ID Handler")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Retrieve a specific waste category by its ID")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "GetCategoryByIdQueryHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.WasteCategories")]
[AllureOwner("Hoàng Phụng")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("waste-categories")]
public class GetCategoryByIdQueryHandlerTests
{
    private readonly Mock<IWasteCategoryRepository> _repositoryMock;
    private readonly GetCategoryByIdQueryHandler _handler;

    public GetCategoryByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IWasteCategoryRepository>();
        _handler = new GetCategoryByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    [AllureDescription("Returns a category DTO when the category exists.")]
    public async Task Handle_WhenCategoryExists_ShouldReturnCategoryDto()
    {
        // Arrange
        var category = new WasteCategory
        {
            Id = 11,
            Name = "Plastic",
            Description = null
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(11, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var query = new GetCategoryByIdQuery { Id = 11 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Should().BeEquivalentTo(new WasteCategoryDto
        {
            Id = 11,
            Name = "Plastic",
            Description = string.Empty
        });

        _repositoryMock.Verify(
            x => x.GetByIdAsync(11, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("Returns null when the category does not exist.")]
    public async Task Handle_WhenCategoryDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WasteCategory?)null);

        // Act
        var result = await _handler.Handle(new GetCategoryByIdQuery { Id = 999 }, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(
            x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}