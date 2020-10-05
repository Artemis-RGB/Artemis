using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Artemis.Core.Properties;
using SkiaSharp;
using Stylet;

namespace Artemis.Core
{
    /// <summary>
    ///     A gradient containing a list of <see cref="ColorGradientStop" />s
    /// </summary>
    public class ColorGradient : INotifyPropertyChanged
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="ColorGradient" /> class
        /// </summary>
        public ColorGradient()
        {
            Stops = new BindableCollection<ColorGradientStop>();
        }

        /// <summary>
        ///     Gets a list of all the <see cref="ColorGradientStop" />s in the gradient
        /// </summary>
        public BindableCollection<ColorGradientStop> Stops { get; }

        /// <summary>
        ///     Gets all the colors in the color gradient
        /// </summary>
        /// <param name="timesToRepeat">The amount of times to repeat the colors</param>
        /// <returns></returns>
        public SKColor[] GetColorsArray(int timesToRepeat = 0)
        {
            if (timesToRepeat == 0)
                return Stops.OrderBy(c => c.Position).Select(c => c.Color).ToArray();

            List<SKColor> colors = Stops.OrderBy(c => c.Position).Select(c => c.Color).ToList();
            List<SKColor> result = new List<SKColor>();

            for (int i = 0; i <= timesToRepeat; i++)
                result.AddRange(colors);

            return result.ToArray();
        }

        /// <summary>
        ///     Gets all the positions in the color gradient
        /// </summary>
        /// <param name="timesToRepeat">
        ///     The amount of times to repeat the positions, positions will get squished together and
        ///     always stay between 0.0 and 1.0
        /// </param>
        /// <returns></returns>
        public float[] GetPositionsArray(int timesToRepeat = 0)
        {
            if (timesToRepeat == 0)
                return Stops.OrderBy(c => c.Position).Select(c => c.Position).ToArray();

            // Create stops and a list of divided stops
            List<float> stops = Stops.OrderBy(c => c.Position).Select(c => c.Position / (timesToRepeat + 1)).ToList();
            List<float> result = new List<float>();

            // For each repeat cycle, add the base stops to the end result
            for (int i = 0; i <= timesToRepeat; i++)
            {
                List<float> localStops = stops.Select(s => s + result.LastOrDefault()).ToList();
                result.AddRange(localStops);
            }

            return result.ToArray();
        }

        /// <summary>
        ///     Triggers a property changed event of the <see cref="Stops" /> collection
        /// </summary>
        public void OnColorValuesUpdated()
        {
            OnPropertyChanged(nameof(Stops));
        }

        /// <summary>
        ///     Gets a color at any position between 0.0 and 1.0 using interpolation
        /// </summary>
        /// <param name="position">A position between 0.0 and 1.0</param>
        public SKColor GetColor(float position)
        {
            if (!Stops.Any())
                return SKColor.Empty;

            ColorGradientStop[] stops = Stops.OrderBy(x => x.Position).ToArray();
            if (position <= 0) return stops[0].Color;
            if (position >= 1) return stops[^1].Color;
            ColorGradientStop left = stops[0], right = null;
            foreach (ColorGradientStop stop in stops)
            {
                if (stop.Position >= position)
                {
                    right = stop;
                    break;
                }

                left = stop;
            }

            if (right == null || left == right)
                return left.Color;

            position = (float) Math.Round((position - left.Position) / (right.Position - left.Position), 2);
            byte a = (byte) ((right.Color.Alpha - left.Color.Alpha) * position + left.Color.Alpha);
            byte r = (byte) ((right.Color.Red - left.Color.Red) * position + left.Color.Red);
            byte g = (byte) ((right.Color.Green - left.Color.Green) * position + left.Color.Green);
            byte b = (byte) ((right.Color.Blue - left.Color.Blue) * position + left.Color.Blue);
            return new SKColor(r, g, b, a);
        }

        /// <summary>
        ///     Gets a new ColorGradient with colors looping through the HSV-spectrum
        /// </summary>
        /// <returns></returns>
        public static ColorGradient GetUnicornBarf()
        {
            ColorGradient gradient = new ColorGradient();
            for (int i = 0; i < 9; i++)
            {
                SKColor color = i != 8 ? SKColor.FromHsv(i * 32, 100, 100) : SKColor.FromHsv(0, 100, 100);
                gradient.Stops.Add(new ColorGradientStop(color, 0.125f * i));
            }

            return gradient;
        }

        #region PropertyChanged

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        internal virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}