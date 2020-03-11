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
            Colors = new BindableCollection<ColorGradientColor>();
        }

        public BindableCollection<ColorGradientColor> Colors { get; }
        public float Rotation { get; set; }

        public SKColor[] GetColorsArray()
        {
            return Colors.OrderBy(c => c.Position).Select(c => c.Color).ToArray();
        }

        public float[] GetColorPositionsArray()
        {
            return Colors.OrderBy(c => c.Position).Select(c => c.Position).ToArray();
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
            OnPropertyChanged(nameof(Colors));
        }

        public SKColor GetColor(float position)
        {
            var point = Colors.SingleOrDefault(f => f.Position == position);
            if (point != null) return point.Color;

            var before = Colors.First(w => w.Position == Colors.Min(m => m.Position));
            var after = Colors.First(w => w.Position == Colors.Max(m => m.Position));

            foreach (var gs in Colors)
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

    public class ColorGradientColor : INotifyPropertyChanged
    {
        public ColorGradientColor(SKColor color, float position)
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