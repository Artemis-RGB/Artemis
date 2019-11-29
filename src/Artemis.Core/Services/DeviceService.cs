using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.Models.Surface;
using Artemis.Core.Services.Interfaces;

namespace Artemis.Core.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly ICoreService _coreService;

        public DeviceService(ICoreService coreService)
        {
            _coreService = coreService;
        }

        public void IdentifyDevice(ArtemisDevice device)
        {
            BlinkDevice(device, 0);
        }

        private void BlinkDevice(ArtemisDevice device, int blinkCount)
        {
            // Draw a white overlay over the device
            void DrawOverlay(object sender, FrameRenderingEventArgs args)
            {
                using (var g = Graphics.FromImage(args.Bitmap))
                {
                    g.FillPath(new SolidBrush(Color.White), device.RenderPath);
                }
            }

            _coreService.FrameRendering += DrawOverlay;

            // After 200ms, stop drawing the overlay
            Task.Run(async () =>
            {
                await Task.Delay(200);
                _coreService.FrameRendering -= DrawOverlay;

                if (blinkCount < 5)
                {
                    // After another 200ms, draw the overlay again, repeat six times
                    await Task.Delay(200);
                    BlinkDevice(device, blinkCount + 1);
                }
            });
        }
    }

    public interface IDeviceService : IArtemisService
    {
        /// <summary>
        /// Identifies the device by making it blink white 5 times
        /// </summary>
        /// <param name="device"></param>
        void IdentifyDevice(ArtemisDevice device);
    }
}
