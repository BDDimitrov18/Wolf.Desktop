using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class OrderClientLinkViewModel : ViewModelBase
{
    [ObservableProperty] private int _requestId;
    [ObservableProperty] private ObservableCollection<ClientDto> _availableClients = [];
    [ObservableProperty] private ClientDto? _selectedExistingClient;

    // New client form fields
    [ObservableProperty] private string _firstname = "";
    [ObservableProperty] private string? _middlename;
    [ObservableProperty] private string? _lastname;
    [ObservableProperty] private string? _phone;
    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _address;
    [ObservableProperty] private string? _clientlegaltype;

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _errorMessage;

    public string FormTitle => $"Свързване на клиент с поръчка #{RequestId}";

    public event Action? LinkCompleted;

    public void Load()
    {
        RefreshAvailableClients();
    }

    private void RefreshAvailableClients()
    {
        var allClients = ServiceLocator.Cache.GetAllClients();
        var linked = ServiceLocator.Cache.GetClientsForRequest(RequestId);
        var linkedIds = linked.Select(c => c.Clientid).ToHashSet();
        AvailableClients = new ObservableCollection<ClientDto>(
            allClients.Where(c => !linkedIds.Contains(c.Clientid)));
    }

    [RelayCommand]
    private async Task LinkExisting()
    {
        if (SelectedExistingClient is null) return;
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await ServiceLocator.Cache.LinkClientToRequestAsync(RequestId, SelectedExistingClient.Clientid);
            LinkCompleted?.Invoke();
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
    private async Task CreateAndLink()
    {
        if (string.IsNullOrWhiteSpace(Firstname)) return;
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var newClient = await ServiceLocator.Cache.CreateClientAsync(new CreateClientDto
            {
                Firstname = Firstname,
                Middlename = Middlename,
                Lastname = Lastname,
                Phone = Phone,
                Email = Email,
                Address = Address,
                Clientlegaltype = Clientlegaltype
            });
            if (newClient is not null)
            {
                await ServiceLocator.Cache.LinkClientToRequestAsync(RequestId, newClient.Clientid);
                LinkCompleted?.Invoke();
            }
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
}
