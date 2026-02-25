using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public class TaskService : ITaskService
{
    private readonly ApiClient _client;

    public TaskService(ApiClient client) => _client = client;

    public Task<IEnumerable<TaskDto>> GetAllAsync() =>
        _client.GetAsync<IEnumerable<TaskDto>>("api/tasks")!;

    public Task<TaskDto?> GetByIdAsync(int id) =>
        _client.GetAsync<TaskDto>($"api/tasks/{id}");

    public Task<TaskDto?> CreateAsync(CreateTaskDto dto) =>
        _client.PostAsync<TaskDto>("api/tasks", dto);

    public Task UpdateAsync(int id, TaskDto dto) =>
        _client.PutAsync($"api/tasks/{id}", dto);

    public Task DeleteAsync(int id) =>
        _client.DeleteAsync($"api/tasks/{id}");
}
