using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class OrderRowViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isExpanded;
    [ObservableProperty] private bool _isSelected;

    public int Requestid { get; init; }
    public string Requestname { get; init; } = "";
    public string Status { get; init; } = "";
    public string Paymentstatus { get; init; } = "";
    public double Price { get; init; }
    public double Advance { get; init; }
    public string? Comments { get; init; }
    public string? Path { get; init; }
    public int? Requestcreatorid { get; init; }
    public string CreatorName { get; init; } = "";

    public event Action<OrderRowViewModel>? EditRequested;
    public event Action<OrderRowViewModel>? DeleteRequested;

    [RelayCommand]
    private void ToggleExpand() => IsExpanded = !IsExpanded;

    [RelayCommand]
    private void Edit() => EditRequested?.Invoke(this);

    [RelayCommand]
    private void Delete() => DeleteRequested?.Invoke(this);

    public static OrderRowViewModel FromDto(RequestDto dto) => new()
    {
        Requestid = dto.Requestid,
        Requestname = dto.Requestname,
        Status = dto.Status,
        Paymentstatus = dto.Paymentstatus,
        Price = dto.Price,
        Advance = dto.Advance,
        Comments = dto.Comments,
        Path = dto.Path,
        Requestcreatorid = dto.Requestcreatorid,
        CreatorName = Services.ServiceLocator.ResolveEmployeeName(dto.Requestcreatorid)
    };
}
