using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class OrdersViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<OrderRowViewModel> _orders = [];
    [ObservableProperty] private ObservableCollection<OrderRowViewModel> _filteredOrders = [];
    [ObservableProperty] private OrderRowViewModel? _selectedOrder;
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private string _activeFilter = "Всички";
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private int _selectedDetailTabIndex;

    // Detail sub-view-models
    public ActivitiesDetailViewModel ActivitiesDetail { get; } = new();
    public ClientsDetailViewModel ClientsDetail { get; } = new();
    public PlotsDetailViewModel PlotsDetail { get; } = new();
    public InvoicesDetailViewModel InvoicesDetail { get; } = new();

    public static readonly string[] FilterOptions = ["Всички", "Active", "Archived"];

    public event Action<OrderRowViewModel?>? OpenEditOrderRequested;
    public event Action? OpenFiltersRequested;

    private OrderFiltersViewModel? _filters;
    public OrderFiltersViewModel? Filters
    {
        get => _filters;
        set
        {
            if (_filters is not null)
                _filters.FiltersApplied -= ApplyFilter;
            _filters = value;
            if (_filters is not null)
                _filters.FiltersApplied += ApplyFilter;
        }
    }

    public OrdersViewModel()
    {
        ServiceLocator.Cache.RequestsChanged += OnRequestsChanged;
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnActiveFilterChanged(string value) => ApplyFilter();

    partial void OnSelectedOrderChanged(OrderRowViewModel? value)
    {
        if (value is not null)
            LoadDetail(value.Requestid);
        else
        {
            ActivitiesDetail.Clear();
            ClientsDetail.Clear();
            InvoicesDetail.Clear();
            PlotsDetail.Clear();
        }
    }

    private void LoadDetail(int requestId)
    {
        ActivitiesDetail.LoadForOrder(requestId);
        ClientsDetail.LoadForOrder(requestId);
        InvoicesDetail.LoadForOrder(requestId);
        PlotsDetail.LoadForOrder(requestId);
    }

    public void LoadAsync()
    {
        IsLoading = true;
        try
        {
            var dtos = ServiceLocator.Cache.GetAllRequests();
            BuildOrderRows(dtos);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnRequestsChanged()
    {
        var dtos = ServiceLocator.Cache.GetAllRequests();
        BuildOrderRows(dtos);
    }

    private void BuildOrderRows(IReadOnlyList<RequestDto> dtos)
    {
        var selectedId = SelectedOrder?.Requestid;
        Orders = new ObservableCollection<OrderRowViewModel>(
            dtos.Select(dto =>
            {
                var row = OrderRowViewModel.FromDto(dto);
                row.EditRequested += r => OpenEditOrderRequested?.Invoke(r);
                row.DeleteRequested += r => _ = DeleteOrderAsync(r);
                return row;
            }));
        ApplyFilter();

        // Restore selection
        if (selectedId.HasValue)
            SelectedOrder = Orders.FirstOrDefault(o => o.Requestid == selectedId.Value);
    }

    private void ApplyFilter()
    {
        var q = SearchText?.Trim().ToLower() ?? "";
        var f = ActiveFilter;
        var today = DateTime.Today;
        var weekEnd = today.AddDays(7 - (int)today.DayOfWeek);
        var empId = ServiceLocator.Auth.CurrentEmployeeId ?? -1;
        var checkedEmployeeIds = Filters?.EmployeeItems
            .Where(e => e.IsChecked).Select(e => e.EmployeeId).ToHashSet() ?? [];

        var filtered = Orders.Where(o =>
        {
            // Quick filter bar
            if (f != "Всички" && !string.Equals(o.Status, f, StringComparison.OrdinalIgnoreCase))
                return false;

            // Search box
            if (!string.IsNullOrEmpty(q) && !o.Requestname.ToLower().Contains(q))
                return false;

            // Advanced filters (only if Filters is set and has active filters)
            if (Filters is null || !Filters.HasActiveFilters)
                return true;

            var fil = Filters;

            // Get related data lazily
            var activities = ServiceLocator.Cache.GetActivitiesForRequest(o.Requestid);
            var tasks = activities.SelectMany(a => ServiceLocator.Cache.GetTasksForActivity(a.Activityid)).ToList();
            IReadOnlyList<PlotDto>? plots = null;
            IReadOnlyList<PlotDto> GetPlots() => plots ??= ServiceLocator.Cache.GetPlotsForRequest(o.Requestid);

            // Overdue: has tasks with Finishdate < today and status != Done
            if (fil.FilterOverdue)
            {
                if (!tasks.Any(t => t.Finishdate.Date < today &&
                    !string.Equals(t.Status, "Done", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(t.Status, "Completed", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(t.Status, "завършена", StringComparison.OrdinalIgnoreCase)))
                    return false;
            }

            // Personal: user is order creator, activity executor, task executor, or task control
            if (fil.FilterPersonal)
            {
                var isPersonal = o.Requestcreatorid == empId ||
                    activities.Any(a => a.Executantid == empId) ||
                    tasks.Any(t => t.Executantid == empId || t.Controlid == empId);
                if (!isPersonal) return false;
            }

            // For today: tasks with Finishdate == today
            if (fil.FilterForToday)
            {
                if (!tasks.Any(t => t.Finishdate.Date == today))
                    return false;
            }

            // For week: tasks with Finishdate within this week
            if (fil.FilterForWeek)
            {
                if (!tasks.Any(t => t.Finishdate.Date >= today && t.Finishdate.Date <= weekEnd))
                    return false;
            }

            // Archive status
            if (!string.IsNullOrEmpty(fil.ArchiveStatus) && fil.ArchiveStatus != "Всички")
            {
                if (!string.Equals(o.Status, fil.ArchiveStatus, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            // Task status filter
            if (!string.IsNullOrEmpty(fil.TaskStatusFilter) && fil.TaskStatusFilter != "Всички")
            {
                if (!tasks.Any(t => string.Equals(t.Status, fil.TaskStatusFilter, StringComparison.OrdinalIgnoreCase)))
                    return false;
            }

            // Payment status
            if (!string.IsNullOrEmpty(fil.PaymentStatusFilter) && fil.PaymentStatusFilter != "Всички")
            {
                if (!string.Equals(o.Paymentstatus, fil.PaymentStatusFilter, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            // Order number
            if (!string.IsNullOrEmpty(fil.OrderNumber))
            {
                if (!o.Requestid.ToString().Contains(fil.OrderNumber.Trim()))
                    return false;
            }

            // Order name
            if (!string.IsNullOrEmpty(fil.OrderName))
            {
                if (!o.Requestname.ToLower().Contains(fil.OrderName.ToLower()))
                    return false;
            }

            // Comment search
            if (!string.IsNullOrEmpty(fil.CommentSearch))
            {
                if (string.IsNullOrEmpty(o.Comments) || !o.Comments.ToLower().Contains(fil.CommentSearch.ToLower()))
                    return false;
            }

            // Settlement (city on linked plots)
            if (!string.IsNullOrEmpty(fil.Settlement))
            {
                var s = fil.Settlement.ToLower();
                if (!GetPlots().Any(p => p.City?.ToLower().Contains(s) == true))
                    return false;
            }

            // Plot number
            if (!string.IsNullOrEmpty(fil.PlotNumber))
            {
                var pn = fil.PlotNumber.ToLower();
                if (!GetPlots().Any(p => p.Plotnumber.ToLower().Contains(pn)))
                    return false;
            }

            // Regulated plot number (UPI)
            if (!string.IsNullOrEmpty(fil.RegulatedPlotNumber))
            {
                var rp = fil.RegulatedPlotNumber.ToLower();
                if (!GetPlots().Any(p => p.Regulatedplotnumber?.ToLower().Contains(rp) == true))
                    return false;
            }

            // Neighborhood
            if (!string.IsNullOrEmpty(fil.Neighborhood))
            {
                var nb = fil.Neighborhood.ToLower();
                if (!GetPlots().Any(p => p.Neighborhood?.ToLower().Contains(nb) == true))
                    return false;
            }

            // Selected clients
            if (fil.SelectedClients.Count > 0)
            {
                var orderClients = ServiceLocator.Cache.GetClientsForRequest(o.Requestid);
                var clientIds = orderClients.Select(c => c.Clientid).ToHashSet();
                if (!fil.SelectedClients.Any(sc => clientIds.Contains(sc.Clientid)))
                    return false;
            }

            // Selected owners
            if (fil.SelectedOwners.Count > 0)
            {
                var ownerIds = new HashSet<int>();
                foreach (var plot in GetPlots())
                {
                    var docs = ServiceLocator.Cache.GetDocumentsForPlot(plot.Plotid);
                    foreach (var doc in docs)
                    {
                        var owners = ServiceLocator.Cache.GetOwnersForDocument(doc.Documentid);
                        foreach (var owner in owners)
                            ownerIds.Add(owner.Ownerid);
                    }
                }
                if (!fil.SelectedOwners.Any(so => ownerIds.Contains(so.Ownerid)))
                    return false;
            }

            // Checked employees
            if (checkedEmployeeIds.Count > 0)
            {
                var matchesEmployee =
                    (o.Requestcreatorid.HasValue && checkedEmployeeIds.Contains(o.Requestcreatorid.Value)) ||
                    activities.Any(a => checkedEmployeeIds.Contains(a.Executantid)) ||
                    tasks.Any(t => checkedEmployeeIds.Contains(t.Executantid) ||
                                   (t.Controlid.HasValue && checkedEmployeeIds.Contains(t.Controlid.Value)));
                if (!matchesEmployee) return false;
            }

            return true;
        });

        FilteredOrders = new ObservableCollection<OrderRowViewModel>(filtered);
    }

    [RelayCommand]
    private void OpenFilters() => OpenFiltersRequested?.Invoke();

    [RelayCommand]
    private void NewOrder() => OpenEditOrderRequested?.Invoke(null);

    [RelayCommand]
    private void SetFilter(string filter)
    {
        ActiveFilter = filter;
    }

    [RelayCommand]
    private void DeselectOrder() => SelectedOrder = null;

    private async Task DeleteOrderAsync(OrderRowViewModel row)
    {
        try
        {
            await ServiceLocator.Cache.DeleteRequestAsync(row.Requestid);
            if (SelectedOrder == row) SelectedOrder = null;
        }
        catch (Exception ex)
        {
            ServiceLocator.ShowError($"Грешка при изтриване на поръчка: {ex.Message}");
        }
    }
}
