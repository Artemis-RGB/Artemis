using System;
using System.Globalization;
using System.Windows.Data;
using Artemis.Core;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Converts <see cref="T:System.String" /> into <see cref="Numeric" />.
    /// </summary>
    [ValueConversion(typeof(string), typeof(Numeric))]
    public class StringToNumericConverter : IValueConverter
    {
        /// <inheritdoc />
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            Numeric.TryParse(value as string, out Numeric result);
            return result;
        }
    }
}