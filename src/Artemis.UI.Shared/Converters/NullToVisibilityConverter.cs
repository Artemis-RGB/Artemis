using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Artemis.UI.Shared
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Parameters direction;
            if (parameter == null)
                direction = Parameters.Normal;
            else
                direction = (Parameters) Enum.Parse(typeof(Parameters), (string) parameter);

            if (direction == Parameters.Normal)
            {
                if (value == null)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }

            if (value == null)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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