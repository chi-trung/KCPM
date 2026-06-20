using FluentAssertions;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Moq;
using WastePlatform.Application.Complaints.Commands;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using Xunit;

namespace WastePlatform.Tests.Whitebox;

/// <summary>
/// Whitebox Testing — EnterpriseRespondToComplaintCommandHandler.Handle()
/// 
/// Kỹ thuật áp dụng theo Chương 4:
///   1. Control Flow Graph (CFG)     → 12 nodes, 12 edges
///   2. Cyclomatic Complexity V(G)   → V(G) = 6 (5 predicates + 1)
///   3. Independent Paths            → 6 paths (P1-P6)
///   4. Statement Coverage           → 100%
///   5. Branch/Decision Coverage     → 100% (10/10 branches)
///   6. Condition Coverage           → 100% (D3 compound: Status != Open AND Status != InProgress)
///   7. Branch-Condition Coverage    → 100%
///   8. Condition Combination        → Full truth table for D3
/// </summary>
[AllureEpic("Chương 4: Whitebox Testing")]
[AllureFeature("EnterpriseRespondToComplaint — CFG + Path Coverage")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Whitebox: Path + Condition Combination Coverage")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Whitebox")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "EnterpriseRespondWhiteboxTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Whitebox")]
[AllureOwner("Nguyễn Minh Phụng")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("whitebox")]
[Allure.Net.Commons.Attributes.AllureTag("cfg")]
[Allure.Net.Commons.Attributes.AllureTag("path-coverage")]
public class EnterpriseRespondWhiteboxTests
{
    private readonly Mock<IComplaintRepository> _mockComplaintRepo;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly EnterpriseRespondToComplaintCommandHandler _handler;

    private readonly Guid _enterpriseId = Guid.NewGuid();
    private readonly Guid _citizenId = Guid.NewGuid();

    public EnterpriseRespondWhiteboxTests()
    {
        _mockComplaintRepo = new Mock<IComplaintRepository>();
        _mockNotificationService = new Mock<INotificationService>();
        _handler = new EnterpriseRespondToComplaintCommandHandler(
            _mockComplaintRepo.Object,
            _mockNotificationService.Object);
    }

    private Complaint CreateComplaintWithStatus(ComplaintStatus targetStatus)
    {
        // Use factory method, then transition to desired status
        var complaint = Complaint.Create(_citizenId, "Test complaint content", null, _enterpriseId);
        
        // Transition complaint to target status via domain methods
        if (targetStatus == ComplaintStatus.InProgress)
            complaint.AssignCollector(Guid.NewGuid());
        else if (targetStatus == ComplaintStatus.Resolved)
        {
            complaint.AssignCollector(Guid.NewGuid());
            complaint.ResolveByEnterprise("Resolved");
        }
        else if (targetStatus == ComplaintStatus.Escalated)
            complaint.EscalateToAdmin("Escalated reason");
        else if (targetStatus == ComplaintStatus.Rejected)
            complaint.Reject("Rejected by admin");
        // ComplaintStatus.Open = default, no transition needed
        
        return complaint;
    }

    private EnterpriseRespondToComplaintCommand CreateCommand(
        Guid complaintId, bool escalate = false, bool resolve = false, Guid? enterpriseId = null)
    {
        return new EnterpriseRespondToComplaintCommand
        {
            EnterpriseId = enterpriseId ?? _enterpriseId,
            EnterpriseName = "Test Enterprise",
            ComplaintId = complaintId,
            Response = "Test response content",
            EscalateToAdmin = escalate,
            ResolveImmediately = resolve
        };
    }

    // ==========================================
    // PATH COVERAGE — 6 Independent Paths
    // V(G) = 6 = P + 1 = 5 + 1
    // ==========================================

    #region Path P1: complaint == null → return fail (Node: 1→2T→3)

    /// <summary>
    /// Path P1: 1 → 2(T) → 3
    /// D1=True: complaint not found
    /// </summary>
    [Fact]
    [AllureDescription(
        "Path P1: complaint == null → return {Success=false, 'Not found'}\n" +
        "CFG: Node 1 → Node 2(T) → Node 3\n" +
        "V(G) path 1/6")]
    public async Task Path1_ComplaintNull_ReturnsNotFound()
    {
        var complaintId = Guid.NewGuid();
        _mockComplaintRepo.Setup(r => r.GetByIdAsync(complaintId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Complaint?)null);

        var result = await _handler.Handle(CreateCommand(complaintId), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _mockComplaintRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Path P2: wrong enterprise → return fail (Node: 1→2F→4T→5)

    /// <summary>
    /// Path P2: 1 → 2(F) → 4(T) → 5
    /// D1=False, D2=True: enterprise mismatch
    /// </summary>
    [Fact]
    [AllureDescription(
        "Path P2: EnterpriseId mismatch → return {Success=false, 'Not authorized'}\n" +
        "CFG: Node 1 → 2(F) → 4(T) → 5\n" +
        "V(G) path 2/6")]
    public async Task Path2_WrongEnterprise_ReturnsNotAuthorized()
    {
        var complaint = CreateComplaintWithStatus(ComplaintStatus.Open);
        _mockComplaintRepo.Setup(r => r.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        var wrongEnterpriseId = Guid.NewGuid();
        var result = await _handler.Handle(
            CreateCommand(complaint.Id, enterpriseId: wrongEnterpriseId), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not authorized");
    }

    #endregion

    #region Path P3: invalid status → return fail (Node: 1→2F→4F→6T→7)

    /// <summary>
    /// Path P3: 1 → 2(F) → 4(F) → 6(T) → 7
    /// D1=F, D2=F, D3=True: status not Open/InProgress
    /// Condition Coverage D3: C1(!=Open)=T, C2(!=InProgress)=T → D3=True
    /// </summary>
    [Fact]
    [AllureDescription(
        "Path P3: Status=Resolved → return fail 'Cannot respond'\n" +
        "CFG: Node 1 → 2(F) → 4(F) → 6(T) → 7\n" +
        "V(G) path 3/6\n" +
        "Condition Coverage D3: C1=True(!=Open), C2=True(!=InProgress)")]
    public async Task Path3_ResolvedStatus_ReturnsCannotRespond()
    {
        var complaint = CreateComplaintWithStatus(ComplaintStatus.Resolved);
        _mockComplaintRepo.Setup(r => r.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        var result = await _handler.Handle(CreateCommand(complaint.Id), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot respond");
    }

    #endregion

    #region Path P4: escalate to admin (Node: 1→2F→4F→6F→8T→9)

    /// <summary>
    /// Path P4: 1 → 2(F) → 4(F) → 6(F) → 8(T) → 9
    /// D4=True: escalate complaint to admin
    /// </summary>
    [Fact]
    [AllureDescription(
        "Path P4: EscalateToAdmin=true → escalate + notify + return success\n" +
        "CFG: Node 1 → 2(F) → 4(F) → 6(F) → 8(T) → 9\n" +
        "V(G) path 4/6")]
    public async Task Path4_EscalateToAdmin_ReturnsEscalated()
    {
        var complaint = CreateComplaintWithStatus(ComplaintStatus.Open);
        _mockComplaintRepo.Setup(r => r.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        var result = await _handler.Handle(
            CreateCommand(complaint.Id, escalate: true), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("escalated");
        _mockComplaintRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockNotificationService.Verify(n => n.NotifyComplaintRepliedAsync(
            _citizenId, complaint.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Path P5: resolve immediately (Node: 1→2F→4F→6F→8F→10T→11)

    /// <summary>
    /// Path P5: 1 → 2(F) → 4(F) → 6(F) → 8(F) → 10(T) → 11
    /// D4=False, D5=True: resolve immediately
    /// </summary>
    [Fact]
    [AllureDescription(
        "Path P5: ResolveImmediately=true → resolve + notify + return success\n" +
        "CFG: Node 1 → 2(F) → 4(F) → 6(F) → 8(F) → 10(T) → 11\n" +
        "V(G) path 5/6")]
    public async Task Path5_ResolveImmediately_ReturnsResolved()
    {
        var complaint = CreateComplaintWithStatus(ComplaintStatus.InProgress);
        _mockComplaintRepo.Setup(r => r.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        var result = await _handler.Handle(
            CreateCommand(complaint.Id, resolve: true), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("resolved");
        _mockComplaintRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Path P6: just respond (Node: 1→2F→4F→6F→8F→10F→12)

    /// <summary>
    /// Path P6: 1 → 2(F) → 4(F) → 6(F) → 8(F) → 10(F) → 12
    /// D4=False, D5=False: just add response
    /// </summary>
    [Fact]
    [AllureDescription(
        "Path P6: No escalate, no resolve → add response + notify + return success\n" +
        "CFG: Node 1 → 2(F) → 4(F) → 6(F) → 8(F) → 10(F) → 12\n" +
        "V(G) path 6/6 — ALL PATHS COVERED")]
    public async Task Path6_JustRespond_ReturnsResponseAdded()
    {
        var complaint = CreateComplaintWithStatus(ComplaintStatus.Open);
        _mockComplaintRepo.Setup(r => r.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        var result = await _handler.Handle(CreateCommand(complaint.Id), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Response added");
        _mockComplaintRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockNotificationService.Verify(n => n.NotifyComplaintRepliedAsync(
            _citizenId, complaint.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    // ==========================================
    // CONDITION COMBINATION COVERAGE for D3
    // D3: (Status != Open && Status != InProgress)
    // ==========================================

    #region D3: Condition Combination — Status validation

    /// <summary>
    /// Condition Combination Coverage cho D3: (Status != Open && Status != InProgress)
    /// 
    /// | # | C1(!=Open) | C2(!=InProgress) | D3 (&&) | TC            |
    /// |---|------------|-------------------|---------|---------------|
    /// | 1 | F          | T                 | F       | Status=Open   |
    /// | 2 | T          | F                 | F       | Status=InProg |
    /// | 3 | T          | T                 | T       | Status=Resolved|
    /// | 4 | F          | F                 | F       | Impossible    |
    /// 
    /// → 3/3 feasible combinations covered = 100%
    /// </summary>
    [Fact]
    [AllureDescription(
        "Condition Combination #1: C1=F(Open), C2=T → D3=False (proceed)\n" +
        "Status=Open: C1(!=Open)=False → short-circuit → D3=False")]
    public async Task CondCombination_D3_StatusOpen_C1False()
    {
        var complaint = CreateComplaintWithStatus(ComplaintStatus.Open);
        _mockComplaintRepo.Setup(r => r.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        var result = await _handler.Handle(CreateCommand(complaint.Id), CancellationToken.None);
        result.Success.Should().BeTrue();
    }

    [Fact]
    [AllureDescription(
        "Condition Combination #2: C1=T(not Open), C2=F(InProgress) → D3=False\n" +
        "Status=InProgress: C1(!=Open)=True, C2(!=InProgress)=False → D3=False")]
    public async Task CondCombination_D3_StatusInProgress_C2False()
    {
        var complaint = CreateComplaintWithStatus(ComplaintStatus.InProgress);
        _mockComplaintRepo.Setup(r => r.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        var result = await _handler.Handle(CreateCommand(complaint.Id), CancellationToken.None);
        result.Success.Should().BeTrue();
    }

    [Fact]
    [AllureDescription(
        "Condition Combination #3: C1=T, C2=T → D3=True (reject)\n" +
        "Status=Escalated: C1(!=Open)=True, C2(!=InProgress)=True → D3=True")]
    public async Task CondCombination_D3_StatusEscalated_BothTrue()
    {
        var complaint = CreateComplaintWithStatus(ComplaintStatus.Escalated);
        _mockComplaintRepo.Setup(r => r.GetByIdAsync(complaint.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complaint);

        var result = await _handler.Handle(CreateCommand(complaint.Id), CancellationToken.None);
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot respond");
    }

    #endregion
}
