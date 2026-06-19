using WastePlatform.Domain.Common;

namespace WastePlatform.Domain.Events;

/// <summary>
/// Domain event raised when a citizen files a new complaint.
/// </summary>
/// <param name="ComplaintId">The unique identifier of the complaint.</param>
/// <param name="CitizenId">The user ID of the citizen who filed the complaint.</param>
/// <param name="EnterpriseId">The enterprise ID the complaint is against, if applicable.</param>
/// <param name="ReportId">The associated waste report ID, if applicable.</param>
/// <param name="OccurredOn">UTC timestamp when the event occurred.</param>
public sealed record ComplaintCreatedEvent(
    Guid ComplaintId,
    Guid CitizenId,
    Guid? EnterpriseId,
    Guid? ReportId,
    DateTime OccurredOn) : IDomainEvent
{
    /// <inheritdoc />
    public string EventType => "ComplaintCreated";
}
