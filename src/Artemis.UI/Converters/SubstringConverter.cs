using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Artemis.UI.Converters;

public class SubstringConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter == null || !int.TryParse((string) parameter, out int intParameter))
            return value;
        return value?.ToString()?.Substring(0, intParameter);
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}