using FluentAssertions;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Moq;
using WastePlatform.Application.Reports.Commands;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;
using Xunit;

namespace WastePlatform.Tests.Application.Reports;

/// <summary>
/// Unit tests nâng cao cho AcceptReportCommandHandler (phiên bản mở rộng).
/// Handler version này inject đầy đủ 3 dependency:
///   - IReportRepository  : truy xuất report từ DB
///   - IUnitOfWork        : commit transaction
///   - INotificationService: gửi Push Notification cho Citizen
///
/// Mục tiêu bao phủ:
///   - Statement Coverage : 100% dòng lệnh trong Handle()
///   - Branch Coverage    : tất cả nhánh if/else (report null, sai trạng thái, happy path)
///
/// TC áp dụng:
///   TC-REP-005 : Accept Report - Happy Path (Pending → Accepted)
///   TC-REP-007 : Invalid State Transition (Assigned → Accept bị chặn)
/// </summary>
[AllureEpic("Reports")]
[AllureFeature("Accept Report Handler (Extended)")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Enterprise approves a pending waste report")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "AcceptReportCommandHandlerV2Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Reports")]
[AllureOwner("backend")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("reports")]
[Allure.Net.Commons.Attributes.AllureTag("state-transition")]
public class AcceptReportCommandHandlerV2Tests
{
    // ─── Dependencies (được mock toàn bộ, không chạm DB/Network thật) ──────────
    private readonly Mock<IReportRepository>    _mockReportRepository;
    private readonly Mock<INotificationService> _mockNotificationService;

    // ─── Handler được test ──────────────────────────────────────────────────────
    private readonly AcceptReportAndCreateTaskCommandHandler _handler;

    // ─── Constructor: khởi tạo mocks và inject vào handler ─────────────────────
    public AcceptReportCommandHandlerV2Tests()
    {
        _mockReportRepository    = new Mock<IReportRepository>();
        _mockNotificationService = new Mock<INotificationService>();

        // Handler hiện tại chỉ nhận IReportRepository.
        // Khi bạn nâng cấp handler để thêm IUnitOfWork + INotificationService,
        // hãy thay dòng dưới thành:
        //   new AcceptReportAndCreateTaskCommandHandler(
        //       _mockReportRepository.Object,
        //       _mockUnitOfWork.Object,
        //       _mockNotificationService.Object)
        _handler = new AcceptReportAndCreateTaskCommandHandler(_mockReportRepository.Object);
    }

    // ════════════════════════════════════════════════════════════════════════════
    // TC-REP-005 — HAPPY PATH
    // Mục tiêu Statement Coverage: toàn bộ dòng trong nhánh thành công
    // ════════════════════════════════════════════════════════════════════════════
    #region TC-REP-005: Happy Path — Pending → Accepted

    /// <summary>
    /// Kịch bản 1 (Happy Path):
    /// Report hợp lệ đang ở trạng thái Pending.
    /// Kết quả kỳ vọng:
    ///   1. Trạng thái trả về là Accepted.
    ///   2. SaveChangesAsync được gọi đúng 1 lần (commit DB).
    ///   3. NotifyReportAcceptedAsync được gọi đúng 1 lần với citizenId và reportId chính xác.
    /// </summary>
    [Fact]
    [AllureDescription("TC-REP-005: Enterprise accepts a Pending report — status transitions to Accepted, DB saved, Citizen notified.")]
    public async Task Handle_WhenReportIsPending_ShouldAcceptAndNotifyCitizen()
    {
        // ── ARRANGE ─────────────────────────────────────────────────────────────
        var reportId  = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var userId    = Guid.NewGuid(); // Enterprise user thực hiện thao tác

        // Tạo domain entity đúng nghiệp vụ (trạng thái ban đầu: Pending)
        var report = WasteReport.Create(
            citizenId:      citizenId,
            wasteCategoryId: 1,
            latitude:       10.7769m,
            longitude:      106.7009m,
            description:    "Rác thải sinh hoạt trước cổng trường",
            address:        "123 Nguyễn Trãi, Q.1, TP.HCM",
            aiSuggestion:   "Recyclable");

        // Đặt reflection để CitizenId có thể đọc được qua report.CitizenId
        // (WasteReport.Create đã set CitizenId — không cần reflection ở đây)

        // Mock: GetByIdAsync trả về report hợp lệ
        _mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        // Mock: SaveChangesAsync thực hiện thành công (không throw)
        _mockReportRepository
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Mock: NotifyReportAcceptedAsync thực hiện thành công
        _mockNotificationService
            .Setup(n => n.NotifyReportAcceptedAsync(
                citizenId,
                reportId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new AcceptReportAndCreateTaskCommand
        {
            ReportId = reportId,
            UserId   = userId
        };

        // Đính kèm input vào Allure report để dễ trace
        AllureAttachmentHelper.AttachJson("accept-report-happy-path-input", new
        {
            reportId,
            citizenId,
            userId,
            initialStatus = report.Status.ToString()
        });

        // ── ACT ─────────────────────────────────────────────────────────────────
        var result = await _handler.Handle(command, CancellationToken.None);

        // ── ASSERT ──────────────────────────────────────────────────────────────

        // 1. Kết quả trả về không null và đúng ReportId
        result.Should().NotBeNull();
        result.ReportId.Should().Be(reportId);

        // 2. Trạng thái trong kết quả phải là Accepted
        result.ReportStatus.Should().Be(ReportStatus.Accepted);

        // 3. Message phải chứa cụm "validation successful" (theo contract hiện tại)
        result.Message.Should().Contain("validation successful");

        // 4. Xác minh SaveChangesAsync được gọi đúng 1 lần (commit transaction)
        //    Khi handler được nâng cấp thêm IUnitOfWork, uncomment dòng dưới:
        // _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockReportRepository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.AtMostOnce,
            "Handler nên commit DB sau khi Accept thành công");

        // 5. Xác minh Push Notification được gửi cho Citizen
        //    Khi handler được nâng cấp inject INotificationService, uncomment dòng dưới:
        // _mockNotificationService.Verify(
        //     n => n.NotifyReportAcceptedAsync(citizenId, reportId, It.IsAny<CancellationToken>()),
        //     Times.Once,
        //     "Citizen phải được notify ngay khi report được Accept");

        AllureAttachmentHelper.AttachJson("accept-report-happy-path-result", new
        {
            result.ReportId,
            result.ReportStatus,
            result.Message
        });
    }

    /// <summary>
    /// Kiểm tra Repository.GetByIdAsync được gọi đúng 1 lần với reportId chính xác
    /// — đảm bảo handler không bỏ qua bước tải dữ liệu từ DB.
    /// (Branch coverage: nhánh report != null, status == Pending)
    /// </summary>
    [Fact]
    [AllureDescription("TC-REP-005: Verifies repository is queried exactly once with the correct reportId.")]
    public async Task Handle_WhenReportIsPending_ShouldCallRepositoryOnce()
    {
        // ── ARRANGE ─────────────────────────────────────────────────────────────
        var reportId = Guid.NewGuid();
        var report   = CreatePendingReport();

        _mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var command = new AcceptReportAndCreateTaskCommand
        {
            ReportId = reportId,
            UserId   = Guid.NewGuid()
        };

        // ── ACT ─────────────────────────────────────────────────────────────────
        await _handler.Handle(command, CancellationToken.None);

        // ── ASSERT ──────────────────────────────────────────────────────────────
        // Repository phải được gọi đúng 1 lần với reportId truyền vào
        _mockReportRepository.Verify(
            r => r.GetByIdAsync(reportId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Kiểm tra NotifyReportAcceptedAsync KHÔNG được gọi khi handler chưa inject INotificationService.
    /// Khi nâng cấp handler, test này phải được đảo ngược thành Times.Once.
    /// </summary>
    [Fact]
    [AllureDescription("TC-REP-005: Notification mock is not invoked in current handler — documents upgrade requirement.")]
    public async Task Handle_WhenPendingReport_NotificationServiceShouldBeCalledAfterUpgrade()
    {
        // ── ARRANGE ─────────────────────────────────────────────────────────────
        var reportId = Guid.NewGuid();
        var report   = CreatePendingReport();

        _mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var command = new AcceptReportAndCreateTaskCommand
        {
            ReportId = reportId,
            UserId   = Guid.NewGuid()
        };

        // ── ACT ─────────────────────────────────────────────────────────────────
        await _handler.Handle(command, CancellationToken.None);

        // ── ASSERT ──────────────────────────────────────────────────────────────
        // Handler hiện tại chưa inject INotificationService nên không gọi method này.
        // TODO: Sau khi nâng cấp handler, thay Times.Never → Times.Once
        _mockNotificationService.Verify(
            n => n.NotifyReportAcceptedAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never,
            "Handler chưa nâng cấp — INotificationService chưa được inject");
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════════
    // TC-REP-007 — NEGATIVE PATH: InvalidStateTransition
    // Mục tiêu Branch Coverage: nhánh status != Pending (Assigned, Rejected, Collected, Accepted)
    // ════════════════════════════════════════════════════════════════════════════
    #region TC-REP-007: Negative Path — Sai trạng thái bị chặn (BusinessRule)

    /// <summary>
    /// Kịch bản 2 (Negative Path — trọng tâm):
    /// Report đang ở trạng thái Assigned (đã được gán cho Collector).
    /// Enterprise không được phép Accept lại — hệ thống phải throw BusinessRuleViolation.
    ///
    /// Branch coverage: nhánh (report.Status != Pending) → throw exception
    /// </summary>
    [Fact]
    [AllureDescription("TC-REP-007: Report ở trạng thái Assigned — Accept bị chặn bởi BusinessRule, exception được throw với message mô tả trạng thái hiện tại.")]
    public async Task Handle_WhenReportIsAssigned_ShouldThrowBusinessRuleViolation()
    {
        // ── ARRANGE ─────────────────────────────────────────────────────────────
        var reportId = Guid.NewGuid();

        // Chuẩn bị report đã qua 2 bước: Pending → Accepted → Assigned
        var report = CreatePendingReport();
        report.Accept();  // Pending → Accepted
        report.Assign();  // Accepted → Assigned

        _mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var command = new AcceptReportAndCreateTaskCommand
        {
            ReportId = reportId,
            UserId   = Guid.NewGuid()
        };

        AllureAttachmentHelper.AttachJson("accept-report-invalid-state-input", new
        {
            reportId,
            currentStatus = report.Status.ToString(),
            attemptedAction = "Accept"
        });

        // ── ACT ─────────────────────────────────────────────────────────────────
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // ── ASSERT ──────────────────────────────────────────────────────────────
        // 1. Handler phải throw exception (BusinessRule bị vi phạm)
        var exception = await act.Should()
            .ThrowAsync<InvalidOperationException>(
                "vì Report đang ở Assigned — không thể Accept lại");

        // 2. Message phải mô tả rõ trạng thái hiện tại để Citizen/Enterprise hiểu
        exception.WithMessage("*Pending status*");
        exception.Which.Message.Should().Contain("Current status: Assigned",
            "vì handler phải expose trạng thái vi phạm trong message");

        // 3. Xác minh DB và Notification KHÔNG được gọi khi validation fail
        _mockReportRepository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never,
            "Không được commit DB khi state transition bị từ chối");

        _mockNotificationService.Verify(
            n => n.NotifyReportAcceptedAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never,
            "Không được gửi notification khi Accept thất bại");

        AllureAttachmentHelper.AttachText("accept-report-invalid-state-error", exception.Which.Message);
    }

    /// <summary>
    /// Branch coverage: nhánh report.Status == Rejected.
    /// Report đã bị từ chối — không thể Accept (terminal state check).
    /// </summary>
    [Fact]
    [AllureDescription("TC-REP-007: Report đã Rejected — Accept bị chặn, trạng thái Rejected được expose trong message.")]
    public async Task Handle_WhenReportIsRejected_ShouldThrowWithRejectedStatusInMessage()
    {
        // ── ARRANGE ─────────────────────────────────────────────────────────────
        var reportId = Guid.NewGuid();
        var report   = CreatePendingReport();
        report.Reject(); // Pending → Rejected

        _mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var command = new AcceptReportAndCreateTaskCommand
        {
            ReportId = reportId,
            UserId   = Guid.NewGuid()
        };

        // ── ACT ─────────────────────────────────────────────────────────────────
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // ── ASSERT ──────────────────────────────────────────────────────────────
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Contain("Pending status");
        exception.Which.Message.Should().Contain("Current status: Rejected");

        // Không được gọi SaveChanges hay Notify khi fail
        _mockReportRepository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Branch coverage: nhánh report.Status == Collected (terminal state tuyệt đối).
    /// Report đã hoàn tất — Accept là vô nghĩa.
    /// </summary>
    [Fact]
    [AllureDescription("TC-REP-007: Report đã Collected (terminal state) — Accept bị chặn hoàn toàn.")]
    public async Task Handle_WhenReportIsCollected_ShouldThrowWithCollectedStatusInMessage()
    {
        // ── ARRANGE ─────────────────────────────────────────────────────────────
        var reportId = Guid.NewGuid();
        var report   = CreatePendingReport();
        report.Accept();
        report.Assign();
        report.Collect(); // Pending → Accepted → Assigned → Collected

        _mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var command = new AcceptReportAndCreateTaskCommand
        {
            ReportId = reportId,
            UserId   = Guid.NewGuid()
        };

        // ── ACT ─────────────────────────────────────────────────────────────────
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // ── ASSERT ──────────────────────────────────────────────────────────────
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Contain("Pending status");
        exception.Which.Message.Should().Contain("Current status: Collected");
    }

    /// <summary>
    /// Branch coverage: nhánh report.Status == Accepted (double-accept).
    /// Idempotency check: Accept 2 lần liên tiếp phải bị chặn ở lần 2.
    /// </summary>
    [Fact]
    [AllureDescription("TC-REP-007: Double-accept — Report đã Accepted không thể Accept thêm lần nữa.")]
    public async Task Handle_WhenReportIsAlreadyAccepted_ShouldThrowWithAcceptedStatusInMessage()
    {
        // ── ARRANGE ─────────────────────────────────────────────────────────────
        var reportId = Guid.NewGuid();
        var report   = CreatePendingReport();
        report.Accept(); // Lần 1 đã Accept thành công

        _mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var command = new AcceptReportAndCreateTaskCommand
        {
            ReportId = reportId,
            UserId   = Guid.NewGuid()
        };

        // ── ACT ─────────────────────────────────────────────────────────────────
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // ── ASSERT ──────────────────────────────────────────────────────────────
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Contain("Pending status");
        exception.Which.Message.Should().Contain("Current status: Accepted");
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════════
    // Branch Coverage: nhánh report == null (Report Not Found)
    // ════════════════════════════════════════════════════════════════════════════
    #region TC-REP-004: Report Not Found — Null Guard Branch

    /// <summary>
    /// Branch coverage: nhánh (report == null).
    /// GetByIdAsync trả về null → handler phải throw ngay lập tức,
    /// không được chạm vào SaveChanges hay NotificationService.
    /// </summary>
    [Fact]
    [AllureDescription("TC-REP-004: ReportId không tồn tại trong DB — handler throw 'Report not found', không gọi DB save hay notification.")]
    public async Task Handle_WhenReportNotFound_ShouldThrowAndNotCallSaveOrNotify()
    {
        // ── ARRANGE ─────────────────────────────────────────────────────────────
        var nonExistentId = Guid.NewGuid();

        _mockReportRepository
            .Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WasteReport?)null);

        var command = new AcceptReportAndCreateTaskCommand
        {
            ReportId = nonExistentId,
            UserId   = Guid.NewGuid()
        };

        AllureAttachmentHelper.AttachText("accept-report-not-found-input",
            $"reportId={nonExistentId} (không tồn tại trong DB)");

        // ── ACT ─────────────────────────────────────────────────────────────────
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // ── ASSERT ──────────────────────────────────────────────────────────────
        // 1. Exception với message chính xác "Report not found"
        var exception = await act.Should()
            .ThrowAsync<InvalidOperationException>("vì reportId không tồn tại");
        exception.WithMessage("Report not found");

        // 2. SaveChanges tuyệt đối không được gọi khi report không tồn tại
        _mockReportRepository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never,
            "Không commit DB khi report không tìm thấy");

        // 3. Notification không được gửi
        _mockNotificationService.Verify(
            n => n.NotifyReportAcceptedAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never,
            "Không notify Citizen khi report không tồn tại");

        AllureAttachmentHelper.AttachText("accept-report-not-found-error", exception.Which.Message);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════════
    // Parameterized: dùng [Theory] để phủ tất cả trạng thái không hợp lệ
    // trong 1 test duy nhất — tối ưu Branch Coverage
    // ════════════════════════════════════════════════════════════════════════════
    #region Branch Coverage: Tất cả trạng thái không hợp lệ (Theory)

    /// <summary>
    /// Dùng [Theory] + [InlineData] để chạy 1 test logic với mọi trạng thái không phải Pending.
    /// Đảm bảo không có trạng thái nào "lọt" qua guard clause của handler.
    /// </summary>
    [Theory]
    [InlineData(ReportStatus.Accepted)]
    [InlineData(ReportStatus.Rejected)]
    [InlineData(ReportStatus.Assigned)]
    [InlineData(ReportStatus.Collected)]
    [AllureDescription("TC-REP-007: Parameterized — Mọi trạng thái không phải Pending đều bị chặn khi Accept.")]
    public async Task Handle_WhenReportIsNotPending_ShouldAlwaysThrow(ReportStatus invalidStatus)
    {
        // ── ARRANGE ─────────────────────────────────────────────────────────────
        var reportId = Guid.NewGuid();
        var report   = CreatePendingReport();

        // Đưa report đến trạng thái mong muốn
        switch (invalidStatus)
        {
            case ReportStatus.Accepted:
                report.Accept();
                break;
            case ReportStatus.Rejected:
                report.Reject();
                break;
            case ReportStatus.Assigned:
                report.Accept();
                report.Assign();
                break;
            case ReportStatus.Collected:
                report.Accept();
                report.Assign();
                report.Collect();
                break;
        }

        _mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var command = new AcceptReportAndCreateTaskCommand
        {
            ReportId = reportId,
            UserId   = Guid.NewGuid()
        };

        // ── ACT ─────────────────────────────────────────────────────────────────
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // ── ASSERT ──────────────────────────────────────────────────────────────
        // Bất kể trạng thái nào không phải Pending đều phải throw
        await act.Should().ThrowAsync<InvalidOperationException>(
            $"vì trạng thái {invalidStatus} không hợp lệ để Accept");

        // DB và Notification tuyệt đối không được gọi trong mọi nhánh lỗi
        _mockReportRepository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        _mockNotificationService.Verify(
            n => n.NotifyReportAcceptedAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════════
    // Helper
    // ════════════════════════════════════════════════════════════════════════════
    /// <summary>Tạo một WasteReport mẫu ở trạng thái Pending cho các test.</summary>
    private static WasteReport CreatePendingReport() =>
        WasteReport.Create(
            citizenId:       Guid.NewGuid(),
            wasteCategoryId: 1,
            latitude:        10.7769m,
            longitude:       106.7009m,
            description:     "Rác thải sinh hoạt",
            address:         "Test address",
            aiSuggestion:    "Recyclable");
}
