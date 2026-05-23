using Allure.Net.Commons;
using Allure.Xunit.Attributes;
using Microsoft.AspNetCore.SignalR;
using Moq;
using WastePlatform.Infrastructure.SignalR;

namespace WastePlatform.Tests.Infrastructure.SignalR;

[AllureEpic("KIEM-19 SignalR Real-time Notifications")]
[AllureFeature("WRP-BE-TESTS-016 SignalR Real-time Tests")]
public class SignalRRealTimeNotifierTests
{
    [AllureStory("Push notification to a single user")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureOwner("chi-trung")]
    [Fact]
    public async Task NotifyUserAsync_ShouldSendPayloadToTheTargetUser()
    {
        var targetUserId = Guid.NewGuid();
        var hubContext = CreateHubContextMock(out var userProxy, out _);
        var notifier = new SignalRRealTimeNotifier(hubContext.Object);

        var payload = new
        {
            Id = Guid.NewGuid(),
            Type = "ReportCreated",
            Title = "Báo cáo đã gửi thành công",
            Message = "Báo cáo của bạn đã được gửi.",
            ActionUrl = "/citizen/reports/1",
            RelatedEntityId = Guid.NewGuid(),
            RelatedEntityType = "Report",
            CreatedAt = DateTime.UtcNow
        };

        userProxy
            .Setup(x => x.SendCoreAsync("NewNotification", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await notifier.NotifyUserAsync(targetUserId, "NewNotification", payload);

        userProxy.Verify(
            x => x.SendCoreAsync(
                "NewNotification",
                It.Is<object?[]>(args => HasSinglePayloadWith(args, payload)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [AllureStory("Push notification to multiple users")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureOwner("chi-trung")]
    [Fact]
    public async Task NotifyUsersAsync_ShouldSendPayloadToAllTargetUsers()
    {
        var userIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var hubContext = CreateHubContextMock(out _, out var usersProxy);
        var notifier = new SignalRRealTimeNotifier(hubContext.Object);

        var payload = new
        {
            Id = Guid.NewGuid(),
            Type = "TaskAssigned",
            Title = "Đã phân công người thu gom",
            Message = "Collector sẽ đến thu gom báo cáo của bạn.",
            ActionUrl = "/citizen/reports/2",
            RelatedEntityId = Guid.NewGuid(),
            RelatedEntityType = "Report",
            CreatedAt = DateTime.UtcNow
        };

        usersProxy
            .Setup(x => x.SendCoreAsync("TaskStatusUpdated", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await notifier.NotifyUsersAsync(userIds, "TaskStatusUpdated", payload);

        usersProxy.Verify(
            x => x.SendCoreAsync(
                "TaskStatusUpdated",
                It.Is<object?[]>(args => HasSinglePayloadWith(args, payload)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [AllureStory("Task hub requires authorization")]
    [AllureSeverity(SeverityLevel.normal)]
    [AllureOwner("chi-trung")]
    [Fact]
    public void TaskHub_ShouldBeProtectedByAuthorizeAttribute()
    {
        var authorizeAttribute = typeof(TaskHub)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), inherit: true)
            .Should()
            .ContainSingle()
            .Subject;

        authorizeAttribute.Should().NotBeNull();
    }

    private static Mock<IHubContext<TaskHub>> CreateHubContextMock(out Mock<IClientProxy> userProxy, out Mock<IClientProxy> usersProxy)
    {
        userProxy = new Mock<IClientProxy>();
        usersProxy = new Mock<IClientProxy>();

        var hubClients = new Mock<IHubClients>();
        hubClients.Setup(x => x.User(It.IsAny<string>())).Returns(userProxy.Object);
        hubClients.Setup(x => x.Users(It.IsAny<IReadOnlyList<string>>())).Returns(usersProxy.Object);

        var hubContext = new Mock<IHubContext<TaskHub>>();
        hubContext.SetupGet(x => x.Clients).Returns(hubClients.Object);

        return hubContext;
    }

    private static bool HasSinglePayloadWith(object?[] args, object expectedPayload)
    {
        if (args.Length != 1 || args[0] is null)
        {
            return false;
        }

        var actualPayload = args[0];
        return GetPropertyValue<string>(actualPayload, "Type") == GetPropertyValue<string>(expectedPayload, "Type")
            && GetPropertyValue<string>(actualPayload, "Title") == GetPropertyValue<string>(expectedPayload, "Title")
            && GetPropertyValue<string>(actualPayload, "Message") == GetPropertyValue<string>(expectedPayload, "Message")
            && GetPropertyValue<string>(actualPayload, "ActionUrl") == GetPropertyValue<string>(expectedPayload, "ActionUrl")
            && GetPropertyValue<string>(actualPayload, "RelatedEntityType") == GetPropertyValue<string>(expectedPayload, "RelatedEntityType")
            && GetPropertyValue<Guid>(actualPayload, "Id") == GetPropertyValue<Guid>(expectedPayload, "Id")
            && GetPropertyValue<Guid>(actualPayload, "RelatedEntityId") == GetPropertyValue<Guid>(expectedPayload, "RelatedEntityId");
    }

    private static T? GetPropertyValue<T>(object? obj, string propertyName)
    {
        if (obj is null)
        {
            return default;
        }

        var property = obj.GetType().GetProperty(propertyName);
        return property is null ? default : (T?)property.GetValue(obj);
    }
}
