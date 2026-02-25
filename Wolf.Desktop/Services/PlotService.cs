using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public class PlotService : IPlotService
{
    private readonly ApiClient _client;

    public PlotService(ApiClient client) => _client = client;

    public Task<IEnumerable<PlotDto>> GetAllAsync() =>
        _client.GetAsync<IEnumerable<PlotDto>>("api/plots")!;

    public Task<PlotDto?> GetByIdAsync(int id) =>
        _client.GetAsync<PlotDto>($"api/plots/{id}");

    public Task<PlotDto?> CreateAsync(CreatePlotDto dto) =>
        _client.PostAsync<PlotDto>("api/plots", dto);

    public Task UpdateAsync(int id, PlotDto dto) =>
        _client.PutAsync($"api/plots/{id}", dto);

    public Task DeleteAsync(int id) =>
        _client.DeleteAsync($"api/plots/{id}");
}
