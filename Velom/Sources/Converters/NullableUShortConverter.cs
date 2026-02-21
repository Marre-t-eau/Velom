namespace Velom.Sources.Converters;

/// <summary>
/// Converter to handle nullable ushort values in Entry bindings.
/// Allows empty strings to be converted to null.
/// </summary>
public class NullableUShortConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is ushort shortValue)
            return shortValue.ToString();
        
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is string strValue)
        {
            if (string.IsNullOrWhiteSpace(strValue))
                return null;
            
            if (ushort.TryParse(strValue, out ushort result))
                return result;
        }
        
        return null;
    }
}
