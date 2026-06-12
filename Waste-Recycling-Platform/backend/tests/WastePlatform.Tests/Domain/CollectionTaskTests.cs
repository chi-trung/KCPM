using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Domain;

[AllureEpic("Domain Model")]
[AllureFeature("Collection Task Entity")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Status transitions and assignment lifecycle")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Domain")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "CollectionTaskTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Domain")]
[AllureOwner("Thanh Duy")]
[AllureSeverity(SeverityLevel.minor)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("domain")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-18")]
public class CollectionTaskTests
{
    [Fact]
    [AllureDescription("Creates a collection task with assigned status and preserves the linked report and enterprise ids.")]
    public void Create_ShouldInitializeTaskWithAssignedStatusAndIds()
    {
        var reportId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();

        AllureAttachmentHelper.AttachJson("collection-task-create-input", new { reportId, enterpriseId });

        var startedAt = DateTime.UtcNow;
        var task = CollectionTask.Create(reportId, enterpriseId);

        task.ReportId.Should().Be(reportId);
        task.EnterpriseId.Should().Be(enterpriseId);
        task.CollectorId.Should().BeNull();
        task.Status.Should().Be(CollectionTaskStatus.Assigned);
        task.AssignedAt.Should().BeOnOrAfter(startedAt);
        task.CompletedAt.Should().BeNull();
        task.StatusLogs.Should().BeEmpty();

        AllureAttachmentHelper.AttachJson("collection-task-create-result", new
        {
            task.Id,
            task.ReportId,
            task.EnterpriseId,
            task.Status,
            task.AssignedAt,
            task.CompletedAt,
            statusLogCount = task.StatusLogs.Count
        });
    }

    [Fact]
    [AllureDescription("Stores the collector id when a task is assigned to a collector.")]
    public void AssignCollector_ShouldStoreCollectorId()
    {
        var reportId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();
        var task = CollectionTask.Create(reportId, enterpriseId);
        var collectorId = Guid.NewGuid();

        AllureAttachmentHelper.AttachJson("collection-task-assign-input", new { task.Id, reportId, enterpriseId, collectorId });

        task.AssignCollector(collectorId);

        task.CollectorId.Should().Be(collectorId);

        AllureAttachmentHelper.AttachJson("collection-task-assign-result", new { task.Id, task.CollectorId, task.Status });
    }

    [Fact]
    [AllureDescription("Moves an assigned task to OnTheWay and records a status log entry.")]
    public void SetOnTheWay_WhenAssigned_ShouldChangeStatusAndCreateLog()
    {
        var task = CollectionTask.Create(Guid.NewGuid(), Guid.NewGuid());

        AllureAttachmentHelper.AttachText("collection-task-on-the-way-start", $"taskId={task.Id}\ninitialStatus={task.Status}");

        task.SetOnTheWay();

        task.Status.Should().Be(CollectionTaskStatus.OnTheWay);
        task.StatusLogs.Should().ContainSingle();
        task.StatusLogs.Single().Status.Should().Be(CollectionTaskStatus.OnTheWay);
        task.StatusLogs.Single().TaskId.Should().Be(task.Id);

        AllureAttachmentHelper.AttachJson("collection-task-on-the-way-result", new
        {
            task.Id,
            task.Status,
            statusLogCount = task.StatusLogs.Count,
            lastStatus = task.StatusLogs.Last().Status
        });
    }

    [Fact]
    [AllureDescription("Rejects a second OnTheWay transition when the task is no longer Assigned.")]
    public void SetOnTheWay_WhenNotAssigned_ShouldThrow()
    {
        var task = CollectionTask.Create(Guid.NewGuid(), Guid.NewGuid());
        task.SetOnTheWay();

        AllureAttachmentHelper.AttachText("collection-task-on-the-way-invalid-start", $"taskId={task.Id}\nstatusBeforeRetry={task.Status}");

        var act = () => task.SetOnTheWay();

        var exception = act.Should().Throw<InvalidOperationException>()
            .WithMessage("Task must be Assigned before going OnTheWay");

        AllureAttachmentHelper.AttachText("collection-task-on-the-way-invalid-error", exception.Which.Message);
    }

    [Fact]
    [AllureDescription("Completes a task that is already on the way and persists the collected details.")]
    public void Complete_WhenOnTheWay_ShouldMoveToCollectedAndPersistDetails()
    {
        var task = CollectionTask.Create(Guid.NewGuid(), Guid.NewGuid());
        task.SetOnTheWay();

        var startedAt = DateTime.UtcNow;

        AllureAttachmentHelper.AttachJson("collection-task-complete-input", new
        {
            task.Id,
            collectedWeightKg = 12.5m,
            notes = "Collected at front gate"
        });

        task.Complete(12.5m, "Collected at front gate");

        task.Status.Should().Be(CollectionTaskStatus.Collected);
        task.CollectedWeightKg.Should().Be(12.5m);
        task.Notes.Should().Be("Collected at front gate");
        task.CompletedAt.Should().NotBeNull();
        task.CompletedAt.Should().BeOnOrAfter(startedAt);
        task.StatusLogs.Should().HaveCount(2);
        task.StatusLogs.Last().Status.Should().Be(CollectionTaskStatus.Collected);

        AllureAttachmentHelper.AttachJson("collection-task-complete-result", new
        {
            task.Id,
            task.Status,
            task.CollectedWeightKg,
            task.Notes,
            task.CompletedAt,
            statusLogCount = task.StatusLogs.Count
        });
    }

    [Fact]
    [AllureDescription("Rejects completion when the task has not reached OnTheWay yet.")]
    public void Complete_WhenNotOnTheWay_ShouldThrow()
    {
        var task = CollectionTask.Create(Guid.NewGuid(), Guid.NewGuid());

        AllureAttachmentHelper.AttachText("collection-task-complete-invalid-start", $"taskId={task.Id}\nstatusBeforeComplete={task.Status}");

        var act = () => task.Complete(10m, "Invalid transition");

        var exception = act.Should().Throw<InvalidOperationException>()
            .WithMessage("Task must be OnTheWay before Collected");

        AllureAttachmentHelper.AttachText("collection-task-complete-invalid-error", exception.Which.Message);
    }
}
