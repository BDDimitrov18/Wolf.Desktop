using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class ClientsDetailViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<ClientDto> _clients = [];
    private int _currentRequestId;

    public event Action<ClientDto?>? OpenEditClientRequested;
    public event Action<int>? OpenLinkClientRequested;

    public ClientsDetailViewModel()
    {
        ServiceLocator.Cache.ClientsChanged += OnClientsChanged;
    }

    private void OnClientsChanged()
    {
        if (_currentRequestId > 0)
            LoadForOrder(_currentRequestId);
    }

    public void LoadForOrder(int requestId)
    {
        _currentRequestId = requestId;
        var list = ServiceLocator.Cache.GetClientsForRequest(requestId);
        Clients = new ObservableCollection<ClientDto>(list);
    }

    public void Clear()
    {
        _currentRequestId = 0;
        Clients.Clear();
    }

    [RelayCommand]
    private void EditClient(ClientDto? client) => OpenEditClientRequested?.Invoke(client);

    [RelayCommand]
    private async Task UnlinkClient(ClientDto? client)
    {
        if (client is null || _currentRequestId <= 0) return;
        await ServiceLocator.Cache.UnlinkClientFromRequestAsync(_currentRequestId, client.Clientid);
    }

    [RelayCommand]
    private void LinkClient() => OpenLinkClientRequested?.Invoke(_currentRequestId);
}
