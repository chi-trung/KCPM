using MediatR;

namespace WastePlatform.Application.Enterprise.Queries;

public class GetEnterpriseByUserIdQuery : IRequest<EnterpriseDto?>
{
    public Guid UserId { get; set; }
}

public class EnterpriseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}
