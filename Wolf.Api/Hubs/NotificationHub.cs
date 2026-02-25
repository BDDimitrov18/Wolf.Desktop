using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Wolf.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
}
