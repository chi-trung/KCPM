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
[AllureFeature("Enterprise Respond To Complaint Handler")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Enterprise responds to Citizen complaint")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "EnterpriseRespondToComplaintCommandHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Complaints")]
[AllureOwner("Nguyễn Minh Phụng")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("complaints")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-7")]
public class EnterpriseRespondToComplaintCommandHandlerTests
{
    private readonly Mock<IComplaintRepository> _mockComplaintRepository;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly EnterpriseRespondToComplaintCommandHandler _handler;

    public EnterpriseRespondToComplaintCommandHandlerTests()
    {
        _mockComplaintRepository = new Mock<IComplaintRepository>();
        _mockNotificationService = new Mock<INotificationService>();
        _handler = new EnterpriseRespondToComplaintCommandHandler(
            _mockComplaintRepository.Object,
            _mockNotificationService.Object);
    }

    #region Happy Path Tests

    [Fact]
    [AllureDescription("Enterprise responds normally (adds response and changes status to InProgress).")]
    public async Task Handle_WithNormalResponse_ShouldChangeStatusToInProgress()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var responseText = "We are checking with the driver and will collect today.";

        var complaint = Complaint.Create(citizenId, "Collect not done yet", null, enterpriseId);
        typeof(Complaint).GetProperty(nameof(Complaint.Id))?.SetValue(complaint, complaintId);

        var command = new EnterpriseRespondToComplaintCommand
        {
            ComplaintId = complaintId,
            EnterpriseId = enterpriseId,
            EnterpriseName = "Green Recycling",
            Response = responseText,
            ResolveImmediately = false,
            EscalateToAdmin = false
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockNotificationService
            .Setup(x => x.NotifyComplaintRepliedAsync(citizenId, complaintId, "Green Recycling", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachJson("enterprise-normal-response-command", command);
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ComplaintId.Should().Be(complaintId);
        result.NewStatus.Should().Be(ComplaintStatus.InProgress.ToString());

        complaint.Status.Should().Be(ComplaintStatus.InProgress);
        complaint.EnterpriseResponse.Should().Be(responseText);

        _mockComplaintRepository.Verify(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()), Times.Once);
        _mockComplaintRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockNotificationService.Verify(x => x.NotifyComplaintRepliedAsync(citizenId, complaintId, "Green Recycling", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [AllureDescription("Enterprise resolves complaint immediately.")]
    public async Task Handle_WithResolveImmediately_ShouldChangeStatusToResolved()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var responseText = "Issue fixed. Point compensated.";

        var complaint = Complaint.Create(citizenId, "Collect not done yet", null, enterpriseId);
        typeof(Complaint).GetProperty(nameof(Complaint.Id))?.SetValue(complaint, complaintId);

        var command = new EnterpriseRespondToComplaintCommand
        {
            ComplaintId = complaintId,
            EnterpriseId = enterpriseId,
            EnterpriseName = "Green Recycling",
            Response = responseText,
            ResolveImmediately = true,
            EscalateToAdmin = false
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.NewStatus.Should().Be(ComplaintStatus.Resolved.ToString());

        complaint.Status.Should().Be(ComplaintStatus.Resolved);
        complaint.EnterpriseResponse.Should().Be(responseText);

        _mockComplaintRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [AllureDescription("Enterprise escalates complaint to admin directly.")]
    public async Task Handle_WithEscalateToAdmin_ShouldChangeStatusToEscalated()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var responseText = "Citizen request is outside service scope.";

        var complaint = Complaint.Create(citizenId, "Collect not done yet", null, enterpriseId);
        typeof(Complaint).GetProperty(nameof(Complaint.Id))?.SetValue(complaint, complaintId);

        var command = new EnterpriseRespondToComplaintCommand
        {
            ComplaintId = complaintId,
            EnterpriseId = enterpriseId,
            EnterpriseName = "Green Recycling",
            Response = responseText,
            ResolveImmediately = false,
            EscalateToAdmin = true
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.NewStatus.Should().Be(ComplaintStatus.Escalated.ToString());

        complaint.Status.Should().Be(ComplaintStatus.Escalated);
        complaint.EscalationReason.Should().Be(responseText);

        _mockComplaintRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Sad Path Tests

    [Fact]
    [AllureDescription("Fails when complaint is not found.")]
    public async Task Handle_WhenComplaintNotFound_ShouldReturnFailure()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var command = new EnterpriseRespondToComplaintCommand
        {
            ComplaintId = complaintId,
            EnterpriseId = Guid.NewGuid(),
            EnterpriseName = "Enterprise",
            Response = "Rep"
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Complaint?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Complaint not found");

        _mockComplaintRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    [AllureDescription("Fails when enterprise is not authorized for this complaint.")]
    public async Task Handle_WhenEnterpriseNotAuthorized_ShouldReturnFailure()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var realEnterpriseId = Guid.NewGuid();
        var callingEnterpriseId = Guid.NewGuid();

        var complaint = Complaint.Create(Guid.NewGuid(), "Content", null, realEnterpriseId);
        typeof(Complaint).GetProperty(nameof(Complaint.Id))?.SetValue(complaint, complaintId);

        var command = new EnterpriseRespondToComplaintCommand
        {
            ComplaintId = complaintId,
            EnterpriseId = callingEnterpriseId,
            EnterpriseName = "Enterprise",
            Response = "Rep"
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("You are not authorized to respond to this complaint");

        _mockComplaintRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(ComplaintStatus.Resolved)]
    [InlineData(ComplaintStatus.Rejected)]
    [InlineData(ComplaintStatus.Escalated)]
    [AllureDescription("Fails when complaint is already closed (Resolved/Rejected/Escalated).")]
    public async Task Handle_WhenComplaintIsAlreadyClosed_ShouldReturnFailure(ComplaintStatus closedStatus)
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();

        var complaint = Complaint.Create(Guid.NewGuid(), "Content", null, enterpriseId);
        typeof(Complaint).GetProperty(nameof(Complaint.Id))?.SetValue(complaint, complaintId);

        if (closedStatus == ComplaintStatus.Resolved)
        {
            complaint.ResolveByEnterprise("Resolved");
        }
        else if (closedStatus == ComplaintStatus.Rejected)
        {
            complaint.Reject("Rejected");
        }
        else if (closedStatus == ComplaintStatus.Escalated)
        {
            complaint.EscalateToAdmin("Escalated");
        }

        var command = new EnterpriseRespondToComplaintCommand
        {
            ComplaintId = complaintId,
            EnterpriseId = enterpriseId,
            EnterpriseName = "Enterprise",
            Response = "Rep"
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot respond to complaint with status");

        _mockComplaintRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
