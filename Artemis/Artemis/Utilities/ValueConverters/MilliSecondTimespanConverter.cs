using System;
using System.Globalization;
using System.Windows.Data;

namespace Artemis.Utilities.ValueConverters
{
    public class MilliSecondTimespanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? 0.0 : ((TimeSpan) value).TotalMilliseconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? new TimeSpan() : TimeSpan.FromMilliseconds((double) value);
        }
    }
}