using Moq;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Services;
using WastePlatform.Domain.Entities;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Application.Services;

[AllureEpic("Services")]
[AllureFeature("Notification Service")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Sending notifications to citizens for report and complaint events")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Application")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "NotificationServiceTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Application.Services")]
[AllureOwner("Team")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("backend")]
[Allure.Net.Commons.Attributes.AllureTag("notifications")]
public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _mockNotifRepo;
    private readonly Mock<IRealTimeNotifier> _mockRealTimeNotifier;
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _mockNotifRepo = new Mock<INotificationRepository>();
        _mockRealTimeNotifier = new Mock<IRealTimeNotifier>();

        _mockNotifRepo
            .Setup(x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification n, CancellationToken _) => n);
        _mockNotifRepo
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRealTimeNotifier
            .Setup(x => x.NotifyUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        _service = new NotificationService(_mockNotifRepo.Object, _mockRealTimeNotifier.Object);
    }

    #region NotifyReportCreatedAsync

    [Fact]
    [AllureDescription("NotifyReportCreated saves notification and pushes real-time event.")]
    public async Task NotifyReportCreated_ShouldSaveAndPushNotification()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();

        // Act
        await _service.NotifyReportCreatedAsync(citizenId, reportId);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        _mockNotifRepo.Verify(x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockNotifRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockRealTimeNotifier.Verify(
            x => x.NotifyUserAsync(citizenId, "NewNotification", It.IsAny<object>()), Times.Once);
    }

    #endregion

    #region NotifyReportAcceptedAsync

    [Fact]
    [AllureDescription("NotifyReportAccepted saves notification and pushes real-time event.")]
    public async Task NotifyReportAccepted_ShouldSaveAndPushNotification()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();

        // Act
        await _service.NotifyReportAcceptedAsync(citizenId, reportId);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        _mockNotifRepo.Verify(x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockNotifRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockRealTimeNotifier.Verify(
            x => x.NotifyUserAsync(citizenId, "NewNotification", It.IsAny<object>()), Times.Once);
    }

    #endregion

    #region NotifyReportAssignedAsync

    [Fact]
    [AllureDescription("NotifyReportAssigned saves notification with collector name and pushes real-time event.")]
    public async Task NotifyReportAssigned_ShouldSaveAndPushNotification()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        const string collectorName = "Nguyen Van A";

        // Act
        await _service.NotifyReportAssignedAsync(citizenId, reportId, collectorName);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        _mockNotifRepo.Verify(x => x.AddAsync(
            It.Is<Notification>(n => n.Message.Contains(collectorName)),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockRealTimeNotifier.Verify(
            x => x.NotifyUserAsync(citizenId, "NewNotification", It.IsAny<object>()), Times.Once);
    }

    #endregion

    #region NotifyCollectorOnTheWayAsync

    [Fact]
    [AllureDescription("NotifyCollectorOnTheWay saves notification and pushes real-time event.")]
    public async Task NotifyCollectorOnTheWay_ShouldSaveAndPushNotification()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        const string collectorName = "Tran Van B";

        // Act
        await _service.NotifyCollectorOnTheWayAsync(citizenId, reportId, collectorName);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        _mockNotifRepo.Verify(x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockRealTimeNotifier.Verify(
            x => x.NotifyUserAsync(citizenId, "NewNotification", It.IsAny<object>()), Times.Once);
    }

    #endregion

    #region NotifyReportCollectedAsync

    [Fact]
    [AllureDescription("NotifyReportCollected includes points in message and pushes real-time event.")]
    public async Task NotifyReportCollected_ShouldIncludePointsAndPushNotification()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        const int points = 75;

        // Act
        await _service.NotifyReportCollectedAsync(citizenId, reportId, points);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        _mockNotifRepo.Verify(x => x.AddAsync(
            It.Is<Notification>(n => n.Message.Contains("75")),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockRealTimeNotifier.Verify(
            x => x.NotifyUserAsync(citizenId, "NewNotification", It.IsAny<object>()), Times.Once);
    }

    #endregion

    #region NotifyReportRejectedAsync

    [Fact]
    [AllureDescription("NotifyReportRejected with reason includes reason in message.")]
    public async Task NotifyReportRejected_WithReason_ShouldIncludeReasonInMessage()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        const string reason = "Hình ảnh không rõ ràng";

        // Act
        await _service.NotifyReportRejectedAsync(citizenId, reportId, reason);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        _mockNotifRepo.Verify(x => x.AddAsync(
            It.Is<Notification>(n => n.Message.Contains(reason)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [AllureDescription("NotifyReportRejected without reason uses default message without reason text.")]
    public async Task NotifyReportRejected_WithNullReason_ShouldUseDefaultMessage()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();

        // Act
        await _service.NotifyReportRejectedAsync(citizenId, reportId, null);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        _mockNotifRepo.Verify(x => x.AddAsync(
            It.Is<Notification>(n => !n.Message.Contains("Lý do:")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [AllureDescription("NotifyReportRejected with empty reason uses default message.")]
    public async Task NotifyReportRejected_WithEmptyReason_ShouldUseDefaultMessage()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var reportId = Guid.NewGuid();

        // Act
        await _service.NotifyReportRejectedAsync(citizenId, reportId, string.Empty);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        _mockNotifRepo.Verify(x => x.AddAsync(
            It.Is<Notification>(n => !n.Message.Contains("Lý do:")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region NotifyComplaintRepliedAsync

    [Fact]
    [AllureDescription("NotifyComplaintReplied saves complaint notification with repliedBy in message.")]
    public async Task NotifyComplaintReplied_ShouldIncludeRepliedByInMessage()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var complaintId = Guid.NewGuid();
        const string repliedBy = "Admin Nguyen";

        // Act
        await _service.NotifyComplaintRepliedAsync(citizenId, complaintId, repliedBy);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        _mockNotifRepo.Verify(x => x.AddAsync(
            It.Is<Notification>(n => n.Message.Contains(repliedBy) && n.RelatedEntityType == "Complaint"),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockRealTimeNotifier.Verify(
            x => x.NotifyUserAsync(citizenId, "NewNotification", It.IsAny<object>()), Times.Once);
    }

    #endregion

    #region NotifyComplaintEscalatedAsync

    [Fact]
    [AllureDescription("NotifyComplaintEscalated saves escalation notification without pushing to specific user.")]
    public async Task NotifyComplaintEscalated_ShouldSaveNotificationWithoutUserPush()
    {
        // Arrange
        var complaintId = Guid.NewGuid();

        // Act
        await _service.NotifyComplaintEscalatedAsync(complaintId);

        // Assert
        AllureAttachmentHelper.AttachText("assert-result", "Verifying handler result");
        _mockNotifRepo.Verify(x => x.AddAsync(
            It.Is<Notification>(n => n.RelatedEntityId == complaintId && n.RelatedEntityType == "Complaint"),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockNotifRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        // No real-time push for escalated complaints (it's a system-level notification)
        _mockRealTimeNotifier.Verify(
            x => x.NotifyUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    #endregion
}


