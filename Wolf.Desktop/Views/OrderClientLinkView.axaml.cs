using Avalonia.Controls;
using Avalonia.Interactivity;
using Wolf.Dtos;

namespace Wolf.Desktop.Views;

public partial class OrderClientLinkView : UserControl
{
    public OrderClientLinkView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        WireAutoCompleteFilter();
    }

    private void WireAutoCompleteFilter()
    {
        var clientBox = this.FindControl<AutoCompleteBox>("ClientSearchBox");
        if (clientBox is not null)
        {
            clientBox.ItemFilter = (search, item) =>
            {
                if (string.IsNullOrEmpty(search)) return true;
                if (item is not ClientDto client) return false;
                var fullName = string.Join(" ",
                    new[] { client.Firstname, client.Middlename, client.Lastname }
                        .Where(s => !string.IsNullOrWhiteSpace(s)));
                return fullName.Contains(search, StringComparison.OrdinalIgnoreCase);
            };
        }
    }
}
