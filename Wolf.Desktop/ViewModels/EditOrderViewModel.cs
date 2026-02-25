using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class EditOrderViewModel : ViewModelBase
{
    [ObservableProperty] private int? _requestId;
    [ObservableProperty] private string _requestname = "";
    [ObservableProperty] private double _price;
    [ObservableProperty] private double _advance;
    [ObservableProperty] private string _status = "Active";
    [ObservableProperty] private string? _comments;
    [ObservableProperty] private string? _path;
    [ObservableProperty] private int? _requestcreatorid;
    [ObservableProperty] private string _creatorName = "";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _errorMessage;

    public bool IsNew => RequestId is null;
    public string FormTitle => IsNew ? "Нова поръчка" : $"Редакция на поръчка #{RequestId}";

    public event Action? SaveCompleted;
    public event Action? CancelRequested;

    public static readonly string[] StatusOptions = ["Active", "Archived"];

    public EditOrderViewModel()
    {
        // Auto-assign creator to the currently logged-in employee
        if (ServiceLocator.Auth.CurrentEmployeeId.HasValue)
        {
            Requestcreatorid = ServiceLocator.Auth.CurrentEmployeeId;
            CreatorName = ServiceLocator.Auth.CurrentDisplayName;
        }
    }

    public void LoadFromDto(RequestDto dto)
    {
        RequestId = dto.Requestid;
        Requestname = dto.Requestname;
        Price = dto.Price;
        Advance = dto.Advance;
        Status = dto.Status;
        Comments = dto.Comments;
        Path = dto.Path;
        Requestcreatorid = dto.Requestcreatorid;
        CreatorName = ServiceLocator.ResolveEmployeeName(dto.Requestcreatorid);
    }

    private static string ComputePaymentStatus(double price, double advance)
    {
        if (advance >= price && price > 0) return "Платен";
        if (advance > 0) return "Аванс";
        return "Не платен";
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(Requestname))
        {
            ErrorMessage = "Името на поръчката е задължително.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var paymentstatus = ComputePaymentStatus(Price, Advance);

            if (IsNew)
            {
                await ServiceLocator.Cache.CreateRequestAsync(new CreateRequestDto
                {
                    Requestname = Requestname,
                    Price = Price,
                    Advance = Advance,
                    Status = Status,
                    Paymentstatus = paymentstatus,
                    Comments = Comments,
                    Path = Path,
                    Requestcreatorid = Requestcreatorid
                });
            }
            else
            {
                await ServiceLocator.Cache.UpdateRequestAsync(RequestId!.Value, new RequestDto
                {
                    Requestid = RequestId.Value,
                    Requestname = Requestname,
                    Price = Price,
                    Advance = Advance,
                    Status = Status,
                    Paymentstatus = paymentstatus,
                    Comments = Comments,
                    Path = Path,
                    Requestcreatorid = Requestcreatorid
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
