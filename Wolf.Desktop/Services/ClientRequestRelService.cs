using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public class ClientRequestRelService : IClientRequestRelService
{
    private readonly ApiClient _client;

    public ClientRequestRelService(ApiClient client) => _client = client;

    public Task<IEnumerable<ClientRequestrelashionshipDto>> GetAllAsync() =>
        _client.GetAsync<IEnumerable<ClientRequestrelashionshipDto>>("api/clientrequestrelashionships")!;

    public Task<ClientRequestrelashionshipDto?> CreateAsync(CreateClientRequestrelashionshipDto dto) =>
        _client.PostAsync<ClientRequestrelashionshipDto>("api/clientrequestrelashionships", dto);

    public Task DeleteAsync(int requestId, int clientId) =>
        _client.DeleteAsync($"api/clientrequestrelashionships/{requestId}/{clientId}");
}
