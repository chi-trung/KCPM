using FluentAssertions;
using Moq;
using WastePlatform.Application.Reports.Commands;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using Xunit;

namespace WastePlatform.Tests.Application.Reports;

/// <summary>
/// Unit tests for RejectReportCommandHandler
/// TC-REP-006: Reject Report with Reason - Authorized Role
/// TC-REP-007: Invalid State Transition
/// </summary>
public class RejectReportCommandHandlerTests
{
    private readonly Mock<IReportRepository> _mockReportRepository;
    private readonly RejectReportCommandHandler _handler;

    public RejectReportCommandHandlerTests()
    {
        _mockReportRepository = new Mock<IReportRepository>();
        _handler = new RejectReportCommandHandler(_mockReportRepository.Object);
    }

    #region TC-REP-006: Happy Path - Reject Pending Report with Reason

    [Fact]
    public async Task Handle_WhenReportIsPending_WithValidReason_ShouldRejectSuccessfully()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var report = WasteReport.Create(
            citizenId: Guid.NewGuid(),
            wasteCategoryId: 1,
            latitude: 10.7769m,
            longitude: 106.7009m,
            description: "Test report",
            address: "Test address",
            aiSuggestion: "Recyclable");

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var command = new RejectReportCommand
        {
            ReportId = reportId,
            RejectionReason = "Vị trí không chính xác, không tìm thấy rác thải"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ReportId.Should().Be(reportId);
        result.ReportStatus.Should().Be(ReportStatus.Rejected);
        result.Message.Should().Contain("validation successful");
    }

    [Fact]
    public async Task Handle_WhenReportIsPending_WithEmptyReason_ShouldRejectSuccessfully()
    {
        // Arrange - Empty reason is allowed (optional)
        var reportId = Guid.NewGuid();
        var report = WasteReport.Create(
            citizenId: Guid.NewGuid(),
            wasteCategoryId: 1,
            latitude: 10m,
            longitude: 106m,
            description: "Test",
            address: "Test",
            aiSuggestion: "Mixed");

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var command = new RejectReportCommand
        {
            ReportId = reportId,
            RejectionReason = "" // Empty reason
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ReportStatus.Should().Be(ReportStatus.Rejected);
    }

    #endregion

    #region TC-REP-007: Invalid State Transitions

    [Fact]
    public async Task Handle_WhenReportIsAccepted_ShouldThrowInvalidOperationException()
    {
        // Arrange - Report already Accepted
        var reportId = Guid.NewGuid();
        var report = WasteReport.Create(
            citizenId: Guid.NewGuid(),
            wasteCategoryId: 1,
            latitude: 10m,
            longitude: 106m,
            description: "Test",
            address: "Test",
            aiSuggestion: "Mixed");
        report.Accept(); // Pre-accept the report

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var command = new RejectReportCommand
        {
            ReportId = reportId,
            RejectionReason = "Trying to reject accepted report"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("can only be rejected if it is in Pending status");
        exception.Message.Should().Contain("Current status: Accepted");
    }

    [Fact]
    public async Task Handle_WhenReportIsAlreadyRejected_ShouldThrowInvalidOperationException()
    {
        // Arrange - Report already Rejected
        var reportId = Guid.NewGuid();
        var report = WasteReport.Create(
            citizenId: Guid.NewGuid(),
            wasteCategoryId: 1,
            latitude: 10m,
            longitude: 106m,
            description: "Test",
            address: "Test",
            aiSuggestion: "Mixed");
        report.Reject(); // Pre-reject the report

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var command = new RejectReportCommand
        {
            ReportId = reportId,
            RejectionReason = "Trying to reject again"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("can only be rejected if it is in Pending status");
        exception.Message.Should().Contain("Current status: Rejected");
    }

    [Fact]
    public async Task Handle_WhenReportIsAssigned_ShouldThrowInvalidOperationException()
    {
        // Arrange - Report already Assigned
        var reportId = Guid.NewGuid();
        var report = WasteReport.Create(
            citizenId: Guid.NewGuid(),
            wasteCategoryId: 1,
            latitude: 10m,
            longitude: 106m,
            description: "Test",
            address: "Test",
            aiSuggestion: "Mixed");
        report.Accept();
        report.Assign(); // Move to Assigned

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var command = new RejectReportCommand
        {
            ReportId = reportId,
            RejectionReason = "Trying to reject assigned report"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("can only be rejected if it is in Pending status");
        exception.Message.Should().Contain("Current status: Assigned");
    }

    #endregion

    #region TC-REP-004: Report Not Found

    [Fact]
    public async Task Handle_WhenReportDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var nonExistentReportId = Guid.NewGuid();

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(nonExistentReportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WasteReport?)null);

        var command = new RejectReportCommand
        {
            ReportId = nonExistentReportId,
            RejectionReason = "Some reason"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Report not found");
    }

    #endregion
}
