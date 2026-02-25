using System.Globalization;
using Avalonia.Data.Converters;
using Wolf.Dtos;

namespace Wolf.Desktop.Converters;

public class ClientNameConverter : IValueConverter
{
    public static readonly ClientNameConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ClientDto client)
            return string.Join(" ",
                new[] { client.Firstname, client.Middlename, client.Lastname }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
        return value?.ToString() ?? "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
