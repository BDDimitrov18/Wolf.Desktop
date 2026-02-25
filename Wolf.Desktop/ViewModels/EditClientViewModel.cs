using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class EditClientViewModel : ViewModelBase
{
    [ObservableProperty] private int? _clientId;
    [ObservableProperty] private string _firstname = "";
    [ObservableProperty] private string? _middlename;
    [ObservableProperty] private string? _lastname;
    [ObservableProperty] private string? _phone;
    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _address;
    [ObservableProperty] private string? _clientlegaltype;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _errorMessage;

    public bool IsNew => ClientId is null;
    public string FormTitle => IsNew ? "Нов клиент" : $"Редакция на клиент #{ClientId}";

    public event Action? SaveCompleted;
    public event Action? CancelRequested;

    public static readonly string[] LegalTypeOptions = ["Физическо лице", "Юридическо лице", "Държавата", "Общината"];

    public void LoadFromDto(ClientDto dto)
    {
        ClientId = dto.Clientid;
        Firstname = dto.Firstname;
        Middlename = dto.Middlename;
        Lastname = dto.Lastname;
        Phone = dto.Phone;
        Email = dto.Email;
        Address = dto.Address;
        Clientlegaltype = dto.Clientlegaltype;
    }

    [RelayCommand]
    private async Task Save()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            if (IsNew)
            {
                await ServiceLocator.Cache.CreateClientAsync(new CreateClientDto
                {
                    Firstname = Firstname,
                    Middlename = Middlename,
                    Lastname = Lastname,
                    Phone = Phone,
                    Email = Email,
                    Address = Address,
                    Clientlegaltype = Clientlegaltype
                });
            }
            else
            {
                await ServiceLocator.Cache.UpdateClientAsync(ClientId!.Value, new ClientDto
                {
                    Clientid = ClientId.Value,
                    Firstname = Firstname,
                    Middlename = Middlename,
                    Lastname = Lastname,
                    Phone = Phone,
                    Email = Email,
                    Address = Address,
                    Clientlegaltype = Clientlegaltype
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
