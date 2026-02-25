using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class EditPlotViewModel : ViewModelBase
{
    [ObservableProperty] private int? _plotId;
    [ObservableProperty] private int _requestId;
    [ObservableProperty] private string _plotnumber = "";
    [ObservableProperty] private string? _regulatedplotnumber;
    [ObservableProperty] private string? _neighborhood;
    [ObservableProperty] private string? _city;
    [ObservableProperty] private string? _municipality;
    [ObservableProperty] private string? _street;
    [ObservableProperty] private int? _streetnumber;
    [ObservableProperty] private string _designation = "";
    [ObservableProperty] private string? _locality;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _errorMessage;

    // Order-mode pickers
    [ObservableProperty] private ObservableCollection<ActivityDto> _availableActivities = [];
    [ObservableProperty] private ActivityDto? _selectedActivity;
    [ObservableProperty] private ObservableCollection<PlotDto> _availableExistingPlots = [];
    [ObservableProperty] private string _plotNumberSearchText = "";

    public bool IsFromOrder => RequestId > 0;
    public bool IsNew => PlotId is null;
    public string FormTitle => IsNew ? "Нов парцел" : $"Редакция на парцел #{PlotId}";

    public static string[] DesignationOptions { get; } =
    [
        "горска територия",
        "урбанизирана",
        "територия на транспорта",
        "замеделска",
        "територия заета от води и водни",
        "защитена",
        "нарушена",
        "урбанизирана територия - защитена",
        "замеделска територия - защитена",
        "горска територия - защитена"
    ];

    public event Action? SaveCompleted;
    public event Action? CancelRequested;

    public void LoadFromDto(PlotDto dto)
    {
        PlotId = dto.Plotid;
        Plotnumber = dto.Plotnumber;
        Regulatedplotnumber = dto.Regulatedplotnumber;
        Neighborhood = dto.Neighborhood;
        City = dto.City;
        Municipality = dto.Municipality;
        Street = dto.Street;
        Streetnumber = dto.Streetnumber;
        Designation = dto.Designation;
        Locality = dto.Locality;
    }

    public void LoadPickerData()
    {
        if (!IsFromOrder) return;

        var activities = ServiceLocator.Cache.GetActivitiesForRequest(RequestId);
        AvailableActivities = new ObservableCollection<ActivityDto>(activities);
        if (IsNew && AvailableActivities.Count > 0)
            SelectedActivity = AvailableActivities[0];

        var plots = ServiceLocator.Cache.GetAllPlots();
        AvailableExistingPlots = new ObservableCollection<PlotDto>(plots);
    }

    partial void OnPlotNumberSearchTextChanged(string value)
    {
        if (!IsFromOrder || !IsNew) return;

        var match = AvailableExistingPlots
            .FirstOrDefault(p => p.Plotnumber.Equals(value, StringComparison.OrdinalIgnoreCase));

        if (match is not null)
        {
            Regulatedplotnumber = match.Regulatedplotnumber;
            Neighborhood = match.Neighborhood;
            City = match.City;
            Municipality = match.Municipality;
            Street = match.Street;
            Streetnumber = match.Streetnumber;
            Designation = match.Designation;
            Locality = match.Locality;
        }
        else
        {
            Regulatedplotnumber = null;
            Neighborhood = null;
            City = null;
            Municipality = null;
            Street = null;
            Streetnumber = null;
            Designation = "";
            Locality = null;
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            if (IsNew)
            {
                if (IsFromOrder)
                {
                    if (SelectedActivity is null)
                    {
                        ErrorMessage = "Моля, изберете дейност.";
                        return;
                    }

                    // Check if an existing plot matches by number
                    var searchText = PlotNumberSearchText.Trim();
                    if (string.IsNullOrWhiteSpace(searchText))
                        searchText = Plotnumber.Trim();

                    var existingPlot = AvailableExistingPlots
                        .FirstOrDefault(p => p.Plotnumber.Equals(searchText, StringComparison.OrdinalIgnoreCase));

                    if (existingPlot is not null)
                    {
                        // Link existing plot — don't create a new one
                        await ServiceLocator.Cache.LinkPlotToActivityAsync(
                            SelectedActivity.Activityid, existingPlot.Plotid);
                    }
                    else
                    {
                        // Create new plot, then link
                        var created = await ServiceLocator.Cache.CreatePlotAsync(new CreatePlotDto
                        {
                            Plotnumber = string.IsNullOrWhiteSpace(searchText) ? Plotnumber : searchText,
                            Regulatedplotnumber = Regulatedplotnumber,
                            Neighborhood = Neighborhood,
                            City = City,
                            Municipality = Municipality,
                            Street = Street,
                            Streetnumber = Streetnumber,
                            Designation = Designation,
                            Locality = Locality
                        });
                        if (created is not null)
                            await ServiceLocator.Cache.LinkPlotToActivityAsync(
                                SelectedActivity.Activityid, created.Plotid);
                    }
                }
                else
                {
                    // Standalone — validate uniqueness
                    var allPlots = ServiceLocator.Cache.GetAllPlots();
                    var duplicate = allPlots
                        .Any(p => p.Plotnumber.Equals(Plotnumber.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (duplicate)
                    {
                        ErrorMessage = $"Имот с номер \"{Plotnumber.Trim()}\" вече съществува.";
                        return;
                    }

                    await ServiceLocator.Cache.CreatePlotAsync(new CreatePlotDto
                    {
                        Plotnumber = Plotnumber,
                        Regulatedplotnumber = Regulatedplotnumber,
                        Neighborhood = Neighborhood,
                        City = City,
                        Municipality = Municipality,
                        Street = Street,
                        Streetnumber = Streetnumber,
                        Designation = Designation,
                        Locality = Locality
                    });
                }
            }
            else
            {
                await ServiceLocator.Cache.UpdatePlotAsync(PlotId!.Value, new PlotDto
                {
                    Plotid = PlotId.Value,
                    Plotnumber = Plotnumber,
                    Regulatedplotnumber = Regulatedplotnumber,
                    Neighborhood = Neighborhood,
                    City = City,
                    Municipality = Municipality,
                    Street = Street,
                    Streetnumber = Streetnumber,
                    Designation = Designation,
                    Locality = Locality
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
