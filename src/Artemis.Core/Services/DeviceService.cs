using System.Linq;
using System.Threading.Tasks;
using RGB.NET.Brushes;
using RGB.NET.Core;
using RGB.NET.Groups;

namespace Artemis.Core.Services
{
    internal class DeviceService : IDeviceService
    {
        private readonly IRgbService _rgbService;

        public DeviceService(IRgbService rgbService)
        {
            _rgbService = rgbService;
        }

        public void IdentifyDevice(ArtemisDevice device)
        {
            BlinkDevice(device, 0);
        }

        private void BlinkDevice(ArtemisDevice device, int blinkCount)
        {
            // Create a LED group way at the top
            ListLedGroup ledGroup = new(_rgbService.Surface, device.Leds.Select(l => l.RgbLed))
            {
                Brush = new SolidColorBrush(new Color(255, 255, 255)),
                ZIndex = 999
            };

            // After 200ms, detach the LED group
            Task.Run(async () =>
            {
                await Task.Delay(200);
                ledGroup.Detach(_rgbService.Surface);

                if (blinkCount < 5)
                {
                    // After another 200ms, start over, repeat six times
                    await Task.Delay(200);
                    BlinkDevice(device, blinkCount + 1);
                }
            });
        }
    }
}