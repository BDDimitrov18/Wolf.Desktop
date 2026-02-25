using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public interface IDocumentOwnerRelService
{
    Task<IEnumerable<DocumentofownershipOwnerrelashionshipDto>> GetAllAsync();
    Task<DocumentofownershipOwnerrelashionshipDto?> GetByIdAsync(int id);
    Task<DocumentofownershipOwnerrelashionshipDto?> CreateAsync(CreateDocumentofownershipOwnerrelashionshipDto dto);
    Task UpdateAsync(int id, DocumentofownershipOwnerrelashionshipDto dto);
    Task DeleteAsync(int id);
}
