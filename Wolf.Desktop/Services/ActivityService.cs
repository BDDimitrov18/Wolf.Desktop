using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public class ActivityService : IActivityService
{
    private readonly ApiClient _client;

    public ActivityService(ApiClient client) => _client = client;

    public Task<IEnumerable<ActivityDto>> GetAllAsync() =>
        _client.GetAsync<IEnumerable<ActivityDto>>("api/activities")!;

    public Task<ActivityDto?> GetByIdAsync(int id) =>
        _client.GetAsync<ActivityDto>($"api/activities/{id}");

    public Task<ActivityDto?> CreateAsync(CreateActivityDto dto) =>
        _client.PostAsync<ActivityDto>("api/activities", dto);

    public Task UpdateAsync(int id, ActivityDto dto) =>
        _client.PutAsync($"api/activities/{id}", dto);

    public Task DeleteAsync(int id) =>
        _client.DeleteAsync($"api/activities/{id}");
}
