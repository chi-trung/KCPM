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
/// Unit tests for AcceptReportAndCreateTaskCommandHandler
/// TC-REP-005: Accept Report - Authorized Role (Enterprise/Admin)
/// TC-REP-007: Invalid State Transition
/// </summary>
[AllureEpic("KIEM-5: Reports Module Testing")]
[AllureFeature("Accept Report Handler")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Enterprise accepts a pending waste report")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "AcceptReportCommandHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Reports")]
[AllureOwner("Nguyễn Minh Phụng")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("reports")]
[Allure.Net.Commons.Attributes.AllureTag("state-transition")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-5")]
public class AcceptReportCommandHandlerTests
{
    private readonly Mock<IReportRepository> _mockReportRepository;
    private readonly AcceptReportAndCreateTaskCommandHandler _handler;

    public AcceptReportCommandHandlerTests()
    {
        _mockReportRepository = new Mock<IReportRepository>();
        _handler = new AcceptReportAndCreateTaskCommandHandler(_mockReportRepository.Object);
    }

    #region TC-REP-005: Happy Path - Accept Pending Report

    [Fact]
    [AllureDescription("TC-REP-005: Enterprise accepts a Pending report — status transitions to Accepted and handler returns success result.")]
    public async Task Handle_WhenReportIsPending_ShouldAcceptSuccessfully()
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

        var command = new AcceptReportAndCreateTaskCommand
        {
            ReportId = reportId,
            UserId = Guid.NewGuid()
        };

        AllureAttachmentHelper.AttachJson("accept-report-command", new { command.ReportId, command.UserId, InitialStatus = "Pending" });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        AllureAttachmentHelper.AttachJson("accept-report-result", new { result.ReportId, ReportStatus = result.ReportStatus.ToString(), result.Message });

        // Assert
        result.Should().NotBeNull();
        result.ReportId.Should().Be(reportId);
        result.ReportStatus.Should().Be(ReportStatus.Accepted);
        result.Message.Should().Contain("validation successful");
    }

    #endregion

    #region TC-REP-007: Invalid State Transitions

    [Fact]
    [AllureDescription("TC-REP-007: Attempting to accept an already-Accepted report throws InvalidOperationException.")]
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

        var command = new AcceptReportAndCreateTaskCommand { ReportId = reportId, UserId = Guid.NewGuid() };

        AllureAttachmentHelper.AttachJson("accept-already-accepted-command", new { command.ReportId, CurrentStatus = "Accepted" });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        AllureAttachmentHelper.AttachText("accept-already-accepted-error", exception.Message);
        exception.Message.Should().Contain("can only be accepted if it is in Pending status");
        exception.Message.Should().Contain("Current status: Accepted");
    }

    [Fact]
    [AllureDescription("TC-REP-007: Attempting to accept an already-Rejected report throws InvalidOperationException.")]
    public async Task Handle_WhenReportIsRejected_ShouldThrowInvalidOperationException()
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

        var command = new AcceptReportAndCreateTaskCommand { ReportId = reportId, UserId = Guid.NewGuid() };

        AllureAttachmentHelper.AttachJson("accept-rejected-report-command", new { command.ReportId, CurrentStatus = "Rejected" });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        AllureAttachmentHelper.AttachText("accept-rejected-report-error", exception.Message);
        exception.Message.Should().Contain("can only be accepted if it is in Pending status");
        exception.Message.Should().Contain("Current status: Rejected");
    }

    [Fact]
    [AllureDescription("TC-REP-007: Attempting to accept a Collected report throws InvalidOperationException.")]
    public async Task Handle_WhenReportIsCollected_ShouldThrowInvalidOperationException()
    {
        // Arrange - Report already Collected
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
        report.Assign();
        report.Collect(); // Full workflow to Collected

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var command = new AcceptReportAndCreateTaskCommand
        {
            ReportId = reportId,
            UserId = Guid.NewGuid()
        };

        // Act & Assert
        AllureAttachmentHelper.AttachJson("accept-collected-report-command", new { command.ReportId, CurrentStatus = "Collected" });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        AllureAttachmentHelper.AttachText("accept-collected-report-error", exception.Message);
        exception.Message.Should().Contain("can only be accepted if it is in Pending status");
        exception.Message.Should().Contain("Current status: Collected");
    }

    #endregion

    #region TC-REP-004: Report Not Found

    [Fact]
    [AllureDescription("TC-REP-004: Accept handler returns InvalidOperationException when report ID does not exist.")]
    public async Task Handle_WhenReportDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var nonExistentReportId = Guid.NewGuid();

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(nonExistentReportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WasteReport?)null);

        var command = new AcceptReportAndCreateTaskCommand
        {
            ReportId = nonExistentReportId,
            UserId = Guid.NewGuid()
        };

        // Act & Assert
        AllureAttachmentHelper.AttachJson("accept-not-found-command", new { command.ReportId });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        AllureAttachmentHelper.AttachText("accept-not-found-error", exception.Message);
        exception.Message.Should().Be("Report not found");
    }

    #endregion
}
