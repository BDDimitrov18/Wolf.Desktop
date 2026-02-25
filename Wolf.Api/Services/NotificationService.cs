using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Wolf.Api.Hubs;

namespace Wolf.Api.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hub;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NotificationService(IHubContext<NotificationHub> hub,
        IHttpContextAccessor httpContextAccessor)
    {
        _hub = hub;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task NotifyAsync(string entityType, string action, object payload)
    {
        var json = JsonSerializer.Serialize(payload);

        var excludeId = _httpContextAccessor.HttpContext?
            .Request.Headers["X-SignalR-ConnectionId"].FirstOrDefault();

        var clients = excludeId is not null
            ? _hub.Clients.AllExcept(excludeId)
            : _hub.Clients.All;

        await clients.SendAsync("EntityChanged", entityType, action, json);
    }
}
