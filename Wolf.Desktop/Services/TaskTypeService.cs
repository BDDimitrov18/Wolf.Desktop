using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public class TaskTypeService : ITaskTypeService
{
    private readonly ApiClient _client;

    public TaskTypeService(ApiClient client) => _client = client;

    public Task<IEnumerable<TasktypeDto>> GetAllAsync() =>
        _client.GetAsync<IEnumerable<TasktypeDto>>("api/tasktypes")!;

    public Task<TasktypeDto?> CreateAsync(CreateTasktypeDto dto) =>
        _client.PostAsync<TasktypeDto>("api/tasktypes", dto);
}
