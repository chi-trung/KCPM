using WastePlatform.Application.Tasks.Commands;

namespace WastePlatform.Tests.Application.Tasks;

[AllureEpic("Enterprise Operations")]
[AllureFeature("Assign Collector Handler")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Assign a collector and stamp the assignment time")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "AssignCollectorCommandHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Tasks")]
[AllureOwner("Nguyễn Chí Trung")]
[AllureSeverity(SeverityLevel.minor)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("task")]
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