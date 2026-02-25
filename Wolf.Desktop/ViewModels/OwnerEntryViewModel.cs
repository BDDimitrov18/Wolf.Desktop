using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class OwnerEntryViewModel : ViewModelBase
{
    public ObservableCollection<OwnerDto> AvailableOwners { get; set; } = [];

    [ObservableProperty] private OwnerDto? _selectedOwner;
    [ObservableProperty] private string _ownerSearchText = "";
    [ObservableProperty] private bool _isNewOwner;
    public bool IsExistingOwner => !IsNewOwner;

    partial void OnIsNewOwnerChanged(bool value) => OnPropertyChanged(nameof(IsExistingOwner));
    [ObservableProperty] private string _newOwnerFullname = "";
    [ObservableProperty] private string? _newOwnerEgn;
    [ObservableProperty] private string? _newOwnerAddress;
    [ObservableProperty] private string _idealPartsText = "";
    [ObservableProperty] private string _wayofacquiring = "";
    [ObservableProperty] private string? _poaNumber;
    [ObservableProperty] private DateTime? _poaDateofissuing;
    [ObservableProperty] private string? _poaIssuer;

    // Tracking IDs for existing relationships (when editing)
    public int? ExistingRelId { get; set; }
    public int? ExistingDocOwnerRelId { get; set; }
    public int? ExistingPoaId { get; set; }

    // When true, this entry represents an already-saved owner (shown as read-only info)
    [ObservableProperty] private bool _isReadOnly;

    public static string[] WayOfAcquiringOptions { get; } =
        ["Дарение", "Покупко-Продажба", "наследство", "давност", "делба"];

    public bool IsIdealPartsFraction => IdealPartsText.Contains('/');

    public double IdealPartsValue
    {
        get
        {
            if (string.IsNullOrWhiteSpace(IdealPartsText))
                return 0;

            if (IdealPartsText.Contains('/'))
            {
                var parts = IdealPartsText.Split('/');
                if (parts.Length == 2 &&
                    double.TryParse(parts[0].Trim(), out var num) &&
                    double.TryParse(parts[1].Trim(), out var den) &&
                    den != 0)
                    return num / den;
                return 0;
            }

            return double.TryParse(IdealPartsText.Trim(), out var val) ? val : 0;
        }
    }
}
