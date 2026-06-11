using Allure.Xunit.Attributes;
using FluentAssertions;
using Moq;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.WasteCategories.Queries;
using WastePlatform.Domain.Entities;
using Xunit;

namespace WastePlatform.Tests.Application.WasteCategories;

[AllureEpic("KIEM-12: Waste Category Update Test Data And Report")]
[AllureFeature("Get All Categories Handler")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Retrieve the full list of waste categories")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "GetAllCategoriesQueryHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.WasteCategories")]
[AllureOwner("Hoàng Phụng")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("https://ut-team-36.atlassian.net/browse/KIEM-12")]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("waste-categories")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-12")]
public class GetAllCategoriesQueryHandlerTests
{
    private readonly Mock<IWasteCategoryRepository> _repositoryMock;
    private readonly GetAllCategoriesQueryHandler _handler;

    public GetAllCategoriesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IWasteCategoryRepository>();
        _handler = new GetAllCategoriesQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    [AllureDescription("Maps all categories from the repository into DTOs.")]
    public async Task Handle_WhenCategoriesExist_ShouldMapAndReturnCategories()
    {
        // Arrange
        var categories = new List<WasteCategory>
        {
            new() { Id = 2, Name = "Organic", Description = "" },
            new() { Id = 1, Name = "Plastic", Description = null }
        };

        _repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var query = new GetAllCategoriesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);

        var dtoList = result.ToList();
        dtoList[0].Should().BeEquivalentTo(new WasteCategoryDto
        {
            Id = 2,
            Name = "Organic",
            Description = string.Empty
        });
        dtoList[1].Should().BeEquivalentTo(new WasteCategoryDto
        {
            Id = 1,
            Name = "Plastic",
            Description = string.Empty
        });

        _repositoryMock.Verify(
            x => x.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("Returns an empty collection when there are no categories.")]
    public async Task Handle_WhenNoCategories_ShouldReturnEmptyCollection()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<WasteCategory>());

        // Act
        var result = await _handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _repositoryMock.Verify(
            x => x.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}