using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public class DocPlotDocOwnerRelService : IDocPlotDocOwnerRelService
{
    private readonly ApiClient _client;

    public DocPlotDocOwnerRelService(ApiClient client) => _client = client;

    public Task<IEnumerable<DocumentplotDocumentowenerrelashionshipDto>> GetAllAsync() =>
        _client.GetAsync<IEnumerable<DocumentplotDocumentowenerrelashionshipDto>>("api/documentplotdocumentowenerrelashionships")!;

    public Task<DocumentplotDocumentowenerrelashionshipDto?> GetByIdAsync(int id) =>
        _client.GetAsync<DocumentplotDocumentowenerrelashionshipDto>($"api/documentplotdocumentowenerrelashionships/{id}");

    public Task<DocumentplotDocumentowenerrelashionshipDto?> CreateAsync(CreateDocumentplotDocumentowenerrelashionshipDto dto) =>
        _client.PostAsync<DocumentplotDocumentowenerrelashionshipDto>("api/documentplotdocumentowenerrelashionships", dto);

    public Task UpdateAsync(int id, DocumentplotDocumentowenerrelashionshipDto dto) =>
        _client.PutAsync($"api/documentplotdocumentowenerrelashionships/{id}", dto);

    public Task DeleteAsync(int id) =>
        _client.DeleteAsync($"api/documentplotdocumentowenerrelashionships/{id}");
}
