using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public interface IOwnerService
{
    Task<IEnumerable<OwnerDto>> GetAllAsync();
    Task<OwnerDto?> GetByIdAsync(int id);
    Task<OwnerDto?> CreateAsync(CreateOwnerDto dto);
    Task UpdateAsync(int id, OwnerDto dto);
    Task DeleteAsync(int id);
}
