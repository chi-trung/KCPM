using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WastePlatform.API.Controllers;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using Xunit;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;

namespace WastePlatform.Tests.Controllers;

[AllureEpic("Support Modules")]
[AllureFeature("Notifications")]
public class NotificationControllerTests
{
    private readonly Mock<INotificationRepository> _mockNotificationRepository;
    private readonly NotificationController _controller;

    public NotificationControllerTests()
    {
        _mockNotificationRepository = new Mock<INotificationRepository>();
        _controller = new NotificationController(_mockNotificationRepository.Object);
    }

    [AllureStory("List notifications for citizen")]
    [AllureSeverity(SeverityLevel.normal)]
    [AllureOwner("chi-trung")]
    [Fact]
    public async Task GetNotifications_WithValidCitizen_ShouldReturnPagedNotificationsAndUnreadCount()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var notifications = new List<Notification>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CitizenId = citizenId,
                Type = NotificationType.ReportCreated,
                Channel = NotificationChannel.InApp,
                Status = NotificationStatus.Unread,
                Title = "Test title",
                Message = "Test message",
                ActionUrl = "/citizen/reports/1",
                RelatedEntityId = Guid.NewGuid(),
                RelatedEntityType = "Report",
                CreatedAt = DateTime.UtcNow
            }
        };

        _controller.ControllerContext = BuildControllerContext(citizenId);

        _mockNotificationRepository
            .Setup(x => x.GetByCitizenIdAsync(citizenId, 2, 10, NotificationStatus.Unread, It.IsAny<CancellationToken>()))
            .ReturnsAsync((notifications, 1));

        _mockNotificationRepository
            .Setup(x => x.GetUnreadCountAsync(citizenId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.GetNotifications(page: 2, pageSize: 10, status: "Unread");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value!;

        GetPropertyValue<string>(value, "message").Should().Be("Notifications retrieved successfully");
        GetPropertyValue<int>(value, "unreadCount").Should().Be(3);

        var pagination = GetPropertyValue<object>(value, "pagination");
        GetPropertyValue<int>(pagination!, "page").Should().Be(2);
        GetPropertyValue<int>(pagination!, "pageSize").Should().Be(10);
        GetPropertyValue<int>(pagination!, "total").Should().Be(1);
        GetPropertyValue<int>(pagination!, "totalPages").Should().Be(1);

        _mockNotificationRepository.Verify(
            x => x.GetByCitizenIdAsync(citizenId, 2, 10, NotificationStatus.Unread, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockNotificationRepository.Verify(
            x => x.GetUnreadCountAsync(citizenId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetNotifications_WithMissingCitizenId_ShouldReturnUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.GetNotifications();

        // Assert
        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        GetPropertyValue<string>(unauthorized.Value!, "message").Should().Be("Invalid or missing user ID");
    }

    [Fact]
    public async Task GetNotifications_WithInvalidPaging_ShouldReturnBadRequest()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        _controller.ControllerContext = BuildControllerContext(citizenId);

        // Act
        var result = await _controller.GetNotifications(page: 0, pageSize: 10);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        GetPropertyValue<string>(badRequest.Value!, "message").Should().Be("Page and PageSize must be greater than 0");
        _mockNotificationRepository.Verify(
            x => x.GetByCitizenIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<NotificationStatus?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task MarkAsRead_WhenNotificationExists_ShouldReturnOkAndSaveChanges()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        _controller.ControllerContext = BuildControllerContext(citizenId);

        _mockNotificationRepository
            .Setup(x => x.MarkAsReadAsync(notificationId, citizenId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockNotificationRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.MarkAsRead(notificationId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockNotificationRepository.Verify(
            x => x.MarkAsReadAsync(notificationId, citizenId, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockNotificationRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MarkAsRead_WhenNotificationDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        _controller.ControllerContext = BuildControllerContext(citizenId);

        _mockNotificationRepository
            .Setup(x => x.MarkAsReadAsync(notificationId, citizenId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.MarkAsRead(notificationId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockNotificationRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task MarkAllAsRead_ShouldReturnOkAndPersistChanges()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        _controller.ControllerContext = BuildControllerContext(citizenId);

        _mockNotificationRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.MarkAllAsRead();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockNotificationRepository.Verify(
            x => x.MarkAllAsReadAsync(citizenId, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockNotificationRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUnreadCount_ShouldReturnUnreadCount()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        _controller.ControllerContext = BuildControllerContext(citizenId);

        _mockNotificationRepository
            .Setup(x => x.GetUnreadCountAsync(citizenId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(7);

        // Act
        var result = await _controller.GetUnreadCount();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        GetPropertyValue<string>(okResult.Value!, "message").Should().Be("Unread count retrieved successfully");
        GetPropertyValue<int>(okResult.Value!, "unreadCount").Should().Be(7);
    }

    private static ControllerContext BuildControllerContext(Guid citizenId)
    {
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, citizenId.ToString())],
                    "TestAuth"))
            }
        };
    }

    private static T? GetPropertyValue<T>(object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(propertyName);
        if (property is null)
        {
            return default;
        }

        return (T?)property.GetValue(obj);
    }
}