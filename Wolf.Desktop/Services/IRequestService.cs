using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public interface IRequestService
{
    Task<IEnumerable<RequestDto>> GetAllAsync();
    Task<RequestDto?> GetByIdAsync(int id);
    Task<RequestDto?> CreateAsync(CreateRequestDto dto);
    Task UpdateAsync(int id, RequestDto dto);
    Task DeleteAsync(int id);
}
