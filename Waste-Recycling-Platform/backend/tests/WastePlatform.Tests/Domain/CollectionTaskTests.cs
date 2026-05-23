using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Tests.Domain;

public class CollectionTaskTests
{
    [Fact]
    public void Create_ShouldInitializeTaskWithAssignedStatusAndIds()
    {
        var reportId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();

        var startedAt = DateTime.UtcNow;
        var task = CollectionTask.Create(reportId, enterpriseId);

        task.ReportId.Should().Be(reportId);
        task.EnterpriseId.Should().Be(enterpriseId);
        task.CollectorId.Should().BeNull();
        task.Status.Should().Be(CollectionTaskStatus.Assigned);
        task.AssignedAt.Should().BeOnOrAfter(startedAt);
        task.CompletedAt.Should().BeNull();
        task.StatusLogs.Should().BeEmpty();
    }

    [Fact]
    public void AssignCollector_ShouldStoreCollectorId()
    {
        var task = CollectionTask.Create(Guid.NewGuid(), Guid.NewGuid());
        var collectorId = Guid.NewGuid();

        task.AssignCollector(collectorId);

        task.CollectorId.Should().Be(collectorId);
    }

    [Fact]
    public void SetOnTheWay_WhenAssigned_ShouldChangeStatusAndCreateLog()
    {
        var task = CollectionTask.Create(Guid.NewGuid(), Guid.NewGuid());

        task.SetOnTheWay();

        task.Status.Should().Be(CollectionTaskStatus.OnTheWay);
        task.StatusLogs.Should().ContainSingle();
        task.StatusLogs.Single().Status.Should().Be(CollectionTaskStatus.OnTheWay);
        task.StatusLogs.Single().TaskId.Should().Be(task.Id);
    }

    [Fact]
    public void SetOnTheWay_WhenNotAssigned_ShouldThrow()
    {
        var task = CollectionTask.Create(Guid.NewGuid(), Guid.NewGuid());
        task.SetOnTheWay();

        var act = () => task.SetOnTheWay();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Task must be Assigned before going OnTheWay");
    }

    [Fact]
    public void Complete_WhenOnTheWay_ShouldMoveToCollectedAndPersistDetails()
    {
        var task = CollectionTask.Create(Guid.NewGuid(), Guid.NewGuid());
        task.SetOnTheWay();

        var startedAt = DateTime.UtcNow;

        task.Complete(12.5m, "Collected at front gate");

        task.Status.Should().Be(CollectionTaskStatus.Collected);
        task.CollectedWeightKg.Should().Be(12.5m);
        task.Notes.Should().Be("Collected at front gate");
        task.CompletedAt.Should().NotBeNull();
        task.CompletedAt.Should().BeOnOrAfter(startedAt);
        task.StatusLogs.Should().HaveCount(2);
        task.StatusLogs.Last().Status.Should().Be(CollectionTaskStatus.Collected);
    }

    [Fact]
    public void Complete_WhenNotOnTheWay_ShouldThrow()
    {
        var task = CollectionTask.Create(Guid.NewGuid(), Guid.NewGuid());

        var act = () => task.Complete(10m, "Invalid transition");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Task must be OnTheWay before Collected");
    }
}