using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class InvoicesViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<InvoiceRowViewModel> _invoices = [];
    [ObservableProperty] private ObservableCollection<InvoiceRowViewModel> _filteredInvoices = [];
    [ObservableProperty] private InvoiceRowViewModel? _selectedInvoice;
    [ObservableProperty] private string _searchText = "";

    public event Action<InvoiceDto?>? OpenEditInvoiceRequested;

    public InvoicesViewModel()
    {
        ServiceLocator.Cache.InvoicesChanged += OnInvoicesChanged;
        Load();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void Load()
    {
        var list = ServiceLocator.Cache.GetAllInvoices();
        var rows = list.Select(i =>
        {
            var request = ServiceLocator.Cache.GetRequest(i.Requestid);
            return new InvoiceRowViewModel
            {
                Invoiceid = i.Invoiceid,
                Number = i.Number,
                Sum = i.Sum,
                Requestid = i.Requestid,
                OrderName = request?.Requestname ?? $"Поръчка #{i.Requestid}"
            };
        }).ToList();
        Invoices = new ObservableCollection<InvoiceRowViewModel>(rows);
        ApplyFilter();
    }

    private void OnInvoicesChanged()
    {
        var selectedId = SelectedInvoice?.Invoiceid;
        Load();
        if (selectedId.HasValue)
            SelectedInvoice = Invoices.FirstOrDefault(i => i.Invoiceid == selectedId.Value);
    }

    private void ApplyFilter()
    {
        var q = SearchText?.Trim().ToLower() ?? "";
        var filtered = Invoices.Where(i =>
        {
            if (string.IsNullOrEmpty(q)) return true;
            return i.Number.ToLower().Contains(q) ||
                   i.OrderName.ToLower().Contains(q) ||
                   i.Invoiceid.ToString().Contains(q) ||
                   i.Sum.ToString("N2").Contains(q);
        });
        FilteredInvoices = new ObservableCollection<InvoiceRowViewModel>(filtered);
    }

    [RelayCommand]
    private void NewInvoice() => OpenEditInvoiceRequested?.Invoke(null);

    [RelayCommand]
    private void EditInvoice(InvoiceRowViewModel? row)
    {
        if (row is null) return;
        var dto = ServiceLocator.Cache.GetInvoice(row.Invoiceid);
        if (dto is not null)
            OpenEditInvoiceRequested?.Invoke(dto);
    }

    [RelayCommand]
    private async Task DeleteInvoice(InvoiceRowViewModel? row)
    {
        if (row is null) return;
        try
        {
            await ServiceLocator.Cache.DeleteInvoiceAsync(row.Invoiceid);
            if (SelectedInvoice?.Invoiceid == row.Invoiceid)
                SelectedInvoice = null;
        }
        catch { /* TODO: surface error */ }
    }

    [RelayCommand]
    private void DeselectInvoice() => SelectedInvoice = null;
}

public class InvoiceRowViewModel
{
    public int Invoiceid { get; set; }
    public string Number { get; set; } = "";
    public double Sum { get; set; }
    public int Requestid { get; set; }
    public string OrderName { get; set; } = "";
}
