using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class EditTaskViewModel : ViewModelBase
{
    [ObservableProperty] private int? _taskId;
    [ObservableProperty] private int _activityId;

    // Task type — AutoCompleteBox with create-on-new
    [ObservableProperty] private ObservableCollection<TasktypeDto> _availableTaskTypes = [];
    [ObservableProperty] private TasktypeDto? _selectedTaskType;
    [ObservableProperty] private string _taskTypeSearchText = "";

    // Status — ComboBox
    [ObservableProperty] private string _status = "Зададена";

    // Executant — ComboBox
    [ObservableProperty] private ObservableCollection<EmployeeDto> _availableEmployees = [];
    [ObservableProperty] private EmployeeDto? _selectedExecutant;

    // Control — ComboBox with "Няма контрол" option
    [ObservableProperty] private ObservableCollection<EmployeeDto?> _availableControllers = [];
    [ObservableProperty] private EmployeeDto? _selectedController;

    [ObservableProperty] private double _executantpayment;
    [ObservableProperty] private double _tax;
    [ObservableProperty] private string _commenttax = "";
    [ObservableProperty] private string? _comments;
    [ObservableProperty] private TimeSpan _duration = TimeSpan.FromHours(1);
    [ObservableProperty] private DateTime? _startdate = DateTime.Today;
    [ObservableProperty] private DateTime? _finishdate = DateTime.Today.AddDays(7);
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _errorMessage;

    public bool IsNew => TaskId is null;
    public string FormTitle => IsNew ? "Нова задача" : $"Редакция на задача #{TaskId}";

    public event Action? SaveCompleted;
    public event Action? CancelRequested;

    public static readonly string[] StatusOptions = ["Зададена", "Завършена", "Оферта"];

    private int? _deferredTaskTypeId;
    private int? _deferredExecutantId;
    private int? _deferredControllerId;

    public void LoadPickerData()
    {
        // Task types — show all; filtered list used only for create-on-new
        AvailableTaskTypes = new ObservableCollection<TasktypeDto>(
            ServiceLocator.Cache.GetAllTaskTypes());

        var employees = ServiceLocator.Cache.GetAllEmployees();
        AvailableEmployees = new ObservableCollection<EmployeeDto>(employees);

        // Controllers: null entry represents "Няма контрол"
        var controllers = new List<EmployeeDto?> { null };
        controllers.AddRange(employees);
        AvailableControllers = new ObservableCollection<EmployeeDto?>(controllers);
    }

    private ActivityDto? GetActivityDto()
    {
        if (ActivityId <= 0) return null;
        // Look up activity directly from cache
        var allActivities = ServiceLocator.Cache.GetAllRequests()
            .SelectMany(r => ServiceLocator.Cache.GetActivitiesForRequest(r.Requestid));
        return allActivities.FirstOrDefault(a => a.Activityid == ActivityId);
    }

    public void LoadFromDto(TaskDto dto)
    {
        TaskId = dto.Taskid;
        ActivityId = dto.Activityid;
        Executantpayment = dto.Executantpayment;
        Tax = dto.Tax;
        Commenttax = dto.Commenttax;
        Comments = dto.Comments;
        Status = dto.Status;
        Startdate = dto.Startdate;
        Finishdate = dto.Finishdate;
        Duration = dto.Duration.ToTimeSpan();

        _deferredTaskTypeId = dto.Tasktypeid;
        _deferredExecutantId = dto.Executantid;
        _deferredControllerId = dto.Controlid;
    }

    public void ApplyDeferredSelections()
    {
        if (_deferredTaskTypeId.HasValue)
        {
            SelectedTaskType = AvailableTaskTypes
                .FirstOrDefault(tt => tt.Tasktypeid == _deferredTaskTypeId.Value);
            if (SelectedTaskType is not null)
                TaskTypeSearchText = SelectedTaskType.Tasktypename;
        }

        if (_deferredExecutantId.HasValue)
        {
            SelectedExecutant = AvailableEmployees
                .FirstOrDefault(e => e.Employeeid == _deferredExecutantId.Value);
        }

        if (_deferredControllerId.HasValue)
        {
            SelectedController = AvailableControllers
                .FirstOrDefault(e => e is not null && e.Employeeid == _deferredControllerId.Value);
        }
        // else SelectedController stays null = "Няма контрол"
    }

    [RelayCommand]
    private async Task Save()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            // Resolve task type
            var taskTypeId = SelectedTaskType?.Tasktypeid ?? 0;
            var typeName = TaskTypeSearchText?.Trim() ?? "";

            if (taskTypeId == 0 && !string.IsNullOrEmpty(typeName))
            {
                var existing = AvailableTaskTypes
                    .FirstOrDefault(tt => string.Equals(tt.Tasktypename, typeName, StringComparison.OrdinalIgnoreCase));
                if (existing is not null)
                {
                    taskTypeId = existing.Tasktypeid;
                }
                else
                {
                    var activityDto = GetActivityDto();
                    var activityTypeId = activityDto?.Activitytypeid ?? 0;
                    if (activityTypeId == 0)
                    {
                        ErrorMessage = "Не може да се създаде нов тип задача — липсва тип дейност.";
                        return;
                    }
                    var created = await ServiceLocator.Cache.CreateTaskTypeAsync(
                        new CreateTasktypeDto { Tasktypename = typeName, Activitytypeid = activityTypeId });
                    if (created is not null)
                        taskTypeId = created.Tasktypeid;
                }
            }

            if (taskTypeId == 0)
            {
                ErrorMessage = "Моля, изберете или въведете тип задача.";
                return;
            }

            var executantId = SelectedExecutant?.Employeeid ?? 0;
            if (executantId == 0)
            {
                ErrorMessage = "Моля, изберете изпълнител.";
                return;
            }

            var controlId = SelectedController?.Employeeid;
            var startDate = Startdate ?? DateTime.Today;
            var finishDate = Finishdate ?? DateTime.Today.AddDays(7);
            var duration = TimeOnly.FromTimeSpan(Duration);

            if (IsNew)
            {
                await ServiceLocator.Cache.CreateTaskAsync(new CreateTaskDto
                {
                    Activityid = ActivityId,
                    Executantid = executantId,
                    Controlid = controlId,
                    Tasktypeid = taskTypeId,
                    Executantpayment = Executantpayment,
                    Tax = Tax,
                    Commenttax = Commenttax,
                    Comments = Comments,
                    Status = Status,
                    Startdate = startDate,
                    Finishdate = finishDate,
                    Duration = duration
                });
            }
            else
            {
                await ServiceLocator.Cache.UpdateTaskAsync(TaskId!.Value, new TaskDto
                {
                    Taskid = TaskId.Value,
                    Activityid = ActivityId,
                    Executantid = executantId,
                    Controlid = controlId,
                    Tasktypeid = taskTypeId,
                    Executantpayment = Executantpayment,
                    Tax = Tax,
                    Commenttax = Commenttax,
                    Comments = Comments,
                    Status = Status,
                    Startdate = startDate,
                    Finishdate = finishDate,
                    Duration = duration
                });
            }
            SaveCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel() => CancelRequested?.Invoke();
}
