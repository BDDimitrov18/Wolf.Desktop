using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public interface IPowerofattorneyService
{
    Task<IEnumerable<PowerofattorneydocumentDto>> GetAllAsync();
    Task<PowerofattorneydocumentDto?> GetByIdAsync(int id);
    Task<PowerofattorneydocumentDto?> CreateAsync(CreatePowerofattorneydocumentDto dto);
    Task UpdateAsync(int id, PowerofattorneydocumentDto dto);
    Task DeleteAsync(int id);
}
