using System;
using System.Globalization;
using System.Windows.Data;

namespace Artemis.VisualScripting.Converters
{
    internal class CenterTranslateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double doubleValue)
                return value;

            return doubleValue / 2 * -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double doubleValue)
                return value;

            return doubleValue * -1 * 2;
        }
    }
}