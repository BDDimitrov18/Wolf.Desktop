using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public interface IPlotDocumentRelService
{
    Task<IEnumerable<PlotDocumentofownershipDto>> GetAllAsync();
    Task<PlotDocumentofownershipDto?> CreateAsync(CreatePlotDocumentofownershipDto dto);
    Task DeleteAsync(int id);
}
