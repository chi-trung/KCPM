using Moq;
using WastePlatform.Application.Admin.Complaints.Commands;
using WastePlatform.Application.Admin.Complaints.Commands.Handlers;
using WastePlatform.Application.Admin.Complaints.Queries;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Application.Complaints;

[AllureEpic("Complaints")]
[AllureFeature("Admin Complaint Handlers")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Admin managing complaints via reject, resolve and query operations")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "AdminComplaintHandlersTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Complaints")]
[AllureOwner("Team")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("admin")]
[Allure.Net.Commons.Attributes.AllureTag("complaints")]
public class AdminComplaintHandlersTests
{
    private readonly Mock<IComplaintRepository> _mockRepo;

    public AdminComplaintHandlersTests()
    {
        _mockRepo = new Mock<IComplaintRepository>();
    }

    #region RejectComplaintCommandHandler - Not Found

    [Fact]
    [AllureDescription("Reject returns failure when complaint does not exist.")]
    public async Task RejectComplaint_WhenNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new RejectComplaintCommand
        {
            ComplaintId = Guid.NewGuid(),
            AdminResponse = "Rejected - not valid"
        };

        _mockRepo
            .Setup(x => x.GetByIdAsync(command.ComplaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Complaint?)null);

        var handler = new RejectComplaintCommandHandler(_mockRepo.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying command handler result");
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region RejectComplaintCommandHandler - Happy Path

    [Fact]
    [AllureDescription("Reject successfully rejects an open complaint.")]
    public async Task RejectComplaint_WhenFound_ShouldRejectSuccessfully()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var complaint = Complaint.Create(citizenId, "Complaint that will be rejected");

        _mockRepo
            .Setup(x => x.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);
        _mockRepo
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new RejectComplaintCommandHandler(_mockRepo.Object);
        var command = new RejectComplaintCommand
        {
            ComplaintId = complaint.Id,
            AdminResponse = "This complaint is invalid"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying command handler result");
        result.Success.Should().BeTrue();
        result.ComplaintId.Should().Be(complaint.Id);
        complaint.Status.Should().Be(ComplaintStatus.Rejected);
        _mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ResolveComplaintCommandHandler - Not Found

    [Fact]
    [AllureDescription("Resolve returns failure when complaint does not exist.")]
    public async Task ResolveComplaint_WhenNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new ResolveComplaintCommand
        {
            ComplaintId = Guid.NewGuid(),
            AdminResponse = "Case resolved"
        };

        _mockRepo
            .Setup(x => x.GetByIdAsync(command.ComplaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Complaint?)null);

        var handler = new ResolveComplaintCommandHandler(_mockRepo.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying command handler result");
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region ResolveComplaintCommandHandler - Happy Path

    [Fact]
    [AllureDescription("Resolve successfully resolves an open complaint.")]
    public async Task ResolveComplaint_WhenFound_ShouldResolveSuccessfully()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var complaint = Complaint.Create(citizenId, "Complaint that will be resolved");

        _mockRepo
            .Setup(x => x.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);
        _mockRepo
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new ResolveComplaintCommandHandler(_mockRepo.Object);
        var command = new ResolveComplaintCommand
        {
            ComplaintId = complaint.Id,
            AdminResponse = "Issue has been resolved by admin team"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying command handler result");
        result.Success.Should().BeTrue();
        result.ComplaintId.Should().Be(complaint.Id);
        complaint.Status.Should().Be(ComplaintStatus.Resolved);
        _mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetComplaintsQueryHandler

    [Fact]
    [AllureDescription("GetComplaints returns paginated list without status filter.")]
    public async Task GetComplaints_WithNoFilter_ShouldReturnAllComplaints()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var complaints = new List<Complaint>
        {
            Complaint.Create(citizenId, "Complaint number one"),
            Complaint.Create(citizenId, "Complaint number two"),
        };

        _mockRepo
            .Setup(x => x.GetAllAsync(1, 10, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((complaints, 2));

        var handler = new GetComplaintsQueryHandler(_mockRepo.Object);
        var query = new GetComplaintsQuery { Page = 1, PageSize = 10 };

        // Act
        var (resultComplaints, total, totalPages) = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying command handler result");
        resultComplaints.Should().HaveCount(2);
        total.Should().Be(2);
        totalPages.Should().Be(1);
        _mockRepo.Verify(
            x => x.GetAllAsync(1, 10, null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("GetComplaints parses valid status string into enum filter.")]
    public async Task GetComplaints_WithValidStatusString_ShouldParseAndPassToRepository()
    {
        // Arrange
        _mockRepo
            .Setup(x => x.GetAllAsync(1, 10, ComplaintStatus.Open, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Complaint>(), 0));

        var handler = new GetComplaintsQueryHandler(_mockRepo.Object);
        var query = new GetComplaintsQuery { Page = 1, PageSize = 10, Status = "Open" };

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert - should pass parsed enum status
        _mockRepo.Verify(
            x => x.GetAllAsync(1, 10, ComplaintStatus.Open, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("GetComplaints passes null when status string is invalid/unrecognized.")]
    public async Task GetComplaints_WithInvalidStatusString_ShouldPassNullStatus()
    {
        // Arrange
        _mockRepo
            .Setup(x => x.GetAllAsync(1, 10, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Complaint>(), 0));

        var handler = new GetComplaintsQueryHandler(_mockRepo.Object);
        var query = new GetComplaintsQuery { Page = 1, PageSize = 10, Status = "InvalidStatus" };

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert - invalid status string → null passed to repository
        _mockRepo.Verify(
            x => x.GetAllAsync(1, 10, null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetComplaintDetailQueryHandler

    [Fact]
    [AllureDescription("GetComplaintDetail returns DTO when complaint is found.")]
    public async Task GetComplaintDetail_WhenFound_ShouldReturnDto()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var complaint = Complaint.Create(citizenId, "Detailed complaint content here");

        _mockRepo
            .Setup(x => x.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        var handler = new GetComplaintDetailQueryHandler(_mockRepo.Object);
        var query = new GetComplaintDetailQuery { ComplaintId = complaint.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying command handler result");
        result.Should().NotBeNull();
        result!.Id.Should().Be(complaint.Id);
        result.CitizenId.Should().Be(citizenId);
        result.Content.Should().Be("Detailed complaint content here");
        result.Status.Should().Be(ComplaintStatus.Open);
    }

    [Fact]
    [AllureDescription("GetComplaintDetail returns null when complaint is not found.")]
    public async Task GetComplaintDetail_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        _mockRepo
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Complaint?)null);

        var handler = new GetComplaintDetailQueryHandler(_mockRepo.Object);
        var query = new GetComplaintDetailQuery { ComplaintId = id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying command handler result");
        result.Should().BeNull();
    }

    #endregion
}

