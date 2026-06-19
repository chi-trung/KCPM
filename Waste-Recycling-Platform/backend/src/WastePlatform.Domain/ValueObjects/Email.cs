using WastePlatform.Domain.Common;

namespace WastePlatform.Domain.ValueObjects;

/// <summary>
/// Value object representing a validated email address.
/// </summary>
public sealed class Email : ValueObject
{
    /// <summary>
    /// The validated email string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new <see cref="Email"/> after validating the format.
    /// </summary>
    /// <param name="value">The email address string.</param>
    /// <exception cref="ArgumentException">Thrown when the email is empty or has an invalid format.</exception>
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email address cannot be empty.", nameof(value));

        if (!value.Contains('@'))
            throw new ArgumentException("Email address must contain '@'.", nameof(value));

        Value = value.Trim().ToLowerInvariant();
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>Implicit conversion from <see cref="Email"/> to <see cref="string"/>.</summary>
    public static implicit operator string(Email email) => email.Value;

    /// <summary>Implicit conversion from <see cref="string"/> to <see cref="Email"/>.</summary>
    public static implicit operator Email(string value) => new(value);
}
