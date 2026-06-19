using WastePlatform.Domain.Common;

namespace WastePlatform.Domain.ValueObjects;

/// <summary>
/// Value object representing geographic coordinates (latitude and longitude).
/// Used by <c>WasteReport</c> to store the report location.
/// </summary>
public sealed class GeoLocation : ValueObject
{
    /// <summary>
    /// Latitude in decimal degrees. Valid range: -90 to 90.
    /// </summary>
    public decimal Latitude { get; }

    /// <summary>
    /// Longitude in decimal degrees. Valid range: -180 to 180.
    /// </summary>
    public decimal Longitude { get; }

    /// <summary>
    /// Creates a new <see cref="GeoLocation"/> with validated coordinate ranges.
    /// </summary>
    /// <param name="latitude">Latitude (-90 to 90).</param>
    /// <param name="longitude">Longitude (-180 to 180).</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when latitude or longitude is outside the valid range.
    /// </exception>
    public GeoLocation(decimal latitude, decimal longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude),
                "Latitude must be between -90 and 90 degrees.");

        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude),
                "Longitude must be between -180 and 180 degrees.");

        Latitude = latitude;
        Longitude = longitude;
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }

    /// <inheritdoc />
    public override string ToString() => $"({Latitude}, {Longitude})";
}
