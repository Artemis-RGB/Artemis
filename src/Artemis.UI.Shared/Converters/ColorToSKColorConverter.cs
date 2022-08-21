using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentAvalonia.UI.Media;
using SkiaSharp;

namespace Artemis.UI.Shared.Converters;

/// <summary>
///     Converts <see cref="T:Avalonia.Media.Color" /> into <see cref="T:SkiaSharp.SKColor" />.
/// </summary>
public class ColorToSKColorConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color avaloniaColor)
            return new SKColor(avaloniaColor.R, avaloniaColor.G, avaloniaColor.B, avaloniaColor.A);
        if (value is Color2 fluentAvaloniaColor)
            return new SKColor(fluentAvaloniaColor.R, fluentAvaloniaColor.G, fluentAvaloniaColor.B, fluentAvaloniaColor.A);

        return SKColor.Empty;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Color result = new(0, 0, 0, 0);
        if (value is SKColor skColor)
            result = new Color(skColor.Alpha, skColor.Red, skColor.Green, skColor.Blue);

        if (targetType == typeof(Color2))
            return (Color2) result;
        return result;
    }
}