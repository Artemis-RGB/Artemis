using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Artemis.UI.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        private enum Parameters
        {
            Normal,
            Inverted
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var direction = (Parameters) Enum.Parse(typeof(Parameters), (string) parameter ?? throw new InvalidOperationException());
            if (direction == Parameters.Normal)
            {
                if (value == null)
                    return Visibility.Hidden;
                return Visibility.Visible;
            }
            if (value == null)
                return Visibility.Visible;
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}