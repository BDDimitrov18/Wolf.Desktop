using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class EmployeeStatsViewModel : ViewModelBase
{
    // Employee info
    [ObservableProperty] private int _employeeId;
    [ObservableProperty] private string _employeeName = "";
    [ObservableProperty] private string? _phone;
    [ObservableProperty] private string? _email;
    [ObservableProperty] private bool _isOutsider;

    // Summary stats
    [ObservableProperty] private int _totalActivities;
    [ObservableProperty] private int _totalTasks;
    [ObservableProperty] private double _totalPayment;
    [ObservableProperty] private int _completedTasks;
    [ObservableProperty] private int _pendingTasks;
    [ObservableProperty] private int _uniqueOrders;

    // Activity list
    [ObservableProperty] private ObservableCollection<EmployeeActivityRow> _activities = [];

    // Task list
    [ObservableProperty] private ObservableCollection<EmployeeTaskRow> _tasks = [];

    public void Load(EmployeeDto dto)
    {
        EmployeeId = dto.Employeeid;
        EmployeeName = string.Join(" ",
            new[] { dto.Firstname, dto.Secondname, dto.Lastname }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
        Phone = dto.Phone;
        Email = dto.Email;
        IsOutsider = dto.Outsider == true;

        Recalculate();

        ServiceLocator.Cache.ActivitiesChanged += Recalculate;
        ServiceLocator.Cache.TasksChanged += Recalculate;
    }

    private void Recalculate()
    {
        var activities = ServiceLocator.Cache.GetActivitiesForEmployee(EmployeeId);
        var tasks = ServiceLocator.Cache.GetTasksForEmployee(EmployeeId);

        TotalActivities = activities.Count;
        TotalTasks = tasks.Count;
        TotalPayment = activities.Sum(a => a.Employeepayment)
                       + tasks.Sum(t => t.Executantpayment);
        CompletedTasks = tasks.Count(t =>
            string.Equals(t.Status, "Done", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(t.Status, "Completed", StringComparison.OrdinalIgnoreCase));
        PendingTasks = TotalTasks - CompletedTasks;
        UniqueOrders = activities.Select(a => a.Requestid).Distinct().Count();

        Activities = new ObservableCollection<EmployeeActivityRow>(
            activities.Select(a => new EmployeeActivityRow
            {
                ActivityId = a.Activityid,
                RequestId = a.Requestid,
                OrderName = ServiceLocator.Cache.GetRequest(a.Requestid)?.Requestname ?? $"Поръчка #{a.Requestid}",
                StartDate = a.Startdate,
                ExpectedDuration = a.Expectedduration,
                Payment = a.Employeepayment
            }));

        Tasks = new ObservableCollection<EmployeeTaskRow>(
            tasks.Select(t => new EmployeeTaskRow
            {
                TaskId = t.Taskid,
                ActivityId = t.Activityid,
                StartDate = t.Startdate,
                FinishDate = t.Finishdate,
                Status = t.Status,
                Payment = t.Executantpayment,
                Tax = t.Tax,
                Comments = t.Comments
            }));
    }
}

public class EmployeeActivityRow
{
    public int ActivityId { get; set; }
    public int RequestId { get; set; }
    public string OrderName { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime ExpectedDuration { get; set; }
    public double Payment { get; set; }
}

public class EmployeeTaskRow
{
    public int TaskId { get; set; }
    public int ActivityId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime FinishDate { get; set; }
    public string Status { get; set; } = "";
    public double Payment { get; set; }
    public double Tax { get; set; }
    public string? Comments { get; set; }
}
