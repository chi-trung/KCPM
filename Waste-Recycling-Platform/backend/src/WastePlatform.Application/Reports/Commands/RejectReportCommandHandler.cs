using MediatR;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Application.Reports.Commands;

/// <summary>
/// Handler that validates and prepares data for report rejection.
/// The actual database persistence is handled in the controller via DbContext.
/// </summary>
public class RejectReportCommandHandler : IRequestHandler<RejectReportCommand, RejectReportResult>
{
    private readonly IReportRepository _reportRepository;

    public RejectReportCommandHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<RejectReportResult> Handle(RejectReportCommand request, CancellationToken cancellationToken)
    {
        // Validate that the report exists and is in valid state
        var report = await _reportRepository.GetByIdAsync(request.ReportId);
        if (report == null)
        {
            throw new InvalidOperationException("Report not found");
        }

        if (report.Status != ReportStatus.Pending)
        {
            throw new InvalidOperationException($"Report can only be rejected if it is in Pending status. Current status: {report.Status}");
        }

        // Return validation result - controller will handle actual persistence
        return new RejectReportResult
        {
            ReportId = request.ReportId,
            ReportStatus = ReportStatus.Rejected,
            Message = "Report validation successful. Ready for rejection."
        };
    }
}
