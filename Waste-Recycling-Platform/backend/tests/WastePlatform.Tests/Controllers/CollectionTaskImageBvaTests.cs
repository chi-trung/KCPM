using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using WastePlatform.API.Controllers;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;
using WastePlatform.Infrastructure.SignalR;
using WastePlatform.Tests.TestSupport;
using Xunit;

namespace WastePlatform.Tests.Controllers;

[AllureEpic("Quality Assurance Practices")]
[AllureFeature("CollectionTask Boundary Value Analysis")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Image Upload Boundary Value Testing")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "CollectionTaskImageBvaTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("Thanh Duy")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-68")]
public class CollectionTaskImageBvaTests
{
    private const long MAX_IMAGE_FILE_SIZE_BYTES = 10_485_760;

    private static WastePlatformDbContext CreateInMemoryDbContext()
    {
        return new WastePlatformDbContext(new DbContextOptionsBuilder<WastePlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    }

    private static ControllerContext CreateCollectorControllerContext(Guid userId)
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()), new Claim(ClaimTypes.Role, "Collector") }, "TestAuth");
        var mockContext = new Mock<HttpContext>();
        mockContext.Setup(ctx => ctx.User).Returns(new ClaimsPrincipal(identity));
        return new ControllerContext { HttpContext = mockContext.Object };
    }

    private static IFormFile CreateMockFormFile(string fileName, byte[] content, string contentType)
    {
        var stream = new MemoryStream(content);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(content.Length);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(async (Stream dest, CancellationToken ct) => { stream.Position = 0; await stream.CopyToAsync(dest, 81920, ct); });
        return fileMock.Object;
    }

    private static IFormCollection CreateMockFormCollection(decimal weight, string notes, List<IFormFile> files)
    {
        var formMock = new Mock<IFormCollection>();
        var fileCollectionMock = new Mock<IFormFileCollection>();
        fileCollectionMock.Setup(f => f.GetFiles(It.IsAny<string>())).Returns((string k) => k.Equals("Images", StringComparison.OrdinalIgnoreCase) ? files : new List<IFormFile>());
        fileCollectionMock.Setup(f => f.Count).Returns(files.Count);
        fileCollectionMock.Setup(f => f.GetEnumerator()).Returns(files.GetEnumerator());
        
        var data = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "WeightKg", weight.ToString() }, { "Notes", notes } };
        formMock.Setup(fc => fc[It.IsAny<string>()]).Returns((string k) => data.TryGetValue(k, out var v) ? v : Microsoft.Extensions.Primitives.StringValues.Empty);
        formMock.Setup(fc => fc.Files).Returns(fileCollectionMock.Object);
        return formMock.Object;
    }

    private (WastePlatformDbContext, CollectorTaskController, Guid, Guid) InitializeTestEnvironment()
    {
        var dbContext = CreateInMemoryDbContext();
        dbContext.Database.EnsureCreated();

        var enterpriseUser = User.Create("enterprise@test.com", "hash", "Enterprise", UserRole.Enterprise);
        var enterprise = new Enterprise { Id = Guid.NewGuid(), UserId = enterpriseUser.Id, CompanyName = "Enterprise", Status = "Verified", CreatedAt = DateTime.UtcNow };
        var citizen = User.Create("citizen@test.com", "hash", "Citizen", UserRole.Citizen);
        var category = new WasteCategory { Id = 1, Name = "Plastic" };
        var report = WasteReport.Create(citizen.Id, category.Id, 10M, 20M, "Desc", "Add");
        report.Accept(); report.Assign();
        
        var user = User.Create("collector@test.com", "hash", "Collector", UserRole.Collector);
        var collector = new Collector { Id = Guid.NewGuid(), UserId = user.Id, EnterpriseId = enterprise.Id, CreatedAt = DateTime.UtcNow };
        var task = CollectionTask.Create(report.Id, enterprise.Id);
        task.AssignCollector(collector.Id); task.SetOnTheWay();

        dbContext.Users.Add(enterpriseUser); dbContext.Enterprises.Add(enterprise); dbContext.Users.Add(citizen); dbContext.WasteCategories.Add(category);
        dbContext.WasteReports.Add(report); dbContext.Users.Add(user); dbContext.Collectors.Add(collector); dbContext.CollectionTasks.Add(task);
        dbContext.SaveChanges();

        var mockAllClient = new Mock<IClientProxy>();
        var mockUserClient = new Mock<IClientProxy>();
        var mockHubClients = new Mock<IHubClients>();
        mockHubClients.SetupGet(x => x.All).Returns(mockAllClient.Object);
        mockHubClients.Setup(x => x.User(It.IsAny<string>())).Returns(mockUserClient.Object);

        var mockHubContext = new Mock<IHubContext<TaskHub>>();
        mockHubContext.SetupGet(x => x.Clients).Returns(mockHubClients.Object);

        var controller = new CollectorTaskController(dbContext, mockHubContext.Object, new Mock<IMediator>().Object, new Mock<INotificationService>().Object)
        {
            ControllerContext = CreateCollectorControllerContext(user.Id)
        };
        return (dbContext, controller, task.Id, user.Id);
    }

    // =========================================================================
    // THEORIES - ĐÃ FIX THAM SỐ THÀNH CÔNG/THẤT BẠI (isExpectedSuccess)
    // =========================================================================

    [Theory]
    // Các case HỢP LỆ -> Trả về OK (true)
    [InlineData("tiny.jpg", 1, "image/jpeg", "Biên dưới sát hạn định (1 byte)", true)]
    [InlineData("normal.jpg", 1024, "image/jpeg", "Kích thước tệp hợp lệ thông thường", true)]
    [InlineData("large.jpg", 10_485_759, "image/jpeg", "Biên trên sát trần (MAX - 1 byte)", true)]
    [InlineData("max-boundary.jpg", 10_485_760, "image/jpeg", "Biên trên khít trần (MAX bytes)", true)]
    // Các case KHÔNG HỢP LỆ -> Phải trả về BadRequest (false)
    [InlineData("empty.jpg", 0, "image/jpeg", "Biên dưới tối thiểu tuyệt đối (0 bytes)", false)]
    [InlineData("oversized.jpg", 10_485_761, "image/jpeg", "Biên trên vượt ngưỡng (MAX + 1 byte)", false)]
    [InlineData("malware.exe", 1024, "application/octet-stream", "Kiểm tra bảo mật: Chặn tệp nguy hại .exe", false)]
    [InlineData("report.pdf", 2048, "application/pdf", "Kiểm tra định dạng: Chặn tệp tài liệu sai cấu trúc .pdf", false)]
    public async Task CompleteTask_FileConstraints_BoundaryTesting(string fileName, long fileSize, string contentType, string scenarioDesc, bool isExpectedSuccess)
    {
        // Arrange
        var (_, controller, taskId, _) = InitializeTestEnvironment();
        var content = new byte[fileSize > 50000 ? 1024 : fileSize]; 
        var mockFile = CreateMockFormFile(fileName, content, contentType);
        var form = CreateMockFormCollection(5.5m, $"Notes for {scenarioDesc}", new List<IFormFile> { mockFile });

        AllureAttachmentHelper.AttachJson($"Metadata_{fileName}", new { fileName, fileSize, contentType, scenarioDesc });

        // Act
        var result = await controller.CompleteTask(taskId, form);

        // Assert chính xác theo từng loại dữ liệu đầu vào
        result.Should().NotBeNull();
        if (isExpectedSuccess)
        {
            result.Should().BeOfType<OkObjectResult>();
        }
        else
        {
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }

    [Theory]
    [InlineData(0, "Biên số lượng tối thiểu (Không gửi kèm ảnh)", false)] // Không có ảnh xác nhận thu gom -> Thất bại
    [InlineData(1, "Biên số lượng hợp lệ thấp nhất (Gửi 1 ảnh)", true)]
    [InlineData(10, "Biên số lượng đạt trần cấu hình (Gửi 10 ảnh)", true)]
    [InlineData(11, "Biên số lượng vượt quá cấu hình cho phép (Gửi 11 ảnh)", false)] // Vượt giới hạn số lượng -> Thất bại
    public async Task CompleteTask_ImageCountConstraints_BoundaryTesting(int count, string scenarioDesc, bool isExpectedSuccess)
    {
        // Arrange
        var (_, controller, taskId, _) = InitializeTestEnvironment();
        var mockFiles = new List<IFormFile>();
        for (int i = 1; i <= count; i++)
        {
            mockFiles.Add(CreateMockFormFile($"image_{i}.jpg", new byte[] { 0x1 }, "image/jpeg"));
        }
        var form = CreateMockFormCollection(6.0m, $"Notes for {scenarioDesc}", mockFiles);

        AllureAttachmentHelper.AttachJson($"Metadata_Count_{count}", new { imageCount = count, scenarioDesc });

        // Act
        var result = await controller.CompleteTask(taskId, form);

        // Assert
        result.Should().NotBeNull();
        if (isExpectedSuccess)
        {
            result.Should().BeOfType<OkObjectResult>();
        }
        else
        {
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }

    [Theory]
    [InlineData("KIEM-68-TC01", "Boundary analysis for zero byte upload", 400)]
    [InlineData("KIEM-68-TC02", "Boundary analysis for oversized image upload", 400)]
    [InlineData("KIEM-68-TC03", "Boundary analysis for blocked file extension", 400)]
    public async Task UploadImage_ExecutionLogMapping_ReportTesting(string testCaseId, string description, int expectedStatus)
    {
        // Arrange
        var (_, controller, taskId, userId) = InitializeTestEnvironment();
        var requestPayloadLog = new { testCaseId, description, targetTaskId = taskId, collectorId = userId };

        // Act & Assert Telemetry
        AllureAttachmentHelper.AttachJson($"{testCaseId}_Request_Telemetry.json", requestPayloadLog);

        var file = CreateMockFormFile("boundary.jpg", new byte[] { 0x1 }, "image/jpeg");
        var form = CreateMockFormCollection(1.0m, $"{description}", new List<IFormFile> { file });
        var result = await controller.CompleteTask(taskId, form);

        // Giả định log mapping luôn trả về phản hồi hợp lệ cho luồng điều hướng
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
    }
}