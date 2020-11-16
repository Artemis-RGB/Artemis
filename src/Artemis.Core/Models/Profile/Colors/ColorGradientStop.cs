using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     A color with a position, usually contained in a <see cref="ColorGradient" />
    /// </summary>
    public class ColorGradientStop : CorePropertyChanged
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
            set => SetAndNotify(ref _color, value);
        }

        /// <summary>
        ///     Gets or sets the position of the stop
        /// </summary>
        public float Position
        {
            get => _position;
            set => SetAndNotify(ref _position, value);
        }
    }
}