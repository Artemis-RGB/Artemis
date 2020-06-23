using System.ComponentModel;
using System.Runtime.CompilerServices;
using Artemis.Core.Annotations;
using SkiaSharp;

namespace Artemis.Core.Models.Profile.Colors
{
    public class ColorGradientStop : INotifyPropertyChanged
    {
        private SKColor _color;
        private float _position;

        public ColorGradientStop(SKColor color, float position)
        {
            Color = color;
            Position = position;
        }

        public SKColor Color
        {
            get => _color;
            set
            {
                if (value.Equals(_color)) return;
                _color = value;
                OnPropertyChanged();
            }
        }

        public float Position
        {
            get => _position;
            set
            {
                if (value.Equals(_position)) return;
                _position = value;
                OnPropertyChanged();
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