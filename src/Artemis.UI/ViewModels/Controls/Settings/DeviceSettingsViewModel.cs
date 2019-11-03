using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.Models.Surface;
using Artemis.Core.Services.Interfaces;
using Humanizer;

namespace Artemis.UI.ViewModels.Controls.Settings
{
    public class DeviceSettingsViewModel
    {
        private readonly ICoreService _coreService;

        public DeviceSettingsViewModel(Device device, ICoreService coreService)
        {
            _coreService = coreService;

            Device = device;

            Type = Device.RgbDevice.DeviceInfo.DeviceType.ToString().Humanize();
            Name = Device.RgbDevice.DeviceInfo.Model;
            Manufacturer = Device.RgbDevice.DeviceInfo.Manufacturer;
            IsDeviceEnabled = true;
        }

        public Device Device { get; }

        public string Type { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public bool IsDeviceEnabled { get; set; }

        public void Identify()
        {
            BlinkDevice(0);
        }

        private void BlinkDevice(int blinkCount)
        {
            // Draw a white overlay over the device
            void DrawOverlay(object sender, FrameRenderingEventArgs args)
            {
                using (var g = Graphics.FromImage(args.Bitmap))
                {
                    g.FillPath(new SolidBrush(Color.White), Device.RenderPath);
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
                    BlinkDevice(blinkCount + 1);
                }
            });
        }

        public void ShowDeviceDebugger()
        {
        }

        public void OpenPluginDirectory()
        {
            Process.Start(Device.Plugin.PluginInfo.Directory.FullName);
        }
    }
}