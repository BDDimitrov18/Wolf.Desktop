using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

/// <summary>
/// Represents a plot card with its nested documents, for display in the Plots &amp; Docs tab.
/// </summary>
public partial class PlotCardViewModel : ViewModelBase
{
    [ObservableProperty] private int _plotId;
    [ObservableProperty] private string _plotnumber = "";
    [ObservableProperty] private string? _regulatedplotnumber;
    [ObservableProperty] private string? _neighborhood;
    [ObservableProperty] private string? _city;
    [ObservableProperty] private string? _municipality;
    [ObservableProperty] private string? _street;
    [ObservableProperty] private int? _streetnumber;
    [ObservableProperty] private string _designation = "";
    [ObservableProperty] private string? _locality;
    [ObservableProperty] private bool _isShared;
    [ObservableProperty] private ObservableCollection<DocumentsofownershipDto> _documents = [];

    public string LocationSummary
    {
        get
        {
            var parts = new[] { City, Municipality, Neighborhood, Locality }
                .Where(s => !string.IsNullOrWhiteSpace(s));
            return string.Join(", ", parts);
        }
    }

    public string StreetSummary
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Street)) return "";
            return Streetnumber.HasValue ? $"{Street} {Streetnumber}" : Street;
        }
    }

    public static PlotCardViewModel FromDto(PlotDto dto, IReadOnlyList<DocumentsofownershipDto> docs, bool isShared)
    {
        return new PlotCardViewModel
        {
            PlotId = dto.Plotid,
            Plotnumber = dto.Plotnumber,
            Regulatedplotnumber = dto.Regulatedplotnumber,
            Neighborhood = dto.Neighborhood,
            City = dto.City,
            Municipality = dto.Municipality,
            Street = dto.Street,
            Streetnumber = dto.Streetnumber,
            Designation = dto.Designation,
            Locality = dto.Locality,
            IsShared = isShared,
            Documents = new ObservableCollection<DocumentsofownershipDto>(docs)
        };
    }
}
