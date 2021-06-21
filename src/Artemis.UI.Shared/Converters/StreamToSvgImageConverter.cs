using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace Artemis.UI.Shared
{
    /// <inheritdoc />
    /// <summary>
    ///     Converts SVG file in the form of a <see cref="T:Stream" /> into <see cref="T:BitmapImage" />.
    /// </summary>
    [ValueConversion(typeof(Stream), typeof(BitmapImage))]
    public class StreamToSvgImageConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Stream stream)
                return Binding.DoNothing;

            stream.Position = 0;

            StreamSvgConverter converter = new(new WpfDrawingSettings());
            using MemoryStream imageStream = new();
            converter.Convert(stream, imageStream);

            BitmapImage selectedBitmap = new();
            selectedBitmap.BeginInit();
            selectedBitmap.StreamSource = imageStream;
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