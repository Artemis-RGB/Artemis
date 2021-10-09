using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using RGBColor = RGB.NET.Core.Color;

namespace Artemis.UI.Avalonia.Converters
{
    /// <summary>
    ///     Converts <see cref="T:RGB.NET.Core.Color" /> into <see cref="T:Avalonia.Media.Color" />.
    /// </summary>
    public class ColorToSolidColorBrushConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            return new SolidColorBrush(!(value is RGBColor color)
                ? new Color(0, 0, 0, 0)
                : new Color((byte) color.A, (byte) color.R, (byte) color.G, (byte) color.B));
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is SolidColorBrush brush)
                ? RGBColor.Transparent
                : new RGBColor(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
        }
    }
}