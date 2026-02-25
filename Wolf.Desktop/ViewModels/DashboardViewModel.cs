using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    // ── Summary cards ────────────────────────────────────────────
    [ObservableProperty] private int _totalOrders;
    [ObservableProperty] private int _activeOrders;
    [ObservableProperty] private int _archivedOrders;
    [ObservableProperty] private int _totalClients;
    [ObservableProperty] private int _totalActivities;
    [ObservableProperty] private int _totalPlots;

    // ── Financial ────────────────────────────────────────────────
    [ObservableProperty] private double _totalRevenue;
    [ObservableProperty] private double _totalAdvance;
    [ObservableProperty] private double _outstanding;
    [ObservableProperty] private double _totalInvoiced;
    [ObservableProperty] private int _paidOrders;
    [ObservableProperty] private int _unpaidOrders;

    // ── Status bars (percentage) ─────────────────────────────────
    [ObservableProperty] private double _activePercent;
    [ObservableProperty] private double _archivedPercent;

    // ── Top employees ────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<EmployeeStatViewModel> _topEmployees = [];

    // ── Recent orders ────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<RequestDto> _recentOrders = [];

    // ── Selected filter for interactive status breakdown ─────────
    [ObservableProperty] private string _selectedStatus = "Всички";

    [ObservableProperty] private ObservableCollection<RequestDto> _statusFilteredOrders = [];

    public static readonly string[] StatusFilters = ["Всички", "Active", "Archived"];

    private DispatcherTimer? _debounceTimer;

    public DashboardViewModel()
    {
        ServiceLocator.Cache.RequestsChanged += ScheduleRefresh;
        ServiceLocator.Cache.ClientsChanged += ScheduleRefresh;
        ServiceLocator.Cache.ActivitiesChanged += ScheduleRefresh;
        ServiceLocator.Cache.PlotsChanged += ScheduleRefresh;
        ServiceLocator.Cache.InvoicesChanged += ScheduleRefresh;
        Refresh();
    }

    private void ScheduleRefresh()
    {
        _debounceTimer?.Stop();
        _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _debounceTimer.Tick += (_, _) =>
        {
            _debounceTimer.Stop();
            Refresh();
        };
        _debounceTimer.Start();
    }

    partial void OnSelectedStatusChanged(string value) => ApplyStatusFilter();

    private void Refresh()
    {
        var requests = ServiceLocator.Cache.GetAllRequests();
        var clients = ServiceLocator.Cache.GetAllClients();

        // Summary
        TotalOrders = requests.Count;
        ActiveOrders = requests.Count(r => eq(r.Status, "Active"));
        ArchivedOrders = requests.Count(r => eq(r.Status, "Archived"));
        TotalClients = clients.Count;

        // Count activities and plots via cache queries
        var activityCount = 0;
        var plotIds = new HashSet<int>();
        foreach (var r in requests)
        {
            var acts = ServiceLocator.Cache.GetActivitiesForRequest(r.Requestid);
            activityCount += acts.Count;
            var plots = ServiceLocator.Cache.GetPlotsForRequest(r.Requestid);
            foreach (var p in plots)
                plotIds.Add(p.Plotid);
        }
        TotalActivities = activityCount;
        TotalPlots = plotIds.Count;

        // Financial
        TotalRevenue = requests.Sum(r => r.Price);
        TotalAdvance = requests.Sum(r => r.Advance);
        Outstanding = TotalRevenue - TotalAdvance;
        PaidOrders = requests.Count(r => eq(r.Paymentstatus, "Платен"));
        UnpaidOrders = requests.Count(r => !eq(r.Paymentstatus, "Платен"));

        // Invoices
        double invoiceSum = 0;
        foreach (var r in requests)
        {
            var invoices = ServiceLocator.Cache.GetInvoicesForRequest(r.Requestid);
            invoiceSum += invoices.Sum(i => i.Sum);
        }
        TotalInvoiced = invoiceSum;

        // Status percentages
        var total = (double)Math.Max(TotalOrders, 1);
        ActivePercent = ActiveOrders / total * 100;
        ArchivedPercent = ArchivedOrders / total * 100;

        // Top employees by activity count
        var empActivity = new Dictionary<int, int>();
        foreach (var r in requests)
        {
            var acts = ServiceLocator.Cache.GetActivitiesForRequest(r.Requestid);
            foreach (var a in acts)
            {
                empActivity.TryGetValue(a.Executantid, out var count);
                empActivity[a.Executantid] = count + 1;
            }
        }
        var topEmps = empActivity
            .OrderByDescending(kv => kv.Value)
            .Take(5)
            .Select(kv => new EmployeeStatViewModel
            {
                EmployeeId = kv.Key,
                Name = ServiceLocator.ResolveEmployeeName(kv.Key),
                ActivityCount = kv.Value
            });
        TopEmployees = new ObservableCollection<EmployeeStatViewModel>(topEmps);

        // Recent orders (last 10)
        RecentOrders = new ObservableCollection<RequestDto>(
            requests.OrderByDescending(r => r.Requestid).Take(10));

        ApplyStatusFilter();
    }

    private void ApplyStatusFilter()
    {
        var requests = ServiceLocator.Cache.GetAllRequests();
        var filtered = SelectedStatus == "Всички"
            ? requests
            : requests.Where(r => eq(r.Status, SelectedStatus)).ToList();
        StatusFilteredOrders = new ObservableCollection<RequestDto>(
            filtered.OrderByDescending(r => r.Requestid).Take(20));
    }

    [RelayCommand]
    private void SetStatusFilter(string status)
    {
        SelectedStatus = status;
    }

    private static bool eq(string? a, string b) =>
        string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}

public class EmployeeStatViewModel
{
    public int EmployeeId { get; set; }
    public string Name { get; set; } = "";
    public int ActivityCount { get; set; }
}
