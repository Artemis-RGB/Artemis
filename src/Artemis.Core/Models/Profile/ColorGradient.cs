using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Artemis.Core.Annotations;
using SkiaSharp;
using Stylet;

namespace Artemis.Core.Models.Profile
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

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public void OnColorValuesUpdated()
        {
            OnPropertyChanged(nameof(Stops));
        }

        public SKColor GetColor(float position)
        {
            var point = Stops.SingleOrDefault(f => f.Position == position);
            if (point != null) return point.Color;

            var before = Stops.First(w => w.Position == Stops.Min(m => m.Position));
            var after = Stops.First(w => w.Position == Stops.Max(m => m.Position));

            foreach (var gs in Stops)
            {
                if (gs.Position < position && gs.Position > before.Position)
                    before = gs;

                if (gs.Position >= position && gs.Position < after.Position)
                    after = gs;
            }

            return new SKColor(
                (byte) ((position - before.Position) * (after.Color.Red - before.Color.Red) / (after.Position - before.Position) + before.Color.Red),
                (byte) ((position - before.Position) * (after.Color.Green - before.Color.Green) / (after.Position - before.Position) + before.Color.Green),
                (byte) ((position - before.Position) * (after.Color.Blue - before.Color.Blue) / (after.Position - before.Position) + before.Color.Blue),
                (byte) ((position - before.Position) * (after.Color.Alpha - before.Color.Alpha) / (after.Position - before.Position) + before.Color.Alpha)
            );
        }
    }

    public class ColorGradientStop : INotifyPropertyChanged
    {
        public ColorGradientStop(SKColor color, float position)
        {
            Color = color;
            Position = position;
        }

        public SKColor Color { get; set; }
        public float Position { get; set; }

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