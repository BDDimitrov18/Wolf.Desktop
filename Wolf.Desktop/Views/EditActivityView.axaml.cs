using Avalonia.Controls;
using Avalonia.Interactivity;
using Wolf.Dtos;

namespace Wolf.Desktop.Views;

public partial class EditActivityView : UserControl
{
    public EditActivityView()
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
        var activityTypeBox = this.FindControl<AutoCompleteBox>("ActivityTypeBox");
        if (activityTypeBox is not null)
        {
            activityTypeBox.ItemFilter = (search, item) =>
            {
                if (string.IsNullOrEmpty(search)) return true;
                if (item is not ActivitytypeDto at) return false;
                return at.Activitytypename.Contains(search, StringComparison.OrdinalIgnoreCase);
            };
        }

    }
}
