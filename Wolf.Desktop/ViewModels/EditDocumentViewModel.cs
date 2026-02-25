using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class EditDocumentViewModel : ViewModelBase
{
    [ObservableProperty] private int? _documentId;
    [ObservableProperty] private int _plotId;
    [ObservableProperty] private string _typeofdocument = "";
    [ObservableProperty] private string _numberofdocument = "";
    [ObservableProperty] private string _issuer = "";
    [ObservableProperty] private int _tom;
    [ObservableProperty] private string _register = "";
    [ObservableProperty] private string _doccase = "";
    [ObservableProperty] private DateTime _dateofissuing = DateTime.Today;
    [ObservableProperty] private DateTime _dateofregistering = DateTime.Today;
    [ObservableProperty] private string _typeofownership = "";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _errorMessage;

    public ObservableCollection<OwnerEntryViewModel> OwnerEntries { get; } = [];

    // Existing documents available for linking
    public ObservableCollection<DocumentPickerItem> AvailableDocuments { get; } = [];
    [ObservableProperty] private DocumentPickerItem? _selectedExistingDocument;
    [ObservableProperty] private bool _isLinkingExisting;
    public bool IsCreatingNew => !IsLinkingExisting;

    partial void OnIsLinkingExistingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsCreatingNew));
        OnPropertyChanged(nameof(FormTitle));
        if (!value)
        {
            // Switching back to "new" mode — clear selected doc and owners
            SelectedExistingDocument = null;
            DocumentId = null;
            Typeofdocument = "";
            Numberofdocument = "";
            Issuer = "";
            Tom = 0;
            Register = "";
            Doccase = "";
            Dateofissuing = DateTime.Today;
            Dateofregistering = DateTime.Today;
            Typeofownership = "";
            OwnerEntries.Clear();
            _originalRelIds.Clear();
        }
    }

    partial void OnSelectedExistingDocumentChanged(DocumentPickerItem? value)
    {
        if (value is null) return;
        var dto = value.Dto;
        DocumentId = dto.Documentid;
        Typeofdocument = dto.Typeofdocument;
        Numberofdocument = dto.Numberofdocument;
        Issuer = dto.Issuer;
        Tom = dto.Tom;
        Register = dto.Register;
        Doccase = dto.Doccase;
        Dateofissuing = dto.Dateofissuing;
        Dateofregistering = dto.Dateofregistering;
        Typeofownership = dto.Typeofownership;
        OnPropertyChanged(nameof(FormTitle));

        // Load existing owners for this document
        LoadOwnersForDocument(dto.Documentid);
    }

    private void LoadOwnersForDocument(int docId)
    {
        OwnerEntries.Clear();
        _originalRelIds.Clear();

        var cache = ServiceLocator.Cache;
        var owners = cache.GetOwnersForDocument(docId);

        foreach (var owner in owners)
        {
            var docOwnerRel = cache.GetDocOwnerRelForDocumentAndOwner(docId, owner.Ownerid);
            if (docOwnerRel is null) continue;

            var entry = new OwnerEntryViewModel
            {
                AvailableOwners = new ObservableCollection<OwnerDto>(_allOwners),
                SelectedOwner = owner,
                OwnerSearchText = owner.Fullname,
                ExistingDocOwnerRelId = docOwnerRel.Documentownerid,
                IsReadOnly = true
            };

            // Try to load three-way rel data if a plot context exists
            if (PlotId > 0)
            {
                var plotDocRel = cache.GetPlotDocumentRel(PlotId, docId);
                if (plotDocRel is not null)
                {
                    var threeWayRels = cache.GetDocPlotDocOwnerRelsForDocPlot(plotDocRel.Documentplotid);
                    var threeWay = threeWayRels.FirstOrDefault(r => r.Documentownerid == docOwnerRel.Documentownerid);
                    if (threeWay is not null)
                    {
                        _originalRelIds.Add(threeWay.Id);
                        entry.ExistingRelId = threeWay.Id;
                        entry.IdealPartsText = threeWay.Idealparts.ToString("G");
                        entry.Wayofacquiring = threeWay.Wayofacquiring;

                        if (threeWay.Powerofattorneyid > 0)
                        {
                            entry.ExistingPoaId = threeWay.Powerofattorneyid;
                            var poa = cache.GetPowerofattorneyDoc(threeWay.Powerofattorneyid);
                            if (poa is not null)
                            {
                                entry.PoaNumber = poa.Number;
                                entry.PoaDateofissuing = poa.Dateofissuing;
                                entry.PoaIssuer = poa.Issuer;
                            }
                        }
                    }
                }
            }

            OwnerEntries.Add(entry);
        }
    }

    public static string[] DocumentTypeOptions { get; } =
    [
        "нотариален акт", "договор делба", "договор за покупко продажба",
        "завещание", "акт за общинска собственост", "акт за държавна собственост",
        "постановление ЧСИ", "договор за отстъпено право на строеж", "дружествен договор"
    ];

    public static string[] IssuerOptions { get; } =
    [
        "Агенция по Вписвания при РС", "Община", "Областен управител",
        "Общинска служба замеделие", "Окръжен народен съвет"
    ];

    public static string[] OwnershipTypeOptions { get; } =
    [
        "Няма данни", "Държавна публична", "Дъжавна частна",
        "Общинска публична", "Общинска частна", "Частна", "Частна кооперативна",
        "Частна обществени организации", "Частна чужди физически и юридически лица",
        "Частна международни организации", "Частна религиозни организации",
        "Собственост", "Изключителна държавна собственост",
        "стопанисвана от държавата по чл. 14а от ЗВСГЗГФ",
        "държавна от собственотст по чл.6 ЗВСГЗГФ", "Стопанисвано от Общината"
    ];

    // All owners available for picker (shared across entries)
    private List<OwnerDto> _allOwners = [];

    // Track original three-way rel IDs so we can detect removals
    private readonly List<int> _originalRelIds = [];

    // Whether the form was opened for a brand-new document (not editing an existing one)
    private bool _openedAsNew = true;
    public bool IsOpenedAsNew => _openedAsNew;

    public bool IsNew => _openedAsNew && !IsLinkingExisting;
    public string FormTitle => IsLinkingExisting
        ? $"Добави собственост към документ #{DocumentId}"
        : _openedAsNew ? "Нов документ" : $"Редакция на документ #{DocumentId}";

    public event Action? SaveCompleted;
    public event Action? CancelRequested;

    public void LoadFromDto(DocumentsofownershipDto dto)
    {
        _openedAsNew = false;
        DocumentId = dto.Documentid;
        Typeofdocument = dto.Typeofdocument;
        Numberofdocument = dto.Numberofdocument;
        Issuer = dto.Issuer;
        Tom = dto.Tom;
        Register = dto.Register;
        Doccase = dto.Doccase;
        Dateofissuing = dto.Dateofissuing;
        Dateofregistering = dto.Dateofregistering;
        Typeofownership = dto.Typeofownership;
    }

    public void LoadPickerData()
    {
        _allOwners = ServiceLocator.Cache.GetAllOwners().ToList();

        // Load all existing documents for the "link existing" picker
        AvailableDocuments.Clear();
        foreach (var doc in ServiceLocator.Cache.GetAllDocuments().OrderBy(d => d.Numberofdocument))
            AvailableDocuments.Add(new DocumentPickerItem(doc));
    }

    public void LoadExistingOwners()
    {
        if (DocumentId is null || PlotId <= 0)
            return;

        var cache = ServiceLocator.Cache;
        var plotDocRel = cache.GetPlotDocumentRel(PlotId, DocumentId.Value);
        if (plotDocRel is null)
            return;

        var threeWayRels = cache.GetDocPlotDocOwnerRelsForDocPlot(plotDocRel.Documentplotid);
        foreach (var rel in threeWayRels)
        {
            _originalRelIds.Add(rel.Id);

            // Find the doc-owner rel to get the owner
            var docOwnerRels = cache.GetAllOwners(); // we need the doc-owner rel itself
            // Look up via _docOwnerRels through cache
            var ownerDto = FindOwnerForDocOwnerRel(rel.Documentownerid);
            var poaDoc = rel.Powerofattorneyid > 0
                ? cache.GetPowerofattorneyDoc(rel.Powerofattorneyid)
                : null;

            var entry = new OwnerEntryViewModel
            {
                AvailableOwners = new ObservableCollection<OwnerDto>(_allOwners),
                SelectedOwner = ownerDto,
                OwnerSearchText = ownerDto?.Fullname ?? "",
                IdealPartsText = rel.Isdrob == true
                    ? rel.Idealparts.ToString("G")
                    : rel.Idealparts.ToString("G"),
                Wayofacquiring = rel.Wayofacquiring,
                ExistingRelId = rel.Id,
                ExistingDocOwnerRelId = rel.Documentownerid,
                ExistingPoaId = rel.Powerofattorneyid > 0 ? rel.Powerofattorneyid : null,
                PoaNumber = poaDoc?.Number,
                PoaDateofissuing = poaDoc?.Dateofissuing,
                PoaIssuer = poaDoc?.Issuer
            };
            OwnerEntries.Add(entry);
        }
    }

    private OwnerDto? FindOwnerForDocOwnerRel(int documentownerid)
    {
        // documentownerid is the PK of DocumentofownershipOwnerrelashionship
        // We need to find which owner it points to
        var cache = ServiceLocator.Cache;
        // Search through all doc-owner rels to find the one with this ID
        var allOwners = cache.GetOwnersForDocument(DocumentId!.Value);
        // We need to find the specific doc-owner rel by documentownerid
        // The doc-owner rel links Documentid + Ownerid; its PK is Documentownerid
        // Since we can't directly look up by Documentownerid in cache, we check all owners for this doc
        // and match by trying each one's doc-owner rel
        foreach (var owner in cache.GetAllOwners())
        {
            var rel = cache.GetDocOwnerRelForDocumentAndOwner(DocumentId.Value, owner.Ownerid);
            if (rel is not null && rel.Documentownerid == documentownerid)
                return owner;
        }
        return null;
    }

    [RelayCommand]
    private void AddOwner()
    {
        OwnerEntries.Add(new OwnerEntryViewModel
        {
            AvailableOwners = new ObservableCollection<OwnerDto>(_allOwners)
        });
    }

    [RelayCommand]
    private void RemoveOwner(OwnerEntryViewModel entry)
    {
        OwnerEntries.Remove(entry);
    }

    [RelayCommand]
    private async Task Save()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            int documentId;
            int documentPlotId;

            if (IsLinkingExisting && SelectedExistingDocument is not null)
            {
                // Link an existing document to this plot
                documentId = SelectedExistingDocument.Dto.Documentid;

                // Create plot-document link if it doesn't already exist
                if (PlotId > 0)
                {
                    var existingLink = ServiceLocator.Cache.GetPlotDocumentRel(PlotId, documentId);
                    if (existingLink is null)
                        await ServiceLocator.Cache.LinkDocumentToPlotAsync(PlotId, documentId);
                }

                var plotDocRel = ServiceLocator.Cache.GetPlotDocumentRel(PlotId, documentId);
                documentPlotId = plotDocRel?.Documentplotid ?? 0;
            }
            else if (DocumentId is null)
            {
                // Create a brand new document
                var created = await ServiceLocator.Cache.CreateDocumentAsync(new CreateDocumentsofownershipDto
                {
                    Typeofdocument = Typeofdocument,
                    Numberofdocument = Numberofdocument,
                    Issuer = Issuer,
                    Tom = Tom,
                    Register = Register,
                    Doccase = Doccase,
                    Dateofissuing = Dateofissuing,
                    Dateofregistering = Dateofregistering,
                    Typeofownership = Typeofownership
                });
                if (created is null)
                    throw new Exception("Неуспешно създаване на документ.");

                documentId = created.Documentid;

                if (PlotId > 0)
                    await ServiceLocator.Cache.LinkDocumentToPlotAsync(PlotId, documentId);

                var plotDocRel = ServiceLocator.Cache.GetPlotDocumentRel(PlotId, documentId);
                documentPlotId = plotDocRel?.Documentplotid ?? 0;
            }
            else
            {
                // Update existing document
                documentId = DocumentId!.Value;
                await ServiceLocator.Cache.UpdateDocumentAsync(documentId, new DocumentsofownershipDto
                {
                    Documentid = documentId,
                    Typeofdocument = Typeofdocument,
                    Numberofdocument = Numberofdocument,
                    Issuer = Issuer,
                    Tom = Tom,
                    Register = Register,
                    Doccase = Doccase,
                    Dateofissuing = Dateofissuing,
                    Dateofregistering = Dateofregistering,
                    Typeofownership = Typeofownership
                });

                var plotDocRel = ServiceLocator.Cache.GetPlotDocumentRel(PlotId, documentId);
                documentPlotId = plotDocRel?.Documentplotid ?? 0;
            }

            // Save owner entries
            if (PlotId > 0 && documentPlotId > 0)
            {
                // Delete removed three-way rels
                var currentRelIds = OwnerEntries
                    .Where(e => e.ExistingRelId.HasValue)
                    .Select(e => e.ExistingRelId!.Value)
                    .ToHashSet();

                foreach (var origId in _originalRelIds)
                {
                    if (!currentRelIds.Contains(origId))
                        await ServiceLocator.Cache.DeleteDocPlotDocOwnerRelAsync(origId);
                }

                // Create/update owner entries
                foreach (var entry in OwnerEntries)
                {
                    if (entry.ExistingRelId.HasValue)
                        continue; // Already exists, skip (no update logic for simplicity)

                    // 1. Get or create owner
                    int ownerId;
                    if (entry.IsNewOwner)
                    {
                        if (string.IsNullOrWhiteSpace(entry.NewOwnerFullname))
                            continue;
                        var newOwner = await ServiceLocator.Cache.CreateOwnerAsync(new CreateOwnerDto
                        {
                            Fullname = entry.NewOwnerFullname,
                            Egn = entry.NewOwnerEgn,
                            Address = entry.NewOwnerAddress
                        });
                        if (newOwner is null) continue;
                        ownerId = newOwner.Ownerid;
                    }
                    else
                    {
                        if (entry.SelectedOwner is null)
                            continue;
                        ownerId = entry.SelectedOwner.Ownerid;
                    }

                    // 2. Create doc-owner rel (if not already linked)
                    var existingDocOwnerRel = ServiceLocator.Cache.GetDocOwnerRelForDocumentAndOwner(documentId, ownerId);
                    int documentOwnerId;
                    if (existingDocOwnerRel is not null)
                    {
                        documentOwnerId = existingDocOwnerRel.Documentownerid;
                    }
                    else
                    {
                        var docOwnerRel = await ServiceLocator.Cache.CreateDocOwnerRelAsync(
                            new CreateDocumentofownershipOwnerrelashionshipDto
                            {
                                Documentid = documentId,
                                Ownerid = ownerId
                            });
                        if (docOwnerRel is null) continue;
                        documentOwnerId = docOwnerRel.Documentownerid;
                    }

                    // 3. Create PoA if fields are filled
                    int poaId = 0;
                    if (!string.IsNullOrWhiteSpace(entry.PoaNumber))
                    {
                        var poa = await ServiceLocator.Cache.CreatePowerofattorneyDocAsync(
                            new CreatePowerofattorneydocumentDto
                            {
                                Number = entry.PoaNumber,
                                Dateofissuing = entry.PoaDateofissuing ?? DateTime.Today,
                                Issuer = entry.PoaIssuer ?? ""
                            });
                        if (poa is not null)
                            poaId = poa.Powerofattorneyid;
                    }

                    // 4. Create three-way rel
                    await ServiceLocator.Cache.CreateDocPlotDocOwnerRelAsync(
                        new CreateDocumentplotDocumentowenerrelashionshipDto
                        {
                            Documentplotid = documentPlotId,
                            Documentownerid = documentOwnerId,
                            Idealparts = entry.IdealPartsValue,
                            Isdrob = entry.IsIdealPartsFraction,
                            Wayofacquiring = entry.Wayofacquiring ?? "",
                            Powerofattorneyid = poaId
                        });
                }
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

public class DocumentPickerItem
{
    public DocumentsofownershipDto Dto { get; }
    public string DisplayText { get; }

    public DocumentPickerItem(DocumentsofownershipDto dto)
    {
        Dto = dto;
        DisplayText = $"#{dto.Documentid} — {dto.Typeofdocument} {dto.Numberofdocument}".Trim();
    }

    public override string ToString() => DisplayText;
}
