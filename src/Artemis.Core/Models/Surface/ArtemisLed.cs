using RGB.NET.Core;
using SkiaSharp;
using Stylet;

namespace Artemis.Core
{
    public class ArtemisLed : PropertyChangedBase
    {
        private SKRect _absoluteRenderRectangle;
        private SKRect _renderRectangle;

        public ArtemisLed(Led led, ArtemisDevice device)
        {
            RgbLed = led;
            Device = device;
            CalculateRenderRectangle();
        }

        public int LedIndex => Device.Leds.IndexOf(this);
        public Led RgbLed { get; }
        public ArtemisDevice Device { get; }

        public SKRect RenderRectangle
        {
            get => _renderRectangle;
            private set => SetAndNotify(ref _renderRectangle, value);
        }

        public SKRect AbsoluteRenderRectangle
        {
            get => _absoluteRenderRectangle;
            private set => SetAndNotify(ref _absoluteRenderRectangle, value);
        }

        public void CalculateRenderRectangle()
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