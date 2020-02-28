using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Artemis.Core.Models.Profile;
using SkiaSharp;

namespace Artemis.UI.Shared.Converters
{
    /// <inheritdoc />
    /// <summary>
    ///     Converts  <see cref="T:Artemis.Core.Models.Profile.ColorGradient" /> into a
    ///     <see cref="T:System.Windows.Media.GradientStopCollection" />.
    /// </summary>
    [ValueConversion(typeof(ColorGradient), typeof(GradientStopCollection))]
    public class ColorGradientToGradientStopsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var colorGradient = (ColorGradient) value;
            var collection = new GradientStopCollection();
            if (colorGradient == null)
                return collection;

            foreach (var c in colorGradient.Colors)
                collection.Add(new GradientStop(Color.FromArgb(c.Color.Alpha, c.Color.Red, c.Color.Green, c.Color.Blue), c.Position));
            return collection;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = (GradientStopCollection) value;
            var colorGradient = new ColorGradient();
            if (collection == null)
                return colorGradient;

            foreach (var c in collection)
                colorGradient.Colors.Add(new ColorGradientColor(new SKColor(c.Color.R, c.Color.G, c.Color.B, c.Color.A), (float) c.Offset));
            return colorGradient;
        }
    }
}