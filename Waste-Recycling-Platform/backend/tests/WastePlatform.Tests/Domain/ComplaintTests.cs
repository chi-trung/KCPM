using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Domain;

[AllureEpic("Domain Model")]
[AllureFeature("Complaint Entity")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Complaint lifecycle: create, assign, resolve, reject, escalate")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Domain")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "ComplaintTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Domain")]
[AllureOwner("Chi Trung")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("domain")]
public class ComplaintTests
{
    [Fact]
    [AllureDescription("Creates a complaint with required fields and default status Open.")]
    public void Create_ShouldInitializeComplaintWithOpenStatus()
    {
        var citizenId = Guid.NewGuid();
        var complaint = Complaint.Create(citizenId, "Rác thải không được thu gom");

        AllureAttachmentHelper.AttachJson("complaint-create", new
        {
            complaint.Id, complaint.CitizenId, complaint.Content, complaint.Status
        });

        complaint.Id.Should().NotBe(Guid.Empty);
        complaint.CitizenId.Should().Be(citizenId);
        complaint.Content.Should().Be("Rác thải không được thu gom");
        complaint.Status.Should().Be(ComplaintStatus.Open);
        complaint.ReportId.Should().BeNull();
        complaint.EnterpriseId.Should().BeNull();
    }

    [Fact]
    [AllureDescription("Creates a complaint linked to a report and enterprise.")]
    public void Create_WithReportAndEnterprise_ShouldSetOptionalIds()
    {
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();

        var complaint = Complaint.Create(citizenId, "Chậm xử lý", reportId, enterpriseId);

        complaint.ReportId.Should().Be(reportId);
        complaint.EnterpriseId.Should().Be(enterpriseId);
    }

    [Fact]
    [AllureDescription("Assigns a collector to the complaint and changes status to InProgress.")]
    public void AssignCollector_ShouldSetCollectorIdAndChangeStatusToInProgress()
    {
        var complaint = CreateComplaint();
        var collectorId = Guid.NewGuid();

        complaint.AssignCollector(collectorId);

        AllureAttachmentHelper.AttachJson("complaint-assign", new
        {
            complaint.CollectorId, complaint.Status
        });

        complaint.CollectorId.Should().Be(collectorId);
        complaint.Status.Should().Be(ComplaintStatus.InProgress);
    }

    [Fact]
    [AllureDescription("Resolves a complaint with admin response.")]
    public void Resolve_ShouldSetResolvedStatusAndAdminResponse()
    {
        var complaint = CreateComplaint();

        complaint.Resolve("Đã xử lý xong, vui lòng kiểm tra lại.");

        AllureAttachmentHelper.AttachJson("complaint-resolve", new
        {
            complaint.Status, complaint.AdminResponse, complaint.ResolvedAt
        });

        complaint.Status.Should().Be(ComplaintStatus.Resolved);
        complaint.AdminResponse.Should().Be("Đã xử lý xong, vui lòng kiểm tra lại.");
        complaint.ResolvedAt.Should().NotBeNull();
        complaint.ResolvedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [AllureDescription("Rejects a complaint with admin response.")]
    public void Reject_ShouldSetRejectedStatusAndAdminResponse()
    {
        var complaint = CreateComplaint();

        complaint.Reject("Không đủ bằng chứng");

        AllureAttachmentHelper.AttachJson("complaint-reject", new
        {
            complaint.Status, complaint.AdminResponse, complaint.ResolvedAt
        });

        complaint.Status.Should().Be(ComplaintStatus.Rejected);
        complaint.AdminResponse.Should().Be("Không đủ bằng chứng");
        complaint.ResolvedAt.Should().NotBeNull();
    }

    [Fact]
    [AllureDescription("Adds enterprise response and changes status to InProgress.")]
    public void AddEnterpriseResponse_ShouldSetResponseAndUpdateStatus()
    {
        var complaint = CreateComplaint();

        complaint.AddEnterpriseResponse("Chúng tôi đang xử lý vấn đề này.");

        AllureAttachmentHelper.AttachJson("complaint-enterprise-response", new
        {
            complaint.EnterpriseResponse, complaint.EnterpriseRespondedAt, complaint.Status, complaint.UpdatedAt
        });

        complaint.EnterpriseResponse.Should().Be("Chúng tôi đang xử lý vấn đề này.");
        complaint.EnterpriseRespondedAt.Should().NotBeNull();
        complaint.Status.Should().Be(ComplaintStatus.InProgress);
        complaint.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    [AllureDescription("Enterprise resolves a complaint directly.")]
    public void ResolveByEnterprise_ShouldSetResolvedStatusAndTimestamps()
    {
        var complaint = CreateComplaint();

        complaint.ResolveByEnterprise("Vấn đề đã được khắc phục.");

        AllureAttachmentHelper.AttachJson("complaint-enterprise-resolve", new
        {
            complaint.Status, complaint.EnterpriseResponse, complaint.ResolvedAt, complaint.UpdatedAt
        });

        complaint.Status.Should().Be(ComplaintStatus.Resolved);
        complaint.EnterpriseResponse.Should().Be("Vấn đề đã được khắc phục.");
        complaint.EnterpriseRespondedAt.Should().NotBeNull();
        complaint.ResolvedAt.Should().NotBeNull();
        complaint.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    [AllureDescription("Enterprise resolves without providing a response text.")]
    public void ResolveByEnterprise_WithNullResponse_ShouldStillResolve()
    {
        var complaint = CreateComplaint();

        complaint.ResolveByEnterprise(null);

        complaint.Status.Should().Be(ComplaintStatus.Resolved);
        complaint.ResolvedAt.Should().NotBeNull();
    }

    [Fact]
    [AllureDescription("Citizen escalates complaint to admin with reason.")]
    public void EscalateToAdmin_WithReason_ShouldSetEscalatedStatusAndReason()
    {
        var complaint = CreateComplaint();

        complaint.EscalateToAdmin("Doanh nghiệp không phản hồi sau 3 ngày");

        AllureAttachmentHelper.AttachJson("complaint-escalate", new
        {
            complaint.Status, complaint.EscalationReason, complaint.UpdatedAt
        });

        complaint.Status.Should().Be(ComplaintStatus.Escalated);
        complaint.EscalationReason.Should().Be("Doanh nghiệp không phản hồi sau 3 ngày");
        complaint.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    [AllureDescription("Citizen escalates without providing a reason.")]
    public void EscalateToAdmin_WithoutReason_ShouldSetEscalatedStatusOnly()
    {
        var complaint = CreateComplaint();

        complaint.EscalateToAdmin();

        complaint.Status.Should().Be(ComplaintStatus.Escalated);
        complaint.EscalationReason.Should().BeNull();
    }

    [Fact]
    [AllureDescription("Complaint full lifecycle: Open → InProgress → Resolved")]
    public void FullLifecycle_OpenToInProgressToResolved()
    {
        var complaint = CreateComplaint();
        complaint.Status.Should().Be(ComplaintStatus.Open);

        complaint.AssignCollector(Guid.NewGuid());
        complaint.Status.Should().Be(ComplaintStatus.InProgress);

        complaint.Resolve("Completed successfully");
        complaint.Status.Should().Be(ComplaintStatus.Resolved);

        AllureAttachmentHelper.AttachText("complaint-lifecycle", 
            "Open → InProgress → Resolved ✅");
    }

    [Fact]
    [AllureDescription("Complaint escalation lifecycle: Open → Enterprise Response → Escalated → Admin Resolved")]
    public void FullLifecycle_WithEscalation()
    {
        var complaint = CreateComplaint();

        complaint.AddEnterpriseResponse("We're looking into it");
        complaint.Status.Should().Be(ComplaintStatus.InProgress);

        complaint.EscalateToAdmin("Not satisfied with enterprise response");
        complaint.Status.Should().Be(ComplaintStatus.Escalated);

        complaint.Resolve("Admin has resolved the issue");
        complaint.Status.Should().Be(ComplaintStatus.Resolved);

        AllureAttachmentHelper.AttachText("complaint-escalation-lifecycle",
            "Open → InProgress → Escalated → Resolved ✅");
    }

    private static Complaint CreateComplaint()
    {
        return Complaint.Create(Guid.NewGuid(), "Test complaint content");
    }
}
