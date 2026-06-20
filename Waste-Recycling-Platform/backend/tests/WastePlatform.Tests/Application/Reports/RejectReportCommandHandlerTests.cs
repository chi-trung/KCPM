using Allure.Xunit.Attributes;
using FluentAssertions;
using Moq;
using WastePlatform.Application.Reports.Commands;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;
using Xunit;

namespace WastePlatform.Tests.Application.Reports;

/// <summary>
/// Unit tests for RejectReportCommandHandler
/// TC-REP-006: Reject Report with Reason - Authorized Role
/// TC-REP-007: Invalid State Transition
/// </summary>
[AllureEpic("KIEM-5: Reports Module Testing")]
[AllureFeature("Reject Report Handler")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Enterprise rejects a pending waste report with reason")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "RejectReportCommandHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Reports")]
[AllureOwner("Nguyễn Minh Phụng")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("reports")]
[Allure.Net.Commons.Attributes.AllureTag("state-transition")]
[Allure.Net.Commons.Attributes.AllureIssue("KIEM-5")]
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
    [AllureDescription("TC-REP-006: Enterprise rejects a Pending report with a reason — status transitions to Rejected.")]
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

        AllureAttachmentHelper.AttachJson("reject-report-command", new { command.ReportId, command.RejectionReason, InitialStatus = "Pending" });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        AllureAttachmentHelper.AttachJson("reject-report-result", new { result.ReportId, ReportStatus = result.ReportStatus.ToString(), result.Message });

        // Assert
        result.Should().NotBeNull();
        result.ReportId.Should().Be(reportId);
        result.ReportStatus.Should().Be(ReportStatus.Rejected);
        result.Message.Should().Contain("validation successful");
    }

    [Fact]
    [AllureDescription("TC-REP-006: Reject with empty reason is allowed — reason is optional.")]
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

        AllureAttachmentHelper.AttachJson("reject-empty-reason-command", new { command.ReportId, command.RejectionReason, InitialStatus = "Pending" });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        AllureAttachmentHelper.AttachJson("reject-empty-reason-result", new { result.ReportId, ReportStatus = result.ReportStatus.ToString() });

        // Assert
        result.Should().NotBeNull();
        result.ReportStatus.Should().Be(ReportStatus.Rejected);
    }

    #endregion

    #region TC-REP-007: Invalid State Transitions

    [Fact]
    [AllureDescription("TC-REP-007: Rejecting an Accepted report throws InvalidOperationException.")]
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
        report.Accept();

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var command = new RejectReportCommand { ReportId = reportId, RejectionReason = "Trying to reject accepted report" };

        AllureAttachmentHelper.AttachJson("reject-accepted-report-command", new { command.ReportId, CurrentStatus = "Accepted" });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        AllureAttachmentHelper.AttachText("reject-accepted-report-error", exception.Message);
        exception.Message.Should().Contain("can only be rejected if it is in Pending status");
        exception.Message.Should().Contain("Current status: Accepted");
    }

    [Fact]
    [AllureDescription("TC-REP-007: Rejecting an already-Rejected report throws InvalidOperationException.")]
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

        var command = new RejectReportCommand { ReportId = reportId, RejectionReason = "Trying to reject again" };

        AllureAttachmentHelper.AttachJson("reject-already-rejected-command", new { command.ReportId, CurrentStatus = "Rejected" });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        AllureAttachmentHelper.AttachText("reject-already-rejected-error", exception.Message);
        exception.Message.Should().Contain("can only be rejected if it is in Pending status");
        exception.Message.Should().Contain("Current status: Rejected");
    }

    [Fact]
    [AllureDescription("TC-REP-007: Rejecting an Assigned report throws InvalidOperationException.")]
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

        var command = new RejectReportCommand { ReportId = reportId, RejectionReason = "Trying to reject assigned report" };

        AllureAttachmentHelper.AttachJson("reject-assigned-report-command", new { command.ReportId, CurrentStatus = "Assigned" });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        AllureAttachmentHelper.AttachText("reject-assigned-report-error", exception.Message);
        exception.Message.Should().Contain("can only be rejected if it is in Pending status");
        exception.Message.Should().Contain("Current status: Assigned");
    }

    #endregion

    #region TC-REP-004: Report Not Found

    [Fact]
    [AllureDescription("TC-REP-004: Reject handler returns InvalidOperationException when report ID does not exist.")]
    public async Task Handle_WhenReportDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var nonExistentReportId = Guid.NewGuid();

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(nonExistentReportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WasteReport?)null);

        var command = new RejectReportCommand { ReportId = nonExistentReportId, RejectionReason = "Some reason" };

        AllureAttachmentHelper.AttachJson("reject-not-found-command", new { command.ReportId });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        AllureAttachmentHelper.AttachText("reject-not-found-error", exception.Message);
        exception.Message.Should().Be("Report not found");
    }

    #endregion
}
