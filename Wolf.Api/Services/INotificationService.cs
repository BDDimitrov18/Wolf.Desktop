namespace Wolf.Api.Services;

public interface INotificationService
{
    Task NotifyAsync(string entityType, string action, object payload);
}
