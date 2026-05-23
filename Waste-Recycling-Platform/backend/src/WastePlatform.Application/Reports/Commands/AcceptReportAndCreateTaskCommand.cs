using MediatR;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Application.Reports.Commands;

public class AcceptReportAndCreateTaskCommand : IRequest<AcceptReportAndCreateTaskResult>
{
    public Guid ReportId { get; set; }
    public Guid UserId { get; set; }
}

public class AcceptReportAndCreateTaskResult
{
    public Guid ReportId { get; set; }
    public Guid CollectionTaskId { get; set; }
    public ReportStatus ReportStatus { get; set; }
    public string Message { get; set; } = string.Empty;
}
