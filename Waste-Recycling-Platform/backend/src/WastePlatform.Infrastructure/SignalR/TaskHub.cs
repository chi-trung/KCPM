using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WastePlatform.Infrastructure.SignalR;

[Authorize]
public class TaskHub : Hub
{
    // Hub can be extended later for specific group connections
}
