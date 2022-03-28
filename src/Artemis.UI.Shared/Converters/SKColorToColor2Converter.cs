using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Media;
using SkiaSharp;

namespace Artemis.UI.Shared.Converters;

/// <summary>
///     Converts <see cref="SKColor" /> into <see cref="Color2" />.
/// </summary>
public class SKColorToColor2Converter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SKColor skColor)
            return new Color2(skColor.Red, skColor.Green, skColor.Blue, skColor.Alpha);
        return new Color2();
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color2 color2)
            return new SKColor(color2.R, color2.G, color2.B, color2.A);
        return SKColor.Empty;
    }
}