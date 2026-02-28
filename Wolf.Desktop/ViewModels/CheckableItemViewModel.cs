using CommunityToolkit.Mvvm.ComponentModel;

namespace Wolf.Desktop.ViewModels;

public partial class CheckableItemViewModel : ObservableObject
{
    [ObservableProperty] private bool _isChecked = true;
    public int Id { get; init; }
    public string Label { get; init; } = "";
}
