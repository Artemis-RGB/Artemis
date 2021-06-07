using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Artemis.UI.Shared
{
    /// <inheritdoc />
    /// <summary>
    ///     Converts <see cref="T:Stream" /> into <see cref="T:BitmapImage" />.
    /// </summary>
    [ValueConversion(typeof(Stream), typeof(BitmapImage))]
    public class StreamToBitmapImageConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Stream stream) 
                return null;

            stream.Position = 0;

            BitmapImage selectedBitmap = new();
            selectedBitmap.BeginInit();
            selectedBitmap.StreamSource = stream;
            selectedBitmap.CacheOption = BitmapCacheOption.OnLoad;
            selectedBitmap.EndInit();
            selectedBitmap.Freeze();

            stream.Position = 0;
            return selectedBitmap;
        }


        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}