using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Artemis.UI.Converters;

/// <summary>
///     Converts <see cref="Color" /> into <see cref="SolidColorBrush" />.
/// </summary>
public class ColorToSolidColorBrushConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new SolidColorBrush(value is not Color color ? new Color(0, 0, 0, 0) : color);
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not SolidColorBrush brush ? new Color(0, 0, 0, 0) : brush.Color;
    }
}