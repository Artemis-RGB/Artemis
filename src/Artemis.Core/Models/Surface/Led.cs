using System;
using RGB.NET.Core;
using Rectangle = System.Drawing.Rectangle;

namespace Artemis.Core.Models.Surface
{
    public class DeviceLed
    {
        public DeviceLed(Led led, Device device)
        {
            RgbLed = led;
            Device = device;
            CalculateRenderRectangle();
        }

        public Led RgbLed { get; }
        public Device Device { get; }

        public Rectangle RenderRectangle { get; private set; }
        public Rectangle AbsoluteRenderRectangle { get; private set; }

        public void CalculateRenderRectangle()
        {
            RenderRectangle = new Rectangle(
                (int) Math.Round(RgbLed.LedRectangle.X * Device.Surface.Scale, MidpointRounding.AwayFromZero),
                (int) Math.Round(RgbLed.LedRectangle.Y * Device.Surface.Scale, MidpointRounding.AwayFromZero),
                (int) Math.Round(RgbLed.LedRectangle.Width * Device.Surface.Scale, MidpointRounding.AwayFromZero),
                (int) Math.Round(RgbLed.LedRectangle.Height * Device.Surface.Scale, MidpointRounding.AwayFromZero)
            );
            AbsoluteRenderRectangle = new Rectangle(
                (int) Math.Round(RgbLed.AbsoluteLedRectangle.X * Device.Surface.Scale, MidpointRounding.AwayFromZero),
                (int) Math.Round(RgbLed.AbsoluteLedRectangle.Y * Device.Surface.Scale, MidpointRounding.AwayFromZero),
                (int) Math.Round(RgbLed.AbsoluteLedRectangle.Width * Device.Surface.Scale, MidpointRounding.AwayFromZero),
                (int) Math.Round(RgbLed.AbsoluteLedRectangle.Height * Device.Surface.Scale, MidpointRounding.AwayFromZero)
            );
        }
    }
}