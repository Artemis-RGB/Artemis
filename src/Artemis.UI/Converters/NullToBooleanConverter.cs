using System;
using System.Globalization;
using System.Windows.Data;

namespace Artemis.UI.Converters
{
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Parameters direction;
            if (parameter == null)
                direction = Parameters.Normal;
            else
                direction = (Parameters)Enum.Parse(typeof(Parameters), (string)parameter);

            if (direction == Parameters.Normal)
            {
                if (value == null)
                    return false;
                return true;
            }

            if (value == null)
                return true;
            return false;
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