using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public class EmployeeService : IEmployeeService
{
    private readonly ApiClient _client;

    public EmployeeService(ApiClient client) => _client = client;

    public Task<IEnumerable<EmployeeDto>> GetAllAsync() =>
        _client.GetAsync<IEnumerable<EmployeeDto>>("api/employees")!;

    public Task<EmployeeDto?> GetByIdAsync(int id) =>
        _client.GetAsync<EmployeeDto>($"api/employees/{id}");

    public Task<EmployeeDto?> CreateAsync(CreateEmployeeDto dto) =>
        _client.PostAsync<EmployeeDto>("api/employees", dto);

    public Task UpdateAsync(int id, EmployeeDto dto) =>
        _client.PutAsync($"api/employees/{id}", dto);

    public Task DeleteAsync(int id) =>
        _client.DeleteAsync($"api/employees/{id}");
}
