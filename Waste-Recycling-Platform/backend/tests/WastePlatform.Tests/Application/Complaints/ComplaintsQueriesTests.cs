using FluentAssertions;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Moq;
using WastePlatform.Application.Complaints.Queries;
using WastePlatform.Application.Admin.Complaints.Queries;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;
using Xunit;

namespace WastePlatform.Tests.Application.Complaints;

[AllureEpic("Complaints")]
[AllureFeature("Complaints Queries")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Queries for retrieving complaint details and lists")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "ComplaintsQueriesTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Complaints")]
[AllureOwner("Nguyễn Minh Phụng")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("complaints")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-7")]
public class ComplaintsQueriesTests
{
    private readonly Mock<IComplaintRepository> _mockComplaintRepository;

    public ComplaintsQueriesTests()
    {
        _mockComplaintRepository = new Mock<IComplaintRepository>();
    }

    #region GetComplaintByIdQuery Tests

    [Fact]
    [AllureDescription("GetComplaintById returns the correct Complaint DTO when the complaint exists.")]
    public async Task GetComplaintById_WhenExists_ShouldReturnComplaintDto()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        
        var complaint = Complaint.Create(citizenId, "Delayed trash pickup", reportId);
        typeof(Complaint).GetProperty(nameof(Complaint.Id))?.SetValue(complaint, complaintId);

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        var query = new GetComplaintByIdQuery { Id = complaintId };
        var handler = new GetComplaintByIdQueryHandler(_mockComplaintRepository.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachJson("get-complaint-by-id-result", result!);
        result.Should().NotBeNull();
        result!.Id.Should().Be(complaintId);
        result.CitizenId.Should().Be(citizenId);
        result.ReportId.Should().Be(reportId);
        result.Content.Should().Be("Delayed trash pickup");
        result.Status.Should().Be(ComplaintStatus.Open);
    }

    [Fact]
    [AllureDescription("GetComplaintById returns null when the complaint does not exist.")]
    public async Task GetComplaintById_WhenDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Complaint?)null);

        var query = new GetComplaintByIdQuery { Id = complaintId };
        var handler = new GetComplaintByIdQueryHandler(_mockComplaintRepository.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetCitizenComplaintsQuery Tests

    [Fact]
    [AllureDescription("GetCitizenComplaints returns a paginated list of complaints for a citizen.")]
    public async Task GetCitizenComplaints_ShouldReturnPaginatedResults()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var list = new List<Complaint>
        {
            Complaint.Create(citizenId, "Complaint 1"),
            Complaint.Create(citizenId, "Complaint 2")
        };

        _mockComplaintRepository
            .Setup(x => x.GetByCitizenIdAsync(citizenId, 1, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((list, 2));

        var query = new GetCitizenComplaintsQuery { CitizenId = citizenId, Page = 1, PageSize = 10, Status = null };
        var handler = new GetCitizenComplaintsQueryHandler(_mockComplaintRepository.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachJson("get-citizen-complaints-result", result);
        result.Should().NotBeNull();
        result.Total.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.First().Content.Should().Be("Complaint 1");
    }

    #endregion

    #region GetEnterpriseComplaintsQuery Tests

    [Fact]
    [AllureDescription("GetEnterpriseComplaints returns paginated complaints for an enterprise.")]
    public async Task GetEnterpriseComplaints_ShouldReturnPaginatedResults()
    {
        // Arrange
        var enterpriseId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var list = new List<Complaint>
        {
            Complaint.Create(citizenId, "Complaint 1", null, enterpriseId)
        };

        _mockComplaintRepository
            .Setup(x => x.GetByEnterpriseIdAsync(enterpriseId, 1, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((list, 1));

        var query = new GetEnterpriseComplaintsQuery { EnterpriseId = enterpriseId, Page = 1, PageSize = 10, Status = null };
        var handler = new GetEnterpriseComplaintsQueryHandler(_mockComplaintRepository.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Complaints.Should().HaveCount(1);
        result.Total.Should().Be(1);
        result.TotalPages.Should().Be(1);
    }

    #endregion

    #region GetComplaintsQuery (Admin) Tests

    [Fact]
    [AllureDescription("GetComplaints returns paginated list of complaints for Admin.")]
    public async Task GetComplaints_Admin_ShouldReturnPaginatedResults()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var list = new List<Complaint>
        {
            Complaint.Create(citizenId, "Complaint to Admin")
        };

        _mockComplaintRepository
            .Setup(x => x.GetAllAsync(1, 10, ComplaintStatus.Open, "Admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync((list, 1));

        var query = new GetComplaintsQuery { Page = 1, PageSize = 10, Status = "Open", SearchTerm = "Admin" };
        var handler = new GetComplaintsQueryHandler(_mockComplaintRepository.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Complaints.Should().HaveCount(1);
        result.Total.Should().Be(1);
        result.TotalPages.Should().Be(1);
    }

    #endregion

    #region GetComplaintDetailQuery (Admin) Tests

    [Fact]
    [AllureDescription("GetComplaintDetail returns details of a specific complaint for Admin.")]
    public async Task GetComplaintDetail_WhenExists_ShouldReturnComplaintDto()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var complaint = Complaint.Create(citizenId, "Admin check details");
        typeof(Complaint).GetProperty(nameof(Complaint.Id))?.SetValue(complaint, complaintId);

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        var query = new GetComplaintDetailQuery { ComplaintId = complaintId };
        var handler = new GetComplaintDetailQueryHandler(_mockComplaintRepository.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(complaintId);
        result.Content.Should().Be("Admin check details");
    }

    #endregion
}
