using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public interface IDocumentsOfOwnershipService
{
    Task<IEnumerable<DocumentsofownershipDto>> GetAllAsync();
    Task<DocumentsofownershipDto?> GetByIdAsync(int id);
    Task<DocumentsofownershipDto?> CreateAsync(CreateDocumentsofownershipDto dto);
    Task UpdateAsync(int id, DocumentsofownershipDto dto);
    Task DeleteAsync(int id);
}
