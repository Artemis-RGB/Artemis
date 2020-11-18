using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Artemis.UI.Shared
{
    /// <summary>
    /// Converts <see langword="null"/> to <see cref="Visibility"/>
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            Parameters direction;
            if (parameter == null)
                direction = Parameters.Normal;
            else
                direction = (Parameters) Enum.Parse(typeof(Parameters), (string) parameter);

            if (value is string stringValue && string.IsNullOrWhiteSpace(stringValue))
                value = null;

            if (direction == Parameters.Normal)
                return value == null ? Visibility.Collapsed : Visibility.Visible;
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private enum Parameters
        {
            Normal,
            Inverted
        }
    }
}