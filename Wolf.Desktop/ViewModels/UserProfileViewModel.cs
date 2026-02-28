using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    // ── Period ─────────────────────────────────────────────────
    [ObservableProperty] private string _periodLabel = "";
    [ObservableProperty] private string _selectedPeriodKey = "this-month";
    private DateTime _periodStart;
    private DateTime _periodEnd;

    // ── Period stats ──────────────────────────────────────────────
    [ObservableProperty] private int _tasksPeriod;
    [ObservableProperty] private int _completedTasksPeriod;
    [ObservableProperty] private int _pendingTasksPeriod;
    [ObservableProperty] private int _activitiesPeriod;

    // ── All-time stats ───────────────────────────────────────────
    [ObservableProperty] private int _totalTasks;
    [ObservableProperty] private int _totalActivities;
    [ObservableProperty] private int _totalCompletedTasks;

    // ── Charts ───────────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<ChartBarViewModel> _taskChart = [];
    [ObservableProperty] private ObservableCollection<ChartBarViewModel> _activityChart = [];
    [ObservableProperty] private double _completionPercent;

    // ── Recent completed tasks ───────────────────────────────────
    [ObservableProperty] private ObservableCollection<TaskDto> _recentTasks = [];

    public UserProfileViewModel()
    {
        ServiceLocator.Cache.TasksChanged += Refresh;
        ServiceLocator.Cache.ActivitiesChanged += Refresh;
        SetPeriod("this-month");
    }

    [RelayCommand]
    private void SetPeriod(string key)
    {
        SelectedPeriodKey = key;
        RecalcPeriodBounds();
        Refresh();
    }

    private void RecalcPeriodBounds()
    {
        var now = DateTime.Now;
        var today = now.Date;

        switch (SelectedPeriodKey)
        {
            case "this-month":
                _periodStart = new DateTime(now.Year, now.Month, 1);
                _periodEnd = today.AddDays(1); // always up to today
                PeriodLabel = now.ToString("MMMM yyyy");
                break;
            case "last-month":
                _periodStart = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
                _periodEnd = new DateTime(now.Year, now.Month, 1);
                PeriodLabel = _periodStart.ToString("MMMM yyyy");
                break;
            case "last-3-months":
                _periodStart = new DateTime(now.Year, now.Month, 1).AddMonths(-2);
                _periodEnd = today.AddDays(1);
                PeriodLabel = $"{_periodStart:MMM yyyy} – {now:MMM yyyy}";
                break;
            case "all-time":
                _periodStart = DateTime.MinValue;
                _periodEnd = DateTime.MaxValue;
                PeriodLabel = "Цялото време";
                break;
        }
    }

    private void Refresh()
    {
        // Recalculate period boundaries so "this-month" always ends at today
        RecalcPeriodBounds();

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

        // Period filter
        var periodTasks = allTasks
            .Where(t => t.Startdate >= _periodStart && t.Startdate < _periodEnd)
            .ToList();

        var periodActivities = allActivities
            .Where(a => a.Startdate >= _periodStart && a.Startdate < _periodEnd)
            .ToList();

        TasksPeriod = periodTasks.Count;
        CompletedTasksPeriod = periodTasks.Count(t => IsDone(t.Status));
        PendingTasksPeriod = periodTasks.Count(t => IsPending(t.Status));
        ActivitiesPeriod = periodActivities.Count;

        // Completion percent
        CompletionPercent = TasksPeriod > 0
            ? Math.Round((double)CompletedTasksPeriod / TasksPeriod * 100, 1)
            : 0;

        // Build charts
        BuildCharts(periodTasks, periodActivities);

        // Recent tasks (last 10 completed in period)
        RecentTasks = new ObservableCollection<TaskDto>(
            periodTasks
                .Where(t => IsDone(t.Status))
                .OrderByDescending(t => t.Finishdate)
                .Take(10));
    }

    private void BuildCharts(List<TaskDto> tasks, List<ActivityDto> activities)
    {
        // Determine grouping: daily for <=31 days, weekly for longer
        var totalDays = (_periodEnd - _periodStart).TotalDays;
        var useWeeks = totalDays > 45 || SelectedPeriodKey == "all-time";

        if (SelectedPeriodKey == "all-time")
        {
            // Group by month
            BuildMonthlyCharts(tasks, activities);
            return;
        }

        if (useWeeks)
        {
            BuildWeeklyCharts(tasks, activities);
            return;
        }

        // Daily grouping
        var dayCount = (int)Math.Ceiling(totalDays);
        var tasksByDay = tasks.GroupBy(t => (t.Startdate.Date - _periodStart.Date).Days)
            .ToDictionary(g => g.Key, g => g.Count());
        var actsByDay = activities.GroupBy(a => (a.Startdate.Date - _periodStart.Date).Days)
            .ToDictionary(g => g.Key, g => g.Count());

        var maxTask = tasksByDay.Values.DefaultIfEmpty(0).Max();
        var maxAct = actsByDay.Values.DefaultIfEmpty(0).Max();

        var taskBars = new List<ChartBarViewModel>();
        var actBars = new List<ChartBarViewModel>();

        for (var i = 0; i < dayCount; i++)
        {
            var date = _periodStart.AddDays(i);
            tasksByDay.TryGetValue(i, out var tc);
            actsByDay.TryGetValue(i, out var ac);

            taskBars.Add(new ChartBarViewModel
            {
                Label = date.ToString("dd"),
                Value = tc,
                HeightPercent = maxTask > 0 ? (double)tc / maxTask * 100 : 0,
                Tooltip = $"{date:dd.MM} — {tc} задачи"
            });

            actBars.Add(new ChartBarViewModel
            {
                Label = date.ToString("dd"),
                Value = ac,
                HeightPercent = maxAct > 0 ? (double)ac / maxAct * 100 : 0,
                Tooltip = $"{date:dd.MM} — {ac} дейности"
            });
        }

        TaskChart = new ObservableCollection<ChartBarViewModel>(taskBars);
        ActivityChart = new ObservableCollection<ChartBarViewModel>(actBars);
    }

    private void BuildWeeklyCharts(List<TaskDto> tasks, List<ActivityDto> activities)
    {
        var weeks = new List<(DateTime Start, DateTime End)>();
        var cur = _periodStart;
        while (cur < _periodEnd)
        {
            var end = cur.AddDays(7) > _periodEnd ? _periodEnd : cur.AddDays(7);
            weeks.Add((cur, end));
            cur = end;
        }

        var maxTask = 0;
        var maxAct = 0;
        var taskCounts = new int[weeks.Count];
        var actCounts = new int[weeks.Count];

        for (var i = 0; i < weeks.Count; i++)
        {
            taskCounts[i] = tasks.Count(t => t.Startdate >= weeks[i].Start && t.Startdate < weeks[i].End);
            actCounts[i] = activities.Count(a => a.Startdate >= weeks[i].Start && a.Startdate < weeks[i].End);
            if (taskCounts[i] > maxTask) maxTask = taskCounts[i];
            if (actCounts[i] > maxAct) maxAct = actCounts[i];
        }

        var taskBars = new List<ChartBarViewModel>();
        var actBars = new List<ChartBarViewModel>();

        for (var i = 0; i < weeks.Count; i++)
        {
            var label = $"{weeks[i].Start:dd.MM}";
            taskBars.Add(new ChartBarViewModel
            {
                Label = label, Value = taskCounts[i],
                HeightPercent = maxTask > 0 ? (double)taskCounts[i] / maxTask * 100 : 0,
                Tooltip = $"{weeks[i].Start:dd.MM}–{weeks[i].End.AddDays(-1):dd.MM} — {taskCounts[i]} задачи"
            });
            actBars.Add(new ChartBarViewModel
            {
                Label = label, Value = actCounts[i],
                HeightPercent = maxAct > 0 ? (double)actCounts[i] / maxAct * 100 : 0,
                Tooltip = $"{weeks[i].Start:dd.MM}–{weeks[i].End.AddDays(-1):dd.MM} — {actCounts[i]} дейности"
            });
        }

        TaskChart = new ObservableCollection<ChartBarViewModel>(taskBars);
        ActivityChart = new ObservableCollection<ChartBarViewModel>(actBars);
    }

    private void BuildMonthlyCharts(List<TaskDto> tasks, List<ActivityDto> activities)
    {
        if (tasks.Count == 0 && activities.Count == 0)
        {
            TaskChart = [];
            ActivityChart = [];
            return;
        }

        var allDates = tasks.Select(t => t.Startdate)
            .Concat(activities.Select(a => a.Startdate))
            .ToList();
        var minDate = allDates.Min();
        var maxDate = allDates.Max();

        var months = new List<(int Year, int Month)>();
        var cur = new DateTime(minDate.Year, minDate.Month, 1);
        var end = new DateTime(maxDate.Year, maxDate.Month, 1).AddMonths(1);
        while (cur < end)
        {
            months.Add((cur.Year, cur.Month));
            cur = cur.AddMonths(1);
        }

        // Limit to last 12 months for readability
        if (months.Count > 12)
            months = months.Skip(months.Count - 12).ToList();

        var tasksByMonth = tasks.GroupBy(t => (t.Startdate.Year, t.Startdate.Month))
            .ToDictionary(g => g.Key, g => g.Count());
        var actsByMonth = activities.GroupBy(a => (a.Startdate.Year, a.Startdate.Month))
            .ToDictionary(g => g.Key, g => g.Count());

        var maxTask = tasksByMonth.Values.DefaultIfEmpty(0).Max();
        var maxAct = actsByMonth.Values.DefaultIfEmpty(0).Max();

        var taskBars = new List<ChartBarViewModel>();
        var actBars = new List<ChartBarViewModel>();

        foreach (var (year, month) in months)
        {
            tasksByMonth.TryGetValue((year, month), out var tc);
            actsByMonth.TryGetValue((year, month), out var ac);
            var label = new DateTime(year, month, 1).ToString("MMM");

            taskBars.Add(new ChartBarViewModel
            {
                Label = label, Value = tc,
                HeightPercent = maxTask > 0 ? (double)tc / maxTask * 100 : 0,
                Tooltip = $"{new DateTime(year, month, 1):MMM yyyy} — {tc} задачи"
            });
            actBars.Add(new ChartBarViewModel
            {
                Label = label, Value = ac,
                HeightPercent = maxAct > 0 ? (double)ac / maxAct * 100 : 0,
                Tooltip = $"{new DateTime(year, month, 1):MMM yyyy} — {ac} дейности"
            });
        }

        TaskChart = new ObservableCollection<ChartBarViewModel>(taskBars);
        ActivityChart = new ObservableCollection<ChartBarViewModel>(actBars);
    }

    private static bool IsDone(string? status) =>
        string.Equals(status, "Done", StringComparison.OrdinalIgnoreCase);

    private static bool IsPending(string? status) =>
        string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase);
}

public class ChartBarViewModel
{
    public string Label { get; set; } = "";
    public int Value { get; set; }
    public double HeightPercent { get; set; }
    public string Tooltip { get; set; } = "";
}
