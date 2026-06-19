using WastePlatform.Domain.Common;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Domain.Events;

/// <summary>
/// Domain event raised when a collection task transitions between statuses.
/// </summary>
/// <param name="TaskId">The unique identifier of the collection task.</param>
/// <param name="ReportId">The associated waste report ID.</param>
/// <param name="OldStatus">The previous task status before the transition.</param>
/// <param name="NewStatus">The new task status after the transition.</param>
/// <param name="CollectorId">The collector assigned to the task, if any.</param>
/// <param name="OccurredOn">UTC timestamp when the event occurred.</param>
public sealed record TaskStatusChangedEvent(
    Guid TaskId,
    Guid ReportId,
    CollectionTaskStatus OldStatus,
    CollectionTaskStatus NewStatus,
    Guid? CollectorId,
    DateTime OccurredOn) : IDomainEvent
{
    /// <inheritdoc />
    public string EventType => "TaskStatusChanged";
}
