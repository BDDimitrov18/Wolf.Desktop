using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public class DocumentsOfOwnershipService : IDocumentsOfOwnershipService
{
    private readonly ApiClient _client;

    public DocumentsOfOwnershipService(ApiClient client) => _client = client;

    public Task<IEnumerable<DocumentsofownershipDto>> GetAllAsync() =>
        _client.GetAsync<IEnumerable<DocumentsofownershipDto>>("api/documentsofownership")!;

    public Task<DocumentsofownershipDto?> GetByIdAsync(int id) =>
        _client.GetAsync<DocumentsofownershipDto>($"api/documentsofownership/{id}");

    public Task<DocumentsofownershipDto?> CreateAsync(CreateDocumentsofownershipDto dto) =>
        _client.PostAsync<DocumentsofownershipDto>("api/documentsofownership", dto);

    public Task UpdateAsync(int id, DocumentsofownershipDto dto) =>
        _client.PutAsync($"api/documentsofownership/{id}", dto);

    public Task DeleteAsync(int id) =>
        _client.DeleteAsync($"api/documentsofownership/{id}");
}
