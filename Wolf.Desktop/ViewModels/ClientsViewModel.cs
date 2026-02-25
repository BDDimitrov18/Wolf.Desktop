using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class ClientsViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<ClientDto> _clients = [];
    [ObservableProperty] private ObservableCollection<ClientDto> _filteredClients = [];
    [ObservableProperty] private ClientDto? _selectedClient;
    [ObservableProperty] private string _searchText = "";

    public event Action<ClientDto?>? OpenEditClientRequested;

    public ClientsViewModel()
    {
        ServiceLocator.Cache.ClientsChanged += OnClientsChanged;
        Load();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void Load()
    {
        var list = ServiceLocator.Cache.GetAllClients();
        Clients = new ObservableCollection<ClientDto>(list);
        ApplyFilter();
    }

    private void OnClientsChanged()
    {
        var selectedId = SelectedClient?.Clientid;
        Load();
        if (selectedId.HasValue)
            SelectedClient = Clients.FirstOrDefault(c => c.Clientid == selectedId.Value);
    }

    private void ApplyFilter()
    {
        var q = SearchText?.Trim().ToLower() ?? "";
        var filtered = Clients.Where(c =>
        {
            if (string.IsNullOrEmpty(q)) return true;
            var fullName = $"{c.Firstname} {c.Middlename} {c.Lastname}".ToLower();
            return fullName.Contains(q) ||
                   (c.Phone?.ToLower().Contains(q) == true) ||
                   (c.Email?.ToLower().Contains(q) == true);
        });
        FilteredClients = new ObservableCollection<ClientDto>(filtered);
    }

    [RelayCommand]
    private void NewClient() => OpenEditClientRequested?.Invoke(null);

    [RelayCommand]
    private void EditClient(ClientDto? client)
    {
        if (client is not null)
            OpenEditClientRequested?.Invoke(client);
    }

    [RelayCommand]
    private async Task DeleteClient(ClientDto? client)
    {
        if (client is null) return;
        try
        {
            await ServiceLocator.Cache.DeleteClientAsync(client.Clientid);
            if (SelectedClient?.Clientid == client.Clientid)
                SelectedClient = null;
        }
        catch (Exception ex) { ServiceLocator.ShowError($"Грешка при изтриване на клиент: {ex.Message}"); }
    }

    [RelayCommand]
    private void DeselectClient() => SelectedClient = null;
}
