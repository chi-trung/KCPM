using MediatR;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Application.Reports.Commands;

public class RejectReportCommand : IRequest<RejectReportResult>
{
    public Guid ReportId { get; set; }
    public Guid UserId { get; set; }
    public string? RejectionReason { get; set; }
}

public class RejectReportResult
{
    public Guid ReportId { get; set; }
    public ReportStatus ReportStatus { get; set; }
    public string Message { get; set; } = string.Empty;
}
