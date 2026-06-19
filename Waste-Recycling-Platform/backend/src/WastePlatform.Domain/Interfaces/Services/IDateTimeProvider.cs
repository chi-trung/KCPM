namespace WastePlatform.Domain.Interfaces.Services;

/// <summary>
/// Abstraction for the system clock, enabling deterministic testing
/// and consistent UTC time usage across the domain.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
}
