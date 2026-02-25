using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Wolf.Desktop.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ApiClient(string baseUrl)
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        _http = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
    }

    public void SetToken(string token)
    {
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearToken()
    {
        _http.DefaultRequestHeaders.Authorization = null;
    }

    private void SetSignalRHeader(HttpRequestMessage request)
    {
        var connId = ServiceLocator.Realtime.ConnectionId;
        if (connId is not null)
            request.Headers.Add("X-SignalR-ConnectionId", connId);
    }

    public async Task<T?> GetAsync<T>(string path)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        SetSignalRHeader(request);
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    public async Task<T?> PostAsync<T>(string path, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };
        SetSignalRHeader(request);
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    public async Task PostAsync(string path, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };
        SetSignalRHeader(request);
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task PutAsync(string path, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, path)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };
        SetSignalRHeader(request);
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string path)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, path);
        SetSignalRHeader(request);
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
