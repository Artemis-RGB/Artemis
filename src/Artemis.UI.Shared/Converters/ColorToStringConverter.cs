using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Artemis.UI.Shared
{
    /// <inheritdoc />
    /// <summary>
    ///     Converts <see cref="T:System.Windows.Media.Color" /> into <see cref="T:System.String" />.
    /// </summary>
    [ValueConversion(typeof(Color), typeof(string))]
    public class ColorToStringConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (string.IsNullOrWhiteSpace((string) value))
                    return default(Color);

                var color = ColorConverter.ConvertFromString((string) value);
                if (color is Color c)
                    return c;

                return default(Color);
            }
            catch (FormatException)
            {
                return default(Color);
            }
        }
    }
}