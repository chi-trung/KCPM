namespace WastePlatform.Application.Common.Interfaces;

/// <summary>
/// Interface for pushing real-time notifications to clients
/// Implementation should use SignalR or similar technology
/// </summary>
public interface IRealTimeNotifier
{
    Task NotifyUserAsync(Guid userId, string eventName, object data);
    Task NotifyUsersAsync(IEnumerable<Guid> userIds, string eventName, object data);
}
