using Microsoft.EntityFrameworkCore;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;
using WastePlatform.Infrastructure.Persistence.Repositories;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Infrastructure;

[AllureEpic("Infrastructure")]
[AllureFeature("Notification Repository")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Citizen notification persistence and paging")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Infrastructure")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "NotificationRepositoryTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Infrastructure")]
[AllureOwner("Hoàng Phụng")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("repository")]
public class NotificationRepositoryTests
{
    [Fact]
    [AllureDescription("Persists a new notification through the repository and saves it to the database.")]
    public async Task AddAsync_ShouldTrackNotificationAndSaveChanges_ShouldPersistIt()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new NotificationRepository(context);
        var notification = CreateNotification(Guid.NewGuid(), NotificationStatus.Unread, DateTime.UtcNow);

        AllureAttachmentHelper.AttachJson("notification-add-input", new
        {
            notification.CitizenId,
            notification.Type,
            notification.Channel,
            notification.Status,
            notification.Title,
            notification.RelatedEntityType
        });

        // Act
        var added = await repository.AddAsync(notification, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        added.Should().BeSameAs(notification);
        (await context.Notifications.CountAsync()).Should().Be(1);
        (await context.Notifications.SingleAsync()).Id.Should().Be(notification.Id);

        AllureAttachmentHelper.AttachJson("notification-add-result", new { added.Id, count = await context.Notifications.CountAsync() });
    }

    [Fact]
    [AllureDescription("Returns notifications for one citizen ordered by newest first and paginated.")]
    public async Task GetByCitizenIdAsync_ShouldFilterOrderAndPaginateNotifications()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new NotificationRepository(context);
        var citizenId = Guid.NewGuid();
        var otherCitizenId = Guid.NewGuid();

        var older = CreateNotification(citizenId, NotificationStatus.Unread, DateTime.UtcNow.AddMinutes(-20));
        var newer = CreateNotification(citizenId, NotificationStatus.Read, DateTime.UtcNow.AddMinutes(-5));
        var otherCitizen = CreateNotification(otherCitizenId, NotificationStatus.Unread, DateTime.UtcNow);

        context.Notifications.AddRange(older, newer, otherCitizen);
        await context.SaveChangesAsync();

        AllureAttachmentHelper.AttachJson("notification-page-seed", new
        {
            citizenId,
            older = older.CreatedAt,
            newer = newer.CreatedAt,
            otherCitizen = otherCitizen.CitizenId
        });

        // Act
        var (notifications, total) = await repository.GetByCitizenIdAsync(citizenId, page: 1, pageSize: 10, status: null, CancellationToken.None);

        // Assert
        total.Should().Be(2);
        notifications.Should().HaveCount(2);
        notifications.First().Id.Should().Be(newer.Id);
        notifications.Last().Id.Should().Be(older.Id);

        AllureAttachmentHelper.AttachJson("notification-page-result", new
        {
            total,
            returnedCount = notifications.Count(),
            firstId = notifications.First().Id,
            lastId = notifications.Last().Id
        });
    }

    [Fact]
    [AllureDescription("Filters notifications by status so only matching items are returned.")]
    public async Task GetByCitizenIdAsync_WithStatusFilter_ShouldReturnOnlyMatchingStatus()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new NotificationRepository(context);
        var citizenId = Guid.NewGuid();

        var unread = CreateNotification(citizenId, NotificationStatus.Unread, DateTime.UtcNow.AddMinutes(-2));
        var read = CreateNotification(citizenId, NotificationStatus.Read, DateTime.UtcNow.AddMinutes(-1));

        context.Notifications.AddRange(unread, read);
        await context.SaveChangesAsync();

        AllureAttachmentHelper.AttachJson("notification-status-filter-seed", new { citizenId, unreadId = unread.Id, readId = read.Id });

        // Act
        var (notifications, total) = await repository.GetByCitizenIdAsync(citizenId, page: 1, pageSize: 10, status: NotificationStatus.Read, CancellationToken.None);

        // Assert
        total.Should().Be(1);
        notifications.Should().ContainSingle(n => n.Id == read.Id);

        AllureAttachmentHelper.AttachJson("notification-status-filter-result", new
        {
            total,
            returnedCount = notifications.Count(),
            returnedStatus = notifications.Single().Status
        });
    }

    [Fact]
    [AllureDescription("Counts only unread notifications for a given citizen.")]
    public async Task GetUnreadCountAsync_ShouldCountOnlyUnreadNotificationsForCitizen()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new NotificationRepository(context);
        var citizenId = Guid.NewGuid();
        var otherCitizenId = Guid.NewGuid();

        context.Notifications.AddRange(
            CreateNotification(citizenId, NotificationStatus.Unread, DateTime.UtcNow.AddMinutes(-3)),
            CreateNotification(citizenId, NotificationStatus.Unread, DateTime.UtcNow.AddMinutes(-2)),
            CreateNotification(citizenId, NotificationStatus.Read, DateTime.UtcNow.AddMinutes(-1)),
            CreateNotification(otherCitizenId, NotificationStatus.Unread, DateTime.UtcNow));
        await context.SaveChangesAsync();

        AllureAttachmentHelper.AttachText("notification-unread-count-seed", $"citizenId={citizenId}\notherCitizenId={otherCitizenId}");

        // Act
        var count = await repository.GetUnreadCountAsync(citizenId, CancellationToken.None);

        // Assert
        count.Should().Be(2);

        AllureAttachmentHelper.AttachJson("notification-unread-count-result", new { citizenId, count });
    }

    [Fact]
    [AllureDescription("Marks a notification as read and persists the read timestamp when the notification exists.")]
    public async Task MarkAsReadAsync_WhenNotificationExists_ShouldUpdateStatusAndReadAt()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new NotificationRepository(context);
        var citizenId = Guid.NewGuid();
        var notification = CreateNotification(citizenId, NotificationStatus.Unread, DateTime.UtcNow.AddMinutes(-1));
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        AllureAttachmentHelper.AttachJson("notification-mark-read-seed", new { notification.Id, citizenId });

        // Act
        var result = await repository.MarkAsReadAsync(notification.Id, citizenId, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var updated = await context.Notifications.SingleAsync(x => x.Id == notification.Id);
        updated.Status.Should().Be(NotificationStatus.Read);
        updated.ReadAt.Should().NotBeNull();

        AllureAttachmentHelper.AttachJson("notification-mark-read-result", new { updated.Id, updated.Status, updated.ReadAt });
    }

    [Fact]
    [AllureDescription("Returns false when the notification does not exist for the given citizen.")]
    public async Task MarkAsReadAsync_WhenNotificationDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new NotificationRepository(context);

        AllureAttachmentHelper.AttachText("notification-mark-read-missing-seed", "No notification inserted for this test.");

        // Act
        var result = await repository.MarkAsReadAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        AllureAttachmentHelper.AttachText("notification-mark-read-missing-result", "Result was false as expected.");
    }

    [Fact]
    [AllureDescription("Marks all unread notifications for one citizen without touching other citizens.")]
    public async Task MarkAllAsReadAsync_ShouldMarkOnlyUnreadNotificationsForCitizen()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new NotificationRepository(context);
        var citizenId = Guid.NewGuid();
        var otherCitizenId = Guid.NewGuid();

        var citizenUnread1 = CreateNotification(citizenId, NotificationStatus.Unread, DateTime.UtcNow.AddMinutes(-4));
        var citizenUnread2 = CreateNotification(citizenId, NotificationStatus.Unread, DateTime.UtcNow.AddMinutes(-3));
        var citizenRead = CreateNotification(citizenId, NotificationStatus.Read, DateTime.UtcNow.AddMinutes(-2));
        var otherUnread = CreateNotification(otherCitizenId, NotificationStatus.Unread, DateTime.UtcNow.AddMinutes(-1));

        context.Notifications.AddRange(citizenUnread1, citizenUnread2, citizenRead, otherUnread);
        await context.SaveChangesAsync();

        AllureAttachmentHelper.AttachJson("notification-mark-all-seed", new
        {
            citizenId,
            citizenUnreadIds = new[] { citizenUnread1.Id, citizenUnread2.Id },
            citizenReadId = citizenRead.Id,
            otherUnreadId = otherUnread.Id
        });

        // Act
        await repository.MarkAllAsReadAsync(citizenId, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        var citizenNotifications = await context.Notifications.Where(x => x.CitizenId == citizenId).ToListAsync();
        citizenNotifications.Should().ContainSingle(x => x.Id == citizenRead.Id && x.Status == NotificationStatus.Read);
        citizenNotifications.Where(x => x.Status == NotificationStatus.Read).Should().HaveCount(3);
        citizenNotifications.Where(x => x.Id == citizenUnread1.Id || x.Id == citizenUnread2.Id).Should().OnlyContain(x => x.ReadAt.HasValue);

        var otherNotification = await context.Notifications.SingleAsync(x => x.Id == otherUnread.Id);
        otherNotification.Status.Should().Be(NotificationStatus.Unread);

        AllureAttachmentHelper.AttachJson("notification-mark-all-result", new
        {
            citizenReadCount = citizenNotifications.Count(x => x.Status == NotificationStatus.Read),
            citizenUnreadCount = citizenNotifications.Count(x => x.Status == NotificationStatus.Unread),
            otherNotification.Status
        });
    }

    private static WastePlatformDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<WastePlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new WastePlatformDbContext(options);
    }

    private static Notification CreateNotification(Guid citizenId, NotificationStatus status, DateTime createdAt)
    {
        return new Notification
        {
            CitizenId = citizenId,
            Type = NotificationType.ReportCreated,
            Channel = NotificationChannel.InApp,
            Status = status,
            Title = "Test notification",
            Message = "Test message",
            ActionUrl = "/citizen/reports/test",
            RelatedEntityId = Guid.NewGuid(),
            RelatedEntityType = "Report",
            CreatedAt = createdAt,
            ReadAt = status == NotificationStatus.Read ? createdAt.AddMinutes(1) : null
        };
    }
}