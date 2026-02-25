using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public interface IClientService
{
    Task<IEnumerable<ClientDto>> GetAllAsync();
    Task<ClientDto?> GetByIdAsync(int id);
    Task<ClientDto?> CreateAsync(CreateClientDto dto);
    Task UpdateAsync(int id, ClientDto dto);
    Task DeleteAsync(int id);
}
