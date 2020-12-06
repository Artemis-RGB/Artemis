using System.Collections.Generic;
using System.Linq;
using RGB.NET.Core;

namespace Artemis.Core.Services.Models
{
    internal class SurfaceArrangement
    {
        public SurfaceArrangement()
        {
            Types = new List<SurfaceArrangementType>();
        }

        public List<SurfaceArrangementType> Types { get; }

        internal static SurfaceArrangement GetDefaultArrangement()
        {
            SurfaceArrangement arrangement = new SurfaceArrangement();

            SurfaceArrangementType keypad = new SurfaceArrangementType(RGBDeviceType.Keypad);
            keypad.Configurations.Add(new SurfaceArrangementConfiguration(null, HorizontalArrangementPosition.Equal, VerticalArrangementPosition.Equal, 20));
            arrangement.Types.Add(keypad);

            SurfaceArrangementType keyboard = new SurfaceArrangementType(RGBDeviceType.Keyboard);
            keyboard.Configurations.Add(new SurfaceArrangementConfiguration(keypad, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Equal, 20));
            arrangement.Types.Add(keyboard);

            SurfaceArrangementType mousepad = new SurfaceArrangementType(RGBDeviceType.Mousepad);
            mousepad.Configurations.Add(new SurfaceArrangementConfiguration(keyboard, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Equal, 10));
            arrangement.Types.Add(mousepad);

            SurfaceArrangementType mouse = new SurfaceArrangementType(RGBDeviceType.Mouse);
            mouse.Configurations.Add(new SurfaceArrangementConfiguration(mousepad, HorizontalArrangementPosition.Center, VerticalArrangementPosition.Center, 0));
            mouse.Configurations.Add(new SurfaceArrangementConfiguration(keyboard, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Center, 100));
            arrangement.Types.Add(mouse);

            SurfaceArrangementType headset = new SurfaceArrangementType(RGBDeviceType.Headset);
            headset.Configurations.Add(new SurfaceArrangementConfiguration(keyboard, HorizontalArrangementPosition.Center, VerticalArrangementPosition.Bottom, 100));
            arrangement.Types.Add(headset);

            SurfaceArrangementType headsetStand = new SurfaceArrangementType(RGBDeviceType.HeadsetStand);
            headsetStand.Configurations.Add(new SurfaceArrangementConfiguration(mousepad, HorizontalArrangementPosition.Center, VerticalArrangementPosition.Top, 100));
            headsetStand.Configurations.Add(new SurfaceArrangementConfiguration(mouse, HorizontalArrangementPosition.Center, VerticalArrangementPosition.Top, 100));
            headsetStand.Configurations.Add(new SurfaceArrangementConfiguration(keyboard, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Top, 100));
            arrangement.Types.Add(headsetStand);

            SurfaceArrangementType mainboard = new SurfaceArrangementType(RGBDeviceType.Mainboard);
            mainboard.Configurations.Add(new SurfaceArrangementConfiguration(mousepad, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Bottom, 500));
            mainboard.Configurations.Add(new SurfaceArrangementConfiguration(mouse, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Bottom, 500));
            mainboard.Configurations.Add(new SurfaceArrangementConfiguration(keyboard, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Bottom, 500));
            arrangement.Types.Add(mainboard);

            SurfaceArrangementType cooler = new SurfaceArrangementType(RGBDeviceType.Cooler);
            cooler.Configurations.Add(new SurfaceArrangementConfiguration(mainboard, HorizontalArrangementPosition.Center, VerticalArrangementPosition.Center, 0));
            arrangement.Types.Add(cooler);

            SurfaceArrangementType dram = new SurfaceArrangementType(RGBDeviceType.DRAM);
            dram.Configurations.Add(new SurfaceArrangementConfiguration(cooler, HorizontalArrangementPosition.Left, VerticalArrangementPosition.Equal, 10));
            dram.Configurations.Add(new SurfaceArrangementConfiguration(mainboard, HorizontalArrangementPosition.Center, VerticalArrangementPosition.Center, 0));
            arrangement.Types.Add(dram);

            SurfaceArrangementType graphicsCard = new SurfaceArrangementType(RGBDeviceType.GraphicsCard);
            graphicsCard.Configurations.Add(new SurfaceArrangementConfiguration(cooler, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Bottom, 10));
            graphicsCard.Configurations.Add(new SurfaceArrangementConfiguration(dram, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Bottom, 10));
            graphicsCard.Configurations.Add(new SurfaceArrangementConfiguration(mainboard, HorizontalArrangementPosition.Center, VerticalArrangementPosition.Center, 0));
            arrangement.Types.Add(graphicsCard);

            SurfaceArrangementType fan = new SurfaceArrangementType(RGBDeviceType.Fan);
            fan.Configurations.Add(new SurfaceArrangementConfiguration(mainboard, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Equal, 100));
            arrangement.Types.Add(fan);

            SurfaceArrangementType ledStripe = new SurfaceArrangementType(RGBDeviceType.LedStripe);
            ledStripe.Configurations.Add(new SurfaceArrangementConfiguration(fan, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Equal, 100));
            arrangement.Types.Add(ledStripe);

            SurfaceArrangementType speaker = new SurfaceArrangementType(RGBDeviceType.Speaker);
            arrangement.Types.Add(speaker);

            return arrangement;
        }

        public void Arrange(ArtemisSurface surface)
        {
            foreach (ArtemisDevice surfaceDevice in surface.Devices)
            {
                surfaceDevice.X = 0;
                surfaceDevice.X = 0;
                surfaceDevice.ApplyToRgbDevice();
            }

            foreach (SurfaceArrangementType surfaceArrangementType in Types)
                surfaceArrangementType.Arrange(surface);

            // See if we need to move the surface to keep X and Y values positive
            double x = surface.Devices.Min(d => d.RgbDevice.Location.X);
            double y = surface.Devices.Min(d => d.RgbDevice.Location.Y);
            if (x < 0)
            {
                foreach (ArtemisDevice surfaceDevice in surface.Devices)
                {
                    surfaceDevice.X += x * -1;
                    surfaceDevice.ApplyToRgbDevice();
                }
            }
            if (y < 0)
            {
                foreach (ArtemisDevice surfaceDevice in surface.Devices)
                {
                    surfaceDevice.Y += y * -1;
                    surfaceDevice.ApplyToRgbDevice();
                }
            }
        }
    }
}