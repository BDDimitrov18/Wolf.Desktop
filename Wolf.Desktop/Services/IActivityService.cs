using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public interface IActivityService
{
    Task<IEnumerable<ActivityDto>> GetAllAsync();
    Task<ActivityDto?> GetByIdAsync(int id);
    Task<ActivityDto?> CreateAsync(CreateActivityDto dto);
    Task UpdateAsync(int id, ActivityDto dto);
    Task DeleteAsync(int id);
}
