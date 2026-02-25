using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public interface IActivityTypeService
{
    Task<IEnumerable<ActivitytypeDto>> GetAllAsync();
    Task<ActivitytypeDto?> CreateAsync(CreateActivitytypeDto dto);
}
