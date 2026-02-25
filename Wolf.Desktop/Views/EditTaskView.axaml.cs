using Avalonia.Controls;
using Avalonia.Interactivity;
using Wolf.Dtos;

namespace Wolf.Desktop.Views;

public partial class EditTaskView : UserControl
{
    public EditTaskView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        WireAutoCompleteFilters();
    }

    private void WireAutoCompleteFilters()
    {
        var taskTypeBox = this.FindControl<AutoCompleteBox>("TaskTypeBox");
        if (taskTypeBox is not null)
        {
            taskTypeBox.ItemFilter = (search, item) =>
            {
                if (string.IsNullOrEmpty(search)) return true;
                if (item is not TasktypeDto tt) return false;
                return tt.Tasktypename.Contains(search, StringComparison.OrdinalIgnoreCase);
            };
        }
    }
}
