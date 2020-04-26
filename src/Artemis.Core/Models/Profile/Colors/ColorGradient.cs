using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Artemis.Core.Annotations;
using SkiaSharp;
using Stylet;

namespace Artemis.Core.Models.Profile.Colors
{
    public class ColorGradient : INotifyPropertyChanged
    {
        public ColorGradient()
        {
            Stops = new BindableCollection<ColorGradientStop>();
        }

        public BindableCollection<ColorGradientStop> Stops { get; }
        public float Rotation { get; set; }

        public SKColor[] GetColorsArray()
        {
            return Stops.OrderBy(c => c.Position).Select(c => c.Color).ToArray();
        }

        public float[] GetPositionsArray()
        {
            return Stops.OrderBy(c => c.Position).Select(c => c.Position).ToArray();
        }

        public void OnColorValuesUpdated()
        {
            OnPropertyChanged(nameof(Stops));
        }

        public SKColor GetColor(float position)
        {
            if (!Stops.Any())
                return SKColor.Empty;

            var stops = Stops.OrderBy(x => x.Position).ToArray();
            if (position <= 0) return stops[0].Color;
            if (position >= 1) return stops[^1].Color;
            ColorGradientStop left = stops[0], right = null;
            foreach (var stop in stops)
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
            var a = (byte) ((right.Color.Alpha - left.Color.Alpha) * position + left.Color.Alpha);
            var r = (byte) ((right.Color.Red - left.Color.Red) * position + left.Color.Red);
            var g = (byte) ((right.Color.Green - left.Color.Green) * position + left.Color.Green);
            var b = (byte) ((right.Color.Blue - left.Color.Blue) * position + left.Color.Blue);
            return new SKColor(r, g, b, a);
        }

        /// <summary>
        ///     [PH] Looping through HSV, adds 8 rainbow colors
        /// </summary>
        public void MakeFabulous()
        {
            for (var i = 0; i < 9; i++)
            {
                var color = i != 8 ? SKColor.FromHsv(i * 32, 100, 100) : SKColor.FromHsv(0, 100, 100);
                Stops.Add(new ColorGradientStop(color, 0.125f * i));
            }
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}