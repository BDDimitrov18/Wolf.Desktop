using System.Globalization;
using Avalonia.Data.Converters;
using Wolf.Desktop.Services;

namespace Wolf.Desktop.Converters;

public class EmployeeIdToNameConverter : IValueConverter
{
    public static readonly EmployeeIdToNameConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int id)
            return ServiceLocator.ResolveEmployeeName(id);
        return value?.ToString() ?? "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class ActivityTypeIdToNameConverter : IValueConverter
{
    public static readonly ActivityTypeIdToNameConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int id)
            return ServiceLocator.Cache.GetActivityType(id)?.Activitytypename ?? $"#{id}";
        return value?.ToString() ?? "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
