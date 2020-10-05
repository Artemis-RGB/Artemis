using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using SkiaSharp;

namespace Artemis.UI.Shared
{
    /// <inheritdoc />
    /// <summary>
    ///     Converts <see cref="T:SkiaSharp.SKColor" /> into a <see cref="T:System.Windows.Media.Color" />.
    /// </summary>
    [ValueConversion(typeof(Color), typeof(SKColor))]
    public class SKColorToColorConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SKColor skColor = (SKColor) value;
            return Color.FromArgb(skColor.Alpha, skColor.Red, skColor.Green, skColor.Blue);
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color color = (Color) value;
            return new SKColor(color.R, color.G, color.B, color.A);
        }
    }
}