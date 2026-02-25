using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public class PowerofattorneyService : IPowerofattorneyService
{
    private readonly ApiClient _client;

    public PowerofattorneyService(ApiClient client) => _client = client;

    public Task<IEnumerable<PowerofattorneydocumentDto>> GetAllAsync() =>
        _client.GetAsync<IEnumerable<PowerofattorneydocumentDto>>("api/powerofattorneydocuments")!;

    public Task<PowerofattorneydocumentDto?> GetByIdAsync(int id) =>
        _client.GetAsync<PowerofattorneydocumentDto>($"api/powerofattorneydocuments/{id}");

    public Task<PowerofattorneydocumentDto?> CreateAsync(CreatePowerofattorneydocumentDto dto) =>
        _client.PostAsync<PowerofattorneydocumentDto>("api/powerofattorneydocuments", dto);

    public Task UpdateAsync(int id, PowerofattorneydocumentDto dto) =>
        _client.PutAsync($"api/powerofattorneydocuments/{id}", dto);

    public Task DeleteAsync(int id) =>
        _client.DeleteAsync($"api/powerofattorneydocuments/{id}");
}
