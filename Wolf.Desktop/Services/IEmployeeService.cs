using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllAsync();
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<EmployeeDto?> CreateAsync(CreateEmployeeDto dto);
    Task UpdateAsync(int id, EmployeeDto dto);
    Task DeleteAsync(int id);
}
