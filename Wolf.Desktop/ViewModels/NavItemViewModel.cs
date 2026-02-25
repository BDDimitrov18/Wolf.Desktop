using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Wolf.Desktop.ViewModels;

public partial class NavItemViewModel : ObservableObject
{
    [ObservableProperty] private bool _isActive;

    public string Label { get; init; } = "";
    public string Icon { get; init; } = "";
    public string? Badge { get; init; }
    public string TabKey { get; init; } = "";

    public event Action<NavItemViewModel>? NavigateRequested;

    [RelayCommand]
    private void Navigate() => NavigateRequested?.Invoke(this);
}
