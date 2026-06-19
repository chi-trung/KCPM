using MediatR;
using WastePlatform.Application.Common.DTOs;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Application.Complaints.Commands;

public class CreateComplaintCommand : IRequest<Guid>
{
    public Guid CitizenId { get; set; }
    public string Content { get; set; } = null!;
    public Guid? ReportId { get; set; }
    public Guid? EnterpriseId { get; set; }
}

public class CreateComplaintCommandHandler : IRequestHandler<CreateComplaintCommand, Guid>
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly IReportRepository _reportRepository;

    public CreateComplaintCommandHandler(IComplaintRepository complaintRepository, IReportRepository reportRepository)
    {
        _complaintRepository = complaintRepository;
        _reportRepository = reportRepository;
    }

    public async Task<Guid> Handle(CreateComplaintCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[DEBUG] CreateComplaintCommandHandler.Handle invoked, ReportId.HasValue={request.ReportId.HasValue}");
        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Complaint content cannot be empty", nameof(request));

        // Fix bug: CreateComplaintCommandHandler chưa validate content length > 2000
        // Fixed by: Nguyễn Hoàng Phụng (KIEM-7)
        if (request.Content.Length > 2000)
            throw new ArgumentException("Complaint content cannot exceed 2000 characters", nameof(request));

        // Debug: write a file so we know this handler is executed
        System.IO.File.WriteAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "CreateComplaintCommandHandlerDebug.txt"), "Handler invoked");

        // BR-05: Check if citizen already has a complaint for this report
        if (request.ReportId.HasValue)
        {
            var reportId = request.ReportId.Value;
            if (await _complaintRepository.ExistsByCitizenAndReportAsync(request.CitizenId, reportId, cancellationToken))
            {
                throw new InvalidOperationException("Citizen chỉ gửi được 1 khiếu nại cho 1 report.");
            }
        }

        // If EnterpriseId was not provided, try to infer it from the referenced report's collection task
        Guid? enterpriseId = request.EnterpriseId;
        if (!enterpriseId.HasValue && request.ReportId.HasValue)
        {
            var report = await _reportRepository.GetByIdAsync(request.ReportId.Value, cancellationToken);
            if (report == null)
                throw new ArgumentException("Report not found", nameof(request));

            // Only allow complaints for reports that have been accepted/assigned/collected
            if (report.Status == ReportStatus.Pending)
                throw new InvalidOperationException("Cannot file a complaint for a report that has not been accepted by an enterprise yet.");

            if (report.CollectionTask != null)
            {
                enterpriseId = report.CollectionTask.EnterpriseId;
            }
        }

        var complaint = Domain.Entities.Complaint.Create(
            request.CitizenId,
            request.Content,
            request.ReportId,
            enterpriseId);

        await _complaintRepository.AddAsync(complaint, cancellationToken);
        await _complaintRepository.SaveChangesAsync(cancellationToken);

        return complaint.Id;
    }
}
