using Avalonia.Threading;
using Microsoft.AspNetCore.SignalR.Client;

namespace Wolf.Desktop.Services;

public class SignalRService
{
    private HubConnection? _connection;
    private readonly string _hubUrl;

    public string? ConnectionId => _connection?.ConnectionId;

    public SignalRService(string baseUrl)
    {
        _hubUrl = baseUrl.TrimEnd('/') + "/hubs/notifications";
    }

    public async Task StartAsync()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options =>
            {
                options.AccessTokenProvider = () =>
                    Task.FromResult(ServiceLocator.Auth.CurrentToken);
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<string, string, string>("EntityChanged", OnEntityChanged);

        try
        {
            await _connection.StartAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SignalR connect failed: {ex.Message}");
        }
    }

    public async Task StopAsync()
    {
        if (_connection is not null)
        {
            try
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
            }
            catch { }
            _connection = null;
        }
    }

    private void OnEntityChanged(string entityType, string action, string jsonPayload)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
            ServiceLocator.Cache.ApplyRemoteChange(entityType, action, jsonPayload));
    }
}
