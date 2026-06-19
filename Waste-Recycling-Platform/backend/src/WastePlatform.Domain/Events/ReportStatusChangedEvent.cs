using WastePlatform.Domain.Common;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Domain.Events;

/// <summary>
/// Domain event raised when a waste report transitions between statuses.
/// </summary>
/// <param name="ReportId">The unique identifier of the report.</param>
/// <param name="OldStatus">The previous status before the transition.</param>
/// <param name="NewStatus">The new status after the transition.</param>
/// <param name="OccurredOn">UTC timestamp when the event occurred.</param>
public sealed record ReportStatusChangedEvent(
    Guid ReportId,
    ReportStatus OldStatus,
    ReportStatus NewStatus,
    DateTime OccurredOn) : IDomainEvent
{
    /// <inheritdoc />
    public string EventType => "ReportStatusChanged";
}
