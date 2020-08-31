using System.ComponentModel;
using System.Runtime.CompilerServices;
using Artemis.Core.Properties;
using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     A color with a position, usually contained in a <see cref="ColorGradient" />
    /// </summary>
    public class ColorGradientStop : INotifyPropertyChanged
    {
        private SKColor _color;
        private float _position;

        /// <summary>
        ///     Creates a new instance of the <see cref="ColorGradientStop" /> class
        /// </summary>
        public ColorGradientStop(SKColor color, float position)
        {
            Color = color;
            Position = position;
        }

        /// <summary>
        ///     Gets or sets the color of the stop
        /// </summary>
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

        /// <summary>
        ///     Gets or sets the position of the stop
        /// </summary>
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

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}