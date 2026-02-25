using System.Globalization;
using Avalonia.Data.Converters;
using Wolf.Dtos;

namespace Wolf.Desktop.Converters;

public class EmployeeNameConverter : IValueConverter
{
    public static readonly EmployeeNameConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EmployeeDto emp)
            return string.Join(" ",
                new[] { emp.Firstname, emp.Secondname, emp.Lastname }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
        return value?.ToString() ?? "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class NullableEmployeeNameConverter : IValueConverter
{
    public static readonly NullableEmployeeNameConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EmployeeDto emp)
            return string.Join(" ",
                new[] { emp.Firstname, emp.Secondname, emp.Lastname }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
        return "Няма контрол";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
