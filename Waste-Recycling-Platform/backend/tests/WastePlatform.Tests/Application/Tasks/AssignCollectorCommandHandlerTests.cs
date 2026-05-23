using WastePlatform.Application.Tasks.Commands;

namespace WastePlatform.Tests.Application.Tasks;

public class AssignCollectorCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnCommandDataAndUtcAssignmentTime()
    {
        var handler = new AssignCollectorCommandHandler();
        var request = new AssignCollectorCommand
        {
            TaskId = Guid.NewGuid(),
            CollectorId = Guid.NewGuid(),
            EnterpriseId = Guid.NewGuid()
        };

        var startedAt = DateTime.UtcNow;

        var result = await handler.Handle(request, CancellationToken.None);

        var finishedAt = DateTime.UtcNow;

        result.TaskId.Should().Be(request.TaskId);
        result.CollectorId.Should().Be(request.CollectorId);
        result.AssignedAt.Should().BeOnOrAfter(startedAt);
        result.AssignedAt.Should().BeOnOrBefore(finishedAt);
    }
}