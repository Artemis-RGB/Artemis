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