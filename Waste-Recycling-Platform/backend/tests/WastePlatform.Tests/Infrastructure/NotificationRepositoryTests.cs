using Microsoft.EntityFrameworkCore;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;
using WastePlatform.Infrastructure.Persistence.Repositories;

namespace WastePlatform.Tests.Infrastructure;

[AllureEpic("Infrastructure")]
[AllureFeature("Notification Repository")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Citizen notification persistence and paging")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Infrastructure")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "NotificationRepositoryTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Infrastructure")]
[AllureOwner("backend")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("unit")]
[Allure.Net.Commons.Attributes.AllureTag("repository")]
public class NotificationRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldTrackNotificationAndSaveChanges_ShouldPersistIt()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new NotificationRepository(context);
        var notification = CreateNotification(Guid.NewGuid(), NotificationStatus.Unread, DateTime.UtcNow);

        // Act
        var added = await repository.AddAsync(notification, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        added.Should().BeSameAs(notification);
        (await context.Notifications.CountAsync()).Should().Be(1);
        (await context.Notifications.SingleAsync()).Id.Should().Be(notification.Id);
    }

    [Fact]
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

        // Act
        var (notifications, total) = await repository.GetByCitizenIdAsync(citizenId, page: 1, pageSize: 10, status: null, CancellationToken.None);

        // Assert
        total.Should().Be(2);
        notifications.Should().HaveCount(2);
        notifications.First().Id.Should().Be(newer.Id);
        notifications.Last().Id.Should().Be(older.Id);
    }

    [Fact]
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

        // Act
        var (notifications, total) = await repository.GetByCitizenIdAsync(citizenId, page: 1, pageSize: 10, status: NotificationStatus.Read, CancellationToken.None);

        // Assert
        total.Should().Be(1);
        notifications.Should().ContainSingle(n => n.Id == read.Id);
    }

    [Fact]
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

        // Act
        var count = await repository.GetUnreadCountAsync(citizenId, CancellationToken.None);

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenNotificationExists_ShouldUpdateStatusAndReadAt()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new NotificationRepository(context);
        var citizenId = Guid.NewGuid();
        var notification = CreateNotification(citizenId, NotificationStatus.Unread, DateTime.UtcNow.AddMinutes(-1));
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.MarkAsReadAsync(notification.Id, citizenId, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var updated = await context.Notifications.SingleAsync(x => x.Id == notification.Id);
        updated.Status.Should().Be(NotificationStatus.Read);
        updated.ReadAt.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenNotificationDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new NotificationRepository(context);

        // Act
        var result = await repository.MarkAsReadAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
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