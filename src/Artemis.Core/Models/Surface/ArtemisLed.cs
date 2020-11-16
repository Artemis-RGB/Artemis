using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents an RGB LED contained in an <see cref="ArtemisDevice" />
    /// </summary>
    public class ArtemisLed : CorePropertyChanged
    {
        private SKRect _absoluteRenderRectangle;
        private SKRect _renderRectangle;

        internal ArtemisLed(Led led, ArtemisDevice device)
        {
            RgbLed = led;
            Device = device;
            CalculateRenderRectangle();
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
        ///     Gets the rectangle covering the LED, sized to match the render scale and positioned relative to the
        ///     <see cref="Device" />
        /// </summary>
        public SKRect RenderRectangle
        {
            get => _renderRectangle;
            private set => SetAndNotify(ref _renderRectangle, value);
        }

        /// <summary>
        ///     Gets the rectangle covering the LED, sized to match the render scale
        /// </summary>
        public SKRect AbsoluteRenderRectangle
        {
            get => _absoluteRenderRectangle;
            private set => SetAndNotify(ref _absoluteRenderRectangle, value);
        }

        internal void CalculateRenderRectangle()
        {
            RenderRectangle = SKRect.Create(
                (RgbLed.LedRectangle.Location.X * Device.Surface.Scale).RoundToInt(),
                (RgbLed.LedRectangle.Location.Y * Device.Surface.Scale).RoundToInt(),
                (RgbLed.LedRectangle.Size.Width * Device.Surface.Scale).RoundToInt(),
                (RgbLed.LedRectangle.Size.Height * Device.Surface.Scale).RoundToInt()
            );
            AbsoluteRenderRectangle = SKRect.Create(
                (RgbLed.AbsoluteLedRectangle.Location.X * Device.Surface.Scale).RoundToInt(),
                (RgbLed.AbsoluteLedRectangle.Location.Y * Device.Surface.Scale).RoundToInt(),
                (RgbLed.AbsoluteLedRectangle.Size.Width * Device.Surface.Scale).RoundToInt(),
                (RgbLed.AbsoluteLedRectangle.Size.Height * Device.Surface.Scale).RoundToInt()
            );
        }
    }
}