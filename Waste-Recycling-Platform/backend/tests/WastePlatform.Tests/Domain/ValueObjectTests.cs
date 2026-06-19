using WastePlatform.Domain.Common;
using WastePlatform.Domain.ValueObjects;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Domain;

[AllureEpic("Domain Model")]
[AllureFeature("Value Objects")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "ValueObject base class and concrete implementations")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Domain")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "ValueObjectTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Domain")]
[AllureOwner("Nguyễn Chí Trung")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("domain")]
[Allure.Net.Commons.Attributes.AllureTag("value-object")]
public class ValueObjectTests
{
    // ── Email Value Object ──────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Valid email is normalized to lowercase and stored correctly.")]
    public void Email_ValidEmail_ShouldCreateAndNormalize()
    {
        var email = new Email("Test@Example.COM");

        Assert.Equal("test@example.com", email.Value);
        Assert.Equal("test@example.com", email.ToString());
    }

    [Fact]
    [AllureDescription("Email with leading/trailing whitespace is trimmed.")]
    public void Email_WithWhitespace_ShouldTrim()
    {
        var email = new Email("  user@test.com  ");

        Assert.Equal("user@test.com", email.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [AllureDescription("Empty or null email throws ArgumentException.")]
    public void Email_EmptyOrNull_ShouldThrow(string? value)
    {
        Assert.Throws<ArgumentException>(() => new Email(value!));
    }

    [Fact]
    [AllureDescription("Email without '@' symbol throws ArgumentException.")]
    public void Email_WithoutAtSymbol_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new Email("invalid-email"));
    }

    [Fact]
    [AllureDescription("Two Email objects with same value are structurally equal.")]
    public void Email_SameValue_ShouldBeEqual()
    {
        var email1 = new Email("user@test.com");
        var email2 = new Email("USER@TEST.COM");

        Assert.Equal(email1, email2);
        Assert.True(email1 == email2);
        Assert.False(email1 != email2);
        Assert.Equal(email1.GetHashCode(), email2.GetHashCode());
    }

    [Fact]
    [AllureDescription("Two Email objects with different values are not equal.")]
    public void Email_DifferentValue_ShouldNotBeEqual()
    {
        var email1 = new Email("user1@test.com");
        var email2 = new Email("user2@test.com");

        Assert.NotEqual(email1, email2);
        Assert.True(email1 != email2);
    }

    [Fact]
    [AllureDescription("Implicit conversion from Email to string returns the value.")]
    public void Email_ImplicitConversionToString_ShouldWork()
    {
        var email = new Email("user@test.com");
        string value = email;

        Assert.Equal("user@test.com", value);
    }

    [Fact]
    [AllureDescription("Implicit conversion from string to Email creates valid object.")]
    public void Email_ImplicitConversionFromString_ShouldWork()
    {
        Email email = "user@test.com";

        Assert.Equal("user@test.com", email.Value);
    }

    // ── GeoLocation Value Object ────────────────────────────────────────────

    [Fact]
    [AllureDescription("Valid coordinates create GeoLocation successfully.")]
    public void GeoLocation_ValidCoordinates_ShouldCreate()
    {
        var geo = new GeoLocation(10.762622m, 106.660172m);

        Assert.Equal(10.762622m, geo.Latitude);
        Assert.Equal(106.660172m, geo.Longitude);
    }

    [Theory]
    [InlineData(-90, -180)]    // Min boundary
    [InlineData(90, 180)]      // Max boundary
    [InlineData(0, 0)]         // Origin
    [AllureDescription("Boundary values for lat/lng are accepted (BVA).")]
    public void GeoLocation_BoundaryValues_ShouldAccept(decimal lat, decimal lng)
    {
        var geo = new GeoLocation(lat, lng);

        Assert.Equal(lat, geo.Latitude);
        Assert.Equal(lng, geo.Longitude);
    }

    [Theory]
    [InlineData(-91, 0)]
    [InlineData(91, 0)]
    [AllureDescription("Latitude outside [-90, 90] throws ArgumentOutOfRangeException (BVA).")]
    public void GeoLocation_InvalidLatitude_ShouldThrow(decimal lat, decimal lng)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoLocation(lat, lng));
    }

    [Theory]
    [InlineData(0, -181)]
    [InlineData(0, 181)]
    [AllureDescription("Longitude outside [-180, 180] throws ArgumentOutOfRangeException (BVA).")]
    public void GeoLocation_InvalidLongitude_ShouldThrow(decimal lat, decimal lng)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoLocation(lat, lng));
    }

    [Fact]
    [AllureDescription("Two GeoLocations with same coordinates are equal.")]
    public void GeoLocation_SameCoordinates_ShouldBeEqual()
    {
        var geo1 = new GeoLocation(10.762622m, 106.660172m);
        var geo2 = new GeoLocation(10.762622m, 106.660172m);

        Assert.Equal(geo1, geo2);
        Assert.True(geo1 == geo2);
        Assert.Equal(geo1.GetHashCode(), geo2.GetHashCode());
    }

    [Fact]
    [AllureDescription("Two GeoLocations with different coordinates are not equal.")]
    public void GeoLocation_DifferentCoordinates_ShouldNotBeEqual()
    {
        var geo1 = new GeoLocation(10.0m, 106.0m);
        var geo2 = new GeoLocation(11.0m, 107.0m);

        Assert.NotEqual(geo1, geo2);
    }

    [Fact]
    [AllureDescription("GeoLocation.ToString() returns formatted coordinate string.")]
    public void GeoLocation_ToString_ShouldFormatCorrectly()
    {
        var geo = new GeoLocation(10.5m, 106.5m);

        Assert.Equal("(10.5, 106.5)", geo.ToString());
    }

    // ── Address Value Object ────────────────────────────────────────────────

    [Fact]
    [AllureDescription("Address with both district and ward stores values correctly.")]
    public void Address_WithBothFields_ShouldCreate()
    {
        var address = new Address("Quận 1", "Phường Bến Nghé");

        Assert.Equal("Quận 1", address.District);
        Assert.Equal("Phường Bến Nghé", address.Ward);
    }

    [Fact]
    [AllureDescription("Address with null fields is valid (optional address).")]
    public void Address_WithNullFields_ShouldCreate()
    {
        var address = new Address();

        Assert.Null(address.District);
        Assert.Null(address.Ward);
    }

    [Fact]
    [AllureDescription("Address trims whitespace from district and ward.")]
    public void Address_WithWhitespace_ShouldTrim()
    {
        var address = new Address("  Quận 3  ", "  Phường 1  ");

        Assert.Equal("Quận 3", address.District);
        Assert.Equal("Phường 1", address.Ward);
    }

    [Fact]
    [AllureDescription("Two Addresses with same district and ward are equal.")]
    public void Address_SameValues_ShouldBeEqual()
    {
        var addr1 = new Address("Quận 1", "Phường Bến Nghé");
        var addr2 = new Address("Quận 1", "Phường Bến Nghé");

        Assert.Equal(addr1, addr2);
        Assert.True(addr1 == addr2);
    }

    [Fact]
    [AllureDescription("Address.ToString() joins non-empty parts with comma.")]
    public void Address_ToString_ShouldJoinParts()
    {
        var address = new Address("Quận 1", "Phường Bến Nghé");

        Assert.Equal("Phường Bến Nghé, Quận 1", address.ToString());
    }

    [Fact]
    [AllureDescription("Address.ToString() with only district shows district only.")]
    public void Address_ToString_OnlyDistrict_ShouldShowDistrict()
    {
        var address = new Address("Quận 1");

        Assert.Equal("Quận 1", address.ToString());
    }

    [Fact]
    [AllureDescription("Address.ToString() with all null returns empty string.")]
    public void Address_ToString_AllNull_ShouldReturnEmpty()
    {
        var address = new Address();

        Assert.Equal(string.Empty, address.ToString());
    }

    // ── ValueObject base class equality ─────────────────────────────────────

    [Fact]
    [AllureDescription("ValueObject.Equals with null returns false.")]
    public void ValueObject_EqualsNull_ShouldReturnFalse()
    {
        var email = new Email("user@test.com");

        Assert.False(email.Equals(null));
    }

    [Fact]
    [AllureDescription("ValueObject.Equals with different type returns false.")]
    public void ValueObject_EqualsDifferentType_ShouldReturnFalse()
    {
        var email = new Email("user@test.com");
        var geo = new GeoLocation(10m, 106m);

        Assert.False(email.Equals(geo));
    }

    [Fact]
    [AllureDescription("ValueObject null == null returns true via operator.")]
    public void ValueObject_NullEqualsNull_ShouldReturnTrue()
    {
        Email? a = null;
        Email? b = null;

        Assert.True(a == b);
    }

    [Fact]
    [AllureDescription("ValueObject null != value returns true via operator.")]
    public void ValueObject_NullNotEqualsValue_ShouldReturnTrue()
    {
        Email? a = null;
        var b = new Email("user@test.com");

        Assert.True(a != b);
    }
}
