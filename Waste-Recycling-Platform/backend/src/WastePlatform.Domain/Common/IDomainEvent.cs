namespace WastePlatform.Domain.Common;

/// <summary>
/// Marker interface for domain events following DDD patterns.
/// Events capture side-effects of state changes in the domain.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// The UTC timestamp when this domain event occurred.
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// A string identifier for the event type (e.g. "ReportCreated").
    /// </summary>
    string EventType { get; }
}
