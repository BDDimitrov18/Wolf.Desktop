using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public class ActivityPlotRelService : IActivityPlotRelService
{
    private readonly ApiClient _client;

    public ActivityPlotRelService(ApiClient client) => _client = client;

    public Task<IEnumerable<ActivityPlotrelashionshipDto>> GetAllAsync() =>
        _client.GetAsync<IEnumerable<ActivityPlotrelashionshipDto>>("api/activityplotrelashionships")!;

    public Task<ActivityPlotrelashionshipDto?> CreateAsync(CreateActivityPlotrelashionshipDto dto) =>
        _client.PostAsync<ActivityPlotrelashionshipDto>("api/activityplotrelashionships", dto);

    public Task DeleteAsync(int activityId, int plotId) =>
        _client.DeleteAsync($"api/activityplotrelashionships/{activityId}/{plotId}");
}
