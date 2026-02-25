using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class AllPlotsViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<PlotDto> _plots = [];
    [ObservableProperty] private ObservableCollection<PlotDto> _filteredPlots = [];
    [ObservableProperty] private PlotDto? _selectedPlot;
    [ObservableProperty] private string _searchText = "";

    public event Action<PlotDto?>? OpenEditPlotRequested;

    public AllPlotsViewModel()
    {
        ServiceLocator.Cache.PlotsChanged += OnPlotsChanged;
        Load();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void Load()
    {
        var list = ServiceLocator.Cache.GetAllPlots();
        Plots = new ObservableCollection<PlotDto>(list);
        ApplyFilter();
    }

    private void OnPlotsChanged()
    {
        var selectedId = SelectedPlot?.Plotid;
        Load();
        if (selectedId.HasValue)
            SelectedPlot = Plots.FirstOrDefault(p => p.Plotid == selectedId.Value);
    }

    private void ApplyFilter()
    {
        var q = SearchText?.Trim().ToLower() ?? "";
        var filtered = Plots.Where(p =>
        {
            if (string.IsNullOrEmpty(q)) return true;
            return p.Plotnumber.ToLower().Contains(q) ||
                   (p.City?.ToLower().Contains(q) == true) ||
                   (p.Municipality?.ToLower().Contains(q) == true) ||
                   (p.Neighborhood?.ToLower().Contains(q) == true) ||
                   (p.Street?.ToLower().Contains(q) == true) ||
                   p.Designation.ToLower().Contains(q);
        });
        FilteredPlots = new ObservableCollection<PlotDto>(filtered);
    }

    [RelayCommand]
    private void NewPlot() => OpenEditPlotRequested?.Invoke(null);

    [RelayCommand]
    private void EditPlot(PlotDto? plot)
    {
        if (plot is not null)
            OpenEditPlotRequested?.Invoke(plot);
    }

    [RelayCommand]
    private async Task DeletePlot(PlotDto? plot)
    {
        if (plot is null) return;
        try
        {
            await ServiceLocator.Cache.DeletePlotAsync(plot.Plotid);
            if (SelectedPlot?.Plotid == plot.Plotid)
                SelectedPlot = null;
        }
        catch (Exception ex) { ServiceLocator.ShowError($"Грешка при изтриване на парцел: {ex.Message}"); }
    }

    [RelayCommand]
    private void DeselectPlot() => SelectedPlot = null;
}
