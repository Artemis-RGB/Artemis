using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using SkiaSharp;

namespace Artemis.UI.Shared
{
    /// <inheritdoc />
    /// <summary>
    ///     Converts <see cref="SKColor" />into <see cref="T:System.String" />.
    /// </summary>
    [ValueConversion(typeof(Color), typeof(string))]
    public class SKColorToStringConverter : IValueConverter
    {
        /// <inheritdoc />
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value as string))
                return SKColor.Empty;

            return SKColor.TryParse((string) value!, out SKColor color) ? color : SKColor.Empty;
        }
    }
}