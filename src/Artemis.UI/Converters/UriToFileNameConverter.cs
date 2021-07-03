using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Artemis.UI.Converters
{
    [ValueConversion(typeof(Uri), typeof(string))]
    public class UriToFileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Uri uri && uri.IsFile)
                return Path.GetFileName(uri.LocalPath);
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}