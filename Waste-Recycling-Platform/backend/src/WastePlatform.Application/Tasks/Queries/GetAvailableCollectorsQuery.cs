using MediatR;

namespace WastePlatform.Application.Tasks.Queries;

public class GetAvailableCollectorsQuery : IRequest<List<CollectorSummaryDto>>
{
    public Guid EnterpriseId { get; set; }
}

public class CollectorSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TaskCount { get; set; }
}
