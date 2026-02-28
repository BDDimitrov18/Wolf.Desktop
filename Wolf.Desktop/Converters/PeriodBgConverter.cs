using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Wolf.Desktop.Converters;

public class PeriodBgConverter : IValueConverter
{
    public static readonly PeriodBgConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var selected = value?.ToString();
        var key = parameter?.ToString();
        return string.Equals(selected, key, StringComparison.Ordinal)
            ? SolidColorBrush.Parse("#c45d2c")
            : new SolidColorBrush(Colors.Transparent);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
