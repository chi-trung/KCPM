using WastePlatform.Application.Tasks.Commands;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Application.Tasks;

[AllureEpic("Tasks")]
[AllureFeature("Task Command Handlers")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Assigning collectors to collection tasks")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "TaskCommandHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Tasks")]
[AllureOwner("Team")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("tasks")]
public class TaskCommandHandlerTests
{
    #region AssignCollectorCommandHandler

    [Fact]
    [AllureDescription("AssignCollector returns result with correct TaskId and CollectorId.")]
    public async Task AssignCollector_ShouldReturnResultWithCorrectIds()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var collectorId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();

        var handler = new AssignCollectorCommandHandler();
        var command = new AssignCollectorCommand
        {
            TaskId = taskId,
            CollectorId = collectorId,
            EnterpriseId = enterpriseId
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachText("assert-subject", "Asserting: result");
        result.Should().NotBeNull();
        result.TaskId.Should().Be(taskId);
        result.CollectorId.Should().Be(collectorId);
        result.AssignedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [AllureDescription("AssignCollector sets AssignedAt to approximately now.")]
    public async Task AssignCollector_ShouldSetAssignedAtToNow()
    {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds(-1);
        var handler = new AssignCollectorCommandHandler();
        var command = new AssignCollectorCommand
        {
            TaskId = Guid.NewGuid(),
            CollectorId = Guid.NewGuid(),
            EnterpriseId = Guid.NewGuid()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        var after = DateTime.UtcNow.AddSeconds(1);

        // Assert
        result.AssignedAt.Should().BeOnOrAfter(before);
        result.AssignedAt.Should().BeOnOrBefore(after);
    }

    #endregion
}

