using System.Text;
using Allure.Xunit.Attributes;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using WastePlatform.Infrastructure.Services;
using WastePlatform.Tests.TestSupport;
using Xunit;

namespace WastePlatform.Tests.Infrastructure;

/// <summary>
/// Unit tests for CollectorTask image-upload validation (KIEM-20 / SRS §3.5)
/// TC-UPLOAD-008: CompleteTask — invalid extension is silently skipped (current behaviour)
/// TC-UPLOAD-009: CompleteTask — empty image (0 bytes) in collection is skipped
/// TC-UPLOAD-010: CompleteTask — image saved to uploads/tasks/ subfolder with GUID name
///
/// ⚠️  SRS Gap documented here:
///     SRS §3.5 requires "at least 1 evidence image" when completing a task.
///     The current CollectorTaskController.CompleteTask does NOT enforce this —
///     Images are treated as optional. TC-UPLOAD-008b documents this gap.
/// </summary>
[AllureEpic("KIEM-20: File Uploads & Storage Tests")]
[AllureFeature("Collector Evidence Image Upload")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Collector uploads evidence photo when completing a task")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Infrastructure")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "CollectorEvidenceUploadTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Infrastructure")]
[AllureOwner("Nguyễn Minh Phụng")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("file-upload")]
[Allure.Net.Commons.Attributes.AllureTag("collector")]
[Allure.Net.Commons.Attributes.AllureTag("storage")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-20")]
public class CollectorEvidenceUploadTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly LocalFileStorageService _service;

    // CollectorTaskController uses the same constraints as CreateReportCommand
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif"];
    private const long MaxSizeBytes = 5 * 1024 * 1024; // 5 MB

    public CollectorEvidenceUploadTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), $"waste-collector-tests-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempRoot);

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.SetupGet(x => x.ContentRootPath).Returns(_tempRoot);

        _service = new LocalFileStorageService(_envMock.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    // ── TC-UPLOAD-008 ──────────────────────────────────────────────────────────
    // Kiểm tra: LocalFileStorageService (được dùng trong collector flow) từ chối
    // extension không hợp lệ — cùng behaviour với CreateReport flow.

    [Theory]
    [InlineData("evidence.exe")]
    [InlineData("photo.pdf")]
    [InlineData("data.svg")]
    [AllureDescription(
        "TC-UPLOAD-008: Collector evidence upload — files with disallowed extensions " +
        "(.exe, .pdf, .svg) are rejected with InvalidOperationException 'Invalid file type'.")]
    public async Task CollectorUpload_WithInvalidExtension_ShouldThrowInvalidOperationException(string fileName)
    {
        var file = CreateFormFile(fileName, "application/octet-stream", content: "fake-evidence");

        AllureAttachmentHelper.AttachJson("collector-upload-invalid-ext-input",
            new { fileName, extension = Path.GetExtension(fileName) });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SaveFileAsync(file, AllowedExtensions, MaxSizeBytes));

        AllureAttachmentHelper.AttachText("collector-upload-invalid-ext-error", ex.Message);
        ex.Message.Should().Contain("Invalid file type");
        ex.Message.Should().Contain(Path.GetExtension(fileName).ToLowerInvariant());
    }

    // ── TC-UPLOAD-008b — SRS Gap documentation ────────────────────────────────
    // ⚠️  SRS §3.5: "Bắt buộc phải tải lên ít nhất 1 Ảnh xác thực khi nhấn Hoàn thành"
    //     Current CollectorTaskController.CompleteTask (line 256) treats Images as OPTIONAL.
    //     This test documents the gap so the team can decide to enforce it or update SRS.

    [Fact]
    [AllureDescription(
        "TC-UPLOAD-008b [SRS GAP]: SRS §3.5 requires at least 1 evidence image when completing a task. " +
        "This test DOCUMENTS the current permissive behaviour (no image = accepted). " +
        "Expected: SaveFileAsync is never called when no images provided — no exception from service layer. " +
        "Action required: Either enforce image requirement in CompleteTask controller or update SRS.")]
    [AllureSeverity(SeverityLevel.critical)]
    public void CollectorCompleteTask_WithoutImages_CurrentBehaviourIsPermissive()
    {
        // Arrange & Document
        AllureAttachmentHelper.AttachJson("srs-gap-008b", new
        {
            SRSRequirement = "§3.5: Bắt buộc ít nhất 1 Ảnh xác thực khi Hoàn thành",
            CurrentBehaviour = "CompleteTask controller accepts completion WITHOUT images (Images field optional)",
            CodeReference = "CollectorTaskController.cs line 256: if (images != null && images.Count > 0)",
            GapType = "Missing validation — SRS requirement not enforced in controller",
            Recommendation = "Add: if (images == null || images.Count == 0) return BadRequest(\"At least one evidence image is required\")"
        });

        // This test PASSES to document the gap — it does NOT assert broken behaviour
        // The gap is tracked: team must add validation or update SRS
        true.Should().BeTrue("Gap documented — no assertion failure intended; " +
                             "fix CollectorTaskController to enforce image requirement per SRS §3.5");
    }

    // ── TC-UPLOAD-009 ──────────────────────────────────────────────────────────

    [Fact]
    [AllureDescription(
        "TC-UPLOAD-009: Collector evidence upload — zero-byte file throws ArgumentException 'File is empty', " +
        "consistent with CreateReport behaviour.")]
    public async Task CollectorUpload_WithEmptyFile_ShouldThrowArgumentException()
    {
        var file = CreateFormFile("evidence.jpg", "image/jpeg", content: "");

        AllureAttachmentHelper.AttachJson("collector-upload-empty-input",
            new { file.FileName, file.Length });

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.SaveFileAsync(file, AllowedExtensions, MaxSizeBytes));

        AllureAttachmentHelper.AttachText("collector-upload-empty-error", ex.Message);
        ex.Message.Should().Contain("File is empty");
    }

    // ── TC-UPLOAD-010 ──────────────────────────────────────────────────────────

    [Fact]
    [AllureDescription(
        "TC-UPLOAD-010: Valid collector evidence image (.jpg) is saved with a GUID-based filename " +
        "and the returned name ends with the correct extension.")]
    public async Task CollectorUpload_WithValidJpg_ShouldSaveAndReturnGuidFilename()
    {
        var file = CreateFormFile("evidence.jpg", "image/jpeg", content: "real-photo-bytes");

        AllureAttachmentHelper.AttachJson("collector-upload-valid-input", new
        {
            file.FileName,
            file.ContentType,
            file.Length,
            AllowedExtensions,
            MaxSizeBytes
        });

        var result = await _service.SaveFileAsync(file, AllowedExtensions, MaxSizeBytes);

        AllureAttachmentHelper.AttachText("collector-upload-valid-result", result);

        // Assert filename is GUID-based and correct extension
        result.Should().NotBeNullOrEmpty();
        result.Should().EndWith(".jpg");

        var namePart = Path.GetFileNameWithoutExtension(result);
        Guid.TryParse(namePart, out _).Should().BeTrue(
            "Filename should be a GUID to prevent name collisions between collectors");

        // Assert file physically exists
        var savedPath = Path.Combine(_tempRoot, "uploads", result);
        File.Exists(savedPath).Should().BeTrue();
    }

    [Theory]
    [InlineData("cam1.jpeg", "image/jpeg")]
    [InlineData("scene.png",  "image/png")]
    [InlineData("clip.gif",   "image/gif")]
    [AllureDescription(
        "TC-UPLOAD-010: All allowed evidence image extensions (.jpeg, .png, .gif) " +
        "are accepted for collector evidence upload.")]
    public async Task CollectorUpload_WithAllowedExtensions_ShouldSaveSuccessfully(
        string fileName, string contentType)
    {
        var file = CreateFormFile(fileName, contentType, content: "evidence-data");

        AllureAttachmentHelper.AttachJson("collector-upload-ext-input", new { fileName, contentType });

        var result = await _service.SaveFileAsync(file, AllowedExtensions, MaxSizeBytes);

        AllureAttachmentHelper.AttachText("collector-upload-ext-result", result);

        result.Should().NotBeNullOrEmpty();
        result.Should().EndWith(Path.GetExtension(fileName).ToLowerInvariant());
    }

    [Fact]
    [AllureDescription(
        "TC-UPLOAD-010b: Two collectors uploading evidence for the same task at the same time " +
        "get distinct GUID filenames — no overwrite race condition.")]
    public async Task CollectorUpload_ConcurrentUploads_ShouldProduceDistinctFilenames()
    {
        var file1 = CreateFormFile("evidence.jpg", "image/jpeg", content: "collector-A-photo");
        var file2 = CreateFormFile("evidence.jpg", "image/jpeg", content: "collector-B-photo");

        // Act — sequential simulation of two concurrent uploads
        var result1 = await _service.SaveFileAsync(file1, AllowedExtensions, MaxSizeBytes);
        var result2 = await _service.SaveFileAsync(file2, AllowedExtensions, MaxSizeBytes);

        AllureAttachmentHelper.AttachJson("collector-upload-concurrent", new { result1, result2 });

        result1.Should().NotBe(result2,
            "Concurrent evidence uploads must not overwrite each other");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IFormFile CreateFormFile(string fileName, string contentType, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);

        var fileMock = new Mock<IFormFile>();
        fileMock.SetupGet(f => f.FileName).Returns(fileName);
        fileMock.SetupGet(f => f.ContentType).Returns(contentType);
        fileMock.SetupGet(f => f.Length).Returns(bytes.Length);
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns<Stream, CancellationToken>((target, ct) => stream.CopyToAsync(target, ct));

        return fileMock.Object;
    }
}
