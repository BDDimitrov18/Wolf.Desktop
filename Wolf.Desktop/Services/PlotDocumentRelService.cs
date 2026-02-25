using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public class PlotDocumentRelService : IPlotDocumentRelService
{
    private readonly ApiClient _client;

    public PlotDocumentRelService(ApiClient client) => _client = client;

    public Task<IEnumerable<PlotDocumentofownershipDto>> GetAllAsync() =>
        _client.GetAsync<IEnumerable<PlotDocumentofownershipDto>>("api/plotdocumentofownerships")!;

    public Task<PlotDocumentofownershipDto?> CreateAsync(CreatePlotDocumentofownershipDto dto) =>
        _client.PostAsync<PlotDocumentofownershipDto>("api/plotdocumentofownerships", dto);

    public Task DeleteAsync(int id) =>
        _client.DeleteAsync($"api/plotdocumentofownerships/{id}");
}
