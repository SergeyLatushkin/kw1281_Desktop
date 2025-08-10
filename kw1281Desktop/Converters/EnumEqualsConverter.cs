using System.Globalization;

namespace kw1281Desktop.Converters;

internal class EnumEqualsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null)
        {
            return false;
        }

        return value.ToString() == parameter.ToString();
    }


    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b && parameter is string str)
        {
            return Enum.Parse(targetType, str);
        }

        return Binding.DoNothing;
    }
}
