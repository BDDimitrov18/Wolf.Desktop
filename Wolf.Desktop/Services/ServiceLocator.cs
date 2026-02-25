using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public static class ServiceLocator
{
    private static ApiClient _apiClient = null!;

    public static IAuthService Auth { get; private set; } = null!;
    public static IRequestService Requests { get; private set; } = null!;
    public static IActivityService Activities { get; private set; } = null!;
    public static ITaskService Tasks { get; private set; } = null!;
    public static IClientService Clients { get; private set; } = null!;
    public static IEmployeeService Employees { get; private set; } = null!;
    public static IInvoiceService Invoices { get; private set; } = null!;
    public static IClientRequestRelService ClientRequestRels { get; private set; } = null!;
    public static IPlotService Plots { get; private set; } = null!;
    public static IDocumentsOfOwnershipService Documents { get; private set; } = null!;
    public static IActivityPlotRelService ActivityPlotRels { get; private set; } = null!;
    public static IPlotDocumentRelService PlotDocumentRels { get; private set; } = null!;
    public static IOwnerService Owners { get; private set; } = null!;
    public static IDocumentOwnerRelService DocOwnerRels { get; private set; } = null!;
    public static IPowerofattorneyService PowerofattorneyDocs { get; private set; } = null!;
    public static IDocPlotDocOwnerRelService DocPlotDocOwnerRels { get; private set; } = null!;
    public static IActivityTypeService ActivityTypes { get; private set; } = null!;
    public static ITaskTypeService TaskTypes { get; private set; } = null!;
    public static DataCacheService Cache { get; private set; } = null!;
    public static SignalRService Realtime { get; private set; } = null!;

    /// <summary>Maps EmployeeId → display name, populated by DataCacheService.</summary>
    public static Dictionary<int, string> EmployeesCache { get; } = new();

    public static void Initialize(string baseUrl = "http://localhost:5284/")
    {
        _apiClient = new ApiClient(baseUrl);

        Auth = new AuthService(_apiClient);
        Requests = new RequestService(_apiClient);
        Activities = new ActivityService(_apiClient);
        Tasks = new TaskService(_apiClient);
        Clients = new ClientService(_apiClient);
        Employees = new EmployeeService(_apiClient);
        Invoices = new InvoiceService(_apiClient);
        ClientRequestRels = new ClientRequestRelService(_apiClient);
        Plots = new PlotService(_apiClient);
        Documents = new DocumentsOfOwnershipService(_apiClient);
        ActivityPlotRels = new ActivityPlotRelService(_apiClient);
        PlotDocumentRels = new PlotDocumentRelService(_apiClient);
        Owners = new OwnerService(_apiClient);
        DocOwnerRels = new DocumentOwnerRelService(_apiClient);
        PowerofattorneyDocs = new PowerofattorneyService(_apiClient);
        DocPlotDocOwnerRels = new DocPlotDocOwnerRelService(_apiClient);
        ActivityTypes = new ActivityTypeService(_apiClient);
        TaskTypes = new TaskTypeService(_apiClient);
        Cache = new DataCacheService();
        Realtime = new SignalRService(baseUrl);
    }

    public static string ResolveEmployeeName(int? id)
    {
        if (id is null) return "";
        return EmployeesCache.TryGetValue(id.Value, out var name) ? name : $"#{id}";
    }
}
