using System.Text.Json;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using WastePlatform.API.Converters;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Infrastructure;

[AllureEpic("Infrastructure")]
[AllureFeature("DateTime UTC Converter")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "UTC DateTime serialization for consistent timezone handling")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Infrastructure")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "DateTimeUtcConverterTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Infrastructure")]
[AllureOwner("Nguyễn Chí Trung")]
[AllureSeverity(SeverityLevel.minor)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("serialization")]
public class DateTimeUtcConverterTests
{
    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new DateTimeUtcConverter());
        options.Converters.Add(new DateTimeNullableUtcConverter());
        return options;
    }

    #region DateTimeUtcConverter

    [Fact]
    [AllureDescription("Serializes UTC DateTime to ISO 8601 format with Z suffix.")]
    public void Write_UtcDateTime_ShouldWriteWithZSuffix()
    {
        var options = CreateOptions();
        var dt = new DateTime(2026, 6, 13, 10, 30, 0, DateTimeKind.Utc);

        var json = JsonSerializer.Serialize(dt, options);

        AllureAttachmentHelper.AttachText("utc-datetime-json", json);
        json.Should().Be("\"2026-06-13T10:30:00Z\"");
    }

    [Fact]
    [AllureDescription("Serializes local DateTime by converting to UTC first.")]
    public void Write_LocalDateTime_ShouldConvertToUtcAndWriteWithZSuffix()
    {
        var options = CreateOptions();
        var dt = new DateTime(2026, 6, 13, 10, 30, 0, DateTimeKind.Local);

        var json = JsonSerializer.Serialize(dt, options);

        AllureAttachmentHelper.AttachText("local-datetime-json", json);
        json.Should().Contain("Z");
        json.Should().StartWith("\"").And.EndWith("\"");
    }

    [Fact]
    [AllureDescription("Serializes unspecified DateTime by converting to UTC.")]
    public void Write_UnspecifiedDateTime_ShouldConvertToUtcAndWriteWithZSuffix()
    {
        var options = CreateOptions();
        var dt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        var json = JsonSerializer.Serialize(dt, options);

        AllureAttachmentHelper.AttachText("unspecified-datetime-json", json);
        json.Should().Contain("Z");
    }

    [Fact]
    [AllureDescription("Deserializes ISO 8601 datetime string correctly.")]
    public void Read_IsoString_ShouldParseCorrectly()
    {
        var options = CreateOptions();
        var json = "\"2026-06-13T10:30:00Z\"";

        var result = JsonSerializer.Deserialize<DateTime>(json, options);

        AllureAttachmentHelper.AttachJson("iso-string-parse", new { Input = json, Parsed = result.ToString("O"), result.Year, result.Month, result.Day });
        result.Year.Should().Be(2026);
        result.Month.Should().Be(6);
        result.Day.Should().Be(13);
    }

    #endregion

    #region DateTimeNullableUtcConverter

    [Fact]
    [AllureDescription("Serializes null DateTime? as null.")]
    public void Write_NullDateTime_ShouldWriteNull()
    {
        var options = CreateOptions();
        DateTime? dt = null;

        var json = JsonSerializer.Serialize(dt, options);

        AllureAttachmentHelper.AttachText("null-datetime-json", json);
        json.Should().Be("null");
    }

    [Fact]
    [AllureDescription("Serializes non-null DateTime? with Z suffix.")]
    public void Write_NonNullDateTime_ShouldWriteWithZSuffix()
    {
        var options = CreateOptions();
        DateTime? dt = new DateTime(2026, 12, 25, 15, 0, 0, DateTimeKind.Utc);

        var json = JsonSerializer.Serialize(dt, options);

        AllureAttachmentHelper.AttachText("nullable-datetime-json", json);
        json.Should().Be("\"2026-12-25T15:00:00Z\"");
    }

    [Fact]
    [AllureDescription("Deserializes null JSON token to null DateTime?.")]
    public void Read_NullToken_ShouldReturnNull()
    {
        var options = CreateOptions();
        var json = "null";

        var result = JsonSerializer.Deserialize<DateTime?>(json, options);

        AllureAttachmentHelper.AttachJson("null-token-parse", new { Input = json, Result = (object?)result ?? "null" });
        result.Should().BeNull();
    }

    [Fact]
    [AllureDescription("Deserializes valid ISO 8601 string to non-null DateTime?.")]
    public void Read_ValidString_ShouldReturnDateTime()
    {
        var options = CreateOptions();
        var json = "\"2026-12-25T15:00:00Z\"";

        var result = JsonSerializer.Deserialize<DateTime?>(json, options);

        AllureAttachmentHelper.AttachJson("valid-string-parse", new { Input = json, Parsed = result?.ToString("O"), Year = result?.Year, Month = result?.Month });
        result.Should().NotBeNull();
        result!.Value.Year.Should().Be(2026);
        result.Value.Month.Should().Be(12);
    }

    #endregion

    #region Round-trip

    [Fact]
    [AllureDescription("DateTime serialization round-trip preserves the value.")]
    public void RoundTrip_ShouldPreserveDateTime()
    {
        var options = CreateOptions();
        var original = new DateTime(2026, 6, 13, 10, 30, 45, DateTimeKind.Utc);

        var json = JsonSerializer.Serialize(original, options);
        var deserialized = JsonSerializer.Deserialize<DateTime>(json, options);

        AllureAttachmentHelper.AttachJson("roundtrip-result", new { Original = original.ToString("O"), Serialized = json, Deserialized = deserialized.ToString("O") });
        deserialized.Year.Should().Be(original.Year);
        deserialized.Month.Should().Be(original.Month);
        deserialized.Day.Should().Be(original.Day);
        deserialized.Hour.Should().Be(original.Hour);
        deserialized.Minute.Should().Be(original.Minute);
        deserialized.Second.Should().Be(original.Second);
    }

    #endregion
}
