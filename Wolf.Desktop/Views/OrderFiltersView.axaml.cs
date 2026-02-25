using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Wolf.Desktop.ViewModels;
using Wolf.Dtos;

namespace Wolf.Desktop.Views;

public partial class OrderFiltersView : UserControl
{
    public OrderFiltersView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        WireTextBoxEvents();
        WireAutoCompleteFilters();
    }

    private void WireTextBoxEvents()
    {
        foreach (var textBox in this.GetVisualDescendants().OfType<TextBox>())
        {
            if (textBox.Classes.Contains("filter-text"))
            {
                textBox.LostFocus += OnFilterTextBoxLostFocus;
                textBox.KeyDown += OnFilterTextBoxKeyDown;
            }
        }
    }

    private void WireAutoCompleteFilters()
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

    private void OnFilterTextBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is OrderFiltersViewModel vm)
            vm.NotifyTextFiltersApplied();
    }

    private void OnFilterTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // Move focus to the UserControl to blur the TextBox
            // This triggers LostFocus which will call NotifyTextFiltersApplied
            this.Focus();
        }
    }
}
