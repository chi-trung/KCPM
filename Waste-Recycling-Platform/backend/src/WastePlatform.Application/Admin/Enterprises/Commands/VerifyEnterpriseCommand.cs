using MediatR;

namespace WastePlatform.Application.Admin.Enterprises.Commands;

public class VerifyEnterpriseCommand : IRequest<VerifyEnterpriseResult>
{
    public Guid EnterpriseId { get; set; }
}

public class VerifyEnterpriseResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid EnterpriseId { get; set; }
}
