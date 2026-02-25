using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public interface IClientRequestRelService
{
    Task<IEnumerable<ClientRequestrelashionshipDto>> GetAllAsync();
    Task<ClientRequestrelashionshipDto?> CreateAsync(CreateClientRequestrelashionshipDto dto);
    Task DeleteAsync(int requestId, int clientId);
}
