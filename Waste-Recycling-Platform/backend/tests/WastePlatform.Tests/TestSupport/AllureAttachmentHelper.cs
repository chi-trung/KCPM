using System.Text.Json;
using Allure.Net.Commons;

namespace WastePlatform.Tests.TestSupport;

internal static class AllureAttachmentHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static void AttachJson(string name, object payload)
    {
        var directory = Path.Combine(Path.GetTempPath(), "wasteplatform-allure-attachments");
        Directory.CreateDirectory(directory);

        var filePath = Path.Combine(directory, $"{SanitizeFileName(name)}-{Guid.NewGuid():N}.json");
        File.WriteAllText(filePath, JsonSerializer.Serialize(payload, JsonOptions));

        AllureApi.AddAttachment(name, "application/json", filePath);
    }

    public static void AttachText(string name, string content)
    {
        var directory = Path.Combine(Path.GetTempPath(), "wasteplatform-allure-attachments");
        Directory.CreateDirectory(directory);

        var filePath = Path.Combine(directory, $"{SanitizeFileName(name)}-{Guid.NewGuid():N}.txt");
        File.WriteAllText(filePath, content);

        AllureApi.AddAttachment(name, "text/plain", filePath);
    }

    private static string SanitizeFileName(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(value.Select(character => invalidChars.Contains(character) ? '_' : character));
    }
}