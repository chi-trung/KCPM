using MediatR;

namespace WastePlatform.Application.Admin.Enterprises.Commands;

public class RejectEnterpriseCommand : IRequest<RejectEnterpriseResult>
{
    public Guid EnterpriseId { get; set; }
    public string ReasonForRejection { get; set; } = null!;
}

public class RejectEnterpriseResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid EnterpriseId { get; set; }
}
