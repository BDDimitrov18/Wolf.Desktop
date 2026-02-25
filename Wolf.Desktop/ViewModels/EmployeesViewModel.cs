using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class EmployeesViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<EmployeeDto> _employees = [];
    [ObservableProperty] private ObservableCollection<EmployeeDto> _filteredEmployees = [];
    [ObservableProperty] private EmployeeDto? _selectedEmployee;
    [ObservableProperty] private string _searchText = "";

    public bool IsAdmin => string.Equals(
        ServiceLocator.Auth.CurrentRole, "Admin", StringComparison.OrdinalIgnoreCase);

    /// <summary>Raised when admin clicks to open employee statistics. Passes the EmployeeDto.</summary>
    public event Action<EmployeeDto>? OpenEmployeeStatsRequested;

    public EmployeesViewModel()
    {
        Load();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void Load()
    {
        var list = ServiceLocator.Cache.GetAllEmployees();
        Employees = new ObservableCollection<EmployeeDto>(list);
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchText?.Trim().ToLower() ?? "";
        var filtered = Employees.Where(e =>
        {
            if (string.IsNullOrEmpty(q)) return true;
            var fullName = $"{e.Firstname} {e.Secondname} {e.Lastname}".ToLower();
            return fullName.Contains(q) ||
                   (e.Phone?.ToLower().Contains(q) == true) ||
                   (e.Email?.ToLower().Contains(q) == true);
        });
        FilteredEmployees = new ObservableCollection<EmployeeDto>(filtered);
    }

    [RelayCommand]
    private void ViewStats(EmployeeDto? employee)
    {
        if (employee is not null && IsAdmin)
            OpenEmployeeStatsRequested?.Invoke(employee);
    }
}
