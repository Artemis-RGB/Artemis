using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents an RGB LED contained in an <see cref="ArtemisDevice" />
    /// </summary>
    public class ArtemisLed : CorePropertyChanged
    {
        private SKRect _absoluteRectangle;
        private SKRect _rectangle;

        internal ArtemisLed(Led led, ArtemisDevice device)
        {
            RgbLed = led;
            Device = device;
            CalculateRectangles();
        }

        /// <summary>
        ///     Gets the RGB.NET LED backing this Artemis LED
        /// </summary>
        public Led RgbLed { get; }

        /// <summary>
        ///     Gets the device that contains this LED
        /// </summary>
        public ArtemisDevice Device { get; }

        /// <summary>
        ///     Gets the rectangle covering the LED positioned relative to the<see cref="Device" />
        /// </summary>
        public SKRect Rectangle
        {
            get => _rectangle;
            private set => SetAndNotify(ref _rectangle, value);
        }

        /// <summary>
        ///     Gets the rectangle covering the LED
        /// </summary>
        public SKRect AbsoluteRectangle
        {
            get => _absoluteRectangle;
            private set => SetAndNotify(ref _absoluteRectangle, value);
        }

        internal void CalculateRectangles()
        {
            Rectangle = RgbLed.LedRectangle.ToSKRect();
            AbsoluteRectangle = RgbLed.AbsoluteLedRectangle.ToSKRect();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return RgbLed.ToString();
        }
    }
}