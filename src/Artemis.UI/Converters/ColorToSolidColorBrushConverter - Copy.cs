using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Color = RGB.NET.Core.Color;

namespace Artemis.UI.Converters
{
    /// <inheritdoc />
    /// <summary>
    ///     Converts <see cref="T:RGB.NET.Core.Color" /> into <see cref="T:System.Windows.Media.SolidColorBrush" />.
    /// </summary>
    [ValueConversion(typeof(Color), typeof(SolidColorBrush))]
    public class ColorToSolidColorBrushConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush(!(value is Color color)
                ? System.Windows.Media.Color.FromArgb(0, 0, 0, 0)
                : System.Windows.Media.Color.FromArgb((byte) color.A, (byte) color.R, (byte) color.G, (byte) color.B));
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is SolidColorBrush brush)
                ? Color.Transparent
                : new Color(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
        }
    }
}