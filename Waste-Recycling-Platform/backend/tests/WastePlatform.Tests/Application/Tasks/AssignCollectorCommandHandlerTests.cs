using WastePlatform.Application.Tasks.Commands;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;

namespace WastePlatform.Tests.Application.Tasks;

[AllureEpic("KIEM-16 Enterprise Task Module")]
[AllureFeature("WRP-BE-TESTS-013 Assign Collector Command")]
public class AssignCollectorCommandHandlerTests
{
    [AllureStory("Assign collector command returns timestamps")]
    [AllureSeverity(SeverityLevel.normal)]
    [AllureOwner("chi-trung")]
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