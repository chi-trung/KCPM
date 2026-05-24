using FluentAssertions;
using Moq;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Services;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using Xunit;

namespace WastePlatform.Tests.Application.Notifications;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _mockNotificationRepository;
    private readonly Mock<IRealTimeNotifier> _mockRealTimeNotifier;
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _mockNotificationRepository = new Mock<INotificationRepository>();
        _mockRealTimeNotifier = new Mock<IRealTimeNotifier>();
        _service = new NotificationService(
            _mockNotificationRepository.Object,
            _mockRealTimeNotifier.Object);
    }

    [Fact]
    public async Task NotifyReportCreatedAsync_ShouldPersistNotificationAndPushRealtimeMessage()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        Notification? capturedNotification = null;

        _mockNotificationRepository
            .Setup(x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((notification, _) => capturedNotification = notification)
            .ReturnsAsync((Notification notification, CancellationToken _) => notification);

        _mockNotificationRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRealTimeNotifier
            .Setup(x => x.NotifyUserAsync(citizenId, "NewNotification", It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.NotifyReportCreatedAsync(citizenId, reportId, CancellationToken.None);

        // Attach payload snapshot for Allure
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(capturedNotification, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            var path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "allure-results", $"notification-{capturedNotification.Id}.json");
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
            System.IO.File.WriteAllText(path, json);
        }
        catch { /* best-effort: don't fail test if attachment write fails */ }

        // Assert
        capturedNotification.Should().NotBeNull();
        capturedNotification!.CitizenId.Should().Be(citizenId);
        capturedNotification.Type.Should().Be(NotificationType.ReportCreated);
        capturedNotification.Channel.Should().Be(NotificationChannel.InApp);
        capturedNotification.Title.Should().Be("Báo cáo đã gửi thành công");
        capturedNotification.Message.Should().Contain(reportId.ToString()[..8]);
        capturedNotification.RelatedEntityId.Should().Be(reportId);
        capturedNotification.RelatedEntityType.Should().Be("Report");
        capturedNotification.ActionUrl.Should().Be($"/citizen/reports/{reportId}");

        _mockNotificationRepository.Verify(
            x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockNotificationRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _mockRealTimeNotifier.Verify(
            x => x.NotifyUserAsync(
                citizenId,
                "NewNotification",
                It.Is<object>(payload => HasNotificationPayload(payload, capturedNotification!))),
            Times.Once);
    }

    [Fact]
    public async Task NotifyReportRejectedAsync_WithReason_ShouldIncludeReasonInMessage()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var reason = "Thiếu ảnh rõ ràng";
        Notification? capturedNotification = null;

        _mockNotificationRepository
            .Setup(x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((notification, _) => capturedNotification = notification)
            .ReturnsAsync((Notification notification, CancellationToken _) => notification);

        _mockNotificationRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRealTimeNotifier
            .Setup(x => x.NotifyUserAsync(citizenId, "NewNotification", It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.NotifyReportRejectedAsync(citizenId, reportId, reason, CancellationToken.None);

        // Assert
        capturedNotification.Should().NotBeNull();
        capturedNotification!.Type.Should().Be(NotificationType.ReportRejected);
        capturedNotification.Channel.Should().Be(NotificationChannel.InApp);
        capturedNotification.Message.Should().Contain(reason);
        capturedNotification.ActionUrl.Should().Be($"/citizen/reports/{reportId}");

        _mockRealTimeNotifier.Verify(
            x => x.NotifyUserAsync(citizenId, "NewNotification", It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyReportRejectedAsync_WithoutReason_ShouldUseDefaultMessage()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        Notification? capturedNotification = null;

        _mockNotificationRepository
            .Setup(x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((notification, _) => capturedNotification = notification)
            .ReturnsAsync((Notification notification, CancellationToken _) => notification);

        _mockNotificationRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRealTimeNotifier
            .Setup(x => x.NotifyUserAsync(citizenId, "NewNotification", It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.NotifyReportRejectedAsync(citizenId, reportId, null, CancellationToken.None);

        // Assert
        capturedNotification.Should().NotBeNull();
        capturedNotification!.Message.Should().Be($"Báo cáo #{reportId.ToString()[..8]} không được chấp nhận.");
    }

    [Fact]
    public async Task NotifyComplaintEscalatedAsync_ShouldPersistAdminNotificationWithoutRealtimePush()
    {
        // Arrange
        var complaintId = Guid.NewGuid();
        Notification? capturedNotification = null;

        _mockNotificationRepository
            .Setup(x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((notification, _) => capturedNotification = notification)
            .ReturnsAsync((Notification notification, CancellationToken _) => notification);

        _mockNotificationRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.NotifyComplaintEscalatedAsync(complaintId, CancellationToken.None);

        // Assert
        capturedNotification.Should().NotBeNull();
        capturedNotification!.CitizenId.Should().BeNull();
        capturedNotification.Type.Should().Be(NotificationType.ComplaintEscalated);
        capturedNotification.Channel.Should().Be(NotificationChannel.InApp);
        capturedNotification.Title.Should().Be("Khiếu nại được chuyển lên Admin");
        capturedNotification.RelatedEntityId.Should().Be(complaintId);
        capturedNotification.RelatedEntityType.Should().Be("Complaint");
        capturedNotification.ActionUrl.Should().Be($"/admin/complaints/{complaintId}");

        _mockRealTimeNotifier.Verify(
            x => x.NotifyUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<object>()),
            Times.Never);
        _mockNotificationRepository.Verify(
            x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockNotificationRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static bool HasNotificationPayload(object payload, Notification expected)
    {
        return GetPropertyValue<Guid>(payload, nameof(Notification.Id)) == expected.Id
            && GetPropertyValue<NotificationType>(payload, nameof(Notification.Type)) == expected.Type
            && GetPropertyValue<string>(payload, nameof(Notification.Title)) == expected.Title
            && GetPropertyValue<string>(payload, nameof(Notification.Message)) == expected.Message
            && GetPropertyValue<string?>(payload, nameof(Notification.ActionUrl)) == expected.ActionUrl
            && GetPropertyValue<Guid?>(payload, nameof(Notification.RelatedEntityId)) == expected.RelatedEntityId
            && GetPropertyValue<string?>(payload, nameof(Notification.RelatedEntityType)) == expected.RelatedEntityType
            && GetPropertyValue<DateTime>(payload, nameof(Notification.CreatedAt)) == expected.CreatedAt;
    }

    private static T? GetPropertyValue<T>(object payload, string propertyName)
    {
        var property = payload.GetType().GetProperty(propertyName);
        if (property is null)
        {
            return default;
        }

        return (T?)property.GetValue(payload);
    }
}