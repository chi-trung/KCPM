using FluentAssertions;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Moq;
using WastePlatform.Application.Complaints.Commands;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;
using Xunit;

namespace WastePlatform.Tests.Application.Complaints;

[AllureEpic("Complaints")]
[AllureFeature("Citizen Escalate Complaint Handler")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Citizen escalates unresolved complaint to Admin")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "CitizenEscalateComplaintCommandHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Complaints")]
[AllureOwner("Nguyễn Minh Phụng")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("complaints")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-7")]
public class CitizenEscalateComplaintCommandHandlerTests
{
    private readonly Mock<IComplaintRepository> _mockComplaintRepository;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly CitizenEscalateComplaintCommandHandler _handler;

    public CitizenEscalateComplaintCommandHandlerTests()
    {
        _mockComplaintRepository = new Mock<IComplaintRepository>();
        _mockNotificationService = new Mock<INotificationService>();
        _handler = new CitizenEscalateComplaintCommandHandler(
            _mockComplaintRepository.Object,
            _mockNotificationService.Object);
    }

    #region Happy Path Tests

    [Theory]
    [InlineData(ComplaintStatus.InProgress)]
    [InlineData(ComplaintStatus.Resolved)]
    [AllureDescription("Citizen escalates complaint to admin successfully when current status is InProgress or Resolved.")]
    public async Task Handle_WithValidCommand_ShouldEscalateComplaintSuccessfully(ComplaintStatus initialStatus)
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var reason = "Enterprise response was not satisfactory.";

        var complaint = Complaint.Create(citizenId, "Original content");
        
        // Force state by calling domain methods or reflection
        if (initialStatus == ComplaintStatus.InProgress)
        {
            complaint.AssignCollector(Guid.NewGuid());
        }
        else if (initialStatus == ComplaintStatus.Resolved)
        {
            complaint.Resolve("Admin response");
        }

        typeof(Complaint).GetProperty(nameof(Complaint.Id))?.SetValue(complaint, complaintId);

        var command = new CitizenEscalateComplaintCommand
        {
            ComplaintId = complaintId,
            CitizenId = citizenId,
            Reason = reason
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockNotificationService
            .Setup(x => x.NotifyComplaintEscalatedAsync(complaintId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachJson("escalate-complaint-command", command);
        result.Should().NotBeNull();
        result.Success.Should().BeTrue("Citizen should be able to escalate complaint when status is InProgress or Resolved");
        result.ComplaintId.Should().Be(complaintId);
        result.Message.Should().Be("Đã chuyển khiếu nại lên Admin");

        complaint.Status.Should().Be(ComplaintStatus.Escalated);
        complaint.EscalationReason.Should().Be(reason);

        _mockComplaintRepository.Verify(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()), Times.Once);
        _mockComplaintRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockNotificationService.Verify(x => x.NotifyComplaintEscalatedAsync(complaintId, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Sad Path Tests

    [Fact]
    [AllureDescription("Fails to escalate when the complaint does not exist.")]
    public async Task Handle_WhenComplaintDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var command = new CitizenEscalateComplaintCommand
        {
            ComplaintId = complaintId,
            CitizenId = Guid.NewGuid(),
            Reason = "No reason"
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Complaint?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Không tìm thấy khiếu nại");

        _mockComplaintRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockNotificationService.Verify(x => x.NotifyComplaintEscalatedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    [AllureDescription("Fails to escalate when the complaint belongs to another citizen.")]
    public async Task Handle_WhenComplaintBelongsToAnotherCitizen_ShouldReturnFailure()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var otherCitizenId = Guid.NewGuid();

        var complaint = Complaint.Create(otherCitizenId, "Content");
        typeof(Complaint).GetProperty(nameof(Complaint.Id))?.SetValue(complaint, complaintId);

        var command = new CitizenEscalateComplaintCommand
        {
            ComplaintId = complaintId,
            CitizenId = citizenId, // current caller is different from complaint.CitizenId
            Reason = "Reason"
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Bạn không có quyền thực hiện hành động này");

        _mockComplaintRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockNotificationService.Verify(x => x.NotifyComplaintEscalatedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(ComplaintStatus.Open)]
    [InlineData(ComplaintStatus.Rejected)]
    [InlineData(ComplaintStatus.Escalated)]
    [AllureDescription("Fails to escalate when the complaint status is not InProgress or Resolved.")]
    public async Task Handle_WhenComplaintStatusIsInvalid_ShouldReturnFailure(ComplaintStatus invalidStatus)
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();

        var complaint = Complaint.Create(citizenId, "Content");
        typeof(Complaint).GetProperty(nameof(Complaint.Id))?.SetValue(complaint, complaintId);

        if (invalidStatus == ComplaintStatus.Rejected)
        {
            complaint.Reject("Admin reject");
        }
        else if (invalidStatus == ComplaintStatus.Escalated)
        {
            complaint.EscalateToAdmin("Escalation reason");
        }
        // Open status is default

        var command = new CitizenEscalateComplaintCommand
        {
            ComplaintId = complaintId,
            CitizenId = citizenId,
            Reason = "Reason"
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Không thể chuyển lên Admin ở trạng thái hiện tại");

        _mockComplaintRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockNotificationService.Verify(x => x.NotifyComplaintEscalatedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
