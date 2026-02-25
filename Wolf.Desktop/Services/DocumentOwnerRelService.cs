using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public class DocumentOwnerRelService : IDocumentOwnerRelService
{
    private readonly ApiClient _client;

    public DocumentOwnerRelService(ApiClient client) => _client = client;

    public Task<IEnumerable<DocumentofownershipOwnerrelashionshipDto>> GetAllAsync() =>
        _client.GetAsync<IEnumerable<DocumentofownershipOwnerrelashionshipDto>>("api/documentofownershipownerrelashionships")!;

    public Task<DocumentofownershipOwnerrelashionshipDto?> GetByIdAsync(int id) =>
        _client.GetAsync<DocumentofownershipOwnerrelashionshipDto>($"api/documentofownershipownerrelashionships/{id}");

    public Task<DocumentofownershipOwnerrelashionshipDto?> CreateAsync(CreateDocumentofownershipOwnerrelashionshipDto dto) =>
        _client.PostAsync<DocumentofownershipOwnerrelashionshipDto>("api/documentofownershipownerrelashionships", dto);

    public Task UpdateAsync(int id, DocumentofownershipOwnerrelashionshipDto dto) =>
        _client.PutAsync($"api/documentofownershipownerrelashionships/{id}", dto);

    public Task DeleteAsync(int id) =>
        _client.DeleteAsync($"api/documentofownershipownerrelashionships/{id}");
}
