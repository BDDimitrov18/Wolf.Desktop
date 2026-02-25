using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class AllDocumentsViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<DocumentsofownershipDto> _documents = [];
    [ObservableProperty] private ObservableCollection<DocumentsofownershipDto> _filteredDocuments = [];
    [ObservableProperty] private DocumentsofownershipDto? _selectedDocument;
    [ObservableProperty] private string _searchText = "";

    public event Action<DocumentsofownershipDto?>? OpenEditDocumentRequested;

    public AllDocumentsViewModel()
    {
        ServiceLocator.Cache.DocumentsChanged += OnDocumentsChanged;
        Load();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void Load()
    {
        var list = ServiceLocator.Cache.GetAllDocuments();
        Documents = new ObservableCollection<DocumentsofownershipDto>(list);
        ApplyFilter();
    }

    private void OnDocumentsChanged()
    {
        var selectedId = SelectedDocument?.Documentid;
        Load();
        if (selectedId.HasValue)
            SelectedDocument = Documents.FirstOrDefault(d => d.Documentid == selectedId.Value);
    }

    private void ApplyFilter()
    {
        var q = SearchText?.Trim().ToLower() ?? "";
        var filtered = Documents.Where(d =>
        {
            if (string.IsNullOrEmpty(q)) return true;
            return d.Typeofdocument.ToLower().Contains(q) ||
                   d.Numberofdocument.ToLower().Contains(q) ||
                   d.Issuer.ToLower().Contains(q) ||
                   d.Typeofownership.ToLower().Contains(q);
        });
        FilteredDocuments = new ObservableCollection<DocumentsofownershipDto>(filtered);
    }

    [RelayCommand]
    private void NewDocument() => OpenEditDocumentRequested?.Invoke(null);

    [RelayCommand]
    private void EditDocument(DocumentsofownershipDto? doc)
    {
        if (doc is not null)
            OpenEditDocumentRequested?.Invoke(doc);
    }

    [RelayCommand]
    private async Task DeleteDocument(DocumentsofownershipDto? doc)
    {
        if (doc is null) return;
        try
        {
            await ServiceLocator.Cache.DeleteDocumentAsync(doc.Documentid);
            if (SelectedDocument?.Documentid == doc.Documentid)
                SelectedDocument = null;
        }
        catch (Exception ex) { ServiceLocator.ShowError($"Грешка при изтриване на документ: {ex.Message}"); }
    }

    [RelayCommand]
    private void DeselectDocument() => SelectedDocument = null;
}
