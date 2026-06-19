using WastePlatform.Domain.Common;

namespace WastePlatform.Domain.ValueObjects;

/// <summary>
/// Value object representing a Vietnamese address with district and ward.
/// Both fields are optional to support partial address information.
/// </summary>
public sealed class Address : ValueObject
{
    /// <summary>
    /// The district name (Quận/Huyện). Optional.
    /// </summary>
    public string? District { get; }

    /// <summary>
    /// The ward name (Phường/Xã). Optional.
    /// </summary>
    public string? Ward { get; }

    /// <summary>
    /// Creates a new <see cref="Address"/> with optional district and ward.
    /// </summary>
    /// <param name="district">District name, or null.</param>
    /// <param name="ward">Ward name, or null.</param>
    public Address(string? district = null, string? ward = null)
    {
        District = district?.Trim();
        Ward = ward?.Trim();
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return District;
        yield return Ward;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var parts = new[] { Ward, District }.Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join(", ", parts);
    }
}
