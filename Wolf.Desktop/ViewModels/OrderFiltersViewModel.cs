using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class OrderFiltersViewModel : ViewModelBase
{
    // ── Checkboxes ──────────────────────────────────────────────
    [ObservableProperty] private bool _filterOverdue;
    [ObservableProperty] private bool _filterPersonal;
    [ObservableProperty] private bool _filterForToday;
    [ObservableProperty] private bool _filterForWeek;
    [ObservableProperty] private bool _filterStarred;

    // ── Dropdowns ───────────────────────────────────────────────
    [ObservableProperty] private string? _archiveStatus;
    [ObservableProperty] private string? _taskStatusFilter;
    [ObservableProperty] private string? _paymentStatusFilter;

    // ── Text inputs ─────────────────────────────────────────────
    [ObservableProperty] private string? _orderNumber;
    [ObservableProperty] private string? _settlement;
    [ObservableProperty] private string? _plotNumber;
    [ObservableProperty] private string? _regulatedPlotNumber;
    [ObservableProperty] private string? _neighborhood;
    [ObservableProperty] private string? _orderName;
    [ObservableProperty] private string? _commentSearch;

    // ── Multi-select: Clients ───────────────────────────────────
    [ObservableProperty] private ObservableCollection<ClientDto> _availableClients = [];
    [ObservableProperty] private ObservableCollection<ClientDto> _selectedClients = [];
    [ObservableProperty] private ClientDto? _clientToAdd;

    // ── Multi-select: Owners ────────────────────────────────────
    [ObservableProperty] private ObservableCollection<OwnerDto> _availableOwners = [];
    [ObservableProperty] private ObservableCollection<OwnerDto> _selectedOwners = [];
    [ObservableProperty] private OwnerDto? _ownerToAdd;

    // ── Employee checkboxes ─────────────────────────────────────
    [ObservableProperty] private ObservableCollection<EmployeeCheckItem> _employeeItems = [];

    // ── Dropdown options ────────────────────────────────────────
    public static readonly string[] ArchiveStatusOptions = ["Всички", "Active", "Archived"];
    public static readonly string[] TaskStatusOptions = ["Всички", "Зададена", "завършена", "оферта"];
    public static readonly string[] PaymentStatusOptions = ["Всички", "Платен", "Аванс", "Не платен"];

    public event Action? FiltersApplied;

    public OrderFiltersViewModel()
    {
        LoadPickerData();
        ServiceLocator.Cache.ClientsChanged += RefreshAvailableClients;
    }

    private void RefreshAvailableClients()
    {
        AvailableClients = new ObservableCollection<ClientDto>(ServiceLocator.Cache.GetAllClients());
    }

    // ── Auto-fire on checkbox changes ───────────────────────────
    partial void OnFilterOverdueChanged(bool value) => FiltersApplied?.Invoke();
    partial void OnFilterPersonalChanged(bool value) => FiltersApplied?.Invoke();
    partial void OnFilterForTodayChanged(bool value) => FiltersApplied?.Invoke();
    partial void OnFilterForWeekChanged(bool value) => FiltersApplied?.Invoke();
    partial void OnFilterStarredChanged(bool value) => FiltersApplied?.Invoke();

    // ── Auto-fire on dropdown changes ───────────────────────────
    partial void OnArchiveStatusChanged(string? value) => FiltersApplied?.Invoke();
    partial void OnTaskStatusFilterChanged(string? value) => FiltersApplied?.Invoke();
    partial void OnPaymentStatusFilterChanged(string? value) => FiltersApplied?.Invoke();

    /// <summary>Called by the View on text field LostFocus / Enter.</summary>
    public void NotifyTextFiltersApplied() => FiltersApplied?.Invoke();

    private void LoadPickerData()
    {
        AvailableClients = new ObservableCollection<ClientDto>(ServiceLocator.Cache.GetAllClients());
        AvailableOwners = new ObservableCollection<OwnerDto>(ServiceLocator.Cache.GetAllOwners());

        var employees = ServiceLocator.Cache.GetAllEmployees();
        EmployeeItems = new ObservableCollection<EmployeeCheckItem>(
            employees.Select(e => new EmployeeCheckItem
            {
                EmployeeId = e.Employeeid,
                Name = string.Join(" ",
                    new[] { e.Firstname, e.Secondname, e.Lastname }
                        .Where(s => !string.IsNullOrWhiteSpace(s))),
                IsChecked = false,
                OnCheckedChanged = () => FiltersApplied?.Invoke()
            }));
    }

    [RelayCommand]
    private void AddClient()
    {
        if (ClientToAdd is null) return;
        if (SelectedClients.Any(c => c.Clientid == ClientToAdd.Clientid)) return;
        SelectedClients.Add(ClientToAdd);
        ClientToAdd = null;
        FiltersApplied?.Invoke();
    }

    [RelayCommand]
    private void RemoveClient(ClientDto? client)
    {
        if (client is not null)
        {
            SelectedClients.Remove(client);
            FiltersApplied?.Invoke();
        }
    }

    [RelayCommand]
    private void AddOwner()
    {
        if (OwnerToAdd is null) return;
        if (SelectedOwners.Any(o => o.Ownerid == OwnerToAdd.Ownerid)) return;
        SelectedOwners.Add(OwnerToAdd);
        OwnerToAdd = null;
        FiltersApplied?.Invoke();
    }

    [RelayCommand]
    private void RemoveOwner(OwnerDto? owner)
    {
        if (owner is not null)
        {
            SelectedOwners.Remove(owner);
            FiltersApplied?.Invoke();
        }
    }

    [RelayCommand]
    private void Apply() => FiltersApplied?.Invoke();

    [RelayCommand]
    private void Reset()
    {
        FilterOverdue = false;
        FilterPersonal = false;
        FilterForToday = false;
        FilterForWeek = false;
        FilterStarred = false;

        ArchiveStatus = null;
        TaskStatusFilter = null;
        PaymentStatusFilter = null;

        OrderNumber = null;
        Settlement = null;
        PlotNumber = null;
        RegulatedPlotNumber = null;
        Neighborhood = null;
        OrderName = null;
        CommentSearch = null;

        SelectedClients.Clear();
        SelectedOwners.Clear();
        ClientToAdd = null;
        OwnerToAdd = null;

        foreach (var e in EmployeeItems)
            e.IsChecked = false;

        FiltersApplied?.Invoke();
    }

    /// <summary>
    /// Returns true if any filter is active.
    /// </summary>
    private static bool IsActiveDropdown(string? value) =>
        !string.IsNullOrEmpty(value) && value != "Всички";

    public bool HasActiveFilters =>
        FilterOverdue || FilterPersonal || FilterForToday || FilterForWeek || FilterStarred ||
        IsActiveDropdown(ArchiveStatus) || IsActiveDropdown(TaskStatusFilter) ||
        IsActiveDropdown(PaymentStatusFilter) || !string.IsNullOrEmpty(OrderNumber) ||
        !string.IsNullOrEmpty(Settlement) || !string.IsNullOrEmpty(PlotNumber) ||
        !string.IsNullOrEmpty(RegulatedPlotNumber) || !string.IsNullOrEmpty(Neighborhood) ||
        !string.IsNullOrEmpty(OrderName) || !string.IsNullOrEmpty(CommentSearch) ||
        SelectedClients.Count > 0 || SelectedOwners.Count > 0 ||
        EmployeeItems.Any(e => e.IsChecked);
}

public partial class EmployeeCheckItem : ObservableObject
{
    public int EmployeeId { get; set; }
    public string Name { get; set; } = "";
    public Action? OnCheckedChanged { get; set; }
    [ObservableProperty] private bool _isChecked;
    partial void OnIsCheckedChanged(bool value) => OnCheckedChanged?.Invoke();
}
