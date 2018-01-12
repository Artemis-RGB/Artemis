using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Artemis.UI.Converters
{
    /// <inheritdoc />
    /// <summary>
    /// Converts <see cref="T:RGB.NET.Core.Color" /> into <see cref="T:System.Windows.Media.SolidColorBrush" />.
    /// </summary>
    [ValueConversion(typeof(RGB.NET.Core.Color), typeof(SolidColorBrush))]
    public class ColorToSolidColorBrushConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush(!(value is RGB.NET.Core.Color color)
                ? Color.FromArgb(0, 0, 0, 0)
                : Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is SolidColorBrush brush)
                ? RGB.NET.Core.Color.Transparent
                : new RGB.NET.Core.Color(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
        }
    }
}
