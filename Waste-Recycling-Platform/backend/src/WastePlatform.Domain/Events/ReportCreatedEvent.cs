using WastePlatform.Domain.Common;

namespace WastePlatform.Domain.Events;

/// <summary>
/// Domain event raised when a new waste report is created by a citizen.
/// </summary>
/// <param name="ReportId">The unique identifier of the created report.</param>
/// <param name="CitizenId">The user ID of the citizen who filed the report.</param>
/// <param name="WasteCategoryId">The optional waste category ID, if specified.</param>
/// <param name="OccurredOn">UTC timestamp when the event occurred.</param>
public sealed record ReportCreatedEvent(
    Guid ReportId,
    Guid CitizenId,
    int? WasteCategoryId,
    DateTime OccurredOn) : IDomainEvent
{
    /// <inheritdoc />
    public string EventType => "ReportCreated";
}
