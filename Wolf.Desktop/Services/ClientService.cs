using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public class ClientService : IClientService
{
    private readonly ApiClient _client;

    public ClientService(ApiClient client) => _client = client;

    public Task<IEnumerable<ClientDto>> GetAllAsync() =>
        _client.GetAsync<IEnumerable<ClientDto>>("api/clients")!;

    public Task<ClientDto?> GetByIdAsync(int id) =>
        _client.GetAsync<ClientDto>($"api/clients/{id}");

    public Task<ClientDto?> CreateAsync(CreateClientDto dto) =>
        _client.PostAsync<ClientDto>("api/clients", dto);

    public Task UpdateAsync(int id, ClientDto dto) =>
        _client.PutAsync($"api/clients/{id}", dto);

    public Task DeleteAsync(int id) =>
        _client.DeleteAsync($"api/clients/{id}");
}
