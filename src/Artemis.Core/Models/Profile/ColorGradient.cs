using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Artemis.Core.Annotations;
using PropertyChanged;
using SkiaSharp;
using Stylet;

namespace Artemis.Core.Models.Profile
{
    [DoNotNotify]
    public class ColorGradient : INotifyPropertyChanged
    {
        private float _rotation;

        public ColorGradient()
        {
            Colors = new BindableCollection<ColorGradientColor>();
        }

        public BindableCollection<ColorGradientColor> Colors { get; }

        public float Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation != value)
                {
                    _rotation = value;
                    OnPropertyChanged(nameof(Rotation));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SKColor[] GetColorsArray()
        {
            return Colors.Select(c => c.Color).ToArray();
        }
    }

    public struct ColorGradientColor
    {
        public ColorGradientColor(SKColor color, float position)
        {
            Color = color;
            Position = position;
        }

        public SKColor Color { get; set; }
        public float Position { get; set; }
    }
}