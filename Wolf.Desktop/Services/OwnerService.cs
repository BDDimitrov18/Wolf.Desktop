using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public class OwnerService : IOwnerService
{
    private readonly ApiClient _client;

    public OwnerService(ApiClient client) => _client = client;

    public Task<IEnumerable<OwnerDto>> GetAllAsync() =>
        _client.GetAsync<IEnumerable<OwnerDto>>("api/owners")!;

    public Task<OwnerDto?> GetByIdAsync(int id) =>
        _client.GetAsync<OwnerDto>($"api/owners/{id}");

    public Task<OwnerDto?> CreateAsync(CreateOwnerDto dto) =>
        _client.PostAsync<OwnerDto>("api/owners", dto);

    public Task UpdateAsync(int id, OwnerDto dto) =>
        _client.PutAsync($"api/owners/{id}", dto);

    public Task DeleteAsync(int id) =>
        _client.DeleteAsync($"api/owners/{id}");
}
