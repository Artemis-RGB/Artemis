using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace Artemis.UI.Converters
{
    /// <inheritdoc />
    /// <summary>
    ///     Converts <see cref="T:System.Drawing.Color" /> into <see cref="T:System.Windows.Media.Color" />.
    /// </summary>
    [ValueConversion(typeof(Color), typeof(System.Windows.Media.Color))]
    public class ColorToDrawingColorConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color drawingColor)
                return System.Windows.Media.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);

            return default(System.Windows.Media.Color);
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Windows.Media.Color mediaColor)
                return Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);

            return default(Color);
        }
    }
}