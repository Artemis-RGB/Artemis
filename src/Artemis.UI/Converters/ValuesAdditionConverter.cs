using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Artemis.UI.Converters
{
    public class ValuesAdditionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Where(v => v is double).Cast<double>().Sum();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}