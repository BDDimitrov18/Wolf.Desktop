using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public interface IPlotService
{
    Task<IEnumerable<PlotDto>> GetAllAsync();
    Task<PlotDto?> GetByIdAsync(int id);
    Task<PlotDto?> CreateAsync(CreatePlotDto dto);
    Task UpdateAsync(int id, PlotDto dto);
    Task DeleteAsync(int id);
}
