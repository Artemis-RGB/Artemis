using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using Artemis.Core;
using SkiaSharp;
using Stylet;

namespace Artemis.UI.Shared
{
    /// <inheritdoc />
    /// <summary>
    ///     Converts  <see cref="T:Artemis.Core.Models.Profile.ColorGradient" /> into a
    ///     <see cref="T:System.Windows.Media.GradientStopCollection" />.
    /// </summary>
    [ValueConversion(typeof(BindableCollection<ColorGradientStop>), typeof(GradientStopCollection))]
    public class ColorGradientToGradientStopsConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var colorGradients = (BindableCollection<ColorGradientStop>) value;
            var collection = new GradientStopCollection();
            if (colorGradients == null)
                return collection;

            foreach (var c in colorGradients.OrderBy(s => s.Position))
                collection.Add(new GradientStop(Color.FromArgb(c.Color.Alpha, c.Color.Red, c.Color.Green, c.Color.Blue), c.Position));
            return collection;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = (GradientStopCollection) value;
            var colorGradients = new BindableCollection<ColorGradientStop>();
            if (collection == null)
                return colorGradients;

            foreach (var c in collection.OrderBy(s => s.Offset))
                colorGradients.Add(new ColorGradientStop(new SKColor(c.Color.R, c.Color.G, c.Color.B, c.Color.A), (float) c.Offset));
            return colorGradients;
        }
    }
}