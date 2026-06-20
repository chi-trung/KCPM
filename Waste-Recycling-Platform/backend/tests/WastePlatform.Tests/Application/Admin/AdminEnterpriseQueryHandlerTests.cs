using Moq;
using WastePlatform.Application.Admin.Enterprises.Queries;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Application.Admin;

[AllureEpic("Admin")]
[AllureFeature("Admin Enterprise Query Handlers")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Admin queries for enterprise listing and details")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "AdminEnterpriseQueryHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Admin")]
[AllureOwner("Team")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("admin")]
[Allure.Net.Commons.Attributes.AllureTag("enterprises")]
public class AdminEnterpriseQueryHandlerTests
{
    private readonly Mock<IEnterpriseRepository> _mockRepo;

    public AdminEnterpriseQueryHandlerTests()
    {
        _mockRepo = new Mock<IEnterpriseRepository>();
    }

    private static Enterprise CreateEnterprise(string name, bool isVerified = true)
    {
        return new Enterprise
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CompanyName = name,
            IsVerified = isVerified,
            ServiceArea = "Quận 1, Quận 2",
            CapacityKgPerDay = 100,
            CreatedAt = DateTime.UtcNow
        };
    }

    #region GetEnterprisesQueryHandler

    [Fact]
    [AllureDescription("Returns all enterprises when no filters are applied.")]
    public async Task GetEnterprises_WithNoFilter_ShouldReturnAllEnterprises()
    {
        // Arrange
        var enterprises = new List<Enterprise>
        {
            CreateEnterprise("Green Enterprise A", true),
            CreateEnterprise("Eco Enterprise B", false),
        };

        _mockRepo
            .Setup(x => x.GetEnterpriseListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(enterprises);

        var handler = new GetEnterprisesQueryHandler(_mockRepo.Object);
        var query = new GetEnterprisesQuery { Page = 1, PageSize = 10 };

        // Act
        var (result, total, totalPages) = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().HaveCount(2);
        total.Should().Be(2);
        totalPages.Should().Be(1);
    }

    [Fact]
    [AllureDescription("Filters enterprises by IsVerified=true.")]
    public async Task GetEnterprises_WithIsVerifiedFilter_ShouldReturnOnlyVerified()
    {
        // Arrange
        var enterprises = new List<Enterprise>
        {
            CreateEnterprise("Verified Enterprise", true),
            CreateEnterprise("Unverified Enterprise", false),
            CreateEnterprise("Another Verified", true),
        };

        _mockRepo
            .Setup(x => x.GetEnterpriseListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(enterprises);

        var handler = new GetEnterprisesQueryHandler(_mockRepo.Object);
        var query = new GetEnterprisesQuery { Page = 1, PageSize = 10, IsVerified = true };

        // Act
        var (result, total, _) = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().HaveCount(2);
        total.Should().Be(2);
        result.All(e => e.IsVerified).Should().BeTrue();
    }

    [Fact]
    [AllureDescription("Filters enterprises by search term matching company name.")]
    public async Task GetEnterprises_WithSearchTerm_ShouldReturnMatchingEnterprises()
    {
        // Arrange
        var enterprises = new List<Enterprise>
        {
            CreateEnterprise("Green Planet Recycling"),
            CreateEnterprise("Eco Waste Solutions"),
            CreateEnterprise("Green Future Corp"),
        };

        _mockRepo
            .Setup(x => x.GetEnterpriseListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(enterprises);

        var handler = new GetEnterprisesQueryHandler(_mockRepo.Object);
        var query = new GetEnterprisesQuery { Page = 1, PageSize = 10, SearchTerm = "Green" };

        // Act
        var (result, total, _) = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().HaveCount(2);
        total.Should().Be(2);
        result.All(e => e.CompanyName.Contains("Green")).Should().BeTrue();
    }

    [Fact]
    [AllureDescription("Returns paginated results correctly with page 2.")]
    public async Task GetEnterprises_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var enterprises = Enumerable.Range(1, 15)
            .Select(i => CreateEnterprise($"Enterprise {i:D2}", true))
            .ToList();

        _mockRepo
            .Setup(x => x.GetEnterpriseListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(enterprises);

        var handler = new GetEnterprisesQueryHandler(_mockRepo.Object);
        var query = new GetEnterprisesQuery { Page = 2, PageSize = 5 };

        // Act
        var (result, total, totalPages) = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().HaveCount(5);
        total.Should().Be(15);
        totalPages.Should().Be(3); // ceil(15/5) = 3
    }

    #endregion

    #region GetEnterpriseDetailQueryHandler

    [Fact]
    [AllureDescription("Returns enterprise DTO when enterprise exists.")]
    public async Task GetEnterpriseDetail_WhenFound_ShouldReturnDto()
    {
        // Arrange
        var enterprise = CreateEnterprise("Test Enterprise", true);
        enterprise.Collectors = new List<Collector> { new() { Id = Guid.NewGuid(), EnterpriseId = enterprise.Id } };

        _mockRepo
            .Setup(x => x.GetEnterpriseByIdAsync(enterprise.Id.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(enterprise);

        var handler = new GetEnterpriseDetailQueryHandler(_mockRepo.Object);
        var query = new GetEnterpriseDetailQuery { EnterpriseId = enterprise.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().NotBeNull();
        result!.Id.Should().Be(enterprise.Id);
        result.CompanyName.Should().Be("Test Enterprise");
        result.IsVerified.Should().BeTrue();
        result.CollectorCount.Should().Be(1);
    }

    [Fact]
    [AllureDescription("Returns null when enterprise is not found.")]
    public async Task GetEnterpriseDetail_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        _mockRepo
            .Setup(x => x.GetEnterpriseByIdAsync(id.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enterprise?)null);

        var handler = new GetEnterpriseDetailQueryHandler(_mockRepo.Object);
        var query = new GetEnterpriseDetailQuery { EnterpriseId = id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().BeNull();
    }

    #endregion
}

