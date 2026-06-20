using Moq;
using WastePlatform.Application.Complaints.Commands;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Application.Complaints;

[AllureEpic("Complaints")]
[AllureFeature("Citizen Escalate Complaint Handler")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Citizen escalation of complaints to admin")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "CitizenEscalateComplaintCommandHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Complaints")]
[AllureOwner("Team")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("complaints")]
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

    #region Sad Path - Not Found

    [Fact]
    [AllureDescription("Returns failure when complaint does not exist.")]
    public async Task Handle_ComplaintNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new CitizenEscalateComplaintCommand
        {
            ComplaintId = Guid.NewGuid(),
            CitizenId = Guid.NewGuid(),
            Reason = "Not resolved properly"
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(command.ComplaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Complaint?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Không tìm thấy khiếu nại");
        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        _mockNotificationService.Verify(
            x => x.NotifyComplaintEscalatedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Sad Path - Unauthorized

    [Fact]
    [AllureDescription("Returns failure when citizen does not own the complaint.")]
    public async Task Handle_WrongCitizen_ShouldReturnUnauthorizedFailure()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherCitizenId = Guid.NewGuid();
        var complaint = Complaint.Create(ownerId, "Original complaint content");
        // Set InProgress so status check passes
        complaint.AssignCollector(Guid.NewGuid());

        var command = new CitizenEscalateComplaintCommand
        {
            ComplaintId = complaint.Id,
            CitizenId = otherCitizenId, // Different citizen
            Reason = "Escalation attempt by non-owner"
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("không có quyền");
        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Sad Path - Invalid Status

    [Fact]
    [AllureDescription("Returns failure when complaint status is Open (cannot be escalated).")]
    public async Task Handle_ComplaintStatusOpen_ShouldReturnFailure()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var complaint = Complaint.Create(citizenId, "Complaint still open");
        // Status is Open by default

        var command = new CitizenEscalateComplaintCommand
        {
            ComplaintId = complaint.Id,
            CitizenId = citizenId,
            Reason = "Want to escalate but status is Open"
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("trạng thái hiện tại");
        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Happy Path - InProgress Status

    [Fact]
    [AllureDescription("Successfully escalates a complaint with InProgress status to admin.")]
    public async Task Handle_ComplaintInProgress_ShouldEscalateSuccessfully()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var complaint = Complaint.Create(citizenId, "Complaint that needs escalation");
        complaint.AssignCollector(Guid.NewGuid()); // → InProgress

        var command = new CitizenEscalateComplaintCommand
        {
            ComplaintId = complaint.Id,
            CitizenId = citizenId,
            Reason = "Enterprise has not resolved this properly"
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);
        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService
            .Setup(x => x.NotifyComplaintEscalatedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Admin");
        result.ComplaintId.Should().Be(complaint.Id);
        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _mockNotificationService.Verify(
            x => x.NotifyComplaintEscalatedAsync(complaint.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Happy Path - Resolved Status

    [Fact]
    [AllureDescription("Successfully escalates a complaint with Resolved status to admin.")]
    public async Task Handle_ComplaintResolved_ShouldEscalateSuccessfully()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var complaint = Complaint.Create(citizenId, "Complaint resolved but citizen unsatisfied");
        complaint.Resolve("Admin resolved it"); // → Resolved

        var command = new CitizenEscalateComplaintCommand
        {
            ComplaintId = complaint.Id,
            CitizenId = citizenId,
            Reason = "Not satisfied with resolution"
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);
        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService
            .Setup(x => x.NotifyComplaintEscalatedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        result.Success.Should().BeTrue();
        result.ComplaintId.Should().Be(complaint.Id);
        _mockNotificationService.Verify(
            x => x.NotifyComplaintEscalatedAsync(complaint.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}


