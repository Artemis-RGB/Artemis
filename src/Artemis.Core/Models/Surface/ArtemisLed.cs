using Artemis.Core.Extensions;
using RGB.NET.Core;
using SkiaSharp;
using Stylet;

namespace Artemis.Core.Models.Surface
{
    public class ArtemisLed : PropertyChangedBase
    {
        public ArtemisLed(Led led, ArtemisDevice device)
        {
            RgbLed = led;
            Device = device;
            CalculateRenderRectangle();
        }

        public int LedIndex => Device.Leds.IndexOf(this);
        public Led RgbLed { get; }
        public ArtemisDevice Device { get; }

        public SKRect RenderRectangle { get; private set; }
        public SKRect AbsoluteRenderRectangle { get; private set; }
        
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