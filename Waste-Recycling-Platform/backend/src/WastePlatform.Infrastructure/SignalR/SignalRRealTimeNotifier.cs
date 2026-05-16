using Microsoft.AspNetCore.SignalR;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Infrastructure.SignalR;

public class SignalRRealTimeNotifier : IRealTimeNotifier
{
    private readonly IHubContext<TaskHub> _hubContext;

    public SignalRRealTimeNotifier(IHubContext<TaskHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyUserAsync(Guid userId, string eventName, object data)
    {
        await _hubContext.Clients.User(userId.ToString()).SendAsync(eventName, data);
    }

    public async Task NotifyUsersAsync(IEnumerable<Guid> userIds, string eventName, object data)
    {
        var userIdStrings = userIds.Select(u => u.ToString()).ToList();
        await _hubContext.Clients.Users(userIdStrings).SendAsync(eventName, data);
    }
}
