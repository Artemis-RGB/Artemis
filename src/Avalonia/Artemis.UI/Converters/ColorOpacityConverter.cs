using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Artemis.UI.Converters;

public class ColorOpacityConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color color && double.TryParse(parameter?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double multiplier))
            return new Color((byte) (color.A * multiplier), color.R, color.G, color.B);
        return value;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}