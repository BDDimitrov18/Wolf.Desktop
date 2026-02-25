using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class UserProfileViewModel : ViewModelBase
{
    // ── Identity ─────────────────────────────────────────────────
    [ObservableProperty] private string _displayName = "";
    [ObservableProperty] private string _initials = "";
    [ObservableProperty] private string _role = "";
    [ObservableProperty] private string _email = "";
    [ObservableProperty] private string _phone = "";

    // ── Monthly stats ────────────────────────────────────────────
    [ObservableProperty] private int _tasksThisMonth;
    [ObservableProperty] private int _completedTasksThisMonth;
    [ObservableProperty] private int _pendingTasksThisMonth;
    [ObservableProperty] private int _activitiesThisMonth;

    // ── All-time stats ───────────────────────────────────────────
    [ObservableProperty] private int _totalTasks;
    [ObservableProperty] private int _totalActivities;
    [ObservableProperty] private int _totalCompletedTasks;

    // ── Salary ───────────────────────────────────────────────────
    [ObservableProperty] private double _projectedSalary;
    [ObservableProperty] private double _totalEarnedAllTime;
    [ObservableProperty] private string _currentMonth = "";

    // ── Recent completed tasks ───────────────────────────────────
    [ObservableProperty] private ObservableCollection<TaskDto> _recentTasks = [];

    public UserProfileViewModel()
    {
        ServiceLocator.Cache.TasksChanged += Refresh;
        ServiceLocator.Cache.ActivitiesChanged += Refresh;
        Refresh();
    }

    private void Refresh()
    {
        var empId = ServiceLocator.Auth.CurrentEmployeeId;
        if (empId is null) return;

        var id = empId.Value;

        // Identity
        DisplayName = ServiceLocator.Auth.CurrentDisplayName;
        Initials = ServiceLocator.Auth.CurrentInitials;
        Role = ServiceLocator.Auth.CurrentRole;

        var employee = ServiceLocator.Cache.GetEmployee(id);
        if (employee is not null)
        {
            var fullName = string.Join(" ",
                new[] { employee.Firstname, employee.Secondname, employee.Lastname }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
            if (!string.IsNullOrEmpty(fullName))
                DisplayName = fullName;
            Email = employee.Email ?? "";
            Phone = employee.Phone ?? "";
        }

        // All tasks & activities for this employee
        var allTasks = ServiceLocator.Cache.GetTasksForEmployee(id);
        var allActivities = ServiceLocator.Cache.GetActivitiesForEmployee(id);

        TotalTasks = allTasks.Count;
        TotalActivities = allActivities.Count;
        TotalCompletedTasks = allTasks.Count(t => IsDone(t.Status));

        // Current month filter
        var now = DateTime.Now;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var monthEnd = monthStart.AddMonths(1);
        CurrentMonth = now.ToString("MMMM yyyy");

        var tasksThisMonth = allTasks
            .Where(t => t.Startdate >= monthStart && t.Startdate < monthEnd)
            .ToList();

        TasksThisMonth = tasksThisMonth.Count;
        CompletedTasksThisMonth = tasksThisMonth.Count(t => IsDone(t.Status));
        PendingTasksThisMonth = tasksThisMonth.Count(t => IsPending(t.Status));

        var activitiesThisMonth = allActivities
            .Where(a => a.Startdate >= monthStart && a.Startdate < monthEnd)
            .ToList();
        ActivitiesThisMonth = activitiesThisMonth.Count;

        // Projected salary = sum of Executantpayment for done tasks this month
        ProjectedSalary = tasksThisMonth
            .Where(t => IsDone(t.Status))
            .Sum(t => t.Executantpayment);

        // Total earned all time
        TotalEarnedAllTime = allTasks
            .Where(t => IsDone(t.Status))
            .Sum(t => t.Executantpayment);

        // Recent tasks (last 10 completed)
        RecentTasks = new ObservableCollection<TaskDto>(
            allTasks
                .Where(t => IsDone(t.Status))
                .OrderByDescending(t => t.Finishdate)
                .Take(10));
    }

    private static bool IsDone(string? status) =>
        string.Equals(status, "Done", StringComparison.OrdinalIgnoreCase);

    private static bool IsPending(string? status) =>
        string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase);
}
