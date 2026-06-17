using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Allure.Net.Commons;
using Allure.Xunit.Attributes;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WastePlatform.API.Controllers;
using WastePlatform.Application.Reports.Commands;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;
using Xunit;
using WastePlatform.Tests.TestSupport;


namespace WastePlatform.Tests.Controllers
{
    [AllureEpic("KIEM-5: Reports Module Testing")]
    [AllureFeature("Report Controller")]
    [Allure.Net.Commons.Attributes.AllureLabel("story", "Create, retrieve and manage waste reports")]
    [Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
    [Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
    [Allure.Net.Commons.Attributes.AllureLabel("subSuite", "ReportControllerTests")]
    [Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
    [AllureOwner("Nguyen Minh Phung")]
    [AllureSeverity(SeverityLevel.critical)]
    [Allure.Net.Commons.Attributes.AllureTag("api")]
    [Allure.Net.Commons.Attributes.AllureTag("reports")]
    [Allure.Net.Commons.Attributes.AllureIssue("https://ut-team-36.atlassian.net/browse/KIEM-5")]
    public class ValidationBvaEpTests
    {
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IFileStorageService> _mockFileStorageService;

        public ValidationBvaEpTests()
        {
            _mockNotificationService = new Mock<INotificationService>();
            _mockFileStorageService = new Mock<IFileStorageService>();
        }

        private static WastePlatformDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<WastePlatformDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            return new WastePlatformDbContext(options);
        }

        private static ControllerContext BuildControllerContext(Guid userId, string role)
        {
            return new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                            new Claim(ClaimTypes.Role, role)
                        },
                        "TestAuth"))
                }
            };
        }

        #region Module Báo cáo rác (Report Coordinates & Images) BVA & EP Tests

        [Fact]
        [AllureDescription("Report Lat BVA-1: Min valid Latitude (-90) should be accepted.")]
        public async Task CreateReport_WithMinLatitudeBoundary_ShouldSucceed()
        {
            await using var context = CreateContext();
            var category = new WasteCategory { Id = 1, Name = "Plastic" };
            context.WasteCategories.Add(category);
            await context.SaveChangesAsync();

            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateReportCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid());

            var controller = new ReportController(mediatorMock.Object, context, _mockNotificationService.Object)
            {
                ControllerContext = BuildControllerContext(Guid.NewGuid(), "Citizen")
            };

            var form = new FormCollection(
                new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
                {
                    ["WasteCategoryId"] = "1",
                    ["Latitude"] = "-90",
                    ["Longitude"] = "100"
                },
                new FormFileCollection());

            var result = await controller.CreateReport(form);

            AllureAttachmentHelper.AttachText("bva-result", "Boundary accepted: result type validated");
            result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        [AllureDescription("Report Lat BVA-2: Max valid Latitude (90) should be accepted.")]
        public async Task CreateReport_WithMaxLatitudeBoundary_ShouldSucceed()
        {
            await using var context = CreateContext();

            var category = new WasteCategory { Id = 1, Name = "Plastic" };
            context.WasteCategories.Add(category);
            await context.SaveChangesAsync();

            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateReportCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid());

            var controller = new ReportController(mediatorMock.Object, context, _mockNotificationService.Object)
            {
                ControllerContext = BuildControllerContext(Guid.NewGuid(), "Citizen")
            };

            var form = new FormCollection(
                new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
                {
                    ["WasteCategoryId"] = "1",
                    ["Latitude"] = "90",
                    ["Longitude"] = "100"
                },
                new FormFileCollection());

            var result = await controller.CreateReport(form);

            AllureAttachmentHelper.AttachText("bva-result", "Boundary accepted: result type validated");
            result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        [AllureDescription("Report Lat BVA-3: Latitude exceeding min (-90.01) must throw ArgumentException.")]
        public async Task CreateReport_WithLatitudeExceedingMin_ShouldThrowArgumentException()
        {
            var category = new WasteCategory { Id = 1, Name = "Plastic" };
            var mockCatRepo = new Mock<IWasteCategoryRepository>();
            mockCatRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);

            var handler = new CreateReportCommandHandler(
                new Mock<IReportRepository>().Object,
                mockCatRepo.Object,
                _mockFileStorageService.Object);

            var cmd = new CreateReportCommand
            {
                CitizenId = Guid.NewGuid(),
                WasteCategoryId = 1,
                Latitude = -90.01m, // Exceeds boundary
                Longitude = 100m,
                Images = CreateMockImageCollection("test.jpg")
            };

            var act = () => handler.Handle(cmd, CancellationToken.None);

            AllureAttachmentHelper.AttachText("bva-exception", "Boundary violated: ArgumentException expected");
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Invalid latitude or longitude coordinates");
        }

        [Fact]
        [AllureDescription("Report Lat BVA-4: Latitude exceeding max (90.01) must throw ArgumentException.")]
        public async Task CreateReport_WithLatitudeExceedingMax_ShouldThrowArgumentException()
        {
            var category = new WasteCategory { Id = 1, Name = "Plastic" };
            var mockCatRepo = new Mock<IWasteCategoryRepository>();
            mockCatRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);

            var handler = new CreateReportCommandHandler(
                new Mock<IReportRepository>().Object,
                mockCatRepo.Object,
                _mockFileStorageService.Object);

            var cmd = new CreateReportCommand
            {
                CitizenId = Guid.NewGuid(),
                WasteCategoryId = 1,
                Latitude = 90.01m, // Exceeds boundary
                Longitude = 100m,
                Images = CreateMockImageCollection("test.jpg")
            };

            var act = () => handler.Handle(cmd, CancellationToken.None);

            AllureAttachmentHelper.AttachText("bva-exception", "Boundary violated: ArgumentException expected");
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Invalid latitude or longitude coordinates");
        }

        [Fact]
        [AllureDescription("Report Lng BVA-1: Min valid Longitude (-180) should be accepted.")]
        public async Task CreateReport_WithMinLongitudeBoundary_ShouldSucceed()
        {
            await using var context = CreateContext();

            var category = new WasteCategory { Id = 1, Name = "Plastic" };
            context.WasteCategories.Add(category);
            await context.SaveChangesAsync();

            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateReportCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid());

            var controller = new ReportController(mediatorMock.Object, context, _mockNotificationService.Object)
            {
                ControllerContext = BuildControllerContext(Guid.NewGuid(), "Citizen")
            };

            var form = new FormCollection(
                new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
                {
                    ["WasteCategoryId"] = "1",
                    ["Latitude"] = "10.77",
                    ["Longitude"] = "-180"
                },
                new FormFileCollection());

            var result = await controller.CreateReport(form);

            AllureAttachmentHelper.AttachText("bva-result", "Boundary accepted: result type validated");
            result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        [AllureDescription("Report Lng BVA-2: Max valid Longitude (180) should be accepted.")]
        public async Task CreateReport_WithMaxLongitudeBoundary_ShouldSucceed()
        {
            await using var context = CreateContext();

            var category = new WasteCategory { Id = 1, Name = "Plastic" };
            context.WasteCategories.Add(category);
            await context.SaveChangesAsync();

            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateReportCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid());

            var controller = new ReportController(mediatorMock.Object, context, _mockNotificationService.Object)
            {
                ControllerContext = BuildControllerContext(Guid.NewGuid(), "Citizen")
            };

            var form = new FormCollection(
                new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
                {
                    ["WasteCategoryId"] = "1",
                    ["Latitude"] = "10.77",
                    ["Longitude"] = "180"
                },
                new FormFileCollection());

            var result = await controller.CreateReport(form);

            AllureAttachmentHelper.AttachText("bva-result", "Boundary accepted: result type validated");
            result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        [AllureDescription("Report Lng BVA-3: Longitude exceeding min (-180.01) must throw ArgumentException.")]
        public async Task CreateReport_WithLongitudeExceedingMin_ShouldThrowArgumentException()
        {
            var category = new WasteCategory { Id = 1, Name = "Plastic" };
            var mockCatRepo = new Mock<IWasteCategoryRepository>();
            mockCatRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);

            var handler = new CreateReportCommandHandler(
                new Mock<IReportRepository>().Object,
                mockCatRepo.Object,
                _mockFileStorageService.Object);

            var cmd = new CreateReportCommand
            {
                CitizenId = Guid.NewGuid(),
                WasteCategoryId = 1,
                Latitude = 10.77m,
                Longitude = -180.01m, // Exceeds boundary
                Images = CreateMockImageCollection("test.jpg")
            };

            var act = () => handler.Handle(cmd, CancellationToken.None);

            AllureAttachmentHelper.AttachText("bva-exception", "Boundary violated: ArgumentException expected");
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Invalid latitude or longitude coordinates");
        }

        [Fact]
        [AllureDescription("Report Lng BVA-4: Longitude exceeding max (180.01) must throw ArgumentException.")]
        public async Task CreateReport_WithLongitudeExceedingMax_ShouldThrowArgumentException()
        {
            var category = new WasteCategory { Id = 1, Name = "Plastic" };
            var mockCatRepo = new Mock<IWasteCategoryRepository>();
            mockCatRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);

            var handler = new CreateReportCommandHandler(
                new Mock<IReportRepository>().Object,
                mockCatRepo.Object,
                _mockFileStorageService.Object);

            var cmd = new CreateReportCommand
            {
                CitizenId = Guid.NewGuid(),
                WasteCategoryId = 1,
                Latitude = 10.77m,
                Longitude = 180.01m, // Exceeds boundary
                Images = CreateMockImageCollection("test.jpg")
            };

            var act = () => handler.Handle(cmd, CancellationToken.None);

            AllureAttachmentHelper.AttachText("bva-exception", "Boundary violated: ArgumentException expected");
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Invalid latitude or longitude coordinates");
        }

        [Fact]
        [AllureDescription("Report Image count BVA-1: Zero images (boundary) must throw ArgumentException (At least one image is required).")]
        public async Task CreateReport_WithZeroImages_ShouldThrowArgumentException()
        {
            var category = new WasteCategory { Id = 1, Name = "Plastic" };
            var mockCatRepo = new Mock<IWasteCategoryRepository>();
            mockCatRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);

            var handler = new CreateReportCommandHandler(
                new Mock<IReportRepository>().Object,
                mockCatRepo.Object,
                _mockFileStorageService.Object);

            var cmd = new CreateReportCommand
            {
                CitizenId = Guid.NewGuid(),
                WasteCategoryId = 1,
                Latitude = 10.77m,
                Longitude = 106.7m,
                Images = new FormFileCollection() // 0 images
            };

            var act = () => handler.Handle(cmd, CancellationToken.None);

            AllureAttachmentHelper.AttachText("bva-exception", "Boundary violated: ArgumentException expected");
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("At least one image is required");
        }

        [Fact]
        [AllureDescription("Report Image count BVA-2: Max images (5 images boundary) should succeed.")]
        public async Task CreateReport_WithFiveImages_ShouldSucceed()
        {
            var category = new WasteCategory { Id = 1, Name = "Plastic" };
            var mockCatRepo = new Mock<IWasteCategoryRepository>();
            mockCatRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);

            var mockReportRepo = new Mock<IReportRepository>();
            mockReportRepo.Setup(r => r.AddAsync(It.IsAny<WasteReport>(), It.IsAny<CancellationToken>())).ReturnsAsync((WasteReport r, CancellationToken _) => r);

            var handler = new CreateReportCommandHandler(
                mockReportRepo.Object,
                mockCatRepo.Object,
                _mockFileStorageService.Object);

            var images = new FormFileCollection();
            for (int i = 0; i < 5; i++)
            {
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.FileName).Returns($"img{i}.jpg");
                mockFile.Setup(f => f.Length).Returns(1024);
                images.Add(mockFile.Object);
            }

            var cmd = new CreateReportCommand
            {
                CitizenId = Guid.NewGuid(),
                WasteCategoryId = 1,
                Latitude = 10.77m,
                Longitude = 106.7m,
                Images = images
            };

            var result = await handler.Handle(cmd, CancellationToken.None);

            AllureAttachmentHelper.AttachText("bva-result", "Boundary accepted: result type validated");
            result.Should().NotBe(Guid.Empty);
        }

        [Fact]
        [AllureDescription("Report Image count BVA-3: Six images (boundary exceeding max) must throw ArgumentException.")]
        public async Task CreateReport_WithSixImages_ShouldThrowArgumentException()
        {
            var category = new WasteCategory { Id = 1, Name = "Plastic" };
            var mockCatRepo = new Mock<IWasteCategoryRepository>();
            mockCatRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);

            var handler = new CreateReportCommandHandler(
                new Mock<IReportRepository>().Object,
                mockCatRepo.Object,
                _mockFileStorageService.Object);

            var images = new FormFileCollection();
            for (int i = 0; i < 6; i++)
            {
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.FileName).Returns($"img{i}.jpg");
                mockFile.Setup(f => f.Length).Returns(1024);
                images.Add(mockFile.Object);
            }

            var cmd = new CreateReportCommand
            {
                CitizenId = Guid.NewGuid(),
                WasteCategoryId = 1,
                Latitude = 10.77m,
                Longitude = 106.7m,
                Images = images
            };

            var act = () => handler.Handle(cmd, CancellationToken.None);

            AllureAttachmentHelper.AttachText("bva-exception", "Boundary violated: ArgumentException expected");
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Maximum 5 images are allowed");
        }

        #endregion

        #region Helper Setup Methods

        private static FormFileCollection CreateMockImageCollection(string fileName)
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(1024);
            return new FormFileCollection { fileMock.Object };
        }

        #endregion
    }
}




