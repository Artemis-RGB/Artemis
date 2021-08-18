using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Artemis.UI.Shared
{
    /// <summary>
    /// Converts  <see cref="int"/> to <see cref="Visibility"/>
    /// </summary>
    public class IntToVisibilityConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            Parameters direction;
            if (parameter == null)
                direction = Parameters.Normal;
            else
                direction = (Parameters) Enum.Parse(typeof(Parameters), (string) parameter);

            return direction == Parameters.Normal 
                ? value is > 1 ? Visibility.Visible : Visibility.Collapsed 
                : value is > 1 ? Visibility.Collapsed : Visibility.Visible;
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