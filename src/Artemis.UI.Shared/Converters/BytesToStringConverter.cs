using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Humanizer;
using Humanizer.Bytes;

namespace Artemis.UI.Shared.Converters;

/// <summary>
/// Converts bytes to a string
/// </summary>
public class BytesToStringConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intBytes)
            return intBytes.Bytes().Humanize();
        if (value is long longBytes)
            return longBytes.Bytes().Humanize();
        if (value is double doubleBytes)
            return doubleBytes.Bytes().Humanize();
        
        return value;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string formatted && ByteSize.TryParse(formatted, out ByteSize result))
            return result.Bytes;
        return value;
    }
}