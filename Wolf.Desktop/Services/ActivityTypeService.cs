using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public class ActivityTypeService : IActivityTypeService
{
    private readonly ApiClient _client;

    public ActivityTypeService(ApiClient client) => _client = client;

    public Task<IEnumerable<ActivitytypeDto>> GetAllAsync() =>
        _client.GetAsync<IEnumerable<ActivitytypeDto>>("api/activitytypes")!;

    public Task<ActivitytypeDto?> CreateAsync(CreateActivitytypeDto dto) =>
        _client.PostAsync<ActivitytypeDto>("api/activitytypes", dto);
}
