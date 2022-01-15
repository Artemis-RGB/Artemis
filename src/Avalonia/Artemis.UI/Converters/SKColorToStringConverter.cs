using System;
using System.Globalization;
using Avalonia.Data.Converters;
using SkiaSharp;

namespace Artemis.UI.Converters;

/// <inheritdoc />
/// <summary>
///     Converts <see cref="SKColor" />into <see cref="T:System.String" />.
/// </summary>
public class SKColorToStringConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString();
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(value as string))
            return SKColor.Empty;

        return SKColor.TryParse((string) value!, out SKColor color) ? color : SKColor.Empty;
    }
}