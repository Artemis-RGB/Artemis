using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Skia;
using SkiaSharp;

namespace Artemis.UI.Converters;

public class ColorLuminosityConverter : IValueConverter
{
    private static Color Brighten(Color color, float multiplier)
    {
        color.ToSKColor().ToHsl(out float h, out float s, out float l);
        l += l * (multiplier / 100f);
        SKColor skColor = SKColor.FromHsl(h, s, l);

        return new Color(skColor.Alpha, skColor.Red, skColor.Green, skColor.Blue);
    }

    private static Color Darken(Color color, float multiplier)
    {
        color.ToSKColor().ToHsl(out float h, out float s, out float l);
        l -= l * (multiplier / 100f);
        SKColor skColor = SKColor.FromHsl(h, s, l);

        return new Color(skColor.Alpha, skColor.Red, skColor.Green, skColor.Blue);
    }

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color color && double.TryParse(parameter?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double multiplier))
            return multiplier > 0 ? Brighten(color, (float) multiplier) : Darken(color, (float) multiplier * -1);

        return value;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}