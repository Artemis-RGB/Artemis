using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using SkiaSharp;

namespace Artemis.UI.Shared.Converters
{
    /// <inheritdoc />
    /// <summary>
    ///     Converts <see cref="SKColor"/>into <see cref="T:System.String" />.
    /// </summary>
    [ValueConversion(typeof(Color), typeof(string))]
    public class SKColorToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace((string) value))
                return SKColor.Empty;

            return SKColor.TryParse((string)value, out var color) ? color : SKColor.Empty;
        }
    }
}