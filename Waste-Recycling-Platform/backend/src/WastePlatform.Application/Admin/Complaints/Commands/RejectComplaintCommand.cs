using MediatR;

namespace WastePlatform.Application.Admin.Complaints.Commands;

public class RejectComplaintCommand : IRequest<RejectComplaintResult>
{
    public Guid ComplaintId { get; set; }
    public string AdminResponse { get; set; } = null!;
}

public class RejectComplaintResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid ComplaintId { get; set; }
}
