using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public interface ITaskTypeService
{
    Task<IEnumerable<TasktypeDto>> GetAllAsync();
    Task<TasktypeDto?> CreateAsync(CreateTasktypeDto dto);
}
