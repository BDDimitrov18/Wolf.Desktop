using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class PlotsDetailViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<PlotCardViewModel> _plotCards = [];

    private int _currentRequestId;

    // Events for MainWindowViewModel to open edit tabs
    public event Action<PlotDto?, int>? OpenEditPlotRequested;       // (dto or null, requestId)
    public event Action<DocumentsofownershipDto?, int>? OpenEditDocumentRequested; // (dto or null, plotId)

    public PlotsDetailViewModel()
    {
        ServiceLocator.Cache.PlotsChanged += OnPlotsOrDocsChanged;
        ServiceLocator.Cache.DocumentsChanged += OnPlotsOrDocsChanged;
    }

    public void LoadForOrder(int requestId)
    {
        _currentRequestId = requestId;
        RebuildCards();
    }

    public void Clear()
    {
        _currentRequestId = 0;
        PlotCards.Clear();
    }

    private void OnPlotsOrDocsChanged()
    {
        if (_currentRequestId > 0)
            RebuildCards();
    }

    private void RebuildCards()
    {
        var plots = ServiceLocator.Cache.GetPlotsForRequest(_currentRequestId);
        var cards = plots.Select(p =>
        {
            var docs = ServiceLocator.Cache.GetDocumentsForPlot(p.Plotid);
            var isShared = ServiceLocator.Cache.IsPlotShared(p.Plotid);
            return PlotCardViewModel.FromDto(p, docs, isShared);
        });
        PlotCards = new ObservableCollection<PlotCardViewModel>(cards);
    }

    [RelayCommand]
    private void AddPlot() => OpenEditPlotRequested?.Invoke(null, _currentRequestId);

    [RelayCommand]
    private void EditPlot(PlotCardViewModel? card)
    {
        if (card is null) return;
        var dto = ServiceLocator.Cache.GetPlot(card.PlotId);
        if (dto is not null)
            OpenEditPlotRequested?.Invoke(dto, _currentRequestId);
    }

    [RelayCommand]
    private async Task DeletePlot(PlotCardViewModel? card)
    {
        if (card is null) return;
        try
        {
            await ServiceLocator.Cache.DeletePlotAsync(card.PlotId);
        }
        catch { /* TODO: surface error */ }
    }

    [RelayCommand]
    private void AddDocument(PlotCardViewModel? card)
    {
        if (card is null) return;
        OpenEditDocumentRequested?.Invoke(null, card.PlotId);
    }

    [RelayCommand]
    private void EditDocument(DocumentsofownershipDto? dto)
    {
        if (dto is null) return;
        // Find which plot this document belongs to (use first match)
        var card = PlotCards.FirstOrDefault(c => c.Documents.Any(d => d.Documentid == dto.Documentid));
        if (card is not null)
            OpenEditDocumentRequested?.Invoke(dto, card.PlotId);
    }

    [RelayCommand]
    private async Task DeleteDocument(DocumentsofownershipDto? dto)
    {
        if (dto is null) return;
        try
        {
            // Only unlink the document from the plot, don't delete the document itself
            var card = PlotCards.FirstOrDefault(c =>
                c.Documents.Any(d => d.Documentid == dto.Documentid));
            if (card is null) return;

            var rel = ServiceLocator.Cache.GetPlotDocumentRel(card.PlotId, dto.Documentid);
            if (rel is not null)
                await ServiceLocator.Cache.UnlinkDocumentFromPlotAsync(rel.Documentplotid);
        }
        catch { /* TODO: surface error */ }
    }
}
