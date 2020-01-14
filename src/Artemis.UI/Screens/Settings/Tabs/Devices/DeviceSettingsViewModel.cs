using System.Diagnostics;
using Artemis.Core.Models.Surface;
using Artemis.Core.Services;
using Humanizer;

namespace Artemis.UI.Screens.Settings.Tabs.Devices
{
    public class DeviceSettingsViewModel
    {
        private readonly IDeviceService _deviceService;

        public DeviceSettingsViewModel(ArtemisDevice device, IDeviceService deviceService)
        {
            _deviceService = deviceService;
            Device = device;

            Type = Device.RgbDevice.DeviceInfo.DeviceType.ToString().Humanize();
            Name = Device.RgbDevice.DeviceInfo.Model;
            Manufacturer = Device.RgbDevice.DeviceInfo.Manufacturer;
            IsDeviceEnabled = true;
        }

        public ArtemisDevice Device { get; }

        public string Type { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public bool IsDeviceEnabled { get; set; }

        public void IdentifyDevice()
        {
            _deviceService.IdentifyDevice(Device);
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