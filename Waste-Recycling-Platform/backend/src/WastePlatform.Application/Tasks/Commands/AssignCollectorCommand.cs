using MediatR;

namespace WastePlatform.Application.Tasks.Commands;

public class AssignCollectorCommand : IRequest<AssignCollectorCommandResult>
{
    public Guid TaskId { get; set; }
    public Guid CollectorId { get; set; }
    public Guid EnterpriseId { get; set; }
}

public class AssignCollectorCommandResult
{
    public Guid TaskId { get; set; }
    public Guid CollectorId { get; set; }
    public DateTime AssignedAt { get; set; }
}
