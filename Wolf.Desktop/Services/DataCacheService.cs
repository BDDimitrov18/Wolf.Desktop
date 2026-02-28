using System.Text.Json;
using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public class DataCacheService
{
    // Primary stores
    private Dictionary<int, RequestDto> _requests = new();
    private Dictionary<int, ActivityDto> _activities = new();
    private Dictionary<int, TaskDto> _tasks = new();
    private Dictionary<int, ClientDto> _clients = new();
    private Dictionary<int, InvoiceDto> _invoices = new();
    private Dictionary<int, EmployeeDto> _employees = new();
    private Dictionary<int, PlotDto> _plots = new();
    private Dictionary<int, DocumentsofownershipDto> _documents = new();
    private List<ClientRequestrelashionshipDto> _clientRequestRels = [];
    private List<ActivityPlotrelashionshipDto> _activityPlotRels = [];
    private List<PlotDocumentofownershipDto> _plotDocumentRels = [];
    private Dictionary<int, OwnerDto> _owners = new();
    private List<DocumentofownershipOwnerrelashionshipDto> _docOwnerRels = [];
    private Dictionary<int, ActivitytypeDto> _activityTypes = new();
    private Dictionary<int, TasktypeDto> _taskTypes = new();
    private Dictionary<int, PowerofattorneydocumentDto> _powerofattorneyDocs = new();
    private List<DocumentplotDocumentowenerrelashionshipDto> _docPlotDocOwnerRels = [];

    // Grouped indexes (rebuilt on data change)
    private Dictionary<int, List<ActivityDto>> _activitiesByRequest = new();
    private Dictionary<int, List<ActivityDto>> _activitiesByEmployee = new();
    private Dictionary<int, List<TaskDto>> _tasksByActivity = new();
    private Dictionary<int, List<TaskDto>> _tasksByEmployee = new();
    private Dictionary<int, List<InvoiceDto>> _invoicesByRequest = new();
    private Dictionary<int, List<int>> _clientIdsByRequest = new();
    private Dictionary<int, List<int>> _plotIdsByActivity = new();
    private Dictionary<int, List<int>> _docIdsByPlot = new();
    private Dictionary<int, List<int>> _ownerIdsByDocument = new();
    private Dictionary<int, List<DocumentplotDocumentowenerrelashionshipDto>> _docPlotDocOwnerRelsByDocPlot = new();

    // Change events
    public event Action? RequestsChanged;
    public event Action? ActivitiesChanged;
    public event Action? TasksChanged;
    public event Action? ClientsChanged;
    public event Action? InvoicesChanged;
    public event Action? PlotsChanged;
    public event Action? DocumentsChanged;

    public async Task LoadAllAsync()
    {
        var requestsTask = ServiceLocator.Requests.GetAllAsync();
        var activitiesTask = ServiceLocator.Activities.GetAllAsync();
        var tasksTask = ServiceLocator.Tasks.GetAllAsync();
        var clientsTask = ServiceLocator.Clients.GetAllAsync();
        var invoicesTask = ServiceLocator.Invoices.GetAllAsync();
        var employeesTask = ServiceLocator.Employees.GetAllAsync();
        var relsTask = ServiceLocator.ClientRequestRels.GetAllAsync();
        var plotsTask = ServiceLocator.Plots.GetAllAsync();
        var documentsTask = ServiceLocator.Documents.GetAllAsync();
        var actPlotRelsTask = ServiceLocator.ActivityPlotRels.GetAllAsync();
        var plotDocRelsTask = ServiceLocator.PlotDocumentRels.GetAllAsync();
        var ownersTask = ServiceLocator.Owners.GetAllAsync();
        var docOwnerRelsTask = ServiceLocator.DocOwnerRels.GetAllAsync();
        var activityTypesTask = ServiceLocator.ActivityTypes.GetAllAsync();
        var taskTypesTask = ServiceLocator.TaskTypes.GetAllAsync();
        var poaDocsTask = ServiceLocator.PowerofattorneyDocs.GetAllAsync();
        var docPlotDocOwnerRelsTask = ServiceLocator.DocPlotDocOwnerRels.GetAllAsync();

        await Task.WhenAll(requestsTask, activitiesTask, tasksTask,
            clientsTask, invoicesTask, employeesTask, relsTask,
            plotsTask, documentsTask, actPlotRelsTask, plotDocRelsTask,
            ownersTask, docOwnerRelsTask, activityTypesTask, taskTypesTask,
            poaDocsTask, docPlotDocOwnerRelsTask);

        _requests = (await requestsTask).ToDictionary(r => r.Requestid);
        _activities = (await activitiesTask).ToDictionary(a => a.Activityid);
        _tasks = (await tasksTask).ToDictionary(t => t.Taskid);
        _clients = (await clientsTask).ToDictionary(c => c.Clientid);
        _invoices = (await invoicesTask).ToDictionary(i => i.Invoiceid);
        _employees = (await employeesTask).ToDictionary(e => e.Employeeid);
        _clientRequestRels = (await relsTask).ToList();
        _plots = (await plotsTask).ToDictionary(p => p.Plotid);
        _documents = (await documentsTask).ToDictionary(d => d.Documentid);
        _activityPlotRels = (await actPlotRelsTask).ToList();
        _plotDocumentRels = (await plotDocRelsTask).ToList();
        _owners = (await ownersTask).ToDictionary(o => o.Ownerid);
        _docOwnerRels = (await docOwnerRelsTask).ToList();
        _activityTypes = (await activityTypesTask).ToDictionary(at => at.Activitytypeid);
        _taskTypes = (await taskTypesTask).ToDictionary(tt => tt.Tasktypeid);
        _powerofattorneyDocs = (await poaDocsTask).ToDictionary(p => p.Powerofattorneyid);
        _docPlotDocOwnerRels = (await docPlotDocOwnerRelsTask).ToList();

        RebuildIndexes();
    }

    private void RebuildIndexes()
    {
        _activitiesByRequest = _activities.Values
            .GroupBy(a => a.Requestid)
            .ToDictionary(g => g.Key, g => g.ToList());

        _activitiesByEmployee = _activities.Values
            .GroupBy(a => a.Executantid)
            .ToDictionary(g => g.Key, g => g.ToList());

        _tasksByActivity = _tasks.Values
            .GroupBy(t => t.Activityid)
            .ToDictionary(g => g.Key, g => g.ToList());

        _tasksByEmployee = _tasks.Values
            .GroupBy(t => t.Executantid)
            .ToDictionary(g => g.Key, g => g.ToList());

        _invoicesByRequest = _invoices.Values
            .GroupBy(i => i.Requestid)
            .ToDictionary(g => g.Key, g => g.ToList());

        _clientIdsByRequest = _clientRequestRels
            .GroupBy(r => r.Requestid)
            .ToDictionary(g => g.Key, g => g.Select(r => r.Clientid).ToList());

        _plotIdsByActivity = _activityPlotRels
            .GroupBy(r => r.Activityid)
            .ToDictionary(g => g.Key, g => g.Select(r => r.Plotid).ToList());

        _docIdsByPlot = _plotDocumentRels
            .GroupBy(r => r.Plotid)
            .ToDictionary(g => g.Key, g => g.Select(r => r.Documentofownershipid).ToList());

        _ownerIdsByDocument = _docOwnerRels
            .GroupBy(r => r.Documentid)
            .ToDictionary(g => g.Key, g => g.Select(r => r.Ownerid).ToList());

        _docPlotDocOwnerRelsByDocPlot = _docPlotDocOwnerRels
            .GroupBy(r => r.Documentplotid)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Rebuild employee name cache in ServiceLocator
        ServiceLocator.EmployeesCache.Clear();
        foreach (var e in _employees.Values)
        {
            var name = string.Join(" ",
                new[] { e.Firstname, e.Secondname, e.Lastname }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
            ServiceLocator.EmployeesCache[e.Employeeid] = name;
        }
    }

    public void Clear()
    {
        _requests.Clear();
        _activities.Clear();
        _tasks.Clear();
        _clients.Clear();
        _invoices.Clear();
        _employees.Clear();
        _plots.Clear();
        _documents.Clear();
        _clientRequestRels.Clear();
        _activityPlotRels.Clear();
        _plotDocumentRels.Clear();
        _activitiesByRequest.Clear();
        _activitiesByEmployee.Clear();
        _tasksByActivity.Clear();
        _tasksByEmployee.Clear();
        _invoicesByRequest.Clear();
        _clientIdsByRequest.Clear();
        _plotIdsByActivity.Clear();
        _docIdsByPlot.Clear();
        _owners.Clear();
        _docOwnerRels.Clear();
        _ownerIdsByDocument.Clear();
        _activityTypes.Clear();
        _taskTypes.Clear();
        _powerofattorneyDocs.Clear();
        _docPlotDocOwnerRels.Clear();
        _docPlotDocOwnerRelsByDocPlot.Clear();
        ServiceLocator.EmployeesCache.Clear();
    }

    // ── Query methods ──────────────────────────────────────────────

    public IReadOnlyList<RequestDto> GetAllRequests() =>
        _requests.Values.ToList();

    public RequestDto? GetRequest(int id) =>
        _requests.GetValueOrDefault(id);

    public IReadOnlyList<ActivityDto> GetActivitiesForRequest(int requestId) =>
        _activitiesByRequest.TryGetValue(requestId, out var list) ? list : [];

    public IReadOnlyList<TaskDto> GetTasksForActivity(int activityId) =>
        _tasksByActivity.TryGetValue(activityId, out var list) ? list : [];

    public IReadOnlyList<ClientDto> GetClientsForRequest(int requestId)
    {
        if (!_clientIdsByRequest.TryGetValue(requestId, out var clientIds))
            return [];
        return clientIds
            .Where(id => _clients.ContainsKey(id))
            .Select(id => _clients[id])
            .ToList();
    }

    public IReadOnlyList<InvoiceDto> GetInvoicesForRequest(int requestId) =>
        _invoicesByRequest.TryGetValue(requestId, out var list) ? list : [];

    public IReadOnlyList<InvoiceDto> GetAllInvoices() =>
        _invoices.Values.ToList();

    public InvoiceDto? GetInvoice(int id) => _invoices.GetValueOrDefault(id);

    public IReadOnlyList<ClientDto> GetAllClients() =>
        _clients.Values.ToList();

    public ClientDto? GetClient(int id) => _clients.GetValueOrDefault(id);

    public IReadOnlyList<EmployeeDto> GetAllEmployees() =>
        _employees.Values.ToList();

    public EmployeeDto? GetEmployee(int id) => _employees.GetValueOrDefault(id);

    /// <summary>Gets all tasks assigned to a specific employee.</summary>
    public IReadOnlyList<TaskDto> GetTasksForEmployee(int employeeId) =>
        _tasksByEmployee.TryGetValue(employeeId, out var list) ? list : [];

    /// <summary>Gets all activities assigned to a specific employee.</summary>
    public IReadOnlyList<ActivityDto> GetActivitiesForEmployee(int employeeId) =>
        _activitiesByEmployee.TryGetValue(employeeId, out var list) ? list : [];

    /// <summary>
    /// Gets all plots linked to an order via: request → activities → activity-plot rels → plots.
    /// </summary>
    public IReadOnlyList<PlotDto> GetPlotsForRequest(int requestId)
    {
        if (!_activitiesByRequest.TryGetValue(requestId, out var activities))
            return [];

        var plotIds = new HashSet<int>();
        foreach (var act in activities)
        {
            if (_plotIdsByActivity.TryGetValue(act.Activityid, out var pIds))
                plotIds.UnionWith(pIds);
        }

        return plotIds
            .Where(id => _plots.ContainsKey(id))
            .Select(id => _plots[id])
            .ToList();
    }

    /// <summary>
    /// Gets all documents linked to a plot via: plot → plot-document rels → documents.
    /// </summary>
    public IReadOnlyList<DocumentsofownershipDto> GetDocumentsForPlot(int plotId)
    {
        if (!_docIdsByPlot.TryGetValue(plotId, out var docIds))
            return [];
        return docIds
            .Where(id => _documents.ContainsKey(id))
            .Select(id => _documents[id])
            .ToList();
    }

    public IReadOnlyList<PlotDto> GetPlotsForActivity(int activityId)
    {
        if (!_plotIdsByActivity.TryGetValue(activityId, out var plotIds))
            return [];
        return plotIds
            .Where(id => _plots.ContainsKey(id))
            .Select(id => _plots[id])
            .ToList();
    }

    public IReadOnlyList<PlotDto> GetAllPlots() =>
        _plots.Values.ToList();

    public IReadOnlyList<DocumentsofownershipDto> GetAllDocuments() =>
        _documents.Values.ToList();

    public PlotDto? GetPlot(int id) => _plots.GetValueOrDefault(id);
    public DocumentsofownershipDto? GetDocument(int id) => _documents.GetValueOrDefault(id);

    public IReadOnlyList<OwnerDto> GetAllOwners() =>
        _owners.Values.ToList();

    public OwnerDto? GetOwner(int id) => _owners.GetValueOrDefault(id);

    public IReadOnlyList<OwnerDto> GetOwnersForDocument(int docId)
    {
        if (!_ownerIdsByDocument.TryGetValue(docId, out var ownerIds))
            return [];
        return ownerIds
            .Where(id => _owners.ContainsKey(id))
            .Select(id => _owners[id])
            .ToList();
    }

    // Power of Attorney documents
    public IReadOnlyList<PowerofattorneydocumentDto> GetAllPowerofattorneyDocs() =>
        _powerofattorneyDocs.Values.ToList();

    public PowerofattorneydocumentDto? GetPowerofattorneyDoc(int id) =>
        _powerofattorneyDocs.GetValueOrDefault(id);

    // Three-way rels (DocumentplotDocumentowenerrelashionship)
    public IReadOnlyList<DocumentplotDocumentowenerrelashionshipDto> GetDocPlotDocOwnerRelsForDocPlot(int documentplotId) =>
        _docPlotDocOwnerRelsByDocPlot.TryGetValue(documentplotId, out var list) ? list : [];

    // Find the DocOwnerRel for a specific document+owner pair
    public DocumentofownershipOwnerrelashionshipDto? GetDocOwnerRelForDocumentAndOwner(int docId, int ownerId) =>
        _docOwnerRels.FirstOrDefault(r => r.Documentid == docId && r.Ownerid == ownerId);

    // Find the PlotDocumentRel for a specific plot+document pair
    public PlotDocumentofownershipDto? GetPlotDocumentRel(int plotId, int documentId) =>
        _plotDocumentRels.FirstOrDefault(r => r.Plotid == plotId && r.Documentofownershipid == documentId);

    /// <summary>
    /// Checks how many requests reference this plot (via activities).
    /// Returns true if more than one request uses it.
    /// </summary>
    public bool IsPlotShared(int plotId)
    {
        var requestIds = new HashSet<int>();
        foreach (var rel in _activityPlotRels.Where(r => r.Plotid == plotId))
        {
            if (_activities.TryGetValue(rel.Activityid, out var act))
                requestIds.Add(act.Requestid);
        }
        return requestIds.Count > 1;
    }

    // Activity types
    public IReadOnlyList<ActivitytypeDto> GetAllActivityTypes() =>
        _activityTypes.Values.ToList();

    public ActivitytypeDto? GetActivityType(int id) =>
        _activityTypes.GetValueOrDefault(id);

    public async Task<ActivitytypeDto?> CreateActivityTypeAsync(CreateActivitytypeDto dto)
    {
        var created = await ServiceLocator.ActivityTypes.CreateAsync(dto);
        if (created is not null)
            _activityTypes[created.Activitytypeid] = created;
        return created;
    }

    // Task types
    public IReadOnlyList<TasktypeDto> GetAllTaskTypes() =>
        _taskTypes.Values.ToList();

    public IReadOnlyList<TasktypeDto> GetTaskTypesForActivityType(int activityTypeId) =>
        _taskTypes.Values.Where(tt => tt.Activitytypeid == activityTypeId).ToList();

    public TasktypeDto? GetTaskType(int id) =>
        _taskTypes.GetValueOrDefault(id);

    public async Task<TasktypeDto?> CreateTaskTypeAsync(CreateTasktypeDto dto)
    {
        var created = await ServiceLocator.TaskTypes.CreateAsync(dto);
        if (created is not null)
            _taskTypes[created.Tasktypeid] = created;
        return created;
    }

    // ── Mutation methods (API + cache) ─────────────────────────────

    // Requests
    public async Task<RequestDto?> CreateRequestAsync(CreateRequestDto dto)
    {
        var created = await ServiceLocator.Requests.CreateAsync(dto);
        if (created is not null)
        {
            _requests[created.Requestid] = created;
            RequestsChanged?.Invoke();
        }
        return created;
    }

    public async Task UpdateRequestAsync(int id, RequestDto dto)
    {
        await ServiceLocator.Requests.UpdateAsync(id, dto);
        _requests[id] = dto;
        RequestsChanged?.Invoke();
    }

    public async Task DeleteRequestAsync(int id)
    {
        await ServiceLocator.Requests.DeleteAsync(id);

        // Cascade-clean local cache: tasks → activities → invoices → rels → request
        var activityIds = _activities.Values
            .Where(a => a.Requestid == id)
            .Select(a => a.Activityid).ToList();

        foreach (var aid in activityIds)
        {
            var taskIds = _tasks.Values
                .Where(t => t.Activityid == aid)
                .Select(t => t.Taskid).ToList();
            foreach (var tid in taskIds) _tasks.Remove(tid);
            _activities.Remove(aid);
        }

        _activityPlotRels.RemoveAll(r => activityIds.Contains(r.Activityid));
        _clientRequestRels.RemoveAll(r => r.Requestid == id);

        var invoiceIds = _invoices.Values
            .Where(i => i.Requestid == id)
            .Select(i => i.Invoiceid).ToList();
        foreach (var iid in invoiceIds) _invoices.Remove(iid);

        _requests.Remove(id);

        RebuildActivityIndex();
        RebuildTaskIndex();
        RebuildInvoiceIndex();
        RebuildClientRequestIndex();
        RebuildActivityPlotIndex();

        TasksChanged?.Invoke();
        ActivitiesChanged?.Invoke();
        InvoicesChanged?.Invoke();
        ClientsChanged?.Invoke();
        PlotsChanged?.Invoke();
        RequestsChanged?.Invoke();
    }

    // Clients
    public async Task<ClientDto?> CreateClientAsync(CreateClientDto dto)
    {
        var created = await ServiceLocator.Clients.CreateAsync(dto);
        if (created is not null)
        {
            _clients[created.Clientid] = created;
            ClientsChanged?.Invoke();
        }
        return created;
    }

    public async Task UpdateClientAsync(int id, ClientDto dto)
    {
        await ServiceLocator.Clients.UpdateAsync(id, dto);
        _clients[id] = dto;
        ClientsChanged?.Invoke();
    }

    public async Task DeleteClientAsync(int id)
    {
        await ServiceLocator.Clients.DeleteAsync(id);
        _clients.Remove(id);
        ClientsChanged?.Invoke();
    }

    // Activities
    public async Task<ActivityDto?> CreateActivityAsync(CreateActivityDto dto)
    {
        var created = await ServiceLocator.Activities.CreateAsync(dto);
        if (created is not null)
        {
            _activities[created.Activityid] = created;
            RebuildActivityIndex();
            ActivitiesChanged?.Invoke();
        }
        return created;
    }

    public async Task UpdateActivityAsync(int id, ActivityDto dto)
    {
        await ServiceLocator.Activities.UpdateAsync(id, dto);
        _activities[id] = dto;
        RebuildActivityIndex();
        ActivitiesChanged?.Invoke();
    }

    public async Task DeleteActivityAsync(int id)
    {
        await ServiceLocator.Activities.DeleteAsync(id);

        // Cascade-clean local cache: tasks → activity-plot rels → activity
        var taskIds = _tasks.Values
            .Where(t => t.Activityid == id)
            .Select(t => t.Taskid).ToList();
        foreach (var tid in taskIds) _tasks.Remove(tid);

        _activityPlotRels.RemoveAll(r => r.Activityid == id);
        _activities.Remove(id);

        // DB sets Parentactivityid = null via ON DELETE SET NULL; sync local cache
        foreach (var child in _activities.Values.Where(a => a.Parentactivityid == id))
            child.Parentactivityid = null;

        RebuildActivityIndex();
        RebuildTaskIndex();
        RebuildActivityPlotIndex();

        TasksChanged?.Invoke();
        ActivitiesChanged?.Invoke();
        PlotsChanged?.Invoke();
    }

    // Tasks
    public async Task<TaskDto?> CreateTaskAsync(CreateTaskDto dto)
    {
        var created = await ServiceLocator.Tasks.CreateAsync(dto);
        if (created is not null)
        {
            _tasks[created.Taskid] = created;
            RebuildTaskIndex();
            TasksChanged?.Invoke();
        }
        return created;
    }

    public async Task UpdateTaskAsync(int id, TaskDto dto)
    {
        await ServiceLocator.Tasks.UpdateAsync(id, dto);
        _tasks[id] = dto;
        RebuildTaskIndex();
        TasksChanged?.Invoke();
    }

    public async Task DeleteTaskAsync(int id)
    {
        await ServiceLocator.Tasks.DeleteAsync(id);
        _tasks.Remove(id);
        RebuildTaskIndex();
        TasksChanged?.Invoke();
    }

    // Plots
    public async Task<PlotDto?> CreatePlotAsync(CreatePlotDto dto)
    {
        var created = await ServiceLocator.Plots.CreateAsync(dto);
        if (created is not null)
        {
            _plots[created.Plotid] = created;
            PlotsChanged?.Invoke();
        }
        return created;
    }

    public async Task UpdatePlotAsync(int id, PlotDto dto)
    {
        await ServiceLocator.Plots.UpdateAsync(id, dto);
        _plots[id] = dto;
        PlotsChanged?.Invoke();
    }

    public async Task DeletePlotAsync(int id)
    {
        await ServiceLocator.Plots.DeleteAsync(id);
        _plots.Remove(id);
        // Also remove activity-plot rels referencing this plot
        _activityPlotRels.RemoveAll(r => r.Plotid == id);
        _plotDocumentRels.RemoveAll(r => r.Plotid == id);
        RebuildPlotIndexes();
        PlotsChanged?.Invoke();
    }

    // Documents
    public async Task<DocumentsofownershipDto?> CreateDocumentAsync(CreateDocumentsofownershipDto dto)
    {
        var created = await ServiceLocator.Documents.CreateAsync(dto);
        if (created is not null)
        {
            _documents[created.Documentid] = created;
            DocumentsChanged?.Invoke();
        }
        return created;
    }

    public async Task UpdateDocumentAsync(int id, DocumentsofownershipDto dto)
    {
        await ServiceLocator.Documents.UpdateAsync(id, dto);
        _documents[id] = dto;
        DocumentsChanged?.Invoke();
    }

    public async Task DeleteDocumentAsync(int id)
    {
        await ServiceLocator.Documents.DeleteAsync(id);

        // Cascade-clean local cache: relationships only (not owners or plots)
        var plotDocRelIds = _plotDocumentRels
            .Where(r => r.Documentofownershipid == id)
            .Select(r => r.Documentplotid).ToList();

        _docPlotDocOwnerRels.RemoveAll(r => plotDocRelIds.Contains(r.Documentplotid));
        _docOwnerRels.RemoveAll(r => r.Documentid == id);
        _plotDocumentRels.RemoveAll(r => r.Documentofownershipid == id);
        _documents.Remove(id);

        RebuildPlotDocIndex();
        RebuildDocOwnerIndex();
        RebuildDocPlotDocOwnerRelIndex();
        DocumentsChanged?.Invoke();
    }

    // Invoices
    public async Task<InvoiceDto?> CreateInvoiceAsync(CreateInvoiceDto dto)
    {
        var created = await ServiceLocator.Invoices.CreateAsync(dto);
        if (created is not null)
        {
            _invoices[created.Invoiceid] = created;
            RebuildInvoiceIndex();
            InvoicesChanged?.Invoke();
        }
        return created;
    }

    public async Task UpdateInvoiceAsync(int id, InvoiceDto dto)
    {
        await ServiceLocator.Invoices.UpdateAsync(id, dto);
        _invoices[id] = dto;
        RebuildInvoiceIndex();
        InvoicesChanged?.Invoke();
    }

    public async Task DeleteInvoiceAsync(int id)
    {
        await ServiceLocator.Invoices.DeleteAsync(id);
        _invoices.Remove(id);
        RebuildInvoiceIndex();
        InvoicesChanged?.Invoke();
    }

    // Client-Request relationship
    public async Task LinkClientToRequestAsync(int requestId, int clientId)
    {
        var created = await ServiceLocator.ClientRequestRels.CreateAsync(
            new CreateClientRequestrelashionshipDto { Requestid = requestId, Clientid = clientId });
        if (created is not null)
        {
            _clientRequestRels.Add(created);
            RebuildClientRequestIndex();
            ClientsChanged?.Invoke();
        }
    }

    public async Task UnlinkClientFromRequestAsync(int requestId, int clientId)
    {
        await ServiceLocator.ClientRequestRels.DeleteAsync(requestId, clientId);
        _clientRequestRels.RemoveAll(r => r.Requestid == requestId && r.Clientid == clientId);
        RebuildClientRequestIndex();
        ClientsChanged?.Invoke();
    }

    // Activity-Plot relationship
    public async Task LinkPlotToActivityAsync(int activityId, int plotId)
    {
        var created = await ServiceLocator.ActivityPlotRels.CreateAsync(
            new CreateActivityPlotrelashionshipDto { Activityid = activityId, Plotid = plotId });
        if (created is not null)
        {
            _activityPlotRels.Add(created);
            RebuildActivityPlotIndex();
            PlotsChanged?.Invoke();
        }
    }

    public async Task UnlinkPlotFromActivityAsync(int activityId, int plotId)
    {
        await ServiceLocator.ActivityPlotRels.DeleteAsync(activityId, plotId);
        _activityPlotRels.RemoveAll(r => r.Activityid == activityId && r.Plotid == plotId);
        RebuildActivityPlotIndex();
        PlotsChanged?.Invoke();
    }

    // Plot-Document relationship
    public async Task LinkDocumentToPlotAsync(int plotId, int documentId)
    {
        var created = await ServiceLocator.PlotDocumentRels.CreateAsync(
            new CreatePlotDocumentofownershipDto { Plotid = plotId, Documentofownershipid = documentId });
        if (created is not null)
        {
            _plotDocumentRels.Add(created);
            RebuildPlotDocIndex();
            DocumentsChanged?.Invoke();
        }
    }

    public async Task UnlinkDocumentFromPlotAsync(int documentPlotId)
    {
        var rel = _plotDocumentRels.FirstOrDefault(r => r.Documentplotid == documentPlotId);
        if (rel is not null)
        {
            await ServiceLocator.PlotDocumentRels.DeleteAsync(documentPlotId);
            _plotDocumentRels.Remove(rel);
            RebuildPlotDocIndex();
            DocumentsChanged?.Invoke();
        }
    }

    // Power of Attorney documents
    public async Task<PowerofattorneydocumentDto?> CreatePowerofattorneyDocAsync(CreatePowerofattorneydocumentDto dto)
    {
        var created = await ServiceLocator.PowerofattorneyDocs.CreateAsync(dto);
        if (created is not null)
            _powerofattorneyDocs[created.Powerofattorneyid] = created;
        return created;
    }

    // Three-way rels (DocumentplotDocumentowenerrelashionship)
    public async Task<DocumentplotDocumentowenerrelashionshipDto?> CreateDocPlotDocOwnerRelAsync(
        CreateDocumentplotDocumentowenerrelashionshipDto dto)
    {
        var created = await ServiceLocator.DocPlotDocOwnerRels.CreateAsync(dto);
        if (created is not null)
        {
            _docPlotDocOwnerRels.Add(created);
            RebuildDocPlotDocOwnerRelIndex();
        }
        return created;
    }

    public async Task DeleteDocPlotDocOwnerRelAsync(int id)
    {
        await ServiceLocator.DocPlotDocOwnerRels.DeleteAsync(id);
        _docPlotDocOwnerRels.RemoveAll(r => r.Id == id);
        RebuildDocPlotDocOwnerRelIndex();
    }

    // Document-Owner relationship
    public async Task<DocumentofownershipOwnerrelashionshipDto?> CreateDocOwnerRelAsync(
        CreateDocumentofownershipOwnerrelashionshipDto dto)
    {
        var created = await ServiceLocator.DocOwnerRels.CreateAsync(dto);
        if (created is not null)
        {
            _docOwnerRels.Add(created);
            RebuildDocOwnerIndex();
        }
        return created;
    }

    // Owners
    public async Task<OwnerDto?> CreateOwnerAsync(CreateOwnerDto dto)
    {
        var created = await ServiceLocator.Owners.CreateAsync(dto);
        if (created is not null)
            _owners[created.Ownerid] = created;
        return created;
    }

    // Partial index rebuilds
    private void RebuildActivityIndex()
    {
        _activitiesByRequest = _activities.Values
            .GroupBy(a => a.Requestid)
            .ToDictionary(g => g.Key, g => g.ToList());
        _activitiesByEmployee = _activities.Values
            .GroupBy(a => a.Executantid)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private void RebuildTaskIndex()
    {
        _tasksByActivity = _tasks.Values
            .GroupBy(t => t.Activityid)
            .ToDictionary(g => g.Key, g => g.ToList());
        _tasksByEmployee = _tasks.Values
            .GroupBy(t => t.Executantid)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private void RebuildInvoiceIndex()
    {
        _invoicesByRequest = _invoices.Values
            .GroupBy(i => i.Requestid)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private void RebuildClientRequestIndex()
    {
        _clientIdsByRequest = _clientRequestRels
            .GroupBy(r => r.Requestid)
            .ToDictionary(g => g.Key, g => g.Select(r => r.Clientid).ToList());
    }

    private void RebuildActivityPlotIndex()
    {
        _plotIdsByActivity = _activityPlotRels
            .GroupBy(r => r.Activityid)
            .ToDictionary(g => g.Key, g => g.Select(r => r.Plotid).ToList());
    }

    private void RebuildPlotDocIndex()
    {
        _docIdsByPlot = _plotDocumentRels
            .GroupBy(r => r.Plotid)
            .ToDictionary(g => g.Key, g => g.Select(r => r.Documentofownershipid).ToList());
    }

    private void RebuildPlotIndexes()
    {
        RebuildActivityPlotIndex();
        RebuildPlotDocIndex();
    }

    private void RebuildDocPlotDocOwnerRelIndex()
    {
        _docPlotDocOwnerRelsByDocPlot = _docPlotDocOwnerRels
            .GroupBy(r => r.Documentplotid)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private void RebuildDocOwnerIndex()
    {
        _ownerIdsByDocument = _docOwnerRels
            .GroupBy(r => r.Documentid)
            .ToDictionary(g => g.Key, g => g.Select(r => r.Ownerid).ToList());
    }

    // ── Remote change handling (SignalR) ──────────────────────────

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public void ApplyRemoteChange(string entityType, string action, string jsonPayload)
    {
        switch (entityType)
        {
            case "Request":
                ApplyDictChange(_requests, jsonPayload, action, d => d.Requestid,
                    () => RequestsChanged?.Invoke());
                break;
            case "Activity":
                ApplyDictChange(_activities, jsonPayload, action, d => d.Activityid,
                    () => { RebuildActivityIndex(); ActivitiesChanged?.Invoke(); });
                break;
            case "Task":
                ApplyDictChange(_tasks, jsonPayload, action, d => d.Taskid,
                    () => { RebuildTaskIndex(); TasksChanged?.Invoke(); });
                break;
            case "Client":
                ApplyDictChange(_clients, jsonPayload, action, d => d.Clientid,
                    () => ClientsChanged?.Invoke());
                break;
            case "Invoice":
                ApplyDictChange(_invoices, jsonPayload, action, d => d.Invoiceid,
                    () => { RebuildInvoiceIndex(); InvoicesChanged?.Invoke(); });
                break;
            case "Employee":
                ApplyDictChange(_employees, jsonPayload, action, d => d.Employeeid, () =>
                {
                    ServiceLocator.EmployeesCache.Clear();
                    foreach (var e in _employees.Values)
                    {
                        var name = string.Join(" ",
                            new[] { e.Firstname, e.Secondname, e.Lastname }
                            .Where(s => !string.IsNullOrWhiteSpace(s)));
                        ServiceLocator.EmployeesCache[e.Employeeid] = name;
                    }
                });
                break;
            case "Plot":
                ApplyDictChange(_plots, jsonPayload, action, d => d.Plotid,
                    () => PlotsChanged?.Invoke());
                break;
            case "Document":
                ApplyDictChange(_documents, jsonPayload, action, d => d.Documentid,
                    () => DocumentsChanged?.Invoke());
                break;
            case "Owner":
                ApplyDictChange(_owners, jsonPayload, action, d => d.Ownerid, null);
                break;
            case "ActivityType":
                ApplyDictChange(_activityTypes, jsonPayload, action, d => d.Activitytypeid, null);
                break;
            case "TaskType":
                ApplyDictChange(_taskTypes, jsonPayload, action, d => d.Tasktypeid, null);
                break;
            case "PowerofattorneyDoc":
                ApplyDictChange(_powerofattorneyDocs, jsonPayload, action, d => d.Powerofattorneyid, null);
                break;
            case "ClientRequestRel":
                ApplyListChange<ClientRequestrelashionshipDto>(_clientRequestRels, jsonPayload, action,
                    (list, d) => list.RemoveAll(r => r.Requestid == d.Requestid && r.Clientid == d.Clientid),
                    () => { RebuildClientRequestIndex(); ClientsChanged?.Invoke(); });
                break;
            case "ActivityPlotRel":
                ApplyListChange<ActivityPlotrelashionshipDto>(_activityPlotRels, jsonPayload, action,
                    (list, d) => list.RemoveAll(r => r.Activityid == d.Activityid && r.Plotid == d.Plotid),
                    () => { RebuildActivityPlotIndex(); PlotsChanged?.Invoke(); });
                break;
            case "PlotDocumentRel":
                ApplyListChange<PlotDocumentofownershipDto>(_plotDocumentRels, jsonPayload, action,
                    (list, d) => list.RemoveAll(r => r.Documentplotid == d.Documentplotid),
                    () => { RebuildPlotDocIndex(); DocumentsChanged?.Invoke(); });
                break;
            case "DocOwnerRel":
                ApplyListChange<DocumentofownershipOwnerrelashionshipDto>(_docOwnerRels, jsonPayload, action,
                    (list, d) => list.RemoveAll(r => r.Documentownerid == d.Documentownerid),
                    () => RebuildDocOwnerIndex());
                break;
            case "DocPlotDocOwnerRel":
                ApplyListChange<DocumentplotDocumentowenerrelashionshipDto>(_docPlotDocOwnerRels, jsonPayload, action,
                    (list, d) => list.RemoveAll(r => r.Id == d.Id),
                    () => RebuildDocPlotDocOwnerRelIndex());
                break;
        }
    }

    private void ApplyDictChange<T>(Dictionary<int, T> dict, string json, string action,
        Func<T, int> getId, Action? onChange)
    {
        if (action is "Created" or "Updated")
        {
            var dto = JsonSerializer.Deserialize<T>(json, _jsonOpts);
            if (dto is not null)
                dict[getId(dto)] = dto;
        }
        else if (action == "Deleted")
        {
            var id = JsonSerializer.Deserialize<JsonElement>(json, _jsonOpts)
                .GetProperty("Id").GetInt32();
            dict.Remove(id);
        }
        onChange?.Invoke();
    }

    private void ApplyListChange<T>(List<T> list, string json, string action,
        Action<List<T>, T> removeMatch, Action? onChange)
    {
        var dto = JsonSerializer.Deserialize<T>(json, _jsonOpts);
        if (dto is null) return;

        if (action is "Created")
        {
            list.Add(dto);
        }
        else if (action is "Updated")
        {
            removeMatch(list, dto);
            list.Add(dto);
        }
        else if (action is "Deleted")
        {
            removeMatch(list, dto);
        }
        onChange?.Invoke();
    }
}
