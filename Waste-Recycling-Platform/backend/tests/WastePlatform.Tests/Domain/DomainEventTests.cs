using WastePlatform.Domain.Common;
using WastePlatform.Domain.Events;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Domain;

[AllureEpic("Domain Model")]
[AllureFeature("Domain Events")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Domain events: construction, properties, EventType")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Domain")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "DomainEventTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Domain")]
[AllureOwner("Nguyễn Chí Trung")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("domain")]
[Allure.Net.Commons.Attributes.AllureTag("event")]
public class DomainEventTests
{
    // ── ReportCreatedEvent ──────────────────────────────────────────────────

    [Fact]
    [AllureDescription("ReportCreatedEvent stores all properties correctly.")]
    public void ReportCreatedEvent_ShouldStoreProperties()
    {
        var reportId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var occurredOn = DateTime.UtcNow;

        var evt = new ReportCreatedEvent(reportId, citizenId, 1, occurredOn);

        Assert.Equal(reportId, evt.ReportId);
        Assert.Equal(citizenId, evt.CitizenId);
        Assert.Equal(1, evt.WasteCategoryId);
        Assert.Equal(occurredOn, evt.OccurredOn);
    }

    [Fact]
    [AllureDescription("ReportCreatedEvent.EventType returns 'ReportCreated'.")]
    public void ReportCreatedEvent_EventType_ShouldBeCorrect()
    {
        var evt = new ReportCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow);

        Assert.Equal("ReportCreated", evt.EventType);
    }

    [Fact]
    [AllureDescription("ReportCreatedEvent with null WasteCategoryId is valid.")]
    public void ReportCreatedEvent_NullCategory_ShouldBeValid()
    {
        var evt = new ReportCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow);

        Assert.Null(evt.WasteCategoryId);
    }

    [Fact]
    [AllureDescription("ReportCreatedEvent implements IDomainEvent interface.")]
    public void ReportCreatedEvent_ShouldImplementIDomainEvent()
    {
        var evt = new ReportCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), 1, DateTime.UtcNow);

        Assert.IsAssignableFrom<IDomainEvent>(evt);
    }

    // ── ReportStatusChangedEvent ────────────────────────────────────────────

    [Fact]
    [AllureDescription("ReportStatusChangedEvent stores status transition correctly.")]
    public void ReportStatusChangedEvent_ShouldStoreTransition()
    {
        var reportId = Guid.NewGuid();
        var occurredOn = DateTime.UtcNow;

        var evt = new ReportStatusChangedEvent(
            reportId, ReportStatus.Pending, ReportStatus.Accepted, occurredOn);

        Assert.Equal(reportId, evt.ReportId);
        Assert.Equal(ReportStatus.Pending, evt.OldStatus);
        Assert.Equal(ReportStatus.Accepted, evt.NewStatus);
        Assert.Equal(occurredOn, evt.OccurredOn);
    }

    [Fact]
    [AllureDescription("ReportStatusChangedEvent.EventType returns 'ReportStatusChanged'.")]
    public void ReportStatusChangedEvent_EventType_ShouldBeCorrect()
    {
        var evt = new ReportStatusChangedEvent(
            Guid.NewGuid(), ReportStatus.Pending, ReportStatus.Rejected, DateTime.UtcNow);

        Assert.Equal("ReportStatusChanged", evt.EventType);
    }

    [Theory]
    [InlineData(ReportStatus.Pending, ReportStatus.Accepted)]
    [InlineData(ReportStatus.Accepted, ReportStatus.Assigned)]
    [InlineData(ReportStatus.Assigned, ReportStatus.Collected)]
    [InlineData(ReportStatus.Pending, ReportStatus.Rejected)]
    [AllureDescription("ReportStatusChangedEvent supports all valid status transitions.")]
    public void ReportStatusChangedEvent_AllTransitions_ShouldBeValid(
        ReportStatus oldStatus, ReportStatus newStatus)
    {
        var evt = new ReportStatusChangedEvent(
            Guid.NewGuid(), oldStatus, newStatus, DateTime.UtcNow);

        Assert.Equal(oldStatus, evt.OldStatus);
        Assert.Equal(newStatus, evt.NewStatus);
    }

    // ── TaskStatusChangedEvent ──────────────────────────────────────────────

    [Fact]
    [AllureDescription("TaskStatusChangedEvent stores all properties including CollectorId.")]
    public void TaskStatusChangedEvent_ShouldStoreAllProperties()
    {
        var taskId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var collectorId = Guid.NewGuid();
        var occurredOn = DateTime.UtcNow;

        var evt = new TaskStatusChangedEvent(
            taskId, reportId,
            CollectionTaskStatus.Assigned, CollectionTaskStatus.OnTheWay,
            collectorId, occurredOn);

        Assert.Equal(taskId, evt.TaskId);
        Assert.Equal(reportId, evt.ReportId);
        Assert.Equal(CollectionTaskStatus.Assigned, evt.OldStatus);
        Assert.Equal(CollectionTaskStatus.OnTheWay, evt.NewStatus);
        Assert.Equal(collectorId, evt.CollectorId);
        Assert.Equal(occurredOn, evt.OccurredOn);
    }

    [Fact]
    [AllureDescription("TaskStatusChangedEvent.EventType returns 'TaskStatusChanged'.")]
    public void TaskStatusChangedEvent_EventType_ShouldBeCorrect()
    {
        var evt = new TaskStatusChangedEvent(
            Guid.NewGuid(), Guid.NewGuid(),
            CollectionTaskStatus.OnTheWay, CollectionTaskStatus.Collected,
            Guid.NewGuid(), DateTime.UtcNow);

        Assert.Equal("TaskStatusChanged", evt.EventType);
    }

    [Fact]
    [AllureDescription("TaskStatusChangedEvent with null CollectorId is valid.")]
    public void TaskStatusChangedEvent_NullCollector_ShouldBeValid()
    {
        var evt = new TaskStatusChangedEvent(
            Guid.NewGuid(), Guid.NewGuid(),
            CollectionTaskStatus.Assigned, CollectionTaskStatus.OnTheWay,
            null, DateTime.UtcNow);

        Assert.Null(evt.CollectorId);
    }

    [Fact]
    [AllureDescription("TaskStatusChangedEvent implements IDomainEvent interface.")]
    public void TaskStatusChangedEvent_ShouldImplementIDomainEvent()
    {
        var evt = new TaskStatusChangedEvent(
            Guid.NewGuid(), Guid.NewGuid(),
            CollectionTaskStatus.Assigned, CollectionTaskStatus.Collected,
            null, DateTime.UtcNow);

        Assert.IsAssignableFrom<IDomainEvent>(evt);
    }

    // ── ComplaintCreatedEvent ────────────────────────────────────────────────

    [Fact]
    [AllureDescription("ComplaintCreatedEvent stores all properties correctly.")]
    public void ComplaintCreatedEvent_ShouldStoreProperties()
    {
        var complaintId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var enterpriseId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var occurredOn = DateTime.UtcNow;

        var evt = new ComplaintCreatedEvent(
            complaintId, citizenId, enterpriseId, reportId, occurredOn);

        Assert.Equal(complaintId, evt.ComplaintId);
        Assert.Equal(citizenId, evt.CitizenId);
        Assert.Equal(enterpriseId, evt.EnterpriseId);
        Assert.Equal(reportId, evt.ReportId);
        Assert.Equal(occurredOn, evt.OccurredOn);
    }

    [Fact]
    [AllureDescription("ComplaintCreatedEvent.EventType returns 'ComplaintCreated'.")]
    public void ComplaintCreatedEvent_EventType_ShouldBeCorrect()
    {
        var evt = new ComplaintCreatedEvent(
            Guid.NewGuid(), Guid.NewGuid(), null, null, DateTime.UtcNow);

        Assert.Equal("ComplaintCreated", evt.EventType);
    }

    [Fact]
    [AllureDescription("ComplaintCreatedEvent with null optional fields is valid.")]
    public void ComplaintCreatedEvent_NullOptionals_ShouldBeValid()
    {
        var evt = new ComplaintCreatedEvent(
            Guid.NewGuid(), Guid.NewGuid(), null, null, DateTime.UtcNow);

        Assert.Null(evt.EnterpriseId);
        Assert.Null(evt.ReportId);
    }

    [Fact]
    [AllureDescription("ComplaintCreatedEvent implements IDomainEvent interface.")]
    public void ComplaintCreatedEvent_ShouldImplementIDomainEvent()
    {
        var evt = new ComplaintCreatedEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

        Assert.IsAssignableFrom<IDomainEvent>(evt);
    }

    // ── Record equality ─────────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Two identical ReportCreatedEvents are equal (record equality).")]
    public void ReportCreatedEvent_SameProperties_ShouldBeEqual()
    {
        var reportId = Guid.NewGuid();
        var citizenId = Guid.NewGuid();
        var time = DateTime.UtcNow;

        var evt1 = new ReportCreatedEvent(reportId, citizenId, 1, time);
        var evt2 = new ReportCreatedEvent(reportId, citizenId, 1, time);

        Assert.Equal(evt1, evt2);
    }

    [Fact]
    [AllureDescription("Two ReportCreatedEvents with different data are not equal.")]
    public void ReportCreatedEvent_DifferentProperties_ShouldNotBeEqual()
    {
        var time = DateTime.UtcNow;

        var evt1 = new ReportCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), 1, time);
        var evt2 = new ReportCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), 2, time);

        Assert.NotEqual(evt1, evt2);
    }
}
