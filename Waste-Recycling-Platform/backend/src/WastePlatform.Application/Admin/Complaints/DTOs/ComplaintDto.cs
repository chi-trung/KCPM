using WastePlatform.Domain.Enums;

namespace WastePlatform.Application.Admin.Complaints.DTOs;

public class ComplaintDto
{
    public Guid Id { get; set; }
    public Guid CitizenId { get; set; }
    public string? CitizenName { get; set; }
    public Guid? ReportId { get; set; }
    public string? ReportAddress { get; set; }
    public string Content { get; set; } = null!;
    public ComplaintStatus Status { get; set; }
    public string? AdminResponse { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class ComplaintListDto
{
    public Guid Id { get; set; }
    public string? CitizenName { get; set; }
    public string Content { get; set; } = null!;
    public ComplaintStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool HasReport { get; set; }
    public string? EnterpriseResponse { get; set; }
    public DateTime? EnterpriseRespondedAt { get; set; }
    public string? EscalationReason { get; set; }  // Lý do citizen escalate lên admin
    public string? AdminResponse { get; set; }     // Phản hồi của admin
}

public class CreateOrUpdateComplaintResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid ComplaintId { get; set; }
}
