using System;
using System.Globalization;
using System.Windows.Data;

namespace Artemis.UI.Installer.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(bool))
                return !(bool) value;
            if (targetType == typeof(bool?))
                return !(bool?) value;

            throw new InvalidOperationException("The target must be a boolean");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(bool))
                return !(bool) value;
            if (targetType == typeof(bool?))
                return !(bool?) value;

            throw new InvalidOperationException("The target must be a boolean");
        }

        #endregion
    }
}