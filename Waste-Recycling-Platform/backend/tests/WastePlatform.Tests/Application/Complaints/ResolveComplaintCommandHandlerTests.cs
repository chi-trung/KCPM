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
[AllureFeature("Resolve Complaint Handler")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Complaint resolution and admin response tracking")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "ResolveComplaintCommandHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Complaints")]
[AllureOwner("backend")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("complaints")]
public class ResolveComplaintCommandHandlerTests
{
    private readonly Mock<IComplaintRepository> _mockComplaintRepository;
    private readonly ResolveComplaintCommandHandler _handler;

    public ResolveComplaintCommandHandlerTests()
    {
        _mockComplaintRepository = new Mock<IComplaintRepository>();
        _handler = new ResolveComplaintCommandHandler(_mockComplaintRepository.Object);
    }

    #region Happy Path Tests

    [Fact]
    [AllureDescription("Resolves a complaint successfully and returns a success response.")]
    public async Task Handle_WithValidComplaintId_ShouldResolveComplaintSuccessfully()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var adminResponse = "After careful investigation, the complaint has been resolved. The enterprise has agreed to improve their service.";
        
        var command = new ResolveComplaintCommand
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
        AllureAttachmentHelper.AttachJson("resolve-complaint-command", command);
        result.Should().NotBeNull();
        result.Success.Should().BeTrue("Admin should successfully resolve the complaint");
        result.Message.Should().Be("Complaint resolved successfully");
        result.ComplaintId.Should().Be(complaintId);
        
        _mockComplaintRepository.Verify(
            x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()),
            Times.Once, "GetByIdAsync should be called exactly once to retrieve the complaint");
        
        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once, "SaveChangesAsync should be called exactly once to persist the resolution");
    }

    [Fact]
    [AllureDescription("Updates the complaint status to resolved and stores the admin response.")]
    public async Task Handle_WithValidComplaintId_ShouldUpdateComplaintStatusToResolved()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var adminResponse = "Complaint has been reviewed and resolved.";
        
        var command = new ResolveComplaintCommand
        {
            ComplaintId = complaintId,
            AdminResponse = adminResponse
        };

        var complaint = Complaint.Create(citizenId, "Original complaint content");
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
        AllureAttachmentHelper.AttachJson("resolve-complaint-status-command", command);
        result.Success.Should().BeTrue();
        
        // Verify complaint status has been changed to Resolved
        var complaintStatusProperty = complaint.GetType().GetProperty(nameof(Complaint.Status));
        var status = (ComplaintStatus?)complaintStatusProperty?.GetValue(complaint);
        status.Should().Be(ComplaintStatus.Resolved, "Complaint status should be changed to Resolved");
        
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
    [AllureDescription("Returns the complaint id in the success result after resolution.")]
    public async Task Handle_WithValidData_ShouldReturnCorrectComplaintIdInResult()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        
        var command = new ResolveComplaintCommand
        {
            ComplaintId = complaintId,
            AdminResponse = "Complaint resolved after comprehensive review."
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
        result.ComplaintId.Should().Be(complaintId, "Result should contain the exact complaint ID that was resolved");
    }

    [Fact]
    [AllureDescription("Stores each admin response correctly for different complaint resolutions.")]
    public async Task Handle_WithDifferentAdminResponses_ShouldStoreEachResponseCorrectly()
    {
        // Arrange
        var responses = new[] 
        { 
            "Short response",
            "This is a longer response with more details about the resolution process and outcome.",
            "Response with special characters: @#$%^&*()"
        };

        foreach (var adminResponse in responses)
        {
            // Arrange
            var complaintId = Guid.NewGuid();
            var citizenId = Guid.NewGuid();
            
            var command = new ResolveComplaintCommand
            {
                ComplaintId = complaintId,
                AdminResponse = adminResponse
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
            result.Success.Should().BeTrue();
            var adminResponseProperty = complaint.GetType().GetProperty(nameof(Complaint.AdminResponse));
            var storedResponse = (string?)adminResponseProperty?.GetValue(complaint);
            storedResponse.Should().Be(adminResponse);
        }
    }

    #endregion

    #region Sad Path Tests - Complaint Not Found

    [Fact]
    [AllureDescription("Returns a failure result when the complaint cannot be found.")]
    public async Task Handle_WithNonExistentComplaintId_ShouldReturnFailureResult()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        
        var command = new ResolveComplaintCommand
        {
            ComplaintId = complaintId,
            AdminResponse = "Attempting to resolve non-existent complaint."
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Complaint?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachJson("resolve-missing-complaint-command", command);
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
        
        var command = new ResolveComplaintCommand
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
        result.ComplaintId.Should().Be(Guid.Empty);
        
        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never, "SaveChangesAsync should not be called");
    }

    [Fact]
    public async Task Handle_WithMultipleNonExistentIds_ShouldReturnFailureForEach()
    {
        // Arrange
        var nonExistentIds = new[] 
        { 
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            Guid.NewGuid() 
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Complaint?)null);

        foreach (var complaintId in nonExistentIds)
        {
            // Arrange
            var command = new ResolveComplaintCommand
            {
                ComplaintId = complaintId,
                AdminResponse = "Response"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Complaint not found");
        }

        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never, "SaveChangesAsync should never be called for non-existent complaints");
    }

    #endregion

    #region Sad Path Tests - Invalid Input

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithNullOrEmptyAdminResponse_ShouldStillProcessResolution(string? adminResponse)
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        
        var command = new ResolveComplaintCommand
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
        // The handler doesn't explicitly validate AdminResponse, so it should still resolve
        result.Success.Should().BeTrue("Handler should process resolution even with empty response");
        
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

        var command = new ResolveComplaintCommand
        {
            ComplaintId = complaintId,
            AdminResponse = "Complaint resolved."
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
    public async Task Handle_WithMultipleResolutionAttempts_ShouldAllowSecondResolution()
    {
        // Arrange - First resolution
        var complaintId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var firstResponse = "Initial resolution details.";
        var secondResponse = "Updated resolution details.";

        var complaint = Complaint.Create(citizenId, "Complaint content");
        complaint.GetType()
            .GetProperty(nameof(Complaint.Id))
            ?.SetValue(complaint, complaintId);

        // First resolution command
        var firstCommand = new ResolveComplaintCommand
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

        // Act - First resolution
        var firstResult = await _handler.Handle(firstCommand, CancellationToken.None);

        // Assert - First resolution should succeed
        firstResult.Success.Should().BeTrue();
        var statusAfterFirst = complaint.GetType().GetProperty(nameof(Complaint.Status))?.GetValue(complaint);
        statusAfterFirst.Should().Be(ComplaintStatus.Resolved);

        // Act - Second resolution (simulating another admin action)
        var secondCommand = new ResolveComplaintCommand
        {
            ComplaintId = complaintId,
            AdminResponse = secondResponse
        };

        var secondResult = await _handler.Handle(secondCommand, CancellationToken.None);

        // Assert - Second resolution should also succeed (no status restriction at handler level)
        secondResult.Success.Should().BeTrue("Handler should allow re-resolving a complaint");
    }

    [Fact]
    public async Task Handle_ShouldNotThrowException_WhenRepositoryThrowsAndComplaintNotFound()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        
        var command = new ResolveComplaintCommand
        {
            ComplaintId = complaintId,
            AdminResponse = "Response"
        };

        _mockComplaintRepository
            .Setup(x => x.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Complaint?)null);

        // Act
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync("Handler should handle null complaint gracefully");
    }

    [Fact]
    public async Task Handle_WithValidComplaint_ShouldVerifyDataPersistence()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var adminResponse = "Final resolution details.";
        Complaint? capturedComplaint = null;

        var command = new ResolveComplaintCommand
        {
            ComplaintId = complaintId,
            AdminResponse = adminResponse
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
            .Callback(() => capturedComplaint = complaint)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedComplaint.Should().NotBeNull();
        var capturedStatus = capturedComplaint?.GetType().GetProperty(nameof(Complaint.Status))?.GetValue(capturedComplaint);
        capturedStatus.Should().Be(ComplaintStatus.Resolved);
    }

    #endregion
}
