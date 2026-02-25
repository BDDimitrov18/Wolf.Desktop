using Dock.Model.Mvvm.Controls;

namespace Wolf.Desktop.ViewModels;

public class WolfDocument : Document
{
    private ViewModelBase _content = null!;

    public string Key { get; init; } = "";

    public string Icon { get; init; } = "";

    public ViewModelBase Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }
}
