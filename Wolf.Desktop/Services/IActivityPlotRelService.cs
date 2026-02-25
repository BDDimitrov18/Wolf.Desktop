using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public interface IActivityPlotRelService
{
    Task<IEnumerable<ActivityPlotrelashionshipDto>> GetAllAsync();
    Task<ActivityPlotrelashionshipDto?> CreateAsync(CreateActivityPlotrelashionshipDto dto);
    Task DeleteAsync(int activityId, int plotId);
}
