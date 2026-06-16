using FluentAssertions;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Microsoft.AspNetCore.Http;
using Moq;
using WastePlatform.Application.Reports.Commands;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;
using Xunit;

namespace WastePlatform.Tests.Application.Reports;

[AllureEpic("KIEM-5: Reports Module Testing")]
[AllureFeature("Create Report Handler")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Valid report submission with media evidence")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "CreateReportCommandHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Reports")]
[AllureOwner("Nguyễn Minh Phụng")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("reports")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-5")]
public class CreateReportCommandHandlerTests
{
    private readonly Mock<IReportRepository> _mockReportRepository;
    private readonly Mock<IWasteCategoryRepository> _mockCategoryRepository;
    private readonly Mock<IFileStorageService> _mockFileStorageService;
    private readonly CreateReportCommandHandler _handler;

    public CreateReportCommandHandlerTests()
    {
        _mockReportRepository = new Mock<IReportRepository>();
        _mockCategoryRepository = new Mock<IWasteCategoryRepository>();
        _mockFileStorageService = new Mock<IFileStorageService>();
        _handler = new CreateReportCommandHandler(
            _mockReportRepository.Object,
            _mockCategoryRepository.Object,
            _mockFileStorageService.Object);
    }

    #region TC-REP-001: Happy Path - Valid Data

    [Fact]
    [AllureDescription("Creates a report successfully when the command has valid category, coordinates, and at least one image.")]
    public async Task Handle_WithValidCommand_ShouldCreateReportSuccessfully()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var categoryId = 1;
        var command = new CreateReportCommand
        {
            CitizenId = citizenId,
            WasteCategoryId = categoryId,
            Latitude = 10.7769m,
            Longitude = 106.7009m,
            Description = "Rác thải sinh hoạt tại vỉa hè",
            Address = "123 Nguyễn Trãi, P.1, Q.1",
            Images = CreateMockImageCollection("report.jpg")
        };

        var category = new WasteCategory { Id = categoryId, Name = "Rác hữu cơ" };
        _mockCategoryRepository
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _mockFileStorageService
            .Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("uploaded-report.jpg");

        _mockReportRepository
            .Setup(x => x.AddAsync(It.IsAny<WasteReport>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WasteReport report, CancellationToken _) => report);

        _mockReportRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachJson("create-report-command", command);
        result.Should().NotBe(Guid.Empty, "Handler should return a valid report ID");
        
        // Vấn đề 4: Verify entity properties
        _mockReportRepository.Verify(
            x => x.AddAsync(
                It.Is<WasteReport>(r =>
                    r.Status == ReportStatus.Pending &&
                    r.WasteCategoryId == categoryId &&
                    r.CitizenId == citizenId &&
                    r.Latitude == 10.7769m &&
                    r.Longitude == 106.7009m &&
                    r.Images.Count == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
        
        _mockReportRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region TC-REP-002: Missing/Invalid Required Fields

    [Fact]
    [AllureDescription("Rejects report creation when no images are supplied.")]
    public async Task Handle_WithoutImages_ShouldThrowArgumentException()
    {
        // Arrange - Vấn đề 2: SRS yêu cầu ít nhất 1 ảnh
        var command = new CreateReportCommand
        {
            CitizenId = Guid.NewGuid(),
            WasteCategoryId = 1,
            Latitude = 10.7769m,
            Longitude = 106.7009m,
            Description = "Test without images",
            Address = "Test address",
            Images = null  // No images
        };

        var category = new WasteCategory { Id = 1, Name = "Test" };
        _mockCategoryRepository
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));

        AllureAttachmentHelper.AttachJson("create-report-without-images", command);
        AllureAttachmentHelper.AttachText("create-report-without-images-error", exception.Message);
        exception.Message.Should().Be("At least one image is required");
        _mockReportRepository.Verify(
            x => x.AddAsync(It.IsAny<WasteReport>(), It.IsAny<CancellationToken>()),
            Times.Never, "Should not create report without images");
    }

    [Fact]
    [AllureDescription("Rejects report creation when the image collection is empty.")]
    public async Task Handle_WithEmptyImages_ShouldThrowArgumentException()
    {
        // Arrange - Empty image collection
        var command = new CreateReportCommand
        {
            CitizenId = Guid.NewGuid(),
            WasteCategoryId = 1,
            Latitude = 10.7769m,
            Longitude = 106.7009m,
            Description = "Test with empty images",
            Address = "Test address",
            Images = new FormFileCollection()  // Empty
        };

        var category = new WasteCategory { Id = 1, Name = "Test" };
        _mockCategoryRepository
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));

        AllureAttachmentHelper.AttachJson("create-report-empty-images", command);
        AllureAttachmentHelper.AttachText("create-report-empty-images-error", exception.Message);
        exception.Message.Should().Be("At least one image is required");
    }

    [Fact]
    [AllureDescription("Rejects report creation when the waste category id does not exist.")]
    public async Task Handle_WithInvalidCategoryId_ShouldThrowArgumentException()
    {
        // Arrange - TC-REP-002 Scenario 1: Invalid/Missing WasteCategoryId
        var command = new CreateReportCommand
        {
            CitizenId = Guid.NewGuid(),
            WasteCategoryId = 999, // Non-existent category
            Latitude = 10.7769m,
            Longitude = 106.7009m,
            Description = "Test report",
            Address = "Test address"
        };

        _mockCategoryRepository
            .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WasteCategory?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));

        AllureAttachmentHelper.AttachJson("create-report-invalid-category", command);
        AllureAttachmentHelper.AttachText("create-report-invalid-category-error", exception.Message);
        exception.Message.Should().Be("Invalid waste category");
        _mockReportRepository.Verify(
            x => x.AddAsync(It.IsAny<WasteReport>(), It.IsAny<CancellationToken>()),
            Times.Never, "Should not create report with invalid category");
    }

    [Theory]
    [InlineData(-91, 106.7009)] // Latitude < -90
    [InlineData(91, 106.7009)]  // Latitude > 90
    [InlineData(10.7769, -181)] // Longitude < -180
    [InlineData(10.7769, 181)]  // Longitude > 180
    [AllureDescription("Rejects coordinates outside the valid latitude and longitude range.")]
    public async Task Handle_WithInvalidCoordinates_ShouldThrowArgumentException(decimal lat, decimal lng)
    {
        // Arrange - TC-REP-002 Scenario 2: Invalid Location
        var command = new CreateReportCommand
        {
            CitizenId = Guid.NewGuid(),
            WasteCategoryId = 1,
            Latitude = lat,
            Longitude = lng,
            Description = "Test report",
            Address = "Test address",
            Images = CreateMockImageCollection("test.jpg")  // Cần ảnh để test đúng flow
        };

        var category = new WasteCategory { Id = 1, Name = "Test" };
        _mockCategoryRepository
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act & Assert - Lỗi coordinate xảy ra trước khi upload ảnh
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));

        AllureAttachmentHelper.AttachJson("create-report-invalid-coordinates", command);
        AllureAttachmentHelper.AttachText("create-report-invalid-coordinates-error", exception.Message);
        exception.Message.Should().Be("Invalid latitude or longitude coordinates");
    }

    [Fact]
    [AllureDescription("Creates a report successfully when coordinates sit exactly on the allowed boundary.")]
    public async Task Handle_WithBoundaryCoordinates_ShouldCreateReportSuccessfully()
    {
        // Arrange - Valid boundary values
        var command = new CreateReportCommand
        {
            CitizenId = Guid.NewGuid(),
            WasteCategoryId = 1,
            Latitude = 90m,    // Max valid latitude
            Longitude = 180m,  // Max valid longitude
            Description = "Boundary test",
            Address = "Test address",
            Images = CreateMockImageCollection("boundary.jpg")
        };

        var category = new WasteCategory { Id = 1, Name = "Test" };
        _mockCategoryRepository
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _mockFileStorageService
            .Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("uploaded-boundary.jpg");

        _mockReportRepository
            .Setup(x => x.AddAsync(It.IsAny<WasteReport>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WasteReport report, CancellationToken _) => report);

        _mockReportRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        AllureAttachmentHelper.AttachJson("create-report-boundary-command", command);
        result.Should().NotBe(Guid.Empty);
        
        // Vấn đề 4: Verify entity properties
        _mockReportRepository.Verify(
            x => x.AddAsync(
                It.Is<WasteReport>(r =>
                    r.Status == ReportStatus.Pending &&
                    r.WasteCategoryId == 1 &&
                    r.Latitude == 90m &&
                    r.Longitude == 180m &&
                    r.Images.Count == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Vấn đề 3: Test Upload File

    [Fact]
    public async Task Handle_WithImages_ShouldUploadFilesAndAddImageEntities()
    {
        AllureAttachmentHelper.AttachText("test-h-a-n-d-l-e_-w-i-t-h-i-m-a-g-e-s_-s-h-o-u-l-d-u-p-", "Executed: Handle_WithImages_ShouldUploadFilesAndAddImageEntities");
        // Arrange
        var citizenId = Guid.NewGuid();
        var categoryId = 1;
        
        // Mock IFormFile
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.jpg");
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
        
        var files = new FormFileCollection { mockFile.Object };
        
        var command = new CreateReportCommand
        {
            CitizenId = citizenId,
            WasteCategoryId = categoryId,
            Latitude = 10.7769m,
            Longitude = 106.7009m,
            Description = "Test with images",
            Address = "Test address",
            Images = files
        };

        var category = new WasteCategory { Id = categoryId, Name = "Rác hữu cơ" };
        _mockCategoryRepository
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _mockFileStorageService
            .Setup(x => x.SaveFileAsync(mockFile.Object, It.IsAny<string[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("uploaded-test.jpg");

        _mockReportRepository
            .Setup(x => x.AddAsync(It.IsAny<WasteReport>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WasteReport report, CancellationToken _) => report);

        _mockReportRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        
        // Verify SaveFileAsync được gọi đúng 1 lần
        _mockFileStorageService.Verify(
            x => x.SaveFileAsync(
                mockFile.Object,
                It.Is<string[]>(exts => exts.Contains(".jpg") && exts.Contains(".png")),
                5 * 1024 * 1024, // 5MB max
                It.IsAny<CancellationToken>()),
            Times.Once);
        
        // Verify report có image được thêm
        _mockReportRepository.Verify(
            x => x.AddAsync(
                It.Is<WasteReport>(r =>
                    r.Images.Count == 1 &&
                    r.Images.First().ImageUrl == "uploaded-test.jpg"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUploadFails_ShouldThrowException()
    {
        AllureAttachmentHelper.AttachText("test-h-a-n-d-l-e_-w-h-e-n-u-p-l-o-a-d-f-a-i-l-s_-s-h-o-", "Executed: Handle_WhenUploadFails_ShouldThrowException");
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.jpg");
        mockFile.Setup(f => f.Length).Returns(1024);
        
        var files = new FormFileCollection { mockFile.Object };
        
        var command = new CreateReportCommand
        {
            CitizenId = Guid.NewGuid(),
            WasteCategoryId = 1,
            Latitude = 10.7769m,
            Longitude = 106.7009m,
            Description = "Test",
            Address = "Test",
            Images = files
        };

        var category = new WasteCategory { Id = 1, Name = "Test" };
        _mockCategoryRepository
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _mockFileStorageService
            .Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Upload failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Upload failed");
        
        // Không tạo report nếu upload fail
        _mockReportRepository.Verify(
            x => x.AddAsync(It.IsAny<WasteReport>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Vấn đề 7: Cancellation Token

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowTaskCanceledException()
    {
        AllureAttachmentHelper.AttachText("test-h-a-n-d-l-e_-w-h-e-n-c-a-n-c-e-l-l-e-d_-s-h-o-u-l-", "Executed: Handle_WhenCancelled_ShouldThrowTaskCanceledException");
        // Arrange
        var command = new CreateReportCommand
        {
            CitizenId = Guid.NewGuid(),
            WasteCategoryId = 1,
            Latitude = 10.7769m,
            Longitude = 106.7009m,
            Description = "Test",
            Address = "Test"
        };

        var category = new WasteCategory { Id = 1, Name = "Test" };
        _mockCategoryRepository
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException());

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _handler.Handle(command, cts.Token));
    }

    #endregion

    #region Helper Methods

    private static FormFileCollection CreateMockImageCollection(string fileName)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
        return new FormFileCollection { mockFile.Object };
    }

    private static FormFileCollection CreateMockImageCollectionMultiple(int count)
    {
        var collection = new FormFileCollection();
        for (int i = 0; i < count; i++)
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns($"image_{i+1}.jpg");
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
            collection.Add(mockFile.Object);
        }
        return collection;
    }

    #endregion

    #region BVA-F11: Images Count Boundary Value Analysis (KIEM-26, KIEM-29)
    // Áp dụng Standard BVA theo Ch.4 giáo trình:
    // min=1, max=5 → test: 0(invalid), 1(min), 2(min+1), 4(max-1), 5(max), 6(over max, invalid)
    // Số TCs = 4n+1 = 4(1)+1 = 5 valid + invalid cases

    [Fact]
    [AllureDescription("BVA-02: BVA Standard — 1 ảnh (đúng min) phải được chấp nhận (KIEM-26 fix)")]
    public async Task Handle_WithExactlyOneImage_ShouldCreateReportSuccessfully_BVA_Min()
    {
        AllureAttachmentHelper.AttachText("test-h-a-n-d-l-e_-w-i-t-h-e-x-a-c-t-l-y-o-n-e-i-m-a-g-e", "Executed: Handle_WithExactlyOneImage_ShouldCreateReportSuccessfully_BVA_Min");
        // Arrange — BVA: images = 1 (minimum boundary, valid)
        var command = new CreateReportCommand
        {
            CitizenId = Guid.NewGuid(),
            WasteCategoryId = 1,
            Latitude = 10.7769m,
            Longitude = 106.7009m,
            Description = "BVA min images test",
            Address = "Test address",
            Images = CreateMockImageCollectionMultiple(1)
        };

        var category = new WasteCategory { Id = 1, Name = "Test" };
        _mockCategoryRepository
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _mockFileStorageService
            .Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("image_1.jpg");

        _mockReportRepository
            .Setup(x => x.AddAsync(It.IsAny<WasteReport>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WasteReport report, CancellationToken _) => report);
        _mockReportRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — BVA-02: min boundary must pass
        result.Should().NotBe(Guid.Empty, "1 ảnh (min boundary) phải được chấp nhận");
        _mockReportRepository.Verify(
            x => x.AddAsync(It.Is<WasteReport>(r => r.Images.Count == 1), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("BVA-06: BVA Standard — 5 ảnh (đúng max) phải được chấp nhận (KIEM-29 boundary)")]
    public async Task Handle_WithExactlyFiveImages_ShouldCreateReportSuccessfully_BVA_Max()
    {
        AllureAttachmentHelper.AttachText("test-h-a-n-d-l-e_-w-i-t-h-e-x-a-c-t-l-y-f-i-v-e-i-m-a-g", "Executed: Handle_WithExactlyFiveImages_ShouldCreateReportSuccessfully_BVA_Max");
        // Arrange — BVA: images = 5 (maximum boundary, valid)
        var command = new CreateReportCommand
        {
            CitizenId = Guid.NewGuid(),
            WasteCategoryId = 1,
            Latitude = 10.7769m,
            Longitude = 106.7009m,
            Description = "BVA max images test",
            Address = "Test address",
            Images = CreateMockImageCollectionMultiple(5)
        };

        var category = new WasteCategory { Id = 1, Name = "Test" };
        _mockCategoryRepository
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _mockFileStorageService
            .Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("uploaded.jpg");

        _mockReportRepository
            .Setup(x => x.AddAsync(It.IsAny<WasteReport>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WasteReport report, CancellationToken _) => report);
        _mockReportRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — BVA-06: max boundary must pass
        result.Should().NotBe(Guid.Empty, "5 ảnh (max boundary) phải được chấp nhận");
        _mockReportRepository.Verify(
            x => x.AddAsync(It.Is<WasteReport>(r => r.Images.Count == 5), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    [AllureDescription("BVA-07: KIEM-29 Bug — 6 ảnh (vượt max) phải bị từ chối với ArgumentException")]
    public async Task Handle_WithSixImages_ShouldThrowArgumentException_BVA_OverMax_KIEM29()
    {
        // Arrange — BVA: images = 6 (above max boundary, INVALID — KIEM-29 bug)
        var command = new CreateReportCommand
        {
            CitizenId = Guid.NewGuid(),
            WasteCategoryId = 1,
            Latitude = 10.7769m,
            Longitude = 106.7009m,
            Description = "BVA over-max images test",
            Address = "Test address",
            Images = CreateMockImageCollectionMultiple(6)
        };

        var category = new WasteCategory { Id = 1, Name = "Test" };
        _mockCategoryRepository
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act & Assert — BVA-07: KIEM-29 — must throw when > 5 images
        // NOTE: This test FAILS on current implementation (bug KIEM-29 not yet fixed)
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("5", "Thông báo lỗi phải đề cập giới hạn 5 ảnh");
        _mockReportRepository.Verify(
            x => x.AddAsync(It.IsAny<WasteReport>(), It.IsAny<CancellationToken>()),
            Times.Never, "Không được tạo report khi vượt quá 5 ảnh");
    }

    [Theory]
    [InlineData(2)]  // BVA-03: min+1
    [InlineData(3)]  // BVA-04: nominal
    [InlineData(4)]  // BVA-05: max-1
    [AllureDescription("BVA-03/04/05: Images trong khoảng hợp lệ (2, 3, 4) phải được chấp nhận")]
    public async Task Handle_WithValidImageCount_ShouldCreateReportSuccessfully_BVA_Mid(int imageCount)
    {
        // Arrange — BVA: mid-range values (all valid)
        var command = new CreateReportCommand
        {
            CitizenId = Guid.NewGuid(),
            WasteCategoryId = 1,
            Latitude = 10.7769m,
            Longitude = 106.7009m,
            Description = $"BVA mid test with {imageCount} images",
            Address = "Test address",
            Images = CreateMockImageCollectionMultiple(imageCount)
        };

        var category = new WasteCategory { Id = 1, Name = "Test" };
        _mockCategoryRepository
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _mockFileStorageService
            .Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("uploaded.jpg");

        _mockReportRepository
            .Setup(x => x.AddAsync(It.IsAny<WasteReport>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WasteReport report, CancellationToken _) => report);
        _mockReportRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty, $"{imageCount} ảnh (valid range) phải được chấp nhận");
        _mockReportRepository.Verify(
            x => x.AddAsync(It.Is<WasteReport>(r => r.Images.Count == imageCount), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}
