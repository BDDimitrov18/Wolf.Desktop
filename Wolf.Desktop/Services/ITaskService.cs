using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetAllAsync();
    Task<TaskDto?> GetByIdAsync(int id);
    Task<TaskDto?> CreateAsync(CreateTaskDto dto);
    Task UpdateAsync(int id, TaskDto dto);
    Task DeleteAsync(int id);
}
