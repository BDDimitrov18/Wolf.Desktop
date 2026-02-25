using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class ActivitiesDetailViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<ActivityDto> _activities = [];
    [ObservableProperty] private ActivityDto? _selectedActivity;
    [ObservableProperty] private ObservableCollection<TaskDto> _tasks = [];
    [ObservableProperty] private TaskDto? _selectedTask;

    private int _currentRequestId;

    // Events for MainWindowViewModel to open edit tabs
    public event Action<ActivityDto?, int>? OpenEditActivityRequested; // (dto or null, requestId)
    public event Action<TaskDto?, int>? OpenEditTaskRequested;         // (dto or null, activityId)

    public ActivitiesDetailViewModel()
    {
        ServiceLocator.Cache.ActivitiesChanged += OnActivitiesChanged;
        ServiceLocator.Cache.TasksChanged += OnTasksChanged;
    }

    partial void OnSelectedActivityChanged(ActivityDto? value)
    {
        SelectedTask = null;
        if (value != null)
            LoadTasks(value.Activityid);
        else
            Tasks.Clear();
    }

    public void LoadForOrder(int requestId)
    {
        _currentRequestId = requestId;
        var list = ServiceLocator.Cache.GetActivitiesForRequest(requestId);
        Activities = new ObservableCollection<ActivityDto>(list);
        Tasks.Clear();
        SelectedActivity = null;
        SelectedTask = null;
    }

    public void Clear()
    {
        _currentRequestId = 0;
        Activities.Clear();
        Tasks.Clear();
        SelectedActivity = null;
        SelectedTask = null;
    }

    private void LoadTasks(int activityId)
    {
        var list = ServiceLocator.Cache.GetTasksForActivity(activityId);
        Tasks = new ObservableCollection<TaskDto>(list);
        SelectedTask = null;
    }

    private void OnActivitiesChanged()
    {
        if (_currentRequestId > 0)
        {
            var selectedId = SelectedActivity?.Activityid;
            LoadForOrder(_currentRequestId);
            if (selectedId.HasValue)
                SelectedActivity = Activities.FirstOrDefault(a => a.Activityid == selectedId.Value);
        }
    }

    private void OnTasksChanged()
    {
        if (SelectedActivity is not null)
            LoadTasks(SelectedActivity.Activityid);
    }

    [RelayCommand]
    private void DeselectActivity() => SelectedActivity = null;

    [RelayCommand]
    private void DeselectTask() => SelectedTask = null;

    [RelayCommand]
    private void AddActivity() => OpenEditActivityRequested?.Invoke(null, _currentRequestId);

    [RelayCommand]
    private void EditActivity()
    {
        if (SelectedActivity is not null)
            OpenEditActivityRequested?.Invoke(SelectedActivity, _currentRequestId);
    }

    [RelayCommand]
    private async Task DeleteActivity()
    {
        if (SelectedActivity is null) return;
        try
        {
            await ServiceLocator.Cache.DeleteActivityAsync(SelectedActivity.Activityid);
        }
        catch (Exception ex) { ServiceLocator.ShowError($"Грешка: {ex.Message}"); }
    }

    [RelayCommand]
    private void AddTask()
    {
        if (SelectedActivity is not null)
            OpenEditTaskRequested?.Invoke(null, SelectedActivity.Activityid);
    }

    [RelayCommand]
    private void EditTask(TaskDto? task)
    {
        if (task is not null && SelectedActivity is not null)
            OpenEditTaskRequested?.Invoke(task, SelectedActivity.Activityid);
    }

    [RelayCommand]
    private async Task DeleteTask(TaskDto? task)
    {
        if (task is null) return;
        try
        {
            await ServiceLocator.Cache.DeleteTaskAsync(task.Taskid);
        }
        catch (Exception ex) { ServiceLocator.ShowError($"Грешка: {ex.Message}"); }
    }
}
