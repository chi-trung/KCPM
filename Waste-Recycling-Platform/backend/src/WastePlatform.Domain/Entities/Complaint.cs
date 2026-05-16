using WastePlatform.Domain.Enums;

namespace WastePlatform.Domain.Entities;

public class Complaint
{
    public Guid Id { get; private set; }
    public Guid CitizenId { get; private set; }
    public Guid? EnterpriseId { get; private set; }
    public Guid? ReportId { get; private set; }
    public Guid? CollectorId { get; private set; }
    public string Content { get; private set; } = null!;
    public ComplaintStatus Status { get; private set; } = ComplaintStatus.Open;
    public string? AdminResponse { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public User Citizen { get; private set; } = null!;
    public Enterprise? Enterprise { get; private set; }
    public WasteReport? WasteReport { get; private set; }

    protected Complaint() { }

    public static Complaint Create(Guid citizenId, string content, Guid? reportId = null, Guid? enterpriseId = null)
        => new() { Id = Guid.NewGuid(), CitizenId = citizenId, Content = content, ReportId = reportId, EnterpriseId = enterpriseId };

    public void AssignCollector(Guid collectorId)
    {
        CollectorId = collectorId;
        Status = ComplaintStatus.InProgress;
    }

    public void Resolve(string adminResponse)
    {
        Status = ComplaintStatus.Resolved;
        AdminResponse = adminResponse;
        ResolvedAt = DateTime.UtcNow;
    }

    public void Reject(string adminResponse)
    {
        Status = ComplaintStatus.Rejected;
        AdminResponse = adminResponse;
        ResolvedAt = DateTime.UtcNow;
    }

    public string? EnterpriseResponse { get; private set; }
    public DateTime? EnterpriseRespondedAt { get; private set; }
    public string? EscalationReason { get; private set; }  // Lý do citizen escalate lên admin

    public void AddEnterpriseResponse(string response)
    {
        EnterpriseResponse = response;
        EnterpriseRespondedAt = DateTime.UtcNow;
        Status = ComplaintStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResolveByEnterprise(string? response)
    {
        if (!string.IsNullOrEmpty(response))
            EnterpriseResponse = response;
        EnterpriseRespondedAt = DateTime.UtcNow;
        Status = ComplaintStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EscalateToAdmin(string? reason = null)
    {
        Status = ComplaintStatus.Escalated;
        if (!string.IsNullOrEmpty(reason))
        {
            EscalationReason = reason;
        }
        UpdatedAt = DateTime.UtcNow;
    }
}
