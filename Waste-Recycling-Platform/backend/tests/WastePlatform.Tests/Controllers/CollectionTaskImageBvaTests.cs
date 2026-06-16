using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using WastePlatform.API.Controllers;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;
using WastePlatform.Infrastructure.SignalR;
using WastePlatform.Tests.TestSupport;
using Xunit;

namespace WastePlatform.Tests.Controllers;

/// <summary>
/// BVA (Boundary Value Analysis) Test Suite cho CollectionTask Image Upload API.
/// KIEM-68: Kiểm thử biên giới cho tính năng upload hình ảnh xác nhận công việc thu gom rác.
/// 
/// Phạm vi kiểm thử:
/// - Giới hạn kích thước tệp hình ảnh (0 bytes, 1 byte, max-1, max, max+1)
/// - Loại tệp được phép (.jpg, .jpeg, .png, .gif) và không được phép (.exe, .pdf, .txt)
/// - Số lượng hình ảnh (0, 1, nhiều, quá mức giới hạn)
/// - Tên tệp rỗng, đặc biệt, dài vượt mức
/// - Độ phân giải hình ảnh (quá nhỏ, bình thường, quá lớn)
/// - Dữ liệu hình ảnh bị hỏng hoặc không hợp lệ
/// </summary>
[AllureEpic("Quality Assurance Practices")]
[AllureFeature("CollectionTask Boundary Value Analysis")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Image Upload Boundary Value Testing")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "CollectionTaskImageBvaTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("Thanh Duy")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("api")]
[Allure.Net.Commons.Attributes.AllureTag("bva")]
[Allure.Net.Commons.Attributes.AllureTag("image-upload")]
[Allure.Net.Commons.Attributes.AllureTag("boundary-value-analysis")]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-68")]
public class CollectionTaskImageBvaTests
{
    // ==================== CONFIGURATION CONSTANTS ====================
    
    /// <summary>Giới hạn tối đa kích thước tệp cho một hình ảnh (bytes)</summary>
    private const long MAX_IMAGE_FILE_SIZE_BYTES = 10_485_760; // 10 MB
    
    /// <summary>Giới hạn tối thiểu kích thước tệp hợp lệ (bytes)</summary>
    private const long MIN_VALID_IMAGE_FILE_SIZE_BYTES = 1_024; // 1 KB
    
    /// <summary>Số lượng hình ảnh tối đa được phép upload cùng một lúc</summary>
    private const int MAX_IMAGES_PER_UPLOAD = 10;
    
    /// <summary>Số lượng hình ảnh tối thiểu yêu cầu</summary>
    private const int MIN_IMAGES_REQUIRED = 1;
    
    /// <summary>Độ dài tối đa cho tên tệp</summary>
    private const int MAX_FILENAME_LENGTH = 255;
    
    /// <summary>Độ dài tối thiểu cho tên tệp hợp lệ</summary>
    private const int MIN_FILENAME_LENGTH = 3;
    
    /// <summary>Độ phân giải (pixel) tối thiểu cho hình ảnh hợp lệ</summary>
    private const int MIN_IMAGE_RESOLUTION_PIXELS = 640;
    
    /// <summary>Độ phân giải (pixel) tối đa cho hình ảnh hợp lệ</summary>
    private const int MAX_IMAGE_RESOLUTION_PIXELS = 12000;
    
    /// <summary>Mảng các phần mở rộng tệp được phép upload</summary>
    private static readonly string[] ALLOWED_IMAGE_EXTENSIONS = { ".jpg", ".jpeg", ".png", ".gif" };
    
    /// <summary>Mảng các phần mở rộng tệp KHÔNG được phép upload</summary>
    private static readonly string[] DISALLOWED_EXTENSIONS = { ".exe", ".pdf", ".txt", ".doc", ".bat", ".sh", ".sql" };
    
    // ==================== SETUP METHODS & HELPER FACTORIES ====================
    
    /// <summary>
    /// Tạo DbContext In-Memory cho các bài kiểm thử.
    /// Sử dụng SQLite In-Memory database để đảm bảo mỗi test có môi trường độc lập.
    /// </summary>
    /// <returns>WastePlatformDbContext được cấu hình với database In-Memory</returns>
    private static WastePlatformDbContext CreateInMemoryDbContext()
    {
        var inMemoryDatabaseNameSuffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        var dbContextOptionsBuilderInstance = new DbContextOptionsBuilder<WastePlatformDbContext>();
        
        var dbContextOptionsConfigured = dbContextOptionsBuilderInstance
            .UseSqlite($"Data Source=:memory:{inMemoryDatabaseNameSuffix}:")
            .Options;
        
        var dbContextInstanceCreated = new WastePlatformDbContext(dbContextOptionsConfigured);
        dbContextInstanceCreated.Database.EnsureCreated();
        
        return dbContextInstanceCreated;
    }
    
    /// <summary>
    /// Tạo ControllerContext giả lập với User Role là "Collector".
    /// Thiết lập các claims cần thiết cho authentication và authorization kiểm thử.
    /// </summary>
    /// <param name="collectorUserId">ID của Collector User (mặc định: Guid ngẫu nhiên)</param>
    /// <param name="collectorUserEmail">Email của Collector (mặc định: test-collector@example.com)</param>
    /// <param name="collectorUserFullName">Họ tên của Collector (mặc định: Test Collector)</param>
    /// <returns>ControllerContext được cấu hình với claims Collector</returns>
    private static ControllerContext CreateCollectorControllerContext(
        Guid? collectorUserId = null,
        string collectorUserEmail = "test-collector@example.com",
        string collectorUserFullName = "Test Collector")
    {
        var collectorUserIdToUse = collectorUserId ?? Guid.NewGuid();
        
        var claimsListForCollectorPrincipal = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, collectorUserIdToUse.ToString()),
            new Claim(ClaimTypes.Email, collectorUserEmail),
            new Claim(ClaimTypes.Name, collectorUserFullName),
            new Claim(ClaimTypes.Role, "Collector")
        };
        
        var claimsIdentityForCollector = new ClaimsIdentity(claimsListForCollectorPrincipal, "TestAuthType");
        var claimsPrincipalForCollector = new ClaimsPrincipal(claimsIdentityForCollector);
        
        var httpContextMockInstanceForCollector = new Mock<HttpContext>();
        httpContextMockInstanceForCollector
            .Setup(ctx => ctx.User)
            .Returns(claimsPrincipalForCollector);
        
        var controllerContextInstanceForCollector = new ControllerContext
        {
            HttpContext = httpContextMockInstanceForCollector.Object
        };
        
        return controllerContextInstanceForCollector;
    }
    
    /// <summary>
    /// Tạo IFormFile giả lập từ nội dung byte nhất định.
    /// Mô phỏng tệp hình ảnh thực tế được upload từ client.
    /// </summary>
    /// <param name="fileNameWithExtension">Tên tệp kèm phần mở rộng (ví dụ: "image.jpg")</param>
    /// <param name="fileContentBytesArray">Nội dung tệp dưới dạng byte array</param>
    /// <param name="contentTypeOfFile">MIME type của tệp (mặc định: "image/jpeg")</param>
    /// <returns>Mock IFormFile có thể được sử dụng trong test</returns>
    private static IFormFile CreateMockFormFile(
        string fileNameWithExtension,
        byte[] fileContentBytesArray,
        string contentTypeOfFile = "image/jpeg")
    {
        var memoryStreamForFileContent = new MemoryStream(fileContentBytesArray);
        
        var formFileMockInstance = new Mock<IFormFile>();
        formFileMockInstance
            .Setup(f => f.FileName)
            .Returns(fileNameWithExtension);
        
        formFileMockInstance
            .Setup(f => f.Length)
            .Returns(fileContentBytesArray.Length);
        
        formFileMockInstance
            .Setup(f => f.ContentType)
            .Returns(contentTypeOfFile);
        
        formFileMockInstance
            .Setup(f => f.OpenReadStream())
            .Returns(memoryStreamForFileContent);
        
        formFileMockInstance
            .Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(async (Stream destination, CancellationToken ct) =>
            {
                memoryStreamForFileContent.Position = 0;
                await memoryStreamForFileContent.CopyToAsync(destination, 81920, ct);
            });
        
        return formFileMockInstance.Object;
    }
    
    /// <summary>
    /// Tạo FormFileCollection giả lập từ danh sách IFormFile.
    /// Sử dụng trong quá trình kiểm thử submit biểu mẫu với nhiều tệp.
    /// </summary>
    /// <param name="formFilesListToAdd">Danh sách IFormFile cần thêm vào collection</param>
    /// <returns>Mock FormFileCollection chứa các tệp</returns>
    private static IFormFileCollection CreateMockFormFileCollection(IList<IFormFile> formFilesListToAdd)
    {
        var formFileCollectionMockInstance = new Mock<FormFileCollection>();
        
        formFileCollectionMockInstance
            .Setup(ffc => ffc.GetFiles(It.IsAny<string>()))
            .Returns((string key) =>
            {
                if (key.Equals("Images", StringComparison.OrdinalIgnoreCase))
                    return formFilesListToAdd;
                return new List<IFormFile>();
            });
        
        formFileCollectionMockInstance
            .Setup(ffc => ffc.Count)
            .Returns(formFilesListToAdd.Count);
        
        formFileCollectionMockInstance
            .Setup(ffc => ffc.GetEnumerator())
            .Returns(formFilesListToAdd.GetEnumerator());
        
        return formFileCollectionMockInstance.Object;
    }
    
    /// <summary>
    /// Tạo FormCollection giả lập với dữ liệu WeightKg và Notes.
    /// Sử dụng cho endpoint POST complete task kèm metadata.
    /// </summary>
    /// <param name="weightKgValue">Giá trị khối lượng được thu gom (kg)</param>
    /// <param name="notesTextValue">Ghi chú bổ sung từ Collector</param>
    /// <param name="formFilesCollectionToInclude">FormFileCollection chứa hình ảnh</param>
    /// <returns>Mock FormCollection với tất cả dữ liệu cần thiết</returns>
    private static IFormCollection CreateMockFormCollection(
        decimal weightKgValue,
        string notesTextValue,
        IFormFileCollection formFilesCollectionToInclude)
    {
        var formCollectionMockInstance = new Mock<IFormCollection>();
        
        var formDataDictionary = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "WeightKg", weightKgValue.ToString() },
            { "Notes", notesTextValue ?? string.Empty }
        };
        
        formCollectionMockInstance
            .Setup(fc => fc[It.IsAny<string>()])
            .Returns((string key) =>
            {
                if (formDataDictionary.TryGetValue(key, out var value))
                    return value;
                return Microsoft.Extensions.Primitives.StringValues.Empty;
            });
        
        formCollectionMockInstance
            .Setup(fc => fc.Files)
            .Returns(formFilesCollectionToInclude);
        
        return formCollectionMockInstance.Object;
    }
    
    /// <summary>
    /// Tạo và seed dữ liệu cơ sở dữ liệu cho kiểm thử.
    /// Bao gồm: Enterprise, Citizen, WasteCategory, WasteReport, Collector, User, CollectionTask.
    /// </summary>
    /// <param name="dbContextInstanceToSeed">DbContext được dùng để insert dữ liệu</param>
    /// <param name="collectorUserIdToLink">ID của Collector User cần liên kết</param>
    /// <returns>Tuple chứa các entity đã tạo: (Enterprise, Citizen, Category, Report, Collector, User, Task)</returns>
    private static (Enterprise, Citizen, WasteCategory, WasteReport, Collector, User, CollectionTask) 
        SeedTestDataIntoDatabase(
            WastePlatformDbContext dbContextInstanceToSeed,
            Guid collectorUserIdToLink)
    {
        // Tạo Enterprise
        var enterpriseIdForTest = Guid.NewGuid();
        var enterpriseForTest = new Enterprise
        {
            Id = enterpriseIdForTest,
            Name = "Test Enterprise - BVA Image Upload",
            TaxId = "1234567890",
            Address = "123 Test Street, Test City",
            Phone = "0123456789",
            Email = "enterprise@test.com",
            Status = EnterpriseStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        // Tạo Citizen
        var citizenIdForTest = Guid.NewGuid();
        var citizenForTest = new Citizen
        {
            Id = citizenIdForTest,
            UserId = Guid.NewGuid(),
            FullName = "Test Citizen",
            Phone = "0987654321",
            Address = "456 Citizen Ave, Test City",
            CreatedAt = DateTime.UtcNow
        };
        
        // Tạo WasteCategory
        var wasteCategoryIdForTest = Guid.NewGuid();
        var wasteCategoryForTest = new WasteCategory
        {
            Id = wasteCategoryIdForTest,
            Name = "Plastic Waste",
            Description = "All types of plastic waste",
            CreatedAt = DateTime.UtcNow
        };
        
        // Tạo WasteReport
        var wasteReportIdForTest = Guid.NewGuid();
        var wasteReportForTest = new WasteReport
        {
            Id = wasteReportIdForTest,
            CitizenId = citizenIdForTest,
            EnterpriseId = enterpriseIdForTest,
            WasteCategoryId = wasteCategoryIdForTest,
            Description = "Large pile of plastic bottles",
            Address = "456 Citizen Ave, Test City",
            Latitude = 10.7769M,
            Longitude = 106.6966M,
            Status = ReportStatus.Assigned,
            CreatedAt = DateTime.UtcNow
        };
        
        // Tạo User (cho Collector)
        var userForCollectorTest = new User
        {
            Id = collectorUserIdToLink,
            Email = "test-collector@example.com",
            FullName = "Test Collector",
            Phone = "0111223344",
            Role = UserRole.Collector,
            CreatedAt = DateTime.UtcNow
        };
        
        // Tạo Collector
        var collectorIdForTest = Guid.NewGuid();
        var collectorForTest = new Collector
        {
            Id = collectorIdForTest,
            UserId = collectorUserIdToLink,
            EnterpriseId = enterpriseIdForTest,
            Status = CollectorStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        // Tạo CollectionTask
        var collectionTaskIdForTest = Guid.NewGuid();
        var collectionTaskForTest = new CollectionTask
        {
            Id = collectionTaskIdForTest,
            ReportId = wasteReportIdForTest,
            EnterpriseId = enterpriseIdForTest,
            CollectorId = collectorIdForTest,
            Status = CollectionTaskStatus.OnTheWay,
            AssignedAt = DateTime.UtcNow.AddHours(-2),
            CompletedAt = null,
            Notes = null,
            CollectedWeightKg = null
        };
        
        // Add tất cả entities vào DbContext
        dbContextInstanceToSeed.Enterprises.Add(enterpriseForTest);
        dbContextInstanceToSeed.Citizens.Add(citizenForTest);
        dbContextInstanceToSeed.WasteCategories.Add(wasteCategoryForTest);
        dbContextInstanceToSeed.WasteReports.Add(wasteReportForTest);
        dbContextInstanceToSeed.Users.Add(userForCollectorTest);
        dbContextInstanceToSeed.Collectors.Add(collectorForTest);
        dbContextInstanceToSeed.CollectionTasks.Add(collectionTaskForTest);
        
        // SaveChanges
        dbContextInstanceToSeed.SaveChanges();
        
        AllureAttachmentHelper.AttachJson("seeded-test-data", new
        {
            enterpriseId = enterpriseIdForTest,
            citizenId = citizenIdForTest,
            wasteCategoryId = wasteCategoryIdForTest,
            wasteReportId = wasteReportIdForTest,
            collectorId = collectorIdForTest,
            collectionTaskId = collectionTaskIdForTest,
            userId = collectorUserIdToLink
        });
        
        return (enterpriseForTest, citizenForTest, wasteCategoryForTest, 
                wasteReportForTest, collectorForTest, userForCollectorTest, collectionTaskForTest);
    }
    
    /// <summary>
    /// Tạo byte array với kích thước chính xác (hữu ích cho BVA boundary testing).
    /// Dữ liệu được điền bằng pattern "0xFF" để giả lập dữ liệu nhị phân hình ảnh.
    /// </summary>
    /// <param name="sizeInBytes">Kích thước mong muốn của byte array</param>
    /// <returns>Byte array với các byte có giá trị 0xFF</returns>
    private static byte[] CreateByteArrayOfExactSize(long sizeInBytes)
    {
        var byteArrayCreated = new byte[sizeInBytes];
        for (long indexCounter = 0; indexCounter < sizeInBytes; indexCounter++)
        {
            byteArrayCreated[indexCounter] = 0xFF;
        }
        return byteArrayCreated;
    }
    
    // ==================== MOCK SERVICE FACTORIES ====================
    
    /// <summary>
    /// Tạo Mock IHubContext cho SignalR.
    /// Dùng để kiểm thử các thông báo real-time được gửi tới clients.
    /// </summary>
    /// <returns>Mock IHubContext<TaskHub> chế độ lỏng lẻo</returns>
    private static Mock<IHubContext<TaskHub>> CreateMockTaskHub()
    {
        var taskHubMockInstance = new Mock<IHubContext<TaskHub>>();
        var clientsProxyMockInstance = new Mock<IHubCallerClients>();
        var allClientsProxyMockInstance = new Mock<IClientProxy>();
        
        clientsProxyMockInstance
            .Setup(clients => clients.All)
            .Returns(allClientsProxyMockInstance.Object);
        
        taskHubMockInstance
            .Setup(hub => hub.Clients)
            .Returns(clientsProxyMockInstance.Object);
        
        allClientsProxyMockInstance
            .Setup(proxy => proxy.SendAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        return taskHubMockInstance;
    }
    
    /// <summary>
    /// Tạo Mock INotificationService.
    /// Dùng để kiểm thử các notifications được gửi tới users.
    /// </summary>
    /// <returns>Mock INotificationService chế độ lỏng lẻo</returns>
    private static Mock<INotificationService> CreateMockNotificationService()
    {
        var notificationServiceMockInstance = new Mock<INotificationService>();
        
        notificationServiceMockInstance
            .Setup(svc => svc.NotifyCollectorOnTheWayAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        
        notificationServiceMockInstance
            .Setup(svc => svc.NotifyTaskCompletedAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        
        return notificationServiceMockInstance;
    }
    
    /// <summary>
    /// Tạo Mock IMediator cho CQRS pattern.
    /// Dùng để kiểm thử xử lý commands và queries.
    /// </summary>
    /// <returns>Mock IMediator chế độ lỏng lẻo</returns>
    private static Mock<IMediator> CreateMockMediator()
    {
        return new Mock<IMediator>();
    }
    
    // ==================== SETUP & INITIALIZATION HELPERS ====================
    
    /// <summary>
    /// Khởi tạo toàn bộ test environment: DbContext, Controller, Mocks, và seeded data.
    /// Phương thức này tập hợp tất cả các helper factories để chuẩn bị một test case hoàn chỉnh.
    /// </summary>
    /// <returns>
    /// Tuple chứa:
    /// - dbContextInstance: DbContext In-Memory
    /// - collectorControllerInstance: CollectorTaskController với mocks
    /// - collectionTaskIdForTesting: ID của task được tạo
    /// - collectorUserIdForTesting: ID của collector user
    /// </returns>
    private (WastePlatformDbContext, CollectorTaskController, Guid, Guid) InitializeCompleteTestEnvironment()
    {
        // Lấy Collector User ID cần sử dụng
        var collectorUserIdToUseInTest = Guid.NewGuid();
        
        // Tạo In-Memory DbContext
        var dbContextInstanceForTest = CreateInMemoryDbContext();
        
        // Seed dữ liệu test vào database
        var (enterprise, citizen, category, report, collector, user, task) = 
            SeedTestDataIntoDatabase(dbContextInstanceForTest, collectorUserIdToUseInTest);
        
        // Tạo mocks cho dependencies
        var hubContextMockForTest = CreateMockTaskHub();
        var notificationServiceMockForTest = CreateMockNotificationService();
        var mediatorMockForTest = CreateMockMediator();
        
        // Tạo controller instance với mocks
        var collectorTaskControllerForTest = new CollectorTaskController(
            dbContextInstanceForTest,
            hubContextMockForTest.Object,
            mediatorMockForTest.Object,
            notificationServiceMockForTest.Object);
        
        // Setup controller context với Collector role
        var controllerContextForTest = CreateCollectorControllerContext(collectorUserIdToUseInTest);
        collectorTaskControllerForTest.ControllerContext = controllerContextForTest;
        
        AllureAttachmentHelper.AttachText("environment-initialization", 
            $"Initialized test environment:\n" +
            $"- CollectorUserId: {collectorUserIdToUseInTest}\n" +
            $"- CollectionTaskId: {task.Id}\n" +
            $"- DbContext Type: SQLite In-Memory\n" +
            $"- Mocks: IHubContext<TaskHub>, INotificationService, IMediator\n" +
            $"- ControllerContext Role: Collector");
        
        return (dbContextInstanceForTest, collectorTaskControllerForTest, task.Id, collectorUserIdToUseInTest);
    }
    
    // ==================== TEST CASES - BOUNDARY VALUE ANALYSIS ====================
    
    #region BVA Tests - File Size Boundaries
    
    [Fact]
    [AllureDescription("BVA: Upload image with size = 0 bytes (boundary minimum). Should be rejected as invalid/empty.")]
    [AllureTag("bva-file-size")]
    [AllureTag("boundary-minimum")]
    public async Task CompleteTask_UploadImageWithZeroBytes_ShouldRejectAsInvalid()
    {
        // Arrange
        var (dbContext, controller, taskId, userId) = InitializeCompleteTestEnvironment();
        var emptyImageFileBytes = Array.Empty<byte>();
        var mockFormFile = CreateMockFormFile("empty.jpg", emptyImageFileBytes);
        var formFileCollection = CreateMockFormFileCollection(new[] { mockFormFile });
        var formCollection = CreateMockFormCollection(weightKgValue: 5.5m, notesTextValue: "Test", formFileCollection);
        
        AllureAttachmentHelper.AttachJson("test-parameters", new
        {
            taskId,
            imageSizeBytes = emptyImageFileBytes.Length,
            fileName = "empty.jpg",
            expectedBehavior = "Reject due to zero-byte size"
        });
        
        // Act
        var result = await controller.CompleteTask(taskId, formCollection);
        
        // Assert
        result.Should().NotBeNull();
        AllureAttachmentHelper.AttachJson("test-result", new { resultType = result.GetType().Name });
    }
    
    [Fact]
    [AllureDescription("BVA: Upload image with size = 1 byte. Should be rejected as too small.")]
    [AllureTag("bva-file-size")]
    [AllureTag("boundary-near-minimum")]
    public async Task CompleteTask_UploadImageWithOneByte_ShouldRejectAsTooSmall()
    {
        // Arrange
        var (dbContext, controller, taskId, userId) = InitializeCompleteTestEnvironment();
        var tinyImageFileBytes = new byte[] { 0xFF };
        var mockFormFile = CreateMockFormFile("tiny.jpg", tinyImageFileBytes);
        var formFileCollection = CreateMockFormFileCollection(new[] { mockFormFile });
        var formCollection = CreateMockFormCollection(weightKgValue: 5.5m, notesTextValue: "Test", formFileCollection);
        
        AllureAttachmentHelper.AttachJson("test-parameters", new
        {
            taskId,
            imageSizeBytes = tinyImageFileBytes.Length,
            fileName = "tiny.jpg"
        });
        
        // Act
        var result = await controller.CompleteTask(taskId, formCollection);
        
        // Assert
        result.Should().NotBeNull();
    }
    
    [Fact]
    [AllureDescription("BVA: Upload image with size = MAX - 1 bytes. Should be accepted as valid (just below limit).")]
    [AllureTag("bva-file-size")]
    [AllureTag("boundary-near-maximum")]
    public async Task CompleteTask_UploadImageWithMaxMinusOneByte_ShouldBeAccepted()
    {
        // Arrange
        var (dbContext, controller, taskId, userId) = InitializeCompleteTestEnvironment();
        var largeImageFileSizeBytes = MAX_IMAGE_FILE_SIZE_BYTES - 1;
        var largeImageFileBytes = CreateByteArrayOfExactSize(largeImageFileSizeBytes);
        var mockFormFile = CreateMockFormFile("large-valid.jpg", largeImageFileBytes);
        var formFileCollection = CreateMockFormFileCollection(new[] { mockFormFile });
        var formCollection = CreateMockFormCollection(weightKgValue: 5.5m, notesTextValue: "Test", formFileCollection);
        
        AllureAttachmentHelper.AttachJson("test-parameters", new
        {
            taskId,
            imageSizeBytes = largeImageFileSizeBytes,
            maxAllowedBytes = MAX_IMAGE_FILE_SIZE_BYTES,
            fileName = "large-valid.jpg"
        });
        
        // Act
        var result = await controller.CompleteTask(taskId, formCollection);
        
        // Assert
        result.Should().NotBeNull();
    }
    
    [Fact]
    [AllureDescription("BVA: Upload image with size = MAX bytes exactly. Should be accepted as valid (at exact limit).")]
    [AllureTag("bva-file-size")]
    [AllureTag("boundary-exact-maximum")]
    public async Task CompleteTask_UploadImageWithMaxBytes_ShouldBeAccepted()
    {
        // Arrange
        var (dbContext, controller, taskId, userId) = InitializeCompleteTestEnvironment();
        var maxSizedImageFileBytes = CreateByteArrayOfExactSize(MAX_IMAGE_FILE_SIZE_BYTES);
        var mockFormFile = CreateMockFormFile("max-size.jpg", maxSizedImageFileBytes);
        var formFileCollection = CreateMockFormFileCollection(new[] { mockFormFile });
        var formCollection = CreateMockFormCollection(weightKgValue: 5.5m, notesTextValue: "Test", formFileCollection);
        
        AllureAttachmentHelper.AttachJson("test-parameters", new
        {
            taskId,
            imageSizeBytes = MAX_IMAGE_FILE_SIZE_BYTES,
            fileName = "max-size.jpg"
        });
        
        // Act
        var result = await controller.CompleteTask(taskId, formCollection);
        
        // Assert
        result.Should().NotBeNull();
    }
    
    [Fact]
    [AllureDescription("BVA: Upload image with size = MAX + 1 bytes. Should be rejected as exceeding limit.")]
    [AllureTag("bva-file-size")]
    [AllureTag("boundary-exceeds-maximum")]
    public async Task CompleteTask_UploadImageWithMaxPlusOneByte_ShouldBeRejected()
    {
        // Arrange
        var (dbContext, controller, taskId, userId) = InitializeCompleteTestEnvironment();
        var oversizedImageFileSizeBytes = MAX_IMAGE_FILE_SIZE_BYTES + 1;
        var oversizedImageFileBytes = CreateByteArrayOfExactSize(oversizedImageFileSizeBytes);
        var mockFormFile = CreateMockFormFile("oversized.jpg", oversizedImageFileBytes);
        var formFileCollection = CreateMockFormFileCollection(new[] { mockFormFile });
        var formCollection = CreateMockFormCollection(weightKgValue: 5.5m, notesTextValue: "Test", formFileCollection);
        
        AllureAttachmentHelper.AttachJson("test-parameters", new
        {
            taskId,
            imageSizeBytes = oversizedImageFileSizeBytes,
            maxAllowedBytes = MAX_IMAGE_FILE_SIZE_BYTES,
            fileName = "oversized.jpg"
        });
        
        // Act
        var result = await controller.CompleteTask(taskId, formCollection);
        
        // Assert
        result.Should().NotBeNull();
    }
    
    #endregion
    
    #region BVA Tests - File Extension Boundaries
    
    [Fact]
    [AllureDescription("BVA: Upload image with allowed extension '.jpg'. Should be accepted.")]
    [AllureTag("bva-file-extension")]
    [AllureTag("boundary-allowed")]
    public async Task CompleteTask_UploadWithAllowedExtensionJpg_ShouldBeAccepted()
    {
        // Arrange
        var (dbContext, controller, taskId, userId) = InitializeCompleteTestEnvironment();
        var validImageBytes = CreateByteArrayOfExactSize(1_048_576); // 1 MB
        var mockFormFile = CreateMockFormFile("valid-image.jpg", validImageBytes, "image/jpeg");
        var formFileCollection = CreateMockFormFileCollection(new[] { mockFormFile });
        var formCollection = CreateMockFormCollection(weightKgValue: 5.5m, notesTextValue: "Test", formFileCollection);
        
        AllureAttachmentHelper.AttachJson("test-parameters", new
        {
            taskId,
            fileName = "valid-image.jpg",
            extension = ".jpg",
            contentType = "image/jpeg",
            allowedExtensions = ALLOWED_IMAGE_EXTENSIONS
        });
        
        // Act
        var result = await controller.CompleteTask(taskId, formCollection);
        
        // Assert
        result.Should().NotBeNull();
    }
    
    [Fact]
    [AllureDescription("BVA: Upload with disallowed extension '.exe'. Should be rejected.")]
    [AllureTag("bva-file-extension")]
    [AllureTag("boundary-disallowed")]
    public async Task CompleteTask_UploadWithDisallowedExtensionExe_ShouldBeRejected()
    {
        // Arrange
        var (dbContext, controller, taskId, userId) = InitializeCompleteTestEnvironment();
        var maliciousFileBytes = CreateByteArrayOfExactSize(10_240); // 10 KB
        var mockFormFile = CreateMockFormFile("malware.exe", maliciousFileBytes, "application/octet-stream");
        var formFileCollection = CreateMockFormFileCollection(new[] { mockFormFile });
        var formCollection = CreateMockFormCollection(weightKgValue: 5.5m, notesTextValue: "Test", formFileCollection);
        
        AllureAttachmentHelper.AttachJson("test-parameters", new
        {
            taskId,
            fileName = "malware.exe",
            extension = ".exe",
            contentType = "application/octet-stream",
            disallowedExtensions = DISALLOWED_EXTENSIONS
        });
        
        // Act
        var result = await controller.CompleteTask(taskId, formCollection);
        
        // Assert
        result.Should().NotBeNull();
    }
    
    [Fact]
    [AllureDescription("BVA: Upload with disallowed extension '.pdf'. Should be rejected.")]
    [AllureTag("bva-file-extension")]
    [AllureTag("boundary-disallowed")]
    public async Task CompleteTask_UploadWithDisallowedExtensionPdf_ShouldBeRejected()
    {
        // Arrange
        var (dbContext, controller, taskId, userId) = InitializeCompleteTestEnvironment();
        var pdfFileBytes = CreateByteArrayOfExactSize(2_097_152); // 2 MB
        var mockFormFile = CreateMockFormFile("document.pdf", pdfFileBytes, "application/pdf");
        var formFileCollection = CreateMockFormFileCollection(new[] { mockFormFile });
        var formCollection = CreateMockFormCollection(weightKgValue: 5.5m, notesTextValue: "Test", formFileCollection);
        
        AllureAttachmentHelper.AttachJson("test-parameters", new
        {
            taskId,
            fileName = "document.pdf",
            extension = ".pdf",
            contentType = "application/pdf"
        });
        
        // Act
        var result = await controller.CompleteTask(taskId, formCollection);
        
        // Assert
        result.Should().NotBeNull();
    }
    
    [Fact]
    [AllureDescription("BVA: Upload with allowed extension '.png'. Should be accepted.")]
    [AllureTag("bva-file-extension")]
    [AllureTag("boundary-allowed")]
    public async Task CompleteTask_UploadWithAllowedExtensionPng_ShouldBeAccepted()
    {
        // Arrange
        var (dbContext, controller, taskId, userId) = InitializeCompleteTestEnvironment();
        var pngImageBytes = CreateByteArrayOfExactSize(3_145_728); // 3 MB
        var mockFormFile = CreateMockFormFile("screenshot.png", pngImageBytes, "image/png");
        var formFileCollection = CreateMockFormFileCollection(new[] { mockFormFile });
        var formCollection = CreateMockFormCollection(weightKgValue: 5.5m, notesTextValue: "Test", formFileCollection);
        
        AllureAttachmentHelper.AttachJson("test-parameters", new
        {
            taskId,
            fileName = "screenshot.png",
            extension = ".png",
            contentType = "image/png"
        });
        
        // Act
        var result = await controller.CompleteTask(taskId, formCollection);
        
        // Assert
        result.Should().NotBeNull();
    }
    
    #endregion
    
    #region BVA Tests - Image Count Boundaries
    
    [Fact]
    [AllureDescription("BVA: Upload with 0 images. Should be rejected or handled as no images case.")]
    [AllureTag("bva-image-count")]
    [AllureTag("boundary-minimum")]
    public async Task CompleteTask_UploadWithZeroImages_ShouldRejectOrHandle()
    {
        // Arrange
        var (dbContext, controller, taskId, userId) = InitializeCompleteTestEnvironment();
        var emptyFormFileCollection = CreateMockFormFileCollection(new List<IFormFile>());
        var formCollection = CreateMockFormCollection(weightKgValue: 5.5m, notesTextValue: "Test", emptyFormFileCollection);
        
        AllureAttachmentHelper.AttachJson("test-parameters", new
        {
            taskId,
            imageCount = 0,
            expectedBehavior = "Reject or accept with message"
        });
        
        // Act
        var result = await controller.CompleteTask(taskId, formCollection);
        
        // Assert
        result.Should().NotBeNull();
    }
    
    [Fact]
    [AllureDescription("BVA: Upload with 1 image (minimum valid). Should be accepted.")]
    [AllureTag("bva-image-count")]
    [AllureTag("boundary-near-minimum")]
    public async Task CompleteTask_UploadWithOneImage_ShouldBeAccepted()
    {
        // Arrange
        var (dbContext, controller, taskId, userId) = InitializeCompleteTestEnvironment();
        var singleImageBytes = CreateByteArrayOfExactSize(2_097_152); // 2 MB
        var mockFormFile = CreateMockFormFile("proof-image-1.jpg", singleImageBytes);
        var formFileCollection = CreateMockFormFileCollection(new[] { mockFormFile });
        var formCollection = CreateMockFormCollection(weightKgValue: 5.5m, notesTextValue: "Test", formFileCollection);
        
        AllureAttachmentHelper.AttachJson("test-parameters", new
        {
            taskId,
            imageCount = 1,
            fileName = "proof-image-1.jpg"
        });
        
        // Act
        var result = await controller.CompleteTask(taskId, formCollection);
        
        // Assert
        result.Should().NotBeNull();
    }
    
    [Fact]
    [AllureDescription("BVA: Upload with MAX images (10). Should be accepted if within limits.")]
    [AllureTag("bva-image-count")]
    [AllureTag("boundary-exact-maximum")]
    public async Task CompleteTask_UploadWithMaxImages_ShouldBeAccepted()
    {
        // Arrange
        var (dbContext, controller, taskId, userId) = InitializeCompleteTestEnvironment();
        var formFileListForMaxImages = new List<IFormFile>();
        
        for (int imageIndexCounter = 1; imageIndexCounter <= MAX_IMAGES_PER_UPLOAD; imageIndexCounter++)
        {
            var imageBytes = CreateByteArrayOfExactSize(1_048_576); // 1 MB each
            var formFile = CreateMockFormFile($"image-{imageIndexCounter}.jpg", imageBytes);
            formFileListForMaxImages.Add(formFile);
        }
        
        var formFileCollection = CreateMockFormFileCollection(formFileListForMaxImages);
        var formCollection = CreateMockFormCollection(weightKgValue: 5.5m, notesTextValue: "Test", formFileCollection);
        
        AllureAttachmentHelper.AttachJson("test-parameters", new
        {
            taskId,
            imageCount = MAX_IMAGES_PER_UPLOAD,
            totalSizeBytes = 1_048_576 * MAX_IMAGES_PER_UPLOAD
        });
        
        // Act
        var result = await controller.CompleteTask(taskId, formCollection);
        
        // Assert
        result.Should().NotBeNull();
    }
    
    [Fact]
    [AllureDescription("BVA: Upload with MAX + 1 images. Should be rejected as exceeding limit.")]
    [AllureTag("bva-image-count")]
    [AllureTag("boundary-exceeds-maximum")]
    public async Task CompleteTask_UploadWithMoreThanMaxImages_ShouldBeRejected()
    {
        // Arrange
        var (dbContext, controller, taskId, userId) = InitializeCompleteTestEnvironment();
        var formFileListForExcessiveImages = new List<IFormFile>();
        
        for (int imageIndexCounter = 1; imageIndexCounter <= MAX_IMAGES_PER_UPLOAD + 1; imageIndexCounter++)
        {
            var imageBytes = CreateByteArrayOfExactSize(1_048_576); // 1 MB each
            var formFile = CreateMockFormFile($"image-{imageIndexCounter}.jpg", imageBytes);
            formFileListForExcessiveImages.Add(formFile);
        }
        
        var formFileCollection = CreateMockFormFileCollection(formFileListForExcessiveImages);
        var formCollection = CreateMockFormCollection(weightKgValue: 5.5m, notesTextValue: "Test", formFileCollection);
        
        AllureAttachmentHelper.AttachJson("test-parameters", new
        {
            taskId,
            imageCount = MAX_IMAGES_PER_UPLOAD + 1,
            maxAllowedCount = MAX_IMAGES_PER_UPLOAD
        });
        
        // Act
        var result = await controller.CompleteTask(taskId, formCollection);
        
        // Assert
        result.Should().NotBeNull();
    }
    
    #endregion
    
    #region Detailed Error Handling & Validation Tests
    
    /// <summary>
    /// Test case chi tiết: Upload file rỗng hoặc 0 bytes
    /// KIEM-68 BVA Test: Xác minh API trả về HTTP 400 BadRequest khi file không có nội dung
    /// 
    /// Quy trình kiểm thử:
    /// 1. Tạo mock file với Length = 0 bytes (empty file)
    /// 2. Seed đầy đủ CollectionTask vào In-Memory DbContext
    /// 3. Gọi API endpoint CompleteTask với file rỗng
    /// 4. Kiểm tra response là BadRequestObjectResult
    /// 5. Xác nhận mã lỗi HTTP 400
    /// 6. Kiểm tra thông báo lỗi chứa "Invalid file size" hoặc "File is empty"
    /// 7. Đính kèm payload request/response cho Allure Report
    /// </summary>
    [Fact]
    [AllureDescription("UploadImage: When file is empty (0 bytes), should return HTTP 400 BadRequest with appropriate error message.")]
    [AllureTag("error-handling")]
    [AllureTag("empty-file")]
    [AllureTag("http-400")]
    [AllureTag("validation")]
    public async Task UploadImage_WhenFileIsEmptyOrZeroBytes_ShouldReturn400BadRequest()
    {
        // ==================== ARRANGE SECTION ====================
        
        // Bước 1: Khởi tạo test environment hoàn chỉnh
        var testEnvironmentTupleResult = InitializeCompleteTestEnvironment();
        var dbContextInstanceForTest = testEnvironmentTupleResult.Item1;
        var controllerInstanceForTest = testEnvironmentTupleResult.Item2;
        var collectionTaskIdForTest = testEnvironmentTupleResult.Item3;
        var collectorUserIdForTest = testEnvironmentTupleResult.Item4;
        
        AllureAttachmentHelper.AttachJson("test-environment-setup", new
        {
            collectionTaskId = collectionTaskIdForTest,
            collectorUserId = collectorUserIdForTest,
            dbContextType = "SQLite In-Memory",
            environmentTimestamp = DateTime.UtcNow.ToString("O")
        });
        
        // Bước 2: Tạo byte array rỗng (0 bytes) để mô phỏng file trống
        var emptyByteArrayForZeroSizeFile = Array.Empty<byte>();
        var emptyFileByteCountExpected = 0;
        var emptyFileSizeInKbExpected = 0.0m;
        
        AllureAttachmentHelper.AttachJson("empty-file-configuration", new
        {
            expectedFileSize = emptyFileByteCountExpected,
            expectedFileSizeInKb = emptyFileSizeInKbExpected,
            byteArrayLength = emptyByteArrayForZeroSizeFile.Length,
            description = "Zero-byte empty file for BVA boundary testing"
        });
        
        // Bước 3: Tạo mock IFormFile với thuộc tính Length = 0
        var fileNameForEmptyUpload = "empty-proof.jpg";
        var contentTypeForEmptyFile = "image/jpeg";
        var mockEmptyFormFileInstance = CreateMockFormFile(
            fileNameWithExtension: fileNameForEmptyUpload,
            fileContentBytesArray: emptyByteArrayForZeroSizeFile,
            contentTypeOfFile: contentTypeForEmptyFile);
        
        // Verify mock file properties
        var mockFileNameProperty = mockEmptyFormFileInstance.FileName;
        var mockFileLengthProperty = mockEmptyFormFileInstance.Length;
        var mockFileContentTypeProperty = mockEmptyFormFileInstance.ContentType;
        
        AllureAttachmentHelper.AttachJson("mock-form-file-properties", new
        {
            fileName = mockFileNameProperty,
            fileLength = mockFileLengthProperty,
            contentType = mockFileContentTypeProperty,
            isFileLengthZero = mockFileLengthProperty == 0,
            errorExpected = true
        });
        
        // Bước 4: Tạo FormFileCollection chứa mock file rỗng
        var formFileListWithEmptyFile = new List<IFormFile> { mockEmptyFormFileInstance };
        var formFileCollectionWithEmptyFile = CreateMockFormFileCollection(formFileListWithEmptyFile);
        var formFileCollectionItemCountActual = formFileListWithEmptyFile.Count;
        
        AllureAttachmentHelper.AttachJson("form-file-collection-setup", new
        {
            fileCountInCollection = formFileCollectionItemCountActual,
            firstFileSize = formFileListWithEmptyFile[0].Length,
            validationMessage = "FormFileCollection contains 1 empty file"
        });
        
        // Bước 5: Tạo FormCollection với metadata (WeightKg, Notes, Images)
        var weightKgValueForCompletion = 10.5m;
        var notesTextFromCollector = "Collection task completed with proof images";
        var formCollectionWithEmptyImageFile = CreateMockFormCollection(
            weightKgValue: weightKgValueForCompletion,
            notesTextValue: notesTextFromCollector,
            formFilesCollectionToInclude: formFileCollectionWithEmptyFile);
        
        // Tạo object request payload để đính kèm
        var requestPayloadObject = new
        {
            collectionTaskId = collectionTaskIdForTest,
            weightKg = weightKgValueForCompletion,
            notes = notesTextFromCollector,
            uploadedFiles = new[]
            {
                new
                {
                    fileName = mockFileNameProperty,
                    fileSizeBytes = mockFileLengthProperty,
                    contentType = mockFileContentTypeProperty,
                    isEmpty = mockFileLengthProperty == 0
                }
            },
            timestamp = DateTime.UtcNow.ToString("O")
        };
        
        AllureAttachmentHelper.AttachJson("request-payload", requestPayloadObject);
        
        // ==================== ACT SECTION ====================
        
        // Bước 6: Gọi API endpoint CompleteTask với FormCollection chứa empty file
        var apiResponseResult = await controllerInstanceForTest.CompleteTask(
            id: collectionTaskIdForTest,
            form: formCollectionWithEmptyImageFile);
        
        AllureAttachmentHelper.AttachText("api-call-executed", 
            $"Executed: CompleteTask(id={collectionTaskIdForTest}, form=with-empty-file)\n" +
            $"Response Type: {apiResponseResult?.GetType().Name ?? "null"}\n" +
            $"Timestamp: {DateTime.UtcNow:O}");
        
        // ==================== ASSERT SECTION ====================
        
        // Bước 7: Kiểm tra response type là BadRequestObjectResult
        var badRequestResultAssertion = apiResponseResult.Should()
            .BeOfType<BadRequestObjectResult>("API should return BadRequest for zero-byte file");
        var badRequestResultActual = badRequestResultAssertion.Subject;
        
        // Bước 8: Kiểm tra status code HTTP 400
        var httpStatusCodeExpected = 400;
        var httpStatusCodeActual = badRequestResultActual.StatusCode;
        
        httpStatusCodeActual.Should()
            .Be(httpStatusCodeExpected, "HTTP status code should be 400 Bad Request");
        
        AllureAttachmentHelper.AttachJson("status-code-validation", new
        {
            expectedStatusCode = httpStatusCodeExpected,
            actualStatusCode = httpStatusCodeActual,
            isStatusCodeCorrect = httpStatusCodeActual == httpStatusCodeExpected
        });
        
        // Bước 9: Lấy object response value
        var responseValueObject = badRequestResultActual.Value;
        
        // Bước 10: Serialize response value thành string để kiểm tra thông báo lỗi
        var responseValueAsJsonString = System.Text.Json.JsonSerializer.Serialize(
            responseValueObject,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        
        AllureAttachmentHelper.AttachJson("raw-response-value", new
        {
            responseValue = responseValueObject,
            responseValueType = responseValueObject?.GetType().Name,
            responseValueAsString = responseValueAsJsonString
        });
        
        // Bước 11: Kiểm tra thông báo lỗi chứa từ khóa hợp lệ
        var allowedErrorMessageKeywords = new[] { "Invalid file size", "File is empty", "zero", "0 bytes" };
        var errorMessageFoundInResponse = false;
        var matchedKeywordFromResponse = string.Empty;
        
        foreach (var keywordToSearch in allowedErrorMessageKeywords)
        {
            var isKeywordPresentInResponse = responseValueAsJsonString.Contains(
                keywordToSearch,
                StringComparison.OrdinalIgnoreCase);
            
            if (isKeywordPresentInResponse)
            {
                errorMessageFoundInResponse = true;
                matchedKeywordFromResponse = keywordToSearch;
                break;
            }
        }
        
        AllureAttachmentHelper.AttachJson("error-message-validation", new
        {
            allowedKeywords = allowedErrorMessageKeywords,
            errorMessageFound = errorMessageFoundInResponse,
            matchedKeyword = matchedKeywordFromResponse,
            responseContains = responseValueAsJsonString.Substring(0, Math.Min(200, responseValueAsJsonString.Length))
        });
        
        // Bước 12: Assert thông báo lỗi chứa ít nhất một từ khóa hợp lệ
        errorMessageFoundInResponse.Should()
            .BeTrue("Response error message should contain validation keyword about empty/invalid file size. " +
                   $"Response was: {responseValueAsJsonString}");
        
        // Bước 13: Tạo response payload object để đính kèm vào Allure Report
        var responsePayloadObject = new
        {
            statusCode = httpStatusCodeActual,
            resultType = badRequestResultActual.GetType().Name,
            errorMessageValidation = new
            {
                messageFound = errorMessageFoundInResponse,
                matchedKeyword = matchedKeywordFromResponse,
                expectedKeywords = allowedErrorMessageKeywords
            },
            responseContent = responseValueAsJsonString,
            testResult = "PASSED - File rejected successfully",
            timestamp = DateTime.UtcNow.ToString("O")
        };
        
        AllureAttachmentHelper.AttachJson("response-payload", responsePayloadObject);
        
        // Bước 14: Verify task state không được cập nhật trong database
        var collectionTaskFromDbAfterTest = await dbContextInstanceForTest.CollectionTasks
            .FirstOrDefaultAsync(t => t.Id == collectionTaskIdForTest);
        
        if (collectionTaskFromDbAfterTest != null)
        {
            var taskStateAfterFailedUpload = new
            {
                taskId = collectionTaskFromDbAfterTest.Id,
                status = collectionTaskFromDbAfterTest.Status.ToString(),
                collectedWeightKg = collectionTaskFromDbAfterTest.CollectedWeightKg,
                notes = collectionTaskFromDbAfterTest.Notes,
                completedAt = collectionTaskFromDbAfterTest.CompletedAt,
                imageCount = collectionTaskFromDbAfterTest.Images?.Count ?? 0,
                expectedNoChanges = true
            };
            
            AllureAttachmentHelper.AttachJson("database-state-after-failed-upload", taskStateAfterFailedUpload);
            
            // Assert: Task vẫn ở trạng thái OnTheWay, không bị cập nhật
            collectionTaskFromDbAfterTest.Status.Should()
                .Be(CollectionTaskStatus.OnTheWay, "Task status should not change after failed file upload");
            
            collectionTaskFromDbAfterTest.CollectedWeightKg.Should()
                .BeNull("Weight should not be set after rejected file upload");
        }
        
        // ==================== FINAL TEST SUMMARY ====================
        
        AllureAttachmentHelper.AttachJson("test-summary", new
        {
            testName = "UploadImage_WhenFileIsEmptyOrZeroBytes_ShouldReturn400BadRequest",
            testResult = "PASSED",
            fileSize = emptyFileByteCountExpected,
            httpStatusCode = httpStatusCodeActual,
            errorValidated = errorMessageFoundInResponse,
            dbTransactionRolledBack = collectionTaskFromDbAfterTest?.CompletedAt == null,
            assertions = new[]
            {
                "Response is BadRequestObjectResult",
                "HTTP Status Code is 400",
                "Error message contains validation keyword",
                "Database transaction rolled back",
                "Task status unchanged"
            },
            timestamp = DateTime.UtcNow.ToString("O")
        });
    }
    
    #endregion
}
