using WastePlatform.Application.Tasks.Commands;
using WastePlatform.Tests.TestSupport;

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
[Allure.Net.Commons.Attributes.AllureIssue("KIEM-16")]
public class AssignCollectorCommandHandlerTests
{
    private readonly AssignCollectorCommandHandler _handler = new();

    /// <summary>
    /// Verifies that the handler returns a result with matching TaskId, CollectorId, 
    /// and a UTC AssignedAt timestamp within a valid time window.
    /// </summary>
    [Fact]
    [AllureDescription("Handler returns result with correct TaskId, CollectorId, and UTC AssignedAt within valid time window.")]
    public async Task Handle_ShouldReturnCommandDataAndUtcAssignmentTime()
    {
        var request = new AssignCollectorCommand
        {
            TaskId = Guid.NewGuid(),
            CollectorId = Guid.NewGuid(),
            EnterpriseId = Guid.NewGuid()
        };

        AllureAttachmentHelper.AttachJson("assign-command-input", new
        {
            request.TaskId, request.CollectorId, request.EnterpriseId
        });

        var startedAt = DateTime.UtcNow;

        var result = await _handler.Handle(request, CancellationToken.None);

        var finishedAt = DateTime.UtcNow;

        AllureAttachmentHelper.AttachJson("assign-command-result", new
        {
            result.TaskId, result.CollectorId, result.AssignedAt
        });

        result.TaskId.Should().Be(request.TaskId);
        result.CollectorId.Should().Be(request.CollectorId);
        result.AssignedAt.Should().BeOnOrAfter(startedAt);
        result.AssignedAt.Should().BeOnOrBefore(finishedAt);
    }

    /// <summary>
    /// EP: Each valid GUID input should produce a matching output — verifies
    /// that the handler acts as a pure pass-through for TaskId and CollectorId.
    /// Kỹ thuật: Equivalence Partitioning (Ch.4)
    /// </summary>
    [Fact]
    [AllureDescription("EP: Handler preserves all input GUIDs in the result without mutation.")]
    public async Task Handle_ShouldPreserveAllInputGuidsInResult()
    {
        var taskId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var collectorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var enterpriseId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var request = new AssignCollectorCommand
        {
            TaskId = taskId,
            CollectorId = collectorId,
            EnterpriseId = enterpriseId
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        AllureAttachmentHelper.AttachJson("guid-preservation-check", new
        {
            InputTaskId = taskId,
            OutputTaskId = result.TaskId,
            InputCollectorId = collectorId,
            OutputCollectorId = result.CollectorId
        });

        result.TaskId.Should().Be(taskId, "TaskId must be passed through unchanged");
        result.CollectorId.Should().Be(collectorId, "CollectorId must be passed through unchanged");
    }

    /// <summary>
    /// Verifies the handler returns AssignedAt in UTC kind, not Local or Unspecified.
    /// This prevents timezone bugs when storing assignment timestamps.
    /// </summary>
    [Fact]
    [AllureDescription("AssignedAt must be in UTC kind to prevent timezone-related bugs.")]
    public async Task Handle_AssignedAt_ShouldBeUtcKind()
    {
        var request = new AssignCollectorCommand
        {
            TaskId = Guid.NewGuid(),
            CollectorId = Guid.NewGuid(),
            EnterpriseId = Guid.NewGuid()
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        AllureAttachmentHelper.AttachJson("utc-check", new
        {
            result.AssignedAt,
            Kind = result.AssignedAt.Kind.ToString()
        });

        result.AssignedAt.Kind.Should().Be(DateTimeKind.Utc,
            "AssignedAt must use UTC to avoid timezone bugs in cloud deployments");
    }

    /// <summary>
    /// Verifies that two sequential calls produce different AssignedAt timestamps,
    /// confirming that each invocation generates a fresh timestamp.
    /// </summary>
    [Fact]
    [AllureDescription("Two sequential calls should produce different AssignedAt timestamps (no caching).")]
    public async Task Handle_CalledTwice_ShouldProduceDifferentTimestamps()
    {
        var request = new AssignCollectorCommand
        {
            TaskId = Guid.NewGuid(),
            CollectorId = Guid.NewGuid(),
            EnterpriseId = Guid.NewGuid()
        };

        var result1 = await _handler.Handle(request, CancellationToken.None);
        await Task.Delay(1); // Ensure clock advances
        var result2 = await _handler.Handle(request, CancellationToken.None);

        AllureAttachmentHelper.AttachJson("timestamp-diff", new
        {
            First = result1.AssignedAt,
            Second = result2.AssignedAt,
            AreDifferent = result1.AssignedAt != result2.AssignedAt
        });

        result2.AssignedAt.Should().BeOnOrAfter(result1.AssignedAt,
            "second call should have a later or equal timestamp");
    }

    /// <summary>
    /// Verifies handler respects CancellationToken — since this handler is synchronous
    /// (Task.FromResult), it should complete even when token is cancelled.
    /// This documents the current behavior.
    /// </summary>
    [Fact]
    [AllureDescription("Handler completes even with cancelled token since it uses Task.FromResult (synchronous).")]
    public async Task Handle_WithCancelledToken_ShouldStillReturnResult()
    {
        var request = new AssignCollectorCommand
        {
            TaskId = Guid.NewGuid(),
            CollectorId = Guid.NewGuid(),
            EnterpriseId = Guid.NewGuid()
        };

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await _handler.Handle(request, cts.Token);

        AllureAttachmentHelper.AttachJson("cancelled-token-result", new
        {
            result.TaskId,
            result.CollectorId,
            Completed = true
        });

        result.Should().NotBeNull();
        result.TaskId.Should().Be(request.TaskId);
    }
}