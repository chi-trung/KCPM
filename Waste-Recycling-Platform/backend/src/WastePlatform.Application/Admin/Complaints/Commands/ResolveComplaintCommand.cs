using MediatR;

namespace WastePlatform.Application.Admin.Complaints.Commands;

public class ResolveComplaintCommand : IRequest<ResolveComplaintResult>
{
    public Guid ComplaintId { get; set; }
    public string AdminResponse { get; set; } = null!;
}

public class ResolveComplaintResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid ComplaintId { get; set; }
}
