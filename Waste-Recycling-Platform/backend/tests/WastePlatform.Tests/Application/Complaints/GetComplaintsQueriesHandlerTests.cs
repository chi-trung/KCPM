using Moq;
using WastePlatform.Application.Complaints.Queries;
using WastePlatform.Application.Common.DTOs;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Application.Complaints;

[AllureEpic("Complaints")]
[AllureFeature("Complaint Query Handlers")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Querying complaints by citizen, enterprise, and ID")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "GetComplaintsQueriesHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Complaints")]
[AllureOwner("Team")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("complaints")]
public class GetComplaintsQueriesHandlerTests
{
    private readonly Mock<IComplaintRepository> _mockRepo;

    public GetComplaintsQueriesHandlerTests()
    {
        _mockRepo = new Mock<IComplaintRepository>();
    }

    #region GetCitizenComplaintsQuery

    [Fact]
    [AllureDescription("Returns paginated list of complaints for a citizen.")]
    public async Task GetCitizenComplaints_ShouldReturnPaginatedList()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();
        var complaints = new List<Complaint>
        {
            Complaint.Create(citizenId, "Complaint 1 - long enough content here", null, enterpriseId),
            Complaint.Create(citizenId, "Complaint 2 - long enough content here", null, enterpriseId),
        };
        const int total = 2;

        _mockRepo
            .Setup(x => x.GetByCitizenIdAsync(
                citizenId, 1, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((complaints, total));

        var handler = new GetCitizenComplaintsQueryHandler(_mockRepo.Object);
        var query = new GetCitizenComplaintsQuery
        {
            CitizenId = citizenId,
            Page = 1,
            PageSize = 10,
            Status = null
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Total.Should().Be(total);
        _mockRepo.Verify(
            x => x.GetByCitizenIdAsync(citizenId, 1, 10, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("Returns empty list when citizen has no complaints.")]
    public async Task GetCitizenComplaints_WithNoComplaints_ShouldReturnEmptyList()
    {
        // Arrange
        var citizenId = Guid.NewGuid();

        _mockRepo
            .Setup(x => x.GetByCitizenIdAsync(
                citizenId, 1, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Complaint>(), 0));

        var handler = new GetCitizenComplaintsQueryHandler(_mockRepo.Object);
        var query = new GetCitizenComplaintsQuery
        {
            CitizenId = citizenId,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
    }

    [Fact]
    [AllureDescription("Passes status filter through to the repository.")]
    public async Task GetCitizenComplaints_WithStatusFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var status = ComplaintStatus.Open;

        _mockRepo
            .Setup(x => x.GetByCitizenIdAsync(
                citizenId, 1, 5, status, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Complaint>(), 0));

        var handler = new GetCitizenComplaintsQueryHandler(_mockRepo.Object);
        var query = new GetCitizenComplaintsQuery
        {
            CitizenId = citizenId,
            Page = 1,
            PageSize = 5,
            Status = status
        };

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepo.Verify(
            x => x.GetByCitizenIdAsync(citizenId, 1, 5, status, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetEnterpriseComplaintsQuery

    [Fact]
    [AllureDescription("Returns paginated list of complaints for an enterprise.")]
    public async Task GetEnterpriseComplaints_ShouldReturnPaginatedList()
    {
        // Arrange
        var enterpriseId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var complaints = new List<Complaint>
        {
            Complaint.Create(citizenId, "Complaint about enterprise service", null, enterpriseId),
        };
        const int total = 1;

        _mockRepo
            .Setup(x => x.GetByEnterpriseIdAsync(
                enterpriseId, 1, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((complaints, total));

        var handler = new GetEnterpriseComplaintsQueryHandler(_mockRepo.Object);
        var query = new GetEnterpriseComplaintsQuery
        {
            EnterpriseId = enterpriseId,
            Page = 1,
            PageSize = 10
        };

        // Act
        var (resultComplaints, resultTotal, resultPages) = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: resultComplaints");
        resultComplaints.Should().HaveCount(1);
        resultTotal.Should().Be(total);
        resultPages.Should().Be(1); // ceil(1/10) = 1
        _mockRepo.Verify(
            x => x.GetByEnterpriseIdAsync(enterpriseId, 1, 10, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("Calculates total pages correctly for pagination.")]
    public async Task GetEnterpriseComplaints_WithMultiplePages_ShouldCalculateTotalPagesCorrectly()
    {
        // Arrange
        var enterpriseId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var complaints = Enumerable.Range(1, 5)
            .Select(_ => Complaint.Create(citizenId, "Test complaint content", null, enterpriseId))
            .ToList();

        _mockRepo
            .Setup(x => x.GetByEnterpriseIdAsync(
                enterpriseId, 1, 5, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((complaints, 23)); // 23 total, page size 5 → 5 pages

        var handler = new GetEnterpriseComplaintsQueryHandler(_mockRepo.Object);
        var query = new GetEnterpriseComplaintsQuery
        {
            EnterpriseId = enterpriseId,
            Page = 1,
            PageSize = 5
        };

        // Act
        var (_, _, totalPages) = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: totalPages");
        totalPages.Should().Be(5); // ceil(23/5) = 5
    }

    #endregion

    #region GetComplaintByIdQuery

    [Fact]
    [AllureDescription("Returns complaint DTO when complaint exists.")]
    public async Task GetComplaintById_WhenFound_ShouldReturnDto()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var complaint = Complaint.Create(citizenId, "Test complaint with enough content", null, null);

        _mockRepo
            .Setup(x => x.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        var handler = new GetComplaintByIdQueryHandler(_mockRepo.Object);
        var query = new GetComplaintByIdQuery { Id = complaint.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().NotBeNull();
        result!.Id.Should().Be(complaint.Id);
        result.CitizenId.Should().Be(citizenId);
        result.Content.Should().Be("Test complaint with enough content");
        result.Status.Should().Be(ComplaintStatus.Open);
    }

    [Fact]
    [AllureDescription("Returns null when complaint does not exist.")]
    public async Task GetComplaintById_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        _mockRepo
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Complaint?)null);

        var handler = new GetComplaintByIdQueryHandler(_mockRepo.Object);
        var query = new GetComplaintByIdQuery { Id = id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().BeNull();
        _mockRepo.Verify(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}

