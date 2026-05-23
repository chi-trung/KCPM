using MediatR;

namespace WastePlatform.Application.Tasks.Commands;

public class AssignCollectorCommandHandler : IRequestHandler<AssignCollectorCommand, AssignCollectorCommandResult>
{
    public async Task<AssignCollectorCommandResult> Handle(AssignCollectorCommand request, CancellationToken cancellationToken)
    {
        // This handler would interact with the database through repositories
        // For now, the business logic is directly in the controller
        // This can be further refactored to use proper CQRS pattern
        
        return await Task.FromResult(new AssignCollectorCommandResult
        {
            TaskId = request.TaskId,
            CollectorId = request.CollectorId,
            AssignedAt = DateTime.UtcNow
        });
    }
}
