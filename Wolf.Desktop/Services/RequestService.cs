using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public class RequestService : IRequestService
{
    private readonly ApiClient _client;

    public RequestService(ApiClient client) => _client = client;

    public Task<IEnumerable<RequestDto>> GetAllAsync() =>
        _client.GetAsync<IEnumerable<RequestDto>>("api/requests")!;

    public Task<RequestDto?> GetByIdAsync(int id) =>
        _client.GetAsync<RequestDto>($"api/requests/{id}");

    public Task<RequestDto?> CreateAsync(CreateRequestDto dto) =>
        _client.PostAsync<RequestDto>("api/requests", dto);

    public Task UpdateAsync(int id, RequestDto dto) =>
        _client.PutAsync($"api/requests/{id}", dto);

    public Task DeleteAsync(int id) =>
        _client.DeleteAsync($"api/requests/{id}");
}
