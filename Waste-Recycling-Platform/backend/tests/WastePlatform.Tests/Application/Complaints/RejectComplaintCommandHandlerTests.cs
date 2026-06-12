using FluentAssertions;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Moq;
using WastePlatform.Application.Admin.Complaints.Commands;
using WastePlatform.Application.Admin.Complaints.Commands.Handlers;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;
using Xunit;

namespace WastePlatform.Tests.Application.Complaints;

[AllureEpic("Complaints")]
[AllureFeature("Reject Complaint Handler")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Complaint rejection and admin response tracking")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "RejectComplaintCommandHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Complaints")]
[AllureOwner("Thanh Duy")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("complaints")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-7")]
public class RejectComplaintCommandHandlerTests
{
    private readonly Mock<IComplaintRepository> _mockComplaintRepository;
    private readonly RejectComplaintCommandHandler _handler;

    public RejectComplaintCommandHandlerTests()
    {
        _mockComplaintRepository = new Mock<IComplaintRepository>();
        _handler = new RejectComplaintCommandHandler(_mockComplaintRepository.Object);
    }

    #region Happy Path Tests

    [Fact]
    [AllureDescription("Rejects a complaint successfully and returns a success response.")]
    public async Task Handle_WithValidComplaintId_ShouldRejectComplaintSuccessfully()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var adminResponse = "This complaint does not meet our criteria for further investigation.";
        
        var command = new RejectComplaintCommand
        {
            ComplaintId = complaintId,
            AdminResponse = adminResponse
        };

        var complaint = Complaint.Create(citizenId, "Some complaint content");
        complaint.GetType()
            .GetProperty(nameof(Complaint.Id))
            ?.SetValue(complaint, complaintId);

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachJson("reject-complaint-command", command);
        result.Should().NotBeNull();
        result.Success.Should().BeTrue("Admin should successfully reject the complaint");
        result.Message.Should().Be("Complaint rejected successfully");
        result.ComplaintId.Should().Be(complaintId);
        
        _mockComplaintRepository.Verify(
            x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()),
            Times.Once, "GetByIdAsync should be called exactly once to retrieve the complaint");
        
        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once, "SaveChangesAsync should be called exactly once to persist the rejection");
    }

    [Fact]
    [AllureDescription("Updates the complaint status to rejected and stores the admin response.")]
    public async Task Handle_WithValidComplaintId_ShouldUpdateComplaintStatusToRejected()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var adminResponse = "Investigation concluded. Complaint rejected.";
        
        var command = new RejectComplaintCommand
        {
            ComplaintId = complaintId,
            AdminResponse = adminResponse
        };

        var complaint = Complaint.Create(citizenId, "Original complaint content");
        complaint.GetType()
            .GetProperty(nameof(Complaint.Id))
            ?.SetValue(complaint, complaintId);

        Complaint? capturedComplaint = null;
        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                // Capture the state of complaint after Reject is called
                capturedComplaint = complaint;
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachJson("reject-complaint-status-command", command);
        result.Success.Should().BeTrue();
        
        // Verify complaint status has been changed to Rejected
        var complaintStatusProperty = complaint.GetType().GetProperty(nameof(Complaint.Status));
        var status = (ComplaintStatus?)complaintStatusProperty?.GetValue(complaint);
        status.Should().Be(ComplaintStatus.Rejected, "Complaint status should be changed to Rejected");
        
        // Verify AdminResponse was set
        var adminResponseProperty = complaint.GetType().GetProperty(nameof(Complaint.AdminResponse));
        var response = (string?)adminResponseProperty?.GetValue(complaint);
        response.Should().Be(adminResponse, "Admin response should be stored");
        
        // Verify ResolvedAt was set
        var resolvedAtProperty = complaint.GetType().GetProperty(nameof(Complaint.ResolvedAt));
        var resolvedAt = (DateTime?)resolvedAtProperty?.GetValue(complaint);
        resolvedAt.Should().NotBeNull("ResolvedAt should be set");
        resolvedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [AllureDescription("Returns the complaint id in the rejection result.")]
    public async Task Handle_WithValidData_ShouldReturnCorrectComplaintIdInResult()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        
        var command = new RejectComplaintCommand
        {
            ComplaintId = complaintId,
            AdminResponse = "Complaint rejected after review."
        };

        var complaint = Complaint.Create(citizenId, "Complaint content");
        complaint.GetType()
            .GetProperty(nameof(Complaint.Id))
            ?.SetValue(complaint, complaintId);

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ComplaintId.Should().Be(complaintId, "Result should contain the exact complaint ID that was rejected");
    }

    #endregion

    #region Sad Path Tests - Complaint Not Found

    [Fact]
    [AllureDescription("Returns a failure result when the complaint cannot be found.")]
    public async Task Handle_WithNonExistentComplaintId_ShouldReturnFailureResult()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        
        var command = new RejectComplaintCommand
        {
            ComplaintId = complaintId,
            AdminResponse = "Attempting to reject non-existent complaint."
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Complaint?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachJson("reject-missing-complaint-command", command);
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("Should return failure when complaint is not found");
        result.Message.Should().Be("Complaint not found");
        result.ComplaintId.Should().Be(complaintId);
        
        _mockComplaintRepository.Verify(
            x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()),
            Times.Once, "Should attempt to retrieve the complaint");
        
        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never, "SaveChangesAsync should not be called when complaint is not found");
    }

    [Fact]
    [AllureDescription("Returns a not-found style failure when the complaint id is empty.")]
    public async Task Handle_WithEmptyComplaintId_ShouldReturnNotFoundResult()
    {
        // Arrange
        var complaintId = Guid.Empty;
        
        var command = new RejectComplaintCommand
        {
            ComplaintId = complaintId,
            AdminResponse = "Response to empty ID."
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Complaint?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Complaint not found");
        
        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never, "SaveChangesAsync should not be called");
    }

    #endregion

    #region Sad Path Tests - Invalid Input

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [AllureDescription("Processes the rejection even when the admin response is empty or whitespace.")]
    public async Task Handle_WithNullOrEmptyAdminResponse_ShouldStillProcessRejection(string? adminResponse)
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        
        var command = new RejectComplaintCommand
        {
            ComplaintId = complaintId,
            AdminResponse = adminResponse ?? ""
        };

        var complaint = Complaint.Create(citizenId, "Complaint content");
        complaint.GetType()
            .GetProperty(nameof(Complaint.Id))
            ?.SetValue(complaint, complaintId);

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // The handler doesn't explicitly validate AdminResponse, so it should still reject
        result.Success.Should().BeTrue("Handler should process rejection even with empty response");
        
        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once, "SaveChangesAsync should be called even with empty response");
    }

    #endregion

    #region Additional Edge Case Tests

    [Fact]
    public async Task Handle_ShouldCallRepositoryMethodsInCorrectOrder()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var callOrder = new List<string>();

        var command = new RejectComplaintCommand
        {
            ComplaintId = complaintId,
            AdminResponse = "Complaint rejected."
        };

        var complaint = Complaint.Create(citizenId, "Complaint content");
        complaint.GetType()
            .GetProperty(nameof(Complaint.Id))
            ?.SetValue(complaint, complaintId);

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("GetByIdAsync"))
            .ReturnsAsync(complaint);

        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChangesAsync"))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        callOrder.Should().Equal("GetByIdAsync", "SaveChangesAsync");
    }

    [Fact]
    public async Task Handle_WithMultipleRejectionAttempts_ShouldAllowSecondRejection()
    {
        // Arrange - First rejection
        var complaintId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var firstResponse = "Initial rejection reason.";
        var secondResponse = "Updated rejection reason.";

        var complaint = Complaint.Create(citizenId, "Complaint content");
        complaint.GetType()
            .GetProperty(nameof(Complaint.Id))
            ?.SetValue(complaint, complaintId);

        // First rejection command
        var firstCommand = new RejectComplaintCommand
        {
            ComplaintId = complaintId,
            AdminResponse = firstResponse
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act - First rejection
        var firstResult = await _handler.Handle(firstCommand, CancellationToken.None);

        // Assert - First rejection should succeed
        firstResult.Success.Should().BeTrue();
        var statusAfterFirst = complaint.GetType().GetProperty(nameof(Complaint.Status))?.GetValue(complaint);
        statusAfterFirst.Should().Be(ComplaintStatus.Rejected);

        // Act - Second rejection (simulating another admin action)
        var secondCommand = new RejectComplaintCommand
        {
            ComplaintId = complaintId,
            AdminResponse = secondResponse
        };

        var secondResult = await _handler.Handle(secondCommand, CancellationToken.None);

        // Assert - Second rejection should also succeed (no status restriction at handler level)
        secondResult.Success.Should().BeTrue("Handler should allow re-rejecting a complaint");
    }

    #endregion
}

