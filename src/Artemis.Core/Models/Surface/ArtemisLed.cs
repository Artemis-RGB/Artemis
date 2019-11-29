using System;
using RGB.NET.Core;
using Stylet;
using Rectangle = System.Drawing.Rectangle;

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

        public Led RgbLed { get; }
        public ArtemisDevice Device { get; }

        public Rectangle RenderRectangle { get; private set; }
        public Rectangle AbsoluteRenderRectangle { get; private set; }

        public void CalculateRenderRectangle()
        {
            RenderRectangle = new Rectangle(
                (int) Math.Round(RgbLed.LedRectangle.Location.X * Device.Surface.Scale, MidpointRounding.AwayFromZero),
                (int) Math.Round(RgbLed.LedRectangle.Location.Y * Device.Surface.Scale, MidpointRounding.AwayFromZero),
                (int) Math.Round(RgbLed.LedRectangle.Size.Width * Device.Surface.Scale, MidpointRounding.AwayFromZero),
                (int) Math.Round(RgbLed.LedRectangle.Size.Height * Device.Surface.Scale, MidpointRounding.AwayFromZero)
            );
            AbsoluteRenderRectangle = new Rectangle(
                (int) Math.Round(RgbLed.AbsoluteLedRectangle.Location.X * Device.Surface.Scale, MidpointRounding.AwayFromZero),
                (int) Math.Round(RgbLed.AbsoluteLedRectangle.Location.Y * Device.Surface.Scale, MidpointRounding.AwayFromZero),
                (int) Math.Round(RgbLed.AbsoluteLedRectangle.Size.Width * Device.Surface.Scale, MidpointRounding.AwayFromZero),
                (int) Math.Round(RgbLed.AbsoluteLedRectangle.Size.Height * Device.Surface.Scale, MidpointRounding.AwayFromZero)
            );
        }
    }
}