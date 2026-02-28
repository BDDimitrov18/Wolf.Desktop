using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Inqueries;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class InqueriesViewModel : ViewModelBase
{
    // Date range
    [ObservableProperty] private DateTimeOffset _dateFrom = new(DateTime.Today.AddMonths(-1));
    [ObservableProperty] private DateTimeOffset _dateTo = new(DateTime.Today);

    // Admin-only: employee filter
    public bool IsAdmin { get; }
    [ObservableProperty] private bool _allEmployeesChecked = true;
    [ObservableProperty] private ObservableCollection<CheckableItemViewModel> _employees = [];

    // Payment status filter
    [ObservableProperty] private bool _allPaymentStatusChecked = true;
    [ObservableProperty] private ObservableCollection<CheckableItemViewModel> _paymentStatuses = [];

    // Activity type filter
    [ObservableProperty] private bool _allActivityTypesChecked = true;
    [ObservableProperty] private string _activityTypeSearch = "";
    [ObservableProperty] private ObservableCollection<CheckableItemViewModel> _activityTypes = [];
    [ObservableProperty] private ObservableCollection<CheckableItemViewModel> _filteredActivityTypes = [];

    // Task type filter
    [ObservableProperty] private bool _allTaskTypesChecked = true;
    [ObservableProperty] private string _taskTypeSearch = "";
    [ObservableProperty] private ObservableCollection<CheckableItemViewModel> _taskTypes = [];
    [ObservableProperty] private ObservableCollection<CheckableItemViewModel> _filteredTaskTypes = [];

    // Task status filter
    [ObservableProperty] private bool _allTaskStatusChecked = true;
    [ObservableProperty] private ObservableCollection<CheckableItemViewModel> _taskStatuses = [];

    public InqueriesViewModel()
    {
        IsAdmin = string.Equals(ServiceLocator.Auth.CurrentRole, "Admin", StringComparison.OrdinalIgnoreCase);
        LoadFilterData();
    }

    private void LoadFilterData()
    {
        var cache = ServiceLocator.Cache;

        // Employees (admin only)
        if (IsAdmin)
        {
            var emps = cache.GetAllEmployees();
            Employees = new ObservableCollection<CheckableItemViewModel>(
                emps.Select(e => new CheckableItemViewModel
                {
                    Id = e.Employeeid,
                    Label = string.Join(" ", new[] { e.Firstname, e.Secondname, e.Lastname }
                        .Where(s => !string.IsNullOrEmpty(s)))
                }));
        }

        // Payment statuses
        PaymentStatuses = new ObservableCollection<CheckableItemViewModel>(
            new[]
            {
                new CheckableItemViewModel { Id = 1, Label = "Платен" },
                new CheckableItemViewModel { Id = 2, Label = "Аванс" },
                new CheckableItemViewModel { Id = 3, Label = "Не платен" }
            });

        // Activity types
        var actTypes = cache.GetAllActivityTypes();
        ActivityTypes = new ObservableCollection<CheckableItemViewModel>(
            actTypes.Select(a => new CheckableItemViewModel
            {
                Id = a.Activitytypeid,
                Label = a.Activitytypename
            }));
        FilteredActivityTypes = new ObservableCollection<CheckableItemViewModel>(ActivityTypes);

        // Task types
        var taskTypes = cache.GetAllTaskTypes();
        TaskTypes = new ObservableCollection<CheckableItemViewModel>(
            taskTypes.Select(t => new CheckableItemViewModel
            {
                Id = t.Tasktypeid,
                Label = t.Tasktypename
            }));
        FilteredTaskTypes = new ObservableCollection<CheckableItemViewModel>(TaskTypes);

        // Task statuses
        TaskStatuses = new ObservableCollection<CheckableItemViewModel>(
            new[]
            {
                new CheckableItemViewModel { Id = 1, Label = "Зададена" },
                new CheckableItemViewModel { Id = 2, Label = "Завършена" },
                new CheckableItemViewModel { Id = 3, Label = "Оферта" }
            });
    }

    // "Select All" toggle handlers
    partial void OnAllEmployeesCheckedChanged(bool value)
    {
        foreach (var e in Employees) e.IsChecked = value;
    }

    partial void OnAllPaymentStatusCheckedChanged(bool value)
    {
        foreach (var s in PaymentStatuses) s.IsChecked = value;
    }

    partial void OnAllActivityTypesCheckedChanged(bool value)
    {
        foreach (var a in ActivityTypes) a.IsChecked = value;
    }

    partial void OnAllTaskTypesCheckedChanged(bool value)
    {
        foreach (var t in TaskTypes) t.IsChecked = value;
    }

    partial void OnAllTaskStatusCheckedChanged(bool value)
    {
        foreach (var s in TaskStatuses) s.IsChecked = value;
    }

    // Search text changed → filter visible items
    partial void OnActivityTypeSearchChanged(string value)
    {
        var term = value?.Trim() ?? "";
        FilteredActivityTypes = new ObservableCollection<CheckableItemViewModel>(
            string.IsNullOrEmpty(term)
                ? ActivityTypes
                : ActivityTypes.Where(a => a.Label.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }

    partial void OnTaskTypeSearchChanged(string value)
    {
        var term = value?.Trim() ?? "";
        FilteredTaskTypes = new ObservableCollection<CheckableItemViewModel>(
            string.IsNullOrEmpty(term)
                ? TaskTypes
                : TaskTypes.Where(t => t.Label.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }

    // ── Filtering logic ─────────────────────────────────────────────

    private (List<RequestDto> requests, List<EmployeeDto> employees) GetFilteredData()
    {
        var cache = ServiceLocator.Cache;
        var fromDate = DateFrom.DateTime;
        var toDate = DateTo.DateTime;

        var selectedPayments = PaymentStatuses.Where(s => s.IsChecked).Select(s => s.Label).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var selectedActTypes = ActivityTypes.Where(a => a.IsChecked).Select(a => a.Id).ToHashSet();
        var selectedTaskTypes = TaskTypes.Where(t => t.IsChecked).Select(t => t.Id).ToHashSet();
        var selectedTaskStatuses = TaskStatuses.Where(s => s.IsChecked).Select(s => s.Label).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Employees
        List<EmployeeDto> employees;
        HashSet<int> selectedEmpIds;
        if (IsAdmin)
        {
            var selectedEmps = Employees.Where(e => e.IsChecked).Select(e => e.Id).ToHashSet();
            employees = cache.GetAllEmployees().Where(e => selectedEmps.Contains(e.Employeeid)).ToList();
            selectedEmpIds = selectedEmps;
        }
        else
        {
            var myId = ServiceLocator.Auth.CurrentEmployeeId;
            employees = myId.HasValue
                ? cache.GetAllEmployees().Where(e => e.Employeeid == myId.Value).ToList()
                : [];
            selectedEmpIds = employees.Select(e => e.Employeeid).ToHashSet();
        }

        // Filter requests
        var allRequests = cache.GetAllRequests();
        var filteredRequests = allRequests.Where(r =>
        {
            // Payment status filter
            if (!selectedPayments.Contains(r.Paymentstatus)) return false;

            // Check if request has qualifying activities/tasks within date range
            var activities = cache.GetActivitiesForRequest(r.Requestid);
            return activities.Any(act =>
            {
                if (!selectedActTypes.Contains(act.Activitytypeid)) return false;

                var tasks = cache.GetTasksForActivity(act.Activityid);
                return tasks.Any(t =>
                    selectedTaskTypes.Contains(t.Tasktypeid) &&
                    selectedTaskStatuses.Contains(t.Status) &&
                    selectedEmpIds.Contains(t.Executantid) &&
                    t.Startdate >= fromDate && t.Startdate <= toDate);
            });
        }).ToList();

        return (filteredRequests, employees);
    }

    private string GetExportPath(string prefix)
    {
        var dir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return System.IO.Path.Combine(dir, $"{prefix}_{timestamp}.xlsx");
    }

    // ── 4 Inquiry Commands ─────────────────────────────────────────

    [RelayCommand]
    private void RunAllTasksInquiry()
    {
        try
        {
            var (requests, employees) = GetFilteredData();
            if (requests.Count == 0)
            {
                ServiceLocator.ShowError("Няма данни за избраните филтри.");
                return;
            }
            var inquiry = new AllTasksInquiry(requests, employees);
            inquiry.ExportToExcel(GetExportPath("Справка_оборот_задачи"));
        }
        catch (Exception ex) { ServiceLocator.ShowError($"Грешка: {ex.Message}"); }
    }

    [RelayCommand]
    private void RunObligationsInquiry()
    {
        try
        {
            var (requests, _) = GetFilteredData();
            if (requests.Count == 0)
            {
                ServiceLocator.ShowError("Няма данни за избраните филтри.");
                return;
            }
            var inquiry = new ObligationsInquiry(requests);
            inquiry.ExportToExcel(GetExportPath("Справка_задължения"));
        }
        catch (Exception ex) { ServiceLocator.ShowError($"Грешка: {ex.Message}"); }
    }

    [RelayCommand]
    private void RunTaskTypePaymentInquiry()
    {
        try
        {
            var (requests, employees) = GetFilteredData();
            if (requests.Count == 0)
            {
                ServiceLocator.ShowError("Няма данни за избраните филтри.");
                return;
            }
            var inquiry = new TaskTypePaymentInquiry(requests, employees);
            inquiry.ExportToExcel(GetExportPath("Справка_плащания_по_вид_задача"));
        }
        catch (Exception ex) { ServiceLocator.ShowError($"Грешка: {ex.Message}"); }
    }

    [RelayCommand]
    private void RunMonthlyInquiry()
    {
        try
        {
            var (requests, employees) = GetFilteredData();
            if (requests.Count == 0)
            {
                ServiceLocator.ShowError("Няма данни за избраните филтри.");
                return;
            }
            var inquiry = new EmployeeTaskTypePaymentInquiry(requests, employees);
            inquiry.ExportToExcel(GetExportPath("Месечна_справка"));
        }
        catch (Exception ex) { ServiceLocator.ShowError($"Грешка: {ex.Message}"); }
    }
}
