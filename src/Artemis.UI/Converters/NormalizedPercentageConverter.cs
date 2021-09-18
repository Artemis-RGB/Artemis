using System;
using System.Globalization;
using System.Windows.Data;

namespace Artemis.UI.Converters
{
    [ValueConversion(typeof(double), typeof(double))]
    public class NormalizedPercentageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double number)
                return number * 100.0;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double number)
                return number / 100.0;

            return value;
        }

        #endregion
    }
}