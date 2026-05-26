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

[AllureEpic("Reports")]
[AllureFeature("Create Report Handler")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Valid report submission with media evidence")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "CreateReportCommandHandlerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Reports")]
[AllureOwner("backend")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("reports")]
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

    #endregion
}
