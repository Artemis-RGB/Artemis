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
            ArrangedDevices = new List<ArtemisDevice>();
        }

        public List<SurfaceArrangementType> Types { get; }
        public List<ArtemisDevice> ArrangedDevices { get; }

        internal static SurfaceArrangement GetDefaultArrangement()
        {
            SurfaceArrangement arrangement = new();

            SurfaceArrangementType keypad = arrangement.AddType(RGBDeviceType.Keypad, 1);
            keypad.AddConfiguration(new SurfaceArrangementConfiguration(null, HorizontalArrangementPosition.Equal, VerticalArrangementPosition.Equal, 20));

            SurfaceArrangementType keyboard = arrangement.AddType(RGBDeviceType.Keyboard, 1);
            keyboard.AddConfiguration(new SurfaceArrangementConfiguration(keypad, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Equal, 20));

            SurfaceArrangementType mousepad = arrangement.AddType(RGBDeviceType.Mousepad, 1);
            mousepad.AddConfiguration(new SurfaceArrangementConfiguration(keyboard, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Equal, 10));

            SurfaceArrangementType mouse = arrangement.AddType(RGBDeviceType.Mouse, 2);
            mouse.AddConfiguration(new SurfaceArrangementConfiguration(mousepad, HorizontalArrangementPosition.Center, VerticalArrangementPosition.Center, 0));
            mouse.AddConfiguration(new SurfaceArrangementConfiguration(keyboard, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Center, 100));

            SurfaceArrangementType headset = arrangement.AddType(RGBDeviceType.Headset, 1);
            headset.AddConfiguration(new SurfaceArrangementConfiguration(keyboard, HorizontalArrangementPosition.Center, VerticalArrangementPosition.Bottom, 100));

            SurfaceArrangementType headsetStand = arrangement.AddType(RGBDeviceType.HeadsetStand, 1);
            headsetStand.AddConfiguration(new SurfaceArrangementConfiguration(mousepad, HorizontalArrangementPosition.Center, VerticalArrangementPosition.Top, 100));
            headsetStand.AddConfiguration(new SurfaceArrangementConfiguration(mouse, HorizontalArrangementPosition.Center, VerticalArrangementPosition.Top, 100));
            headsetStand.AddConfiguration(new SurfaceArrangementConfiguration(keyboard, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Top, 100));

            SurfaceArrangementType mainboard = arrangement.AddType(RGBDeviceType.Mainboard, 1);
            mainboard.AddConfiguration(new SurfaceArrangementConfiguration(mousepad, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Bottom, 500));
            mainboard.AddConfiguration(new SurfaceArrangementConfiguration(mouse, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Bottom, 500));
            mainboard.AddConfiguration(new SurfaceArrangementConfiguration(keyboard, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Bottom, 500));

            SurfaceArrangementType cooler = arrangement.AddType(RGBDeviceType.Cooler, 2);
            cooler.AddConfiguration(new SurfaceArrangementConfiguration(mainboard, HorizontalArrangementPosition.Center, VerticalArrangementPosition.Center, 0));

            SurfaceArrangementType dram = arrangement.AddType(RGBDeviceType.DRAM, 2);
            dram.AddConfiguration(new SurfaceArrangementConfiguration(cooler, HorizontalArrangementPosition.Left, VerticalArrangementPosition.Equal, 10));
            dram.AddConfiguration(new SurfaceArrangementConfiguration(mainboard, HorizontalArrangementPosition.Center, VerticalArrangementPosition.Center, 0));

            SurfaceArrangementType graphicsCard = arrangement.AddType(RGBDeviceType.GraphicsCard, 2);
            graphicsCard.AddConfiguration(new SurfaceArrangementConfiguration(cooler, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Bottom, 10));
            graphicsCard.AddConfiguration(new SurfaceArrangementConfiguration(dram, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Bottom, 10));
            graphicsCard.AddConfiguration(new SurfaceArrangementConfiguration(mainboard, HorizontalArrangementPosition.Center, VerticalArrangementPosition.Center, 0));

            SurfaceArrangementType fan = arrangement.AddType(RGBDeviceType.Fan, 2);
            fan.AddConfiguration(new SurfaceArrangementConfiguration(mainboard, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Equal, 100));

            SurfaceArrangementType ledStripe = arrangement.AddType(RGBDeviceType.LedStripe, 2);
            ledStripe.AddConfiguration(new SurfaceArrangementConfiguration(fan, HorizontalArrangementPosition.Right, VerticalArrangementPosition.Equal, 100));

            arrangement.AddType(RGBDeviceType.Speaker, 1);
            arrangement.AddType(RGBDeviceType.None, 1);

            return arrangement;
        }

        private SurfaceArrangementType AddType(RGBDeviceType type, int zIndex)
        {
            SurfaceArrangementType surfaceArrangementType = new(this, type, zIndex);
            Types.Add(surfaceArrangementType);
            return surfaceArrangementType;
        }

        public void Arrange(List<ArtemisDevice> devices)
        {
            ArrangedDevices.Clear();
            
            // Not much to do here
            if (!devices.Any())
                return;

            foreach (ArtemisDevice surfaceDevice in devices)
            {
                surfaceDevice.X = 0;
                surfaceDevice.Y = 0;
                surfaceDevice.ApplyToRgbDevice();
            }

            foreach (SurfaceArrangementType surfaceArrangementType in Types)
                surfaceArrangementType.Arrange(devices);

            // See if we need to move the surface to keep X and Y values positive
            double x = devices.Min(d => d.RgbDevice.Location.X);
            double y = devices.Min(d => d.RgbDevice.Location.Y);
            if (x < 0)
            {
                foreach (ArtemisDevice surfaceDevice in devices)
                {
                    surfaceDevice.X += x * -1;
                    surfaceDevice.ApplyToRgbDevice();
                }
            }

            if (y < 0)
            {
                foreach (ArtemisDevice surfaceDevice in devices)
                {
                    surfaceDevice.Y += y * -1;
                    surfaceDevice.ApplyToRgbDevice();
                }
            }
        }
    }
}