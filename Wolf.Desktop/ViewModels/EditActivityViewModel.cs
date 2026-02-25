using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class EditActivityViewModel : ViewModelBase
{
    [ObservableProperty] private int? _activityId;
    [ObservableProperty] private int _requestId;

    // Activity type — AutoCompleteBox with create-on-new
    [ObservableProperty] private ObservableCollection<ActivitytypeDto> _availableActivityTypes = [];
    [ObservableProperty] private ActivitytypeDto? _selectedActivityType;
    [ObservableProperty] private string _activityTypeSearchText = "";

    // Executant — AutoCompleteBox
    [ObservableProperty] private ObservableCollection<EmployeeDto> _availableEmployees = [];
    [ObservableProperty] private EmployeeDto? _selectedExecutant;

    // Parent activity — ComboBox
    [ObservableProperty] private ObservableCollection<ActivityDto> _availableParentActivities = [];
    [ObservableProperty] private ActivityDto? _selectedParentActivity;

    [ObservableProperty] private double _employeepayment;
    [ObservableProperty] private DateTime? _startdate = DateTime.Today;
    [ObservableProperty] private DateTime? _expectedduration = DateTime.Today.AddDays(7);
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _errorMessage;

    public bool IsNew => ActivityId is null;
    public string FormTitle => IsNew ? "Нова дейност" : $"Редакция на дейност #{ActivityId}";

    public event Action? SaveCompleted;
    public event Action? CancelRequested;

    public void LoadPickerData()
    {
        AvailableActivityTypes = new ObservableCollection<ActivitytypeDto>(
            ServiceLocator.Cache.GetAllActivityTypes());
        AvailableEmployees = new ObservableCollection<EmployeeDto>(
            ServiceLocator.Cache.GetAllEmployees());

        var activities = ServiceLocator.Cache.GetActivitiesForRequest(RequestId);
        // Exclude self when editing
        AvailableParentActivities = new ObservableCollection<ActivityDto>(
            activities.Where(a => a.Activityid != (ActivityId ?? 0)));
    }

    public void LoadFromDto(ActivityDto dto)
    {
        ActivityId = dto.Activityid;
        RequestId = dto.Requestid;
        Employeepayment = dto.Employeepayment;
        Startdate = dto.Startdate;
        Expectedduration = dto.Expectedduration;

        // Defer picker selection until LoadPickerData populates the lists
        _deferredActivityTypeId = dto.Activitytypeid;
        _deferredExecutantId = dto.Executantid;
        _deferredParentActivityId = dto.Parentactivityid;
    }

    private int? _deferredActivityTypeId;
    private int? _deferredExecutantId;
    private int? _deferredParentActivityId;

    /// <summary>Call after LoadPickerData to resolve deferred selections.</summary>
    public void ApplyDeferredSelections()
    {
        if (_deferredActivityTypeId.HasValue)
        {
            SelectedActivityType = AvailableActivityTypes
                .FirstOrDefault(at => at.Activitytypeid == _deferredActivityTypeId.Value);
            if (SelectedActivityType is not null)
                ActivityTypeSearchText = SelectedActivityType.Activitytypename;
        }

        if (_deferredExecutantId.HasValue)
        {
            SelectedExecutant = AvailableEmployees
                .FirstOrDefault(e => e.Employeeid == _deferredExecutantId.Value);
        }

        if (_deferredParentActivityId.HasValue)
        {
            SelectedParentActivity = AvailableParentActivities
                .FirstOrDefault(a => a.Activityid == _deferredParentActivityId.Value);
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            // Resolve activity type: if text doesn't match any existing, create new
            var activityTypeId = SelectedActivityType?.Activitytypeid ?? 0;
            var typeName = ActivityTypeSearchText?.Trim() ?? "";

            if (activityTypeId == 0 && !string.IsNullOrEmpty(typeName))
            {
                // Check if name matches an existing type (case-insensitive)
                var existing = AvailableActivityTypes
                    .FirstOrDefault(at => string.Equals(at.Activitytypename, typeName, StringComparison.OrdinalIgnoreCase));
                if (existing is not null)
                {
                    activityTypeId = existing.Activitytypeid;
                }
                else
                {
                    // Create new activity type
                    var created = await ServiceLocator.Cache.CreateActivityTypeAsync(
                        new CreateActivitytypeDto { Activitytypename = typeName });
                    if (created is not null)
                        activityTypeId = created.Activitytypeid;
                }
            }

            if (activityTypeId == 0)
            {
                ErrorMessage = "Моля, изберете или въведете тип дейност.";
                return;
            }

            var executantId = SelectedExecutant?.Employeeid ?? 0;
            if (executantId == 0)
            {
                ErrorMessage = "Моля, изберете изпълнител.";
                return;
            }

            var parentActivityId = SelectedParentActivity?.Activityid;

            if (IsNew)
            {
                var startDate = Startdate ?? DateTime.Today;
                var expectedDuration = Expectedduration ?? DateTime.Today.AddDays(7);

                await ServiceLocator.Cache.CreateActivityAsync(new CreateActivityDto
                {
                    Requestid = RequestId,
                    Activitytypeid = activityTypeId,
                    Parentactivityid = parentActivityId,
                    Executantid = executantId,
                    Employeepayment = Employeepayment,
                    Startdate = startDate,
                    Expectedduration = expectedDuration
                });
            }
            else
            {
                var startDate = Startdate ?? DateTime.Today;
                var expectedDuration = Expectedduration ?? DateTime.Today.AddDays(7);

                await ServiceLocator.Cache.UpdateActivityAsync(ActivityId!.Value, new ActivityDto
                {
                    Activityid = ActivityId.Value,
                    Requestid = RequestId,
                    Activitytypeid = activityTypeId,
                    Parentactivityid = parentActivityId,
                    Executantid = executantId,
                    Employeepayment = Employeepayment,
                    Startdate = startDate,
                    Expectedduration = expectedDuration
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
