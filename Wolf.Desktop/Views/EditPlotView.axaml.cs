using Avalonia.Controls;
using Wolf.Dtos;

namespace Wolf.Desktop.Views;

public partial class EditPlotView : UserControl
{
    public EditPlotView()
    {
        InitializeComponent();

        var plotNumberBox = this.FindControl<AutoCompleteBox>("PlotNumberBox");
        if (plotNumberBox is not null)
        {
            plotNumberBox.ItemFilter = (search, item) =>
            {
                if (item is PlotDto plot && !string.IsNullOrEmpty(search))
                    return plot.Plotnumber.Contains(search, StringComparison.OrdinalIgnoreCase);
                return false;
            };
        }
    }
}
