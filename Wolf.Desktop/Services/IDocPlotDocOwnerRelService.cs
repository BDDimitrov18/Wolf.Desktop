using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public interface IDocPlotDocOwnerRelService
{
    Task<IEnumerable<DocumentplotDocumentowenerrelashionshipDto>> GetAllAsync();
    Task<DocumentplotDocumentowenerrelashionshipDto?> GetByIdAsync(int id);
    Task<DocumentplotDocumentowenerrelashionshipDto?> CreateAsync(CreateDocumentplotDocumentowenerrelashionshipDto dto);
    Task UpdateAsync(int id, DocumentplotDocumentowenerrelashionshipDto dto);
    Task DeleteAsync(int id);
}
