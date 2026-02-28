using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class InvoicesDetailViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<InvoiceDto> _invoices = [];

    private int _currentRequestId;

    public event Action<InvoiceDto?, int>? OpenEditInvoiceRequested; // (dto or null, requestId)

    public InvoicesDetailViewModel()
    {
        ServiceLocator.Cache.InvoicesChanged += OnInvoicesChanged;
    }

    public void LoadForOrder(int requestId)
    {
        _currentRequestId = requestId;
        var list = ServiceLocator.Cache.GetInvoicesForRequest(requestId);
        Invoices = new ObservableCollection<InvoiceDto>(list);
    }

    public void Clear()
    {
        _currentRequestId = 0;
        Invoices.Clear();
    }

    private void OnInvoicesChanged()
    {
        if (_currentRequestId > 0)
            LoadForOrder(_currentRequestId);
    }

    [RelayCommand]
    private void AddInvoice()
    {
        if (_currentRequestId <= 0)
        {
            ServiceLocator.ShowError("Моля, първо изберете поръчка.");
            return;
        }
        OpenEditInvoiceRequested?.Invoke(null, _currentRequestId);
    }

    [RelayCommand]
    private void EditInvoice(InvoiceDto? dto)
    {
        if (dto is not null)
            OpenEditInvoiceRequested?.Invoke(dto, _currentRequestId);
    }

    [RelayCommand]
    private async Task DeleteInvoice(InvoiceDto? dto)
    {
        if (dto is null) return;
        try
        {
            await ServiceLocator.Cache.DeleteInvoiceAsync(dto.Invoiceid);
        }
        catch (Exception ex) { ServiceLocator.ShowError($"Грешка при изтриване на фактура: {ex.Message}"); }
    }
}
