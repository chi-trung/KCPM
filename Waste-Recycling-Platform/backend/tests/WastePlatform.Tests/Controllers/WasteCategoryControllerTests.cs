using System.Reflection;
using Allure.Xunit.Attributes;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WastePlatform.API.Controllers;
using WastePlatform.Application.WasteCategories.Queries;
using Xunit;

namespace WastePlatform.Tests.Controllers;

[AllureEpic("Waste Categories")]
[AllureFeature("Waste Category Controller")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "List and get waste categories")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "WasteCategoryControllerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("backend")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("waste-categories")]
public class WasteCategoryControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly WasteCategoryController _controller;

    public WasteCategoryControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new WasteCategoryController(_mediatorMock.Object);
    }

    [Fact]
    [AllureDescription("Returns the full list of categories when the mediator succeeds.")]
    public async Task GetAllCategories_ShouldReturnOkWithMessageAndData()
    {
        // Arrange
        var categories = new List<WasteCategoryDto>
        {
            new() { Id = 1, Name = "Plastic", Description = "Plastic waste category" },
            new() { Id = 2, Name = "Organic", Description = "Organic waste category" }
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetAllCategoriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _controller.GetAllCategories();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
        GetPropertyValue<string>(okResult.Value!, "message").Should().Be("Categories retrieved successfully");

        var data = GetPropertyValue<IEnumerable<WasteCategoryDto>>(okResult.Value!, "data")?.ToList() ?? [];
        data.Should().HaveCount(2);
        data.Select(x => x.Name).Should().ContainInOrder("Plastic", "Organic");
        data.Should().ContainSingle(x => x.Id == 1 && x.Description == "Plastic waste category");

        _mediatorMock.Verify(
            x => x.Send(It.IsAny<GetAllCategoriesQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("Returns a single category when the category exists.")]
    public async Task GetCategoryById_WhenFound_ShouldReturnOkWithMessageAndData()
    {
        // Arrange
        var category = new WasteCategoryDto
        {
            Id = 11,
            Name = "Plastic",
            Description = "Updated description"
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCategoryByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _controller.GetCategoryById(11);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        GetPropertyValue<string>(okResult.Value!, "message").Should().Be("Category retrieved successfully");

        var data = GetPropertyValue<object>(okResult.Value!, "data");
        data.Should().NotBeNull();
        GetPropertyValue<int>(data!, "id").Should().Be(11);
        GetPropertyValue<string>(data!, "name").Should().Be("Plastic");
        GetPropertyValue<string>(data!, "description").Should().Be("Updated description");

        _mediatorMock.Verify(
            x => x.Send(It.IsAny<GetCategoryByIdQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("Returns not found when the category does not exist.")]
    public async Task GetCategoryById_WhenMissing_ShouldReturnNotFoundWithMessage()
    {
        // Arrange
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCategoryByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WasteCategoryDto?)null);

        // Act
        var result = await _controller.GetCategoryById(999);

        // Assert
        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        GetPropertyValue<string>(notFound.Value!, "message").Should().Be("Category not found");
    }

    private static T? GetPropertyValue<T>(object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (property is null)
        {
            return default;
        }

        return (T?)property.GetValue(obj);
    }
}