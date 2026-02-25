using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class EditInvoiceViewModel : ViewModelBase
{
    [ObservableProperty] private int? _invoiceId;
    [ObservableProperty] private string _number = "";
    [ObservableProperty] private double _sum;
    [ObservableProperty] private int _requestId;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _errorMessage;

    [ObservableProperty] private ObservableCollection<RequestPickerItem> _availableOrders = [];

    public bool IsNew => InvoiceId is null;
    public string FormTitle => IsNew ? "Нова фактура" : $"Редакция на фактура #{InvoiceId}";

    public event Action? SaveCompleted;
    public event Action? CancelRequested;

    public EditInvoiceViewModel()
    {
        LoadOrders();
    }

    private void LoadOrders()
    {
        var requests = ServiceLocator.Cache.GetAllRequests();
        AvailableOrders = new ObservableCollection<RequestPickerItem>(
            requests.Select(r => new RequestPickerItem
            {
                Requestid = r.Requestid,
                Display = $"#{r.Requestid} — {r.Requestname}"
            }));
    }

    public void LoadFromDto(InvoiceDto dto)
    {
        InvoiceId = dto.Invoiceid;
        Number = dto.Number;
        Sum = dto.Sum;
        RequestId = dto.Requestid;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(Number))
        {
            ErrorMessage = "Номерът на фактурата е задължителен.";
            return;
        }
        if (RequestId <= 0)
        {
            ErrorMessage = "Моля, изберете поръчка.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            if (IsNew)
            {
                await ServiceLocator.Cache.CreateInvoiceAsync(new CreateInvoiceDto
                {
                    Number = Number,
                    Sum = Sum,
                    Requestid = RequestId
                });
            }
            else
            {
                await ServiceLocator.Cache.UpdateInvoiceAsync(InvoiceId!.Value, new InvoiceDto
                {
                    Invoiceid = InvoiceId.Value,
                    Number = Number,
                    Sum = Sum,
                    Requestid = RequestId
                });
            }
            SaveCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel() => CancelRequested?.Invoke();
}

public class RequestPickerItem
{
    public int Requestid { get; set; }
    public string Display { get; set; } = "";
    public override string ToString() => Display;
}
