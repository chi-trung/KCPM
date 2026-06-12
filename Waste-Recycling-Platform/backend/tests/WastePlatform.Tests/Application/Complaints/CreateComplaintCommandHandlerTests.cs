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
[AllureFeature("Create Complaint Handler")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Complaint creation and linkage to reports")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "CreateComplaintCommandHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Complaints")]
[AllureOwner("Thanh Duy")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("complaints")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-7")]
public class CreateComplaintCommandHandlerTests
{
    private readonly Mock<IComplaintRepository> _mockComplaintRepository;
    private readonly Mock<IReportRepository> _mockReportRepository;
    private readonly CreateComplaintCommandHandler _handler;

    public CreateComplaintCommandHandlerTests()
    {
        _mockComplaintRepository = new Mock<IComplaintRepository>();
        _mockReportRepository = new Mock<IReportRepository>();
        _handler = new CreateComplaintCommandHandler(_mockComplaintRepository.Object, _mockReportRepository.Object);
    }

    #region Happy Path Tests

    [Fact]
    [AllureDescription("Creates a complaint successfully when the input content is valid.")]
    public async Task Handle_WithValidCommand_ShouldCreateComplaintSuccessfully()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var content = "This is a valid complaint about waste collection service.";
        var command = new CreateComplaintCommand
        {
            CitizenId = citizenId,
            Content = content,
            ReportId = null,
            EnterpriseId = null
        };

        var createdComplaint = Complaint.Create(citizenId, content);
        _mockComplaintRepository
            .Setup(x => x.AddAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdComplaint);
        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachJson("create-complaint-command", command);
        result.Should().NotBe(Guid.Empty, "Handler should return a valid complaint ID");
        _mockComplaintRepository.Verify(
            x => x.AddAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()),
            Times.Once, "AddAsync should be called exactly once");
        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once, "SaveChangesAsync should be called exactly once");
    }

    [Fact]
    [AllureDescription("Creates a complaint from a report and resolves the enterprise id from that report.")]
    public async Task Handle_WithValidCommandAndReportId_ShouldCreateComplaintWithEnterpriseIdFromReport()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();
        var content = "Complaint about report waste collection.";
        
        var command = new CreateComplaintCommand
        {
            CitizenId = citizenId,
            Content = content,
            ReportId = reportId,
            EnterpriseId = null
        };

        var collectionTask = CollectionTask.Create(reportId, enterpriseId);
        var report = WasteReport.Create(citizenId, 1, 10.5m, 20.5m, "Test description");
        // Set the status to Assigned (not Pending) so it's valid for complaint
        report.Accept();
        report.Assign();

        // Set the report Id and CollectionTask using reflection
        var reportType = typeof(WasteReport);
        var idProperty = reportType.GetProperty(nameof(WasteReport.Id));
        var collectionTaskProperty = reportType.GetProperty(nameof(WasteReport.CollectionTask));

        idProperty?.SetValue(report, reportId);
        collectionTaskProperty?.SetValue(report, collectionTask);

        var reportWithTask = report;

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reportWithTask);

        var createdComplaint = Complaint.Create(citizenId, content, reportId, enterpriseId);
        _mockComplaintRepository
            .Setup(x => x.AddAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdComplaint);
        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachJson("create-complaint-from-report-command", command);
        result.Should().NotBe(Guid.Empty, "Handler should return a valid complaint ID");
        _mockComplaintRepository.Verify(
            x => x.AddAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()),
            Times.Once, "AddAsync should be called exactly once");
        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once, "SaveChangesAsync should be called exactly once");
        _mockReportRepository.Verify(
            x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>()),
            Times.Once, "GetByIdAsync should be called once to retrieve the report");
    }

    #endregion

    #region Sad Path Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [AllureDescription("Rejects empty or whitespace-only complaint content.")]
    public async Task Handle_WithInvalidContent_ShouldThrowArgumentException(string? invalidContent)
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var command = new CreateComplaintCommand
        {
            CitizenId = citizenId,
            Content = invalidContent ?? "",
            ReportId = null,
            EnterpriseId = null
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));

        AllureAttachmentHelper.AttachJson("invalid-complaint-content-command", command);
        AllureAttachmentHelper.AttachText("invalid-complaint-content-error", exception.Message);
        
        _mockComplaintRepository.Verify(
            x => x.AddAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()),
            Times.Never, "AddAsync should not be called when content is invalid");
        _mockComplaintRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never, "SaveChangesAsync should not be called when content is invalid");
    }

    [Fact]
    [AllureDescription("Rejects complaint creation when the referenced report cannot be found.")]
    public async Task Handle_WithNonExistentReportId_ShouldThrowArgumentException()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var content = "Complaint about non-existent report.";

        var command = new CreateComplaintCommand
        {
            CitizenId = citizenId,
            Content = content,
            ReportId = reportId,
            EnterpriseId = null
        };

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WasteReport?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        AllureAttachmentHelper.AttachJson("non-existent-report-complaint-command", command);
        AllureAttachmentHelper.AttachText("non-existent-report-complaint-error", exception.Message);
        exception.Message.Should().Contain("Report not found");
        _mockReportRepository.Verify(
            x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>()),
            Times.Once, "GetByIdAsync should be called to retrieve the report");
        _mockComplaintRepository.Verify(
            x => x.AddAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()),
            Times.Never, "AddAsync should not be called when report is not found");
    }

    [Fact]
    [AllureDescription("Rejects complaint creation when the referenced report is still pending.")]
    public async Task Handle_WithPendingReportStatus_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var content = "Complaint about pending report.";

        var command = new CreateComplaintCommand
        {
            CitizenId = citizenId,
            Content = content,
            ReportId = reportId,
            EnterpriseId = null
        };

        var pendingReport = WasteReport.Create(citizenId, 1, 10.5m, 20.5m, "Test description");
        // pendingReport status remains Pending by default

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingReport);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        AllureAttachmentHelper.AttachJson("pending-report-complaint-command", command);
        AllureAttachmentHelper.AttachText("pending-report-complaint-error", exception.Message);
        exception.Message.Should().Contain("Cannot file a complaint for a report that has not been accepted by an enterprise yet");
        _mockReportRepository.Verify(
            x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>()),
            Times.Once, "GetByIdAsync should be called to retrieve the report");
        _mockComplaintRepository.Verify(
            x => x.AddAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()),
            Times.Never, "AddAsync should not be called when report status is Pending");
    }

    [Fact]
    [AllureDescription("Uses the explicit enterprise id when one is supplied in the complaint command.")]
    public async Task Handle_WithExplicitEnterpriseId_ShouldUseProvidedEnterpriseId()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();
        var content = "Complaint with explicit enterprise id.";

        var command = new CreateComplaintCommand
        {
            CitizenId = citizenId,
            Content = content,
            ReportId = null,
            EnterpriseId = enterpriseId
        };

        var createdComplaint = Complaint.Create(citizenId, content, null, enterpriseId);
        _mockComplaintRepository
            .Setup(x => x.AddAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdComplaint);
        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachJson("explicit-enterprise-complaint-command", command);
        result.Should().NotBe(Guid.Empty);
        _mockReportRepository.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never, "GetByIdAsync should not be called when EnterpriseId is provided");
        _mockComplaintRepository.Verify(
            x => x.AddAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()),
            Times.Once, "AddAsync should be called exactly once");
    }

    #endregion
}

