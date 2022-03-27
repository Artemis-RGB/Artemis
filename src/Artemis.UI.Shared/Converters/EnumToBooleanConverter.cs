using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Artemis.UI.Shared.Converters
{
    /// <summary>
    ///     Converts an enum into a boolean.
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return Equals(value, parameter);
        }

        /// <inheritdoc />
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : BindingOperations.DoNothing;
        }
    }
}