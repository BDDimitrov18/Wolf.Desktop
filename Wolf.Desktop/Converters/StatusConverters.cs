using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Wolf.Desktop.Converters;

public class StatusBgConverter : IValueConverter
{
    public static readonly StatusBgConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString()?.ToLower() switch
        {
            "active"   => SolidColorBrush.Parse("#edf5ee"),
            "archived" => SolidColorBrush.Parse("#edf3f9"),
            _          => SolidColorBrush.Parse("#f4f1ec")
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class StatusFgConverter : IValueConverter
{
    public static readonly StatusFgConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString()?.ToLower() switch
        {
            "active"   => SolidColorBrush.Parse("#3a7d44"),
            "archived" => SolidColorBrush.Parse("#3b6fa0"),
            _          => SolidColorBrush.Parse("#7a6f60")
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
