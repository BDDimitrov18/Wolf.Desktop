using System.Globalization;
using Avalonia.Data.Converters;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.Converters;

public class ActivityDisplayConverter : IValueConverter
{
    public static readonly ActivityDisplayConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ActivityDto act)
        {
            var typeName = ServiceLocator.Cache.GetActivityType(act.Activitytypeid)?.Activitytypename ?? $"Тип #{act.Activitytypeid}";
            var executant = ServiceLocator.ResolveEmployeeName(act.Executantid);
            return $"#{act.Activityid} — {typeName} ({executant})";
        }
        return value?.ToString() ?? "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
