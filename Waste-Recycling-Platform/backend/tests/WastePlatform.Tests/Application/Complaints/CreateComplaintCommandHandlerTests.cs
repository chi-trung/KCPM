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

    #region DT-F12: Decision Table Testing — Complaint Creation (KIEM-7)
    // Áp dụng Decision Table Testing theo Ch.4 §IV.3 giáo trình:
    // Conditions: Content (Valid/Empty/TooLong) × Report Status (Accepted/Pending/NotFound/None)
    // → 6 Rules: DT-01 đến DT-06

    [Fact]
    [AllureDescription("DT-01: Decision Table — Content valid + Report Accepted → 201 Created")]
    public async Task Handle_ContentValid_ReportAccepted_ShouldCreateComplaint_DT01()
    {
        // DT-01: Content hợp lệ + Report tồn tại status Accepted → Complaint created
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();
        var content = "Khiếu nại về báo cáo rác chưa được thu gom";

        var report = WasteReport.Create(citizenId, 1, 10.5m, 20.5m, "Test report for DT-01");
        report.Accept();  // Pending → Accepted
        // Set Id via reflection (same pattern as existing tests)
        typeof(WasteReport).GetProperty(nameof(WasteReport.Id))?.SetValue(report, reportId);
        var collectionTask = CollectionTask.Create(reportId, enterpriseId);
        typeof(WasteReport).GetProperty(nameof(WasteReport.CollectionTask))?.SetValue(report, collectionTask);

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var command = new CreateComplaintCommand
        {
            CitizenId = citizenId,
            Content = content,
            ReportId = reportId,
            EnterpriseId = null
        };

        var createdComplaint = Complaint.Create(citizenId, content, reportId, enterpriseId);
        _mockComplaintRepository
            .Setup(x => x.AddAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdComplaint);
        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — DT-01: phải thành công
        AllureAttachmentHelper.AttachJson("DT-01-valid-content-accepted-report", command);
        result.Should().NotBe(Guid.Empty, "DT-01: Complaint với content valid + report Accepted phải được tạo");
        _mockComplaintRepository.Verify(x => x.AddAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [AllureDescription("DT-02: Decision Table — Content valid + Report Pending → 400 (InvalidOperationException)")]
    public async Task Handle_ContentValid_ReportPending_ShouldThrowInvalidOperation_DT02()
    {
        // DT-02: Content hợp lệ + Report tồn tại status Pending → không cho khiếu nại
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();

        var report = WasteReport.Create(citizenId, 1, 10.5m, 20.5m, "Test report for DT-02");
        // Status stays Pending (default from Create)
        typeof(WasteReport).GetProperty(nameof(WasteReport.Id))?.SetValue(report, reportId);

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var command = new CreateComplaintCommand
        {
            CitizenId = citizenId,
            Content = "Khiếu nại hợp lệ",
            ReportId = reportId,
            EnterpriseId = null
        };

        // Act & Assert — DT-02: phải throw khi report còn Pending
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        AllureAttachmentHelper.AttachJson("DT-02-valid-content-pending-report", command);
        await act.Should().ThrowAsync<InvalidOperationException>(
            "DT-02: Không thể khiếu nại về report đang Pending");
        _mockComplaintRepository.Verify(
            x => x.AddAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()),
            Times.Never, "DT-02: Không được tạo complaint khi report Pending");
    }

    [Fact]
    [AllureDescription("DT-03: Decision Table — Content valid + Report không tồn tại → 400 (ArgumentException)")]
    public async Task Handle_ContentValid_ReportNotFound_ShouldThrowArgumentException_DT03()
    {
        // DT-03: Content hợp lệ + ReportId không tồn tại → ArgumentException
        var citizenId = Guid.NewGuid();
        var fakeReportId = Guid.NewGuid();

        _mockReportRepository
            .Setup(x => x.GetByIdAsync(fakeReportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WasteReport?)null);  // Report không tồn tại

        var command = new CreateComplaintCommand
        {
            CitizenId = citizenId,
            Content = "Khiếu nại hợp lệ",
            ReportId = fakeReportId,
            EnterpriseId = null
        };

        // Act & Assert — DT-03
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        AllureAttachmentHelper.AttachJson("DT-03-valid-content-report-not-found", command);
        await act.Should().ThrowAsync<ArgumentException>(
            "DT-03: Không tìm thấy report → ArgumentException");
        _mockComplaintRepository.Verify(
            x => x.AddAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()),
            Times.Never, "DT-03: Không được tạo complaint khi report không tồn tại");
    }

    [Fact]
    [AllureDescription("DT-05: Decision Table — Content rỗng/null + bất kỳ report → 400 (ArgumentException)")]
    public async Task Handle_ContentEmpty_ShouldThrowArgumentException_DT05()
    {
        // DT-05: Content rỗng → ArgumentException ngay lập tức (không check report)
        var command = new CreateComplaintCommand
        {
            CitizenId = Guid.NewGuid(),
            Content = "",  // Content rỗng
            ReportId = null,
            EnterpriseId = null
        };

        // Act & Assert — DT-05
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        AllureAttachmentHelper.AttachJson("DT-05-empty-content", command);
        await act.Should().ThrowAsync<ArgumentException>(
            "DT-05: Content rỗng phải throw ArgumentException");
        _mockComplaintRepository.Verify(
            x => x.AddAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _mockReportRepository.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never, "DT-05: Không nên gọi repository khi content rỗng");
    }

    // Bug fixed and test activated by: Nguyễn Minh Phụng (KIEM-7)
    [Fact]
    [AllureDescription("DT-06: Decision Table — Content > 2000 ký tự (BVA max+1) → 400 (ArgumentException)")]
    [AllureOwner("Nguyễn Minh Phụng")]
    public async Task Handle_ContentTooLong_ShouldThrowArgumentException_DT06_BVA()
    {
        // DT-06 + BVA: Content vượt 2000 ký tự (BVA max boundary + 1)
        var longContent = new string('A', 2001);  // 2001 chars, vượt max 2000

        var command = new CreateComplaintCommand
        {
            CitizenId = Guid.NewGuid(),
            Content = longContent,
            ReportId = null,
            EnterpriseId = null
        };

        // Act & Assert — DT-06 + BVA: 2001 chars phải throw
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        AllureAttachmentHelper.AttachText("DT-06-content-length", $"Content length: {longContent.Length} chars (max: 2000)");
        await act.Should().ThrowAsync<ArgumentException>(
            "DT-06: Content > 2000 ký tự phải throw ArgumentException (BVA max+1)");
        _mockComplaintRepository.Verify(
            x => x.AddAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    [AllureDescription("DT-06 BVA boundary: Content = 2000 ký tự (đúng max) phải được chấp nhận")]
    public async Task Handle_ContentExactly2000Chars_ShouldSucceed_DT06_BVA_MaxBoundary()
    {
        // BVA max boundary: content = 2000 chars (đúng max, valid)
        var citizenId = Guid.NewGuid();
        var exactMaxContent = new string('A', 2000);  // 2000 chars = max boundary

        var command = new CreateComplaintCommand
        {
            CitizenId = citizenId,
            Content = exactMaxContent,
            ReportId = null,
            EnterpriseId = null
        };

        var createdComplaint = Complaint.Create(citizenId, exactMaxContent);
        _mockComplaintRepository
            .Setup(x => x.AddAsync(It.IsAny<Complaint>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdComplaint);
        _mockComplaintRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — BVA max (2000) phải pass
        AllureAttachmentHelper.AttachText("DT-BVA-max-content", $"Content length: {exactMaxContent.Length} chars");
        result.Should().NotBe(Guid.Empty, "BVA max boundary (2000 chars) phải được chấp nhận");
    }

    #endregion
}
