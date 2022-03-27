using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using SkiaSharp;

namespace Artemis.UI.Shared.Converters;

/// <summary>
///     Converts <see cref="T:SkiaSharp.SKColor" /> into <see cref="T:Avalonia.Media.SolidColorBrush" />.
/// </summary>
public class SKColorToBrushConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SKColor skColor)
            return new SolidColorBrush(new Color(skColor.Alpha, skColor.Red, skColor.Green, skColor.Blue));
        return new SolidColorBrush(Colors.Transparent);
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SolidColorBrush brush)
            return new SKColor(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
        return SKColor.Empty;
    }
}