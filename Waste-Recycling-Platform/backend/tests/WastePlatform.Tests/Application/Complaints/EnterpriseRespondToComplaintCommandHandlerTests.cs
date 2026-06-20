using Moq;
using WastePlatform.Application.Complaints.Commands;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Application.Complaints;

[AllureEpic("Complaints")]
[AllureFeature("Enterprise Respond To Complaint Handler")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Enterprise responding to citizen complaints")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "EnterpriseRespondToComplaintCommandHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Complaints")]
[AllureOwner("Team")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("complaints")]
public class EnterpriseRespondToComplaintCommandHandlerTests
{
    private readonly Mock<IComplaintRepository> _mockComplaintRepository;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly EnterpriseRespondToComplaintCommandHandler _handler;

    private readonly Guid _enterpriseId = Guid.NewGuid();
    private readonly string _enterpriseName = "Green Life Enterprise";

    public EnterpriseRespondToComplaintCommandHandlerTests()
    {
        _mockComplaintRepository = new Mock<IComplaintRepository>();
        _mockNotificationService = new Mock<INotificationService>();
        _handler = new EnterpriseRespondToComplaintCommandHandler(
            _mockComplaintRepository.Object,
            _mockNotificationService.Object);
    }

    private Complaint CreateOpenComplaint()
    {
        var citizenId = Guid.NewGuid();
        return Complaint.Create(citizenId, "Test complaint content", null, _enterpriseId);
    }

    #region Sad Path - Not Found

    [Fact]
    [AllureDescription("Returns failure when complaint does not exist.")]
    public async Task Handle_ComplaintNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new EnterpriseRespondToComplaintCommand
        {
            EnterpriseId = _enterpriseId,
            EnterpriseName = _enterpriseName,
            ComplaintId = Guid.NewGuid(),
            Response = "Our response",
            ResolveImmediately = false,
            EscalateToAdmin = false
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(command.ComplaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Complaint?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Sad Path - Unauthorized Enterprise

    [Fact]
    [AllureDescription("Returns failure when enterprise does not own the complaint.")]
    public async Task Handle_WrongEnterprise_ShouldReturnUnauthorizedFailure()
    {
        // Arrange
        var complaint = CreateOpenComplaint(); // Created with _enterpriseId
        var otherEnterpriseId = Guid.NewGuid();

        var command = new EnterpriseRespondToComplaintCommand
        {
            EnterpriseId = otherEnterpriseId, // Different enterprise
            EnterpriseName = "Other Enterprise",
            ComplaintId = complaint.Id,
            Response = "Unauthorized response attempt",
            ResolveImmediately = false,
            EscalateToAdmin = false
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not authorized");
        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Sad Path - Invalid Status

    [Fact]
    [AllureDescription("Returns failure when complaint has already been resolved.")]
    public async Task Handle_ComplaintAlreadyResolved_ShouldReturnFailure()
    {
        // Arrange
        var complaint = CreateOpenComplaint();
        complaint.Resolve("Admin resolved"); // → Resolved status

        var command = new EnterpriseRespondToComplaintCommand
        {
            EnterpriseId = _enterpriseId,
            EnterpriseName = _enterpriseName,
            ComplaintId = complaint.Id,
            Response = "Response to resolved complaint",
            ResolveImmediately = false,
            EscalateToAdmin = false
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot respond");
        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Happy Path - Escalate to Admin

    [Fact]
    [AllureDescription("Successfully escalates complaint to admin when EscalateToAdmin is true.")]
    public async Task Handle_EscalateToAdminTrue_ShouldEscalateAndNotify()
    {
        // Arrange
        var complaint = CreateOpenComplaint(); // Open status

        var command = new EnterpriseRespondToComplaintCommand
        {
            EnterpriseId = _enterpriseId,
            EnterpriseName = _enterpriseName,
            ComplaintId = complaint.Id,
            Response = "Cannot resolve, escalating",
            ResolveImmediately = false,
            EscalateToAdmin = true
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);
        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService
            .Setup(x => x.NotifyComplaintRepliedAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("escalated");
        result.NewStatus.Should().Be(ComplaintStatus.Escalated.ToString());
        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _mockNotificationService.Verify(
            x => x.NotifyComplaintRepliedAsync(
                complaint.CitizenId, complaint.Id, _enterpriseName, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Happy Path - Resolve Immediately

    [Fact]
    [AllureDescription("Successfully resolves complaint immediately when ResolveImmediately is true.")]
    public async Task Handle_ResolveImmediatelyTrue_ShouldResolveAndNotify()
    {
        // Arrange
        var complaint = CreateOpenComplaint();

        var command = new EnterpriseRespondToComplaintCommand
        {
            EnterpriseId = _enterpriseId,
            EnterpriseName = _enterpriseName,
            ComplaintId = complaint.Id,
            Response = "We have fixed the issue immediately",
            ResolveImmediately = true,
            EscalateToAdmin = false
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);
        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService
            .Setup(x => x.NotifyComplaintRepliedAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("resolved");
        result.NewStatus.Should().Be(ComplaintStatus.Resolved.ToString());
        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _mockNotificationService.Verify(
            x => x.NotifyComplaintRepliedAsync(
                complaint.CitizenId, complaint.Id, _enterpriseName, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Happy Path - Add Response Only

    [Fact]
    [AllureDescription("Successfully adds a response without resolving when both flags are false.")]
    public async Task Handle_AddResponseOnly_ShouldAddResponseAndNotify()
    {
        // Arrange
        var complaint = CreateOpenComplaint();

        var command = new EnterpriseRespondToComplaintCommand
        {
            EnterpriseId = _enterpriseId,
            EnterpriseName = _enterpriseName,
            ComplaintId = complaint.Id,
            Response = "We are looking into this issue",
            ResolveImmediately = false,
            EscalateToAdmin = false
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);
        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService
            .Setup(x => x.NotifyComplaintRepliedAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Response added");
        result.NewStatus.Should().Be(ComplaintStatus.InProgress.ToString());
        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _mockNotificationService.Verify(
            x => x.NotifyComplaintRepliedAsync(
                complaint.CitizenId, complaint.Id, _enterpriseName, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}


